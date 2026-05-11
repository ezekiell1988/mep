# 06 — Decisiones de Arquitectura (ADRs)

> Cada decisión sigue el formato: contexto → opciones evaluadas → decisión → consecuencias.

---

## ADR-012: Pivote web-first — Adriana usa la web, app móvil es fase futura

**Fecha:** 2026-05-11
**Estado:** ✅ Decidido
**Decidido por:** Ezequiel Baltodano (basado en feedback directo de Adriana Guido)

### Contexto
Al mostrarle a Adriana la primera versión, ella respondió: *"Y si es solo para computadora? Total los planes es imposible manejarlos en teléfono"*. Confirmó que quiere usar la web para planeamiento desde su computadora. Sobre la app móvil (asistencia offline en el aula) dijo: *"Sip ese si pero ese sería otro proyecto"* — lo ve útil pero no como su prioridad inmediata.

### Decisión
**Prioridad Fase 7: web app funcionando para Adriana** (`mep.ezekl.com`).

- Enfocar onboarding de Adriana completamente en la web: institución, grupos, primer planeamiento con IA.
- App móvil (asistencia QR offline) se mantiene construida pero se difiere como producto para una segunda fase de adopción — cuando Adriana ya use la web con fluidez.
- No invertir tiempo en distribuir APK ni en build iOS hasta que la web esté validada con Adriana.
- Las cuentas de tiendas (Google Play $25, Apple Developer $99) se deciden después de la validación web.

### Consecuencias
- ✅ Reduce fricción del piloto — Adriana empieza a usar el producto esta semana desde su computadora, sin instalar nada.
- ✅ El diferenciador principal (planeamiento con IA) es 100% funcional en web — es lo que Adriana quiere probar.
- ✅ Evita el costo de $99/año Apple Developer hasta tener validación real.
- ⚠️ La app móvil queda sin piloto real por ahora — el ciclo de feedback de asistencia QR se retrasa.
- ⚠️ ADR-011 sigue vigente como plan para cuando la app móvil sea la prioridad — se ejecuta en Fase 8.

---

## ADR-011: Estrategia de distribución inicial — APK directo + Xcode ad-hoc

**Fecha:** 2026-05-10
**Estado:** ✅ Decidido — ejecución diferida a Fase 8 (ver ADR-012)
**Decidido por:** Ezequiel Baltodano

### Contexto
La cuenta de Google Play Developer está cerrada permanentemente (ISSUE-007). La cuenta de Apple Developer no es propia ($99/año). Se necesita distribuir la app a Adriana y a docentes piloto del Colegio de Aserrí para validar el producto antes de invertir en cuentas de tiendas.

### Decisión

**Android:** Distribuir APK directamente vía enlace de Google Drive compartido por WhatsApp. Los docentes habilitan "Instalar apps de fuentes desconocidas" una sola vez.

**iOS:** Instalar vía Xcode desde Mac con cuenta de Apple ID gratuita. El perfil de desarrollo expira en 7 días y hay que reinstalar conectando el iPhone por USB. Solo viable para el iPhone de Adriana y el propio durante el piloto.

**Tiendas (diferido):** Abrir cuenta Google Play Developer nueva ($25 único) y cuenta Apple Developer ($99/año) una vez validado el producto con Adriana. La decisión de cuándo invertir se toma con ella tras las primeras semanas de uso real.

### Consecuencias
- ✅ Piloto arranca sin costo de cuentas de tiendas.
- ✅ Adriana puede validar el producto antes de comprometer inversión.
- ⚠️ iOS tiene que reinstalarse cada 7 días — solo viable a corto plazo para 1-2 dispositivos.
- ⚠️ Android requiere que el docente active fuentes desconocidas — hay que incluir instrucciones en el mensaje de WhatsApp.
- ⚠️ Sin actualizaciones automáticas — cada nueva versión hay que reenviar el APK.

---

## ADR-008: Modelo de compensación para Adriana Guido (socia comercial)

**Fecha:** 2026-05-07
**Estado:** ✅ Decidido
**Decidido por:** Ezequiel Baltodano (dev principal)

