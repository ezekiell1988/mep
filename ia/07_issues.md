# 07 — Issues Conocidos

> **Última actualización:** 2026-05-11 (rev 12)

---

## ✅ ISSUE-008: Sin provisioning de usuario Auth0 → BD (gap crítico para onboarding)

**Detectado:** 2026-05-11
**Estado:** ✅ Resuelto
**Componentes:** `CurrentUserService.cs`, `Features/Users/UsersModule.cs`, `callback/page.tsx`, `UserConfiguration.cs`

### Síntoma
`CurrentUserService.ResolveAsync()` lanzaba `UnauthorizedAccessException` ("User with sub '...' not found") si el `auth0_sub` del JWT no tenía una fila correspondiente en la tabla `users`. Ningún mecanismo creaba el usuario automáticamente. Adriana se hubiera registrado en Auth0 y recibido 401 en todas las llamadas.

### Causa raíz
El seed data tenía a Adriana con `Auth0Sub = "auth0|PLACEHOLDER_ADRIANA"` — un sub ficticio que nunca coincidiría con su sub real de Auth0. Y no existía ningún endpoint ni middleware que creara el `User` al primer login.

### Fix aplicado
1. **`POST /api/auth/me`** (`Features/Users/UsersModule.cs`) — endpoint idempotente: busca por `auth0sub` en BD → si no existe, crea `User` nuevo con `Role = Teacher`. Requiere autenticación.
2. **`callback/page.tsx`** — llama `ensureUserProfile(token)` justo después de `handleRedirectCallback()`, antes de redirigir. Provisioning garantizado en cada primer login.
3. **`UserConfiguration.cs`** — Adriana eliminada del seed `HasData`. Solo queda Ezequiel con su sub real.
4. **Migración `RemoveAdrianaFromSeed`** — reasigna grupos demo a Ezequiel (`UPDATE groups SET teacher_id/teacher_sub`) antes del `DELETE FROM users`; respeta FK Restrict.

### Comportamiento post-fix
- Usuario nuevo se registra en Auth0 → `/callback` llama `POST /api/auth/me` → fila `User` creada con `Role = Teacher` → todas las llamadas siguientes funcionan.
- Endpoint es idempotente: si el user ya existe (segundo login), devuelve su perfil sin modificar nada.

---

## ✅ ISSUE-007: Cuenta Google Play Developer cerrada

**Detectado:** 2026-05-10
**Estado:** ✅ Resuelto con ADR-011
**Componentes:** Distribución app Android

### Síntoma
La cuenta de Google Play Developer asociada a `ezekiell1988@gmail.com` está cerrada por inactividad desde 2021 y no puede reactivarse.

### Decisión
Distribución directa por APK (Google Drive → WhatsApp) durante el piloto. Crear cuenta nueva de Play Developer ($25) una vez validado el producto con Adriana. Ver ADR-011.

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

## ✅ ISSUE-006: BlobName con acentos en `POST /api/curriculum/upload` (endpoint admin)

**Detectado:** 2026-05-10 (revisión de código)
**Estado:** ✅ Resuelto
**Componentes:** `CurriculumModule.cs`, `SyncCurriculumJob.cs`, `Shared/Extensions/BlobSlugHelper.cs`

### Síntoma
El endpoint `POST /api/curriculum/upload` generaba el blob name con `asignatura.ToLowerInvariant().Replace(" ", "-")`, que preserva tildes y caracteres no-ASCII (mismo patrón que causó ISSUE-001 en `SyncCurriculumJob`). Un PDF subido para "Artes Plásticas" se guardaba como `artes-plásticas/<uuid>.pdf`, lo que habría causado BlobNotFound en `ExtractCurriculumJob` al procesar la URL.

### Causa raíz
`ToAsciiSlug()` se implementó como método privado en `SyncCurriculumJob` (fix de ISSUE-001) pero no se propagó al endpoint de upload manual en `CurriculumModule`. El código duplicado y sin el fix quedó en producción como deuda técnica silenciosa.

