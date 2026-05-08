# 07 — Issues Conocidos

> **Última actualización:** 2026-05-08 (rev 6)

---

## ✅ ISSUE-001: BlobNotFound en ExtractCurriculumJob para asignaturas con acentos

**Detectado:** 2026-05-08 (job 38 en Hangfire)
**Estado:** ✅ Resuelto
**Componentes:** `SyncCurriculumJob.cs`, `ExtractCurriculumJob.cs`

### Síntoma
`ExtractCurriculumJob` falla con `Azure.RequestFailedException: BlobNotFound (404)` al intentar descargar el PDF de asignaturas cuyo nombre contiene acentos (ej. _Educación Física_, _Artes Plásticas_, _Matemáticas_).

### Causa raíz
Dos problemas combinados:

1. **`SyncCurriculumJob`** generaba el blob name con `asignatura.ToLowerInvariant().Replace(" ", "-")`, que preserva acentos: `educación-física/20260508190108.pdf`.
2. **`ExtractCurriculumJob`** extraía el blob name desde `Uri.AbsolutePath`, que percent-encodea caracteres no-ASCII: `educaci%C3%B3n-f%C3%ADsica/...`. Ese string encoded se usaba literalmente como nombre de blob → no coincidía → 404.

### Fix aplicado
- `ExtractCurriculumJob.cs` línea ~128: `Uri.UnescapeDataString(blobUri.AbsolutePath)` antes de construir el blob name. Resuelve jobs con blobs ya subidos con acentos.
- `SyncCurriculumJob.cs`: nuevo método `ToAsciiSlug()` (NFD normalization + strip NonSpacingMark) para generar slugs ASCII puros en subidas futuras.

### Verificación
- Blob `educación-física/20260508190108.pdf` existe en container `curriculum` (confirmado con `az storage blob list`).
- Build compila sin errores ni advertencias.
- Retry del job 38 en Hangfire dashboard debería pasar tras reiniciar la API con el fix.

---

## ✅ ISSUE-002: NetworkTimeout en ExtractCurriculumJob para PDFs grandes (job 46 — Francés)

**Detectado:** 2026-05-08 (job 46 en Hangfire, ejecutado en Azure Container App)
**Estado:** ✅ Resuelto
**Componentes:** `ExtractCurriculumJob.cs`, `OptionsExtensions.cs`

### Síntoma
`ExtractCurriculumJob` falla con `System.AggregateException: Retry failed after 4 tries. NetworkTimeout 0:01:40` al llamar a Azure AI Foundry para el PDF de _Francés_ (5.4 MB).

### Causa raíz
`AzureOpenAIClient` se instanciaba dentro del método `ExtractUnitsWithAiAsync` en cada ejecución del job. Esto crea un `HttpClient` nuevo por invocación (sin pool de conexiones), y el `NetworkTimeout` por defecto de 100 s era insuficiente para la respuesta de GPT-5.5 procesando un PDF grande desde la red de Azure.

### Fix aplicado
- `OptionsExtensions.cs` — `AzureOpenAIClient` registrado como **singleton** en DI con `NetworkTimeout = TimeSpan.FromMinutes(10)` y `ApiKeyCredential`.
- `ExtractCurriculumJob.cs` — constructor ahora recibe `AzureOpenAIClient aiClient` inyectado; `ExtractUnitsWithAiAsync` usa el cliente del singleton en lugar de instanciar uno propio.

### Pendiente
- `AdecuacionAiService.cs` (línea ~39) y `PlaneamientoAiService.cs` (línea ~49) también instancian `AzureOpenAIClient` por llamada — mismo riesgo potencial, no reportado como fallo todavía.

---

## ✅ ISSUE-003: UpdateExchangeRateJob falla con HTTP 500 / XmlException del BCCR (job 51)

**Detectado:** 2026-05-08 (job 51 en Hangfire)
**Estado:** ✅ Resuelto
**Componentes:** `UpdateExchangeRateJob.cs`, `BccrOptions.cs`, `SinpeOptions.cs`, `OptionsExtensions.cs`

### Síntoma (dos etapas)
1. `HttpRequestException: Response status code does not indicate success: 500` al llamar al SOAP del BCCR.
2. Tras corregir el HTTP 500: `System.Xml.XmlException: Data at the root level is invalid. Line 1, position 1` en `ParseTipoCambio` — el BCCR devuelve 200 con contenido no-XML (texto plano, vacío o `"Nothing"`) cuando no tiene dato para la fecha.

