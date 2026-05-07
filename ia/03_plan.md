# 03 — Plan de Desarrollo

> **Última actualización:** 2026-05-07 (rev 4)
> **Estado general:** 🔄 Fase 6 en progreso — contenedorización completada, ACR + Container Apps pendientes

---

## Visión general

El proyecto se construye en 5 fases progresivas. Las primeras dos fases producen un MVP funcional que Adriana Guido puede usar en el Colegio de Aserrí desde el primer trimestre. Las fases 3 y 4 consolidan los módulos diferenciadores. La Fase 5 migra la infraestructura a Container Apps para soportar escala.

---

## Fase 0 — Infraestructura Azure y Setup del Proyecto ✅ Completada (2026-05-05)

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

## Fase 1 — Core: Grupos, Estudiantes y Asistencia QR ✅ Completada (2026-05-05)

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
| Offline con PowerSync (SQLite local) | ✅ |
| **App Web (Next.js)** | |
| Autenticación con Auth0 | ✅ |
| Vista de grupos y estudiantes | ✅ |
| Descarga / impresión de QRs (PDF con toda la sección) | ✅ |
| Vista historial de asistencia por sección | ✅ |

---

## Fase 2 — Planeamiento Didáctico con IA + Calendario ✅ Completada (2026-05-07)

**Objetivo:** El módulo diferenciador #1 según retroalimentación de Adriana (2026-05-05). El docente genera un planeamiento completo, alineado al programa oficial del MEP y adaptado a su calendario institucional, en minutos.

**Criterio de éxito:** Adriana genera el planeamiento de Artes Plásticas 7° para un trimestre completo — con actividades clase por clase, tareas y ejemplos listos para aplicar — en menos de 3 minutos, con terminología exacta del MEP y sin ediciones manuales necesarias.

### Componentes

| Componente | Estado |
|-----------|--------|
| **Calendario escolar** | |
| Calendario MEP base cargado en el sistema (200 días, 3 trimestres) | ✅ |
| CRUD de eventos no lectivos: feriados, exámenes, consejo de profesores, FEA, semana del deporte, congresos | ✅ |
| Agregar actos cívicos e institucionales al calendario | ✅ |
| Cálculo automático de lecciones disponibles por período | ✅ |
| Reorganización automática del planeamiento al modificar el calendario | ✅ |
| **Base de conocimiento MEP** | |
| PDF programa Artes Plásticas subido → job extracción con GPT-5.5 | ✅ |
| Validación de unidades extraídas (admin) | ✅ |
| **Backend** ✅ | |
| Entidades `CurriculumUnit` + `LessonPlan` + migración EF Core | ✅ |
| Hangfire (queues: default, curriculum, planeamiento) + dashboard `/hangfire` | ✅ |
| `CurriculumModule` — upload PDF → Blob → job extracción | ✅ |
| `ExtractCurriculumJob` — PdfPig + GPT-5.5 → `curriculum_extractions` (encabezado) + `curriculum_units` (detalle, FK) | ✅ |
| Schema curricular normalizado: encabezado `curriculum_extractions` con `ModelUsed`, `TotalTokensUsed`, `UnidadCount`, `PdfSourceUrl` | ✅ |
| PDFs MEP cargados: III Ciclo (7 unidades, 82.678 tokens) + I y II Ciclo (14 unidades, 85.618 tokens) | ✅ |
| `PlaneamientoAiService` — GPT-5.5 anclado al currículo validado | ✅ |
| `PlaneamientoModule` — POST crear, GET estado, GET lista | ✅ |
| `GenerarPlaneamientoJob` — Pending → Generating → Ready/Failed | ✅ |
| App Service configurado (AI + Storage + todas las vars) | ✅ |
| Migración aplicada a BD producción | ✅ |
| **App Web** | |
| Formulario de parámetros del planeamiento | ✅ |
| Vista planeamiento generado (render Markdown + polling) | ✅ |
| Descarga .md + imprimir/PDF | ✅ |
| Historial de planeamientos generados | ✅ |
| **App Móvil** | |
| Vista de planeamientos guardados (offline) | ✅ |
| Consulta de planeamiento por clase del día | ✅ |

---

## Fase 3 — Notas, Promedios y Reportes Básicos ✅ Completada (2026-05-07)

**Objetivo:** El docente puede registrar toda la evaluación del período y generar el acta de notas para el MEP. Eliminar el doble trabajo con Excel.

**Criterio de éxito:** Adriana cierra el I Trimestre usando solo la app para notas, genera el acta y la exporta al SEA sin necesidad de Excel.

### Componentes

