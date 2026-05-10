'use client';

import { useAuth0 } from '@auth0/auth0-react';
import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';

export default function HomePage() {
  const { isAuthenticated, isLoading, loginWithRedirect, getAccessTokenSilently } = useAuth0();
  const router = useRouter();
  const [hangfireMsg, setHangfireMsg] = useState<string | null>(null);

  useEffect(() => {
    if (isLoading) return;
    const params = typeof window !== 'undefined'
      ? new URLSearchParams(window.location.search)
      : null;
    const isHangfireReturn = params?.get('hangfire_return') === '1';

    if (isHangfireReturn) {
      if (!isAuthenticated) {
        // Sin sesión → ir a Auth0 y volver aquí
        void loginWithRedirect({
          authorizationParams: {
            redirect_uri: window.location.origin + '/callback',
            audience:     'https://api.aulaia.mep.go.cr',
            scope:        'openid profile email',
          },
          appState: { returnTo: '/?hangfire_return=1' },
        });
        return;
      }
      // Ya autenticado: obtener token y crear cookie de Hangfire directamente
      setHangfireMsg('Accediendo al dashboard…');
      getAccessTokenSilently({ authorizationParams: { audience: 'https://api.aulaia.mep.go.cr' } })
        .then(token => fetch('/hangfire-session', {
          method: 'POST',
          headers: { Authorization: 'Bearer ' + token },
        }))
        .then(resp => {
          if (resp.ok) {
            window.location.replace('/hangfire');
          } else if (resp.status === 403) {
            setHangfireMsg('Acceso denegado: se requiere rol admin.');
          } else {
            setHangfireMsg('Error al verificar sesión (' + resp.status + ').');
          }
        })
        .catch(() => setHangfireMsg('Error de red al conectar con Hangfire.'));
      return;
    }

    if (isAuthenticated) {
      router.replace('/dashboard');
    }
  }, [isAuthenticated, isLoading, loginWithRedirect, getAccessTokenSilently, router]);

  if (hangfireMsg) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <p className="text-gray-400">{hangfireMsg}</p>
      </div>
    );
  }

  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="w-8 h-8 border-4 border-blue-600 border-t-transparent rounded-full animate-spin" />
      </div>
    );
  }

  return (
    <div className="min-h-screen flex flex-col items-center justify-center gap-8 px-4">
      <div className="text-center">
        <h1 className="text-4xl font-bold text-gray-900 mb-2">AulaIA</h1>
        <p className="text-gray-500 text-lg">Asistente pedagógico para docentes del MEP</p>
      </div>
      <button
        onClick={() => loginWithRedirect()}
        className="bg-blue-600 hover:bg-blue-700 text-white font-semibold px-8 py-3 rounded-xl text-base transition-colors"
      >
        Iniciar sesión con Auth0
      </button>
    </div>
  );
}
