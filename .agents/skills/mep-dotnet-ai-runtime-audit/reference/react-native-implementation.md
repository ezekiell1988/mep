# Implementación React Native / Expo — llm-audit-runtime

Integración para Expo (React Native). Detecta la IP del host en dev para conectar al backend local.

---

## Arquitectura

```
Expo App (emulador/físico)
  └─ llmAudit.logEvent(...)
       POST http://{HOST_IP}:{API_PORT}/api/diag/audit-event
            └─ ILlmAuditService → llm-audit.md / BD
```

---

## src/lib/llm-audit.ts

```typescript
import { Platform } from 'react-native';

const API_PORT = process.env.EXPO_PUBLIC_API_PORT ?? '8000';

// Android emulator accede al host via 10.0.2.2; iOS usa localhost
function getBaseUrl(): string {
  if (__DEV__) {
    if (Platform.OS === 'android') return `http://10.0.2.2:${API_PORT}`;
    return `http://localhost:${API_PORT}`;
  }
  // Producción: usar variable de entorno pública si se necesita
  return process.env.EXPO_PUBLIC_API_URL ?? '';
}

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
  if (!__DEV__) return;
  const base = getBaseUrl();
  if (!base) return;
  try {
    await fetch(`${base}/api/diag/audit-event`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(dto),
    });
  } catch {
    // Silenciar — el audit no debe romper la app
  }
}

export const llmAudit = {
  logEvent: (category: string, intent: string, result: string, context?: unknown) =>
    send({ type: 'event', category, intent, result, context }),

  logDecision: (area: string, decision: string, rationale: string) =>
    send({ type: 'decision', area, decision, rationale }),

  logError: (category: string, message: string, error?: Error) =>
    send({ type: 'error', category, message, stack: error?.stack }),
};
```

---

## .env

```
EXPO_PUBLIC_API_PORT=8000
EXPO_PUBLIC_API_URL=https://my-api.example.com   # solo si necesitas en producción
```

---

## Uso en pantallas

```typescript
import { llmAudit } from '../lib/llm-audit';

// Al navegar a una pantalla
useFocusEffect(() => {
  llmAudit.logEvent('HomeScreen', 'Screen focused', `userId=${userId}`);
});

// En acción de usuario
const handleSubmit = async () => {
  llmAudit.logEvent('LoginScreen', 'Intentando login', `provider=${provider}`);
  try {
    await auth.signIn(credentials);
    llmAudit.logEvent('LoginScreen', 'Login exitoso', `userId=${result.userId}`);
  } catch (err) {
    llmAudit.logError('LoginScreen', 'Login fallido', err instanceof Error ? err : undefined);
  }
};
```

---

## Notas para dispositivo físico (red local)

Si el emulador no alcanza localhost, usar la IP real del host:

```typescript
// Temporal para debug con dispositivo físico en misma red:
const BASE_URL = 'http://192.168.1.X:8000';
```

> Para producción, `__DEV__` es `false` y `send()` retorna sin hacer nada. El audit es solo para desarrollo.
