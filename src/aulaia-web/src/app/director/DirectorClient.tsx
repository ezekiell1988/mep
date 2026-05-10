'use client';

import { useAuth0 } from '@auth0/auth0-react';
import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import {
  getDirectorResumen,
  getDirectorDocentes,
  type ResumenInstitucionalResponse,
  type DocenteInstitucionalResponse,
} from '../../lib/api';

export default function DirectorClient() {
  const { isAuthenticated, isLoading, loginWithRedirect, getAccessTokenSilently } = useAuth0();
  const router = useRouter();

  const [resumen, setResumen] = useState<ResumenInstitucionalResponse | null>(null);
  const [docentes, setDocentes] = useState<DocenteInstitucionalResponse[]>([]);
  const [expanded, setExpanded] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (isLoading) return;
    if (!isAuthenticated) { void loginWithRedirect(); return; }

    let cancelled = false;
    void (async () => {
      try {
        const token = await getAccessTokenSilently();
        if (cancelled) return;
        const [res, docs] = await Promise.all([
          getDirectorResumen(token),
          getDirectorDocentes(token),
        ]);
        if (cancelled) return;
        setResumen(res);
        setDocentes(docs);
      } catch (e: unknown) {
        if (!cancelled) setError(e instanceof Error ? e.message : 'Error al cargar datos del panel de director');
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();
    return () => { cancelled = true; };
  }, [isAuthenticated, isLoading, loginWithRedirect, getAccessTokenSilently]);

  if (isLoading || loading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div
          role="status"
          aria-label="Cargando panel de director"
          className="w-8 h-8 border-4 border-blue-600 border-t-transparent rounded-full animate-spin"
        />
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <header className="bg-white border-b border-gray-200 px-4 py-4">
        <div className="max-w-6xl mx-auto flex items-center justify-between">
          <button
            type="button"
            onClick={() => router.push('/dashboard')}
            className="text-xl font-bold text-blue-600"
          >
            AulaIA — Dirección
          </button>
          <span className="text-sm text-gray-500">
            {resumen?.institutionName ?? ''}
          </span>
        </div>
      </header>

      <main className="max-w-6xl mx-auto px-4 py-8">
        <h1 className="text-2xl font-bold text-gray-900 mb-2">Panel institucional</h1>

        {resumen ? (
          <p className="text-sm text-gray-500 mb-6">
            {resumen.institutionName}
            {resumen.planInstitucional ? (
              <span className="ml-3 inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
                Plan {resumen.planInstitucional}
                {resumen.diasRestantes != null ? ` · ${resumen.diasRestantes}d restantes` : null}
              </span>
            ) : null}
          </p>
        ) : null}

        {error ? (
          <div role="alert" className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg text-red-700 text-sm">
            {error}
          </div>
        ) : null}

        {/* Stat cards */}
        {resumen ? (
          <div className="grid grid-cols-2 sm:grid-cols-3 gap-4 mb-8" aria-label="Resumen institucional">
            <StatCard label="Docentes" value={resumen.totalDocentes} icon="👩‍🏫" />
            <StatCard label="Grupos activos" value={resumen.totalGrupos} icon="🏫" />
            <StatCard label="Estudiantes" value={resumen.totalEstudiantes} icon="🎓" />
          </div>
        ) : null}

        {/* Docentes */}
        <section aria-labelledby="docentes-heading">
          <h2 id="docentes-heading" className="text-lg font-semibold text-gray-900 mb-4">
            Docentes de la institución
          </h2>

          {docentes.length === 0 ? (
            <p className="text-sm text-gray-500">Sin docentes registrados en esta institución.</p>
          ) : (
            <div className="space-y-3">
              {docentes.map(d => {
                const isOpen = expanded === d.docenteId;
                const btnShared = {
                  type: 'button' as const,
                  onClick: () => setExpanded(prev => prev === d.docenteId ? null : d.docenteId),
                  'aria-controls': `grupos-${d.docenteId}`,
                  className: 'w-full px-5 py-4 flex items-center justify-between text-left hover:bg-gray-50 transition-colors',
                };
                const btnContent = (
                  <>
                    <div className="flex items-center gap-4">
                      <div>
                        <p className="font-semibold text-gray-900">{d.fullName}</p>
                        <p className="text-sm text-gray-500">{d.email}</p>
                      </div>
                    </div>
                    <div className="flex items-center gap-6 text-sm text-right shrink-0">
                      <span className="hidden sm:block text-gray-500">
                        {d.totalGrupos} grupo{d.totalGrupos !== 1 ? 's' : ''}
                      </span>
                      <span className="hidden sm:block text-gray-500">
                        {d.totalEstudiantes} estudiante{d.totalEstudiantes !== 1 ? 's' : ''}
                      </span>
                      <span className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium ${
                        d.planActivo
                          ? 'bg-green-100 text-green-800'
                          : 'bg-gray-100 text-gray-600'
                      }`}>
                        {d.plan}
                      </span>
                      <svg
                        aria-hidden="true"
                        className={`w-5 h-5 text-gray-400 transition-transform ${isOpen ? 'rotate-180' : ''}`}
                        fill="none"
                        viewBox="0 0 24 24"
                        stroke="currentColor"
                      >
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
                      </svg>
                    </div>
                  </>
                );
                return (
                <div key={d.docenteId} className="bg-white rounded-xl border border-gray-200 overflow-hidden">
                  {isOpen ? (
                    <button {...btnShared} aria-expanded="true" aria-label={`Ocultar grupos de ${d.fullName}`}>
                      {btnContent}
                    </button>
                  ) : (
                    <button {...btnShared} aria-expanded="false" aria-label={`Ver grupos de ${d.fullName}`}>
                      {btnContent}
                    </button>
                  )}

                  {expanded === d.docenteId && (
                    <div
                      id={`grupos-${d.docenteId}`}
                      className="border-t border-gray-100 px-5 py-4"
                    >
                      {d.grupos.length === 0 ? (
                        <p className="text-sm text-gray-400">Sin grupos activos.</p>
                      ) : (
                        <table className="w-full text-sm text-left">
                          <thead>
                            <tr className="text-gray-400 border-b border-gray-100">
                              <th scope="col" className="pb-2 pr-4 font-medium">Grupo</th>
                              <th scope="col" className="pb-2 pr-4 font-medium">Asignatura</th>
                              <th scope="col" className="pb-2 pr-4 font-medium">Nivel</th>
                              <th scope="col" className="pb-2 font-medium">Estudiantes</th>
                            </tr>
                          </thead>
                          <tbody>
                            {d.grupos.map(g => (
                              <tr key={g.id} className="border-b border-gray-50 last:border-0">
                                <td className="py-2 pr-4 font-medium text-gray-800">{g.name}</td>
                                <td className="py-2 pr-4 text-gray-600">{g.subject}</td>
                                <td className="py-2 pr-4 text-gray-600">{g.level}</td>
                                <td className="py-2 text-gray-600">{g.totalEstudiantes}</td>
                              </tr>
                            ))}
                          </tbody>
                        </table>
                      )}
                    </div>
                  )}
                </div>
                );
              })}
            </div>
          )}
        </section>
      </main>
    </div>
  );
}

function StatCard({ label, value, icon }: { label: string; value: number; icon: string }) {
  return (
    <div className="bg-white rounded-xl border border-gray-200 p-5">
      <p className="text-sm text-gray-500 mb-1">
        <span aria-hidden="true">{icon}</span>{' '}
        {label}
      </p>
      <p className="text-3xl font-bold text-gray-900">{value.toLocaleString('es-CR')}</p>
    </div>
  );
}
