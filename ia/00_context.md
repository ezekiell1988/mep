# 00 — Contexto del Proyecto

> **Última actualización:** 2026-05-08 (rev 3)
> **Scope:** `/` (raíz del repositorio)

---

## Identidad del proyecto

**Nombre de trabajo:** AulaIA *(nombre definitivo pendiente; alternativas: ProfeBot, MEPDesk)*
**Propósito:** Asistente pedagógico digital para docentes del MEP de Costa Rica. Automatiza el planeamiento didáctico con IA, la toma de asistencia con QR, el registro de notas, las adecuaciones curriculares y la generación de reportes en los formatos del MEP.
**Socia comercial / Directora Pedagógica:** Adriana Guido — docente de Artes Plásticas, 24 años de experiencia, plaza en Colegio de Aserrí, zona sur de San José. Rol: ventas, validación pedagógica y cara educativa del producto. Compensación: comisión del 20% de la suscripción mensual de cada usuario que ella refiera directamente, durante 12 meses desde la suscripción del usuario. Ver ADR-008.
**Desarrollador y dueño de infraestructura:** Ezequiel Baltodano — 100% del desarrollo técnico y costos de infraestructura Azure. Los costos de infraestructura se descuentan de ingresos brutos antes de calcular cualquier comisión.
**Repositorio:** `ezekiell1988/mep` (GitHub) — rama principal: `main`
**Modelo de negocio:** SaaS por suscripción mensual

| Plan | Precio/mes | Para quién | Qué incluye |
|------|-----------|------------|-------------|
| **Básico** | $6 USD | Docente individual | Planeamiento + asistencia + notas básicas (máx. 5 grupos) |
| **Profesional** | $15 USD | Docente con muchos grupos | Todo + adecuaciones + reportes exportables + sin límite de grupos |
| **Institucional** | $100 USD/mes | Colegio completo | Todos los docentes + panel de director + reportes institucionales |

**Contexto de precio:** $10–15/mes equivale al 0.7–1.3% del salario mensual de un docente MEP (₡600,000–900,000 ≈ $1,100–1,700). Comparable con Spotify.
**Dominio:** `mep.ezekl.com` (web) · `api.mep.ezekl.com` (API backend) — DNS gestionado en Cloudflare
**Azure Tenant ID:** `2f80d4e1-da0e-4b6d-84da-30f67e280e4b`
**Azure Resource Group:** `rg-ezequiel`

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
| Pasarela de pagos | SINPE Móvil (manual) | Sin API; flujo: usuario transfiere → sube comprobante → admin aprueba. Ver ADR-009. |
| Tipo de cambio USD/CRC | API BCCR (indicador 318) | Job Hangfire diario `UpdateExchangeRateJob` — actualiza tabla `exchange_rates` en PG. SINPE solo opera en CRC. |

---

## Módulos del sistema

| # | Módulo | Prioridad | Estado |
|---|--------|-----------|--------|
| 1 | Gestión de Grupos y Estudiantes | 🔴 Core — prerequisito técnico de todo | ✅ Completado (Fase 1) |
| 2 | Planeamiento Didáctico con IA | 🔴 **Prioridad #1 según Adriana — diferenciador principal** | ✅ Completado (Fase 2) |
| 3 | Calendario Escolar Integrado | 🔴 Crítico — alimenta y reorganiza el planeamiento | ✅ Completado (Fase 2) |
| 4 | Asistencia con QR | 🔴 Módulo estrella (viralidad móvil) | ✅ Completado (Fase 1) |
| 5 | Adecuaciones Curriculares | 🔴 Diferenciador secundario + planeamiento individual | ✅ Completado (Fase 4) |
| 6 | Trabajo Cotidiano y Tareas | 🟡 Alta prioridad | ✅ Completado (Fase 3) |
| 7 | Gestión de Notas y Promedios | 🟡 Alta prioridad | ✅ Completado (Fase 3) |
| 8 | Generador de Reportes e Informes | 🟡 Alta prioridad | ✅ Completado (Fase 3/4) |
| 9 | Suscripciones y Pagos (SINPE Móvil) | 🟠 Necesario antes del lanzamiento público | ✅ Completado (Fase 5) |
| 10 | Sistema de Referidos y Comisiones | 🟠 Necesario antes del lanzamiento público — ADR-008 | ✅ Completado (Fase 5) |

---

## Constantes críticas

> Estos valores **NO deben cambiar sin una decisión registrada en `06_decisions.md`**.

