---
name: mep-cloudflare
description: >
  Gestión de DNS de Cloudflare para el proyecto AulaIA (ezekl.com) usando flarectl CLI.
  Documenta credenciales, IDs de registros existentes, comandos frecuentes y el flujo
  completo para vincular un dominio personalizado a un Azure Container App.
  Usar cuando se necesite crear, actualizar o eliminar registros DNS, rotar dominios
  entre recursos Azure, o configurar dominios personalizados en Container Apps.
  Triggers: cloudflare, flarectl, dns, cname, txt, dominio, registros dns, api.mep,
  mep.ezekl.com, zona dns, verificacion dominio, asuid, proxy cloudflare.
applyTo: "**/*.{sh,yml,yaml}"
---

# mep-cloudflare — DNS de Cloudflare para AulaIA

## 1. Credenciales y zona

```bash
export CF_API_TOKEN=2CqTxSpta2AhX5_buCbED2EIJLh6r9FVHEgfcgYi
export CF_ZONE=ezekl.com
export CF_ZONE_ID=1ab102a0434b960afd1ff5543c09c9cd
```

> El token tiene permisos de lectura y escritura sobre la zona `ezekl.com`.

---

## 2. CLI — flarectl

**Instalación:**
```bash
brew install flarectl
```

**El token se pasa como variable de entorno `CF_API_TOKEN` — no tiene flag `--token`.**

```bash
# Siempre exportar antes de usar
export CF_API_TOKEN=2CqTxSpta2AhX5_buCbED2EIJLh6r9FVHEgfcgYi
```

**Comandos frecuentes:**
```bash
flarectl dns list --zone ezekl.com

flarectl dns list --zone ezekl.com | grep "mep.ezekl"

flarectl dns create --zone ezekl.com \
  --type CNAME \
  --name "subdominio.ezekl.com" \
  --content "destino.azurecontainerapps.io" \
  --ttl 1

flarectl dns create-or-update --zone ezekl.com \
  --type TXT \
  --name "asuid.subdominio.ezekl.com" \
  --content "VERIFICATION_ID" \
  --ttl 1

flarectl dns update --zone ezekl.com \
  --id <ID_DEL_REGISTRO> \
  --name "subdominio.ezekl.com" \
  --type CNAME \
  --content "nuevo-destino.io" \
  --ttl 1

flarectl dns delete --zone ezekl.com --id <ID_DEL_REGISTRO>
```

> ⚠️ **TRAMPAS CONOCIDAS — leer antes de ejecutar:**
>
> 1. **`create-or-update` NO actualiza registros existentes.** Si el CNAME ya existe, falla con error 81053. Para registros existentes, obtener el ID con `dns list` y usar `dns update --id`.
> 2. **`--proxy false` no existe como flag.** El proxy está OFF por defecto — simplemente no pasar `--proxy`. Pasar `--proxy` activa el proxy (ON). No hay forma de pasarlo como `false`.
> 3. **El token SIEMPRE debe estar exportado.** Nunca usar `CF_API_TOKEN=valor flarectl ...` (inline puede fallar en zsh). Usar siempre `export CF_API_TOKEN=...` antes de cualquier comando.
> 4. **No pegar comentarios `#` en comandos multi-línea en zsh interactivo.** Zsh trata los `#` como comandos y lanza `command not found`. Ejecutar cada bloque sin comentarios inline.

---

## 3. Registros DNS actuales del proyecto

| ID | Tipo | Nombre | Destino | Proxy |
|----|------|--------|---------|-------|
| `6f568a708630fc755fb619eef7da30e9` | CNAME | `mep.ezekl.com` | `ca-aulaia-api.whitewater-319185f7.eastus.azurecontainerapps.io` | ❌ off |
| `299b07e272e98bd5708c798c12cd807d` | TXT | `asuid.mep.ezekl.com` | `232F49FAC2950455DEE2EC249E1AFFE5D754A7B0AF9ADF8F6B610723E3C15C72` | — |
| `ea7bc78df715bffec06a12d2be08799b` | TXT | `asuid.itqs.ezekl.com` | `232F49FAC2950455DEE2EC249E1AFFE5D754A7B0AF9ADF8F6B610723E3C15C72` | — |

---

## 4. Flujo completo — Vincular dominio a Azure Container App

### Paso 1 — Obtener el Verification ID del Container Apps Environment

```bash
az containerapp env show \
  --resource-group rg-ezequiel \
  --name cae-demo-itqs \
  --query "properties.customDomainConfiguration.customDomainVerificationId" \
  --output tsv
# → 232F49FAC2950455DEE2EC249E1AFFE5D754A7B0AF9ADF8F6B610723E3C15C72
```

