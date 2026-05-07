import { useEffect, useRef } from 'react';
import * as Notifications from 'expo-notifications';
import { useQuery } from '@powersync/react-native';

// Umbral MEP: 65 para III Ciclo y primaria
const UMBRAL_RIESGO = 65;
// Solo notificar si hay ≥ 1 estudiante en riesgo por grupo
const CLAVE_NOTIFICADOS = 'notif_riesgo_enviadas_hoy';

Notifications.setNotificationHandler({
  handleNotification: async () => ({
    shouldShowAlert: true,
    shouldPlaySound: false,
    shouldSetBadge: false,
  }),
});

interface GradeRow {
  student_id: string;
  group_id: string;
  score: number;
}

interface StudentRow {
  id: string;
  full_name: string;
  group_id: string;
}

interface GroupRow {
  id: string;
  name: string;
}

export function useRendimientoNotifications() {
  const notificadosHoy = useRef<Set<string>>(new Set());

  // Notas sincronizadas localmente
  const { data: grades } = useQuery<GradeRow>(
    'SELECT student_id, group_id, score FROM grades',
  );

  const { data: students } = useQuery<StudentRow>(
    'SELECT id, full_name, group_id FROM students WHERE is_active IS NULL OR is_active = 1',
  );

  const { data: groups } = useQuery<GroupRow>(
    'SELECT id, name FROM groups WHERE is_active = 1',
  );

  useEffect(() => {
    if (!grades.length || !students.length || !groups.length) return;

    (async () => {
      const { status } = await Notifications.requestPermissionsAsync();
      if (status !== 'granted') return;

      // Promedio de notas por estudiante
      const scoresByStudent = new Map<string, number[]>();
      for (const g of grades) {
        const arr = scoresByStudent.get(g.student_id) ?? [];
        arr.push(g.score);
        scoresByStudent.set(g.student_id, arr);
      }

      // Estudiantes en riesgo agrupados por grupo
      const enRiesgoPorGrupo = new Map<string, string[]>();
      for (const s of students) {
        const scores = scoresByStudent.get(s.id);
        if (!scores || scores.length === 0) continue;
        const promedio = scores.reduce((a, b) => a + b, 0) / scores.length;
        if (promedio < UMBRAL_RIESGO) {
          const lista = enRiesgoPorGrupo.get(s.group_id) ?? [];
          lista.push(s.full_name);
          enRiesgoPorGrupo.set(s.group_id, lista);
        }
      }

      for (const [groupId, nombres] of enRiesgoPorGrupo.entries()) {
        // Una notificación por grupo por sesión de app
        const clave = `${groupId}_${new Date().toISOString().split('T')[0]}`;
        if (notificadosHoy.current.has(clave)) continue;
        notificadosHoy.current.add(clave);

        const grupo = groups.find(g => g.id === groupId);
        const nombreGrupo = grupo?.name ?? 'Grupo';
        const n = nombres.length;

        await Notifications.scheduleNotificationAsync({
          content: {
            title: `⚠️ Estudiantes en riesgo — ${nombreGrupo}`,
            body: n === 1
              ? `${nombres[0]} tiene promedio bajo 65.`
              : `${n} estudiantes tienen promedio bajo 65 en ${nombreGrupo}.`,
            data: { groupId },
          },
          trigger: null, // inmediata
        });
      }
    })();
  }, [grades, students, groups]);
}
