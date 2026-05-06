import Constants from 'expo-constants';

// Inferir IP del host desde Expo hostUri (ej: "192.168.1.100:8081")
function getApiBase(): string {
  if (!__DEV__) return '';
  const override = process.env.EXPO_PUBLIC_LLM_AUDIT_URL;
  if (override) return override;
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
