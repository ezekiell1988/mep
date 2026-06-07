# GitHub Copilot — Instrucciones del Proyecto AulaIA

## 1. Flujo obligatorio para cualquier tarea de código

Antes de escribir una sola línea de código, seguir este flujo en orden:

### 1.1 Planear
1. Leer `ia/00_context.md` (siempre, es el punto de entrada).
2. Leer los archivos relevantes según el tipo de tarea:
   - `ia/01_requirements.md` — si la tarea toca reglas de negocio o flujos.
   - `ia/02_architecture.md` — si se agrega un servicio, se cambia infraestructura o se tocan pipelines.
   - `ia/03_plan.md` — para saber en qué fase está el proyecto.
   - `ia/04_tasks.md` — para ver si la tarea ya está definida como TASK-*; si no existe, crearla siguiendo el esquema del README.
   - `ia/09_patterns.md` — **siempre antes de codificar** — es la primera consulta técnica después del contexto.
3. Si la tarea no existe en `04_tasks.md`, agregarla con todos los campos del esquema (`Context`, `Steps`, `Expected Output`, `Implementation hint`) antes de continuar.
4. Marcar la tarea como `🔄 En progreso` en `04_tasks.md`.

### 1.2 Ejecutar
- Implementar siguiendo exactamente los `Steps` y el `Implementation hint` de la tarea.
- Usar los patrones de `ia/09_patterns.md`; si se crea un patrón nuevo y funciona, agregarlo al final del archivo.
- No agregar features, refactors ni mejoras fuera del alcance de la tarea actual.

### 1.3 Actualizar los MDs
Al finalizar la tarea, actualizar **obligatoriamente**:

| Archivo | Qué actualizar |
|---------|----------------|
| `ia/04_tasks.md` | Marcar tarea como `✅ Completado`; agregar resultado real en `Expected Output`. |
| `ia/05_progress.md` | Mover el ítem a la sección `✅ Completado`; una línea con archivo y descripción. |
| `ia/09_patterns.md` | Si se validó un patrón nuevo, agregarlo con fecha y ambiente. |
| `ia/07_issues.md` | Si apareció un bug o limitación conocida, registrarlo como `ISSUE-NN`. |
| `ia/06_decisions.md` | Si se tomó una decisión arquitectónica no trivial, registrarla como `ADR-NN`. |

> **Regla:** `05_progress.md` se actualiza al terminar cada sesión de desarrollo, no al final del proyecto.

---

## 2. Flujo de debugging

Cuando se encuentre un bug, un comportamiento inesperado o se necesite entender qué está pasando en tiempo de ejecución, seguir el flujo de debugging correspondiente al tipo de código afectado.

### 2.1 Frontend (Angular, Next.js, Ionic, componentes de UI)
Usar el skill **`playwright-design-review`**:

```
Instrucción al skill: describe el comportamiento inesperado, la ruta o componente afectado,
y los estados de red o datos que hay que interceptar.
```

El skill abre el navegador con Playwright, captura screenshots, intercepta respuestas de API
y entrega evidencia visual + logs de red para diagnosticar sin modificar el código de producción.

### 2.2 Backend (.NET API, jobs de Hangfire, servicios, pipelines)
Usar el skill **`mep-dotnet-ai-runtime-audit`**:

```
Instrucción al skill: describe qué endpoint, job o servicio falla, con qué parámetros
y cuál es el comportamiento observado vs esperado.
```

El skill instrumenta el código con entradas de audit log legibles por LLMs:
- En **desarrollo local**: escribe en `AulaIA.Api/logs/llm-audit.md`.
- En **contenedor remoto**: persiste en la tabla `llm_audit_entries` de PostgreSQL
  (activar con `LlmAudit__PersistToDb=true`).

Los endpoints `/diag/*` son **solo de escritura** — el LLM lee los logs con los scripts PowerShell
de `examples/` incluidos en el skill, nunca vía GET.

> **Regla:** No hacer `Console.WriteLine` ni `_logger.LogInformation` temporales en código de producción
> como herramienta de debug. Usar el sistema de audit log del skill para que el diagnóstico sea
> reproducible y no ensucie el historial de git.

---

## 3. Referencia rápida de los archivos `ia/`

| Archivo | Leer cuando… |
|---------|--------------|
| `ia/00_context.md` | Siempre — es el punto de entrada |
| `ia/01_requirements.md` | Se diseñen features o flujos |
| `ia/02_architecture.md` | Se agreguen servicios o se cambie infraestructura |
| `ia/03_plan.md` | Se evalúe alcance o se planifique trabajo |
| `ia/04_tasks.md` | Se empiece a codificar cualquier feature |
| `ia/05_progress.md` | Se quiera saber qué está hecho y qué falta |
| `ia/06_decisions.md` | Se cuestione un patrón o se evalúe un cambio |
| `ia/07_issues.md` | Se investigue un comportamiento inesperado |
| `ia/08_retrospective.md` | Se cierre una fase o sprint |
| `ia/09_patterns.md` | Antes de implementar cualquier feature nuevo |
