'use client';

import { useAuth0 } from '@auth0/auth0-react';
import { useEffect, useState, useCallback, useMemo, use } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import {
  getCalendario,
  crearEventoCalendario,
  eliminarEventoCalendario,
  getLeccionesDisponibles,
  CALENDAR_EVENT_TYPES,
  CALENDAR_EVENT_LABELS,
  type CalendarEventResponse,
  type LeccionesDisponiblesResponse,
  type CalendarEventType,
} from '../../../lib/api';

const TYPE_COLOR: Record<CalendarEventType, string> = {
  Holiday:        'bg-red-100 text-red-700 border-red-300',
  Exam:           'bg-orange-100 text-orange-700 border-orange-300',
  TeacherMeeting: 'bg-blue-100 text-blue-700 border-blue-300',
  SportWeek:      'bg-green-100 text-green-700 border-green-300',
  Civic:          'bg-purple-100 text-purple-700 border-purple-300',
  Institutional:  'bg-yellow-100 text-yellow-700 border-yellow-300',
  Other:          'bg-gray-100 text-gray-600 border-gray-300',
};

function isoDate(y: number, m: number, d: number) {
  return `${y}-${String(m).padStart(2, '0')}-${String(d).padStart(2, '0')}`;
}

function todayStr() { return new Date().toISOString().slice(0, 10); }

function trimStr(year: number, tri: number): { from: string; to: string } {
  if (tri === 1) return { from: `${year}-02-09`, to: `${year}-04-03` };
  if (tri === 2) return { from: `${year}-04-14`, to: `${year}-06-26` };
  return            { from: `${year}-07-13`, to: `${year}-11-27` };
}

const MONTH_NAMES = ['Enero','Febrero','Marzo','Abril','Mayo','Junio',
                     'Julio','Agosto','Setiembre','Octubre','Noviembre','Diciembre'];
const DAY_NAMES = ['L','M','X','J','V','S','D'];

// Constantes de fecha de carga de módulo — no cambian durante la sesión
const NOW        = new Date();
const YEAR_NOW   = NOW.getFullYear();

