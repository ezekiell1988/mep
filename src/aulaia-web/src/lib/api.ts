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
