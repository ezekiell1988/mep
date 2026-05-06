'use client';

import { useAuth0 } from '@auth0/auth0-react';
import { useRouter } from 'next/navigation';
import { useEffect } from 'react';

export default function CallbackPage() {
  const { isLoading, error, isAuthenticated, handleRedirectCallback } = useAuth0();
  const router = useRouter();

  useEffect(() => {
    if (typeof window === 'undefined') return;

    const params = new URLSearchParams(window.location.search);
    if (params.has('code') && params.has('state')) {
      handleRedirectCallback().then(() => {
        router.replace('/');
      });
    }
  }, [handleRedirectCallback, router]);

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
