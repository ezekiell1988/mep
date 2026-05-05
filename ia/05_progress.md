# 05 — Progreso del Proyecto

> **Última actualización:** 2026-05-04
> **Fase activa:** Fase 0 — Infraestructura Azure y setup del proyecto

---

## ✅ Completado

*(Nada completado aún — proyecto en fase de planificación)*

---

## 🔄 En progreso — Fase 0

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
| F0-15 · CNAME `mep.ezekl.com` → `app-demo-api` en Cloudflare (web + api unificados) | ⏳ |
| F0-15 · CNAME `api.mep.ezekl.com` → `app-demo-api` en Cloudflare | ⏳ |

### PC-03: Repositorio y CI/CD
| Tarea | Estado |
|-------|--------|
| F0-09 · Solución .NET 10 `AulaIA.Api` con Feature Folders | ✅ |
| F0-10 · Proyecto Next.js `aulaia-web` | ⏳ |
| F0-11 · Proyecto Expo `aulaia-app` | ⏳ |
| F0-12 · GitHub Actions CI/CD (backend + web) | ⏳ |

### PC-04: Base de datos y Auth
| Tarea | Estado |
|-------|--------|
| F0-13 · Migración EF Core `InitialCreate` + seed data (20 instituciones MEP, usuarios admin/docente) | ✅ |
| F0-14 · Auth0: tenant, apps, roles, claims, callbacks | ⏳ |