### Contexto
Adriana Guido es docente de Artes Plásticas con 24 años de experiencia y plaza en el Colegio de Aserrí. Su rol en el proyecto es comercial y pedagógico: ventas por su red de contactos docentes, validación pedagógica del producto y cara educativa del proyecto. Se necesita definir un modelo de compensación que reconozca su aporte sin comprometer la viabilidad financiera del proyecto, dado que el 100% del riesgo financiero (desarrollo + infraestructura Azure) lo asume Ezequiel.

### Opciones evaluadas

| Opción | Pros | Contras |
|--------|------|---------|
| **Equity (co-fundadora formal)** | Alineación a largo plazo | Difícil de revertir si deja de aportar; equity en etapa temprana sin producto validado es riesgoso; no hay mecanismo claro de vesting real en una relación informal |
| **Comisión por referidos** | Proporcional al aporte real; sin comprometer propiedad; escala solo si ella genera usuarios | No incentiva tareas no directamente ligadas a referidos (validación, retroalimentación) |
| **Salario fijo** | Claridad | El proyecto no genera ingresos todavía; es inviable en fase inicial |

### Decisión
**Comisión del 20% de la suscripción mensual de cada usuario que Adriana refiera directamente, durante 12 meses desde la suscripción del usuario.**

Reglas adicionales:
- Los costos de infraestructura Azure se descuentan de los ingresos brutos **antes** de calcular la base de comisión.
  - Ejemplo: $500 ingresos − $150 Azure = $350 base → comisión = $70.
- Un usuario "referido por Adriana" se define por un link de referido único o por declaración explícita del docente al registrarse.
- Después de 12 meses, el usuario pasa a ser cuenta sin comisión continua. Si el usuario sube de plan, el nuevo precio aplica durante los 12 meses.
- Adriana recibe cuenta Profesional gratuita de por vida.
- Este acuerdo debe formalizarse por escrito antes del primer cobro real a usuarios.
- Si en el futuro ella mantiene contribución activa sostenida (+12 meses, +100 referidos), se puede convertir parte de las comisiones acumuladas en equity minoritario como reconocimiento formal — requiere nuevo ADR.

### Consecuencias
- ✅ La compensación es proporcional al aporte real y verificable.
- ✅ Ezequiel conserva el 100% de la propiedad del producto y la infraestructura.
- ✅ No hay equity que recuperar si la relación no funciona en los primeros meses.
- ✅ El modelo escala: si Adriana refiere 100 usuarios a $15/mes = $1,500 brutos → ~$300/mes para ella.
- ⚠️ No cubre el valor de su retroalimentación pedagógica directamente. Esto se cubre con la cuenta gratuita y el reconocimiento público como Directora Pedagógica.
- ⚠️ Requiere un sistema de tracking de referidos antes de lanzamiento (link único por usuario o campo en registro).

---

## ADR-009: Pasarela de pagos — SINPE Móvil con verificación manual

**Fecha:** 2026-05-07
**Estado:** ✅ Decidido
**Decidido por:** Ezequiel Baltodano (dev principal)

### Contexto
El producto necesita cobrar suscripciones mensuales a docentes costarricenses. Se evaluaron pasarelas de pago automáticas vs. el sistema local SINPE Móvil.

### Opciones evaluadas

| Opción | Pros | Contras |
|--------|------|---------|
| **Stripe** | Automatización total, renovación sin intervención | No confirma disponibilidad para cuentas CR; comisiones 2.9% + $0.30/tx; los docentes no tienen tarjeta de crédito internacional |
| **PayPal** | Disponible en CR | Comisiones altas; no todos los docentes tienen cuenta; flujo engorroso |
| **SINPE Móvil + verificación manual** | Nativo de CR, todos los bancos participan, sin comisiones, todos los docentes saben usarlo | Sin API pública; requiere verificación manual por admin; no escala automáticamente |

### Decisión
**SINPE Móvil con verificación manual por administrador.**

- No existe API oficial de SINPE Móvil; toda verificación es manual.
- El flujo: usuario paga → sube comprobante → admin verifica → activa suscripción.
- El sistema genera un `reference_code` único por solicitud (formato `AUI-YYYYMMDD-XXXX`) para que el admin identifique cada pago en su app bancaria.
- El número SINPE de AulaIA se configura en `AppSettings` sin redeployar. El tipo de cambio USD/CRC se obtiene automáticamente del API oficial del BCCR mediante el job `UpdateExchangeRateJob` (Hangfire, diario).
- Este modelo es viable para la fase inicial (< 200 usuarios). Si el volumen crece, evaluar agregadores de pago costarricenses que soporten SINPE (ej. CincoDigitos, Kushki CR) — requerirá un nuevo ADR.

