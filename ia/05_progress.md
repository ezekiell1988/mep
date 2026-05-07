# 05 — Progreso del Proyecto

> **Última actualización:** 2026-05-07
> **Fase activa:** Fase 2 — Módulo Calendario (pendiente) + Fase 4 — Adecuaciones ⏳

---

## ✅ Completado

**Fase 0 — Infraestructura Azure y setup del proyecto** (2026-05-05)

**Fase 1 — Core: Grupos, Estudiantes y Asistencia QR** (2026-05-05)

**Fase 3 — Notas, Promedios y Reportes Básicos** (2026-05-07)

---

## 🔄 En progreso — Fase 2 (pendiente: Módulo Calendario)

> El resto de Fase 2 estaba marcado erróneamente como completado. Los ítems de backend/web/móvil del planeamiento están ✅. El sub-módulo Calendario está pendiente.

---

## ⏳ Pendiente — Fase 4

---

## ⏳ Pendiente — Fase 5 — Monetización: Suscripciones, SINPE Móvil y Referidos

> Detalles de componentes en `03_plan.md` — Fase 5.

---

## 🔄 Historial — Fase 2 (complemento 2026-05-07)
### PC-10: Pendientes Fase 1 y Fase 2 completados
| Tarea | Estado |
|-------|--------|
| F1 · App web: `GET /api/grupos/{grupoId}/asistencia/historial` — endpoint rango fechas | ✅ |
| F1 · App web: `/asistencia/[grupoId]` — tabla asistencia histórica (sticky col, badge P/A/T/J, conteos por fila, selector de rango) | ✅ |
| F1 · App web: botón `📅 Asistencia` en tarjetas de grupos | ✅ |
| F2 · App móvil: `PlaneamientoHoyScreen` — query PowerSync por rango de fecha, render Markdown, botón "Ver completo" | ✅ |
| F2 · App móvil: ruta `PlaneamientoHoy` en `RootStackParamList` + `AppNavigator` | ✅ |
| F2 · App móvil: botón `Hoy` en header de pantalla Planeamientos | ✅ |

---

## 🔄 Historial — Fase 3

> **Decisiones de diseño confirmadas (2026-05-04):**
> - Stack definido: .NET 10 + EF Core 10 + PostgreSQL + PowerSync + Auth0 + Azure AI Foundry GPT-5.5
> - Patrón backend: Feature Folders / Vertical Slices (sin Controllers)
> - Dominio: `mep.ezekl.com` (web) · `api.mep.ezekl.com` (API) — DNS en Cloudflare
> - Resource group: `rg-ezequiel` · todos los recursos con prefijo `demo`
> - Autenticación: Managed Identity para comunicación Azure-to-Azure (cero API keys en código)

### PC-01: Infraestructura Azure
| Tarea | Estado |
|-------|--------|
| F0-01 · Storage Account `stdemomep` + 5 contenedores | ✅ |
| F0-02 · Key Vault `kv-demomep` | ✅ |
| F0-03 · PostgreSQL 16 en VM `demo-itqs` (ver `credentials/db.txt`) + DB `aulaia` | ✅ |
| F0-04 · App Service Plan `asp-demomep` + App Service `app-demo-api` | ✅ |
| F0-05 · Managed Identity asignada (roles KV/Storage pendientes — Fase 3) | ⚠️ |
| F0-06 · AI Foundry configurado — credenciales en `credentials/ai.txt` | ✅ |
| F0-07 · Static Web App — **cancelado** (Next.js SPA servido desde App Service único) | ❌ |
| F0-08 · PowerSync Cloud conectado a PostgreSQL (`aulaia`) — publication `powersync` activa | ✅ |

### PC-02: DNS y Dominio
| Tarea | Estado |
|-------|--------|
| F0-15 · CNAME `mep.ezekl.com` → `app-demo-api` en Cloudflare (web + api unificados) | ✅ |
| F0-15 · CNAME `api.mep.ezekl.com` → `app-demo-api` en Cloudflare | ✅ |

