---
name: eas-cli
description: Referencia de EAS CLI (Expo Application Services) para el proyecto AulaIA. Cubre autenticación, vinculación de proyecto, builds de Android APK/AAB e iOS, y diagnóstico de errores frecuentes. Usar cuando se necesite compilar la app móvil, generar APK para distribución directa, configurar eas.json, o cuando EAS CLI lance errores. Triggers: eas build, eas init, eas login, eas whoami, apk android, build expo, non-interactive, project not configured, eas.json, preview build, eas submit.
---

# EAS CLI — AulaIA

## Datos del proyecto

| Campo | Valor |
|-------|-------|
| Cuenta expo.dev | `ezekiell1988` (email: `ezekiell1988@hotmail.com`) |
| Auth method | GitHub SSO |
| Slug (app.json) | `aulaia-app` |
| Android package | `cr.ezekl.aulaia` |
| iOS bundle ID | `cr.ezekl.aulaia` |
| Directorio del proyecto | `mobile/aulaia-app/` |

## Login

Con usuario/contraseña:
```bash
eas login
```

Con GitHub (cuenta creada vía SSO — usar este):
```bash
eas login --sso
# Abre el browser, autenticá con GitHub
```

Verificar sesión activa:
```bash
eas whoami
# Debe mostrar: ezekiell1988
```

## Vincular proyecto a expo.dev (solo la primera vez)

```bash
cd mobile/aulaia-app
eas init
# Pregunta si crear proyecto nuevo o vincular existente
# → elegir "Create a new project"
# Esto escribe el campo "projectId" en app.json
```

**IMPORTANTE:** `eas build` falla con `"Run this command inside a project directory"` si:
1. No estás parado en `mobile/aulaia-app/` (el directorio con `app.json` y `eas.json`)
2. El proyecto no fue inicializado con `eas init`

## eas.json — Configuración de perfiles

Ubicación: `mobile/aulaia-app/eas.json`

```json
{
  "cli": { "version": ">= 16.0.0" },
  "build": {
    "preview": {
      "android": { "buildType": "apk" }
    },
    "production": {
      "android": { "buildType": "app-bundle" },
      "ios": { "distribution": "store" }
    }
  }
}
```

- `preview` → genera `.apk` instalable directamente (distribución directa / WhatsApp)
- `production` → genera `.aab` para Play Store

## Build APK Android (distribución directa)

```bash
cd /Users/ezequielbaltodanocubillo/Documents/ezekl/mep/mobile/aulaia-app
eas build --platform android --profile preview
```

- Buildea en la nube de Expo (~10-15 min)
- Al terminar da un link de descarga del `.apk`
- Plan gratuito: 30 builds/mes en Linux

Con flag `--non-interactive` (sin prompts):
```bash
eas build --platform android --profile preview --non-interactive
```

## Build iOS (ad-hoc con Xcode local, cuenta gratuita)

No usar EAS para iOS durante el piloto — la cuenta gratuita de Apple expira en 7 días y EAS requiere Apple Developer ($99/año).

Usar Xcode directamente:
1. Conectar iPhone de Adriana por USB
2. Abrir `mobile/aulaia-app/ios/` en Xcode
3. Seleccionar dispositivo físico
4. Product → Run (firma con Apple ID gratuito)

## Errores frecuentes

| Error | Causa | Solución |
|-------|-------|----------|
| `Run this command inside a project directory` | No estás en `mobile/aulaia-app/` o falta `eas init` | `cd mobile/aulaia-app && eas init` |
| `EAS project not configured` | Falta `projectId` en `app.json` | `eas init` |
| `Must configure EAS project` | Igual que arriba | `eas init` |
| `Not logged in` | Sesión expirada | `eas login --sso` |
| Build falla por `package` faltante | `android.package` no definido en `app.json` | Agregar `"package": "cr.ezekl.aulaia"` |

## Flujo completo para generar APK piloto

```bash
# 1. Ir al directorio
cd /Users/ezequielbaltodanocubillo/Documents/ezekl/mep/mobile/aulaia-app

# 2. Verificar login
eas whoami

# 3. Vincular proyecto (solo primera vez)
eas init

# 4. Build APK
eas build --platform android --profile preview

# 5. Descargar el .apk desde el link que da EAS
# 6. Subir a Google Drive y compartir con Adriana por WhatsApp
```
