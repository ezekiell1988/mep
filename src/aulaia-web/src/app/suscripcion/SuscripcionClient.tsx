'use client';

import { useAuth0 } from '@auth0/auth0-react';
import { useEffect, useState, useRef } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import {
  getSuscripcionEstado,
  activarTrial,
  solicitarPago,
  subirComprobante,
  type SuscripcionEstadoResponse,
  type PaymentRequestResponse,
  type SubscriptionPlan,
} from '../../lib/api';

const PLAN_LABELS: Record<string, string> = {
  Trial: 'Trial',
  Basic: 'Básico — $6/mes',
  Professional: 'Profesional — $15/mes',
  Institutional: 'Institucional — $100/mes',
};

export default function SuscripcionClient() {
  const { isAuthenticated, isLoading, loginWithRedirect, getAccessTokenSilently } = useAuth0();
  const router = useRouter();
  const searchParams = useSearchParams();
  const planParam = searchParams.get('plan') as SubscriptionPlan | null;

  const [estado, setEstado] = useState<SuscripcionEstadoResponse | null>(null);
  const [payment, setPayment] = useState<PaymentRequestResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [successMsg, setSuccessMsg] = useState<string | null>(null);
  const [selectedPlan, setSelectedPlan] = useState<SubscriptionPlan>(planParam ?? 'Professional');
  const fileRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    if (isLoading) return;
    if (!isAuthenticated) { void loginWithRedirect({ appState: { returnTo: '/suscripcion' } }); return; }

    let cancelled = false;
    void (async () => {
      try {
        const token = await getAccessTokenSilently();
        if (cancelled) return;
        const data = await getSuscripcionEstado(token);
        if (cancelled) return;
        setEstado(data);
      } catch (e: unknown) {
        if (!cancelled) setError(e instanceof Error ? e.message : 'Error al cargar estado');
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();
    return () => { cancelled = true; };
  }, [isAuthenticated, isLoading, loginWithRedirect, getAccessTokenSilently]);

  const handleActivarTrial = async () => {
    setSaving(true);
    setError(null);
    try {
      const token = await getAccessTokenSilently();
      const data = await activarTrial(token);
      setEstado(data);
      setSuccessMsg('¡Trial activado! Tienes 30 días gratis para explorar AulaIA.');
    } catch (e: unknown) {
      setError(e instanceof Error ? e.message : 'Error activando trial');
    } finally {
      setSaving(false);
    }
  };

  const handleSolicitarPago = async () => {
    setSaving(true);
    setError(null);
    try {
      const token = await getAccessTokenSilently();
      const data = await solicitarPago(token, selectedPlan);
      setPayment(data);
    } catch (e: unknown) {
      setError(e instanceof Error ? e.message : 'Error generando solicitud de pago');
    } finally {
      setSaving(false);
    }
  };

  const handleSubirComprobante = async (file: File) => {
    if (!payment) return;
    setSaving(true);
    setError(null);
    try {
      const token = await getAccessTokenSilently();
      await subirComprobante(token, payment.id, file);
      setSuccessMsg('¡Comprobante enviado! El admin revisará y activará tu suscripción en menos de 24 horas.');
      setPayment(null);
    } catch (e: unknown) {
      setError(e instanceof Error ? e.message : 'Error subiendo comprobante');
    } finally {
      setSaving(false);
    }
  };

  if (isLoading || loading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="w-8 h-8 border-4 border-blue-600 border-t-transparent rounded-full animate-spin" />
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <header className="bg-white border-b border-gray-200 px-4 py-4">
        <div className="max-w-2xl mx-auto flex items-center justify-between">
          <button onClick={() => router.push('/dashboard')} className="text-xl font-bold text-blue-600">
            AulaIA
          </button>
          <button onClick={() => router.back()} className="text-sm text-gray-500 hover:text-gray-700">
            ← Volver
          </button>
        </div>
      </header>

      <main className="max-w-2xl mx-auto px-4 py-10">
        <h1 className="text-2xl font-bold text-gray-900 mb-6">Mi suscripción</h1>

        {/* Estado actual */}
        {estado && (
          <div className="bg-white rounded-xl border border-gray-200 p-6 mb-6">
            <h2 className="font-semibold text-gray-700 mb-3">Estado actual</h2>
            {estado.hasSubscription ? (
              <div className="space-y-1 text-sm text-gray-700">
                <p>Plan: <strong>{estado.plan}</strong></p>
                <p>Estado: <strong className={estado.status === 'Active' ? 'text-green-600' : 'text-red-600'}>{estado.status === 'Active' ? 'Activo' : 'Expirado'}</strong></p>
                {estado.isTrial && <p className="text-amber-600 font-medium">Trial — {estado.daysRemaining} días restantes</p>}
                {estado.currentPeriodEnd && (
                  <p>Vence: {new Date(estado.currentPeriodEnd).toLocaleDateString('es-CR')}</p>
                )}
              </div>
            ) : (
              <p className="text-gray-500 text-sm">No tienes suscripción activa.</p>
            )}
          </div>
        )}

        {/* Alertas */}
        {error && (
          <div role="alert" className="mb-4 p-4 bg-red-50 border border-red-200 rounded-lg text-red-700 text-sm">
            {error}
          </div>
        )}
        {successMsg && (
          <div role="status" className="mb-4 p-4 bg-green-50 border border-green-200 rounded-lg text-green-700 text-sm">
            {successMsg}
          </div>
        )}

        {/* Activar trial (si no tiene suscripción) */}
        {!estado?.hasSubscription && !payment && !successMsg && (
          <div className="bg-white rounded-xl border border-gray-200 p-6 mb-6">
            <h2 className="font-semibold text-gray-900 mb-2">Comenzar gratis</h2>
            <p className="text-sm text-gray-600 mb-4">
              Activa tu <strong>trial gratuito de 30 días</strong> sin necesidad de pago.
              Incluye todas las funciones del plan Profesional.
            </p>
            <button
              onClick={() => void handleActivarTrial()}
              disabled={saving}
              className="w-full py-2.5 bg-green-600 text-white rounded-lg font-semibold text-sm hover:bg-green-700 disabled:opacity-50"
            >
              {saving ? 'Activando...' : 'Activar trial gratuito'}
            </button>
          </div>
        )}

        {/* Solicitar pago SINPE */}
        {!payment && !successMsg && (
          <div className="bg-white rounded-xl border border-gray-200 p-6 mb-6">
            <h2 className="font-semibold text-gray-900 mb-4">
              {estado?.hasSubscription ? 'Renovar o cambiar plan' : 'Suscribirse con SINPE Móvil'}
            </h2>

            <div className="mb-4">
              <label htmlFor="plan-select" className="block text-sm font-medium text-gray-700 mb-1">
                Plan
              </label>
              <select
                id="plan-select"
                value={selectedPlan}
                onChange={e => setSelectedPlan(e.target.value as SubscriptionPlan)}
                className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
              >
                <option value="Basic">Básico — $6/mes</option>
                <option value="Professional">Profesional — $15/mes</option>
                <option value="Institutional">Institucional — $100/mes</option>
              </select>
            </div>

            <button
              onClick={() => void handleSolicitarPago()}
              disabled={saving}
              className="w-full py-2.5 bg-blue-600 text-white rounded-lg font-semibold text-sm hover:bg-blue-700 disabled:opacity-50"
            >
              {saving ? 'Generando solicitud...' : 'Generar instrucciones de pago'}
            </button>
          </div>
        )}

        {/* Instrucciones de pago SINPE */}
        {payment && (
          <div className="bg-white rounded-xl border-2 border-blue-500 p-6 mb-6">
            <h2 className="font-bold text-gray-900 mb-4 text-lg">
              Instrucciones de pago — {PLAN_LABELS[payment.plan]}
            </h2>

            <ol className="list-decimal list-inside space-y-3 text-sm text-gray-700 mb-6">
              <li>
                Abrí <strong>SINPE Móvil</strong> en tu teléfono y elegí{' '}
                <em>Transferencia a número de teléfono</em>.
              </li>
              <li>
                Número destino:{' '}
                <strong className="text-blue-700 text-base">{payment.sinpePhone}</strong>
                {' '}({payment.sinpeAccountName})
              </li>
              <li>
                Monto a transferir:{' '}
                <strong className="text-base">
                  ₡{payment.amountCrc.toLocaleString('es-CR', { minimumFractionDigits: 0 })}
                </strong>
                {' '}(≈ ${payment.amountUsd}/mes)
              </li>
              <li>
                En el <strong>concepto o descripción</strong> escribí exactamente el código:{' '}
                <strong className="font-mono text-blue-700 bg-blue-50 px-2 py-0.5 rounded">
                  {payment.referenceCode}
                </strong>
              </li>
              <li>Hacé la transferencia y guardá el comprobante (captura de pantalla o PDF).</li>
              <li>Subí el comprobante aquí abajo (opcional pero acelera la aprobación).</li>
            </ol>

            {/* Upload comprobante */}
            <div className="border-2 border-dashed border-gray-300 rounded-lg p-4 text-center mb-4">
              <p className="text-sm text-gray-500 mb-2">Sube tu comprobante de SINPE (JPG, PNG, PDF — max 10 MB)</p>
              <input
                ref={fileRef}
                type="file"
                accept=".jpg,.jpeg,.png,.pdf,.webp"
                className="hidden"
                aria-label="Subir comprobante de pago"
                onChange={e => {
                  const file = e.target.files?.[0];
                  if (file) void handleSubirComprobante(file);
                }}
              />
              <button
                onClick={() => fileRef.current?.click()}
                disabled={saving}
                className="px-4 py-2 bg-white border border-gray-300 rounded-lg text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50"
              >
                {saving ? 'Subiendo...' : '📎 Seleccionar archivo'}
              </button>
            </div>

            <button
              onClick={() => {
                setPayment(null);
                setSuccessMsg('Tu solicitud quedó registrada. El admin la revisará en menos de 24 horas.');
              }}
              className="w-full py-2.5 bg-gray-900 text-white rounded-lg font-semibold text-sm hover:bg-gray-800"
            >
              Ya pagué — avisar al admin
            </button>
          </div>
        )}

        {/* Enlace a precios */}
        <p className="text-center text-sm text-gray-500 mt-4">
          ¿Quieres ver los planes?{' '}
          <button onClick={() => router.push('/precios')} className="text-blue-600 underline">
            Ver precios
          </button>
        </p>
      </main>
    </div>
  );
}
