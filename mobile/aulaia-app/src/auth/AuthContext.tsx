import Auth0 from 'react-native-auth0';
import * as SecureStore from 'expo-secure-store';
import React, { createContext, useCallback, useContext, useEffect, useState } from 'react';

const auth0 = new Auth0({
  domain: 'aulaia-mep.us.auth0.com',
  clientId: 'jgL8tnZaoHAeFtNWo8EqZ2i1XRwduZzr', // Mobile client
});

const AUDIENCE   = 'https://api.aulaia.mep.go.cr';
const SCOPE      = 'openid profile email offline_access';
const TOKEN_KEY  = 'aulaia_access_token';
const REFRESH_KEY = 'aulaia_refresh_token';

interface AuthState {
  accessToken: string | null;
  isLoading: boolean;
  isAuthenticated: boolean;
}

interface AuthContextValue extends AuthState {
  login: () => Promise<void>;
  logout: () => Promise<void>;
}

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [state, setState] = useState<AuthState>({
    accessToken: null,
    isLoading: true,
    isAuthenticated: false,
  });

  // Restaurar token guardado al arrancar
  useEffect(() => {
    (async () => {
      try {
        const token = await SecureStore.getItemAsync(TOKEN_KEY);
        if (token) {
          setState({ accessToken: token, isLoading: false, isAuthenticated: true });
        } else {
          setState(s => ({ ...s, isLoading: false }));
        }
      } catch {
        setState(s => ({ ...s, isLoading: false }));
      }
    })();
  }, []);

  const login = useCallback(async () => {
    const result = await auth0.webAuth.authorize({
      scope: SCOPE,
      audience: AUDIENCE,
    });
    await SecureStore.setItemAsync(TOKEN_KEY, result.accessToken);
    if (result.refreshToken) {
      await SecureStore.setItemAsync(REFRESH_KEY, result.refreshToken);
    }
    setState({ accessToken: result.accessToken, isLoading: false, isAuthenticated: true });
  }, []);

  const logout = useCallback(async () => {
    await auth0.webAuth.clearSession();
    await SecureStore.deleteItemAsync(TOKEN_KEY);
    await SecureStore.deleteItemAsync(REFRESH_KEY);
    setState({ accessToken: null, isLoading: false, isAuthenticated: false });
  }, []);

  return (
    <AuthContext.Provider value={{ ...state, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth debe usarse dentro de AuthProvider');
  return ctx;
}
