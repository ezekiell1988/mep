# AulaIA.Tests — Referencia de integración con el audit log

Proyecto: `src/AulaIA.Tests/`  
Stack: xUnit 2.9.3 + FluentAssertions 7.2.0 + .NET 10  
Config: `appsettings.test.json` (gitignoreado) + env vars `AULAIA_TEST_*`

---

## Estructura del proyecto

```
src/AulaIA.Tests/
├── Infrastructure/
│   ├── TestConfig.cs          — lee BaseUrl, Auth0, BearerToken directo
│   ├── Auth0TokenHelper.cs    — obtiene token M2M (client_credentials) con caché
│   ├── ApiClient.cs           — HttpClient con Bearer; CreateAsync() + CreateAnonymous()
│   └── IntegrationTestBase.cs — IAsyncLifetime; expone Api + helpers AssertStatus/ReadJson
├── SanityTests.cs             — /health, 401 sin token, 200 con token
├── CurriculumTests.cs         — upload PDF, validar extracción, flujo completo
├── PlaneamientoTests.cs       — listar, crear sin body, id inexistente, flujo completo
├── appsettings.test.json      — ⛔ gitignoreado — contiene BearerToken real
└── appsettings.test.template.json — ✅ versionado — placeholders
```

---

## Clasificación de tests por autonomía LLM

### ✅ El LLM puede correr solo (deterministas, < 5s)

```bash
# SanityTests — contratos HTTP básicos
dotnet test --filter "FullyQualifiedName~SanityTests" --logger "console;verbosity=normal"

# Validaciones de rechazo — 400, 404 sin dependencias externas
dotnet test --filter "DisplayName~400|DisplayName~404|DisplayName~401" --logger "console;verbosity=normal"
```

Estos tests son seguros para CI/CD y para que un LLM los corra al implementar un endpoint nuevo.

### ⚠️ El LLM necesita observabilidad adicional (jobs en background)

```bash
# Requiere: API corriendo + PDF real + job Hangfire instrumentado con ILlmAuditService
AULAIA_TEST_PdfPath=/ruta/al/programa-artes-plasticas.pdf \
dotnet test --filter "DisplayName~Flujo completo" --logger "console;verbosity=normal"
```

Si el test falla por timeout del polling → leer el audit log **antes** de reintentar:

```bash
# Ver qué hizo el job durante el test
tail -100 src/AulaIA.Api/logs/llm-audit.md
# o vía HTTP:
curl http://localhost:8000/diag/audit | grep -A5 "\[ERROR\]\|\[EVENT\].*Job"
```

**Si no hay eventos del job en el log** → el job no está instrumentado con `ILlmAuditService`. Ver sección "Jobs pendientes de instrumentar" en SKILL.md.

---

## Configuración mínima para correr tests

### appsettings.test.json (gitignoreado — crear localmente)

```json
{
  "Api": {
    "BaseUrl": "http://localhost:8000"
  },
  "Auth0": {
    "Domain": "aulaia-mep.us.auth0.com",
    "ClientId": "",
    "ClientSecret": "",
    "Audience": "https://api.aulaia.mep.go.cr"
  },
  "BearerToken": "PEGAR_TOKEN_ADMIN_AQUI"
}
```

**Opción rápida**: pegar un token con rol `admin` directamente en `BearerToken`. No se necesita ClientId/ClientSecret. El token se puede copiar desde el portal Auth0 (Applications → APIs → Test) o del browser después de login.

**Opción M2M**: configurar ClientId + ClientSecret de una app Machine-to-Machine en Auth0 con permiso `admin`. El `Auth0TokenHelper` lo renueva automáticamente con caché.

### Variables de entorno opcionales

| Variable | Propósito |
|----------|-----------|
| `AULAIA_TEST_PdfPath` | Ruta al PDF real del programa MEP para `CurriculumTests` |
| `AULAIA_TEST_GroupId` | UUID de grupo existente del docente para `PlaneamientoTests` |
| `AULAIA_TEST_Api__BaseUrl` | Override del BaseUrl (ej. apuntar a staging) |

---

## Protocolo completo: test + audit

### Flujo correcto para `FlujoCompleto_UploadValidarListar`

```
1. Limpiar el audit log (nueva sesión limpia)
   DELETE http://localhost:8000/diag/audit

2. Correr el test con el PDF real del programa MEP
   AULAIA_TEST_PdfPath=/ruta/programa-artes-plasticas-iii-ciclo.pdf \
   dotnet test --filter "DisplayName~Flujo completo: upload"

3. Si el test pasa → el job procesó y escribió unidades en BD ✅

4. Si el test falla por timeout →
   a. Leer el audit log: tail -50 src/AulaIA.Api/logs/llm-audit.md
   b. Buscar [ERROR] en ExtractCurriculumJob
   c. Si no hay eventos del job → instrumentarlo primero (ver SKILL.md)
   d. Si hay [ERROR] → corregir la causa raíz (Azure AI config, PDF sin texto, etc.)
```

### PDF correcto vs. incorrecto

| PDF | Resultado esperado del job |
|-----|---------------------------|
| Programa de Artes Plásticas III Ciclo (MEP oficial) | ✅ GPT-5.5 extrae unidades con indicadores, contenidos, estrategias |
| Manual del docente SEA (`sea-manual-docente.pdf`) | ⚠️ GPT-5.5 no encuentra unidades curriculares — log: "0 unidades guardadas" |
| PDF de texto escaneado sin OCR | ⚠️ PdfPig extrae texto vacío — log: "PDF vacío o sin texto extraíble" |

> El PDF correcto es el **Programa de Estudios oficial del MEP** descargable desde:  
> https://www.mep.go.cr/educacion/programas-de-estudio — sección "Artes Plásticas"

---

## Correr todos los tests rápidos (recomendado para CI/CD y LLM)

```bash
cd src/AulaIA.Tests
dotnet test \
  --filter "FullyQualifiedName~SanityTests|DisplayName~400|DisplayName~404|DisplayName~sin token" \
  --logger "console;verbosity=normal"
```

Tiempo esperado: < 10 segundos. Sin dependencias de Azure AI ni Hangfire.
