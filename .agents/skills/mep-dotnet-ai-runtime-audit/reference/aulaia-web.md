# Referencia: mep-dotnet-ai-runtime-audit en aulaia-web (Next.js)

Cadena completa: **aulaia-web** (Next.js dev :3000) → rewrites Next.js → **AulaIA.Api** (:8000) → `logs/llm-audit.md`

---

## Arquitectura

```
Browser (next dev :3000)
  └─ LlmAuditService.send()
       POST /api/diag/audit-event   ← path relativa, Next.js intercepta
         │
         └─ next.config.ts rewrites
              /api/:path* → http://localhost:8000/:path*
                │
                └─ AulaIA.Api POST /diag/audit-event
                     └─ ILlmAuditService → logs/llm-audit.md
```

Sin CORS: el rewrite convierte la llamada en same-origin desde la perspectiva del browser.

---

## Archivos implicados

### aulaia-web (Next.js)

| Archivo | Rol |
|---|---|
| `src/lib/llm-audit.ts` | **CREAR** — Service TypeScript para loguear eventos |
| `src/app/providers.tsx` | Existente — inicializar `LlmAuditService` en startup si `__DEV__` |

### next.config.ts (ya configurado)

El rewrite `/api/:path* → http://localhost:8000/:path*` ya existe en `next.config.ts` para el modo dev. El endpoint `/diag/audit-event` queda cubierto automáticamente.

---

## LlmAuditService — src/lib/llm-audit.ts

```typescript
const BASE_URL = process.env.NEXT_PUBLIC_LLM_AUDIT_URL ?? '/api';

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
  // Solo en desarrollo (Next.js expone process.env.NODE_ENV)
  if (process.env.NODE_ENV !== 'development') return;
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

## Uso en componentes / páginas Next.js

```typescript
// Página o componente
import { llmAudit } from '@/lib/llm-audit';

export default function GruposPage() {
  useEffect(() => {
    llmAudit.logEvent('GruposPage', 'Montar página', '✅ OK');
  }, []);

  const handleError = (err: unknown) => {
    llmAudit.logError('GruposPage', 'Error cargando grupos', err);
  };

  // ...
}
```

---

## Cómo funciona el routing

```
POST /api/diag/audit-event
  → next.config.ts rewrite en PHASE_DEVELOPMENT_SERVER
  → http://localhost:8000/diag/audit-event
  → ILlmAuditService.LogEvent / LogDecision / LogError
  → logs/llm-audit.md ✅
```

> El rewrite solo aplica en `next dev`. En producción (SPA estático en `wwwroot/`), `NODE_ENV !== 'development'` y el servicio es no-op.

---

## Variable de entorno (opcional)

Para usar una URL distinta del proxy por defecto:

```env
# .env.local (no committear)
NEXT_PUBLIC_LLM_AUDIT_URL=http://localhost:8000
```

Sin esta variable, usa `/api` (proxy de Next.js).

---

## Comandos LLM para leer logs

```bash
# Ver audit log (Next.js + API juntos)
curl http://localhost:8000/diag/audit

# Limpiar para nueva sesión
curl -X DELETE http://localhost:8000/diag/audit

# Ver contexto de la app
curl http://localhost:8000/diag/context | jq
```

> Solo funcionan con `AulaIA.Api` corriendo en `:8000` en mode Development.

---

## Requisitos para que funcione

1. `AulaIA.Api` corriendo en `http://localhost:8000` con `LlmAudit.Enabled = true`
2. `next dev` corriendo (usa `PHASE_DEVELOPMENT_SERVER` con los rewrites)
3. `src/lib/llm-audit.ts` creado
4. Llamadas a `llmAudit.logEvent/logDecision/logError` en los componentes relevantes

---

## Activación paso a paso

```bash
# 1. Iniciar AulaIA.Api
cd src/AulaIA.Api && dotnet run --launch-profile http

# 2. Iniciar Next.js dev
cd src/aulaia-web && npm run dev

# 3. Verificar que el log recibe eventos
curl http://localhost:8000/diag/audit
```