### Consecuencias
- ✅ Cero comisiones por transacción — 100% del precio llega al negocio.
- ✅ Universal en Costa Rica — cualquier docente con cualquier banco puede pagar.
- ✅ Sin dependencias de terceros ni cuentas de pasarela de pago.
- ✅ Implementación simple: solo lógica de BD y notificaciones.
- ⚠️ El admin debe verificar cada pago manualmente — carga operativa que crece con el volumen.
- ⚠️ Sin renovación automática — el docente debe recordar pagar cada mes y el admin aprobarlo.
- ⚠️ Riesgo de churn por fricción de renovación manual. Mitigación: notificaciones proactivas 7 días antes del vencimiento.

---

## ADR-007: Frontend Next.js como SPA estático servido por .NET 10

**Fecha:** 2026-05-05
**Estado:** ✅ Decidido
**Decidido por:** Ezequiel Baltodano (dev principal)

### Contexto
Se evaluó usar Azure Static Web App (swa-demo) para el frontend Next.js de forma separada al App Service del backend. Esto implica dos recursos de hosting, dos pipelines de CI/CD y CORS entre dominios diferentes.

### Opciones evaluadas

| Opción | Pros | Contras |
|--------|------|---------|
| Static Web App separado | CDN edge nativa, deploy independiente | Dos recursos, CORS, más complejidad CI/CD |
| SPA en App Service único | Un solo recurso, sin CORS, más simple | Sin CDN edge (se puede agregar Cloudflare CDN) |

### Decisión
**Next.js compilado como `output: 'export'` (HTML/CSS/JS estático) servido por .NET 10 desde `wwwroot/`.**

- Next.js usa `next.config.ts` con `output: 'export'`
- El build genera una carpeta `out/` que se copia a `wwwroot/` del proyecto .NET
- .NET 10 usa `app.UseStaticFiles()` + `app.MapFallbackToFile("index.html")` para el routing SPA
- Un único App Service `app-demo-api` sirve tanto la API (`/api/*`) como el frontend (`/*`)
- CI/CD: un solo workflow construye ambos y despliega al App Service

### Consecuencias
- ✅ Un solo recurso de Azure — menor costo y menor complejidad operativa
- ✅ Sin problemas de CORS entre frontend y backend
- ✅ Un solo pipeline de CI/CD
- ✅ Cloudflare actúa como CDN edge frente al App Service (caché de assets estáticos)
- ⚠️ Next.js pierde SSR/SSG dinámico — solo páginas estáticas o client-side rendering
- ⚠️ Para rutas que requieran SSR en el futuro, evaluar migración a Container Apps

---

## ADR-001: Backend con .NET 10 + Entity Framework Core

**Fecha:** 2026-05-04
**Estado:** ✅ Decidido
**Decidido por:** Ezequiel Baltodano (dev principal)

### Contexto
Se necesita un backend para exponer las APIs REST que consuman las apps de React Native y Next.js. El sistema maneja datos relacionales complejos (grupos, estudiantes, notas, asistencia, planeamientos) que se benefician de un ORM con soporte fuerte de migraciones y LINQ.

### Opciones evaluadas