### PC-03: Repositorio y CI/CD
| Tarea | Estado |
|-------|--------|
| F0-09 · Solución .NET 10 `AulaIA.Api` con Feature Folders | ✅ |
| F0-10 · Proyecto Next.js `aulaia-web` (output: export, SPA estático, 0 vulnerabilidades) | ✅ |
| F0-11 · Proyecto Expo `aulaia-app` (SDK 55, TypeScript, mobile/) | ✅ |
| F0-12 · GitHub Actions CI/CD — workflow unificado `deploy.yml` (Next.js export → wwwroot + .NET publish → App Service) | ✅ |

### PC-04: Base de datos y Auth
| Tarea | Estado |
|-------|--------|
| F0-13 · Migración EF Core `InitialCreate` + seed data (20 instituciones MEP, usuarios admin/docente) | ✅ |
| F0-14 · Auth0: tenant `aulaia-mep`, API, apps web/móvil, roles (admin/teacher/director), Action roles claim | ✅ |
| F0-15 · DNS Cloudflare: `mep.ezekl.com` + `api.mep.ezekl.com` → `app-demo-api` (proxy ✅) | ✅ |

### PC-05: Fase 1 — Core
| Tarea | Estado |
|-------|--------|
| F1 · Auth0 JWT middleware en .NET (`AddAuthentication` + `AddAuthorization`) | ✅ |
| F1 · `ICurrentUserService` — resuelve Auth0 sub → User de BD, scoped por request | ✅ |
| F1 · Módulo Grupos: CRUD completo + endpoints con ownership por docente | ✅ |
| F1 · Módulo Estudiantes: CRUD + ownership + QrCode único por alumno | ✅ |
| F1 · Migración EF Core `AddStudentQrCode` — columna `qr_code` + índice único | ✅ |
| F1 · Módulo Asistencia: GET por fecha (lista completa), POST upsert, POST scan QR | ✅ |
| F1 · Endpoint `/api/powersync/token` — JWT HS256 firmado con sub del usuario | ✅ |
| F1 · App móvil: dependencias (react-native-auth0, @react-navigation, expo-camera, expo-secure-store, @powersync/react-native) | ✅ |
| F1 · App móvil: `AuthContext.tsx` — Auth0 PKCE login/logout, token en SecureStore | ✅ |
| F1 · App móvil: `api/client.ts` + `api/endpoints.ts` — wrapper tipado con Bearer token | ✅ |
| F1 · App móvil: `LoginScreen` — botón Auth0, spinner | ✅ |
| F1 · App móvil: `GruposScreen` — FlatList grupos, pull-to-refresh, logout | ✅ |
| F1 · App móvil: `EstudiantesScreen` — lista alumnos, botón Tomar Lista Hoy | ✅ |
| F1 · App móvil: `TomarListaScreen` — modo manual (ciclar estado) + modo QR (CameraView, cooldown 2s) | ✅ |
| F1 · App móvil: `AppNavigator.tsx` + `App.tsx` — NavigationContainer + AuthProvider, TS 0 errores | ✅ |
| F1 · App móvil: fixes RN best practices (Pressable, SafeAreaView edges, falsy &&, contentInsetAdjustmentBehavior) | ✅ |
| F1 · Sync Rules YAML en PowerSync Cloud dashboard (tablas: groups, students, attendance_records filtradas por teacher_sub) | ✅ |
| F1 · Endpoint `/api/powersync/crud` — recibe mutations de asistencia del Connector | ✅ |
| F1 · App móvil: `powersync/schema.ts` — tablas SQLite locales | ✅ |
| F1 · App móvil: `powersync/PowerSyncContext.tsx` — DB + Connector + Provider | ✅ |
| F1 · App móvil: rewire GruposScreen + EstudiantesScreen → useQuery SQLite | ✅ |
| F1 · App móvil: TomarListaScreen escribe en SQLite local (auto-sync) | ✅ |
| F1 · App web: auth Auth0 + vistas grupos + descarga QRs PDF | ✅ |
| F1 · Assets de la app: icon, adaptive-icon, splash-icon, favicon (Expo) + web icon (Next.js) | ✅ |