### Fix aplicado
- `Shared/Extensions/BlobSlugHelper.cs` — clase estática pública `BlobSlugHelper.ToAsciiSlug(string)` con la lógica NFD+strip NonSpacingMark. Fuente única de verdad.
- `SyncCurriculumJob.cs` — eliminado método privado `ToAsciiSlug()`; usa `BlobSlugHelper.ToAsciiSlug()`.
- `CurriculumModule.cs` — reemplazado `asignatura.ToLowerInvariant().Replace(" ", "-")` por `BlobSlugHelper.ToAsciiSlug(asignatura)`.

### Verificación
- Build: 0 errores, 0 advertencias.
- Subir un PDF de "Artes Plásticas" genera blob name `artes-plasticas/<uuid>.pdf` (sin acento).

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
~~`AdecuacionAiService.cs` y `PlaneamientoAiService.cs` también instancian `AzureOpenAIClient` por llamada~~ → **Resuelto 2026-05-08**: ambos servicios migrados a singleton inyectado por DI (ver ISSUE-004).

---

## ✅ ISSUE-004: AzureOpenAIClient por llamada en AdecuacionAiService y PlaneamientoAiService

**Detectado:** 2026-05-08 (preventivo — mismo patrón que ISSUE-002)
**Estado:** ✅ Resuelto
**Componentes:** `AdecuacionAiService.cs`, `PlaneamientoAiService.cs`

### Síntoma
Ambos servicios instanciaban `new AzureOpenAIClient(...)` dentro de cada método de generación IA, creando un `HttpClient` sin pool por llamada. Riesgo de `NetworkTimeout` en producción bajo carga o PDFs grandes.

### Fix aplicado
`AdecuacionAiService` y `PlaneamientoAiService` — constructor recibe `AzureOpenAIClient aiClient` inyectado desde DI (singleton registrado en `OptionsExtensions.cs`). Se usa `aiClient.GetChatClient(aiOpts.Value.DeploymentName)` en lugar de instanciar propio.

---

## ✅ ISSUE-005: Hangfire dashboard sin auth browser-friendly en producción

**Detectado:** 2026-05-08
**Estado:** ✅ Resuelto
**Componentes:** `HangfireExtensions.cs`, `Program.cs`, `callback/page.tsx`, `page.tsx`

### Síntoma
Navegar a `https://mep.ezekl.com/hangfire` en el browser devolvía 401 sin forma de autenticarse desde el navegador (solo con token JWT en header, imposible en browser estándar).

### Causa raíz (múltiple, iterativa)
1. **Middleware ejecutaba `AuthenticateAsync` después de `UseAuthentication()`** — JwtBearer ya había procesado el header vacío y cacheado el resultado fallido. Re-invocar retornaba el cache → siempre fallo.
2. **CDN auth0-spa-js** (`createAuth0Client`) fallaba con `is not defined` — problema de CSP o carga del script externo.
3. **Loop de encicle** — cuando el token en localStorage no se encontraba por clave exacta (el scope puede incluir claims extra), la página redirigía a `/?hangfire_return=1`, el SPA detectaba usuario autenticado y regresaba a `/hangfire-login` indefinidamente.

### Fix aplicado
- `UseAulaIAHangfireCookieInjection()` — middleware nuevo registrado **antes** de `UseAuthentication()` en `Program.cs`. Inyecta la cookie `hangfire_token` como `Authorization: Bearer` header para que JwtBearer lo procese en su primera pasada.
- `UseAulaIAHangfire()` — solo lee `ctx.User.Identity.IsAuthenticated` + claim admin. No llama `AuthenticateAsync`.
- `/hangfire-login` — HTML inline sin CDN. Lee token de localStorage por **prefijo** (`@@auth0spajs@@::<clientId>::`) escaneando todas las claves; resiste variaciones en el scope.
- `/hangfire-session` (POST) — valida JWT, verifica rol admin, guarda cookie HttpOnly Secure SameSite=Strict 8h.
- `page.tsx` — detecta `?hangfire_return=1`; si ya autenticado usa `getAccessTokenSilently()` (SDK Auth0, no localStorage) → POST directo a `/hangfire-session` → corta el loop.
- `callback/page.tsx` — respeta `appState.returnTo` en lugar de siempre ir a `/`.

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
- Deploy revisión `ca-aulaia-api--0000005` con parse corregido (imagen `6294008` reemplazada con nuevo contenido, mismo tag por falta de commit entre deploys).
- Próxima ejecución diaria (12h UTC) debe guardar el TC en `exchange_rates`.

