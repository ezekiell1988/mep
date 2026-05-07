'use client';

import { useAuth0 } from '@auth0/auth0-react';
import { useEffect, useState, useCallback, use } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import {
  getResumenNotas, getActividades, crearActividad, eliminarActividad, saveCalificaciones,
  getGrupoById, actualizarPonderacion, getInformeDirectorUrl,
  type ResumenGrupoResponse, type ActividadResponse, type ResumenEstudianteResponse,
} from '../../../lib/api';

const TIPOS_ACTIVIDAD = [
  'Prueba Escrita', 'Prueba Oral', 'Trabajo Cotidiano',
  'Proyecto', 'Portafolio', 'Trabajo Extraclase', 'Otro',
];

// Umbral de aprobación MEP: 65 (I–III Ciclo) / 70 (Diversificado)
function umbral(nivel: string | null) {
  if (nivel) {
    const n = parseInt(nivel, 10);
    if (n >= 10) return 70;
  }
  return 65;
}

function badge(promedio: number | null, nivel: string | null) {
  if (promedio === null) return { text: 'Sin notas', cls: 'bg-gray-100 text-gray-500' };
  const u = umbral(nivel);
  if (promedio >= u) return { text: `${promedio}`, cls: 'bg-green-100 text-green-700 font-semibold' };
  return { text: `${promedio}`, cls: 'bg-red-100 text-red-700 font-semibold' };
}

// Estudiante en riesgo: tiene promedio y está bajo el umbral
function isRiesgo(promedio: number | null, nivel: string | null) {
  if (promedio === null) return false;
  return promedio < umbral(nivel);
}