- **Sistema de calificación del MEP:** escala del 1 al 100. Aprobatorio: 65 (primaria y secundaria básica), 70 (diversificada).
- **Ponderación de evaluación en secundaria (configurable):** Trabajo cotidiano 20% / Pruebas 45% / Trabajo extraclase 20% / Otros 15%.
- **Calendario escolar MEP:** 200 días lectivos por año, tres trimestres: I (febrero–abril), II (mayo–julio), III (agosto–noviembre).
- **Duración de la lección en secundaria:** 40 minutos. Artes Plásticas: 2 lecciones por semana por grupo.
- **Ciclos del sistema educativo:** I Ciclo (1°–3°) / II Ciclo (4°–6°) / III Ciclo — EGB (7°–9°) / Diversificado (10°–11°/12°).
- **Marco legal de evaluación:** REAC 2026 (Reglamento de Evaluación de los Aprendizajes, MEP). Tipos: diagnóstica (inicio), formativa (proceso continuo), sumativa (nota final del período).
- **Marco legal de adecuaciones:** Ley 7600 (Igualdad de Oportunidades para Personas con Discapacidad, 1996). Tipos: significativa (AS) modifica contenidos/objetivos; no significativa (ANS) ajusta metodología sin modificar objetivos.
- **Plataforma del MEP para exportación:** SEA (Sistema de Evaluación Ágil) — `sea.mep.go.cr`. Sistema web oficial donde los docentes registran calificaciones por período, asignatura y grupo. El SEA permite **descargar un archivo** (Excel/CSV) para completar offline y luego **subirlo** de vuelta — ese archivo es el que AulaIA debe generar. No hay API pública. (Corrección confirmada por Adriana 2026-05-05. Manual oficial: `ia/assets/sea-manual-docente.pdf`)
- **Offline-first obligatorio:** la app debe funcionar completamente sin internet para: asistencia (QR y manual), registro de notas, consulta de grupos y planeamientos guardados.
- **Programa de estudio inicial (caso de uso base):** Artes Plásticas — III Ciclo (7°, 8°, 9°) — programa oficial del MEP.
- **La IA no inventa contenido:** el generador de planeamientos está anclado al programa oficial del MEP; nunca debe generar aprendizajes esperados o indicadores que no existan en el programa.

---

## Contexto de dominio clave

- **Docente objetivo:** secundaria pública, maneja 8–15 secciones, 30–40 estudiantes por sección (hasta 600 estudiantes simultáneos).
- **Carga administrativa del docente:** 3–5 horas semanales adicionales fuera del horario de clase solo en tareas administrativas (planeamiento, transcripción de notas, reportes). Fuente: OEI.
- **Dolor principal:** doble trabajo — el docente registra la misma información en su herramienta personal (Excel/papel) y luego la transcribe al sistema del MEP.
- **Sin competencia directa en Costa Rica.** Los docentes usan Excel, Word, papel y WhatsApp por falta de una herramienta unificada. Referente internacional más cercano en concepto: **Additio App** (España) — libro de calificaciones, asistencia y diario; sin IA, sin integración MEP, sin contexto costarricense.
- **Estrategia de entrada (Beachhead):** docentes de Artes Plásticas, Circuito 06 de San José (zona de influencia de Adriana). Expansión en ondas concéntricas: colegio → circuito → Artes Plásticas nacional → otras materias.
- **Referencia de dominio completa:** `ia/assets/investigacion_dominio.md`
- **Visión de producto completa:** `ia/assets/vision_producto_integral.md`
- **Requerimiento original:** `ia/assets/requerimiento.txt`
- **Manual SEA rol docente (oficial MEP):** `ia/assets/sea-manual-docente.pdf`
- **Manual SEA rol administrativo (oficial MEP):** `ia/assets/sea-manual-administrativo.pdf`

---

## Estrategia comercial

### Proyección de mercado (conservadora)

| Segmento | Docentes CR | Penetración año 1 | Ingresos/mes proyectados |
|----------|------------|-------------------|-------------------------|
| Secundaria pública | ~20,000 | 0.5% (100 usuarios) | $1,200–1,800 |
| Secundaria privada | ~8,000 | 1% (80 usuarios) | $960–1,440 |
| Primaria pública | ~22,000 | 0.2% (44 usuarios) | $220–350 |
| **Total conservador año 1** | — | **~224 docentes** | **~$2,380–3,590/mes** |

Meta 5% del mercado total (~2,500 docentes) → $30,000–45,000/mes.

### Métricas objetivo

| Métrica | Mes 3 | Mes 6 | Mes 12 |
|---------|-------|-------|--------|
| Docentes activos | 20 | 100 | 500 |
| Docentes pagando | 5 | 40 | 200 |
| Colegios con plan institucional | 0 | 1 | 5 |
| NPS | — | > 50 | > 60 |

### Canales de adquisición

1. **WhatsApp** — grupos por materia, circuito y gremio. Canal más efectivo en CR.
2. **Grupos de Facebook de docentes MEP** — grupos con +30,000 miembros.
3. **Reuniones de área** — demostraciones de 15 min en reuniones del Circuito 06.
4. **Universidades públicas** (UCR / UNED / UNA) — estudiantes de Enseñanza de Artes Plásticas como adoptantes tempranos.
5. **IDP** (Instituto de Desarrollo Profesional Uladislao Gámez Solano) — a mediano plazo, inclusión en cursos de actualización.

### Pitch para docentes
> *"¿Cuántas horas te tomó hacer el planeamiento del trimestre pasado? Con esta app, lo haces en 15 minutos. Y tomar lista en cada clase te toma menos de un minuto."*

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