| Opción | Pros | Contras |
|--------|------|---------|
| Node.js + NestJS | Comparte ecosistema con el frontend (TypeScript) | Menos maduro en ORM relacional; mayor esfuerzo en tipado de datos |
| .NET 10 + EF Core | Maduro, tipado fuerte, LINQ, migraciones automáticas, excelente en APIs REST | Ecosistema diferente al frontend (C# vs TypeScript) |

### Decisión
**Backend: .NET 10 (ASP.NET Core Minimal APIs) + Entity Framework Core 10**

- ORM: Entity Framework Core 10, enfoque **code-first** con migraciones automáticas.
- Base de datos: PostgreSQL (proveedor `Npgsql.EntityFrameworkCore.PostgreSQL`).
- Patrón de APIs: Minimal APIs para endpoints livianos + Controllers para módulos complejos si es necesario.

### Consecuencias
- ✅ Migraciones de base de datos gestionadas con `dotnet ef migrations`.
- ✅ Consultas type-safe con LINQ; menos errores en runtime.
- ✅ Soporte nativo para PostgreSQL con Npgsql.
- ✅ Stack consolidado en el ecosistema Azure (App Service, Key Vault, Managed Identity).
- ⚠️ El equipo debe manejar dos lenguajes: C# en backend, TypeScript en frontend.
- ⚠️ Las validaciones de dominio deben estar en el backend (C#); no duplicar lógica en el frontend.

---

## ADR-002: IA / LLM con Azure AI Foundry — GPT-5.5

**Fecha:** 2026-05-04  
**Estado:** ✅ Decidido  
**Decidido por:** Ezequiel Baltodano (dev principal)

### Contexto
El módulo de Planeamiento Didáctico requiere un LLM capaz de generar contenido estructurado, largo y semánticamente preciso, anclado al programa oficial del MEP. El modelo debe respetar terminología educativa costarricense y producir salidas en formato estructurado (JSON / Markdown).

### Opciones evaluadas

| Opción | Pros | Contras |
|--------|------|--------|
| Azure OpenAI (GPT-4o) | Maduro, bien integrado con Azure | Generación más lenta en documentos largos |
| Gemini API (Google) | Contexto muy largo, multimodal | Fuera del ecosistema Azure; más fricción con Auth |
| Azure AI Foundry — GPT-5.5 | Mejor razonamiento, mayor ventana de contexto, ecosistema Azure unificado | Modelo más reciente; costo por token más alto |

### Decisión
**Azure AI Foundry con modelo GPT-5.5**

- Plataforma: **Azure AI Foundry** (portal unificado de modelos de Azure).
- Modelo: **GPT-5.5** desplegado como endpoint en Azure.
- Autenticación: **Managed Identity** (sin claves en código; integración nativa con Azure App Service).
- Llamadas desde el backend (.NET 10) usando el SDK `Azure.AI.OpenAI` o `Microsoft.Extensions.AI`.
- Las prompts del sistema son **inmutables desde el cliente**; toda la lógica de prompt engineering vive en el backend.

### Consecuencias
- ✅ Ecosistema Azure unificado: Foundry + App Service + Key Vault + Managed Identity.
- ✅ Sin claves de API expuestas; autenticación por identidad administrada.
- ✅ Ventana de contexto grande: puede recibir el programa de estudio completo + instrucciones en un solo request.
- ✅ Salidas estructuradas (Structured Outputs / JSON mode) para generar planeamientos parseables.
- ⚠️ Costo por token más alto que modelos anteriores — implementar caché de planeamientos generados para no repetir llamadas idénticas.
- ⚠️ Requiere conexión a internet — la generación de planeamientos nuevos **no funciona offline** (comportamiento ya documentado en `00_context.md`).
- ⚠️ Los planeamientos ya generados se guardan localmente y están disponibles offline.

---

## ADR-003: Almacenamiento de archivos con Azure Blob Storage

**Fecha:** 2026-05-04  
**Estado:** ✅ Decidido  
**Decidido por:** Ezequiel Baltodano (dev principal)

### Contexto
El sistema genera archivos binarios que deben persistir y ser descargables: PDFs de planeamientos, reportes de notas, reportes de asistencia, archivos de exportación para el SIMAR y materiales adjuntos.

### Opciones evaluadas

| Opción | Pros | Contras |
|--------|------|--------|
| AWS S3 | Muy maduro, barato, amplia documentación | Fuera del ecosistema Azure; requiere credenciales separadas |
| Azure Blob Storage | Nativo en Azure, integra con Managed Identity, mismo ecosistema que Foundry y App Service | Costo ligeramente mayor que S3 en algunos tier |

### Decisión
**Azure Blob Storage**

- Autenticación: **Managed Identity** (sin connection strings con claves en código).
- SDK: `Azure.Storage.Blobs` desde el backend .NET 10.
- Organización de contenedores:
  - `planeamientos/` — PDFs generados por el módulo de IA
  - `reportes/` — actas de notas, asistencia e informes exportados
  - `exportaciones/` — archivos CSV/XLSX para subir al SIMAR
  - `adjuntos/` — materiales que el docente sube manualmente
- Acceso a archivos: URLs firmadas temporales (SAS tokens) generadas por el backend; el cliente nunca accede directamente con credenciales permanentes.

### Consecuencias
- ✅ Ecosistema Azure unificado: un solo proveedor cloud para backend, IA y almacenamiento.
- ✅ Sin claves expuestas; acceso por Managed Identity desde App Service.
- ✅ URLs SAS con expiración controlan el acceso sin lógica de proxy compleja.
- ✅ Integración nativa con Azure CDN si se necesita distribución de contenido estático a futuro.
- ⚠️ Los archivos generados offline (por ejemplo, PDF generado localmente) se sincronizan a Blob Storage cuando hay conexión.
- ⚠️ Definir política de retención y borrado automático de archivos temporales para controlar costos.

---

## ADR-004: Hosting en Azure App Service (Fases 1–2) y migración a Container Apps (Fase 3+)

**Fecha:** 2026-05-04  
**Estado:** ✅ Decidido  
**Decidido por:** Ezequiel Baltodano (dev principal)

### Contexto
El backend es una API .NET 10 monolítica en las fases iniciales. Se necesita una plataforma de hosting que minimice la fricción operativa durante el MVP y permita escalar cuando el sistema madure.

### Opciones evaluadas

| Opción | Pros | Contras |
|--------|------|--------|
| Azure App Service | Setup mínimo, deploy directo desde GitHub Actions, .NET 10 soportado nativamente, tier B1 ~$13/mes | Menos granularidad de escalado por módulo |
| Azure Container Apps | Escalado a cero, granular por servicio, ideal para microservicios | Requiere Docker, ACR, mayor complejidad operativa desde el día 1 |
| Vercel | Excelente para Next.js (frontend) | No aplica para backend .NET |

### Decisión

**Fases 1 y 2 — Azure App Service**
- Tier inicial: **B1** (desarrollo y MVP).
- Tier producción temprana: **S1** (habilita autoscaling y slots de deployment).
- Deploy automático desde GitHub Actions al branch `main`.
- Managed Identity habilitado para acceso a Blob Storage, Key Vault y Azure AI Foundry sin secretos en código.

**Fase 3+ — Migración a Azure Container Apps**
- Disparador de migración: cuando sea necesario escalar módulos de forma independiente (ej. separar el servicio de IA del API principal) o superar los límites de App Service.
- La migración implica: contenedorización de la API (.NET 10 en Docker), publicación en Azure Container Registry (ACR) y configuración de Container Apps Environment.
- El frontend Next.js puede migrar a **Azure Static Web Apps** o mantenerse en Vercel en esa fase.

### Consecuencias
- ✅ Setup rápido en Fase 1: un `az webapp up` o pipeline de GitHub Actions es suficiente.
- ✅ Sin conocimientos de Docker necesarios para el MVP.
- ✅ Ruta de migración clara y no urgente — no hay que prepararse para Container Apps desde el inicio.
- ⚠️ Al llegar a Fase 3, el código debe estar listo para contenedorizarse: sin dependencias de sistema de archivos local, configuración por variables de entorno, sin estado en memoria entre requests.

---

## ADR-005: Sincronización offline con PowerSync

**Fecha:** 2026-05-04  
**Estado:** ✅ Decidido  
**Decidido por:** Ezequiel Baltodano (dev principal)

### Contexto
El sistema debe funcionar offline en el aula (asistencia, notas, consulta de grupos y planeamientos). Los datos deben sincronizarse automáticamente con PostgreSQL cuando haya conexión, con resolución de conflictos y sin pérdida de datos.

### Opciones evaluadas

| Opción | Pros | Contras |
|--------|------|--------|
| PowerSync | Maduro, SDK para React Native y web, escrituras pasan por la API propia, tier gratuito | Requiere configurar un servicio PowerSync adicional |
| ElectricSQL | Open source, SDK JavaScript | v2 aún madurando; escrituras pueden saltarse el backend |
| Sincronización custom | Control total | Alto costo de desarrollo y mantenimiento |

### Decisión
**PowerSync**

- **Arquitectura de escrituras:** cliente → cola local (SQLite) → API .NET 10 → PostgreSQL. Las escrituras **nunca van directo a la base de datos**; siempre pasan por el backend.
- **Arquitectura de lecturas:** PowerSync Service sincroniza PostgreSQL → SQLite del dispositivo en tiempo real cuando hay conexión.
- **Hosting del servicio PowerSync:** self-hosted en Fase 1–2 (Docker en App Service o VM pequeña); tier cloud gratuito como alternativa.
- **SDKs a usar:**
  - `@powersync/react-native` para la app móvil (Expo)
  - `@powersync/web` para Next.js
  - Backend .NET 10 expone endpoint de tokens JWT para autenticar al cliente de PowerSync
- **Resolución de conflictos:** last-write-wins para asistencia y notas (el docente es el único dueño de sus datos); merge automático para listas de estudiantes.

### Consecuencias
- ✅ Toda validación de negocio se aplica en el backend .NET 10 antes de persistir en PostgreSQL.
- ✅ Datos de menores de edad y notas nunca se escriben directamente desde el cliente sin pasar por el backend.
- ✅ Offline funciona completamente para: asistencia (QR y manual), notas, consulta de grupos y planeamientos descargados.
- ✅ Sincronización automática al recuperar conexión, transparente para el docente.
- ⚠️ El backend debe exponer un endpoint `/api/powersync/token` que genere JWTs firmados para autenticar la sesión de sincronización.
- ⚠️ La generación de planeamientos con IA sigue requiriendo internet (limitación del LLM, no de PowerSync).

---

## ADR-006: Autenticación con Auth0

**Fecha:** 2026-05-04  
**Estado:** ✅ Decidido  
**Decidido por:** Ezequiel Baltodano (dev principal)

### Contexto
El sistema requiere autenticación segura para docentes en web y móvil, soporte para social login (Google), MFA, y generación de JWTs que el cliente use para autenticar sesiones de PowerSync. La solución no debe generar ambigüedad con el PostgreSQL ya elegido.

### Opciones evaluadas

| Opción | Pros | Contras |
|--------|------|--------|
| Auth0 | Solo auth, sin base de datos propia, SDK .NET maduro, 7,500 MAU gratis | Setup inicial con más pasos (tenant, callbacks) |
| Supabase Auth | Setup simple, SDK unificado, 50,000 MAU gratis | Viene atado al PG de Supabase; genera tensión con el PostgreSQL independiente ya decidido |

### Decisión
**Auth0**

- **SDK backend:** `Auth0.AspNetCore.Authentication` en .NET 10.
- **SDK frontend:** `react-native-auth0` en Expo; `@auth0/nextjs-auth0` en Next.js.
- **Flujo:** Authorization Code Flow + PKCE para móvil y web.
- **Social login habilitado desde el inicio:** Google. Apple en Fase 2 (requerido por App Store).
- **MFA:** habilitado como opción para docentes; no obligatorio en MVP.
- **JWTs de PowerSync:** el backend .NET 10 valida el token de Auth0 y emite un JWT firmado propio en `/api/powersync/token` para autenticar la sesión de sincronización.
- **Roles y permisos:** definidos como claims en Auth0 (ej. `rol: docente`, `rol: director`, `rol: admin`). El backend valida los claims en cada endpoint.
- **Plan inicial:** Free (7,500 MAU) — suficiente para cubrir la meta del año 1 (~500 docentes activos).

### Consecuencias
- ✅ Sin ambigüedad: Auth0 maneja identidades, PostgreSQL propio maneja datos de negocio.
- ✅ SDK `Auth0.AspNetCore.Authentication` es el más maduro para .NET; misión crítica ya probada en producción.
- ✅ Preparado para SSO institucional con Azure AD si un colegio lo requiere en el futuro (Auth0 soporta conexiones enterprise SAML/OIDC).
- ✅ 7,500 MAU gratis cubren cómodamente el año 1.
- ⚠️ Apple Sign-In es obligatorio en iOS si se ofrece cualquier social login — debe implementarse antes de publicar en App Store.
- ⚠️ Los roles del sistema (docente, director, admin) deben definirse como roles en el tenant de Auth0 desde el inicio para evitar refactorización posterior.
