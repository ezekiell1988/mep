# /ia — Sistema de Contexto Estructurado para LLMs

Esta carpeta es la **fuente de verdad del proyecto para agentes de IA**.  
Cada archivo tiene un propósito fijo y un esquema definido. Un LLM debe leer estos archivos al inicio de cada sesión y puede recrearlos si están vacíos o desactualizados, siguiendo los esquemas de este README.

---

## Índice de archivos

| Archivo | Propósito | Leer cuando… |
|---------|-----------|--------------|
| `00_context.md` | Identidad, stack, constantes y estructura del proyecto | Siempre — es el punto de entrada |
| `01_requirements.md` | Requisitos funcionales y reglas de negocio | Se diseñen features o flujos |
| `02_architecture.md` | Arquitectura técnica, pipelines y diagramas | Se agreguen servicios o se cambie infraestructura |
| `03_plan.md` | Fases de desarrollo y estado de cada fase | Se evalúe alcance o se planifique trabajo |
| `04_tasks.md` | Tareas accionables con estado, pasos y resultado esperado | Se empiece a codificar cualquier feature |
| `05_progress.md` | Progreso detallado por componente y fase | Se quiera saber qué está hecho y qué falta |
| `06_decisions.md` | ADRs — por qué se tomaron decisiones clave | Se cuestione un patrón o se evalúe un cambio |
| `07_issues.md` | Bugs conocidos y problemas pendientes | Se investigue un comportamiento inesperado |
| `08_retrospective.md` | Aprendizajes y mejoras identificadas por fase | Se cierre una fase o sprint |
| `09_patterns.md` | Patrones verificados de código: queries, DI, jobs, LLM | Se implemente cualquier feature nuevo — **leer antes de codificar** |

---

## Esquemas por archivo

### `00_context.md` — Contexto del Proyecto

Contiene la identidad permanente del proyecto. No cambia a menos que cambie el stack o el cliente.

```markdown
# 00 — Contexto del Proyecto

> **Última actualización:** {fecha}
> **Scope:** {ruta del código activo}

## Identidad del proyecto
**Nombre:** {nombre}
**Propósito:** {qué hace}
**Cliente:** {cliente}

## Stack tecnológico
| Capa | Tecnología |
|------|-----------|
| ...  | ...        |

## Constantes críticas
{listado de valores que NO deben cambiar sin ADR}

## Estructura de carpetas clave
{árbol del proyecto con descripciones inline}
```

---

### `01_requirements.md` — Requisitos del Sistema

Describe QUÉ debe hacer el sistema desde la perspectiva del negocio. No incluye cómo se implementa.

```markdown
# 01 — Requisitos del Sistema

> **Última actualización:** {fecha}
> **Fuentes:** {links o archivos de referencia}

## Propósito del sistema
{descripción de una o dos frases}

## Reglas fundamentales de negocio
{lista de invariantes: cosas que NUNCA deben romperse}

## Flujos principales
### Flujo {Nombre}
**Estado de entrada:** {estado}
**Estado de salida:** {estado}
{descripción paso a paso o tabla de intents}
```

---

### `02_architecture.md` — Arquitectura del Sistema

Describe CÓMO está construido. Incluye diagramas ASCII, contratos de interfaces clave y flujo de datos.

```markdown
# 02 — Arquitectura del Sistema

> **Última actualización:** {fecha}
> **Scope:** {proyecto}

## Pipeline principal
{diagrama ASCII del flujo de datos entre componentes}

## Componentes clave
| Componente | Responsabilidad | Archivo |
|-----------|-----------------|---------|
| ...       | ...             | ...     |

## Contratos de interfaces críticas
{firma de interfaces o records importantes}
```

---

### `03_plan.md` — Plan de Desarrollo

Organiza el trabajo en fases. Es la vista de alto nivel; el detalle va en `04_tasks.md`.

```markdown
# 03 — Plan de Desarrollo

> **Última actualización:** {fecha}

## Visión general
{una frase del estado actual}

## Fases del proyecto
### Fase {N} — {Nombre} {estado: ✅ Completada / 🔄 En progreso / ⏳ Pendiente}
| Componente | Estado |
|-----------|--------|
| ...       | ✅ / 🔄 / ⏳ |
```

---

### `04_tasks.md` — Tareas Accionables

Una tarea por sección. Formato estrictamente accionable: un LLM debe poder implementar la tarea leyendo solo esta sección.

```markdown
# 04 — Tareas Accionables

> **Última actualización:** {fecha}
> **Prioridad actual:** {descripción}

## TASK-{PREFIJO}-{NN}: {Título corto}
**Estado:** {⏳ Pendiente | 🔄 En progreso | ✅ Completado}

Title: {título expandido}

Context:
{por qué existe esta tarea, qué problema resuelve}

Steps:
1. {paso concreto}
2. {paso concreto}

Expected Output:
{archivo(s) que deben existir al terminar, o comportamiento esperado}

Dependencies:
{otras tareas que deben estar completas antes}

Implementation hint:
- Archivo: {ruta exacta del archivo a crear o modificar}
- Patrón de referencia: {patrón de 09_patterns.md a seguir, si aplica}
- Inyectar: {dependencias ya registradas en DI que debe recibir}
- Firma esperada: {método o clase principal con su signature}
- Notas críticas: {restricciones, casos edge o errores conocidos relevantes}
```

