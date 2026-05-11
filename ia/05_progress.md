# 05 — Progreso del Proyecto

> **Última actualización:** 2026-05-11 (rev 25)
> **Fase activa:** Fase 6 — Escala: Container Apps + Nuevas Materias 🔄

---

## ✅ Completado

**Fase 0 — Infraestructura Azure y setup del proyecto** (2026-05-05)

**Fase 1 — Core: Grupos, Estudiantes y Asistencia QR** (2026-05-05)

**Fase 2 — Planeamiento Didáctico con IA + Calendario** (2026-05-07)

**Fase 3 — Notas, Promedios y Reportes Básicos** (2026-05-07)

**Fase 4 — Adecuaciones Curriculares e Informes Completos** (2026-05-07)

**Fase 5 — Monetización: Suscripciones, SINPE Móvil y Referidos** (2026-05-07)

---

## 🔄 En progreso — Fase 6

### PC-13: Fase 6 — Contenedorización (infraestructura)
| Tarea | Estado |
|-------|--------|
| F6 · `Dockerfile` multi-stage: Node.js 22 (web-build) → .NET SDK 10 (api-build) → .NET aspnet 10 (final) | ✅ |
| F6 · `.dockerignore` — excluye bin/, obj/, node_modules/, logs/, ia/, mobile/, AulaIA.Tests/ | ✅ |
| F6 · `docker-compose.yml` — dev local con PostgreSQL 16 + API contenedorizada | ✅ |
| F6 · `.env.docker.example` — plantilla de variables de entorno para el contenedor | ✅ |
| F6 · `.gitignore` — agrega `.env.docker` (secretos reales nunca al repo) | ✅ |
| F6 · Script Az CLI `infra/azure/06_acr_container_apps.sh` — ACR `acrdemoitqs` + Container Apps Environment `cae-demo-itqs` + Container App `ca-aulaia-api` | ✅ |
| F6 · Container App `ca-aulaia-api` corriendo — imagen `acrdemoitqs.azurecr.io/aulaia-api:ec90e46`, health `/health` HTTP 200 | ✅ |
| F6 · Fix `UseStaticFiles()` + `MapFallbackToFile("index.html")` — SPA sirve desde `wwwroot/` | ✅ |
| F6 · Fix `AddHttpClient()` en DI — corrige `IHttpClientFactory` para `UpdateExchangeRateJob` | ✅ |
| F6 · Fix `MigrateAsync()` al startup — migraciones EF Core aplicadas automáticamente en arranque | ✅ |
| F6 · Deploy manual documentado en skill `mep-deploy` — `docker buildx --platform linux/amd64` + `az containerapp update` | ✅ |
| F6 · Skills de agente creados: `mep-azure-infra`, `mep-deploy`, `mep-github` | ✅ |
| F6 · GitHub Actions eliminado — cuenta `ebaltodano@itqscr.com` tiene MFA corporativo, no hay Service Principal disponible. Deploy es manual. | ✅ |
| F6 · ADR-010 registrado: decisión de migración App Service → Container Apps | ✅ |
| F6 · Dominio `mep.ezekl.com` → Container App: CNAME actualizado, TXT `asuid.mep.ezekl.com` creado, cert gestionado `mc-cae-demo-itqs-mep-ezekl-com-6484` activo (SniEnabled). HTTPS 200 ✅ | ✅ |
| F6 · `api.mep.ezekl.com` eliminado — hostname removido del Container App, CNAME y TXT borrados de Cloudflare | ✅ |
| F6 · App móvil actualizada — `API_BASE` en `client.ts` y `PowerSyncContext.tsx` apunta a `https://mep.ezekl.com` | ✅ |
| F6 · Skill `mep-cloudflare` creado — documenta flarectl, registros DNS, flujo completo y lecciones aprendidas | ✅ |
| F6 · Dropdown asignaturas actualizado: `'Música'` → `'Artes Musicales'` + `'Educación para el Hogar'` añadido | ✅ |
| F6 · 13 PDFs del MEP descargados a `assets/` con curl + skill `mep-curriculum-pdfs` creado | ✅ |
| F6 · Entidad `CurriculumSource` + tabla `curriculum_sources` — registra URL MEP, ETag, Last-Modified, IsActive por asignatura/ciclo | ✅ |
| F6 · Migración EF Core `AddCurriculumSource` generada (índice único `asignatura+ciclo`) | ✅ |
| F6 · `SyncCurriculumJob` (Hangfire, manual — cron `0 0 30 2 *`) — orquesta HEAD→download→Blob→ExtractCurriculumJob por cada fuente activa; siembra catálogo inicial si tabla vacía | ✅ |
| F6 · `Microsoft.Extensions.Http.Resilience` v10.5.0 — HttpClient "mep" con `AddStandardResilienceHandler()` (TotalTimeout 600s, AttemptTimeout 60s, CircuitBreaker.SamplingDuration 300s, Retry 2) | ✅ |
| F6 · Fix `OptionsValidationException` al arrancar — `CircuitBreaker.SamplingDuration` debe ser `> 2 × AttemptTimeout`; valores corregidos y validados con `dotnet run` | ✅ |
| F6 · `ModulesExtensions.cs` — `AddAulaIAModules()`, `MapAulaIAEndpoints()`, `AddAulaIARecurringJobs()`, `RunMigrationsAsync()` en C# 14 extension blocks | ✅ |
| F6 · `Program.cs` refactorizado: 116 → 47 líneas; un único `using AulaIA.Api.Shared.Extensions` | ✅ |
| F6 · Hangfire dashboard accesible en dev sin JWT — `LocalRequestsOnlyAuthorizationFilter` en env Development; `HangfireAdminAuthFilter` en Production | ✅ |
| F6 · `Hangfire.Console` 1.4.2 integrado — `UseConsole()` en config; `SyncCurriculumJob` y `ExtractCurriculumJob` usan `PerformContext? ctx` + `ctx.WriteLine()` para logs en tiempo real en el dashboard | ✅ |
| F6 · Retries globales = 0 — `UseFilter(new AutomaticRetryAttribute { Attempts = 0 })` en `AddHangfire()`; jobs de curriculum con `[AutomaticRetry(Attempts = 1)]` propio | ✅ |
| F6 · Fix `AddAulaIARecurringJobs()` — migrado de `RecurringJob.AddOrUpdate` (estático, depende de `JobStorage.Current`) a `IRecurringJobManager` desde DI; los 3 jobs se registran correctamente en startup | ✅ |
| F6 · `wwwroot/` agregado a `.gitignore` (SPA compilada, no se versiona) | ✅ |
| F6 · Skill `hangfire-reset` creado — documenta causa raíz de `hangfire.hash`/`hangfire.set` desincronizados, fix y regla de truncate completo | ✅ |
| F6 · Fix BlobNotFound en `ExtractCurriculumJob` — `Uri.AbsolutePath` percent-encodea acentos; corregido con `Uri.UnescapeDataString` al construir el blob name (`ExtractCurriculumJob.cs` line ~128) | ✅ |
| F6 · Fix slug con acentos en `SyncCurriculumJob` — reemplazado `ToLowerInvariant().Replace(" ","-")` por `ToAsciiSlug()` (normalización NFD + strip NonSpacingMark); blobs futuros sin acentos en el nombre | ✅ |
| F6 · Skill `mep-blob-access` creado — documenta az CLI para listar, verificar, descargar y eliminar blobs; convención de slugs; diagnóstico BlobNotFound | ✅ |
| F6 · Fix NetworkTimeout en `ExtractCurriculumJob` (job 46 — Francés 5.4MB) — `AzureOpenAIClient` registrado como singleton en DI con `NetworkTimeout = TimeSpan.FromMinutes(10)`; inyectado por constructor en `ExtractCurriculumJob` | ✅ |
| F6 · Deploy revisión `ca-aulaia-api--0000003` — imagen con ambos fixes (BlobNotFound + NetworkTimeout); health `/health` 200, SPA 200 ✅ | ✅ |
| F6 · Fix `UpdateExchangeRateJob` (job 51, etapa 1) — SOAP body sin `<Token>`/`<CorreoElectronico>` causaba HTTP 500 del BCCR. Fix: `BccrOptions` (clase propia con todos los campos WSDL, `[Required]` + `ValidateOnStart`); `SinpeOptions` sin `BccrToken`; SOAP body completo con `Token`, `CorreoElectronico`, `Nombre`, `SubNiveles`. Env vars `Bccr__*` configuradas en Container App | ✅ |
| F6 · Fix `UpdateExchangeRateJob` (job 51, etapa 2) — BCCR devuelve HTTP 200 con texto no-XML (vacío, `"Nothing"`) en feriados/fines de semana; `XDocument.Parse()` lanzaba `XmlException`. Fix: catch de `XmlException`/`InvalidOperationException` → retorna `null` → job termina Succeeded con log ⚠️ | ✅ |
| F6 · `UpdateExchangeRateJob` con `PerformContext` — logs `[1/4]`–`[4/4]` visibles en dashboard Hangfire: HTTP status + ms de respuesta, XML de respuesta BCCR (hasta 500 chars), resultado del parse. `ModulesExtensions.cs` actualizado: `j.ExecuteAsync(null!, CancellationToken.None)` | ✅ |
| F6 · Deploy revisión `ca-aulaia-api--0000005` — imagen con `PerformContext` en `UpdateExchangeRateJob` + `BccrOptions` completo | ✅ |
| F6 · Fix `ParseTipoCambio` (job 51 etapa 3) — BCCR devuelve `NUM_VALOR` como nodo hijo directo del SOAP (no string XML embebido); eliminado segundo `XDocument.Parse`; ahora busca `NUM_VALOR` en `doc.Descendants()` directamente | ✅ |
| F6 · `UpdateExchangeRateJob`: si TC es null → lanza `InvalidOperationException` (job falla en rojo) en lugar de retornar silenciosamente con Succeeded | ✅ |
| F6 · Fix BD Hangfire: job 51 tenía `invocationdata` con firma antigua `ExecuteAsync(CancellationToken)` — eliminado de `hangfire.job/state/jobparameter`; `LastJobId` limpiado en `hangfire.hash` | ✅ |
| F6 · Deploy revisión `ca-aulaia-api--0000005` — imagen `6294008` reemplazada con fix `ParseTipoCambio` + comportamiento de error correcto (mismo tag, nuevo contenido) | ✅ |
| F6 · Fix `AdecuacionAiService` + `PlaneamientoAiService` — `AzureOpenAIClient` instanciado por llamada (mismo patrón que ISSUE-002); corregido a singleton inyectado por DI en ambos servicios | ✅ |
| F6 · Hangfire browser login flow — `/hangfire` en prod exige cookie `hangfire_token` (JWT HttpOnly 8h); middleware `UseAulaIAHangfireCookieInjection` inyecta cookie como `Authorization: Bearer` **antes** de `UseAuthentication()` para que JwtBearer resuelva `ctx.User` en primera pasada | ✅ |
| F6 · `/hangfire-login` — HTML inline sin CDN externo; lee token de `localStorage` por prefijo `@@auth0spajs@@::<clientId>::` (escaneo de todas las claves); si no hay sesión → redirige a `/?hangfire_return=1` | ✅ |
| F6 · `/hangfire-session` (POST) — valida JWT con JwtBearer, verifica claim admin, guarda `hangfire_token` como cookie HttpOnly Secure SameSite=Strict 8h | ✅ |
| F6 · `page.tsx` (SPA raíz) — detecta `?hangfire_return=1`; si no autenticado → `loginWithRedirect` con `appState.returnTo='/?hangfire_return=1'`; si autenticado → `getAccessTokenSilently` → POST `/hangfire-session` → redirect a `/hangfire`. Evita el loop de encicle | ✅ |
| F6 · `callback/page.tsx` — usa `appState.returnTo` del resultado de `handleRedirectCallback()` en lugar de hardcodear `/` | ✅ |
| F6 · Skill `mep-deploy` actualizado — agrega paso 5 (limpieza imágenes locales Docker) y paso 6 (ACR conserva últimas 4); script completo integra ambas limpiezas post-deploy | ✅ |
| F6 · ISSUE-006 resuelto — `BlobSlugHelper.ToAsciiSlug()` en `Shared/Extensions/BlobSlugHelper.cs`; `SyncCurriculumJob` elimina `ToAsciiSlug()` privado; `CurriculumModule` corrige blob name en `POST /api/curriculum/upload` | ✅ |
| F6 · Tab "Curriculum PDF" en `/admin` — selector asignatura/ciclo + file input + `uploadCurriculumPdf()` en `api.ts`; encola `ExtractCurriculumJob` y muestra Job ID resultante | ✅ |
| F6 · `DirectorModule.cs` — C# 14 extension block; `GET /api/director/resumen` → `ResumenInstitucionalResponse` (institución, totales docentes/grupos/estudiantes, plan institucional, días restantes); `GET /api/director/docentes` → `IReadOnlyList<DocenteInstitucionalResponse>` (breakdown por docente con grupos, plan, conteos); policy `director` (roles director + admin) | ✅ |
| F6 · Plan Institucional propagación — `PaymentsModule.AprobarPagoAsync`: si `payment.Plan == Institutional`, upsert de suscripción `Institutional/Active` a todos los usuarios de la misma `InstitutionId` con el mismo período | ✅ |
| F6 · `ModulesExtensions.cs` — `.MapDirectorEndpoints()` registrado en cadena `MapAulaIAEndpoints()` | ✅ |
| F6 · `api.ts` — 3 tipos (`ResumenInstitucionalResponse`, `GrupoResumenDirector`, `DocenteInstitucionalResponse`) + 2 funciones (`getDirectorResumen`, `getDirectorDocentes`) | ✅ |
| F6 · `/director/page.tsx` — wrapper estático (`generateStaticParams` → `[]`, `<Suspense>` + `<DirectorClient />`) | ✅ |
| F6 · `/director/DirectorClient.tsx` — carga paralela resumen + docentes con `cancelled` flag; stat cards (docentes/grupos/estudiantes); acordeón por docente con tabla de grupos; `isDirector` guard; skill-compliant: ternary+null, sin falsy `&&` | ✅ |
| F6 · `/dashboard/DashboardClient.tsx` — botón "Panel Dirección" condicional a `isDirector` → `router.push('/director')`; falsy `&&` convertidos a ternary | ✅ |
| F6 · A11y `/director/DirectorClient.tsx` — botón acordeón: `aria-expanded` con literales `"true"`/`"false"` (dos `<button>` condicionales, no expresión); `aria-label` dinámico `"Ver/Ocultar grupos de {fullName}"`; `aria-controls` + `id` panel; SVG `aria-hidden="true"` | ✅ |
| F6 · .NET build — 0 errores, 0 advertencias (validado post-Director feature) | ✅ |
| F6 · Google Sign-In — proyecto `aulaia-mep` en Google Cloud; OAuth2 Client ID web configurado; conexión `google-oauth2` en Auth0 con Client ID/Secret propios; habilitada en `AulaIA Mobile` + `AulaIA Web`; test "Successful transaction" con claim `teacher` ✅ | ✅ |
| F6 · Google Cloud en modo Testing — solo cuentas de prueba autorizadas hasta publicar la app (cuando haya usuarios reales) | ⚠️ |
| F6 · `ExtractCurriculumJob` chunking — PDFs grandes (>300k chars) divididos en bloques de 300k con overlap de 30k; deduplicación interna por (Nivel, Trimestre, UnidadNumero); JSON inválido por chunk → continue | ✅ |
| F6 · `ExtractCurriculumJob` hint por asignatura — `GetSubjectHint()` inyecta mapeo área→trimestre para Matemáticas III Ciclo (Números→T1, Geometría→T2, Relaciones y Álgebra→T3, Estadística y Probabilidad→T3); el LLM no inventa trimestres | ✅ |
| F6 · Fix Docker Next.js 16 — `import { Geist } from "next/font/google"` descargaba fonts en build time sin internet; reemplazado por `geist` npm package local (`import { GeistSans } from "geist/font/sans"`) | ✅ |
| F6 · Fix JSX `DashboardClient.tsx` — comentario JSX mal cerrado `{/* ... */` en línea 107; corregido a `{/* ... */}` | ✅ |
| F6 · Curriculum Matemáticas III Ciclo — 12 unidades extraídas y validadas en BD: 4 áreas × 3 niveles (7°/8°/9°) con habilidades reales del PDF MEP | ✅ |
| F6 · Curriculum completo — 130+ unidades validadas para todas las asignaturas; Matemáticas es la última en completarse | ✅ |
| F6 · Deploy revisión `ca-aulaia-api--0000012` — imagen con chunking + hint asignatura + fix Docker fonts + fix JSX | ✅ |
| F6 · Conectar `CurriculumUnit` validadas al `PlaneamientoAiService` — planeamiento anclado al programa oficial MEP | ✅ |
| F6 · `GET /api/planeamiento/curriculum-check?asignatura&nivel&trimestre` — devuelve `{ disponible, unidades }` (autenticado, no admin); usado por el formulario de nuevo planeamiento | ✅ |
| F6 · Badge disponibilidad en `/planeamiento/nuevo` — badge reactivo (debounce 300ms): verde "Programa MEP validado — N unidades" / naranja "Sin programa validado — la IA usará conocimiento general" al cambiar asignatura/nivel/trimestre | ✅ |
| F6 · Separar servicio de IA en Container App independiente — diferido hasta carga real | ⏳ |
| F6 · Apple Sign-In — diferido hasta tener cuenta Apple Developer propia | ⏳ |
| F6 · Cuenta Google Play Developer cerrada — resuelto con ADR-011: APK directo durante piloto, cuenta nueva antes del lanzamiento público | ✅ |

