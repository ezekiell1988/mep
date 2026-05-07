'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import { useAuth0 } from '@auth0/auth0-react';
import { getSuscripcionInfo, type PlanInfo, type SuscripcionInfoResponse } from '../../lib/api';

const PLAN_FEATURES: Record<string, string[]> = {
  Basic: [
    'Planeamiento didáctico con IA',
    'Asistencia con QR',
    'Registro de notas',
    'Hasta 5 grupos activos',
  ],
  Professional: [
    'Todo lo del plan Básico',
    'Adecuaciones curriculares (Ley 7600)',
    'Reportes exportables (PDF/XLSX)',
    'Informe para Dirección',
    'Grupos ilimitados',
  ],
  Institutional: [
    'Todo lo del plan Profesional',
    'Todos los docentes de la institución',
    'Panel del director',
    'Reportes institucionales',
    'Soporte prioritario',
  ],
};

const PLAN_COLORS: Record<string, string> = {
  Basic: 'border-gray-200',
  Professional: 'border-blue-500 ring-2 ring-blue-500',
  Institutional: 'border-purple-400',
};

export default function PreciosClient() {
  const { loginWithRedirect } = useAuth0();
  const router = useRouter();
  const [info, setInfo] = useState<SuscripcionInfoResponse | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    getSuscripcionInfo()
      .then(setInfo)
      .catch(e => setError(e instanceof Error ? e.message : 'Error al cargar precios'));
  }, []);

  const handleSelectPlan = (plan: PlanInfo) => {
    void loginWithRedirect({
      appState: { returnTo: `/suscripcion?plan=${plan.id}` },
    });
  };

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <header className="bg-white border-b border-gray-200 px-4 py-4">
        <div className="max-w-5xl mx-auto flex items-center justify-between">
          <button onClick={() => router.push('/')} className="text-xl font-bold text-blue-600">
            AulaIA
          </button>
          <button
            onClick={() => void loginWithRedirect()}
            className="text-sm text-blue-600 underline"
          >
            Iniciar sesión
          </button>
        </div>
      </header>

      <main className="max-w-5xl mx-auto px-4 py-12">
        {/* Hero */}
        <div className="text-center mb-12">
          <h1 className="text-4xl font-bold text-gray-900 mb-4">
            Planes y precios
          </h1>
          <p className="text-lg text-gray-600">
            Comienza con{' '}
            <strong>{info?.trialDays ?? 30} días gratis</strong>
            {' '}sin tarjeta de crédito. Paga con SINPE Móvil cuando estés listo.
          </p>
          {info && (
            <p className="text-sm text-gray-400 mt-2">
              Tipo de cambio: ₡{info.exchangeRate.toLocaleString('es-CR', { minimumFractionDigits: 0 })} por $1 USD
              {' '}(BCCR, actualizado diariamente)
            </p>
          )}
        </div>

        {error && (
          <div role="alert" className="mb-8 p-4 bg-red-50 border border-red-200 rounded-lg text-red-700 text-center">
            {error}
          </div>
        )}

        {/* Plan cards */}
        {info ? (
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            {info.plans.map(plan => (
              <article
                key={plan.id}
                className={`bg-white rounded-2xl border-2 p-6 flex flex-col ${PLAN_COLORS[plan.id] ?? 'border-gray-200'}`}
              >
                {plan.id === 'Professional' && (
                  <div className="text-center mb-3">
                    <span className="bg-blue-600 text-white text-xs font-semibold px-3 py-1 rounded-full">
                      Más popular
                    </span>
                  </div>
                )}
                <h2 className="text-xl font-bold text-gray-900 mb-1">{plan.nombre}</h2>
                <p className="text-sm text-gray-500 mb-4">{plan.descripcion}</p>

                <div className="mb-4">
                  <span className="text-3xl font-extrabold text-gray-900">
                    ${plan.precioUsd.toFixed(0)}
                  </span>
                  <span className="text-gray-500 text-sm">/mes</span>
                  <p className="text-sm text-gray-500 mt-1">
                    ≈ ₡{plan.precioCrc.toLocaleString('es-CR', { minimumFractionDigits: 0 })}
                  </p>
                </div>

                <ul className="flex-1 space-y-2 mb-6" aria-label={`Características del plan ${plan.nombre}`}>
                  {(PLAN_FEATURES[plan.id] ?? []).map(feature => (
                    <li key={feature} className="flex items-start gap-2 text-sm text-gray-700">
                      <span aria-hidden="true" className="text-green-500 font-bold">✓</span>
                      {feature}
                    </li>
                  ))}
                </ul>

                <button
                  onClick={() => handleSelectPlan(plan)}
                  className={`w-full py-2 rounded-lg font-semibold text-sm transition-colors ${
                    plan.id === 'Professional'
                      ? 'bg-blue-600 text-white hover:bg-blue-700'
                      : 'border border-gray-300 text-gray-700 hover:bg-gray-50'
                  }`}
                >
                  Comenzar gratis
                </button>
              </article>
            ))}
          </div>
        ) : !error ? (
          <div className="flex justify-center py-12">
            <div className="w-8 h-8 border-4 border-blue-600 border-t-transparent rounded-full animate-spin" />
          </div>
        ) : null}

        {/* SINPE info */}
        {info && (
          <div className="mt-12 bg-blue-50 border border-blue-200 rounded-xl p-6 text-center">
            <h3 className="font-semibold text-blue-900 mb-2">¿Cómo funciona el pago?</h3>
            <p className="text-blue-800 text-sm">
              Pagamos con <strong>SINPE Móvil</strong> — el sistema de pagos de Costa Rica.
              Transferís al número <strong>{info.sinpePhone}</strong> ({info.sinpeAccountName}),
              subís el comprobante y activamos tu plan en menos de 24 horas.
              Sin tarjeta de crédito, sin comisiones.
            </p>
          </div>
        )}
      </main>
    </div>
  );
}
