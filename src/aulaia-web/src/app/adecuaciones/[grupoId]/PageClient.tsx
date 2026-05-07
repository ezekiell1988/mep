'use client';

import { useAuth0 } from '@auth0/auth0-react';
import { useEffect, useState, useCallback, use } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import {
  getEstudiantes,
  listAdecuaciones,
  getAdecuacion,
  upsertAdecuacion,
  eliminarAdecuacion,
  generarPropuestaAdecuacion,
  getInformeAdecuacionUrl,
  ACCOMMODATION_TYPE_LABELS,
  type AdecuacionResumen,
  type AdecuacionResponse,
  type AccommodationType,
  type Estudiante,
} from '../../../lib/api';

const STATUS_BADGE: Record<string, string> = {
  Draft:      'bg-gray-100 text-gray-600',
  Pending:    'bg-yellow-100 text-yellow-700',
  Generating: 'bg-blue-100 text-blue-700 animate-pulse',
  Ready:      'bg-green-100 text-green-700',
  Failed:     'bg-red-100 text-red-700',
};

const STATUS_LABEL: Record<string, string> = {
  Draft:      'Borrador',
  Pending:    'Pendiente',
  Generating: 'Generando…',
  Ready:      'Lista',
  Failed:     'Error',
};

export default function AdecuacionesPage({ params }: { params: Promise<{ grupoId: string }> }) {
  const { grupoId } = use(params);
  const { isAuthenticated, isLoading, loginWithRedirect, getAccessTokenSilently } = useAuth0();
  const router = useRouter();
  const searchParams = useSearchParams();
  const nombre = searchParams.get('nombre') ?? 'Grupo';

  const [estudiantes, setEstudiantes] = useState<Estudiante[]>([]);
  const [adecuaciones, setAdecuaciones] = useState<AdecuacionResumen[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Panel detalle / edición
  const [selected, setSelected] = useState<AdecuacionResponse | null>(null);
  const [panelOpen, setPanelOpen] = useState(false);
  const [panelStudentId, setPanelStudentId] = useState<string | null>(null);
  const [panelStudentName, setPanelStudentName] = useState('');
  const [saving, setSaving] = useState(false);
  const [generating, setGenerating] = useState(false);
  const [panelError, setPanelError] = useState<string | null>(null);
  const [reloadTrigger, setReloadTrigger] = useState(0);
  const reload = useCallback(() => setReloadTrigger(t => t + 1), []);

  // Formulario
  const [form, setForm] = useState<{
    type: AccommodationType;
    diagnostico: string;
    condicionEspecial: string;
    estrategiasMediacion: string;
    estrategiasEvaluacion: string;
    observaciones: string;
  }>({ type: 'ANS', diagnostico: '', condicionEspecial: '', estrategiasMediacion: '', estrategiasEvaluacion: '', observaciones: '' });

  useEffect(() => {
    if (isLoading) return;
    if (!isAuthenticated) { void loginWithRedirect(); return; }

    let cancelled = false;
    void (async () => {
      try {
        const token = await getAccessTokenSilently();
        if (cancelled) return;
        const [est, adec] = await Promise.all([
          getEstudiantes(token, grupoId),
          listAdecuaciones(token, grupoId),
        ]);
        if (cancelled) return;
        setEstudiantes(est);
        setAdecuaciones(adec);
        setError(null);
      } catch (e: unknown) {
        if (!cancelled) setError(e instanceof Error ? e.message : 'Error al cargar datos');
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();

    return () => { cancelled = true; };
  }, [isAuthenticated, isLoading, loginWithRedirect, getAccessTokenSilently, grupoId, reloadTrigger]);

  // Polling cuando hay generaciones en curso
  useEffect(() => {
    const hasGenerating = adecuaciones.some(a => a.status === 'Generating' || a.status === 'Pending');
    if (!hasGenerating) return;
    const id = setInterval(reload, 4000);
    return () => clearInterval(id);
  }, [adecuaciones, reload]);

  async function openPanel(studentId: string, studentName: string) {
    setPanelStudentId(studentId);
    setPanelStudentName(studentName);
    setPanelError(null);
    setSelected(null);
    try {
      const token = await getAccessTokenSilently();
      const acc = await getAdecuacion(token, grupoId, studentId);
      setSelected(acc);
      setForm({
        type: acc.type,
        diagnostico: acc.diagnostico,
        condicionEspecial: acc.condicionEspecial ?? '',
        estrategiasMediacion: acc.estrategiasMediacion ?? '',
        estrategiasEvaluacion: acc.estrategiasEvaluacion ?? '',
        observaciones: acc.observaciones ?? '',
      });
    } catch {
      // no existe aún — formulario en blanco
      setForm({ type: 'ANS', diagnostico: '', condicionEspecial: '', estrategiasMediacion: '', estrategiasEvaluacion: '', observaciones: '' });
    }
    setPanelOpen(true);
  }

  async function handleSave() {
    if (!panelStudentId || !form.diagnostico.trim()) return;
    setSaving(true); setPanelError(null);
    try {
      const token = await getAccessTokenSilently();
      const saved = await upsertAdecuacion(token, grupoId, panelStudentId, {
        type: form.type,
        diagnostico: form.diagnostico,
        condicionEspecial: form.condicionEspecial || null,
        estrategiasMediacion: form.estrategiasMediacion || null,
        estrategiasEvaluacion: form.estrategiasEvaluacion || null,
        observaciones: form.observaciones || null,
      });
      setSelected(saved);
      reload();
    } catch (e: unknown) {
      setPanelError(e instanceof Error ? e.message : 'Error al guardar');
    } finally {
      setSaving(false);
    }
  }

  async function handleGenerar() {
    if (!panelStudentId || !selected) return;
    setGenerating(true); setPanelError(null);
    try {
      const token = await getAccessTokenSilently();
      const updated = await generarPropuestaAdecuacion(token, grupoId, panelStudentId);
      setSelected(updated);
      reload();
    } catch (e: unknown) {
      setPanelError(e instanceof Error ? e.message : 'Error al generar propuesta');
    } finally {
      setGenerating(false);
    }
  }

  async function handleEliminar() {
    if (!panelStudentId) return;
    if (!confirm('¿Eliminar la adecuación de este estudiante?')) return;
    try {
      const token = await getAccessTokenSilently();
      await eliminarAdecuacion(token, grupoId, panelStudentId);
      setPanelOpen(false);
      setSelected(null);
      reload();
    } catch (e: unknown) {
      setPanelError(e instanceof Error ? e.message : 'Error al eliminar');
    }
  }

  async function handleDescargarPDF() {
    if (!panelStudentId) return;
    try {
      const token = await getAccessTokenSilently();
      const url = getInformeAdecuacionUrl(grupoId, panelStudentId);
      const res = await fetch(url, { headers: { Authorization: `Bearer ${token}` } });
      if (!res.ok) throw new Error('No se pudo descargar el informe');
      const blob = await res.blob();
      const a = document.createElement('a');
      a.href = URL.createObjectURL(blob);
      a.download = `adecuacion_${panelStudentName.replace(/\s+/g, '_')}.pdf`;
      a.click();
    } catch (e: unknown) {
      setPanelError(e instanceof Error ? e.message : 'Error al descargar');
    }
  }

  if (isLoading || loading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div role="status" aria-label="Cargando" className="w-8 h-8 border-4 border-orange-500 border-t-transparent rounded-full animate-spin" />
      </div>
    );
  }

  const adecuacionesByStudent = new Map(adecuaciones.map(a => [a.studentId, a]));

  return (
    <div className="max-w-4xl mx-auto px-4 py-8">
      {/* Header */}
      <div className="flex items-center gap-3 mb-6">
        <button type="button" onClick={() => router.push('/grupos')} className="text-gray-400 hover:text-gray-600">
          <span aria-hidden="true">←</span> Grupos
        </button>
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Adecuaciones Curriculares</h1>
          <p className="text-sm text-gray-500">{nombre}</p>
        </div>
      </div>

      {error !== null && (
        <div role="alert" className="bg-red-50 border border-red-200 text-red-700 rounded-xl p-4 mb-4">{error}</div>
      )}

      {/* Lista estudiantes */}
      <div className="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
        <table className="w-full text-sm" aria-label="Estudiantes y sus adecuaciones curriculares">
          <thead>
            <tr className="bg-gray-50 border-b border-gray-200">
              <th scope="col" className="text-left px-4 py-3 font-semibold text-gray-700">Estudiante</th>
              <th scope="col" className="text-left px-4 py-3 font-semibold text-gray-700">Expediente</th>
              <th scope="col" className="text-left px-4 py-3 font-semibold text-gray-700">Tipo</th>
              <th scope="col" className="text-left px-4 py-3 font-semibold text-gray-700">Estado</th>
              <th scope="col" className="px-4 py-3" aria-label="Acciones" />
            </tr>
          </thead>
          <tbody>
            {estudiantes.map((est) => {
              const adec = adecuacionesByStudent.get(est.studentId);
              return (
                <tr key={est.studentId} className="border-b border-gray-100 last:border-0 hover:bg-gray-50">
                  <td className="px-4 py-3 font-medium text-gray-900">{est.fullName}</td>
                  <td className="px-4 py-3 text-gray-500">{est.studentCode}</td>
                  <td className="px-4 py-3">
                    {adec
                      ? <span className="font-medium text-orange-700">{ACCOMMODATION_TYPE_LABELS[adec.type]}</span>
                      : <span className="text-gray-300">—</span>
                    }
                  </td>
                  <td className="px-4 py-3" aria-live="polite">
                    {adec
                      ? <span className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium ${STATUS_BADGE[adec.status] ?? ''}`}>
                          {STATUS_LABEL[adec.status] ?? adec.status}
                        </span>
                      : <span className="text-gray-300 text-xs">Sin adecuación</span>
                    }
                  </td>
                  <td className="px-4 py-3 text-right">
                    <button
                      type="button"
                      onClick={() => openPanel(est.studentId, est.fullName)}
                      className="text-xs text-blue-600 hover:text-blue-800 font-medium px-3 py-1.5 border border-blue-200 rounded-lg hover:bg-blue-50 transition-colors"
                    >
                      {adec ? 'Ver / Editar' : '+ Agregar'}
                    </button>
                  </td>
                </tr>
              );
            })}
            {estudiantes.length === 0 && (
              <tr><td colSpan={5} className="text-center py-12 text-gray-400">No hay estudiantes en este grupo</td></tr>
            )}
          </tbody>
        </table>
      </div>

      {/* Panel lateral */}
      {panelOpen && (
        <div className="fixed inset-0 z-50 flex">
          <button
            type="button"
            className="flex-1 bg-black/30"
            onClick={() => setPanelOpen(false)}
            aria-label="Cerrar panel"
          />
          <div
            role="dialog"
            aria-modal="true"
            aria-labelledby="panel-title"
            className="w-full max-w-xl bg-white shadow-2xl overflow-y-auto flex flex-col"
          >
            <div className="sticky top-0 bg-white border-b border-gray-200 px-6 py-4 flex items-center justify-between">
              <div>
                <h2 id="panel-title" className="font-semibold text-gray-900">{panelStudentName}</h2>
                <p className="text-xs text-gray-500">Adecuación curricular</p>
              </div>
              <button type="button" onClick={() => setPanelOpen(false)} aria-label="Cerrar panel" className="text-gray-400 hover:text-gray-600 text-xl leading-none"><span aria-hidden="true">✕</span></button>
            </div>

            <div className="px-6 py-5 flex-1 space-y-4">
              {panelError !== null && (
                <div role="alert" className="bg-red-50 border border-red-200 text-red-700 rounded-lg p-3 text-sm">{panelError}</div>
              )}

              {/* Tipo */}
              <div>
                <label htmlFor="field-tipo" className="block text-xs font-semibold text-gray-700 mb-1">Tipo de adecuación *</label>
                <select
                  id="field-tipo"
                  value={form.type}
                  onChange={e => setForm(f => ({ ...f, type: e.target.value as AccommodationType }))}
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-orange-400 focus:outline-none"
                >
                  {(Object.keys(ACCOMMODATION_TYPE_LABELS) as AccommodationType[]).map(t => (
                    <option key={t} value={t}>{ACCOMMODATION_TYPE_LABELS[t]}</option>
                  ))}
                </select>
                {form.type === 'AS' && (
                  <p className="mt-1 text-xs text-orange-600" role="note">
                    <span aria-hidden="true">⚠</span> La AS requiere registro formal en el SIMED del MEP. AulaIA genera el documento de soporte.
                  </p>
                )}
              </div>

              {/* Diagnóstico */}
              <div>
                <label htmlFor="field-diagnostico" className="block text-xs font-semibold text-gray-700 mb-1">Diagnóstico *</label>
                <input
                  id="field-diagnostico"
                  type="text"
                  value={form.diagnostico}
                  onChange={e => setForm(f => ({ ...f, diagnostico: e.target.value }))}
                  placeholder="Ej: TDAH, dislexia, discapacidad visual moderada…"
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-orange-400 focus:outline-none"
                  maxLength={500}
                />
              </div>

              {/* Condición especial */}
              <div>
                <label htmlFor="field-condicion" className="block text-xs font-semibold text-gray-700 mb-1">Condición especial (opcional)</label>
                <input
                  id="field-condicion"
                  type="text"
                  value={form.condicionEspecial}
                  onChange={e => setForm(f => ({ ...f, condicionEspecial: e.target.value }))}
                  placeholder="Descripción adicional de la condición"
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-orange-400 focus:outline-none"
                  maxLength={300}
                />
              </div>

              {/* Estrategias mediación */}
              <div>
                <label htmlFor="field-mediacion" className="block text-xs font-semibold text-gray-700 mb-1">Estrategias de mediación (opcional)</label>
                <textarea
                  id="field-mediacion"
                  rows={3}
                  value={form.estrategiasMediacion}
                  onChange={e => setForm(f => ({ ...f, estrategiasMediacion: e.target.value }))}
                  placeholder="Estrategias que el docente ya aplica o quiere aplicar…"
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-orange-400 focus:outline-none resize-none"
                />
              </div>

              {/* Estrategias evaluación */}
              <div>
                <label htmlFor="field-evaluacion" className="block text-xs font-semibold text-gray-700 mb-1">Estrategias de evaluación (opcional)</label>
                <textarea
                  id="field-evaluacion"
                  rows={3}
                  value={form.estrategiasEvaluacion}
                  onChange={e => setForm(f => ({ ...f, estrategiasEvaluacion: e.target.value }))}
                  placeholder="Ajustes en la forma de evaluar…"
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-orange-400 focus:outline-none resize-none"
                />
              </div>

              {/* Observaciones */}
              <div>
                <label htmlFor="field-observaciones" className="block text-xs font-semibold text-gray-700 mb-1">Observaciones del docente (opcional)</label>
                <textarea
                  id="field-observaciones"
                  rows={2}
                  value={form.observaciones}
                  onChange={e => setForm(f => ({ ...f, observaciones: e.target.value }))}
                  placeholder="Notas adicionales…"
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-orange-400 focus:outline-none resize-none"
                  maxLength={1000}
                />
              </div>

              {/* Propuesta generada */}
              {selected?.propuestaGenerada && (
                <div>
                  <h3 className="text-xs font-semibold text-gray-700 mb-1">Propuesta pedagógica generada por IA</h3>
                  <div className="bg-gray-50 border border-gray-200 rounded-lg p-3 text-xs text-gray-700 whitespace-pre-wrap max-h-64 overflow-y-auto">
                    {selected.propuestaGenerada}
                  </div>
                  {selected.generatedAt && (
                    <p className="text-xs text-gray-400 mt-1">Generada: {new Date(selected.generatedAt).toLocaleString('es-CR')}</p>
                  )}
                </div>
              )}

              {selected?.status === 'Failed' && selected.errorMessage && (
                <div role="alert" className="bg-red-50 border border-red-200 text-red-600 rounded-lg p-3 text-xs">
                  Error al generar: {selected.errorMessage}
                </div>
              )}
            </div>

            {/* Acciones */}
            <div className="sticky bottom-0 bg-white border-t border-gray-200 px-6 py-4 flex flex-wrap gap-2">
              <button
                type="button"
                onClick={handleSave}
                disabled={saving || !form.diagnostico.trim()}
                className="flex-1 bg-orange-600 text-white font-medium px-4 py-2 rounded-lg hover:bg-orange-700 disabled:opacity-50 disabled:cursor-not-allowed text-sm transition-colors"
              >
                {saving ? 'Guardando…' : 'Guardar'}
              </button>

              {selected && (
                <button
                  type="button"
                  onClick={handleGenerar}
                  disabled={generating || selected.status === 'Generating' || selected.status === 'Pending'}
                  className="flex-1 bg-blue-600 text-white font-medium px-4 py-2 rounded-lg hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed text-sm transition-colors"
                >
                  {generating || selected.status === 'Generating' ? '⏳ Generando…' : '✨ Generar propuesta IA'}
                </button>
              )}

              {selected?.status === 'Ready' && (
                <button
                  type="button"
                  onClick={handleDescargarPDF}
                  className="flex-1 bg-gray-100 text-gray-700 font-medium px-4 py-2 rounded-lg hover:bg-gray-200 text-sm transition-colors"
                >
                  ↓ Informe PDF (CAE)
                </button>
              )}

              {selected && (
                <button
                  type="button"
                  onClick={handleEliminar}
                  className="text-red-500 hover:text-red-700 text-sm px-3 py-2 rounded-lg hover:bg-red-50 transition-colors"
                >
                  Eliminar
                </button>
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
