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

const lesson_plans = new Table(
  {
    group_id:             column.text,
    teacher_sub:          column.text,
    asignatura:           column.text,
    nivel:                column.integer,
    trimestre:            column.integer,
    anio_lectivo:         column.integer,
    fecha_inicio:         column.text,    // 'YYYY-MM-DD'
    fecha_fin:            column.text,    // 'YYYY-MM-DD'
    lecciones_por_semana: column.integer,
    contenido_generado:   column.text,    // Markdown — puede ser null mientras genera
    status:               column.text,    // 'Pending' | 'Generating' | 'Ready' | 'Failed'
    error_message:        column.text,
    created_at:           column.text,
    generated_at:         column.text,
  },
  { indexes: { by_teacher: ['teacher_sub'], by_group: ['group_id'] } },
);

export const AppSchema = new Schema({ groups, students, attendance_records, lesson_plans });

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

export type LessonPlanRow = {
  id: string;
  group_id: string;
  teacher_sub: string;
  asignatura: string;
  nivel: number;
  trimestre: number;
  anio_lectivo: number;
  fecha_inicio: string;
  fecha_fin: string;
  lecciones_por_semana: number;
  contenido_generado: string | null;
  status: string;
  error_message: string | null;
  created_at: string;
  generated_at: string | null;
};
