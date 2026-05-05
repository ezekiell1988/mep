# 03 — Plan de Desarrollo

> **Última actualización:** 2026-05-05
> **Estado general:** 🔄 Fase 1 — En progreso

---

## Visión general

El proyecto se construye en 5 fases progresivas. Las primeras dos fases producen un MVP funcional que Adriana Guido puede usar en el Colegio de Aserrí desde el primer trimestre. Las fases 3 y 4 consolidan los módulos diferenciadores. La Fase 5 migra la infraestructura a Container Apps para soportar escala.

---

## Fase 0 — Infraestructura Azure y Setup del Proyecto ⏳ Pendiente

**Objetivo:** Dejar toda la infraestructura cloud aprovisionada y el repositorio estructurado antes de escribir una sola línea de código de negocio.

**Resource Group:** `rg-ezequiel`
**Convención de nombres:** prefijo `demo` en todos los recursos

### Recursos Azure a crear

| Recurso | Nombre | Tier inicial | Notas |
|---------|--------|-------------|-------|
| Resource Group | `rg-ezequiel` | — | Ya existente |
| App Service Plan | `asp-demo` | B1 | Linux, .NET 10 |
| App Service | `app-demo-api` | B1 | Backend .NET 10 |
| PostgreSQL Flexible Server | `psql-demo` | Burstable B1ms | Version 16 |
| Storage Account | `stdemo` | LRS Standard | Blob Storage |
| Key Vault | `kv-demo` | Standard | Secretos y configuración |
| Azure AI Foundry Hub | `aif-demo` | — | Hub para modelos |
| Azure OpenAI / AI Foundry Project | `aiproj-demo` | — | Deploy GPT-5.5 |
| Static Web App (o Vercel) | `swa-demo` | Free | Next.js frontend web |
| PowerSync Service | Cloud Free tier | — | Sincronización offline |
| Cloudflare DNS | Plan existente | — | Dominio `mep.ezekl.com` + `api.mep.ezekl.com` |

### Componentes del repositorio a crear

```
mep/
├── src/
│   ├── AulaIA.Api/              ← Backend .NET 10
│   └── aulaia-web/              ← Next.js
├── mobile/
│   └── aulaia-app/              ← React Native (Expo)
├── infra/
│   └── azure/                   ← Scripts Az CLI de aprovisionamiento
├── .github/
│   └── workflows/
│       ├── api-deploy.yml       ← CI/CD backend → App Service
│       └── web-deploy.yml       ← CI/CD Next.js → Static Web App
├── ia/                          ← (ya existe)
└── README.md
```

### Tareas Fase 0

| # | Tarea | Estado |
|---|-------|--------|
| F0-01 | Storage Account `stdemomep` + 5 contenedores | ✅ |
| F0-02 | Key Vault `kv-demomep` | ✅ |
| F0-03 | PostgreSQL 16 en VM `demo-itqs` + DB `aulaia` | ✅ |
| F0-04 | App Service Plan `asp-demomep` + App Service `app-demo-api` | ✅ |
| F0-05 | Managed Identity asignada (roles KV/Storage pendientes — Fase 3) | ⚠️ |
| F0-06 | AI Foundry GPT-5.5 configurado | ✅ |
| F0-07 | Static Web App — cancelado (ADR-007: SPA en App Service único) | ❌ |
| F0-08 | PowerSync Cloud conectado a PostgreSQL | ✅ |
| F0-09 | Solución .NET 10 `AulaIA.Api` con Feature Folders | ✅ |
| F0-10 | Proyecto Next.js `aulaia-web` (output: export, 0 vulnerabilidades) | ✅ |
| F0-11 | Proyecto Expo `aulaia-app` (SDK 55, TypeScript) | ✅ |
| F0-12 | GitHub Actions CI/CD — workflow unificado `deploy.yml` | ✅ |
| F0-13 | Migración EF Core `InitialCreate` + seed data (20 instituciones, 2 usuarios) | ✅ |
| F0-14 | Auth0: tenant `aulaia-mep`, API, apps web/móvil, roles, Action claim | ✅ |
| F0-15 | DNS Cloudflare: `mep.ezekl.com` + `api.mep.ezekl.com` → `app-demo-api` | ✅ |

---

## Fase 1 — Core: Grupos, Estudiantes y Asistencia QR 🔄 En progreso

