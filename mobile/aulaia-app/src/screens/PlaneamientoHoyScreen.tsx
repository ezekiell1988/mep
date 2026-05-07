import React, { useMemo } from 'react';
import {
  View, Text, ScrollView, Pressable, StyleSheet, ActivityIndicator,
} from 'react-native';
import { NativeStackScreenProps } from '@react-navigation/native-stack';
import { SafeAreaView } from 'react-native-safe-area-context';
import { useQuery } from '@powersync/react-native';
import type { LessonPlanRow } from '../powersync/schema';
import { RootStackParamList } from '../navigation/types';

// Renderizador Markdown mínimo — igual al de PlaneamientoDetalleScreen
function stripInline(text: string): string {
  return text
    .replace(/\*\*(.+?)\*\*/g, '$1')
    .replace(/\*(.+?)\*/g, '$1')
    .replace(/`(.+?)`/g, '$1');
}

function MarkdownLine({ line }: { line: string }) {
  if (line === '') return <View style={{ height: 8 }} />;
  const h3 = line.match(/^###\s+(.*)/);
  if (h3) return <Text style={md.h3}>{h3[1]}</Text>;
  const h2 = line.match(/^##\s+(.*)/);
  if (h2) return <Text style={md.h2}>{h2[1]}</Text>;
  const h1 = line.match(/^#\s+(.*)/);
  if (h1) return <Text style={md.h1}>{h1[1]}</Text>;
  if (/^-{3,}$/.test(line.trim())) return <View style={md.hr} />;
  const bullet = line.match(/^[-*]\s+(.*)/);
  if (bullet) {
    return (
      <View style={md.bulletRow}>
        <Text style={md.bulletDot}>•</Text>
        <Text style={md.bulletText}>{stripInline(bullet[1])}</Text>
      </View>
    );
  }
  const numbered = line.match(/^\d+\.\s+(.*)/);
  if (numbered) {
    const num = line.match(/^(\d+)\./)?.[1] ?? '1';
    return (
      <View style={md.bulletRow}>
        <Text style={md.bulletDot}>{num}.</Text>
        <Text style={md.bulletText}>{stripInline(numbered[1])}</Text>
      </View>
    );
  }
  if (line.startsWith('```')) return null;
  return <Text style={md.body}>{stripInline(line)}</Text>;
}

type Props = NativeStackScreenProps<RootStackParamList, 'PlaneamientoHoy'>;

export default function PlaneamientoHoyScreen({ navigation }: Props) {
  const today = useMemo(() => new Date().toISOString().slice(0, 10), []);

  // Busca planeamientos Ready cuyo rango cubre hoy
  const { data: planes, isLoading } = useQuery<LessonPlanRow>(
    `SELECT * FROM lesson_plans
     WHERE status = 'Ready'
       AND fecha_inicio <= ?
       AND fecha_fin   >= ?
     ORDER BY fecha_inicio DESC`,
    [today, today],
  );

  const plan = planes?.[0] ?? null;

  const lines = useMemo(
    () => (plan?.contenido_generado ?? '').split('\n'),
    [plan],
  );

  if (isLoading) {
    return (
      <View style={styles.center}>
        <ActivityIndicator size="large" color="#1a56db" />
      </View>
    );
  }

  if (!plan) {
    return (
      <SafeAreaView style={styles.container} edges={['bottom']}>
        <View style={styles.emptyBox}>
          <Text style={styles.emptyTitle}>Sin planeamiento para hoy</Text>
          <Text style={styles.emptySub}>
            Hoy ({today}) no está dentro del rango de ningún planeamiento listo.{'\n'}
            Generá uno desde mep.ezekl.com.
          </Text>
          <Pressable
            style={styles.btn}
            onPress={() => navigation.navigate('Planeamientos')}
            accessibilityRole="button"
          >
            <Text style={styles.btnText}>Ver todos los planeamientos</Text>
          </Pressable>
        </View>
      </SafeAreaView>
    );
  }

  return (
    <SafeAreaView style={styles.container} edges={['bottom']}>
      {/* Encabezado del plan */}
      <View style={styles.header}>
        <Text style={styles.headerTitle}>
          {plan.asignatura} · {plan.nivel}° — Trimestre {plan.trimestre}
        </Text>
        <Text style={styles.headerSub}>
          {plan.fecha_inicio} — {plan.fecha_fin}
        </Text>
        <Pressable
          style={styles.linkBtn}
          onPress={() =>
            navigation.navigate('PlaneamientoDetalle', {
              planId:     plan.id,
              asignatura: plan.asignatura,
              nivel:      plan.nivel,
              trimestre:  plan.trimestre,
            })
          }
          accessibilityRole="button"
        >
          <Text style={styles.linkBtnText}>Ver planeamiento completo →</Text>
        </Pressable>
      </View>

      {/* Contenido Markdown */}
      <ScrollView
        contentContainerStyle={styles.content}
        contentInsetAdjustmentBehavior="automatic"
      >
        {lines.map((line, i) => (
          <MarkdownLine key={i} line={line} />
        ))}
      </ScrollView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: '#f9fafb' },
  center:    { flex: 1, justifyContent: 'center', alignItems: 'center' },
  emptyBox:  { flex: 1, justifyContent: 'center', alignItems: 'center', paddingHorizontal: 32 },
  emptyTitle: { fontSize: 16, fontWeight: '600', color: '#374151', marginBottom: 8, textAlign: 'center' },
  emptySub:   { fontSize: 13, color: '#9ca3af', textAlign: 'center', lineHeight: 20, marginBottom: 24 },
  btn:       { backgroundColor: '#1a56db', paddingHorizontal: 20, paddingVertical: 10, borderRadius: 8 },
  btnText:   { color: '#fff', fontWeight: '600', fontSize: 14 },
  header:    {
    backgroundColor: '#1a56db',
    paddingHorizontal: 16,
    paddingTop: 16,
    paddingBottom: 12,
  },
  headerTitle: { color: '#fff', fontSize: 16, fontWeight: '700' },
  headerSub:   { color: '#bfdbfe', fontSize: 12, marginTop: 2 },
  linkBtn:     { marginTop: 8, alignSelf: 'flex-start' },
  linkBtnText: { color: '#93c5fd', fontSize: 13, fontWeight: '600' },
  content:     { padding: 16 },
});

// Estilos Markdown
const md = StyleSheet.create({
  h1:        { fontSize: 20, fontWeight: '700', color: '#111827', marginTop: 16, marginBottom: 6 },
  h2:        { fontSize: 17, fontWeight: '700', color: '#1e3a8a', marginTop: 14, marginBottom: 4 },
  h3:        { fontSize: 15, fontWeight: '600', color: '#1e40af', marginTop: 10, marginBottom: 3 },
  body:      { fontSize: 14, color: '#374151', lineHeight: 22, marginBottom: 2 },
  bulletRow: { flexDirection: 'row', marginBottom: 3, paddingLeft: 4 },
  bulletDot: { fontSize: 14, color: '#6b7280', marginRight: 6, lineHeight: 22 },
  bulletText: { flex: 1, fontSize: 14, color: '#374151', lineHeight: 22 },
  hr:        { borderBottomWidth: 1, borderBottomColor: '#e5e7eb', marginVertical: 10 },
});
