# Dev Tunnel + Copilot Studio — Probar un MCP Server local

## Caso de uso

Quieres conectar un agente de **Microsoft Copilot Studio** a tu servidor MCP local (ej. `eVista.Mcp` en puerto `8084`) **sin hacer deploy**, usando un Dev Tunnel como URL temporal pública.

---

## El único bloqueador: visibilidad del túnel

Copilot Studio es un servicio cloud. Cuando registras la URL del tunnel en CS, sus servidores hacen un request desde internet a esa URL.

| Visibilidad del tunnel | Resultado en Copilot Studio |
|------------------------|-----------------------------|
| **Privado** (default)  | ❌ CS no puede alcanzar la URL — falla al conectar |
| **Organización**       | ❌ Solo funciona con cuentas del mismo tenant       |
| **Público**            | ✅ CS puede llamar al servidor sin restricciones    |

**Cómo cambiarlo:** En la pestaña **Puertos** → clic derecho sobre la fila del puerto → **"Visibilidad del puerto"** → **Público**.

---

## Checklist completo para eVista.Mcp → Copilot Studio

| Requisito | Cómo verificar |
|-----------|---------------|
| MCP server corriendo en 8084 | Terminal `4. MCP - Local (8084)` activo |
| Puerto 8084 forwardeado en VS Code | Pestaña Puertos muestra fila con 8084 |
| Visibilidad = **Público** | Columna "Visibilidad" dice "Público" (no "Privado") |
| HTTPS en la URL | La URL del tunnel empieza con `https://` ✅ (Dev Tunnels siempre HTTPS) |
| `routePrefix: ""` en `host.json` | Obligatorio para Azure Functions — sin esto CS no conecta |
| Servidor responde `notifications/*` con HTTP 202 | Implementado en `McpEndpoints.cs` |

---

## Pasos

1. Asegúrate de que el MCP esté corriendo (task `4. MCP - Local (8084)`)
2. En la pestaña **Puertos**, si el puerto `8084` no aparece:
   - Clic en **"Agregar puerto"** → escribe `8084`
3. Clic derecho sobre la fila `8084` → **Visibilidad del puerto** → **Público**
4. Copia la URL del tunnel (columna "Dirección reenviada"), ej:
   ```
   https://5d4wx....devtunnels.ms
   ```
5. En Copilot Studio → tu agente → **Acciones** → **Agregar una acción** → **Servidor MCP**
6. Pega la URL → CS descubrirá las tools automáticamente vía `tools/list`

---

## Advertencias

- **La URL cambia** cada vez que reinicias VS Code o el tunnel. Deberás actualizarla en CS para cada sesión.
- Para una URL persistente usa un tunnel nombrado con la CLI:
  ```bash
  devtunnel create evista-mcp
  devtunnel port create evista-mcp -p 8084
  devtunnel host evista-mcp --allow-anonymous
  ```
- **No usar en producción** — el Dev Tunnel expone tu máquina local. Solo para pruebas de desarrollo.

---

## Referencia relacionada

- [Skill mcp-copilot-studio](../../mcp-copilot-studio/SKILL.md) — Implementación completa del servidor MCP compatible con Copilot Studio
- Datos reales del protocolo: `mcp-copilot-studio/reference/08-no-auth-real-data.md` y `09-oauth2-real-data.md`
