import React, { createContext, useContext, useEffect, useRef, useState } from 'react';
import { PowerSyncDatabase } from '@powersync/react-native';
import type { AbstractPowerSyncDatabase, PowerSyncBackendConnector, CrudBatch } from '@powersync/common';
import { AppSchema } from './schema';
import { useAuth } from '../auth/AuthContext';

const POWERSYNC_URL = 'https://69f98b0463989ab5d2ed2a3b.powersync.journeyapps.com';
const API_BASE      = 'https://mep.ezekl.com';

// ── Connector ────────────────────────────────────────────────────────────────

function makeConnector(getToken: () => string | null): PowerSyncBackendConnector {
  return {
    /**
     * PowerSync llama a esto cada vez que necesita un token fresco.
     * Devuelve el JWT de nuestro backend (no el token de Auth0 directamente).
     */
    async fetchCredentials() {
      const token = getToken();
      if (!token) throw new Error('Sin token de autenticación.');

      const res = await fetch(`${API_BASE}/api/powersync/token`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      if (!res.ok) throw new Error(`Token PowerSync: ${res.status}`);

      const body = await res.json();
      return {
        token:     body.token as string,
        expiresAt: new Date(body.expiresAt as string),
        endpoint:  POWERSYNC_URL,
      };
    },

    /**
     * PowerSync llama a esto cuando hay mutaciones pendientes (creadas offline).
     * Solo manejamos `attendance_records`; grupos y estudiantes son read-only en el móvil.
     */
    async uploadData(database: AbstractPowerSyncDatabase) {
      const transaction = await database.getNextCrudTransaction();
      if (!transaction) return;

      const token = getToken();
      if (!token) throw new Error('Sin token para subir datos.');

      try {
        const batch = transaction.crud.map((entry) => ({
          op:   entry.op.toUpperCase(),          // 'PUT' | 'PATCH' | 'DELETE'
          type: entry.table,                      // 'attendance_records'
          id:   entry.id,
          data: entry.opData ?? undefined,
        }));

        const res = await fetch(`${API_BASE}/api/powersync/crud`, {
          method:  'PUT',
          headers: {
            'Content-Type': 'application/json',
            Authorization:  `Bearer ${token}`,
          },
          body: JSON.stringify({ batch }),
        });

        if (!res.ok) {
          const text = await res.text().catch(() => '');
          throw new Error(`CRUD upload ${res.status}: ${text}`);
        }

        await transaction.complete();
      } catch (e) {
        console.error('[PowerSync] uploadData falló:', e);
        // No completamos la transacción → PowerSync reintentará cuando haya conexión.
        throw e;
      }
    },
  };
}

// ── Contexto ─────────────────────────────────────────────────────────────────

interface PowerSyncContextValue {
  db: PowerSyncDatabase;
}

const PowerSyncCtx = createContext<PowerSyncContextValue | null>(null);

export function PowerSyncProvider({ children }: { children: React.ReactNode }) {
  const { accessToken } = useAuth();
  const tokenRef  = useRef(accessToken);
  tokenRef.current = accessToken;

  const dbRef = useRef<PowerSyncDatabase | null>(null);
  const [ready, setReady] = useState(false);

  useEffect(() => {
    const db = new PowerSyncDatabase({
      schema:  AppSchema,
      database: { dbFilename: 'aulaia.db' },
    });

    dbRef.current = db;

    const connector = makeConnector(() => tokenRef.current);
    db.init();
    if (accessToken) {
      db.connect(connector).catch((e) => console.error('[PowerSync] connect error', e));
    }

    setReady(true);

    return () => {
      db.disconnect().catch(() => {});
    };
  // Solo inicializamos una vez al montar.
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // Reconectar cuando llega el token por primera vez.
  useEffect(() => {
    if (!accessToken || !dbRef.current) return;
    const connector = makeConnector(() => tokenRef.current);
    dbRef.current.connect(connector).catch((e) =>
      console.error('[PowerSync] reconnect error', e),
    );
  }, [accessToken]);

  if (!ready || !dbRef.current) return null;

  return (
    <PowerSyncCtx.Provider value={{ db: dbRef.current }}>
      {children}
    </PowerSyncCtx.Provider>
  );
}

export function usePowerSyncDB(): PowerSyncDatabase {
  const ctx = useContext(PowerSyncCtx);
  if (!ctx) throw new Error('usePowerSyncDB debe usarse dentro de <PowerSyncProvider>.');
  return ctx.db;
}
