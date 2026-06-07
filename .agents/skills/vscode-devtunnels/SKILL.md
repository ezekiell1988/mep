---
name: vscode-devtunnels
description: >
  Exponer servidores localhost a Internet usando VS Code Dev Tunnels, sin deploy.
  Cubre cómo abrir un túnel, configurar visibilidad, y conectar Angular + API en
  desarrollo local para que un usuario externo pueda acceder. Incluye el patrón
  proxy Angular para que el browser remoto no llame a localhost directamente.
  Triggers: devtunnel, dev tunnel, exponer localhost, localhost a internet,
  compartir servidor local, tunnel vscode, forward port, webhook localhost,
  probar desde celular, acceso externo local, proxy angular tunnel.
---

# VS Code Dev Tunnels — Exponer localhost a Internet

## ¿Qué son los Dev Tunnels?

Dev Tunnels son túneles seguros que exponen un servidor local a Internet a través de una URL pública temporal. No requieren deploy ni configuración de red.

Casos de uso:
- Compartir una app en desarrollo con un colega o cliente sin subir nada
- Recibir webhooks externos (GitHub, Adobe Sign, Stripe) en localhost
- Probar desde un dispositivo móvil sin estar en la misma red

---

## Cómo abrir un túnel en VS Code

1. Corre tu app localmente (ej. `ng serve` en puerto `4200`)
2. En VS Code, abre la pestaña **Ports** (barra inferior)
3. Clic en **"Forward a Port"** → ingresa el número de puerto
4. VS Code genera una URL pública como:
   ```
   https://abc123-4200.usw3.devtunnels.ms
   ```
5. Comparte esa URL — es accesible desde cualquier lugar

> Requiere cuenta de **GitHub** o **Microsoft** para autenticarse.

---

## Visibilidad del túnel

| Opción | Quién puede acceder |
|--------|-------------------|
| `Private` | Solo tú (requiere login con tu cuenta) |
| `Organization` | Miembros de tu organización GitHub/Microsoft |
| `Public` | Cualquier persona en Internet, sin autenticación |

Clic derecho sobre el puerto en la pestaña Ports → **"Change Port Visibility"**.

---

## Patrón para Angular + API (dos proyectos locales)

### El problema

Cuando alguien accede desde fuera con la URL del tunnel, el `localhost:8082` en el browser apunta a **su máquina**, no a la tuya. Resultado: el API no conecta.

### La solución: un solo túnel + proxy de Angular

Solo expones el **puerto de Angular** (ej. 4200). Angular reenvía internamente las llamadas al API a tu `localhost:8082` usando el proxy del dev server.

```
Browser remoto
    ↓  https://abc123-4200.devtunnels.ms/api/...
Angular Dev Server (tu máquina, puerto 4200)
    ↓  proxy interno → http://localhost:8082/api/...
eVista.Api (tu máquina, puerto 8082)
```

### Configuración del proxy (`proxy.conf.json`)

```json
{
  "/api": {
    "target": "http://localhost:8082",
    "secure": false,
    "changeOrigin": true,
    "logLevel": "debug"
  }
}
```

> En eVista ya está configurado en `eVista.Angular.Web/proxy.conf.json`.

### Environment de Angular — usar rutas relativas

Para que el proxy aplique siempre, el `environment.ts` debe usar una URL **relativa**, no `http://localhost:8082`:

```ts
// environment.ts
export const environment = {
  apiUrl: '/api'   // relativo → pasa por el proxy del dev server
};
```

Con `http://localhost:8082/api` el browser remoto fallaría; con `/api` el proxy lo intercepta en el servidor Angular.

---

## Pasos rápidos para eVista

1. Iniciar el API: `func start --port 8082`
2. Iniciar Angular: `npm run start-local` (usa proxy)
3. En VS Code → pestaña Ports → Forward `4200`
4. Cambiar visibilidad a `Public` o `Organization`
5. Compartir la URL generada

Solo **un tunnel** es necesario (el del puerto 4200).

---

## Uso vía CLI (alternativa)

```bash
# Instalar CLI (una sola vez)
winget install Microsoft.devtunnel

# Exponer un puerto
devtunnel host -p 4200

# Con visibilidad pública
devtunnel host -p 4200 --allow-anonymous
```

---

## Casos de uso avanzados

| Escenario | Referencia |
|-----------|-----------|
| Probar un MCP Server local con Copilot Studio | [references/copilot-studio-mcp.md](references/copilot-studio-mcp.md) |

---

## Limitaciones

- La URL cambia cada vez que reinicias el túnel (no es fija)
- Para URL persistente se necesita un túnel nombrado con la CLI:
  ```bash
  devtunnel create mi-tunnel
  devtunnel port create mi-tunnel -p 4200
  devtunnel host mi-tunnel
  ```
- No es sustituto de un deploy real; es exclusivo para desarrollo y pruebas
