# Implementación Next.js — llm-audit-runtime

Integración para reenviar eventos desde **Next.js** al backend .NET vía proxy de rewrites.
Sin CORS, same-origin desde el browser.

---

## Arquitectura

```
Browser (next dev :3000)
  └─ llmAudit.logEvent(...)
       POST /api/diag/audit-event   ← path relativa, Next.js intercepta
         └─ next.config.ts rewrite → http://localhost:{API_PORT}/api/diag/audit-event
              └─ ILlmAuditService → llm-audit.md / BD
```

---

## next.config.ts — rewrite

```typescript
const nextConfig: NextConfig = {
  async rewrites() {
    const apiUrl = process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:8000';
    return [{ source: '/api/:path*', destination: `${apiUrl}/api/:path*` }];
  },
};
```

Definir en `.env.local` (no commitear):
```
NEXT_PUBLIC_API_URL=http://localhost:8000
```

---

## src/lib/llm-audit.ts

```typescript
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

const ENDPOINT = '/api/diag/audit-event';

async function send(dto: AuditEventDto): Promise<void> {
  if (process.env.NODE_ENV !== 'development') return;
  try {
    await fetch(ENDPOINT, {
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

## Uso en componentes / Server Actions

```typescript
import { llmAudit } from '@/lib/llm-audit';

// En un Server Action o Route Handler:
await llmAudit.logEvent('Dashboard', 'Cargando datos', `userId=${userId}`);

// Error handling:
catch (error) {
  await llmAudit.logError('Dashboard', 'Falló la carga', error instanceof Error ? error : undefined);
}
```

---

## Inicialización en startup (opcional)

En `app/providers.tsx` o en el layout root:

```typescript
'use client';
import { useEffect } from 'react';
import { llmAudit } from '@/lib/llm-audit';

export function Providers({ children }: { children: React.ReactNode }) {
  useEffect(() => {
    llmAudit.logEvent('NextApp', 'App montada', `url=${window.location.pathname}`);
  }, []);
  return <>{children}</>;
}
```