### PC-06: Fase 2 — Planeamiento Didáctico con IA
| Tarea | Estado |
|-------|--------|
| F2 · NuGet: Hangfire.AspNetCore 1.8.20 + Hangfire.PostgreSql 1.20.10 + Azure.AI.OpenAI 2.1.0 + PdfPig 0.1.9 | ✅ |
| F2 · Entidades EF: `CurriculumUnit` (unidades del programa MEP, JSONB) + `LessonPlan` (planeamiento generado) | ✅ |
| F2 · `AiOptions.cs` — config Endpoint/DeploymentName/ApiKey con validación en start | ✅ |
| F2 · `StorageOptions` — container `curriculum` agregado | ✅ |
| F2 · `HangfireExtensions.cs` — Hangfire con PostgreSQL + dashboard `/hangfire` (solo admin) | ✅ |
| F2 · `HangfireAdminAuthFilter.cs` — dashboard protegido con rol admin Auth0 | ✅ |
| F2 · Migración EF Core `AddCurriculumAndPlanning` — tablas `curriculum_units` + `lesson_plans` | ✅ |
| F2 · `CurriculumModule.cs` — POST upload PDF → Blob → job, GET unidades validadas, POST validar (admin) | ✅ |
| F2 · `ExtractCurriculumJob.cs` — queue "curriculum": descarga PDF → PdfPig → GPT-5.5 → BD | ✅ |
| F2 · `PlaneamientoAiService.cs` — consulta curriculum validado → prompt GPT-5.5 → Markdown | ✅ |
| F2 · `PlaneamientoModule.cs` — POST crear (encola job), GET por id, GET lista por docente | ✅ |
| F2 · `GenerarPlaneamientoJob.cs` — queue "planeamiento": Pending→Generating→Ready/Failed | ✅ |
| F2 · App Service `app-demo-api` — 10 variables de entorno configuradas (DB, Auth, Storage, PowerSync, AI) | ✅ |
| F2 · `appsettings.Development.json` — credenciales AI Foundry + storage key completa | ✅ |
| F2 · Migración aplicada a BD de producción | ✅ |
| F2 · Subir PDFs MEP Artes Plásticas (III Ciclo: 7 unidades, I y II Ciclo: 14 unidades) — pipeline completo validado | ✅ |
| F2 · Refactor schema curricular: tabla `curriculum_extractions` (encabezado por PDF) + FK `curriculum_units.ExtractionId` | ✅ |
| F2 · `CurriculumExtraction` — entidad encabezado: `Asignatura`, `Ciclo`, `PdfSourceUrl`, `ModelUsed`, `TotalTokensUsed`, `UnidadCount`, `ExtractedAt` | ✅ |
| F2 · Migración `AddCurriculumExtractionHeader` — crea `curriculum_extractions`, agrega FK cascade, elimina columnas repetidas de `curriculum_units` | ✅ |
| F2 · Migración `AddModelUsedToExtraction` — columna `ModelUsed` varchar(100) en `curriculum_extractions` | ✅ |
| F2 · `ExtractCurriculumJob` — crea encabezado `CurriculumExtraction` antes de insertar units; `ModelUsed` tomado de `AiOptions.DeploymentChat` | ✅ |
| F2 · `ILlmAuditService` — `LogError` incluye cadena completa de `InnerException` | ✅ |
| F2 · Skill `mep-db-access` — documentación psql + queries para `curriculum_extractions` y `curriculum_units` | ✅ |
| F2 · App web: formulario parámetros del planeamiento | ✅ |
| F2 · App web: vista planeamiento generado (render Markdown + polling) | ✅ |
| F2 · App web: descarga .md + imprimir/PDF | ✅ |
| F2 · App móvil: vista planeamientos guardados (offline) | ✅ |
| F2 · `LessonPlanConfiguration` — `HasColumnName` explícito para todas las columnas (snake_case) | ✅ |
| F2 · Migración `RenameLessonPlanColumns` — renombra columnas PascalCase → snake_case en `lesson_plans` | ✅ |
| F2 · PowerSync Sync Rules — tabla `lesson_plans` agregada (`WHERE group_id = bucket.group_id`) | ✅ |

