'use client';

import { Auth0Context, Auth0Provider, type Auth0ContextInterface } from '@auth0/auth0-react';

const e2eAuth = {
  isAuthenticated: true,
  isLoading: false,
  user: {
    sub: 'auth0|e2e-docente',
    name: 'Docente E2E',
    email: 'docente-e2e@aulaia.test',
  },
  getAccessTokenSilently: async () => 'e2e-token',
  getAccessTokenWithPopup: async () => 'e2e-token',
  getIdTokenClaims: async () => undefined,
  loginWithRedirect: async () => undefined,
  loginWithPopup: async () => undefined,
  logout: async () => undefined,
  handleRedirectCallback: async () => ({ appState: undefined }),
} as unknown as Auth0ContextInterface;

// La app web usa Auth0 PKCE browser-side (SPA).
// No hay servidor Next.js en producción (output: 'export') → se usa @auth0/auth0-react.
export default function Providers({ children }: { children: React.ReactNode }) {
  if (process.env.NEXT_PUBLIC_E2E_AUTH_BYPASS === '1') {
    return <Auth0Context.Provider value={e2eAuth}>{children}</Auth0Context.Provider>;
  }

  const redirectUri = typeof window !== 'undefined' ? `${window.location.origin}/callback` : '';

  return (
    <Auth0Provider
      domain="aulaia-mep.us.auth0.com"
      clientId={process.env.NEXT_PUBLIC_AUTH0_CLIENT_ID ?? ''}
      authorizationParams={{
        redirect_uri: redirectUri,
        audience:     'https://api.aulaia.mep.go.cr',
        scope:        'openid profile email',
      }}
      cacheLocation="localstorage"
    >
      {children}
    </Auth0Provider>
  );
}
