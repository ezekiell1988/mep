'use client';

import { useAuth0 } from '@auth0/auth0-react';
import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import { getGrupos, crearPlaneamiento, checkCurriculumDisponible, type Grupo, type CurriculumCheckResponse } from '../../../lib/api';

const ASIGNATURAS = [
  'Artes Plásticas', 'Artes Musicales', 'Educación para el Hogar',
  'Educación Física', 'Matemáticas', 'Español',
  'Ciencias', 'Estudios Sociales', 'Inglés', 'Francés', 'Informática',
  'Religión', 'Orientación',
];

const NIVELES = [
  { value: 7, label: '7° (III Ciclo)' }, { value: 8, label: '8° (III Ciclo)' },
  { value: 9, label: '9° (III Ciclo)' }, { value: 10, label: '10° (Diversificado)' },
  { value: 11, label: '11° (Diversificado)' }, { value: 12, label: '12° (Diversificado)' },
  { value: 1, label: '1° (I Ciclo)' }, { value: 2, label: '2° (I Ciclo)' },
  { value: 3, label: '3° (I Ciclo)' }, { value: 4, label: '4° (II Ciclo)' },
  { value: 5, label: '5° (II Ciclo)' }, { value: 6, label: '6° (II Ciclo)' },
];

function InputField({ label, error, children }: { label: string; error?: string; children: React.ReactNode }) {
  return (
    <div>
      <label className="block text-sm font-medium text-gray-700 mb-1">{label}</label>
      {children}
      {error ? <p className="text-xs text-red-500 mt-1">{error}</p> : null}
    </div>
  );
}

const fieldClass = 'w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white';

