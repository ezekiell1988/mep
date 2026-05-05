# 05 — Progreso del Proyecto

> **Última actualización:** 2026-05-05
> **Fase activa:** Fase 1 — Core: Grupos, Estudiantes y Asistencia QR

---

## ✅ Completado

**Fase 0 — Infraestructura Azure y setup del proyecto** (2026-05-05)

---

## 🔄 En progreso — Fase 1

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
| F1 · Auth0 JWT middleware en .NET (`AddAuthentication` + `AddAuthorization`) | ⏳ |
| F1 · Módulo Grupos: CRUD + endpoints | ⏳ |
| F1 · Módulo Estudiantes: CRUD + importación CSV | ⏳ |
| F1 · Generación QR UUID por estudiante | ⏳ |
| F1 · Módulo Asistencia: registro QR + manual | ⏳ |
| F1 · Endpoint `/api/powersync/token` | ⏳ |
| F1 · App móvil: auth Auth0 PKCE + pantallas grupos/estudiantes/lista | ⏳ |
| F1 · App web: auth Auth0 + vistas grupos + descarga QRs PDF | ⏳ |
