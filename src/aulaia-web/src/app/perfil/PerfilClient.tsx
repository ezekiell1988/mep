'use client';

import { useAuth0 } from '@auth0/auth0-react';
import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import {
  getSuscripcionEstado,
  getMiCodigoReferido,
  getReferralPanel,
  getComisiones,
  type SuscripcionEstadoResponse,
  type ReferralCodeResponse,
  type ReferralPanelResponse,
  type ComisionResponse,
} from '../../lib/api';

export default function PerfilClient() {
  const { isAuthenticated, isLoading, loginWithRedirect, getAccessTokenSilently, user } = useAuth0();
  const router = useRouter();

  const [suscripcion, setSuscripcion] = useState<SuscripcionEstadoResponse | null>(null);
  const [codigoReferido, setCodigoReferido] = useState<ReferralCodeResponse | null>(null);
  const [panel, setPanel] = useState<ReferralPanelResponse | null>(null);
  const [comisiones, setComisiones] = useState<ComisionResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [copied, setCopied] = useState(false);

  useEffect(() => {
    if (isLoading) return;
    if (!isAuthenticated) { void loginWithRedirect(); return; }

    let cancelled = false;
    void (async () => {
      try {
        const token = await getAccessTokenSilently();
        if (cancelled) return;
        const [sus, cod, pan, com] = await Promise.allSettled([
          getSuscripcionEstado(token),
          getMiCodigoReferido(token),
          getReferralPanel(token),
          getComisiones(token),
        ]);
        if (cancelled) return;
        if (sus.status === 'fulfilled') setSuscripcion(sus.value);
        if (cod.status === 'fulfilled') setCodigoReferido(cod.value);
        if (pan.status === 'fulfilled') setPanel(pan.value);
        if (com.status === 'fulfilled') setComisiones(com.value);
      } catch (e: unknown) {
        if (!cancelled) setError(e instanceof Error ? e.message : 'Error al cargar perfil');
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();
    return () => { cancelled = true; };
  }, [isAuthenticated, isLoading, loginWithRedirect, getAccessTokenSilently]);

  const referralLink = codigoReferido
    ? `${typeof window !== 'undefined' ? window.location.origin : 'https://aulaia.cr'}/registro?ref=${codigoReferido.code}`
    : '';

  const handleCopy = async () => {
    try {
      await navigator.clipboard.writeText(referralLink);
      setCopied(true);
      setTimeout(() => setCopied(false), 2500);
    } catch {
      // ignore
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
        <div className="max-w-3xl mx-auto flex items-center justify-between">
          <button onClick={() => router.push('/dashboard')} className="text-xl font-bold text-blue-600">
            AulaIA
          </button>
          <button onClick={() => router.back()} className="text-sm text-gray-500 hover:text-gray-700">
            ← Volver
          </button>
        </div>
      </header>

      <main className="max-w-3xl mx-auto px-4 py-10 space-y-6">
        <h1 className="text-2xl font-bold text-gray-900">Mi perfil</h1>

        {error && (
          <div role="alert" className="p-4 bg-red-50 border border-red-200 rounded-lg text-red-700 text-sm">
            {error}
          </div>
        )}

        {/* Datos del usuario */}
        <section aria-labelledby="datos-heading" className="bg-white rounded-xl border border-gray-200 p-6">
          <h2 id="datos-heading" className="font-semibold text-gray-900 mb-3">Información de la cuenta</h2>
          {user && (
            <div className="flex items-center gap-4">
              {user.picture && (
                // eslint-disable-next-line @next/next/no-img-element
                <img src={user.picture} alt="Avatar" className="w-14 h-14 rounded-full" />
              )}
              <div>
                <p className="font-semibold text-gray-900">{user.name}</p>
                <p className="text-sm text-gray-500">{user.email}</p>
              </div>
            </div>
          )}
        </section>

        {/* Suscripción */}
        <section aria-labelledby="sus-heading" className="bg-white rounded-xl border border-gray-200 p-6">
          <div className="flex items-center justify-between mb-3">
            <h2 id="sus-heading" className="font-semibold text-gray-900">Suscripción</h2>
            <button
              onClick={() => router.push('/suscripcion')}
              className="text-sm text-blue-600 hover:underline"
            >
              Gestionar →
            </button>
          </div>
          {suscripcion?.hasSubscription ? (
            <div className="space-y-1 text-sm text-gray-700">
              <p>Plan: <strong>{suscripcion.plan}</strong></p>
              <p>
                Estado:{' '}
                <strong className={suscripcion.status === 'Active' ? 'text-green-600' : 'text-red-600'}>
                  {suscripcion.status === 'Active' ? 'Activo' : 'Expirado'}
                </strong>
              </p>
              {suscripcion.isTrial && (
                <p className="text-amber-600 font-medium">
                  Trial — quedan {suscripcion.daysRemaining} días
                </p>
              )}
              {suscripcion.currentPeriodEnd && (
                <p className="text-gray-500">
                  Vence: {new Date(suscripcion.currentPeriodEnd).toLocaleDateString('es-CR')}
                </p>
              )}
            </div>
          ) : (
            <div className="text-sm text-gray-500">
              No tienes suscripción activa.{' '}
              <button
                onClick={() => router.push('/suscripcion')}
                className="text-blue-600 underline"
              >
                Suscribirte
              </button>
            </div>
          )}
        </section>

        {/* Código de referido */}
        <section aria-labelledby="ref-heading" className="bg-white rounded-xl border border-gray-200 p-6">
          <h2 id="ref-heading" className="font-semibold text-gray-900 mb-3">Código de referido</h2>
          {codigoReferido ? (
            <div className="space-y-3">
              <p className="text-sm text-gray-600">
                Compartí tu código y ganá <strong>20% de comisión</strong> sobre los ingresos netos
                de cada usuario que suscribas (durante 12 meses).
              </p>
              <div className="flex items-center gap-2">
                <code className="flex-1 bg-gray-100 px-3 py-2 rounded-lg font-mono text-sm break-all">
                  {referralLink}
                </code>
                <button
                  onClick={() => void handleCopy()}
                  className="px-3 py-2 border border-gray-300 rounded-lg text-sm font-medium text-gray-700 hover:bg-gray-50 shrink-0"
                  aria-label="Copiar enlace de referido"
                >
                  {copied ? '✓ Copiado' : 'Copiar'}
                </button>
              </div>
              <p className="text-xs text-gray-400">
                Código: <strong>{codigoReferido.code}</strong>
                {!codigoReferido.isActive && (
                  <span className="ml-2 text-red-500">(inactivo)</span>
                )}
              </p>
            </div>
          ) : (
            <p className="text-sm text-gray-500">No se pudo cargar el código de referido.</p>
          )}
        </section>

        {/* Panel de referidos */}
        {panel && (
          <section aria-labelledby="panel-heading" className="bg-white rounded-xl border border-gray-200 p-6">
            <h2 id="panel-heading" className="font-semibold text-gray-900 mb-4">Usuarios referidos</h2>
            <div className="flex gap-6 mb-4 text-sm">
              <div>
                <p className="text-gray-500">Total referidos</p>
                <p className="text-2xl font-bold text-gray-900">{panel.totalReferidos}</p>
              </div>
              <div>
                <p className="text-gray-500">Comisiones acumuladas</p>
                <p className="text-2xl font-bold text-green-600">
                  ₡{panel.totalComisionesCrc.toLocaleString('es-CR', { minimumFractionDigits: 0 })}
                </p>
              </div>
            </div>

            {panel.referidos.length > 0 && (
              <div className="overflow-x-auto">
                <table className="w-full text-sm text-left">
                  <thead>
                    <tr className="border-b border-gray-200 text-gray-500">
                      <th scope="col" className="py-2 pr-4">Nombre</th>
                      <th scope="col" className="py-2 pr-4">Plan</th>
                      <th scope="col" className="py-2">Desde</th>
                    </tr>
                  </thead>
                  <tbody>
                    {panel.referidos.map((r, i) => (
                      <tr key={r.email ?? i} className="border-b border-gray-100">
                        <td className="py-2 pr-4">{r.nombre}</td>
                        <td className="py-2 pr-4">{r.plan}</td>
                        <td className="py-2 text-gray-400">
                          {new Date(r.registrado).toLocaleDateString('es-CR')}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </section>
        )}

        {/* Historial de comisiones */}
        {comisiones.length > 0 && (
          <section aria-labelledby="com-heading" className="bg-white rounded-xl border border-gray-200 p-6">
            <h2 id="com-heading" className="font-semibold text-gray-900 mb-4">Historial de comisiones</h2>
            <div className="overflow-x-auto">
              <table className="w-full text-sm text-left">
                <thead>
                  <tr className="border-b border-gray-200 text-gray-500">
                    <th scope="col" className="py-2 pr-4">Período</th>
                    <th scope="col" className="py-2 pr-4">Referido</th>
                    <th scope="col" className="py-2 pr-4">Comisión (CRC)</th>
                    <th scope="col" className="py-2">Estado</th>
                  </tr>
                </thead>
                <tbody>
                  {comisiones.map(c => (
                    <tr key={c.id} className="border-b border-gray-100">
                      <td className="py-2 pr-4 font-mono">{String(c.month)}</td>
                      <td className="py-2 pr-4">{c.referidoNombre}</td>
                      <td className="py-2 pr-4 font-semibold">
                        ₡{c.commissionAmountCrc.toLocaleString('es-CR', { minimumFractionDigits: 0 })}
                      </td>
                      <td className="py-2">
                        <span className={c.status === 'Paid' ? 'text-green-600' : 'text-amber-600'}>
                          {c.status === 'Paid' ? 'Pagada' : 'Pendiente'}
                        </span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </section>
        )}
      </main>
    </div>
  );
}