### Paso 2 — Crear registros DNS en Cloudflare

```bash
export CF_API_TOKEN=2CqTxSpta2AhX5_buCbED2EIJLh6r9FVHEgfcgYi
```

Si el CNAME **no existe** (dominio nuevo):
```bash
flarectl dns create --zone ezekl.com \
  --type CNAME \
  --name "nuevo.ezekl.com" \
  --content "ca-aulaia-api.whitewater-319185f7.eastus.azurecontainerapps.io" \
  --ttl 1
```

Si el CNAME **ya existe** (cambiar destino — obtener ID con `dns list` primero):
```bash
flarectl dns update --zone ezekl.com \
  --id <ID_OBTENIDO_DE_DNS_LIST> \
  --name "dominio.ezekl.com" \
  --type CNAME \
  --content "ca-aulaia-api.whitewater-319185f7.eastus.azurecontainerapps.io" \
  --ttl 1
```

TXT de verificación (siempre `create-or-update`, es idempotente para TXT nuevos):
```bash
flarectl dns create-or-update --zone ezekl.com \
  --name "asuid.dominio.ezekl.com" \
  --type TXT \
  --content "232F49FAC2950455DEE2EC249E1AFFE5D754A7B0AF9ADF8F6B610723E3C15C72" \
  --ttl 1
```

> El proxy Cloudflare debe estar **OFF** (no pasar `--proxy`) en cualquier CNAME que apunte a Container App. Azure necesita resolver directo para emitir el certificado gestionado.

### Paso 3 — Esperar propagación DNS antes de continuar

```bash
dig TXT asuid.dominio.ezekl.com +short
```

Si no devuelve nada, esperar 15–30 segundos y reintentar. **No continuar al paso 4 hasta que `dig` devuelva el verification ID.**

### Paso 4 — Agregar hostname al Container App

> ⚠️ **Este paso es obligatorio antes del `hostname bind`.** Intentar `bind` sin `add` previo falla con `RequireCustomHostnameInEnvironment`.

```bash
az containerapp hostname add \
  --resource-group rg-ezequiel \
  --name ca-aulaia-api \
  --hostname dominio.ezekl.com
```

El resultado muestra `"bindingType": "Disabled"` — es correcto, el cert se vincula en el siguiente paso.

### Paso 5 — Emitir y vincular certificado gestionado

```bash
az containerapp hostname bind \
  --resource-group rg-ezequiel \
  --name ca-aulaia-api \
  --environment cae-demo-itqs \
  --hostname dominio.ezekl.com \
  --validation-method CNAME
```

Este comando puede tardar hasta 20 minutos. Al terminar muestra `"bindingType": "SniEnabled"`.

### Paso 6 — Verificar

```bash
az containerapp hostname list \
  --resource-group rg-ezequiel \
  --name ca-aulaia-api \
  --output table

curl -s -o /dev/null -w "%{http_code}" https://dominio.ezekl.com/health
```

---

## 5. Proxy Cloudflare — cuándo activarlo/desactivarlo

| Escenario | Proxy |
|-----------|-------|
| Azure emitiendo certificado gestionado | ❌ OFF (DNS-only) |
| Container App con cert gestionado activo | ❌ OFF (recomendado, Azure ya tiene TLS) |
| App Service con cert propio | ✅ ON (Cloudflare gestiona TLS hacia el cliente) |
| Quiero WAF / DDoS / caché de Cloudflare | ✅ ON (solo si el cert no es gestionado por Azure) |

> Para `mep.ezekl.com` → Container App: dejar **OFF** permanentemente.

---

## 6. Lecciones aprendidas (2026-05-08)

| Error | Causa | Corrección |
|-------|-------|------------|
| `create-or-update` falla con 81053 en CNAME | El comando no actualiza, solo crea | Usar `dns list` para obtener ID y luego `dns update --id` |
| `--proxy false` causa error de sintaxis | El flag no existe | Simplemente omitir `--proxy`; OFF es el default |
| `hostname bind` falla con `RequireCustomHostnameInEnvironment` | Se saltó el `hostname add` previo | Siempre `hostname add` → verificar DNS → `hostname bind` |
| `hostname add` falla con `InvalidCustomHostNameValidation` | El TXT aún no propagó | Verificar con `dig TXT asuid.<dominio> +short` antes de continuar |
| Comentarios `#` causan `command not found` en zsh | Zsh interactivo no acepta `#` inline en multi-línea | Ejecutar comandos sin comentarios inline; anotar en otra parte |
