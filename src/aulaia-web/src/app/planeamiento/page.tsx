'use client';

import { useAuth0 } from '@auth0/auth0-react';
import { useEffect, useState, useCallback } from 'react';
import { useRouter } from 'next/navigation';
import { listPlaneamientos, type PlaneamientoListItem } from '../../lib/api';

const NIVEL_LABEL: Record<number, string> = {
  7: '7°', 8: '8°', 9: '9°', 10: '10°', 11: '11°', 12: '12°',
  1: '1°', 2: '2°', 3: '3°', 4: '4°', 5: '5°', 6: '6°',
};

const STATUS_BADGE: Record<string, { label: string; classes: string }> = {
  Pending:    { label: 'Pendiente',  classes: 'bg-yellow-50 text-yellow-700 border-yellow-200' },
  Generating: { label: 'Generando…', classes: 'bg-blue-50 text-blue-700 border-blue-200' },
  Ready:      { label: 'Listo',      classes: 'bg-green-50 text-green-700 border-green-200' },
  Failed:     { label: 'Error',      classes: 'bg-red-50 text-red-700 border-red-200' },
};

export default function PlaneamientoListPage() {
  const { isAuthenticated, isLoading, loginWithRedirect, getAccessTokenSilently } = useAuth0();
  const router = useRouter();
  const [items, setItems] = useState<PlaneamientoListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    try {
      setError(null);
      const token = await getAccessTokenSilently();
      setItems(await listPlaneamientos(token));
    } catch (e: unknown) {
      setError(e instanceof Error ? e.message : 'Error al cargar planeamientos');
    } finally {
      setLoading(false);
    }
  }, [getAccessTokenSilently]);

  useEffect(() => {
    if (!isLoading && !isAuthenticated) loginWithRedirect();
    if (!isLoading && isAuthenticated) load();
  }, [isAuthenticated, isLoading, loginWithRedirect, load]);

  if (isLoading || loading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div role="status" aria-label="Cargando planeamientos" className="w-8 h-8 border-4 border-blue-600 border-t-transparent rounded-full animate-spin" />
      </div>
    );
  }

  return (
    <div className="max-w-3xl mx-auto px-4 py-8">
      {/* Header */}
      <div className="flex items-center justify-between mb-8">
        <div>
          <button type="button" onClick={() => router.push('/grupos')} className="text-sm text-gray-400 hover:text-gray-600 mb-1 flex items-center gap-1" aria-label="Volver a Mis Grupos">
            <span aria-hidden="true">←</span> Grupos
          </button>
          <h1 className="text-2xl font-bold text-gray-900">Planeamientos</h1>
        </div>
        <button
          type="button"
          onClick={() => router.push('/planeamiento/nuevo')}
          className="bg-blue-600 hover:bg-blue-700 text-white font-semibold px-5 py-2.5 rounded-xl text-sm transition-colors"
        >
          + Nuevo planeamiento
        </button>
      </div>

      {error !== null ? (
        <div role="alert" className="bg-red-50 border border-red-200 text-red-700 rounded-xl p-4 mb-6">{error}</div>
      ) : null}

      {items.length === 0 && !error ? (
        <div className="text-center text-gray-400 mt-16">
          <p className="text-lg mb-2">Aún no hay planeamientos.</p>
          <p className="text-sm">Crea el primero con el botón de arriba.</p>
        </div>
      ) : null}

      <div className="grid gap-3">
        {items.map((item) => {
          const badge = STATUS_BADGE[item.status] ?? STATUS_BADGE['Pending'];
          return (
            <button
              type="button"
              key={item.id}
              onClick={() => router.push(`/planeamiento/detalle?id=${item.id}`)}
              aria-label={`Ver planeamiento de ${item.asignatura} ${NIVEL_LABEL[item.nivel] ?? `${item.nivel}°`} Trimestre ${item.trimestre} — ${STATUS_BADGE[item.status]?.label ?? item.status}`}
              className="bg-white rounded-xl border border-gray-200 p-5 flex items-center justify-between shadow-sm hover:border-blue-300 transition-colors text-left w-full"
            >
              <div>
                <p className="font-semibold text-gray-900">
                  {item.asignatura} — {NIVEL_LABEL[item.nivel] ?? `${item.nivel}°`} · Trimestre {item.trimestre}
                </p>
                <p className="text-sm text-gray-400 mt-0.5">
                  {new Date(item.createdAt).toLocaleDateString('es-CR', { day: 'numeric', month: 'long', year: 'numeric' })}
                </p>
              </div>
              <span className={`text-xs font-medium px-3 py-1 rounded-full border ${badge.classes}`}>
                {badge.label}
              </span>
            </button>
          );
        })}
      </div>
    </div>
  );
}