export default function NuevoPlaneamientoPage() {
  const { isAuthenticated, isLoading, loginWithRedirect, getAccessTokenSilently } = useAuth0();
  const router = useRouter();

  const [grupos, setGrupos] = useState<Grupo[]>([]);
  const [submitting, setSubmitting] = useState(false);
  const [apiError, setApiError] = useState<string | null>(null);
  const [curriculumCheck, setCurriculumCheck] = useState<CurriculumCheckResponse | null>(null);
  const [checkingCurriculum, setCheckingCurriculum] = useState(false);

  const currentYear = new Date().getFullYear();

  const [form, setForm] = useState({
    groupId: '',
    asignatura: 'Artes Plásticas',
    nivel: 7,
    trimestre: 1,
    anioLectivo: currentYear,
    fechaInicio: '',
    fechaFin: '',
    leccionesPorSemana: 3,
  });

  // Redirigir si no autenticado
  useEffect(() => {
    if (!isLoading && !isAuthenticated) loginWithRedirect();
  }, [isAuthenticated, isLoading, loginWithRedirect]);

  // Cargar grupos al autenticarse — batch de los dos setState en un solo bloque async
  useEffect(() => {
    if (!isAuthenticated || isLoading) return;
    let cancelled = false;
    getAccessTokenSilently()
      .then(token => getGrupos(token))
      .then(data => {
        if (cancelled) return;
        // Batch: ambos setState en el mismo microtask para evitar renders intermedios
        setGrupos(data);
        if (data.length > 0) setForm(f => ({ ...f, groupId: data[0].id }));
      })
      .catch(() => { /* sin grupos disponibles */ });
    return () => { cancelled = true; };
  }, [isAuthenticated, isLoading, getAccessTokenSilently]);

  const set = (key: keyof typeof form, value: string | number) =>
    setForm(f => ({ ...f, [key]: value }));

  // Verificar disponibilidad de programa MEP al cambiar asignatura/nivel/trimestre
  useEffect(() => {
    if (!isAuthenticated || isLoading) return;
    let cancelled = false;
    setCurriculumCheck(null);
    setCheckingCurriculum(true);
    const timer = setTimeout(() => {
      getAccessTokenSilently()
        .then(token => checkCurriculumDisponible(token, form.asignatura, form.nivel, form.trimestre))
        .then(result => { if (!cancelled) setCurriculumCheck(result); })
        .catch(() => { if (!cancelled) setCurriculumCheck(null); })
        .finally(() => { if (!cancelled) setCheckingCurriculum(false); });
    }, 300);
    return () => { cancelled = true; clearTimeout(timer); };
  }, [isAuthenticated, isLoading, form.asignatura, form.nivel, form.trimestre, getAccessTokenSilently]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setApiError(null);
    setSubmitting(true);
    try {
      const token = await getAccessTokenSilently();
      const plan = await crearPlaneamiento(token, {
        groupId: form.groupId,
        asignatura: form.asignatura,
        nivel: form.nivel,
        trimestre: form.trimestre,
        anioLectivo: form.anioLectivo,
        fechaInicio: form.fechaInicio,
        fechaFin: form.fechaFin,
        leccionesPorSemana: form.leccionesPorSemana,
      });
      router.push(`/planeamiento/detalle?id=${plan.id}`);
    } catch (e: unknown) {
      setApiError(e instanceof Error ? e.message : 'Error al crear el planeamiento');
      setSubmitting(false);
    }
  };

  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div role="status" aria-label="Cargando" className="w-8 h-8 border-4 border-blue-600 border-t-transparent rounded-full animate-spin" />
      </div>
    );
  }

  return (
    <div className="max-w-xl mx-auto px-4 py-8">
      <button type="button" onClick={() => router.push('/planeamiento')} className="text-sm text-gray-400 hover:text-gray-600 mb-4 flex items-center gap-1" aria-label="Volver a Planeamientos">
        <span aria-hidden="true">←</span> Planeamientos
      </button>
      <h1 className="text-2xl font-bold text-gray-900 mb-6">Nuevo planeamiento</h1>

      {apiError !== null ? (
        <div role="alert" className="bg-red-50 border border-red-200 text-red-700 rounded-xl p-4 mb-6 text-sm">{apiError}</div>
      ) : null}

      <form onSubmit={handleSubmit} className="bg-white rounded-2xl border border-gray-200 shadow-sm p-6 flex flex-col gap-5">
        {/* Grupo */}
        <InputField label="Grupo / Sección">
          <select
            aria-label="Grupo / Sección"
            value={form.groupId}
            onChange={e => set('groupId', e.target.value)}
            required
            className={fieldClass}
          >
            {grupos.length === 0 ? (
              <option value="">Sin grupos disponibles</option>
            ) : (
              grupos.map(g => (
                <option key={g.id} value={g.id}>
                  {g.name} — {g.subject} · {g.level}
                </option>
              ))
            )}
          </select>
        </InputField>

        {/* Asignatura */}
        <InputField label="Asignatura">
          <select
            aria-label="Asignatura"
            value={form.asignatura}
            onChange={e => set('asignatura', e.target.value)}
            required
            className={fieldClass}
          >
            {ASIGNATURAS.map(a => <option key={a} value={a}>{a}</option>)}
          </select>
        </InputField>

        {/* Nivel y Trimestre */}
        <div className="grid grid-cols-2 gap-4">
          <InputField label="Nivel">
            <select
              aria-label="Nivel"
              value={form.nivel}
              onChange={e => set('nivel', Number(e.target.value))}
              required
              className={fieldClass}
            >
              {NIVELES.map(n => <option key={n.value} value={n.value}>{n.label}</option>)}
            </select>
          </InputField>

          <InputField label="Trimestre">
            <select
              aria-label="Trimestre"
              value={form.trimestre}
              onChange={e => set('trimestre', Number(e.target.value))}
              required
              className={fieldClass}
            >
              <option value={1}>I Trimestre</option>
              <option value={2}>II Trimestre</option>
              <option value={3}>III Trimestre</option>
            </select>
          </InputField>
        </div>

        {/* Badge disponibilidad programa MEP */}
        {checkingCurriculum ? (
          <div className="flex items-center gap-2 text-xs text-gray-400" aria-live="polite" aria-label="Verificando programa MEP">
            <span aria-hidden="true" className="w-3.5 h-3.5 border-2 border-gray-300 border-t-transparent rounded-full animate-spin inline-block" />
            Verificando programa MEP…
          </div>
        ) : curriculumCheck !== null ? (
          curriculumCheck.disponible ? (
            <div role="status" aria-live="polite" className="flex items-center gap-2 text-xs text-green-700 bg-green-50 border border-green-200 rounded-lg px-3 py-2">
              <span aria-hidden="true">✓</span>
              <span>Programa MEP validado — <strong>{curriculumCheck.unidades} unidades</strong> del currículo oficial disponibles</span>
            </div>
          ) : (
            <div role="status" aria-live="polite" className="flex items-center gap-2 text-xs text-amber-700 bg-amber-50 border border-amber-200 rounded-lg px-3 py-2">
              <span aria-hidden="true">⚠</span>
              <span>Sin programa MEP validado para esta combinación — la IA usará conocimiento general</span>
            </div>
          )
        ) : null}

        {/* Año lectivo y lecciones/semana */}
        <div className="grid grid-cols-2 gap-4">
          <InputField label="Año lectivo">
            <input
              aria-label="Año lectivo"
              type="number"
              min={2020}
              max={2050}
              value={form.anioLectivo}
              onChange={e => set('anioLectivo', Number(e.target.value))}
              required
              className={fieldClass}
            />
          </InputField>

          <InputField label="Lecciones / semana">
            <input
              aria-label="Lecciones por semana"
              type="number"
              min={1}
              max={10}
              value={form.leccionesPorSemana}
              onChange={e => set('leccionesPorSemana', Number(e.target.value))}
              required
              className={fieldClass}
            />
          </InputField>
        </div>

        {/* Fechas */}
        <div className="grid grid-cols-2 gap-4">
          <InputField label="Fecha inicio">
            <input
              aria-label="Fecha de inicio"
              type="date"
              value={form.fechaInicio}
              onChange={e => set('fechaInicio', e.target.value)}
              required
              className={fieldClass}
            />
          </InputField>

          <InputField label="Fecha fin">
            <input
              aria-label="Fecha de fin"
              type="date"
              value={form.fechaFin}
              onChange={e => set('fechaFin', e.target.value)}
              required
              className={fieldClass}
            />
          </InputField>
        </div>

        <button
          type="submit"
          disabled={submitting || grupos.length === 0}
          className="w-full bg-blue-600 hover:bg-blue-700 disabled:bg-blue-300 text-white font-semibold py-3 rounded-xl text-sm transition-colors flex items-center justify-center gap-2"
        >
          {submitting ? (
            <>
              <span role="status" aria-label="Enviando" className="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin" />
              Enviando…
            </>
          ) : 'Generar planeamiento con IA'}
        </button>
      </form>
    </div>
  );
}
