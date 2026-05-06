import React, { useMemo } from 'react';
import {
  View, Text, ScrollView, StyleSheet, ActivityIndicator,
} from 'react-native';
import { NativeStackScreenProps } from '@react-navigation/native-stack';
import { SafeAreaView } from 'react-native-safe-area-context';
import { useQuery } from '@powersync/react-native';
import type { LessonPlanRow } from '../powersync/schema';
import { RootStackParamList } from '../navigation/types';

type Props = NativeStackScreenProps<RootStackParamList, 'PlaneamientoDetalle'>;

// Renderizador Markdown mínimo: convierte líneas de texto plano con marcas
// comunes en componentes Text estilizados. Sin dependencias externas.
function MarkdownLine({ line }: { line: string }) {
  if (line === '') return <View style={{ height: 8 }} />;

  // Headings
  const h3 = line.match(/^###\s+(.*)/);
  if (h3) return <Text style={md.h3}>{h3[1]}</Text>;
  const h2 = line.match(/^##\s+(.*)/);
  if (h2) return <Text style={md.h2}>{h2[1]}</Text>;
  const h1 = line.match(/^#\s+(.*)/);
  if (h1) return <Text style={md.h1}>{h1[1]}</Text>;

  // Separador horizontal
  if (/^-{3,}$/.test(line.trim()) || /^\*{3,}$/.test(line.trim())) {
    return <View style={md.hr} />;
  }

  // Bullet list
  const bullet = line.match(/^[-*]\s+(.*)/);
  if (bullet) {
    return (
      <View style={md.bulletRow}>
        <Text style={md.bulletDot}>•</Text>
        <Text style={md.bulletText}>{stripInline(bullet[1])}</Text>
      </View>
    );
  }

  // Numbered list
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

  // Código inline o bloque simple (``` línea)
  if (line.startsWith('```')) return null; // ignorar delimitadores de bloque

  return <Text style={md.body}>{stripInline(line)}</Text>;
}

/** Elimina marcas inline (**bold**, *italic*, `code`) para texto plano */
function stripInline(text: string): string {
  return text
    .replace(/\*\*(.+?)\*\*/g, '$1')
    .replace(/\*(.+?)\*/g, '$1')
    .replace(/`(.+?)`/g, '$1')
    .replace(/\[(.+?)\]\(.+?\)/g, '$1');
}

export default function PlaneamientoDetalleScreen({ route }: Props) {
  const { planId } = route.params;

  const { data: rows, isLoading } = useQuery<LessonPlanRow>(
    'SELECT * FROM lesson_plans WHERE id = ?',
    [planId],
  );

  const plan = rows[0] ?? null;
  const lines = useMemo(
    () => (plan?.contenido_generado ?? '').split('\n'),
    [plan?.contenido_generado],
  );

  if (isLoading) {
    return (
      <View style={styles.center}>
        <ActivityIndicator size="large" color="#1a56db" />
      </View>
    );
  }

  if (!plan || !plan.contenido_generado) {
    return (
      <View style={styles.center}>
        <Text style={styles.empty}>El planeamiento aún no tiene contenido.</Text>
      </View>
    );
  }

  return (
    <SafeAreaView style={styles.container} edges={['bottom']}>
      <ScrollView
        contentInsetAdjustmentBehavior="automatic"
        contentContainerStyle={styles.content}
      >
        {lines.map((line, idx) => (
          // eslint-disable-next-line react/no-array-index-key
          <MarkdownLine key={idx} line={line} />
        ))}
      </ScrollView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: '#fff' },
  center:    { flex: 1, justifyContent: 'center', alignItems: 'center', padding: 24 },
  empty:     { color: '#6b7280', textAlign: 'center' },
  content:   { padding: 20, paddingBottom: 40 },
});

const md = StyleSheet.create({
  h1:         { fontSize: 22, fontWeight: '800', color: '#111827', marginTop: 20, marginBottom: 6 },
  h2:         { fontSize: 18, fontWeight: '700', color: '#1f2937', marginTop: 16, marginBottom: 4 },
  h3:         { fontSize: 15, fontWeight: '700', color: '#374151', marginTop: 12, marginBottom: 2 },
  body:       { fontSize: 14, color: '#374151', lineHeight: 22, marginBottom: 2 },
  bulletRow:  { flexDirection: 'row', alignItems: 'flex-start', marginBottom: 2, paddingLeft: 4 },
  bulletDot:  { fontSize: 14, color: '#6b7280', marginRight: 8, lineHeight: 22, minWidth: 14 },
  bulletText: { flex: 1, fontSize: 14, color: '#374151', lineHeight: 22 },
  hr:         { height: 1, backgroundColor: '#e5e7eb', marginVertical: 12 },
});
