'use client';

import { useAuth0 } from '@auth0/auth0-react';
import { useEffect, useState, useCallback, use } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import { getHistorialAsistencia, type HistorialAsistenciaResponse } from '../../../lib/api';

const STATUS_LABEL: Record<string, string> = {
  Present:    'P',
  Absent:     'A',
  Late:       'T',
  Justified:  'J',
};

const STATUS_CLASS: Record<string, string> = {
  Present:   'bg-green-100 text-green-700',
  Absent:    'bg-red-100 text-red-700',
  Late:      'bg-yellow-100 text-yellow-700',
  Justified: 'bg-blue-100 text-blue-700',
};

function todayStr() {
  return new Date().toISOString().slice(0, 10);
}

function monthStartStr() {
  const d = new Date();
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-01`;
}

function shortDate(iso: string) {
  const [, m, d] = iso.split('-');
  return `${d}/${m}`;
}

function countByStatus(row: HistorialAsistenciaResponse['filas'][number]) {
  const counts = { Present: 0, Absent: 0, Late: 0, Justified: 0 };
  for (const v of Object.values(row.asistencia)) {
    if (v && v in counts) counts[v as keyof typeof counts]++;
  }
  return counts;
}

export default function HistorialAsistenciaPage({ params }: { params: Promise<{ grupoId: string }> }) {
  const { grupoId } = use(params);
  const { isAuthenticated, isLoading, loginWithRedirect, getAccessTokenSilently } = useAuth0();
  const router   = useRouter();
  const searchParams = useSearchParams();

  const nombre = searchParams.get('nombre') ?? 'Grupo';

  const [from, setFrom] = useState(monthStartStr());
  const [to,   setTo]   = useState(todayStr());
  const [data,    setData]    = useState<HistorialAsistenciaResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error,   setError]   = useState<string | null>(null);

  const load = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const token = await getAccessTokenSilently();
      const res   = await getHistorialAsistencia(token, grupoId, from, to);
      setData(res);
    } catch (e: unknown) {
      setError(e instanceof Error ? e.message : 'Error al cargar historial');
    } finally {
      setLoading(false);
    }
  }, [getAccessTokenSilently, grupoId, from, to]);

  useEffect(() => {
    if (!isLoading && !isAuthenticated) loginWithRedirect();
    if (!isLoading && isAuthenticated) load();
  }, [isAuthenticated, isLoading, loginWithRedirect, load]);

  if (isLoading || loading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div role="status" aria-label="Cargando historial" className="w-8 h-8 border-4 border-blue-600 border-t-transparent rounded-full animate-spin" />
      </div>
    );
  }

  return (
    <div className="max-w-full px-4 py-8">
      {/* Encabezado */}
      <div className="flex items-center gap-3 mb-6">
        <button
          type="button"
          onClick={() => router.push('/grupos')}
          className="text-sm text-blue-600 hover:underline"
          aria-label="Volver a grupos"
        >
          ← Grupos
        </button>
        <h1 className="text-xl font-bold text-gray-900">Asistencia — {nombre}</h1>
      </div>

      {/* Filtro de rango */}
      <div className="flex flex-wrap items-end gap-4 mb-6 bg-white border border-gray-200 rounded-xl p-4 shadow-sm">
        <label className="flex flex-col gap-1 text-sm font-medium text-gray-700">
          Desde
          <input
            type="date"
            value={from}
            onChange={e => setFrom(e.target.value)}
            className="border border-gray-300 rounded-lg px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </label>
        <label className="flex flex-col gap-1 text-sm font-medium text-gray-700">
          Hasta
          <input
            type="date"
            value={to}
            onChange={e => setTo(e.target.value)}
            className="border border-gray-300 rounded-lg px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </label>
        <button
          type="button"
          onClick={load}
          className="bg-blue-600 hover:bg-blue-700 text-white text-sm font-semibold px-4 py-2 rounded-lg transition-colors"
        >
          Buscar
        </button>
      </div>

      {error !== null ? (
        <div role="alert" className="bg-red-50 border border-red-200 text-red-700 rounded-xl p-4 mb-6">{error}</div>
      ) : null}

      {/* Leyenda */}
      <div className="flex flex-wrap gap-3 mb-4 text-xs">
        {(['Present', 'Absent', 'Late', 'Justified'] as const).map(s => (
          <span key={s} className={`px-2 py-0.5 rounded font-semibold ${STATUS_CLASS[s]}`}>
            {STATUS_LABEL[s]} = {s === 'Present' ? 'Presente' : s === 'Absent' ? 'Ausente' : s === 'Late' ? 'Tardanza' : 'Justificado'}
          </span>
        ))}
        <span className="px-2 py-0.5 rounded bg-gray-100 text-gray-400 font-semibold">— = Sin registro</span>
      </div>

      {data === null || data.fechas.length === 0 ? (
        <p className="text-center text-gray-400 mt-16">No hay registros de asistencia en el rango seleccionado.</p>
      ) : (
        <div className="overflow-x-auto rounded-xl border border-gray-200 shadow-sm">
          <table className="min-w-max text-sm border-collapse">
            <thead>
              <tr className="bg-gray-50">
                <th className="sticky left-0 bg-gray-50 z-10 px-4 py-3 text-left font-semibold text-gray-600 border-b border-r border-gray-200 whitespace-nowrap">
                  Estudiante
                </th>
                {data.fechas.map(f => (
                  <th key={f} className="px-3 py-3 text-center font-medium text-gray-500 border-b border-gray-200 whitespace-nowrap min-w-[3rem]">
                    {shortDate(f)}
                  </th>
                ))}
                <th className="px-3 py-3 text-center font-semibold text-gray-600 border-b border-l border-gray-200 whitespace-nowrap">P</th>
                <th className="px-3 py-3 text-center font-semibold text-gray-600 border-b border-gray-200 whitespace-nowrap">A</th>
                <th className="px-3 py-3 text-center font-semibold text-gray-600 border-b border-gray-200 whitespace-nowrap">T</th>
                <th className="px-3 py-3 text-center font-semibold text-gray-600 border-b border-gray-200 whitespace-nowrap">J</th>
              </tr>
            </thead>
            <tbody>
              {data.filas.map((fila, idx) => {
                const counts = countByStatus(fila);
                return (
                  <tr key={fila.studentId} className={idx % 2 === 0 ? 'bg-white' : 'bg-gray-50'}>
                    <td className="sticky left-0 z-10 px-4 py-2 font-medium text-gray-800 border-r border-gray-200 whitespace-nowrap"
                        style={{ backgroundColor: idx % 2 === 0 ? '#fff' : '#f9fafb' }}>
                      {fila.fullName}
                      <span className="ml-2 text-xs text-gray-400">{fila.studentCode}</span>
                    </td>
                    {data.fechas.map(f => {
                      const st = fila.asistencia[f] ?? null;
                      return (
                        <td key={f} className="px-2 py-2 text-center border-gray-100 border-b">
                          {st ? (
                            <span className={`inline-block w-7 h-7 leading-7 rounded-full text-xs font-bold ${STATUS_CLASS[st] ?? 'bg-gray-100 text-gray-400'}`}>
                              {STATUS_LABEL[st] ?? st[0]}
                            </span>
                          ) : (
                            <span className="text-gray-300 text-xs">—</span>
                          )}
                        </td>
                      );
                    })}
                    <td className="px-3 py-2 text-center text-green-700 font-semibold border-l border-gray-200">{counts.Present}</td>
                    <td className="px-3 py-2 text-center text-red-600 font-semibold">{counts.Absent}</td>
                    <td className="px-3 py-2 text-center text-yellow-600 font-semibold">{counts.Late}</td>
                    <td className="px-3 py-2 text-center text-blue-600 font-semibold">{counts.Justified}</td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