> **Regla:** `Expected Output` debe ser verificable. Si no se puede verificar, la tarea está mal definida.

> **Regla Copilot-ready:** Una tarea es "Copilot-ready" cuando tiene `Implementation hint` completo. Con ese bloque, el prompt a Copilot puede ser tan simple como: *"Implementa TASK-BOT-XXX del 04_tasks.md"*. Sin ese bloque, Copilot necesita preguntas de aclaración adicionales.

---

### `05_progress.md` — Progreso del Proyecto

Estado actualizado de todo lo que está hecho y todo lo que falta. Se actualiza después de cada sesión de desarrollo.

```markdown
# 05 — Progreso del Proyecto

> **Última actualización:** {fecha}
> **Fase activa:** {nombre de la fase en curso}

## ✅ Completado
### {Nombre de fase}
- `{Archivo.cs}` — {qué hace, una línea}

## 🔄 Pendiente — {Nombre de fase}
> **Decisiones de diseño confirmadas ({fecha}):**
> - {decisión relevante para implementar lo pendiente}

### PC-{NN}: {Nombre del step/componente}
{descripción de lo que falta}
```

---

### `06_decisions.md` — Decisiones Arquitectónicas (ADRs)

Registro inmutable de decisiones. Nunca se borran; si una decisión cambia, se agrega un nuevo ADR que la revisa.

```markdown
# 06 — Decisiones Arquitectónicas (ADRs)

## ADR-{NN}: {Título}
**Decisión:** {qué se decidió hacer}
**Razón:** {por qué — la causa raíz, no la consecuencia}
**Alternativas descartadas:** {opcional — qué más se evaluó y por qué no}
```

---

### `07_issues.md` — Issues Conocidos

Bugs, limitaciones o comportamientos inesperados que están documentados pero no resueltos.

```markdown
# 07 — Issues Conocidos

## ISSUE-{NN}: {Título}
**Severidad:** {critical | high | medium | low}
**Estado:** {abierto | en investigación | workaround aplicado}
**Descripción:** {qué ocurre}
**Reproducción:** {cómo reproducirlo}
**Workaround:** {si existe}
**Fix propuesto:** {si se sabe cómo arreglarlo}
```

---

### `08_retrospective.md` — Retrospectiva

Aprendizajes capturados al cerrar una fase. Sirven para mejorar el proceso en la siguiente.

```markdown
# 08 — Retrospectiva

## Fase {N} — {Nombre} ({fecha de cierre})

### ✅ Qué funcionó bien
- {aprendizaje}

### ⚠️ Qué mejorar
- {problema y sugerencia concreta}

### 💡 Decisiones que cambiaría
- {decisión tomada} → {cómo la cambiaría ahora}
```

---

### `09_patterns.md` — Patrones Verificados

Catálogo de patrones de código que **ya funcionaron y fueron validados** en producción o en testing manual. Su propósito es evitar que Copilot repita errores ya resueltos o invente variantes de patrones que el proyecto no usa.

> **Cuándo leer:** Antes de implementar cualquier feature nuevo — es la primera consulta técnica, después del contexto de la tarea.

> **Cuándo actualizar:** Después de validar en producción o testing un patrón nuevo. No agregar patrones especulativos — solo lo que se probó.

```markdown
# 09 — Patrones Verificados

> **Última actualización:** {fecha}

## Patrón: {Nombre del patrón}
**Verificado:** {fecha de validación} · {ambiente: Dev / Prod / Test}
**Archivo de referencia:** `{ruta al archivo donde está implementado}`

```csharp
// {código mínimo representativo del patrón}
```

**Reglas críticas:**
- {restricción 1 — errores comunes a evitar}
- {restricción 2}

**Anti-patrón conocido:**
```csharp
// ❌ NO hacer esto
```
```

---

## Carpetas

### `prompts/`
Prompts reutilizables para tareas recurrentes: generación de código, refactorización, debugging, análisis. Cada prompt es un archivo `.md` independiente con nombre descriptivo.

### `skills/`
Carpeta reservada — actualmente vacía.

> **Los skills del proyecto viven en `.agents/skills/`**, no aquí.  
> VS Code / GitHub Copilot los detecta automáticamente desde esa ruta y los activa sin intervención manual.  
> Skills activos del proyecto: `dotnet-10-csharp-14`, `dotnet-llm-audit`, `voice-bot-dotnet`, `voice-bot-cloudflare-tunnel`, `voice-bot-db-access`, entre otros.  
> Para agregar un nuevo skill, crear el directorio en `.agents/skills/{nombre}/SKILL.md`.

---

## Reglas de mantenimiento

1. **`00_context.md` y `06_decisions.md`** son los más críticos — mantenerlos siempre actualizados.
2. **`04_tasks.md`** se actualiza antes de empezar a codificar, no después.
3. **`05_progress.md`** se actualiza al finalizar cada sesión de desarrollo.
4. Si un archivo está vacío, el LLM debe crearlo usando el esquema de este README más el contexto del proyecto en `00_context.md`.
5. **Nunca borrar** ADRs ni tasks completadas — son historial de decisiones.
6. **`09_patterns.md`** se actualiza solo con patrones validados — nunca especulativos. Un patrón sin validación va en un comentario de tarea, no en este archivo.
7. Al agregar `Implementation hint` a una tarea en `04_tasks.md`, referenciar el patrón de `09_patterns.md` por nombre exacto si aplica.