**Objetivo:** MVP mínimo viable para que Adriana pueda usar la app en el aula desde el primer día. El módulo estrella (QR) y el núcleo del sistema funcionan completamente.

**Criterio de éxito:** Adriana toma lista con QR en sus clases del Colegio de Aserrí y el historial de asistencia queda guardado, disponible offline.

### Componentes

| Componente | Estado |
|-----------|--------|
| **Backend** | |
| Auth0 JWT middleware + roles (docente/director/admin) | ✅ |
| Módulo Grupos: CRUD + endpoints | ✅ |
| Módulo Estudiantes: CRUD + QrCode único por alumno | ✅ |
| Generación de código QR (UUID) por estudiante | ✅ |
| Módulo Asistencia: registro QR + manual + estados | ✅ |
| Endpoint `/api/powersync/token` | ✅ |
| **Base de datos** | |
| Migración EF Core: Institution, User, Group, Student, AttendanceRecord | ✅ |
| Migración `AddStudentQrCode` — columna qr_code + índice único | ✅ |
| **App Móvil (React Native / Expo)** | |
| Autenticación con Auth0 (PKCE) | ✅ |
| Pantalla: lista de grupos del docente | ✅ |
| Pantalla: lista de estudiantes por grupo | ✅ |
| Modo tomar lista con QR (Expo Camera) | ✅ |
| Modo tomar lista manual | ✅ |
| Offline con PowerSync (SQLite local) | ⏳ |
| **App Web (Next.js)** | |
| Autenticación con Auth0 | ⏳ |
| Vista de grupos y estudiantes | ⏳ |
| Descarga / impresión de QRs (PDF con toda la sección) | ⏳ |
| Vista historial de asistencia por sección | ⏳ |

---

## Fase 2 — Planeamiento Didáctico con IA + Calendario ⏳ Pendiente

**Objetivo:** El módulo diferenciador #1 según retroalimentación de Adriana (2026-05-05). El docente genera un planeamiento completo, alineado al programa oficial del MEP y adaptado a su calendario institucional, en minutos.

**Criterio de éxito:** Adriana genera el planeamiento de Artes Plásticas 7° para un trimestre completo — con actividades clase por clase, tareas y ejemplos listos para aplicar — en menos de 3 minutos, con terminología exacta del MEP y sin ediciones manuales necesarias.

### Componentes

| Componente | Estado |
|-----------|--------|
| **Calendario escolar** | |
| Calendario MEP base cargado en el sistema (200 días, 3 trimestres) | ⏳ |
| CRUD de eventos no lectivos: feriados, exámenes, consejo de profesores, FEA, semana del deporte, congresos | ⏳ |
| Agregar actos cívicos e institucionales al calendario | ⏳ |
| Cálculo automático de lecciones disponibles por período | ⏳ |
| Reorganización automática del planeamiento al modificar el calendario | ⏳ |
| **Base de conocimiento MEP** | |
| Estructurar programa de Artes Plásticas 7°, 8°, 9° en JSON | ⏳ |
| Cargar base de conocimiento en el sistema | ⏳ |
| **Backend** | |
| Módulo Planeamiento: LessonPlan CRUD | ⏳ |
| Servicio de generación con Azure AI Foundry (GPT-5.5) | ⏳ |
| Generación de actividades clase por clase + tareas + ejemplos listos para aplicar | ⏳ |
| Soporte de plantilla MEP por defecto + plantilla institucional (upload DOCX/PDF) | ⏳ |
| Sistema de caché con OutputCache (evitar llamadas duplicadas al LLM) | ⏳ |
| Rate limiting en endpoint de generación (5 req/min) | ⏳ |
| Generación de PDF y subida a Azure Blob Storage | ⏳ |
| **App Web** | |
| Formulario de parámetros del planeamiento | ⏳ |
| Vista de calendario escolar personalizable | ⏳ |
| Vista de planeamiento generado con editor básico | ⏳ |
| Descarga en PDF y DOCX | ⏳ |
| Historial de planeamientos generados | ⏳ |
| **App Móvil** | |
| Vista de planeamientos guardados (offline) | ⏳ |
| Consulta de planeamiento por clase del día | ⏳ |

---

## Fase 3 — Notas, Promedios y Reportes Básicos ⏳ Pendiente

**Objetivo:** El docente puede registrar toda la evaluación del período y generar el acta de notas para el MEP. Eliminar el doble trabajo con Excel.