export default function LibroNotasPage({ params }: { params: Promise<{ grupoId: string }> }) {
  const { grupoId } = use(params);
  const { isAuthenticated, isLoading, loginWithRedirect, getAccessTokenSilently } = useAuth0();
  const router = useRouter();
  const searchParams = useSearchParams();

  const nombre    = searchParams.get('nombre') ?? 'Grupo';
  const nivel     = searchParams.get('nivel') ?? '';
  const asignatura = searchParams.get('asignatura') ?? '';

  const [resumen, setResumen] = useState<ResumenGrupoResponse | null>(null);
  const [actividades, setActividades] = useState<ActividadResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Ponderación configurable
  type Ponderacion = { pctCotidiano: number; pctPruebas: number; pctExtraclase: number; pctOtros: number };
  const [ponderacion, setPonderacion] = useState<Ponderacion>({ pctCotidiano: 20, pctPruebas: 45, pctExtraclase: 20, pctOtros: 15 });
  const [pondDraft, setPondDraft] = useState<Ponderacion>({ pctCotidiano: 20, pctPruebas: 45, pctExtraclase: 20, pctOtros: 15 });
  const [showPond, setShowPond] = useState(false);
  const [guardandoPond, setGuardandoPond] = useState(false);

  // Actividad seleccionada para editar calificaciones
  const [actividadActiva, setActividadActiva] = useState<string | null>(null);
  // notas en edición: studentId → score
  const [draft, setDraft] = useState<Record<string, string>>({});
  const [descargando, setDescargando] = useState<'xlsx' | 'pdf' | 'informe' | null>(null);

  const descargarInforme = useCallback(async () => {
    setDescargando('informe');
    try {
      const token = await getAccessTokenSilently();
      const year = new Date().getFullYear();
      const from = `${year}-02-01`;
      const to   = new Date().toISOString().slice(0, 10);
      const res = await fetch(getInformeDirectorUrl(grupoId, from, to),
        { headers: { Authorization: `Bearer ${token}` } });
      if (!res.ok) throw new Error(`Error ${res.status}`);
      const blob = await res.blob();
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `informe-director-${nombre}.pdf`;
      a.click();
      URL.revokeObjectURL(url);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Error al descargar informe');
    } finally {
      setDescargando(null);
    }
  }, [getAccessTokenSilently, grupoId, nombre]);

  const descargar = useCallback(async (formato: 'xlsx' | 'pdf') => {
    setDescargando(formato);
    try {
      const token = await getAccessTokenSilently();
      const res = await fetch(
        `${process.env.NEXT_PUBLIC_API_URL ?? ''}/api/grupos/${grupoId}/reportes/notas/${formato}`,
        { headers: { Authorization: `Bearer ${token}` } },
      );
      if (!res.ok) throw new Error(`Error ${res.status}`);
      const blob = await res.blob();
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `acta-notas-${nombre}.${formato}`;
      a.click();
      URL.revokeObjectURL(url);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Error al descargar');
    } finally {
      setDescargando(null);
    }
  }, [getAccessTokenSilently, grupoId, nombre]);
  const [guardando, setGuardando] = useState(false);

  // Modal nueva actividad
  const [showModal, setShowModal] = useState(false);
  const [newAct, setNewAct] = useState({ name: '', type: TIPOS_ACTIVIDAD[0], maxScore: 100, percentage: 20, dueDate: '' });
  const [creando, setCreando] = useState(false);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const token = await getAccessTokenSilently();
      const [res, acts, grupo] = await Promise.all([
        getResumenNotas(token, grupoId),
        getActividades(token, grupoId),
        getGrupoById(token, grupoId),
      ]);
      setResumen(res);
      setActividades(acts);
      const p = {
        pctCotidiano: grupo.pctCotidiano,
        pctPruebas: grupo.pctPruebas,
        pctExtraclase: grupo.pctExtraclase,
        pctOtros: grupo.pctOtros,
      };
      setPonderacion(p);
      setPondDraft(p);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Error al cargar');
    } finally {
      setLoading(false);
    }
  }, [getAccessTokenSilently, grupoId]);

  useEffect(() => {
    if (!isLoading && !isAuthenticated) loginWithRedirect();
    if (!isLoading && isAuthenticated) load();
  }, [isAuthenticated, isLoading, loginWithRedirect, load]);

  // Inicializar draft al abrir una actividad
  const abrirActividad = useCallback((actId: string) => {
    setActividadActiva(actId);
    const initial: Record<string, string> = {};
    resumen?.estudiantes.forEach(est => {
      const nota = est.notas.find(n => n.actividadId === actId);
      initial[est.studentId] = nota?.nota != null ? String(nota.nota) : '';
    });
    setDraft(initial);
  }, [resumen]);

  const guardarNotas = async () => {
    if (!actividadActiva) return;
    setGuardando(true);
    try {
      const token = await getAccessTokenSilently();
      const body = Object.entries(draft)
        .filter(([, v]) => v !== '')
        .map(([studentId, v]) => ({ studentId, score: parseFloat(v), comments: null }));
      await saveCalificaciones(token, grupoId, actividadActiva, body);
      await load();
      setActividadActiva(null);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Error al guardar');
    } finally {
      setGuardando(false);
    }
  };

  const handleCrearActividad = async () => {
    setCreando(true);
    try {
      const token = await getAccessTokenSilently();
      await crearActividad(token, grupoId, {
        name: newAct.name,
        type: newAct.type,
        maxScore: newAct.maxScore,
        percentage: newAct.percentage,
        dueDate: newAct.dueDate || null,
      });
      setShowModal(false);
      setNewAct({ name: '', type: TIPOS_ACTIVIDAD[0], maxScore: 100, percentage: 20, dueDate: '' });
      await load();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Error al crear');
    } finally {
      setCreando(false);
    }
  };

  const handleEliminar = async (actId: string, actNombre: string) => {
    if (!confirm(`¿Eliminar "${actNombre}" y todas sus calificaciones?`)) return;
    try {
      const token = await getAccessTokenSilently();
      await eliminarActividad(token, grupoId, actId);
      await load();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Error al eliminar');
    }
  };

  const handleGuardarPonderacion = async () => {
    const sum = pondDraft.pctCotidiano + pondDraft.pctPruebas + pondDraft.pctExtraclase + pondDraft.pctOtros;
    if (Math.abs(sum - 100) > 0.01) {
      setError(`Los porcentajes deben sumar 100 (actual: ${sum.toFixed(1)})`);
      return;
    }
    setGuardandoPond(true);
    try {
      const token = await getAccessTokenSilently();
      await actualizarPonderacion(token, grupoId, pondDraft);
      setPonderacion(pondDraft);
      setShowPond(false);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Error al guardar ponderación');
    } finally {
      setGuardandoPond(false);
    }
  };

  if (isLoading || loading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div role="status" aria-label="Cargando libro de notas" className="w-8 h-8 border-4 border-green-600 border-t-transparent rounded-full animate-spin" />
      </div>
    );
  }

  const pesoTotal = actividades.reduce((s, a) => s + a.percentage, 0);

  return (
    <div className="max-w-6xl mx-auto px-4 py-8">
      {/* Header */}
      <div className="flex items-start justify-between mb-6">
        <div>
          <button type="button" onClick={() => router.back()} className="text-sm text-gray-400 hover:text-gray-600 mb-1">← Volver</button>
          <h1 className="text-2xl font-bold text-gray-900">Libro de Notas</h1>
          <p className="text-sm text-gray-500 mt-0.5">{nombre} · {asignatura} · {nivel}</p>
        </div>
        <div className="flex items-center gap-2">
          <button
            type="button"
            onClick={() => descargar('xlsx')}
            disabled={descargando !== null}
            className="text-sm border border-green-300 text-green-700 hover:bg-green-50 font-medium px-3 py-2 rounded-lg transition-colors disabled:opacity-60"
          >
            {descargando === 'xlsx' ? 'Generando…' : '↓ XLSX (SEA)'}
          </button>
          <button
            type="button"
            onClick={() => descargar('pdf')}
            disabled={descargando !== null}
            className="text-sm border border-red-300 text-red-700 hover:bg-red-50 font-medium px-3 py-2 rounded-lg transition-colors disabled:opacity-60"
          >
            {descargando === 'pdf' ? 'Generando…' : '↓ PDF'}
          </button>
          <button
            type="button"
            onClick={() => void descargarInforme()}
            disabled={descargando !== null}
            className="text-sm border border-blue-300 text-blue-700 hover:bg-blue-50 font-medium px-3 py-2 rounded-lg transition-colors disabled:opacity-60"
          >
            {descargando === 'informe' ? 'Generando…' : '↓ Informe Dirección'}
          </button>
          <button
            type="button"
            onClick={() => setShowModal(true)}
            className="bg-green-600 hover:bg-green-700 text-white text-sm font-medium px-4 py-2 rounded-lg transition-colors"
          >
            + Nueva actividad
          </button>
        </div>
      </div>

      {error ? (
        <div role="alert" className="bg-red-50 border border-red-200 text-red-700 rounded-xl p-4 mb-6">{error}</div>
      ) : null}

      {/* Panel ponderación configurable */}
      <div className="mb-5">
        <button
          type="button"
          onClick={() => { setShowPond(v => !v); setPondDraft(ponderacion); }}
          className="text-sm text-gray-500 hover:text-gray-700 flex items-center gap-1"
        >
          <span className="font-medium">⚖ Ponderación MEP</span>
          <span className="text-xs text-gray-400">
            ({ponderacion.pctCotidiano}% Cot. / {ponderacion.pctPruebas}% Pruebas / {ponderacion.pctExtraclase}% Extra. / {ponderacion.pctOtros}% Otros)
          </span>
          <span className="text-xs">{showPond ? '▲' : '▼'}</span>
        </button>
        {showPond && (
          <div className="mt-3 bg-gray-50 border border-gray-200 rounded-xl p-4 max-w-lg">
            <p className="text-xs text-gray-500 mb-3">Distribución porcentual de la nota final. Deben sumar 100.</p>
            {([ 
              ['Trabajo Cotidiano', 'pctCotidiano'],
              ['Pruebas y Exámenes', 'pctPruebas'],
              ['Trabajo Extraclase', 'pctExtraclase'],
              ['Otros', 'pctOtros'],
            ] as [string, keyof typeof pondDraft][]).map(([label, key]) => (
              <div key={key} className="flex items-center gap-3 mb-2">
                <label htmlFor={`pond-${key}`} className="w-40 text-sm text-gray-700 shrink-0">{label}</label>
                <input
                  id={`pond-${key}`}
                  type="number"
                  min={0}
                  max={100}
                  step={0.5}
                  value={pondDraft[key]}
                  onChange={e => setPondDraft(d => ({ ...d, [key]: Number(e.target.value) }))}
                  className="w-20 border border-gray-300 rounded-lg px-2 py-1.5 text-sm text-center focus:outline-none focus:ring-2 focus:ring-green-500"
                />
                <span className="text-sm text-gray-400">%</span>
              </div>
            ))}
            {(() => {
              const s = pondDraft.pctCotidiano + pondDraft.pctPruebas + pondDraft.pctExtraclase + pondDraft.pctOtros;
              return (
                <p className={`text-sm font-medium mt-1 mb-3 ${Math.abs(s - 100) < 0.01 ? 'text-green-700' : 'text-red-600'}`}>
                  Total: {s.toFixed(1)}% {Math.abs(s - 100) < 0.01 ? '✓' : '— debe ser 100'}
                </p>
              );
            })()}
            <div className="flex gap-2">
              <button
                type="button"
                onClick={() => setShowPond(false)}
                className="border border-gray-300 text-gray-600 rounded-lg px-4 py-1.5 text-sm hover:bg-gray-100"
              >
                Cancelar
              </button>
              <button
                type="button"
                onClick={handleGuardarPonderacion}
                disabled={guardandoPond}
                className="bg-green-600 hover:bg-green-700 disabled:opacity-60 text-white rounded-lg px-4 py-1.5 text-sm font-medium"
              >
                {guardandoPond ? 'Guardando…' : 'Guardar'}
              </button>
            </div>
          </div>
        )}
      </div>

      {/* Resumen de peso */}
      {actividades.length > 0 ? (
        <div className="mb-4 flex flex-wrap gap-2">
          {actividades.map(a => (
            <span key={a.id} className="text-xs bg-gray-100 text-gray-600 rounded-full px-3 py-1">
              {a.name} <span className="font-medium">{a.percentage}%</span>
            </span>
          ))}
          <span className={`text-xs rounded-full px-3 py-1 font-semibold ${pesoTotal === 100 ? 'bg-green-100 text-green-700' : 'bg-yellow-100 text-yellow-700'}`}>
            Total: {pesoTotal}%
          </span>
        </div>
      ) : null}

      {/* Tabla */}
      {resumen && resumen.estudiantes.length > 0 ? (
        <div className="overflow-x-auto rounded-xl border border-gray-200 shadow-sm">
          <table className="w-full text-sm">
            <thead className="bg-gray-50">
              <tr>
                <th className="text-left px-4 py-3 font-medium text-gray-600 whitespace-nowrap">Estudiante</th>
                {actividades.map(a => (
                  <th key={a.id} className="px-3 py-3 font-medium text-gray-600 text-center whitespace-nowrap">
                    <div>{a.name}</div>
                    <div className="text-xs text-gray-400 font-normal">{a.type} · {a.percentage}%</div>
                    <div className="flex items-center justify-center gap-1 mt-1">
                      <button
                        type="button"
                        onClick={() => abrirActividad(a.id)}
                        className="text-xs bg-blue-50 hover:bg-blue-100 text-blue-600 rounded px-2 py-0.5"
                      >
                        Editar
                      </button>
                      <button
                        type="button"
                        onClick={() => handleEliminar(a.id, a.name)}
                        className="text-xs bg-red-50 hover:bg-red-100 text-red-500 rounded px-2 py-0.5"
                      >
                        ✕
                      </button>
                    </div>
                  </th>
                ))}
                <th className="px-4 py-3 font-medium text-gray-600 text-center whitespace-nowrap">Promedio</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {resumen.estudiantes.map((est: ResumenEstudianteResponse, i: number) => {
                const b = badge(est.promedio, nivel);
                const riesgo = isRiesgo(est.promedio, nivel);
                return (
                  <tr key={est.studentId} className={`${i % 2 === 0 ? 'bg-white' : 'bg-gray-50/50'} ${riesgo ? 'ring-1 ring-inset ring-red-200' : ''}`}>
                    <td className="px-4 py-3 whitespace-nowrap">
                      <p className="font-medium text-gray-900">
                        {riesgo && <span aria-label="En riesgo" title="Promedio bajo el umbral de aprobación" className="mr-1 text-red-500">⚠</span>}
                        {est.fullName}
                      </p>
                      <p className="text-xs text-gray-400">{est.studentCode}</p>
                    </td>
                    {actividades.map(a => {
                      const nota = est.notas.find(n => n.actividadId === a.id);
                      return (
                        <td key={a.id} className="px-3 py-3 text-center">
                          {nota?.nota != null
                            ? <span className="font-medium text-gray-800">{nota.nota}</span>
                            : <span className="text-gray-300">—</span>}
                        </td>
                      );
                    })}
                    <td className="px-4 py-3 text-center">
                      <span className={`inline-block rounded-full px-3 py-1 text-xs ${b.cls}`}>{b.text}</span>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      ) : (
        <p className="text-center text-gray-400 mt-16">
          {actividades.length === 0
            ? 'Crea una actividad de evaluación para comenzar.'
            : 'No hay estudiantes en este grupo.'}
        </p>
      )}

      {/* Panel edición calificaciones */}
      {actividadActiva ? (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-2xl shadow-xl w-full max-w-md max-h-[80vh] overflow-y-auto p-6">
            <h2 className="text-lg font-semibold mb-4">
              {actividades.find(a => a.id === actividadActiva)?.name}
            </h2>
            <p className="text-xs text-gray-400 mb-4">
              Máx. {actividades.find(a => a.id === actividadActiva)?.maxScore} pts
            </p>
            <div className="space-y-3">
              {resumen?.estudiantes.map(est => (
                <div key={est.studentId} className="flex items-center justify-between gap-3">
                  <span className="text-sm text-gray-800 flex-1 truncate">{est.fullName}</span>
                  <input
                    type="number"
                    min={0}
                    max={actividades.find(a => a.id === actividadActiva)?.maxScore ?? 100}
                    step={0.5}
                    value={draft[est.studentId] ?? ''}
                    onChange={e => setDraft(d => ({ ...d, [est.studentId]: e.target.value }))}
                    placeholder="—"
                    className="w-20 border border-gray-300 rounded-lg px-2 py-1.5 text-sm text-center focus:outline-none focus:ring-2 focus:ring-green-500"
                  />
                </div>
              ))}
            </div>
            <div className="flex gap-2 mt-6">
              <button
                type="button"
                onClick={() => setActividadActiva(null)}
                className="flex-1 border border-gray-300 text-gray-600 rounded-lg py-2 text-sm hover:bg-gray-50"
              >
                Cancelar
              </button>
              <button
                type="button"
                onClick={guardarNotas}
                disabled={guardando}
                className="flex-1 bg-green-600 hover:bg-green-700 disabled:opacity-60 text-white rounded-lg py-2 text-sm font-medium"
              >
                {guardando ? 'Guardando…' : 'Guardar'}
              </button>
            </div>
          </div>
        </div>
      ) : null}

      {/* Modal nueva actividad */}
      {showModal ? (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-2xl shadow-xl w-full max-w-sm p-6">
            <h2 className="text-lg font-semibold mb-4">Nueva actividad</h2>
            <div className="space-y-3">
              <div>
                <label htmlFor="act-nombre" className="block text-sm font-medium text-gray-700 mb-1">Nombre</label>
                <input
                  id="act-nombre"
                  type="text"
                  value={newAct.name}
                  onChange={e => setNewAct(v => ({ ...v, name: e.target.value }))}
                  placeholder="Ej: Prueba del tema 3"
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-green-500"
                />
              </div>
              <div>
                <label htmlFor="act-tipo" className="block text-sm font-medium text-gray-700 mb-1">Tipo</label>
                <select
                  id="act-tipo"
                  value={newAct.type}
                  onChange={e => setNewAct(v => ({ ...v, type: e.target.value }))}
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-green-500"
                >
                  {TIPOS_ACTIVIDAD.map(t => <option key={t} value={t}>{t}</option>)}
                </select>
              </div>
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label htmlFor="act-max" className="block text-sm font-medium text-gray-700 mb-1">Máx. puntos</label>
                  <input
                    id="act-max"
                    type="number" min={1} max={200}
                    value={newAct.maxScore}
                    onChange={e => setNewAct(v => ({ ...v, maxScore: Number(e.target.value) }))}
                    className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-green-500"
                  />
                </div>
                <div>
                  <label htmlFor="act-pct" className="block text-sm font-medium text-gray-700 mb-1">Porcentaje %</label>
                  <input
                    id="act-pct"
                    type="number" min={1} max={100}
                    value={newAct.percentage}
                    onChange={e => setNewAct(v => ({ ...v, percentage: Number(e.target.value) }))}
                    className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-green-500"
                  />
                </div>
              </div>
              <div>
                <label htmlFor="act-fecha" className="block text-sm font-medium text-gray-700 mb-1">Fecha de entrega (opcional)</label>
                <input
                  id="act-fecha"
                  type="date"
                  value={newAct.dueDate}
                  onChange={e => setNewAct(v => ({ ...v, dueDate: e.target.value }))}
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-green-500"
                />
              </div>
            </div>
            <div className="flex gap-2 mt-6">
              <button
                type="button"
                onClick={() => setShowModal(false)}
                className="flex-1 border border-gray-300 text-gray-600 rounded-lg py-2 text-sm hover:bg-gray-50"
              >
                Cancelar
              </button>
              <button
                type="button"
                onClick={handleCrearActividad}
                disabled={creando || !newAct.name.trim()}
                className="flex-1 bg-green-600 hover:bg-green-700 disabled:opacity-60 text-white rounded-lg py-2 text-sm font-medium"
              >
                {creando ? 'Creando…' : 'Crear'}
              </button>
            </div>
          </div>
        </div>
      ) : null}
    </div>
  );
}