---

## 🔄 En progreso — Fase 7 (Lanzamiento Piloto)

> **Pivote 2026-05-11:** Adriana confirmó por WhatsApp que quiere usar la **web primero** para planeamiento desde su computadora. La app móvil (asistencia en el aula) la ve como un proyecto separado para más adelante. Ver ADR-012.

### PC-14: Fase 7 — Onboarding Adriana en la web

#### Prioridad inmediata — Web app lista para Adriana
| Tarea | Estado |
|-------|--------|
| F7 · Landing page pública en `mep.ezekl.com` — descripción del producto antes del login | ⏳ |
| F7 · Onboarding Adriana en web: crear institución, grupos, primeros estudiantes | ⏳ |
| F7 · Primera prueba de planeamiento con IA en clase real (Artes Plásticas, Colegio Aserrí) | ⏳ |
| F7 · Revisar UX del formulario de planeamiento en desktop — mejorar según feedback Adriana | ⏳ |
| F7 · Revisar UX de notas y reportes en desktop | ⏳ |

#### App móvil — diferida (no es prioridad para Adriana en este momento)
| Tarea | Estado |
|-------|--------|
| F7 · Configurar `package` android + `bundleIdentifier` iOS en `app.json` | ✅ (`cr.ezekl.aulaia`) |
| F7 · Instalar EAS CLI + crear cuenta expo.dev (`ezekiell1988`) | ✅ |
| F7 · Vincular proyecto a expo.dev con `eas init` (projectId: `58d237e5-c82d-4ccf-9b05-27b66fcb6246`) | ✅ |
| F7 · Eliminar `expo-barcode-scanner` deprecado (incompatible con RN 0.83 / Gradle 10) | ✅ |
| F7 · Eliminar `newArchEnabled` y `edgeToEdgeEnabled` de `app.json` (campos inválidos SDK 55) | ✅ |
| F7 · Alinear versiones: `react@19.2.0`, `react-native-safe-area-context@5.6.2`, `react-native-screens@4.23.0` | ✅ |
| F7 · Skill `eas-cli` creado — login, init, build, errores frecuentes | ✅ |
| F7 · Build APK Android exitoso — `eas build --platform android --profile preview` | ✅ (build `02bfafc1`) |
| F7 · APK disponible: `https://expo.dev/artifacts/eas/kFQYasZPhh9dNq2cvtEdP.apk` (~30 días) | ✅ |
| F7 · `@journeyapps/react-native-quick-sqlite` instalado + `ios/` regenerado con prebuild | ✅ |
| F7 · Subir APK a Google Drive + mensaje WhatsApp instrucciones — diferido (Adriana prefiere web primero) | ⏳ |
| F7 · Build iOS ad-hoc Xcode — diferido (Adriana no necesita app móvil ahora) | ⏳ |
| F7 · Crear cuenta Google Play Developer nueva ($25) — diferido hasta validar con Adriana | ⏳ |
| F7 · Crear cuenta Apple Developer ($99/año) — diferido hasta validar con Adriana | ⏳ |

