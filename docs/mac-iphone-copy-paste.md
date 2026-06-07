# Activar Copy/Paste entre Mac y iPhone

Guía rápida para activar y diagnosticar el Portapapeles universal de Apple entre macOS y iPhone.

## Requisitos

- Mac y iPhone cerca físicamente.
- Ambos dispositivos con el mismo Apple ID en iCloud.
- Wi-Fi encendido en ambos dispositivos.
- Bluetooth encendido en ambos dispositivos.
- Handoff activado en Mac y iPhone.
- No usar Hotspot Personal durante la prueba.

## Revisar en el iPhone

1. Abrir `Configuración`.
2. Ir a `General > AirPlay y Continuidad`.
3. Activar `Handoff`.
4. Confirmar que Wi-Fi y Bluetooth estén encendidos.
5. Confirmar que el iPhone usa el mismo Apple ID que el Mac.

## Revisar en el Mac desde Configuración

1. Abrir `Configuración del Sistema`.
2. Ir a `General > AirDrop y Handoff`.
3. Activar `Permitir Handoff entre esta Mac y tus dispositivos iCloud`.
4. Confirmar que Wi-Fi y Bluetooth estén encendidos.

## Verificar en el Mac desde Terminal

Comprobar versión de macOS:

```bash
sw_vers
```

Comprobar Bluetooth:

```bash
system_profiler SPBluetoothDataType | sed -n '1,120p'
```

Comprobar Wi-Fi:

```bash
networksetup -getairportpower en0 2>/dev/null || networksetup -getairportpower en1 2>/dev/null
```

Comprobar servicios de Continuity:

```bash
pgrep -lf 'useractivityd|sharingd|rapportd|bluetoothd|identityservicesd'
```

Leer el estado del portapapeles compartido:

```bash
defaults read com.apple.coreservices.useractivityd ClipboardSharingEnabled
```

Resultado esperado:

```text
1
```

Si devuelve `0`, el portapapeles compartido está desactivado en el Mac.

## Activar portapapeles compartido en macOS

Ejecutar:

```bash
defaults write com.apple.coreservices.useractivityd ClipboardSharingEnabled -bool true
defaults write com.apple.coreservices.useractivityd ActivityAdvertisingAllowed -bool true
defaults write com.apple.coreservices.useractivityd ActivityReceivingAllowed -bool true
```

Reiniciar los servicios de Continuity:

```bash
killall useractivityd 2>/dev/null
killall sharingd 2>/dev/null
killall rapportd 2>/dev/null
```

macOS los levanta de nuevo automáticamente. Confirmar:

```bash
pgrep -lf 'useractivityd|sharingd|rapportd|identityservicesd'
```

Validar que quedó activo:

```bash
defaults read com.apple.coreservices.useractivityd ClipboardSharingEnabled
defaults read com.apple.coreservices.useractivityd ActivityAdvertisingAllowed
defaults read com.apple.coreservices.useractivityd ActivityReceivingAllowed
```

Resultado esperado:

```text
1
1
1
```

## Probar

1. Copiar texto simple en el Mac.
2. Esperar 2 a 5 segundos.
3. Pegar en el iPhone.
4. Probar al revés: copiar texto en el iPhone y pegar en el Mac.

## Si sigue fallando

1. Reiniciar el iPhone.
2. Reiniciar el Mac.
3. Apagar y encender Wi-Fi y Bluetooth en ambos dispositivos.
4. Confirmar que ambos estén en el mismo Apple ID.
5. Desactivar y volver a activar Handoff en ambos dispositivos.
6. Evitar VPNs, Hotspot Personal o redes administradas durante la prueba.

## Caso diagnosticado

En el Mac revisado el problema era:

```text
ClipboardSharingEnabled = 0
```

Después de activarlo y reiniciar `useractivityd`, `sharingd` y `rapportd`, los valores quedaron:

```text
ClipboardSharingEnabled = 1
ActivityAdvertisingAllowed = 1
ActivityReceivingAllowed = 1
```
