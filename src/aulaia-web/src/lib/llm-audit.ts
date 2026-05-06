// Proxy Next.js en dev: /api/:path* → http://localhost:8000/api/:path*
// El endpoint /api/diag/audit-event queda cubierto automáticamente.
// En producción NODE_ENV !== 'development', el servicio es no-op.

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

const BASE_URL = process.env.NEXT_PUBLIC_LLM_AUDIT_URL ?? '';

async function send(dto: AuditEventDto): Promise<void> {
  if (process.env.NODE_ENV !== 'development') return;
  try {
    await fetch(`${BASE_URL}/api/diag/audit-event`, {
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