export default function CalendarioPage({ params }: { params: Promise<{ grupoId: string }> }) {
  const { grupoId } = use(params);
  const { isAuthenticated, isLoading, loginWithRedirect, getAccessTokenSilently } = useAuth0();
  const router      = useRouter();
  const searchParams = useSearchParams();
  const nombre = searchParams.get('nombre') ?? 'Grupo';

  const [viewYear,  setViewYear]  = useState(NOW.getFullYear());
  const [viewMonth, setViewMonth] = useState(NOW.getMonth() + 1); // 1-12

  const [events,  setEvents]  = useState<CalendarEventResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving,  setSaving]  = useState(false);
  const [error,   setError]   = useState<string | null>(null);
  // Incrementar para forzar recarga tras mutaciones (sin llamar setState en efectos)
  const [reloadTrigger, setReloadTrigger] = useState(0);
  const reload = useCallback(() => setReloadTrigger(t => t + 1), []);

  // Form agregar evento
  const [form, setForm] = useState({ date: todayStr(), endDate: '', title: '', type: 'Holiday' as CalendarEventType });

  // Panel lecciones
  const [lecForm, setLecForm] = useState({ from: trimStr(YEAR_NOW, 1).from, to: trimStr(YEAR_NOW, 1).to, lps: 4 });
  const [lecResult, setLecResult] = useState<LeccionesDisponiblesResponse | null>(null);
  const [lecLoading, setLecLoading] = useState(false);

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
        setLoading(true);
        const data = await getCalendario(token, grupoId, viewYear);
        if (cancelled) return;
        setEvents(data);
        setError(null);
      } catch (e) {
        if (!cancelled) setError(String(e));
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();

    return () => { cancelled = true; };
  }, [isAuthenticated, isLoading, loginWithRedirect, getAccessTokenSilently, grupoId, viewYear, viewMonth, reloadTrigger]);

  const handleAddEvent = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!form.title.trim()) return;
    try {
      setSaving(true);
      const token = await getAccessTokenSilently();
      await crearEventoCalendario(token, grupoId, {
        date: form.date,
        endDate: form.endDate || null,
        title: form.title,
        type: form.type,
      });
      setForm({ date: todayStr(), endDate: '', title: '', type: 'Holiday' });
      reload();
    } catch (e) {
      setError(String(e));
    } finally {
      setSaving(false);
    }
  };

  // useCallback: referencia estable para los botones de eliminar en la lista
  const handleDelete = useCallback(async (id: string) => {
    if (!confirm('¿Eliminar este evento del calendario?')) return;
    try {
      const token = await getAccessTokenSilently();
      await eliminarEventoCalendario(token, grupoId, id);
      reload();
    } catch (e) {
      setError(String(e));
    }
  }, [getAccessTokenSilently, grupoId, reload]);

  const handleCalcLecciones = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      setLecLoading(true);
      const token = await getAccessTokenSilently();
      const res = await getLeccionesDisponibles(token, grupoId, lecForm.from, lecForm.to, lecForm.lps);
      setLecResult(res);
    } catch (e) {
      setError(String(e));
    } finally {
      setLecLoading(false);
    }
  };

  // ── Construir grilla mensual ──────────────────────────────────────────────
  const daysInMonth    = new Date(viewYear, viewMonth, 0).getDate();
  const firstDayOfWeek = new Date(viewYear, viewMonth - 1, 1).getDay();
  const offset         = firstDayOfWeek === 0 ? 6 : firstDayOfWeek - 1;

  // useMemo: evitar reconstruir el mapa en cada render salvo que events cambie
  const eventsByDate = useMemo(() => {
    const map = new Map<string, CalendarEventResponse[]>();
    for (const evt of events) {
      const start = new Date(evt.date + 'T00:00:00');
      const end   = evt.endDate ? new Date(evt.endDate + 'T00:00:00') : start;
      for (let d = new Date(start); d <= end; d.setDate(d.getDate() + 1)) {
        const key = d.toISOString().slice(0, 10);
        const existing = map.get(key);
        if (existing) {
          existing.push(evt);
        } else {
          map.set(key, [evt]);
        }
      }
    }
    return map;
  }, [events]);

  const todayIso = todayStr();

  // useCallback: instancias estables para prevMonth/nextMonth
  const prevMonth = useCallback(() => {
    if (viewMonth === 1) { setViewYear(y => y - 1); setViewMonth(12); }
    else setViewMonth(m => m - 1);
  }, [viewMonth]);

  const nextMonth = useCallback(() => {
    if (viewMonth === 12) { setViewYear(y => y + 1); setViewMonth(1); }
    else setViewMonth(m => m + 1);
  }, [viewMonth]);

  // useMemo: evitar re-filtrar en cada render salvo que events o viewMonth cambien
  const eventsThisMonth = useMemo(() =>
    events.filter(e => parseInt(e.date.split('-')[1], 10) === viewMonth),
    [events, viewMonth]
  );

  if (isLoading) return <div className="p-8 text-gray-500">Cargando...</div>;

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <div className="bg-indigo-600 text-white px-4 py-4">
        <div className="max-w-6xl mx-auto flex items-center gap-3">
          <button type="button" onClick={() => router.back()} className="text-indigo-200 hover:text-white text-sm">← Volver</button>
          <div>
            <h1 className="text-xl font-bold">📅 Calendario escolar</h1>
            <p className="text-indigo-200 text-sm">{nombre}</p>
          </div>
        </div>
      </div>

      <div className="max-w-6xl mx-auto px-4 py-6 grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* ── Columna izquierda: calendario grilla ── */}
        <div className="lg:col-span-2 space-y-4">
          {/* Navegación mes */}
          <div className="bg-white rounded-xl shadow-sm p-4">
            <div className="flex items-center justify-between mb-4">
              <button type="button" aria-label="Mes anterior" onClick={prevMonth} className="px-3 py-1 text-gray-600 hover:text-gray-900 text-lg" aria-controls="calendar-grid">‹</button>
              <h2 id="calendar-heading" className="text-lg font-semibold text-gray-800">
                {MONTH_NAMES[viewMonth - 1]} {viewYear}
              </h2>
              <button type="button" aria-label="Mes siguiente" onClick={nextMonth} className="px-3 py-1 text-gray-600 hover:text-gray-900 text-lg" aria-controls="calendar-grid">›</button>
            </div>

            {/* Cabecera días */}
            <div className="grid grid-cols-7 mb-1">
              {DAY_NAMES.map(d => (
                <div key={d} className={`text-center text-xs font-semibold py-1 ${d === 'S' || d === 'D' ? 'text-gray-400' : 'text-gray-600'}`}>{d}</div>
              ))}
            </div>

            {/* Grilla */}
            <div id="calendar-grid" aria-labelledby="calendar-heading" className="grid grid-cols-7 gap-px bg-gray-200 rounded-lg overflow-hidden">
              {/* Celdas vacías al inicio */}
              {Array.from({ length: offset }).map((_, i) => (
                <div key={`empty-${i}`} aria-hidden="true" className="bg-gray-50 min-h-[60px]" />
              ))}

              {Array.from({ length: daysInMonth }, (_, i) => i + 1).map(day => {
                const iso = isoDate(viewYear, viewMonth, day);
                const dayEvents = eventsByDate.get(iso) ?? [];
                const isToday = iso === todayIso;
                const dow = new Date(iso + 'T00:00:00').getDay();
                const isWeekend = dow === 0 || dow === 6;

                return (
                  <div
                    key={day}
                    className={`bg-white min-h-[60px] p-1 ${isWeekend ? 'bg-gray-50' : ''}`}
                  >
                    <span className={`text-xs font-medium block text-right mb-1 w-6 h-6 flex items-center justify-center ml-auto rounded-full
                      ${isToday ? 'bg-indigo-600 text-white' : isWeekend ? 'text-gray-400' : 'text-gray-700'}`}>
                      {day}
                    </span>
                    {dayEvents.slice(0, 2).map(evt => (
                      <div key={evt.id}
                           className={`text-[10px] px-1 rounded border truncate mb-0.5 ${TYPE_COLOR[evt.type as CalendarEventType] ?? TYPE_COLOR.Other}`}
                           title={evt.title}>
                        {evt.isNational ? '🔒 ' : ''}{evt.title}
                      </div>
                    ))}
                    {dayEvents.length > 2 && (
                      <div className="text-[10px] text-gray-400">+{dayEvents.length - 2}</div>
                    )}
                  </div>
                );
              })}
            </div>
          </div>

          {/* Leyenda de tipos */}
          <div className="bg-white rounded-xl shadow-sm p-4">
            <h3 className="text-sm font-semibold text-gray-700 mb-2">Tipos de eventos</h3>
            <div className="flex flex-wrap gap-2">
              {CALENDAR_EVENT_TYPES.map(t => (
                <span key={t} className={`text-xs px-2 py-1 rounded border ${TYPE_COLOR[t]}`}>
                  {CALENDAR_EVENT_LABELS[t]}
                </span>
              ))}
            </div>
          </div>

          {/* Lista eventos del mes */}
          <div className="bg-white rounded-xl shadow-sm p-4">
            <h3 className="text-sm font-semibold text-gray-700 mb-3">
              Eventos en {MONTH_NAMES[viewMonth - 1]} ({eventsThisMonth.length})
            </h3>
            {loading ? (
              <p role="status" aria-live="polite" className="text-gray-400 text-sm">Cargando…</p>
            ) : eventsThisMonth.length === 0 ? (
              <p className="text-gray-400 text-sm">Sin eventos este mes.</p>
            ) : (
              <div className="space-y-2">
                {eventsThisMonth.map(evt => (
                  <div key={evt.id} className="flex items-start justify-between gap-2">
                    <div className="flex items-start gap-2 min-w-0">
                      <span className={`text-xs px-2 py-0.5 rounded border mt-0.5 shrink-0 ${TYPE_COLOR[evt.type as CalendarEventType] ?? TYPE_COLOR.Other}`}>
                        {CALENDAR_EVENT_LABELS[evt.type as CalendarEventType] ?? evt.type}
                      </span>
                      <div className="min-w-0">
                        <p className="text-sm text-gray-800 font-medium truncate">{evt.title}</p>
                        <p className="text-xs text-gray-500">
                          {evt.date}{evt.endDate && evt.endDate !== evt.date ? ` → ${evt.endDate}` : ''}
                          {evt.isNational ? ' · 🔒 Nacional' : null}
                        </p>
                      </div>
                    </div>
                    {evt.isEditable && (
                      <button
                        type="button"
                        aria-label={`Eliminar evento: ${evt.title}`}
                        onClick={() => handleDelete(evt.id)}
                        className="text-xs text-red-400 hover:text-red-600 shrink-0 mt-0.5"
                      ><span aria-hidden="true">✕</span></button>
                    )}
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>

        {/* ── Columna derecha: formularios ── */}
        <div className="space-y-5">
          {/* Error */}
          {error && (
            <div role="alert" className="bg-red-50 border border-red-200 text-red-700 rounded-xl p-3 text-sm">
              {error}
              <button type="button" aria-label="Cerrar aviso de error" onClick={() => setError(null)} className="ml-2 text-red-400 hover:text-red-600"><span aria-hidden="true">✕</span></button>
            </div>
          )}

          {/* Agregar evento */}
          <div className="bg-white rounded-xl shadow-sm p-4">
            <h3 className="font-semibold text-gray-800 mb-3">➕ Agregar evento</h3>
            <form onSubmit={handleAddEvent} className="space-y-3">
              <div>
                <label htmlFor="evt-date" className="block text-xs text-gray-600 mb-1">Fecha inicio</label>
                <input id="evt-date" type="date" value={form.date}
                  onChange={e => setForm(f => ({ ...f, date: e.target.value }))}
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm" required />
              </div>
              <div>
                <label htmlFor="evt-end-date" className="block text-xs text-gray-600 mb-1">Fecha fin <span className="text-gray-400">(opcional, para rangos)</span></label>
                <input id="evt-end-date" type="date" value={form.endDate}
                  onChange={e => setForm(f => ({ ...f, endDate: e.target.value }))}
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm" />
              </div>
              <div>
                <label htmlFor="evt-type" className="block text-xs text-gray-600 mb-1">Tipo</label>
                <select id="evt-type" value={form.type}
                  onChange={e => setForm(f => ({ ...f, type: e.target.value as CalendarEventType }))}
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm">
                  {CALENDAR_EVENT_TYPES.map(t => (
                    <option key={t} value={t}>{CALENDAR_EVENT_LABELS[t]}</option>
                  ))}
                </select>
              </div>
              <div>
                <label htmlFor="evt-title" className="block text-xs text-gray-600 mb-1">Descripción</label>
                <input id="evt-title" type="text" value={form.title} placeholder="ej. Día del padre"
                  onChange={e => setForm(f => ({ ...f, title: e.target.value }))}
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm" required />
              </div>
              <button type="submit" disabled={saving}
                className="w-full bg-indigo-600 hover:bg-indigo-700 disabled:opacity-50 text-white rounded-lg px-4 py-2 text-sm font-medium">
                {saving ? 'Guardando…' : 'Agregar evento'}
              </button>
            </form>
          </div>

          {/* Calcular lecciones disponibles */}
          <div className="bg-white rounded-xl shadow-sm p-4">
            <h3 className="font-semibold text-gray-800 mb-1">📊 Lecciones disponibles</h3>
            <p className="text-xs text-gray-500 mb-3">Calcula cuántas lecciones hay en un período, descontando días no lectivos.</p>
            <form onSubmit={handleCalcLecciones} className="space-y-3">
              <div className="grid grid-cols-3 gap-2 text-xs text-center mb-1">
                {[1, 2, 3].map(tri => (
                  <button key={tri} type="button"
                    onClick={() => setLecForm(f => ({ ...f, ...trimStr(YEAR_NOW, tri) }))}
                    className="border border-indigo-300 text-indigo-600 rounded-lg py-1 hover:bg-indigo-50">
                    Trim. {tri}
                  </button>
                ))}
              </div>
              <div>
                <label htmlFor="lec-from" className="block text-xs text-gray-600 mb-1">Fecha inicio</label>
                <input id="lec-from" type="date" value={lecForm.from}
                  onChange={e => setLecForm(f => ({ ...f, from: e.target.value }))}
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm" required />
              </div>
              <div>
                <label htmlFor="lec-to" className="block text-xs text-gray-600 mb-1">Fecha fin</label>
                <input id="lec-to" type="date" value={lecForm.to}
                  onChange={e => setLecForm(f => ({ ...f, to: e.target.value }))}
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm" required />
              </div>
              <div>
                <label htmlFor="lec-lps" className="block text-xs text-gray-600 mb-1">Lecciones por semana</label>
                <input id="lec-lps" type="number" min={1} max={40} value={lecForm.lps}
                  onChange={e => setLecForm(f => ({ ...f, lps: parseInt(e.target.value, 10) || 1 }))}
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm" required />
              </div>
              <button type="submit" disabled={lecLoading}
                className="w-full bg-emerald-600 hover:bg-emerald-700 disabled:opacity-50 text-white rounded-lg px-4 py-2 text-sm font-medium">
                {lecLoading ? 'Calculando…' : 'Calcular'}
              </button>
            </form>

            {lecResult && (
              <div className="mt-4 bg-emerald-50 border border-emerald-200 rounded-xl p-3 space-y-1 text-sm">
                <div className="flex justify-between">
                  <span className="text-gray-600">Días hábiles (L-V):</span>
                  <span className="font-medium">{lecResult.diasHabiles}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-gray-600">Días no lectivos:</span>
                  <span className="font-medium text-red-600">−{lecResult.diasNoLectivos}</span>
                </div>
                <div className="flex justify-between border-t border-emerald-200 pt-1">
                  <span className="text-gray-600">Días efectivos:</span>
                  <span className="font-medium">{lecResult.diasEfectivos}</span>
                </div>
                <div className="flex justify-between text-lg font-bold text-emerald-700 border-t border-emerald-300 pt-1">
                  <span>Lecciones disponibles:</span>
                  <span>{lecResult.leccionesDisponibles}</span>
                </div>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
