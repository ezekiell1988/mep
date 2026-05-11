'use client';

import { useAuth0 } from '@auth0/auth0-react';
import { useRouter } from 'next/navigation';
import { useEffect } from 'react';
import { ensureUserProfile } from '@/lib/api';

export default function CallbackPage() {
  const { isLoading, error, isAuthenticated, handleRedirectCallback, getAccessTokenSilently } = useAuth0();
  const router = useRouter();

  useEffect(() => {
    if (typeof window === 'undefined') return;

    const params = new URLSearchParams(window.location.search);
    if (params.has('code') && params.has('state')) {
      handleRedirectCallback().then(async (result) => {
        // Provisionar el perfil en la BD (idempotente; crea el user si es su primer login)
        try {
          const token = await getAccessTokenSilently();
          await ensureUserProfile(token);
        } catch {
          // No bloqueamos el flujo de login si el provisioning falla;
          // los endpoints de la app mostrarán error 401 y el usuario puede reintentar.
        }
        const returnTo = (result?.appState as { returnTo?: string } | undefined)?.returnTo ?? '/';
        router.replace(returnTo);
      });
    }
  }, [handleRedirectCallback, getAccessTokenSilently, router]);

  if (error) {
    return (
      <div className="flex min-h-full items-center justify-center">
        <p className="text-red-600">Error de autenticación: {error.message}</p>
      </div>
    );
  }

  if (!isLoading && isAuthenticated) {
    router.replace('/');
    return null;
  }

  return (
    <div className="flex min-h-full items-center justify-center">
      <p className="text-gray-500">Iniciando sesión…</p>
    </div>
  );
}

