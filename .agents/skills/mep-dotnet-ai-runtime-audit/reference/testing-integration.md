# Tests de integración con llm-audit-runtime

Protocolo para usar el sistema de audit durante la ejecución de tests de integración.

---

## Objetivo

Cuando un test lanza un job background (Hangfire, Channels, hosted service), el LLM necesita evidencia de qué ocurrió internamente. El audit permite leer esa evidencia directamente desde el archivo MD o la BD.

---

## Protocolo recomendado

### 1. Limpiar audit antes del test

```bash
# PS1 — desde la raíz del repo:
pwsh examples/diag.ps1 clear

# O curl directo:
curl -s -X DELETE http://localhost:{API_PORT}/api/diag/audit
```

### 2. Ejecutar el test

```bash
dotnet test src/MyApp.Tests --filter "Category=Integration" --no-build
```

### 3. Leer evidencia

```bash
# Dev local — desde el archivo MD:
pwsh examples/read-audit.ps1 -Filter "ERROR"

# Contenedor remoto — desde la BD:
pwsh examples/read-db-audit.ps1 -Filter "ERROR" -Limit 50
```

---

## En proyectos .NET con xUnit / nUnit

```csharp
public class MyJobIntegrationTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Job_ShouldComplete_WithoutErrors()
    {
        // Arrange — limpiar audit
        var client = factory.CreateClient();
        await client.DeleteAsync("/api/diag/audit");

        // Act — disparar el job
        await client.PostAsJsonAsync("/api/jobs/trigger", new { param = "test" });
        await Task.Delay(2000);   // dar tiempo al job

        // Assert — verificar en el audit que no hay errores
        // El LLM lee el archivo o BD directamente; aquí solo verificamos el endpoint de escritura
        var clearResponse = await client.DeleteAsync("/api/diag/audit");
        Assert.Equal(HttpStatusCode.NoContent, clearResponse.StatusCode);
    }
}
```

---

## Patrón observado en logs

Cuando el job funciona correctamente, el LLM encontrará en el audit:

```markdown
## [EVENT] MiJob — 2025-01-15T10:30:00Z
Intent: Iniciando
Result: param=test

## [EVENT] MiJob — 2025-01-15T10:30:01Z
Intent: Completado
Result: ✅ 42 items procesados
```

Cuando falla:

```markdown
## [ERROR] MiJob — 2025-01-15T10:30:01Z
❌ Falló la ejecución
Exception: `HttpRequestException` — Connection refused
```

---

## Lectura directa del audit (LLM, no endpoint GET)

El LLM **nunca** usa un endpoint GET para leer el audit. Lee directamente:

```powershell
# Desde la raíz del repo (requiere audit.config.ps1):
pwsh examples/read-audit.ps1               # dev local
pwsh examples/read-db-audit.ps1 -Since 1h  # contenedor remoto
```

O directamente con el tool `read_file`:
- Dev local: leer el archivo `logs/llm-audit.md` (ruta en `LlmAuditOptions.LogPath`)
- Contenedor remoto: ejecutar PS1 que consulta `llm_audit_entries` en PostgreSQL

---

## appsettings.test.json — configuración para tests

```json
{
  "LlmAudit": {
    "Enabled": true,
    "LogPath": "logs/llm-audit-test.md",
    "PersistToDb": false
  }
}
```

> En CI/CD, normalmente `Enabled: false` para no generar archivos MD en el runner.
