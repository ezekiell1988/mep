// Dev: Next.js proxy reescribe /api/* → http://localhost:8000/api/*
// Prod: SPA servido por .NET en el mismo origen → /api/* va directo al backend
const API_BASE = process.env.NEXT_PUBLIC_API_URL ?? '';

async function apiFetch<T>(path: string, token: string, options?: RequestInit): Promise<T> {
  const res = await fetch(`${API_BASE}${path}`, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${token}`,
      ...options?.headers,
    },
  });
  if (!res.ok) {
    const text = await res.text().catch(() => res.statusText);
    throw new Error(`API ${res.status}: ${text}`);
  }
  return res.json() as Promise<T>;
}

export interface Grupo {
  id: string;
  name: string;
  level: string;
  subject: string;
  schoolYear: number;
  teacherId: string;
  pctCotidiano: number;
  pctPruebas: number;
  pctExtraclase: number;
  pctOtros: number;
}

export interface Estudiante {
  studentId: string;
  fullName: string;
  studentCode: string;
  groupId: string;
  qrCode: string;
}

export const getGrupos    = (token: string) => apiFetch<Grupo[]>('/api/grupos', token);
export const getEstudiantes = (token: string, groupId: string) =>
  apiFetch<Estudiante[]>(`/api/grupos/${groupId}/estudiantes`, token);

// ─── Planeamiento ────────────────────────────────────────────────────────────

export interface CrearPlaneamientoRequest {
  groupId: string;
  asignatura: string;
  nivel: number;
  trimestre: number;
  anioLectivo: number;
  fechaInicio: string; // "YYYY-MM-DD"
  fechaFin: string;    // "YYYY-MM-DD"
  leccionesPorSemana: number;
}

export interface PlaneamientoResponse {
  id: string;
  status: 'Pending' | 'Generating' | 'Ready' | 'Failed';
  contenido: string | null;
}

export interface PlaneamientoListItem {
  id: string;
  asignatura: string;
  nivel: number;
  trimestre: number;
  status: string;
  createdAt: string;
}

export const crearPlaneamiento = (token: string, body: CrearPlaneamientoRequest) =>
  apiFetch<PlaneamientoResponse>('/api/planeamiento', token, {
    method: 'POST',
    body: JSON.stringify(body),
  });

export const getPlaneamiento = (token: string, id: string) =>
  apiFetch<PlaneamientoResponse>(`/api/planeamiento/${id}`, token);

export const listPlaneamientos = (token: string, groupId?: string) =>
  apiFetch<PlaneamientoListItem[]>(
    groupId ? `/api/planeamiento?groupId=${groupId}` : '/api/planeamiento',
    token,
  );

// ─── Notas ────────────────────────────────────────────────────────────────────

export interface ActividadResponse {
  id: string;
  name: string;
  type: string;
  maxScore: number;
  percentage: number;
  dueDate: string | null;
}

export interface CrearActividadRequest {
  name: string;
  type: string;
  maxScore: number;
  percentage: number;
  dueDate: string | null;
}

export interface SaveCalificacionRequest {
  studentId: string;
  score: number;
  comments: string | null;
}

export interface NotaActividadItem {
  actividadId: string;
  nombre: string;
  tipo: string;
  maxScore: number;
  porcentaje: number;
  nota: number | null;
  comentario: string | null;
}

export interface ResumenEstudianteResponse {
  studentId: string;
  fullName: string;
  studentCode: string;
  promedio: number | null;
  notas: NotaActividadItem[];
}

export interface ResumenGrupoResponse {
  groupId: string;
  totalActividades: number;
  estudiantes: ResumenEstudianteResponse[];
}

export const getActividades = (token: string, groupId: string) =>
  apiFetch<ActividadResponse[]>(`/api/grupos/${groupId}/actividades`, token);

export const crearActividad = (token: string, groupId: string, body: CrearActividadRequest) =>
  apiFetch<ActividadResponse>(`/api/grupos/${groupId}/actividades`, token, {
    method: 'POST',
    body: JSON.stringify(body),
  });

export const eliminarActividad = (token: string, groupId: string, actividadId: string) =>
  apiFetch<void>(`/api/grupos/${groupId}/actividades/${actividadId}`, token, { method: 'DELETE' });

export const saveCalificaciones = (token: string, groupId: string, actividadId: string, body: SaveCalificacionRequest[]) =>
  apiFetch<void>(`/api/grupos/${groupId}/actividades/${actividadId}/calificaciones`, token, {
    method: 'POST',
    body: JSON.stringify(body),
  });

export const getResumenNotas = (token: string, groupId: string) =>
  apiFetch<ResumenGrupoResponse>(`/api/grupos/${groupId}/notas/resumen`, token);

export interface UpdatePonderacionRequest {
  pctCotidiano: number;
  pctPruebas: number;
  pctExtraclase: number;
  pctOtros: number;
}

export const getGrupoById = (token: string, groupId: string) =>
  apiFetch<Grupo>(`/api/grupos/${groupId}`, token);

export const actualizarPonderacion = (token: string, groupId: string, body: UpdatePonderacionRequest) =>
  apiFetch<void>(`/api/grupos/${groupId}/ponderacion`, token, {
    method: 'PUT',
    body: JSON.stringify(body),
  });

// ─── Asistencia ──────────────────────────────────────────────────────────────

export interface HistorialEstudianteRow {
  studentId: string;
  fullName: string;
  studentCode: string;
  asistencia: Record<string, string | null>; // "yyyy-MM-dd" → "Present"|"Absent"|"Late"|"Justified"|null
}

export interface HistorialAsistenciaResponse {
  grupoId: string;
  from: string;
  to: string;
  fechas: string[];
  filas: HistorialEstudianteRow[];
}

export const getHistorialAsistencia = (
  token: string,
  groupId: string,
  from: string,
  to: string,
) => apiFetch<HistorialAsistenciaResponse>(
  `/api/grupos/${groupId}/asistencia/historial?from=${from}&to=${to}`,
  token,
);

// ─── Calendario ───────────────────────────────────────────────────────────────

export const CALENDAR_EVENT_TYPES = [
  'Holiday', 'Exam', 'TeacherMeeting', 'SportWeek', 'Civic', 'Institutional', 'Other',
] as const;
export type CalendarEventType = (typeof CALENDAR_EVENT_TYPES)[number];

export const CALENDAR_EVENT_LABELS: Record<CalendarEventType, string> = {
  Holiday:        'Feriado',
  Exam:           'Exámenes',
  TeacherMeeting: 'Consejo de profesores',
  SportWeek:      'FEA / Semana del deporte',
  Civic:          'Acto cívico',
  Institutional:  'Día institucional',
  Other:          'Otro',
};

export interface CalendarEventResponse {
  id: string;
  groupId: string | null;
  date: string;        // "YYYY-MM-DD"
  endDate: string | null;
  title: string;
  type: CalendarEventType;
  isNational: boolean;
  isEditable: boolean;
}

export interface LeccionesDisponiblesResponse {
  from: string;
  to: string;
  leccionesPorSemana: number;
  diasHabiles: number;
  diasNoLectivos: number;
  diasEfectivos: number;
  leccionesDisponibles: number;
}

export const getCalendario = (token: string, groupId: string, year?: number, month?: number) => {
  const params = new URLSearchParams();
  if (year)  params.set('year', String(year));
  if (month) params.set('month', String(month));
  const qs = params.toString();
  return apiFetch<CalendarEventResponse[]>(
    `/api/grupos/${groupId}/calendario${qs ? `?${qs}` : ''}`,
    token,
  );
};

export const crearEventoCalendario = (
  token: string,
  groupId: string,
  body: { date: string; endDate?: string | null; title: string; type: string },
) => apiFetch<CalendarEventResponse>(
  `/api/grupos/${groupId}/calendario`,
  token,
  { method: 'POST', body: JSON.stringify(body) },
);

export const eliminarEventoCalendario = (token: string, groupId: string, id: string) =>
  apiFetch<void>(`/api/grupos/${groupId}/calendario/${id}`, token, { method: 'DELETE' });

export const getLeccionesDisponibles = (
  token: string,
  groupId: string,
  from: string,
  to: string,
  leccionesPorSemana: number,
) => apiFetch<LeccionesDisponiblesResponse>(
  `/api/grupos/${groupId}/calendario/lecciones?from=${from}&to=${to}&leccionesPorSemana=${leccionesPorSemana}`,
  token,
);

// ── Adecuaciones ──────────────────────────────────────────────────────────

export type AccommodationType = 'AS' | 'ANS' | 'AA';
export type AccommodationStatus = 'Draft' | 'Pending' | 'Generating' | 'Ready' | 'Failed';

export interface AdecuacionResumen {
  id: string;
  studentId: string;
  studentName: string;
  studentCode: string;
  type: AccommodationType;
  diagnostico: string;
  status: AccommodationStatus;
  generatedAt: string | null;
}

export interface AdecuacionResponse {
  id: string;
  studentId: string;
  groupId: string;
  type: AccommodationType;
  diagnostico: string;
  condicionEspecial: string | null;
  estrategiasMediacion: string | null;
  estrategiasEvaluacion: string | null;
  observaciones: string | null;
  propuestaGenerada: string | null;
  status: AccommodationStatus;
  generatedAt: string | null;
  errorMessage: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface UpsertAdecuacionRequest {
  type: AccommodationType;
  diagnostico: string;
  condicionEspecial?: string | null;
  estrategiasMediacion?: string | null;
  estrategiasEvaluacion?: string | null;
  observaciones?: string | null;
}

export const ACCOMMODATION_TYPE_LABELS: Record<AccommodationType, string> = {
  AS:  'Adecuación Significativa (AS)',
  ANS: 'Adecuación No Significativa (ANS)',
  AA:  'Apoyo Académico (AA)',
};

export const listAdecuaciones = (token: string, groupId: string) =>
  apiFetch<AdecuacionResumen[]>(`/api/grupos/${groupId}/adecuaciones`, token);

export const getAdecuacion = (token: string, groupId: string, studentId: string) =>
  apiFetch<AdecuacionResponse>(`/api/grupos/${groupId}/estudiantes/${studentId}/adecuacion`, token);

export const upsertAdecuacion = (token: string, groupId: string, studentId: string, body: UpsertAdecuacionRequest) =>
  apiFetch<AdecuacionResponse>(`/api/grupos/${groupId}/estudiantes/${studentId}/adecuacion`, token, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  });

export const eliminarAdecuacion = (token: string, groupId: string, studentId: string) =>
  apiFetch<void>(`/api/grupos/${groupId}/estudiantes/${studentId}/adecuacion`, token, { method: 'DELETE' });

export const generarPropuestaAdecuacion = (token: string, groupId: string, studentId: string) =>
  apiFetch<AdecuacionResponse>(`/api/grupos/${groupId}/estudiantes/${studentId}/adecuacion/generar`, token, {
    method: 'POST',
  });

export const getInformeAdecuacionUrl = (groupId: string, studentId: string) =>
  `${process.env.NEXT_PUBLIC_API_URL ?? ''}/api/grupos/${groupId}/estudiantes/${studentId}/adecuacion/informe`;

// ── Reportes de asistencia ──────────────────────────────────────────────────
export const getReporteAsistenciaUrl = (groupId: string, from: string, to: string, format: 'pdf' | 'xlsx') =>
  `${process.env.NEXT_PUBLIC_API_URL ?? ''}/api/grupos/${groupId}/reportes/asistencia/${format}?from=${from}&to=${to}`;

export const getInformeDirectorUrl = (groupId: string, from: string, to: string) =>
  `${process.env.NEXT_PUBLIC_API_URL ?? ''}/api/grupos/${groupId}/reportes/informe-director?from=${from}&to=${to}`;

// ── Dashboard docente ─────────────────────────────────────────────────────

export interface ProximoEvento {
  fecha: string;
  titulo: string;
  tipo: string;
}

export interface DocenteResumenResponse {
  totalGrupos: number;
  totalEstudiantes: number;
  estudiantesEnRiesgo: number;
  planeamientosPendientes: number;
  planeamientosListos: number;
  adecuacionesActivas: number;
  proximosEventos: ProximoEvento[];
}

export const getDocenteResumen = (token: string) =>
  apiFetch<DocenteResumenResponse>('/api/docente/resumen', token);

// ── Suscripciones ─────────────────────────────────────────────────────────

export type SubscriptionPlan = 'Trial' | 'Basic' | 'Professional' | 'Institutional';
export type SubscriptionStatus = 'Active' | 'Expired' | 'Cancelled';
export type PaymentRequestStatus = 'Pending' | 'Approved' | 'Rejected';

export interface SuscripcionEstadoResponse {
  hasSubscription: boolean;
  plan: SubscriptionPlan | null;
  status: SubscriptionStatus | null;
  isTrial: boolean;
  currentPeriodEnd: string | null;
  daysRemaining: number | null;
  trialDays: number;
}

export interface PaymentRequestResponse {
  id: string;
  plan: SubscriptionPlan;
  amountUsd: number;
  amountCrc: number;
  exchangeRateUsed: number;
  referenceCode: string;
  status: PaymentRequestStatus;
  sinpePhone: string;
  sinpeAccountName: string;
  createdAt: string;
}

export interface PlanInfo {
  id: SubscriptionPlan;
  nombre: string;
  precioUsd: number;
  precioCrc: number;
  descripcion: string;
}

export interface SuscripcionInfoResponse {
  sinpePhone: string;
  sinpeAccountName: string;
  exchangeRate: number;
  trialDays: number;
  plans: PlanInfo[];
}

export interface AdminPagoResponse {
  id: string;
  userName: string;
  userEmail: string;
  plan: SubscriptionPlan;
  amountUsd: number;
  amountCrc: number;
  referenceCode: string;
  status: PaymentRequestStatus;
  hasVoucher: boolean;
  adminNote: string | null;
  createdAt: string;
  reviewedAt: string | null;
}

export interface AdminSuscripcionResponse {
  id: string;
  userName: string;
  userEmail: string;
  plan: SubscriptionPlan;
  status: SubscriptionStatus;
  isTrial: boolean;
  periodStart: string;
  periodEnd: string;
  daysRemaining: number;
}

export interface ReferralCodeResponse {
  id: string;
  code: string;
  isActive: boolean;
  createdAt: string;
}

export interface ReferidoResumen {
  nombre: string;
  email: string;
  plan: string;
  estado: string;
  registrado: string;
}

export interface ReferralPanelResponse {
  code: string | null;
  totalReferidos: number;
  totalComisionesCrc: number;
  referidos: ReferidoResumen[];
}

export interface ComisionResponse {
  id: string;
  referidoNombre: string;
  month: number;
  grossRevenueCrc: number;
  infraCostCrc: number;
  baseAmountCrc: number;
  commissionAmountCrc: number;
  status: 'Pending' | 'Paid';
  createdAt: string;
}

export interface AdminComisionResponse {
  id: string;
  codigoReferido: string;
  referidorNombre: string;
  referidoNombre: string;
  month: number;
  commissionAmountCrc: number;
  status: 'Pending' | 'Paid';
}

export const getSuscripcionEstado = (token: string) =>
  apiFetch<SuscripcionEstadoResponse>('/api/suscripcion', token);

export const activarTrial = (token: string) =>
  apiFetch<SuscripcionEstadoResponse>('/api/suscripcion/trial', token, { method: 'POST' });

export const solicitarPago = (token: string, plan: SubscriptionPlan) =>
  apiFetch<PaymentRequestResponse>('/api/suscripcion/pago', token, {
    method: 'POST',
    body: JSON.stringify({ plan }),
  });

export const getSuscripcionInfo = () =>
  fetch(`${process.env.NEXT_PUBLIC_API_URL ?? ''}/api/suscripcion/info`)
    .then(r => r.json() as Promise<SuscripcionInfoResponse>);

export const getAdminPagosPendientes = (token: string) =>
  apiFetch<AdminPagoResponse[]>('/api/admin/pagos/pendientes', token);

export const getAdminPagosHistorial = (token: string) =>
  apiFetch<AdminPagoResponse[]>('/api/admin/pagos/historial', token);

export const aprobarPago = (token: string, id: string, nota?: string) =>
  apiFetch<void>(`/api/admin/pagos/${id}/aprobar`, token, {
    method: 'POST',
    body: JSON.stringify({ nota: nota ?? null }),
  });

export const rechazarPago = (token: string, id: string, nota: string) =>
  apiFetch<void>(`/api/admin/pagos/${id}/rechazar`, token, {
    method: 'POST',
    body: JSON.stringify({ nota }),
  });

export const getAdminSuscripciones = (token: string) =>
  apiFetch<AdminSuscripcionResponse[]>('/api/admin/suscripciones', token);

export const getMiCodigoReferido = (token: string) =>
  apiFetch<ReferralCodeResponse>('/api/referidos/mi-codigo', token);

export const getReferralPanel = (token: string) =>
  apiFetch<ReferralPanelResponse>('/api/referidos/panel', token);

export const getComisiones = (token: string) =>
  apiFetch<ComisionResponse[]>('/api/referidos/comisiones', token);

export const ejecutarCierreMensual = (token: string, month: number, infraCostCrc: number) =>
  apiFetch<string>('/api/admin/referidos/cierre-mensual', token, {
    method: 'POST',
    body: JSON.stringify({ month, infraCostCrc }),
  });

export const getAdminComisiones = (token: string) =>
  apiFetch<AdminComisionResponse[]>('/api/admin/referidos/comisiones', token);

export const marcarComisionPagada = (token: string, id: string) =>
  apiFetch<void>(`/api/admin/referidos/comisiones/${id}/pagar`, token, { method: 'POST' });

export const subirComprobante = async (token: string, paymentId: string, file: File): Promise<void> => {
  const formData = new FormData();
  formData.append('file', file);
  const res = await fetch(
    `${process.env.NEXT_PUBLIC_API_URL ?? ''}/api/suscripcion/pago/${paymentId}/comprobante`,
    {
      method: 'POST',
      headers: { Authorization: `Bearer ${token}` },
      body: formData,
    },
  );
  if (!res.ok) throw new Error(`Error subiendo comprobante: ${res.status}`);
};

// ── Curriculum (admin) ─────────────────────────────────────────────────────

export interface UploadCurriculumResponse {
  jobId: string;
  blobUrl: string;
}

export const uploadCurriculumPdf = async (
  token: string,
  asignatura: string,
  ciclo: string,
  file: File,
): Promise<UploadCurriculumResponse> => {
  const formData = new FormData();
  formData.append('file', file);
  const params = new URLSearchParams({ asignatura, ciclo });
  const res = await fetch(
    `${API_BASE}/api/curriculum/upload?${params.toString()}`,
    {
      method: 'POST',
      headers: { Authorization: `Bearer ${token}` },
      body: formData,
    },
  );
  if (!res.ok) {
    const text = await res.text().catch(() => res.statusText);
    throw new Error(`API ${res.status}: ${text}`);
  }
  return res.json() as Promise<UploadCurriculumResponse>;
};

// ── Director institucional ─────────────────────────────────────────────────

export interface ResumenInstitucionalResponse {
  institutionId: string;
  institutionName: string;
  totalDocentes: number;
  totalGrupos: number;
  totalEstudiantes: number;
  planInstitucional: string | null;
  diasRestantes: number | null;
}

export interface GrupoResumenDirector {
  id: string;
  name: string;
  subject: string;
  level: string;
  totalEstudiantes: number;
}

export interface DocenteInstitucionalResponse {
  docenteId: string;
  fullName: string;
  email: string;
  totalGrupos: number;
  totalEstudiantes: number;
  plan: string;
  planActivo: boolean;
  grupos: GrupoResumenDirector[];
}

export const getDirectorResumen = (token: string) =>
  apiFetch<ResumenInstitucionalResponse>('/api/director/resumen', token);

export const getDirectorDocentes = (token: string) =>
  apiFetch<DocenteInstitucionalResponse[]>('/api/director/docentes', token);
