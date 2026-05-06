# Referencia: mep-dotnet-ai-runtime-audit en aulaia-app (React Native / Expo)

Cadena completa: **aulaia-app** (Expo `__DEV__` mode) → acceso directo IP local → **AulaIA.Api** (:8000) → `logs/llm-audit.md`

---

## Arquitectura

```
Expo app (__DEV__ mode)
  └─ llmAudit.logEvent / logDecision / logError
       POST http://{LOCAL_IP}:8000/diag/audit-event
         └─ ILlmAuditService → logs/llm-audit.md
```

> No hay proxy en React Native. El dispositivo/simulador accede directamente a la IP de la máquina de desarrollo.

---

## IP local para desarrollo

| Plataforma | URL recomendada |
|---|---|
| iOS Simulator | `http://localhost:8000` (mismo host) |
| Android Emulator | `http://10.0.2.2:8000` (alias del host en AVD) |
| Dispositivo físico (misma red) | `http://{IP_LOCAL}:8000` (ej: `http://192.168.1.100:8000`) |

Usar `Constants.expoConfig?.hostUri` de `expo-constants` para obtener la IP automáticamente en Expo Go.

---

## LlmAuditService — src/lib/llm-audit.ts

```typescript
import Constants from 'expo-constants';

// Inferir IP del host desde Expo hostUri (ej: "192.168.1.100:8081")
function getApiBase(): string {
  if (!__DEV__) return '';
  const hostUri = Constants.expoConfig?.hostUri ?? 'localhost:8081';
  const host = hostUri.split(':')[0];
  return `http://${host}:8000`;
}

const BASE_URL = getApiBase();

type AuditEventDto = {
  type: 'event' | 'decision' | 'error';
  category?: string;
  area?: string;
  intent?: string;
  result?: string;
  decision?: string;
  rationale?: string;
  message?: string;
  stack?: string;
  context?: unknown;
};

async function send(dto: AuditEventDto): Promise<void> {
  if (!__DEV__ || !BASE_URL) return;
  try {
    await fetch(`${BASE_URL}/diag/audit-event`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(dto),
    });
  } catch {
    // No-op — no interrumpir la UI si el backend no está disponible
  }
}

export const llmAudit = {
  logEvent: (category: string, intent: string, result: string, context?: unknown) =>
    send({ type: 'event', category, intent, result, context }),

  logDecision: (area: string, decision: string, rationale: string) =>
    send({ type: 'decision', area, decision, rationale }),

  logError: (category: string, message: string, error?: unknown) =>
    send({
      type: 'error',
      category,
      message,
      stack: error instanceof Error ? error.stack : String(error),
    }),
};
```

---

## Uso en screens React Native

```typescript
// screens/GruposScreen.tsx
import { llmAudit } from '@/lib/llm-audit';

export default function GruposScreen() {
  useEffect(() => {
    llmAudit.logEvent('GruposScreen', 'Montar pantalla', '✅ OK');
  }, []);

  const handleSyncError = (err: unknown) => {
    llmAudit.logError('PowerSync', 'Error de sincronización', err);
  };

  // ...
}
```

---

## Integración con PowerSync

PowerSync tiene su propio flujo de sincronización que puede auditarse:

```typescript
// src/powersync/PowerSyncContext.tsx (con audit)
import { llmAudit } from '@/lib/llm-audit';

// Al inicializar el cliente
llmAudit.logEvent('PowerSync', 'Inicializar cliente', '✅ OK', {
  endpoint: POWERSYNC_URL,
});

// Al detectar cambio de estado de conexión
db.statusStream.subscribe(status => {
  llmAudit.logEvent('PowerSync', 'Cambio de estado', status.connected ? '🟢 online' : '🔴 offline');
});
```

---

## Variables de entorno Expo (opcional)

Para hardcodear la URL en lugar de inferirla automáticamente:

```env
# .env.local (no committear)
EXPO_PUBLIC_LLM_AUDIT_URL=http://192.168.1.100:8000
```

```typescript
const BASE_URL = __DEV__ ? (process.env.EXPO_PUBLIC_LLM_AUDIT_URL ?? getApiBase()) : '';
```

---

## Comandos LLM para leer logs

```bash
# Ver audit log (mobile + API juntos)
curl http://localhost:8000/diag/audit

# Limpiar para nueva sesión
curl -X DELETE http://localhost:8000/diag/audit

# Ver contexto de la app
curl http://localhost:8000/diag/context | jq
```

---

## Diferencias respecto a aulaia-web

| Aspecto | aulaia-web (Next.js) | aulaia-app (Expo) |
|---|---|---|
| URL del backend | `/api` (proxy rewrite) | `http://{IP}:8000` (directo) |
| Guard dev-only | `process.env.NODE_ENV !== 'development'` | `!__DEV__` (global Expo) |
| IP dinámica | No aplica | Inferir de `Constants.expoConfig.hostUri` |
| CORS | No necesario (same-origin via proxy) | Configurado en `CorsExtensions.DevPolicy` |

---

## Requisitos para que funcione

1. `AulaIA.Api` corriendo en `:8000` con `LlmAudit.Enabled = true`
2. `CorsExtensions.DevPolicy` permite origen `http://{IP}:8081` (Expo dev server)
3. `expo start` corriendo
4. `src/lib/llm-audit.ts` creado con `getApiBase()` para IP dinámica

---

## Activación paso a paso

```bash
# 1. Iniciar AulaIA.Api
cd src/AulaIA.Api && dotnet run --launch-profile http

# 2. Iniciar Expo
cd mobile/aulaia-app && npx expo start

# 3. Verificar que el log recibe eventos desde el dispositivo
curl http://localhost:8000/diag/audit
```