---

## ✅ ISSUE-007: ExtractCurriculumJob trunca PDFs grandes → IA devuelve `[]` (Matemáticas 1.26M chars)

**Detectado:** 2026-05-10 (job 58 — Matemáticas III Ciclo)
**Estado:** ✅ Resuelto
**Componentes:** `ExtractCurriculumJob.cs`

### Síntoma
`ExtractCurriculumJob` para Matemáticas (PDF 1.26M chars) enviaba el texto completo en un único user message. GPT-5.5 procesaba solo los primeros ~350k chars (el inicio del PDF contiene I y II Ciclo; III Ciclo empieza en ~60% del archivo) y devolvía `[]` para el fragmento del III Ciclo, que quedaba truncado fuera del contexto.

### Causa raíz
El método `ExtractUnitsWithAiAsync` enviaba `pdfText` completo sin truncar ni dividir. Para PDFs > 350k chars, el contexto del LLM se llenaba con la parte inicial (currículo de primaria) y el contenido de III Ciclo nunca era visible para el modelo.

### Fix aplicado
- `ChunkSize = 300_000` / `ChunkOverlap = 30_000` — el texto se divide en bloques con overlap para no cortar unidades a mitad.
- `BuildChunks()` — retorna lista de bloques; si el PDF cabe en uno solo, se comporta igual que antes.
- Deduplicación por `(Nivel, Trimestre, UnidadNumero)` acumulada entre todos los bloques.
- JSON inválido en un bloque → `continue` (no lanza excepción).

### Verificación
- Job 58 (re-run): 5 bloques procesados → 12 unidades extraídas → guardadas en BD.
- 333.977 tokens consumidos en total.

---

## ✅ ISSUE-008: GPT inventa trimestres para Matemáticas (estructura PDF por áreas, no por trimestres)

**Detectado:** 2026-05-10 (job 58 post-chunking — 13 unidades con trimestres incorrectos)
**Estado:** ✅ Resuelto
**Componentes:** `ExtractCurriculumJob.cs`

### Síntoma
Después de aplicar chunking (ISSUE-007), el job extrajo 13 unidades con trimestres inventados (7/T1 Estadística, 10/T1 Geometría, etc.). El PDF de Matemáticas MEP no usa trimestres explícitos — está organizado por 4 áreas matemáticas. GPT asignaba trimestres arbitrariamente.

### Causa raíz
El sistema prompt no contenía instrucciones específicas para asignaturas cuya estructura de PDF difiere del formato estándar MEP (trimestre 1/2/3 por unidad).

### Fix aplicado
- `GetSubjectHint(asignatura, ciclo)` — método privado que retorna instrucciones adicionales según la asignatura.
- Para `("Matemáticas", "III Ciclo")`: inyecta el mapeo estándar MEP área→trimestre (Números→T1, Geometría→T2, Relaciones y Álgebra→T3, Estadística y Probabilidad→T3) en el user message.
- Sistema prompt actualizado: añade regla `"NUNCA inventes trimestres: asigna el valor EXACTO indicado en las instrucciones de asignatura"`.
- Datos incorrectos (13 unidades) eliminados manualmente de la BD; job re-ejecutado.

### Verificación
- Job 58 (re-run con hint): 12 unidades correctas (4 áreas × 3 niveles 7°/8°/9°) con trimestres exactos.
- BD validada: `SELECT Nivel, Trimestre, UnidadNumero, UnidadNombre FROM curriculum_units WHERE Asignatura = 'Matemáticas' ORDER BY Nivel, Trimestre, UnidadNumero` → 12 filas correctas.