**Criterio de éxito:** Adriana cierra el I Trimestre usando solo la app para notas, genera el acta y la exporta al SEA sin necesidad de Excel.

### Componentes

| Componente | Estado |
|-----------|--------|
| **Backend** | |
| Módulo Notas: EvaluationActivity + Grade CRUD | ⏳ |
| Cálculo de promedios con ponderación configurable | ⏳ |
| Alertas de estudiantes en riesgo (< nota mínima) | ⏳ |
| Generación de reportes: acta de notas PDF + XLSX | ⏳ |
| Exportación SEA (archivo en formato MEP) | ⏳ |
| **App Móvil** | |
| Pantalla: actividades de evaluación por grupo | ⏳ |
| Pantalla: libro de notas (tabla editable) | ⏳ |
| Alertas visuales de estudiantes en riesgo | ⏳ |
| Offline: notas y actividades sincronizadas con PowerSync | ⏳ |
| **App Web** | |
| Libro de notas completo (tipo spreadsheet) | ⏳ |
| Configuración de ponderación por sección | ⏳ |
| Generación y descarga de reportes (PDF/XLSX) | ⏳ |

---

## Fase 4 — Adecuaciones Curriculares e Informes Completos ⏳ Pendiente

**Objetivo:** Completar el sistema con el módulo de atención a la diversidad y todos los informes institucionales que el docente debe entregar.

**Criterio de éxito:** El sistema genera el informe de adecuaciones listo para el expediente del CAE sin trabajo manual adicional del docente.

### Componentes

| Componente | Estado |
|-----------|--------|
| **Backend** | |
| Módulo Adecuaciones: Accommodation CRUD | ⏳ |
| Generación de propuesta pedagógica con IA (AS y ANS) | ⏳ |
| Integración de adecuaciones en planeamiento generado | ⏳ |
| Informe de adecuaciones PDF para expediente CAE | ⏳ |
| Reporte de asistencia por período (PDF/XLSX) | ⏳ |
| Informe docente para dirección | ⏳ |
| **App Web** | |
| Perfil completo del estudiante (historial + adecuación) | ⏳ |
| Formulario de adecuación curricular | ⏳ |
| Vista de propuesta pedagógica generada | ⏳ |
| Dashboard del docente: resumen del período | ⏳ |
| **App Móvil** | |
| Perfil del estudiante con indicador de adecuación activa | ⏳ |
| Notificaciones push: alertas de rendimiento | ⏳ |

---

## Fase 5 — Escala: Container Apps + Nuevas Materias ⏳ Pendiente

**Objetivo:** Migrar la infraestructura a Container Apps, agregar nuevas materias al programa MEP y abrir la plataforma a instituciones completas.

**Disparador:** Cuando se supere la capacidad del App Service B1/S1 o se necesite escalar módulos de forma independiente.

### Componentes

| Componente | Estado |
|-----------|--------|
| **Infraestructura** | |
| Contenedorizar `app-demo-api` (Dockerfile) | ⏳ |
| Crear Azure Container Registry (ACR) `acr-demo` | ⏳ |
| Crear Container Apps Environment `cae-demo` | ⏳ |
| Migrar App Service → Container App | ⏳ |
| Separar servicio de IA en Container App independiente (escala propia) | ⏳ |
| **Producto** | |
| Agregar programas: Artes Musicales, Educación para el Hogar | ⏳ |
| Panel de director: vista institucional de todos los docentes | ⏳ |
| Plan institucional: gestión de múltiples docentes | ⏳ |
| Apple Sign-In (obligatorio para App Store) | ⏳ |
| **Negocio** | |
| Onboarding de primer colegio completo (plan institucional) | ⏳ |

---

## Resumen de fases

| Fase | Nombre | Entregable clave | Estado |
|------|--------|-----------------|--------|
| 0 | Infraestructura Azure + Setup | Todos los recursos Azure `demo` creados; repos estructurados | ✅ |
| 1 | Core: Grupos + Asistencia QR | Adriana toma lista con QR en el aula | 🔄 |
| 2 | Planeamiento con IA + Calendario | Planeamiento MEP completo en minutos, reorganizable por calendario | ⏳ |
| 3 | Notas y Reportes básicos | Adriana cierra trimestre sin Excel; exporta al SEA | ⏳ |
| 4 | Adecuaciones e Informes | Informes CAE generados automáticamente | ⏳ |
| 5 | Escala + Nuevas materias | Container Apps + plan institucional | ⏳ |