### Causa raíz
1. **SOAP body incompleto:** faltaban `<Token>` y `<CorreoElectronico>` — campos obligatorios según el WSDL del BCCR. Sin ellos el servicio devuelve HTTP 500.
2. **`EnsureSuccessStatusCode()`** propagaba el 500 como excepción.
3. **`ParseTipoCambio` sin guard:** `XDocument.Parse()` lanza `XmlException` cuando el BCCR responde 200 con texto plano en días sin datos (fines de semana, feriados).

### Fix aplicado
- `BccrOptions.cs` (nuevo): clase de opciones con todos los campos del WSDL — `Token`, `CorreoElectronico`, `Nombre`, `SubNiveles` (default `"N"`), `IndicadorDolar` (default `318`). Validación con `[Required]` + `ValidateOnStart`.
- `SinpeOptions.cs`: propiedad `BccrToken` eliminada (movida a `BccrOptions`).
- `OptionsExtensions.cs`: registro de `BccrOptions` con `BindConfiguration` + `ValidateDataAnnotations`.
- `UpdateExchangeRateJob.cs`:
  - Constructor inyecta `IOptions<BccrOptions>` en lugar de `IOptions<SinpeOptions>`.
  - SOAP body completo con los 7 campos obligatorios (incluyendo `<CorreoElectronico>` y `<Token>`).
  - Reemplaza `EnsureSuccessStatusCode()` por verificación manual → `null` + `LogWarning` si no-2xx.
  - Catch de `XmlException` e `InvalidOperationException` en `FetchTipoCambioAsync` → `null` (no falla el job).
  - `ExecuteAsync(PerformContext? ctx, CancellationToken ct)`: si `FetchTipoCambioAsync` retorna `null` → `ctx?.WriteLine(...)` + `LogWarning` + retorna sin excepción (job queda `Succeeded`).
  - Logs visibles en dashboard Hangfire con `ctx?.WriteLine()`: pasos `[1/4]`–`[4/4]`, HTTP status + ms de respuesta, XML de respuesta BCCR (hasta 500 chars) y resultado del parse.
- `ModulesExtensions.cs`: recurring job registrado con `j.ExecuteAsync(null!, CancellationToken.None)` para que Hangfire inyecte el `PerformContext` real al ejecutar.
- `appsettings.json`: secciones `Bccr` y `Sinpe` con defaults vacíos.
- `appsettings.Development.json`: sección `Bccr` con credenciales de desarrollo.
- Container App `ca-aulaia-api`: env vars `Bccr__Token`, `Bccr__CorreoElectronico`, `Bccr__Nombre`, `Bccr__SubNiveles`, `Bccr__IndicadorDolar` configuradas.

**Etapa 3 — Parse incorrecto (detectado via logs `PerformContext`):**
- Los logs mostraron que el BCCR sí responde 200 con XML válido, pero `ParseTipoCambio` fallaba con `XmlException` en el segundo `XDocument.Parse`. Causa: `ObtenerIndicadoresEconomicosResult` contiene los datos como nodos hijo XML reales (schema + diffgram), no como string embebido. `.Value` devolvía solo texto concatenado sin tags → no era XML válido.
- Fix: eliminado el doble parse. `ParseTipoCambio` ahora hace un solo `XDocument.Parse(soapXml)` y busca `NUM_VALOR` en `doc.Descendants()` directamente.
- Fix adicional: si `FetchTipoCambioAsync` retorna `null` → lanza `InvalidOperationException` (job termina `Failed`) en lugar de retornar silenciosamente.
- Fix BD: job 51 tenía `invocationdata` con firma antigua `ExecuteAsync(CancellationToken)` persistida; eliminado manualmente de `hangfire.job`, `hangfire.state`, `hangfire.jobparameter`; `LastJobId` limpiado en `hangfire.hash`.

### Verificación
- Logs del dashboard confirmaron que el XML del BCCR llegaba correctamente (1633 chars) antes del fix de parse.
- Deploy revisión `ca-aulaia-api--0000006` con parse corregido.
- Próxima ejecución diaria (12h UTC) debe guardar el TC en `exchange_rates`.
