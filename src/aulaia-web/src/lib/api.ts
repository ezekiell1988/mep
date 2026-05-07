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