---

## ✅ Completado — Fase 5 (detalle)

### PC-12: Fase 5 — Monetización
| Tarea | Estado |
|-------|--------|
| F5 · Entidades `Subscription`, `PaymentRequest`, `ReferralCode`, `Commission`, `ExchangeRate` (EF Core, snake_case, enums como string) | ✅ |
| F5 · Campo `referred_by_code` en entidad `User` + `UserConfiguration` actualizada | ✅ |
| F5 · `DbSet<>` para las 5 nuevas entidades en `AulaIADbContext` | ✅ |
| F5 · Configuraciones EF: `SubscriptionConfiguration`, `PaymentRequestConfiguration`, `ReferralCodeConfiguration`, `CommissionConfiguration`, `ExchangeRateConfiguration` | ✅ |
| F5 · Migración EF Core `AddMonetization` (generada, 5 tablas, índices únicos) | ✅ |
| F5 · `SinpeOptions` — `PhoneNumber`, `AccountName`, precios USD por plan, `TrialDays` (sección `Sinpe` en appsettings) | ✅ |
| F5 · `UpdateExchangeRateJob` (Hangfire, 12h UTC) — SOAP BCCR indicador 318, skips si ya existe el día | ✅ |
| F5 · `CheckExpiredSubscriptionsJob` (Hangfire, 8h UTC) — expira suscripciones vencidas | ✅ |
| F5 · `CalculateCommissionsJob` — 20% neto (bruto − infra Azure prorateada), idempotente por `(referral_code_id, referred_user_id, month)` | ✅ |
| F5 · `SuscripcionesModule` — GET estado, POST trial (Conflict si existe), POST solicitar pago (`AUI-YYYYMMDD-XXXX`), POST upload comprobante (Blob `pagos`, max 10 MB, jpg/png/pdf/webp), GET info pública | ✅ |
| F5 · `PaymentsModule` (admin) — GET pendientes, GET historial (últimos 200), POST aprobar (extiende período), POST rechazar con nota, GET suscripciones | ✅ |
| F5 · `ReferralsModule` — GET mi-codigo (auto-genera de nombre + año), GET panel, GET comisiones; admin: POST cierre-mensual (encola job), GET comisiones, POST marcar pagada | ✅ |
| F5 · Registrado en `Program.cs`: `AddSuscripcionesModule()`, `MapSuscripcionesEndpoints()`, `MapPaymentsEndpoints()`, `MapReferralsEndpoints()`, 2 `RecurringJob.AddOrUpdate` | ✅ |
| F5 · Backend build: 0 errores, 0 advertencias | ✅ |
| F5 · Tipos y funciones en `src/lib/api.ts` — 15 tipos + 16 funciones (estado, trial, pago, comprobante, admin pagos, admin suscripciones, referidos, comisiones, cierre mensual) | ✅ |
| F5 · App Web: `/precios` — 3 cards (Basic/Professional/Institutional), precios USD+CRC, TC BCCR, sección SINPE, CTA login | ✅ |
| F5 · App Web: `/suscripcion` — estado actual, activar trial gratuito, generar instrucciones SINPE (código + monto CRC + número), upload comprobante | ✅ |
| F5 · App Web: `/admin` — 4 tabs (Pagos pendientes, Suscripciones, Cierre mensual, Comisiones); A11y: `role="tablist"`, `aria-selected` literal, `role="tabpanel"`, `type="button"`, emojis `aria-hidden` | ✅ |
| F5 · App Web: `/perfil` — info usuario Auth0, suscripción actual, código referido copiable, panel referidos, historial comisiones | ✅ |
| F5 · Frontend build: 0 errores (`npm run build` → `out/`, 12 rutas estáticas) | ✅ |

