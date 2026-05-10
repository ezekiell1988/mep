'use client';

import { useAuth0 } from '@auth0/auth0-react';
import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import {
  getAdminPagosPendientes,
  getAdminPagosHistorial,
  aprobarPago,
  rechazarPago,
  getAdminSuscripciones,
  getAdminComisiones,
  ejecutarCierreMensual,
  marcarComisionPagada,
  uploadCurriculumPdf,
  type AdminPagoResponse,
  type AdminSuscripcionResponse,
  type AdminComisionResponse,
  type UploadCurriculumResponse,
} from '../../lib/api';

type Tab = 'pagos' | 'suscripciones' | 'cierre' | 'comisiones' | 'curriculum';

const ASIGNATURAS = [
  'Artes Plásticas',
  'Artes Musicales',
  'Educación para el Hogar',
  'Educación Física',
  'Matemáticas',
  'Español',
  'Ciencias',
  'Estudios Sociales',
  'Inglés',
  'Francés',
  'Orientación',
] as const;

const CICLOS = ['III Ciclo', 'I y II Ciclo', 'II Ciclo'] as const;

export default function AdminClient() {
  const { isAuthenticated, isLoading, loginWithRedirect, getAccessTokenSilently } = useAuth0();
  const router = useRouter();
  const [tab, setTab] = useState<Tab>('pagos');

  const [pendientes, setPendientes] = useState<AdminPagoResponse[]>([]);
  const [historial, setHistorial] = useState<AdminPagoResponse[]>([]);
  const [suscripciones, setSuscripciones] = useState<AdminSuscripcionResponse[]>([]);
  const [comisiones, setComisiones] = useState<AdminComisionResponse[]>([]);

  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [successMsg, setSuccessMsg] = useState<string | null>(null);

  // Curriculum upload form
  const [currAsignatura, setCurrAsignatura] = useState<string>(ASIGNATURAS[0]);
  const [currCiclo, setCurrCiclo] = useState<string>(CICLOS[0]);
  const [currFile, setCurrFile] = useState<File | null>(null);
  const [currResult, setCurrResult] = useState<UploadCurriculumResponse | null>(null);

  // Cierre mensual form
  const [cierreMonth, setCierreMonth] = useState(() => {
    const d = new Date();
    return `${d.getFullYear()}${String(d.getMonth() + 1).padStart(2, '0')}`;
  });
  const [infraCost, setInfraCost] = useState('');

  useEffect(() => {
    if (isLoading) return;
    if (!isAuthenticated) { void loginWithRedirect(); return; }

    let cancelled = false;
    void (async () => {
      try {
        const token = await getAccessTokenSilently();
        if (cancelled) return;
        const [pend, hist, subs, coms] = await Promise.all([
          getAdminPagosPendientes(token),
          getAdminPagosHistorial(token),
          getAdminSuscripciones(token),
          getAdminComisiones(token),
        ]);
        if (cancelled) return;
        setPendientes(pend);
        setHistorial(hist);
        setSuscripciones(subs);
        setComisiones(coms);
      } catch (e: unknown) {
        if (!cancelled) setError(e instanceof Error ? e.message : 'Error al cargar datos admin');
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();
    return () => { cancelled = true; };
  }, [isAuthenticated, isLoading, loginWithRedirect, getAccessTokenSilently]);

  const handleAprobar = async (id: string) => {
    setSaving(true);
    setError(null);
    try {
      const token = await getAccessTokenSilently();
      await aprobarPago(token, id);
      setPendientes(prev => prev.filter(p => p.id !== id));
      setSuccessMsg('Pago aprobado. Suscripción activada.');
    } catch (e: unknown) {
      setError(e instanceof Error ? e.message : 'Error aprobando pago');
    } finally {
      setSaving(false);
    }
  };

  const handleRechazar = async (id: string) => {
    const nota = window.prompt('Motivo del rechazo (requerido):');
    if (!nota?.trim()) return;
    setSaving(true);
    setError(null);
    try {
      const token = await getAccessTokenSilently();
      await rechazarPago(token, id, nota);
      setPendientes(prev => prev.filter(p => p.id !== id));
      setSuccessMsg('Pago rechazado.');
    } catch (e: unknown) {
      setError(e instanceof Error ? e.message : 'Error rechazando pago');
    } finally {
      setSaving(false);
    }
  };

  const handleCierreMensual = async () => {
    const month = parseInt(cierreMonth, 10);
    const infra = parseFloat(infraCost);
    if (isNaN(month) || isNaN(infra) || infra <= 0) {
      setError('Ingresá un período YYYYMM y un costo de infraestructura válido.');
      return;
    }
    setSaving(true);
    setError(null);
    try {
      const token = await getAccessTokenSilently();
      await ejecutarCierreMensual(token, month, infra);
      setSuccessMsg(`Job de cierre para ${cierreMonth} encolado. Las comisiones estarán listas en breve.`);
    } catch (e: unknown) {
      setError(e instanceof Error ? e.message : 'Error ejecutando cierre mensual');
    } finally {
      setSaving(false);
    }
  };

  const handleUploadCurriculum = async () => {
    if (!currFile) { setError('Seleccioná un archivo PDF.'); return; }
    setSaving(true);
    setError(null);
    setCurrResult(null);
    try {
      const token = await getAccessTokenSilently();
      const result = await uploadCurriculumPdf(token, currAsignatura, currCiclo, currFile);
      setCurrResult(result);
      setCurrFile(null);
      setSuccessMsg(`Job de extracción encolado (ID: ${result.jobId}). Revisá Hangfire para ver el progreso.`);
    } catch (e: unknown) {
      setError(e instanceof Error ? e.message : 'Error subiendo PDF');
    } finally {
      setSaving(false);
    }
  };

  const handleMarcarPagada = async (id: string) => {
    setSaving(true);
    try {
      const token = await getAccessTokenSilently();
      await marcarComisionPagada(token, id);
      setComisiones(prev => prev.map(c => c.id === id ? { ...c, status: 'Paid' as const } : c));
    } catch (e: unknown) {
      setError(e instanceof Error ? e.message : 'Error marcando comisión');
    } finally {
      setSaving(false);
    }
  };

  if (isLoading || loading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div
          role="status"
          aria-label="Cargando panel de administración"
          className="w-8 h-8 border-4 border-blue-600 border-t-transparent rounded-full animate-spin"
        />
      </div>
    );
  }

  const TABS: { id: Tab; label: string }[] = [
    { id: 'pagos', label: `Pagos pendientes (${pendientes.length})` },
    { id: 'suscripciones', label: 'Suscripciones' },
    { id: 'cierre', label: 'Cierre mensual' },
    { id: 'comisiones', label: 'Comisiones' },
    { id: 'curriculum', label: 'Curriculum PDF' },
  ];

  return (
    <div className="min-h-screen bg-gray-50">
      <header className="bg-white border-b border-gray-200 px-4 py-4">
        <div className="max-w-6xl mx-auto flex items-center justify-between">
          <button onClick={() => router.push('/dashboard')} className="text-xl font-bold text-blue-600">
            AulaIA — Admin
          </button>
        </div>
      </header>

      <main className="max-w-6xl mx-auto px-4 py-8">
        <h1 className="text-2xl font-bold text-gray-900 mb-6">Panel de administración</h1>

        {/* Alerts */}
        {error && (
          <div role="alert" className="mb-4 p-4 bg-red-50 border border-red-200 rounded-lg text-red-700 text-sm">
            {error}
          </div>
        )}
        {successMsg && (
          <div role="status" className="mb-4 p-4 bg-green-50 border border-green-200 rounded-lg text-green-700 text-sm">
            {successMsg}
          </div>
        )}

        {/* Tabs */}
        <div className="flex gap-1 border-b border-gray-200 mb-6" role="tablist" aria-label="Secciones del panel admin">
          {TABS.map(t =>
            tab === t.id ? (
              <button
                key={t.id}
                id={`tab-${t.id}`}
                type="button"
                role="tab"
                aria-selected="true"
                aria-controls={`panel-${t.id}`}
                onClick={() => setTab(t.id)}
                className="px-4 py-2 text-sm font-medium rounded-t-lg transition-colors bg-white border border-gray-200 border-b-white -mb-px text-blue-600"
              >
                {t.label}
              </button>
            ) : (
              <button
                key={t.id}
                id={`tab-${t.id}`}
                type="button"
                role="tab"
                aria-selected="false"
                aria-controls={`panel-${t.id}`}
                onClick={() => setTab(t.id)}
                className="px-4 py-2 text-sm font-medium rounded-t-lg transition-colors text-gray-500 hover:text-gray-700"
              >
                {t.label}
              </button>
            )
          )}
        </div>

        {/* Tab: Pagos */}
        {tab === 'pagos' && (
          <section
            role="tabpanel"
            id="panel-pagos"
            tabIndex={0}
            aria-labelledby="pagos-heading"
          >
            <h2 id="pagos-heading" className="text-lg font-semibold text-gray-900 mb-4">
              Pagos pendientes de aprobación
            </h2>
            {pendientes.length === 0 ? (
              <p className="text-gray-500 text-sm">Sin pagos pendientes. <span aria-hidden="true">✅</span></p>
            ) : (
              <div className="space-y-4">
                {pendientes.map(p => (
                  <div key={p.id} className="bg-white rounded-xl border border-gray-200 p-5">
                    <div className="flex flex-wrap items-start justify-between gap-3">
                      <div>
                        <p className="font-semibold text-gray-900">{p.userName}</p>
                        <p className="text-sm text-gray-500">{p.userEmail}</p>
                        <p className="text-sm mt-1">
                          Plan: <strong>{p.plan}</strong> — $
                          {p.amountUsd} (₡{p.amountCrc.toLocaleString('es-CR', { minimumFractionDigits: 0 })})
                        </p>
                        <p className="text-sm text-gray-500">
                          Código:{' '}
                          <code className="bg-gray-100 px-1 rounded font-mono">{p.referenceCode}</code>
                        </p>
                        <p className="text-sm text-gray-400">
                          Solicitado: {new Date(p.createdAt).toLocaleString('es-CR')}
                          {p.hasVoucher && <span className="ml-2 text-green-600"><span aria-hidden="true">📎</span>{' '}Tiene comprobante</span>}
                        </p>
                      </div>
                      <div className="flex gap-2">
                        <button
                          type="button"
                          onClick={() => void handleAprobar(p.id)}
                          disabled={saving}
                          className="px-3 py-1.5 bg-green-600 text-white text-sm rounded-lg hover:bg-green-700 disabled:opacity-50"
                        >
                          <span aria-hidden="true">✓</span>{' '}Aprobar
                        </button>
                        <button
                          type="button"
                          onClick={() => void handleRechazar(p.id)}
                          disabled={saving}
                          className="px-3 py-1.5 bg-red-600 text-white text-sm rounded-lg hover:bg-red-700 disabled:opacity-50"
                        >
                          <span aria-hidden="true">✗</span>{' '}Rechazar
                        </button>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            )}

            {/* Historial */}
            {historial.length > 0 && (
              <>
                <h2 className="text-lg font-semibold text-gray-900 mt-8 mb-4">Historial de pagos</h2>
                <div className="overflow-x-auto">
                  <table className="w-full text-sm text-left">
                    <thead>
                      <tr className="border-b border-gray-200 text-gray-500">
                        <th scope="col" className="py-2 pr-4">Usuario</th>
                        <th scope="col" className="py-2 pr-4">Plan</th>
                        <th scope="col" className="py-2 pr-4">Monto</th>
                        <th scope="col" className="py-2 pr-4">Código</th>
                        <th scope="col" className="py-2 pr-4">Estado</th>
                        <th scope="col" className="py-2">Revisado</th>
                      </tr>
                    </thead>
                    <tbody>
                      {historial.map(p => (
                        <tr key={p.id} className="border-b border-gray-100">
                          <td className="py-2 pr-4">{p.userName}</td>
                          <td className="py-2 pr-4">{p.plan}</td>
                          <td className="py-2 pr-4">${p.amountUsd}</td>
                          <td className="py-2 pr-4 font-mono text-xs">{p.referenceCode}</td>
                          <td className="py-2 pr-4">
                            <span className={p.status === 'Approved' ? 'text-green-600' : 'text-red-600'}>
                              {p.status === 'Approved' ? 'Aprobado' : 'Rechazado'}
                            </span>
                          </td>
                          <td className="py-2 text-gray-400">
                            {p.reviewedAt ? new Date(p.reviewedAt).toLocaleDateString('es-CR') : '—'}
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </>
            )}
          </section>
        )}

        {/* Tab: Suscripciones */}
        {tab === 'suscripciones' && (
          <section
            role="tabpanel"
            id="panel-suscripciones"
            tabIndex={0}
            aria-labelledby="subs-heading"
          >
            <h2 id="subs-heading" className="text-lg font-semibold text-gray-900 mb-4">
              Suscripciones activas
            </h2>
            {suscripciones.length === 0 ? (
              <p className="text-gray-500 text-sm">Sin suscripciones.</p>
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full text-sm text-left">
                  <thead>
                    <tr className="border-b border-gray-200 text-gray-500">
                      <th scope="col" className="py-2 pr-4">Usuario</th>
                      <th scope="col" className="py-2 pr-4">Plan</th>
                      <th scope="col" className="py-2 pr-4">Estado</th>
                      <th scope="col" className="py-2 pr-4">Trial</th>
                      <th scope="col" className="py-2 pr-4">Vence</th>
                      <th scope="col" className="py-2">Días restantes</th>
                    </tr>
                  </thead>
                  <tbody>
                    {suscripciones.map(s => (
                      <tr key={s.id} className="border-b border-gray-100">
                        <td className="py-2 pr-4">
                          <p className="font-medium">{s.userName}</p>
                          <p className="text-gray-400 text-xs">{s.userEmail}</p>
                        </td>
                        <td className="py-2 pr-4">{s.plan}</td>
                        <td className="py-2 pr-4">
                          <span className={s.status === 'Active' ? 'text-green-600' : 'text-red-500'}>
                            {s.status === 'Active' ? 'Activo' : 'Expirado'}
                          </span>
                        </td>
                        <td className="py-2 pr-4">{s.isTrial ? '✓ Trial' : '—'}</td>
                        <td className="py-2 pr-4">{new Date(s.periodEnd).toLocaleDateString('es-CR')}</td>
                        <td className="py-2">
                          <span className={s.daysRemaining <= 7 ? 'text-red-600 font-semibold' : 'text-gray-700'}>
                            {s.daysRemaining}d
                          </span>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </section>
        )}

        {/* Tab: Cierre mensual */}
        {tab === 'cierre' && (
          <section
            role="tabpanel"
            id="panel-cierre"
            tabIndex={0}
            aria-labelledby="cierre-heading"
            className="max-w-md"
          >
            <h2 id="cierre-heading" className="text-lg font-semibold text-gray-900 mb-2">
              Cierre mensual — cálculo de comisiones
            </h2>
            <p className="text-sm text-gray-600 mb-6">
              Ingresá el costo de infraestructura Azure del mes y ejecutá el cálculo de comisiones.
              Se generarán las comisiones para todos los usuarios referidos activos.
            </p>

            <div className="bg-white rounded-xl border border-gray-200 p-6 space-y-4">
              <div>
                <label htmlFor="cierre-month" className="block text-sm font-medium text-gray-700 mb-1">
                  Período (YYYYMM)
                </label>
                <input
                  id="cierre-month"
                  type="text"
                  value={cierreMonth}
                  onChange={e => setCierreMonth(e.target.value)}
                  placeholder="202606"
                  maxLength={6}
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>

              <div>
                <label htmlFor="infra-cost" className="block text-sm font-medium text-gray-700 mb-1">
                  Costo infraestructura Azure del mes (CRC)
                </label>
                <input
                  id="infra-cost"
                  type="number"
                  value={infraCost}
                  onChange={e => setInfraCost(e.target.value)}
                  placeholder="80000"
                  min="0"
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
                <p className="text-xs text-gray-400 mt-1">
                  Este monto se descuenta de los ingresos brutos antes de calcular comisiones (ADR-008).
                </p>
              </div>

              <button
                type="button"
                onClick={() => void handleCierreMensual()}
                disabled={saving}
                className="w-full py-2.5 bg-blue-600 text-white rounded-lg font-semibold text-sm hover:bg-blue-700 disabled:opacity-50"
              >
                {saving ? 'Ejecutando...' : 'Ejecutar cierre mensual'}
              </button>
            </div>
          </section>
        )}

        {/* Tab: Comisiones */}
        {tab === 'comisiones' && (
          <section
            role="tabpanel"
            id="panel-comisiones"
            tabIndex={0}
            aria-labelledby="comisiones-heading"
          >
            <h2 id="comisiones-heading" className="text-lg font-semibold text-gray-900 mb-4">
              Comisiones de referidos
            </h2>
            {comisiones.length === 0 ? (
              <p className="text-gray-500 text-sm">Sin comisiones calculadas todavía.</p>
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full text-sm text-left">
                  <thead>
                    <tr className="border-b border-gray-200 text-gray-500">
                      <th scope="col" className="py-2 pr-4">Referidor</th>
                      <th scope="col" className="py-2 pr-4">Código</th>
                      <th scope="col" className="py-2 pr-4">Referido</th>
                      <th scope="col" className="py-2 pr-4">Período</th>
                      <th scope="col" className="py-2 pr-4">Comisión (CRC)</th>
                      <th scope="col" className="py-2 pr-4">Estado</th>
                      <th scope="col" className="py-2">Acción</th>
                    </tr>
                  </thead>
                  <tbody>
                    {comisiones.map(c => (
                      <tr key={c.id} className="border-b border-gray-100">
                        <td className="py-2 pr-4">{c.referidorNombre}</td>
                        <td className="py-2 pr-4 font-mono text-xs">{c.codigoReferido}</td>
                        <td className="py-2 pr-4">{c.referidoNombre}</td>
                        <td className="py-2 pr-4">{String(c.month)}</td>
                        <td className="py-2 pr-4 font-semibold">
                          ₡{c.commissionAmountCrc.toLocaleString('es-CR', { minimumFractionDigits: 0 })}
                        </td>
                        <td className="py-2 pr-4">
                          <span className={c.status === 'Paid' ? 'text-green-600' : 'text-amber-600'}>
                            {c.status === 'Paid' ? 'Pagada' : 'Pendiente'}
                          </span>
                        </td>
                        <td className="py-2">
                          {c.status === 'Pending' && (
                            <button
                              type="button"
                              onClick={() => void handleMarcarPagada(c.id)}
                              disabled={saving}
                              className="px-2 py-1 bg-green-100 text-green-700 text-xs rounded hover:bg-green-200 disabled:opacity-50"
                            >
                              Marcar pagada
                            </button>
                          )}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </section>
        )}
        {/* Tab: Curriculum */}
        {tab === 'curriculum' && (
          <section
            role="tabpanel"
            id="panel-curriculum"
            tabIndex={0}
            aria-labelledby="curriculum-heading"
            className="max-w-lg"
          >
            <h2 id="curriculum-heading" className="text-lg font-semibold text-gray-900 mb-2">
              Subir PDF de programa curricular MEP
            </h2>
            <p className="text-sm text-gray-600 mb-6">
              Sube manualmente un PDF del MEP para extraer las unidades curriculares con GPT-5.5.
              El job se encola en Hangfire (cola <code className="bg-gray-100 px-1 rounded">curriculum</code>).
            </p>

            <div className="bg-white rounded-xl border border-gray-200 p-6 space-y-4">
              <div>
                <label htmlFor="curr-asignatura" className="block text-sm font-medium text-gray-700 mb-1">
                  Asignatura
                </label>
                <select
                  id="curr-asignatura"
                  value={currAsignatura}
                  onChange={e => setCurrAsignatura(e.target.value)}
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                >
                  {ASIGNATURAS.map(a => (
                    <option key={a} value={a}>{a}</option>
                  ))}
                </select>
              </div>

              <div>
                <label htmlFor="curr-ciclo" className="block text-sm font-medium text-gray-700 mb-1">
                  Ciclo
                </label>
                <select
                  id="curr-ciclo"
                  value={currCiclo}
                  onChange={e => setCurrCiclo(e.target.value)}
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                >
                  {CICLOS.map(c => (
                    <option key={c} value={c}>{c}</option>
                  ))}
                </select>
              </div>

              <div>
                <label htmlFor="curr-file" className="block text-sm font-medium text-gray-700 mb-1">
                  Archivo PDF (máx. 50 MB)
                </label>
                <input
                  id="curr-file"
                  type="file"
                  accept="application/pdf"
                  onChange={e => setCurrFile(e.target.files?.[0] ?? null)}
                  className="w-full text-sm text-gray-700 file:mr-3 file:py-1.5 file:px-3 file:rounded file:border-0 file:bg-blue-50 file:text-blue-700 file:text-sm hover:file:bg-blue-100"
                />
                {currFile && (
                  <p className="text-xs text-gray-500 mt-1">
                    {currFile.name} ({(currFile.size / 1024 / 1024).toFixed(2)} MB)
                  </p>
                )}
              </div>

              <button
                type="button"
                onClick={() => void handleUploadCurriculum()}
                disabled={saving || !currFile}
                className="w-full py-2.5 bg-blue-600 text-white rounded-lg font-semibold text-sm hover:bg-blue-700 disabled:opacity-50"
              >
                {saving ? 'Subiendo...' : 'Subir PDF y extraer con IA'}
              </button>

              {currResult && (
                <div className="mt-2 p-3 bg-gray-50 rounded-lg text-xs text-gray-600 space-y-1">
                  <p><span className="font-medium">Job ID:</span> <code className="font-mono">{currResult.jobId}</code></p>
                  <p><span className="font-medium">Blob URL:</span> <span className="break-all">{currResult.blobUrl}</span></p>
                </div>
              )}
            </div>
          </section>
        )}
      </main>
    </div>
  );
}
