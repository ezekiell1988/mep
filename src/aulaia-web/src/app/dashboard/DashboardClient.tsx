'use client';

import { useAuth0 } from '@auth0/auth0-react';
import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import {
  getDocenteResumen,
  type DocenteResumenResponse,
} from '../../lib/api';

const CALENDAR_EVENT_LABELS: Record<string, string> = {
  Holiday:        'Feriado',
  Exam:           'Exámenes',
  TeacherMeeting: 'Consejo de profesores',
  SportWeek:      'FEA / Deporte',
  Civic:          'Acto cívico',
  Institutional:  'Institucional',
  Other:          'Otro',
};

export default function DashboardClient() {
  const { isAuthenticated, isLoading, loginWithRedirect, logout, getAccessTokenSilently, user } = useAuth0();
  const router = useRouter();
  const [resumen, setResumen] = useState<DocenteResumenResponse | null>(null);
  const [loading, setLoading]   = useState(true);
  const [error, setError]       = useState<string | null>(null);

  useEffect(() => {
    if (isLoading) return;
    if (!isAuthenticated) {
      void loginWithRedirect();
      return;
    }

    let cancelled = false;

    void (async () => {
      try {
        const token = await getAccessTokenSilently();
        if (cancelled) return;
        const data = await getDocenteResumen(token);
        if (cancelled) return;
        setResumen(data);
        setError(null);
      } catch (e: unknown) {
        if (!cancelled) setError(e instanceof Error ? e.message : 'Error al cargar resumen');
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();

    return () => { cancelled = true; };
  }, [isAuthenticated, isLoading, loginWithRedirect, getAccessTokenSilently]);

  if (isLoading || loading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div role="status" aria-label="Cargando" className="w-8 h-8 border-4 border-blue-600 border-t-transparent rounded-full animate-spin" />
      </div>
    );
  }

  return (
    <div className="max-w-4xl mx-auto px-4 py-8">
      {/* Header */}
      <div className="flex items-center justify-between mb-8">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Dashboard</h1>
          {user?.name && <p className="text-sm text-gray-500 mt-0.5">{user.name}</p>}
        </div>
        <div className="flex items-center gap-3">
          <button
            type="button"
            onClick={() => router.push('/grupos')}
            className="text-sm border border-gray-300 text-gray-600 hover:bg-gray-50 font-medium px-4 py-2 rounded-lg transition-colors"
          >
            Mis Grupos
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

      {resumen && (
        <>
          {/* Tarjetas de estadísticas */}
          <div className="grid grid-cols-2 md:grid-cols-3 gap-4 mb-8">
            <StatCard
              label="Grupos activos"
              value={resumen.totalGrupos}
              color="blue"
              onClick={() => router.push('/grupos')}
            />
            <StatCard
              label="Estudiantes"
              value={resumen.totalEstudiantes}
              color="green"
            />
            <StatCard
              label="En riesgo"
              value={resumen.estudiantesEnRiesgo}
              color={resumen.estudiantesEnRiesgo > 0 ? 'red' : 'green'}
              tooltip="Estudiantes con promedio de notas < 65"
            />
            <StatCard
              label="Planeamientos listos"
              value={resumen.planeamientosListos}
              color="indigo"
              onClick={() => router.push('/planeamiento')}
            />
            {resumen.planeamientosPendientes > 0 && (
              <StatCard
                label="Planeamientos generando"
                value={resumen.planeamientosPendientes}
                color="yellow"
                onClick={() => router.push('/planeamiento')}
              />
            )}
            <StatCard
              label="Adecuaciones activas"
              value={resumen.adecuacionesActivas}
              color="orange"
            />
          </div>

          {/* Próximos eventos */}
          {resumen.proximosEventos.length > 0 && (
            <section aria-labelledby="proximos-heading">
              <h2 id="proximos-heading" className="text-lg font-semibold text-gray-800 mb-3">
                Próximos eventos — 14 días
              </h2>
              <ul className="bg-white rounded-xl border border-gray-200 divide-y divide-gray-100 shadow-sm">
                {resumen.proximosEventos.map((ev, i) => (
                  <li key={i} className="flex items-center justify-between px-5 py-3">
                    <span className="text-sm text-gray-800">{ev.titulo}</span>
                    <div className="flex items-center gap-3">
                      <span className="text-xs text-gray-500 bg-gray-100 px-2 py-0.5 rounded-full">
                        {CALENDAR_EVENT_LABELS[ev.tipo] ?? ev.tipo}
                      </span>
                      <span className="text-xs font-mono text-gray-400">{ev.fecha}</span>
                    </div>
                  </li>
                ))}
              </ul>
            </section>
          )}

          {resumen.totalGrupos === 0 && (
            <p className="text-center text-gray-400 mt-16">
              Aún no tienes grupos. <button type="button" onClick={() => router.push('/grupos')} className="text-blue-600 underline">Crear uno</button>
            </p>
          )}
        </>
      )}
    </div>
  );
}

// ── Componente auxiliar ────────────────────────────────────────────────────

interface StatCardProps {
  label: string;
  value: number;
  color: 'blue' | 'green' | 'red' | 'yellow' | 'indigo' | 'orange';
  tooltip?: string;
  onClick?: () => void;
}

const COLOR_MAP: Record<StatCardProps['color'], string> = {
  blue:   'bg-blue-50 text-blue-700 border-blue-200',
  green:  'bg-green-50 text-green-700 border-green-200',
  red:    'bg-red-50 text-red-700 border-red-200',
  yellow: 'bg-yellow-50 text-yellow-700 border-yellow-200',
  indigo: 'bg-indigo-50 text-indigo-700 border-indigo-200',
  orange: 'bg-orange-50 text-orange-700 border-orange-200',
};

function StatCard({ label, value, color, tooltip, onClick }: StatCardProps) {
  const cls = `rounded-xl border p-5 text-left w-full ${COLOR_MAP[color]} ${onClick ? 'cursor-pointer hover:brightness-95 transition-all' : ''}`;
  const content = (
    <>
      <p className="text-3xl font-bold">{value}</p>
      <p className="text-sm mt-1 font-medium opacity-80">{label}</p>
    </>
  );
  if (onClick) {
    return (
      <button type="button" className={cls} onClick={onClick} title={tooltip}>
        {content}
      </button>
    );
  }
  return (
    <div className={cls} title={tooltip}>
      {content}
    </div>
  );
}
