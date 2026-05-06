'use client';

import { useAuth0 } from '@auth0/auth0-react';
import { useEffect, useState, useCallback } from 'react';
import { useRouter } from 'next/navigation';
import { getGrupos, type Grupo } from '../../lib/api';

export default function GruposPage() {
  const { isAuthenticated, isLoading, loginWithRedirect, logout, getAccessTokenSilently, user } = useAuth0();
  const router = useRouter();
  const [grupos, setGrupos] = useState<Grupo[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError]   = useState<string | null>(null);

  const load = useCallback(async () => {
    try {
      setError(null);
      const token = await getAccessTokenSilently();
      const data  = await getGrupos(token);
      setGrupos(data);
    } catch (e: unknown) {
      setError(e instanceof Error ? e.message : 'Error al cargar grupos');
    } finally {
      setLoading(false);
    }
  }, [getAccessTokenSilently]);

  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      loginWithRedirect();
    }
    if (!isLoading && isAuthenticated) {
      load();
    }
  }, [isAuthenticated, isLoading, loginWithRedirect, load]);

  if (isLoading || loading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div role="status" aria-label="Cargando grupos" className="w-8 h-8 border-4 border-blue-600 border-t-transparent rounded-full animate-spin" />
      </div>
    );
  }

  return (
    <div className="max-w-3xl mx-auto px-4 py-8">
      {/* Header */}
      <div className="flex items-center justify-between mb-8">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Mis Grupos</h1>
          {user?.name && <p className="text-sm text-gray-500 mt-1">{user.name}</p>}
        </div>
        <div className="flex items-center gap-3">
          <button
            type="button"
            onClick={() => router.push('/planeamiento')}
            className="text-sm border border-blue-300 text-blue-600 hover:bg-blue-50 font-medium px-4 py-2 rounded-lg transition-colors"
          >
            <span aria-hidden="true">📋</span> Planeamientos
          </button>
          <button
            type="button"
            onClick={() => logout({ logoutParams: { returnTo: window.location.origin } })}
            className="text-sm text-red-500 hover:text-red-700 transition-colors"
          >
            Cerrar sesión
          </button>
        </div>
      </div>

      {error !== null ? (
        <div role="alert" className="bg-red-50 border border-red-200 text-red-700 rounded-xl p-4 mb-6">{error}</div>
      ) : null}

      {grupos.length === 0 && !error ? (
        <p className="text-center text-gray-400 mt-16">No hay grupos activos.</p>
      ) : null}

      <div className="grid gap-4">
        {grupos.map((g) => (
          <div key={g.id} className="bg-white rounded-xl border border-gray-200 p-5 flex items-center justify-between shadow-sm">
            <div>
              <p className="font-semibold text-gray-900">{g.name}</p>
              <p className="text-sm text-gray-500 mt-0.5">{g.subject} · {g.level} · {g.schoolYear}</p>
            </div>
            <button
              type="button"
              onClick={() => router.push(`/qrs?groupId=${g.id}&groupName=${encodeURIComponent(g.name)}&level=${encodeURIComponent(g.level)}`)}
              className="text-sm bg-blue-50 hover:bg-blue-100 text-blue-700 font-medium px-4 py-2 rounded-lg transition-colors"
            >
              <span aria-hidden="true">🖨</span> Imprimir QRs
            </button>
          </div>
        ))}
      </div>
    </div>
  );
}