---

## ✅ Completado — Fase 4 (detalle)

### PC-11: Fase 4 — Adecuaciones Curriculares
| Tarea | Estado |
|-------|--------|
| F4 · Entidad `Accommodation` + `AccommodationConfiguration` (EF, snake_case, enums como string) | ✅ |
| F4 · Índice único `(student_id, group_id)` — una adecuación por alumno por grupo | ✅ |
| F4 · `DbSet<Accommodation>` en `AulaIADbContext` | ✅ |
| F4 · Migración EF Core `AddAccommodations` (generada y verificada) | ✅ |
| F4 · `AdecuacionAiService` — prompt GPT-5.5 con contexto alumno + grupo + hasta 10 `CurriculumUnit` validadas | ✅ |
| F4 · `GenerarAdecuacionJob` (Hangfire `default`, retry 1) — ciclo Draft → Pending → Generating → Ready/Failed | ✅ |
| F4 · `InformeAdecuacionService` — PDF QuestPDF Community: datos generales, estrategias, propuesta IA, nota legal AS, 3 firmas | ✅ |
| F4 · `AdecuacionesModule` — `AddAdecuacionesModule()` + 6 endpoints REST | ✅ |
| F4 · Endpoints: `GET /list`, `GET /one`, `PUT /upsert`, `DELETE`, `POST /generar`, `GET /informe` | ✅ |
| F4 · Registrado en `Program.cs` | ✅ |
| F4 · Backend build: 0 errores, 0 advertencias | ✅ |
| F4 · Tipos y funciones en `src/lib/api.ts` (`AccommodationType`, `AccommodationStatus`, `AdecuacionResumen`, `AdecuacionResponse`, etc.) | ✅ |
| F4 · App Web: `/adecuaciones/[grupoId]/page.tsx` — tabla alumnos + polling 4s + panel lateral con formulario + propuesta IA + descarga PDF | ✅ |
| F4 · App Web: A11y auditada (`role="dialog"`, `aria-modal`, `aria-labelledby`, `htmlFor`/`id`, `scope="col"`, `aria-live="polite"`, emojis `aria-hidden`) | ✅ |
| F4 · App Web: patrón `reloadTrigger`/`reload` + `cancelled` flag (consistente con `calendario/[grupoId]`) | ✅ |
| F4 · App Web: botón `♿ Adecuaciones` en tarjetas de `/grupos` | ✅ |
| F4 · Integración de adecuaciones en planeamiento generado | ✅ |
| F4 · Reporte de asistencia por período (PDF/XLSX) | ✅ |
| F4 · `ReporteAsistenciaService` — PDF landscape (tabla coloreada P/A/T/J, % asistencia semáforo) + XLSX (zebra, freeze rows+cols, colores semáforo) | ✅ |
| F4 · Endpoints `GET /api/grupos/{id}/reportes/asistencia/pdf?from=&to=` y `.../xlsx?from=&to=` en `ReportesModule` | ✅ |
| F4 · `getReporteAsistenciaUrl()` en `api.ts` | ✅ |
| F4 · App Web: botones **📊 XLSX** y **📄 PDF** en `/asistencia/[grupoId]` (mismo rango from/to del filtro, descarga autenticada blob URL) | ✅ |
| F4 · `InformeDirectorService` — PDF QuestPDF Portrait Letter: cápsulas estadísticas, tabla por alumno (promedio color, semáforo asistencia, adecuación), sección adecuaciones activas, nota Ley 7600, 3 líneas de firma | ✅ |
| F4 · Endpoint `GET /api/grupos/{id}/reportes/informe-director?from=&to=` en `ReportesModule` | ✅ |
| F4 · `getInformeDirectorUrl()` en `api.ts` | ✅ |
| F4 · App Web: botón **↓ Informe Dirección** en `/notas/[grupoId]` (descarga autenticada, período feb-hoy) | ✅ |
| F4 · App Web: fix build `output: export` — server wrapper `page.tsx` con `generateStaticParams([{ grupoId: '_' }])` + `<Suspense>` para las 4 rutas `[grupoId]` | ✅ |
| F4 · Frontend build: 0 errores (`npm run build` → `out/`) | ✅ |
| F4 · Dashboard docente: resumen del período | ✅ |
| F4 · `DashboardModule` — `GET /api/docente/resumen` (totalGrupos, totalEstudiantes, estudiantesEnRiesgo, planeamientosPendientes, planeamientosListos, adecuacionesActivas, proximosEventos 14d) | ✅ |
| F4 · App Web: `/dashboard` — 6 stat cards + sección próximos eventos + header usuario + cerrar sesión | ✅ |
| F4 · App Web: `DashboardClient` — patrón IIFE+cancelled (consistente con calendario), `<button>` nativo en cards clickeables (axe fix) | ✅ |
| F4 · `getDocenteResumen()` + tipos `DocenteResumenResponse`/`ProximoEvento` en `api.ts` | ✅ |
| F4 · App móvil: indicador de adecuación activa en perfil del estudiante | ✅ |
| F4 · `EstudiantesScreen` — badge `♿ AS/ANS/AA` por alumno (query PowerSync `accommodations`) | ✅ |
| F4 · PowerSync schema: tabla `accommodations` + `AccommodationRow` type | ✅ |
| F4 · App móvil: notificaciones push alertas de rendimiento | ✅ |
| F4 · `useRendimientoNotifications` hook — promedio por alumno < 65 → notificación local push por grupo (deduplicado por día) | ✅ |
| F4 · `expo-notifications` instalado + plugin configurado en `app.json` | ✅ |
| F4 · `GruposScreen` — hook `useRendimientoNotifications()` activado | ✅ |
| F4 · Tests integración: `DashboardTests.cs` (3 tests) + `AdecuacionesTests.cs` (6 tests) | ✅ |
| F4 · Suite completa 32/32 tests ✅ | ✅ |

---

---

## ⏳ Pendiente — Fase 6 — Escala: Container Apps + Nuevas Materias

> Detalles de componentes en `03_plan.md` — Fase 6.

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