| Componente | Estado |
|-----------|--------|
| **Backend** | |
| Módulo Notas: EvaluationActivity + Grade CRUD | ✅ |
| Cálculo de promedios con ponderación configurable por sección | ✅ |
| Alertas de estudiantes en riesgo (< nota mínima) — indicador ⚠ en App Web | ✅ |
| Generación de reportes: acta de notas PDF landscape (QuestPDF) | ✅ |
| Exportación SEA — acta XLSX compatible SEA (ClosedXML) | ✅ |
| `Group.PctCotidiano/Pruebas/Extraclase/Otros` — pesos configurables, defaults MEP | ✅ |
| Endpoint `PUT /api/grupos/{id}/ponderacion` — valida suma = 100 | ✅ |
| Migración `AddGroupWeighting` — 4 columnas `decimal(5,2)` con defaults MEP | ✅ |
| **App Móvil** | |
| Pantalla: actividades de evaluación por grupo | ✅ |
| Pantalla: libro de notas (tabla offline, badge promedio verde/rojo) | ✅ |
| Alertas visuales de estudiantes en riesgo (badge rojo bajo umbral) | ✅ |
| Offline: notas y actividades sincronizadas con PowerSync | ✅ |
| PowerSync Sync Rules: `evaluation_activities` + `grades` en bucket | ⚠️ Acción manual en dashboard |
| **App Web** | |
| Libro de notas completo (actividades × alumnos, edición inline) | ✅ |
| Botones descarga `↓ XLSX (SEA)` y `↓ PDF` con autenticación Bearer | ✅ |
| Panel colapsable ponderación: 4 inputs, suma en tiempo real, guardar | ✅ |
| Tests de integración: módulo Notas (5 tests) | ✅ |

---

## Fase 4 — Adecuaciones Curriculares e Informes Completos ✅ Completada (2026-05-07)

**Objetivo:** Completar el sistema con el módulo de atención a la diversidad y todos los informes institucionales que el docente debe entregar.

**Criterio de éxito:** El sistema genera el informe de adecuaciones listo para el expediente del CAE sin trabajo manual adicional del docente.

### Componentes

| Componente | Estado |
|-----------|--------|
| **Backend** | |
| Entidad `Accommodation` + EF config (`AccommodationConfiguration`) + índice único `(student_id, group_id)` | ✅ |
| Migración EF Core `AddAccommodations` | ✅ |
| `AdecuacionAiService` — genera propuesta pedagógica con GPT-5.5 (AS/ANS/AA según Ley 7600) | ✅ |
| `GenerarAdecuacionJob` — Hangfire job, transiciones Pending → Generating → Ready/Failed | ✅ |
| `InformeAdecuacionService` — PDF QuestPDF para expediente CAE (datos generales, estrategias, propuesta IA, firmas) | ✅ |
| `AdecuacionesModule` — 6 endpoints: list, get, upsert, delete, generar, informe PDF | ✅ |
| Registrado en `Program.cs` (`AddAdecuacionesModule` + `MapAdecuacionesEndpoints`) | ✅ |
| Integración de adecuaciones en planeamiento generado | ✅ |
| Reporte de asistencia por período (PDF/XLSX) | ✅ |
| Informe docente para dirección | ✅ |
| **App Web** | |
| Tipos + funciones en `api.ts` (`listAdecuaciones`, `upsertAdecuacion`, `generarPropuestaAdecuacion`, `getInformeAdecuacionUrl`, etc.) | ✅ |
| `/adecuaciones/[grupoId]/page.tsx` — tabla de alumnos + panel lateral + polling + descarga PDF | ✅ |
| A11y auditada y corregida (`role="dialog"`, `htmlFor`/`id`, `scope="col"`, `aria-live`, emojis ocultos) | ✅ |
| Botón `♿ Adecuaciones` en tarjetas de grupos (`/grupos`) | ✅ |
| Dashboard del docente: resumen del período | ✅ |
| **App Móvil** | |
| Perfil del estudiante con indicador de adecuación activa | ✅ |
| Notificaciones push: alertas de rendimiento | ✅ |

---

## Fase 5 — Monetización: Suscripciones y Referidos ✅ Completada (2026-05-07)

**Objetivo:** Activar el modelo de negocio antes del lanzamiento público. Sin esta fase no hay ingresos ni tracking del acuerdo con Adriana.

**Criterio de éxito:** Un docente nuevo puede registrarse, usar el trial de 30 días, enviar un SINPE al número de AulaIA, presionar "Ya pagué" y el admin aprueba la suscripción en menos de 24 horas. Adriana ve en su panel la comisión generada por ese referido.

