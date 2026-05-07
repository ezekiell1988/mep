import React, { useMemo } from 'react';
import {
  View, Text, FlatList, StyleSheet, ActivityIndicator, ScrollView,
} from 'react-native';
import { NativeStackScreenProps } from '@react-navigation/native-stack';
import { SafeAreaView } from 'react-native-safe-area-context';
import { useQuery } from '@powersync/react-native';
import type { EvaluationActivityRow, GradeRow, StudentRow } from '../powersync/schema';
import { RootStackParamList } from '../navigation/types';

type Props = NativeStackScreenProps<RootStackParamList, 'Notas'>;

// Umbral MEP: 70 para Diversificado (10°–12°), 65 para el resto
function umbral(level: string): number {
  const n = parseInt(level, 10);
  return n >= 10 ? 70 : 65;
}

export default function NotasScreen({ route }: Props) {
  const { grupo } = route.params;

  const { data: actividades, isLoading: loadActs } = useQuery<EvaluationActivityRow>(
    'SELECT * FROM evaluation_activities WHERE group_id = ? ORDER BY created_at',
    [grupo.id],
  );

  const { data: students, isLoading: loadSts } = useQuery<StudentRow>(
    'SELECT * FROM students WHERE group_id = ? ORDER BY full_name',
    [grupo.id],
  );

  const actividadIds = actividades.map(a => a.id);

  const { data: grades, isLoading: loadGrades } = useQuery<GradeRow>(
    actividadIds.length > 0
      ? `SELECT * FROM grades WHERE activity_id IN (${actividadIds.map(() => '?').join(',')})`
      : 'SELECT * FROM grades WHERE 0',
    actividadIds,
  );

  const isLoading = loadActs || loadSts || loadGrades;

  // Calcular promedio ponderado por estudiante
  const resumen = useMemo(() => {
    return students.map(est => {
      const notasPonderadas = actividades
        .filter(a => a.percentage > 0)
        .map(a => {
          const grade = grades.find(g => g.activity_id === a.id && g.student_id === est.id);
          if (!grade) return null;
          const norm = a.max_score > 0 ? (grade.score / a.max_score) * 100 : 0;
          return { peso: a.percentage, puntaje: norm * a.percentage };
        })
        .filter((n): n is { peso: number; puntaje: number } => n !== null);

      const pesoTotal = notasPonderadas.reduce((s, n) => s + n.peso, 0);
      const promedio = pesoTotal > 0
        ? Math.round((notasPonderadas.reduce((s, n) => s + n.puntaje, 0) / pesoTotal) * 10) / 10
        : null;

      const notasPorActividad = actividades.map(a => {
        const grade = grades.find(g => g.activity_id === a.id && g.student_id === est.id);
        return { actId: a.id, nota: grade?.score ?? null };
      });

      return { ...est, promedio, notasPorActividad };
    });
  }, [students, actividades, grades]);

  const u = umbral(grupo.level);

  if (isLoading) {
    return (
      <SafeAreaView style={styles.centered}>
        <ActivityIndicator size="large" color="#16a34a" />
      </SafeAreaView>
    );
  }

  if (actividades.length === 0) {
    return (
      <SafeAreaView style={styles.centered}>
        <Text style={styles.empty}>No hay actividades de evaluación.{'\n'}Créalas desde la web.</Text>
      </SafeAreaView>
    );
  }

  return (
    <SafeAreaView style={styles.root} edges={['bottom']}>
      {/* Cabecera de actividades (chips) */}
      <ScrollView horizontal showsHorizontalScrollIndicator={false} style={styles.chipsBar} contentContainerStyle={styles.chipsContent}>
        {actividades.map(a => (
          <View key={a.id} style={styles.chip}>
            <Text style={styles.chipText}>{a.name}</Text>
            <Text style={styles.chipSub}>{a.percentage}%</Text>
          </View>
        ))}
      </ScrollView>

      {/* Lista de estudiantes con sus notas */}
      <FlatList
        data={resumen}
        keyExtractor={item => item.id}
        contentContainerStyle={styles.list}
        renderItem={({ item }) => {
          const aprobado = item.promedio !== null && item.promedio >= u;
          const reprobado = item.promedio !== null && item.promedio < u;
          return (
            <View style={styles.row}>
              <View style={styles.rowInfo}>
                <Text style={styles.nombre}>{item.full_name}</Text>
                <Text style={styles.codigo}>{item.student_code}</Text>
              </View>
              <ScrollView horizontal showsHorizontalScrollIndicator={false} style={styles.notaScroll}>
                {item.notasPorActividad.map((n, idx) => (
                  <View key={idx} style={styles.notaCell}>
                    <Text style={styles.notaVal}>
                      {n.nota !== null ? String(n.nota) : '—'}
                    </Text>
                  </View>
                ))}
              </ScrollView>
              <View style={[styles.promBadge, aprobado && styles.aprobado, reprobado && styles.reprobado]}>
                <Text style={[styles.promText, aprobado && styles.aprobadoText, reprobado && styles.reprobadoText]}>
                  {item.promedio !== null ? String(item.promedio) : '—'}
                </Text>
              </View>
            </View>
          );
        }}
      />
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  root: { flex: 1, backgroundColor: '#f9fafb' },
  centered: { flex: 1, justifyContent: 'center', alignItems: 'center', backgroundColor: '#f9fafb' },
  empty: { color: '#9ca3af', textAlign: 'center', fontSize: 15, lineHeight: 24 },

  chipsBar: { flexGrow: 0, borderBottomWidth: 1, borderColor: '#e5e7eb', backgroundColor: '#fff' },
  chipsContent: { paddingHorizontal: 16, paddingVertical: 10, gap: 8 },
  chip: { backgroundColor: '#f0fdf4', borderRadius: 99, paddingHorizontal: 12, paddingVertical: 6 },
  chipText: { fontSize: 12, color: '#166534', fontWeight: '500' },
  chipSub: { fontSize: 11, color: '#4ade80', textAlign: 'center' },

  list: { paddingHorizontal: 16, paddingVertical: 8 },
  row: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: '#fff',
    borderRadius: 12,
    marginBottom: 8,
    paddingVertical: 12,
    paddingHorizontal: 12,
    shadowColor: '#000', shadowOpacity: 0.04, shadowRadius: 4, elevation: 1,
  },
  rowInfo: { width: 130 },
  nombre: { fontSize: 13, fontWeight: '600', color: '#111827' },
  codigo: { fontSize: 11, color: '#9ca3af', marginTop: 1 },

  notaScroll: { flex: 1 },
  notaCell: { width: 44, alignItems: 'center', paddingHorizontal: 2 },
  notaVal: { fontSize: 13, color: '#374151' },

  promBadge: {
    width: 44, height: 32, borderRadius: 8,
    backgroundColor: '#f3f4f6', alignItems: 'center', justifyContent: 'center', marginLeft: 4,
  },
  promText: { fontSize: 13, color: '#6b7280', fontWeight: '600' },
  aprobado: { backgroundColor: '#dcfce7' },
  aprobadoText: { color: '#166534' },
  reprobado: { backgroundColor: '#fee2e2' },
  reprobadoText: { color: '#991b1b' },
});