### PC-07: Tests de Integración — `AulaIA.Tests`
| Tarea | Estado |
|-------|--------|
| Tests · Proyecto `AulaIA.Tests` — xUnit + FluentAssertions + net10.0 | ✅ |
| Tests · `appsettings.test.template.json` — plantilla con placeholders (versionada) | ✅ |
| Tests · `appsettings.test.json` — valores reales (gitignoreado) | ✅ |
| Tests · `Infrastructure/TestConfig.cs` — lee config de archivo + env vars | ✅ |
| Tests · `Infrastructure/Auth0TokenHelper.cs` — obtiene token M2M de Auth0 (Client Credentials) | ✅ |
| Tests · `Infrastructure/ApiClient.cs` — `HttpClient` con Bearer token; `CreateAsync()` + `CreateAnonymous()` | ✅ |
| Tests · `Infrastructure/IntegrationTestBase.cs` — base con `IAsyncLifetime`, helpers `AssertStatusAsync` / `ReadJsonAsync` | ✅ |
| Tests · `SanityTests.cs` — GET /health → 200, GET /api/grupos sin token → 401, con token → 200 | ✅ |
| Tests · `CurriculumTests.cs` — upload PDF (202+jobId), upload no-PDF (400), poll extracción, validar unidad, listar curriculum | ✅ |
| Tests · `PlaneamientoTests.cs` — listar (200), crear sin body (400), id inexistente (404), flujo completo Pending→Ready, leer PDF | ✅ |

