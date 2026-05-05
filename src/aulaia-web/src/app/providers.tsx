'use client';

import { Auth0Provider } from '@auth0/auth0-react';

// La app web usa Auth0 PKCE browser-side (SPA).
// No hay servidor Next.js en producción (output: 'export') → se usa @auth0/auth0-react.
export default function Providers({ children }: { children: React.ReactNode }) {
  const origin = typeof window !== 'undefined' ? window.location.origin : '';

  return (
    <Auth0Provider
      domain="aulaia-mep.us.auth0.com"
      clientId={process.env.NEXT_PUBLIC_AUTH0_CLIENT_ID ?? ''}
      authorizationParams={{
        redirect_uri: origin,
        audience:     'https://api.aulaia.mep.go.cr',
        scope:        'openid profile email',
      }}
      cacheLocation="localstorage"
    >
      {children}
    </Auth0Provider>
  );
}
