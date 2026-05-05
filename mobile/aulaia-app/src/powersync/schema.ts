import { column, Schema, Table } from '@powersync/react-native';

// Cada columna en PowerSync es nullable por defecto.
// El tipo 'text' almacena UUIDs, fechas ISO-8601 y strings.
// El tipo 'integer' almacena booleanos (0/1) y enteros.
// Los IDs (pk) son gestionados por PowerSync — no se declaran aquí.

const groups = new Table(
  {
    name:           column.text,
    level:          column.text,
    subject:        column.text,
    school_year:    column.integer,
    teacher_id:     column.text,
    institution_id: column.text,
    is_active:      column.integer,   // 0 | 1
    created_at:     column.text,
  },
  { indexes: { by_teacher: ['teacher_id'] } },
);

const students = new Table(
  {
    full_name:    column.text,
    student_code: column.text,
    group_id:     column.text,
    qr_code:      column.text,
    created_at:   column.text,
  },
  { indexes: { by_group: ['group_id'] } },
);

const attendance_records = new Table(
  {
    group_id:   column.text,
    student_id: column.text,
    date:       column.text,      // 'YYYY-MM-DD'
    status:     column.text,      // 'Present' | 'Absent' | 'Late' | 'Justified'
    notes:      column.text,
    created_at: column.text,
  },
  { indexes: { by_group_date: ['group_id', 'date'] } },
);

export const AppSchema = new Schema({ groups, students, attendance_records });

// Tipos TypeScript derivados del schema para usar en las pantallas
export type GroupRow = {
  id: string;
  name: string;
  level: string;
  subject: string;
  school_year: number;
  teacher_id: string;
  institution_id: string;
  is_active: number;
  created_at: string;
};

export type StudentRow = {
  id: string;
  full_name: string;
  student_code: string;
  group_id: string;
  qr_code: string;
  created_at: string;
};

export type AttendanceRow = {
  id: string;
  group_id: string;
  student_id: string;
  date: string;
  status: string;
  notes: string | null;
  created_at: string;
};
