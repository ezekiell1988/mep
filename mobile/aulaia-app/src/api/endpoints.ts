import { apiFetch } from './client';

// ── Tipos ────────────────────────────────────────────────────────────────────

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

export type AttendanceStatus = 'Present' | 'Absent' | 'Late' | 'Justified';

export interface AsistenciaDia {
  studentId: string;
  fullName: string;
  studentCode: string;
  qrCode: string;
  status: AttendanceStatus | null;
  notes: string | null;
}

export interface AsistenciaGrupo {
  grupoId: string;
  date: string;
  students: AsistenciaDia[];
}

// ── Grupos ───────────────────────────────────────────────────────────────────

export const getGrupos = (token: string) =>
  apiFetch<Grupo[]>('/api/grupos', token);

// ── Estudiantes ──────────────────────────────────────────────────────────────

export const getEstudiantes = (token: string, grupoId: string) =>
  apiFetch<Estudiante[]>(`/api/grupos/${grupoId}/estudiantes`, token);

// ── Asistencia ───────────────────────────────────────────────────────────────

export const getAsistencia = (token: string, grupoId: string, date: string) =>
  apiFetch<AsistenciaGrupo>(`/api/grupos/${grupoId}/asistencia?date=${date}`, token);

export const upsertAsistencia = (
  token: string,
  grupoId: string,
  date: string,
  records: { studentId: string; status: AttendanceStatus; notes?: string | null }[],
) =>
  apiFetch<void>(`/api/grupos/${grupoId}/asistencia`, token, {
    method: 'POST',
    body: JSON.stringify({ date, records }),
  });

export const scanQr = (
  token: string,
  grupoId: string,
  qrCode: string,
  date?: string,
) =>
  apiFetch<{ studentId: string; fullName: string; studentCode: string; date: string; status: AttendanceStatus }>(
    '/api/asistencia/qr',
    token,
    {
      method: 'POST',
      body: JSON.stringify({ grupoId, qrCode, date }),
    },
  );