**Método de pago:** SINPE Móvil con verificación manual por admin. Sin pasarela de pago automática ni API bancaria (ADR-009).

### Componentes

| Componente | Estado |
|-----------|--------|
| **Backend — Suscripciones** | |
| Entidades `Subscription` + `PaymentRequest` + `ExchangeRate` + migración EF Core `AddMonetization` | ✅ |
| `SubscriptionsModule` — GET estado, POST trial, POST solicitar pago (genera `AUI-YYYYMMDD-XXXX`), POST upload comprobante (Blob `pagos`) | ✅ |
| `PaymentsModule` (admin) — GET pendientes, GET historial, POST aprobar (activa/renueva suscripción), POST rechazar con nota | ✅ |
| `SinpeOptions` — `PhoneNumber`, `AccountName`, precios USD por plan, `TrialDays` (configurables sin redeploy) | ✅ |
| Job diario `UpdateExchangeRateJob` (Hangfire, 12h UTC) — SOAP BCCR indicador 318 → tabla `exchange_rates` | ✅ |
| Job diario `CheckExpiredSubscriptionsJob` (Hangfire, 8h UTC) — expira suscripciones vencidas | ✅ |
| Endpoint público `GET /api/suscripcion/info` — planes, precios, TC actual, número SINPE | ✅ |
| **Backend — Referidos** | |
| Entidades `ReferralCode` + `Commission` + campo `referred_by_code` en `users` | ✅ |
| `ReferralsModule` — GET mi-codigo (auto-genera), GET panel referidos, GET comisiones (usuario) | ✅ |
| Admin endpoints: GET comisiones, POST marcar pagada, POST cierre-mensual (encola `CalculateCommissionsJob`) | ✅ |
| `CalculateCommissionsJob` — 20% sobre ingresos netos (bruto − infra Azure), idempotente por `(codigo, usuario, mes)` | ✅ |
| **App Web** | |
| `/precios` — 3 cards de planes (USD + CRC + TC BCCR), sección SINPE, CTA → `/suscripcion?plan=X` | ✅ |
| `/suscripcion` — estado actual, activar trial, generar instrucciones SINPE, upload comprobante | ✅ |
| `/admin` — 4 tabs: Pagos pendientes (aprobar/rechazar), Suscripciones, Cierre mensual, Comisiones | ✅ |
| `/perfil` — info Auth0, suscripción, código referido + enlace copiable, panel referidos, historial comisiones | ✅ |
| A11y: `role="tablist"`, `aria-selected="true"/"false"` (literales), `role="tabpanel"`, `aria-controls`, `type="button"`, emojis `aria-hidden`, spinners `role="status"` | ✅ |

---

## Fase 6 — Escala: Container Apps + Nuevas Materias 🔄 En progreso (2026-05-07)

**Objetivo:** Migrar la infraestructura a Container Apps, agregar nuevas materias al programa MEP y abrir la plataforma a instituciones completas.

**Disparador:** Cuando se supere la capacidad del App Service B1/S1 o se necesite escalar módulos de forma independiente.

### Componentes

| Componente | Estado |
|-----------|--------|
| **Contenedorización (repo)** | |
| `Dockerfile` multi-stage (Node.js 22 → .NET SDK 10 → .NET aspnet 10) | ✅ |
| `.dockerignore` | ✅ |
| `docker-compose.yml` para dev local | ✅ |
| `.env.docker.example` — plantilla de variables | ✅ |
| CI/CD: pasos Docker Build + Push a ACR (condicional) en `deploy.yml` | ✅ |
| **Infraestructura Azure** | |
| Script Az CLI: crear ACR `acrdemo` + habilitar admin | ⏳ |
| Script Az CLI: Container Apps Environment `cae-demo` + Container App `ca-aulaia-api` | ⏳ |
| Migrar deploy CI/CD: zip → imagen ACR → Container App | ⏳ |
| Managed Identity del Container App con roles ACR/KV/Storage | ⏳ |
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
| 1 | Core: Grupos + Asistencia QR | Adriana toma lista con QR en el aula | ✅ |
| 2 | Planeamiento con IA + Calendario | Planeamiento MEP completo en minutos, reorganizable por calendario | ✅ |
| 3 | Notas y Reportes básicos | Adriana cierra trimestre sin Excel; exporta al SEA | ✅ |
| 4 | Adecuaciones e Informes | Informes CAE generados automáticamente | ✅ |
| 5 | Monetización: Pagos + Referidos | Trial, SINPE, panel admin, panel comisiones Adriana | ✅ |
| 6 | Escala + Nuevas materias | Container Apps + plan institucional | 🔄 |