### PC-08: Fase 3 — Notas y Promedios
| Tarea | Estado |
|-------|--------|
| F3 · Backend: `GET /api/grupos/{id}/notas/resumen` — promedio ponderado MEP por alumno | ✅ |
| F3 · Backend: `DELETE /api/grupos/{id}/actividades/{actId}` — eliminar actividad + cascade | ✅ |
| F3 · App Web: `/notas/[grupoId]` — libro de notas con tabla editable (actividades × alumnos) | ✅ |
| F3 · App Web: modal "Nueva actividad" (nombre, tipo, maxScore, porcentaje, fecha) | ✅ |
| F3 · App Web: panel inline ingreso de notas por actividad + guardar | ✅ |
| F3 · App Web: badge promedio verde/rojo con umbral MEP (65/70 según nivel) | ✅ |
| F3 · App Web: botón "📊 Notas" en cada tarjeta de grupo | ✅ |
| F3 · PowerSync schema: tablas `evaluation_activities` + `grades` + tipos `EvaluationActivityRow` / `GradeRow` | ✅ |
| F3 · PowerSync Sync Rules: `evaluation_activities` y `grades` en bucket `teacher_data` | ✅ |
| F3 · App Móvil: `NotasScreen` — lista offline alumnos × notas, scroll horizontal, badge promedio verde/rojo | ✅ |
| F3 · App Móvil: botón "Notas" en header de `EstudiantesScreen` | ✅ |
| F3 · App Móvil: ruta `Notas` registrada en `AppNavigator` + `RootStackParamList` | ✅ |
| F3 · NuGet: `ClosedXML 0.104.2` + `QuestPDF 2025.4.0` (build 0 errores) | ✅ |
| F3 · `ActaNotasService` — genera XLSX (compatible SEA) y PDF landscape con ClosedXML + QuestPDF | ✅ |
| F3 · Endpoints `GET /api/grupos/{id}/reportes/notas/xlsx` y `.../notas/pdf` | ✅ |
| F3 · App Web: botones `↓ XLSX (SEA)` y `↓ PDF` en libro de notas (descarga directa) | ✅ |
| F3 · App Web: alerta de riesgo ⚠ en filas de estudiantes bajo el umbral MEP (65/70) | ✅ |
| F3 · `NotasTests.cs` — 5 tests: sin token 401, listar 200, sin body 400, resumen 200, flujo completo CRUD | ✅ |
| F3 · `ApiClient.DeleteAsync()` agregado a infraestructura de tests | ✅ |
| F3 · `Grade.GroupId` — columna `group_id` desnormalizada para PowerSync (patrón offline-first) | ✅ |
| F3 · Migración `AddGradeGroupId` — columna `group_id` + índice `ix_grades_group_id` en `grades` | ✅ |
| F3 · `Group.PctCotidiano/Pruebas/Extraclase/Otros` — 4 propiedades decimal con defaults MEP (20/45/20/15) | ✅ |
| F3 · `GroupConfiguration` — columnas `pct_cotidiano/pruebas/extraclase/otros` (`decimal(5,2)`, defaults EF) | ✅ |
| F3 · Migración `AddGroupWeighting` — aplicada a BD producción | ✅ |
| F3 · Endpoint `PUT /api/grupos/{id}/ponderacion` — valida suma=100, rechaza 400 si no cumple | ✅ |
| F3 · `GrupoResponse` ampliado — incluye los 4 pesos en GET /api/grupos y GET /api/grupos/{id} | ✅ |
| F3 · `UpdatePonderacionRequest` — record con `[Range(0,100)]` en los 4 campos | ✅ |
| F3 · App web `api.ts` — interfaz `Grupo` ampliada, `getGrupoById`, `actualizarPonderacion` | ✅ |
| F3 · App web `/notas/[grupoId]` — panel colapsable ponderación: 4 inputs, indicador suma tiempo real, guardar | ✅ |
| F3 · `GruposTests.cs` — 7 tests: 401/200/campos/flujo completo actualizar-persistir-restaurar + validación suma | ✅ |
| F3 · `ApiClient.PutAsJsonAsync()` agregado a infraestructura de tests | ✅ |
| F3 · `CurriculumTests.FlujoCompleto` — guard `PdfPath` null/inexistente (evita falso negativo sin PDF real) | ✅ |
| F3 · BD verificada: tabla `groups` con 4 columnas `pct_*` (`numeric`, defaults 20/45/20/15, NOT NULL) | ✅ |
| F3 · Suite completa 23/23 tests verdes (SanityTests + GruposTests + NotasTests + CurriculumTests + PlaneamientoTests) | ✅ |
| F3 · Migración `SeedDemoData` — 2 grupos (7°A Matemáticas, 8°B Español), 10 estudiantes, 6 actividades de evaluación | ✅ |
| F3 · BD: `users.auth0_sub` de `ezekiell1988@gmail.com` actualizado a `google-oauth2|113068059463803033614` (login Google real) | ✅ |

### PC-09: Mantenimiento de Dependencias (2026-05-06)
| Tarea | Estado |
|-------|--------|
| NuGet: `ClosedXML` → 0.105.0, `Hangfire.AspNetCore` → 1.8.23, `Hangfire.PostgreSql` → 1.21.1 | ✅ |
| NuGet: `Microsoft.AspNetCore.OpenApi` → 10.0.7, `PdfPig` → 0.1.14 (build 0 errores) | ✅ |
| Web: `next` + `eslint-config-next` → 16.2.5, `react` + `react-dom` → 19.2.6 (0 vulnerabilidades) | ✅ |
| Mobile: `expo` → 55.0.23, `expo-camera/status-bar/web-browser` → latest patch, `react` → 19.2.6 | ✅ |
| Mobile: `react-native-screens` → 4.24.0, `react-native-safe-area-context` → 5.7.0 (TS 0 errores) | ✅ |
| Saltados intencionalmente: `typescript 6.x`, `eslint 10.x`, `@types/node 25.x`, `react-native 0.85.x`, `QuestPDF 2026.x` | ⚠️ Revisar en próximo ciclo |
