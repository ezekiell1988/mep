# 00 — Contexto del Proyecto

> **Última actualización:** 2026-05-04
> **Scope:** `/` (raíz del repositorio — código aún no iniciado; fase de planificación)

---

## Identidad del proyecto

**Nombre de trabajo:** AulaIA *(nombre definitivo pendiente; alternativas: ProfeBot, MEPDesk)*
**Propósito:** Asistente pedagógico digital para docentes del MEP de Costa Rica. Automatiza el planeamiento didáctico con IA, la toma de asistencia con QR, el registro de notas, las adecuaciones curriculares y la generación de reportes en los formatos del MEP.
**Cliente / Co-fundadora pedagógica:** Adriana Guido — docente de Artes Plásticas, 24 años de experiencia, plaza en Colegio de Aserrí, zona sur de San José.
**Repositorio:** `ezekiell1988/mep` (GitHub) — rama principal: `main`
**Modelo de negocio:** SaaS por suscripción mensual (Básico $5–8 / Profesional $12–18 / Institucional $80–150 USD)
**Dominio:** `mep.ezekl.com` (web) · `api.mep.ezekl.com` (API backend) — DNS gestionado en Cloudflare

---

## Stack tecnológico

| Capa | Tecnología | Notas |
|------|-----------|-------|
| App iOS / Android | React Native (Expo) | Un solo código TypeScript para ambas plataformas |
| App Web | Next.js (React) | Comparte lógica con React Native; SSR; tareas de escritorio |
| Backend / API | .NET 10 (ASP.NET Core Minimal APIs) | Decidido — ver ADR-001 en `06_decisions.md` |
| Base de datos (servidor) | PostgreSQL | Relacional; notas, asistencia, grupos |\n| ORM | Entity Framework Core 10 | Code-first, migraciones, LINQ — ver ADR-001 |
| Base de datos (local) | SQLite (móvil) + IndexedDB (web) | Almacenamiento offline estructurado |
| Sincronización offline | PowerSync | Decidido — ver ADR-005 en `06_decisions.md` |
| IA / LLM | Azure AI Foundry — GPT-5.5 | Decidido — ver ADR-002 en `06_decisions.md` |
| Lector de QR | Expo Camera / mobile_scanner | Funciona offline; para asistencia en el aula |
| Autenticación | Auth0 | Decidido — ver ADR-006 en `06_decisions.md` |
| Almacenamiento de archivos | Azure Blob Storage | Decidido — ver ADR-003 en `06_decisions.md` |
| Hosting (Fase 1–2) | Azure App Service (tier B1/S1) | Decidido — ver ADR-004 en `06_decisions.md` |
| Hosting (Fase 3+) | Azure Container Apps | Migración planeada cuando se escalen módulos — ver ADR-004 |
| Notificaciones push | Firebase Cloud Messaging (FCM) | iOS y Android |
| DNS / CDN / Proxy | Cloudflare | Dominio `mep.ezekl.com`; WAF, SSL, caché de borde |

---

## Módulos del sistema

| # | Módulo | Prioridad | Estado |
|---|--------|-----------|--------|
| 1 | Gestión de Grupos y Estudiantes | 🔴 Core — todo depende de este | ⏳ No iniciado |
| 2 | Planeamiento Didáctico con IA | 🔴 Diferenciador principal | ⏳ No iniciado |
| 3 | Asistencia con QR | 🔴 Módulo estrella (viralidad) | ⏳ No iniciado |
| 4 | Trabajo Cotidiano y Tareas | 🟡 Alta prioridad | ⏳ No iniciado |
| 5 | Gestión de Notas y Promedios | 🟡 Alta prioridad | ⏳ No iniciado |
| 6 | Generador de Reportes e Informes | 🟡 Alta prioridad | ⏳ No iniciado |
| 7 | Calendario Escolar Integrado | 🟢 Soporte | ⏳ No iniciado |
| 8 | Adecuaciones Curriculares | 🟡 Diferenciador secundario | ⏳ No iniciado |

---

## Constantes críticas

> Estos valores **NO deben cambiar sin una decisión registrada en `06_decisions.md`**.

- **Sistema de calificación del MEP:** escala del 1 al 100. Aprobatorio: 65 (primaria y secundaria básica), 70 (diversificada).
- **Ponderación de evaluación en secundaria (configurable):** Trabajo cotidiano 20% / Pruebas 45% / Trabajo extraclase 20% / Otros 15%.
- **Calendario escolar MEP:** 200 días lectivos por año.
- **Ciclos del sistema educativo:** I Ciclo (1°–3°) / II Ciclo (4°–6°) / III Ciclo — EGB (7°–9°) / Diversificado (10°–11°/12°).
- **Plataformas del MEP a considerar para exportación:** SIMAR (notas y matrícula), SIRC (reportes de calificaciones). No hay API pública; la integración es por exportación de archivos.
- **Offline-first obligatorio:** la app debe funcionar completamente sin internet para: asistencia (QR y manual), registro de notas, consulta de grupos y planeamientos guardados.
- **Programa de estudio inicial (caso de uso base):** Artes Plásticas — III Ciclo (7°, 8°, 9°) — programa oficial del MEP.
- **La IA no inventa contenido:** el generador de planeamientos está anclado al programa oficial del MEP; nunca debe generar aprendizajes esperados o indicadores que no existan en el programa.

---

## Contexto de dominio clave

- **Docente objetivo:** secundaria pública, maneja 8–15 secciones, 30–40 estudiantes por sección (hasta 600 estudiantes simultáneos).
- **Dolor principal:** doble trabajo — el docente registra la misma información en su herramienta personal (Excel/papel) y luego la transcribe al sistema del MEP.
- **Sin competencia directa en Costa Rica.** Los docentes usan Excel, Word, papel y WhatsApp por falta de una herramienta unificada.
- **Estrategia de entrada:** Beachhead en docentes de Artes Plásticas, Circuito 06 de San José (zona de influencia directa de Adriana Guido). Expansión en ondas concéntricas.
- **Referencia de dominio completa:** `ia/assets/investigacion_dominio.md`
- **Visión de producto completa:** `ia/assets/vision_producto_integral.md`
- **Requerimiento original:** `ia/assets/requerimiento.txt`

---

## Estructura de carpetas del repositorio

```
mep/
├── ia/                          ← Fuente de verdad para agentes de IA
│   ├── 00_context.md            ← Este archivo — contexto permanente del proyecto
│   ├── 01_requirements.md       ← Requisitos funcionales y reglas de negocio
│   ├── 02_architecture.md       ← Arquitectura técnica y diagramas
│   ├── 03_plan.md               ← Fases de desarrollo y estado
│   ├── 04_tasks.md              ← Tareas accionables con pasos
│   ├── 05_progress.md           ← Progreso por componente y fase
│   ├── 06_decisions.md          ← ADRs — decisiones y razones
│   ├── 07_issues.md             ← Bugs y problemas conocidos
│   ├── 08_retrospective.md      ← Aprendizajes por fase
│   ├── 09_patterns.md           ← Patrones verificados de código
│   └── assets/
│       ├── requerimiento.txt           ← Documento original del cliente
│       ├── investigacion_dominio.md    ← Investigación profunda del dominio MEP
│       └── vision_producto_integral.md ← Análisis de viabilidad y visión de producto
└── assets/                      ← Recursos estáticos del proyecto (vacío aún)
```

*(El código fuente de la aplicación no existe aún. Se creará en la Fase 1 del plan de desarrollo.)*
