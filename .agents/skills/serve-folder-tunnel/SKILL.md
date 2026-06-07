---
name: serve-folder-tunnel
description: >
  Levantar una carpeta local con archivos estáticos (HTML, CSS, JS, imágenes) en un
  servidor HTTP y exponerla a Internet via VS Code Dev Tunnel. Usa npx serve (sin
  instalación previa) para el servidor y Dev Tunnels para la URL pública. Incluye
  scripts PowerShell para iniciar y detener el servidor desde terminal o desde el LLM.
  Triggers: servir carpeta, archivos html públicos, compartir html, carpeta pública,
  serve folder tunnel, npx serve tunnel, exponer html, static files tunnel,
  ver html en url pública, compartir archivos estáticos, levantar servidor estático,
  mostrar html a cliente, html en internet, serve public, static server devtunnel.
---

# Servir carpeta estática con Dev Tunnel

Patrón mínimo para exponer archivos HTML/CSS/JS a Internet sin deploy, usando
`npx serve` + VS Code Dev Tunnel.

> Ver también: [vscode-devtunnels](../vscode-devtunnels/SKILL.md) — conceptos de visibilidad,
> tunnels nombrados y patrón proxy Angular + API.

---

## Flujo completo

```
carpeta/
├── index.html
├── reporte.html
└── assets/

npx serve carpeta/ -p 3000
        ↓
VS Code Dev Tunnel (puerto 3000 → Público)
        ↓
https://abc123-3000.devtunnels.ms  ← URL pública
```

---

## Pasos manuales (referencia)

1. Abrir terminal en la carpeta o pasar la ruta al script
2. Ejecutar `npx serve <ruta> -p 3000`
3. En VS Code → pestaña **Puertos** → **Agregar puerto** → `3000`
4. Clic derecho → **Visibilidad del puerto** → **Público**
5. Copiar URL de la columna "Dirección reenviada"

---

## Scripts PowerShell (para el LLM)

| Script | Qué hace |
|--------|---------|
| [examples/start-serve.ps1](examples/start-serve.ps1) | Inicia `npx serve` en background, guarda el Job ID, muestra instrucciones del tunnel |
| [examples/stop-serve.ps1](examples/stop-serve.ps1) | Detiene el proceso serve por Job ID guardado |

### Uso

```powershell
# Iniciar — con defaults (carpeta actual, puerto 3000)
.\examples\start-serve.ps1

# Iniciar — ruta y puerto custom
.\examples\start-serve.ps1 -Path "C:\mis-reportes" -Port 4500

# Detener
.\examples\stop-serve.ps1
```

---

## Prerrequisitos

- Node.js y npm instalados (`node --version`)
- VS Code con Dev Tunnels habilitado (requiere cuenta GitHub o Microsoft)

---

## Limitaciones

- La URL del tunnel **cambia** al reiniciar VS Code. Para URL persistente:
  ```powershell
  devtunnel create mi-tunnel
  devtunnel port create mi-tunnel -p 3000
  devtunnel host mi-tunnel --allow-anonymous
  ```
- `npx serve` sirve archivos estáticos; no ejecuta PHP, .NET ni ningún backend
- La carpeta completa queda expuesta públicamente — no incluir archivos con credenciales
