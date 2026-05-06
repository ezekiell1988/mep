import React, { useCallback } from 'react';
import {
  View, Text, FlatList, Pressable,
  StyleSheet, ActivityIndicator,
} from 'react-native';
import { NativeStackScreenProps } from '@react-navigation/native-stack';
import { SafeAreaView } from 'react-native-safe-area-context';
import { useQuery } from '@powersync/react-native';
import type { LessonPlanRow } from '../powersync/schema';
import { RootStackParamList } from '../navigation/types';

type Props = NativeStackScreenProps<RootStackParamList, 'Planeamientos'>;

const NIVEL_LABEL: Record<number, string> = {
  1: '1°', 2: '2°', 3: '3°', 4: '4°', 5: '5°', 6: '6°',
  7: '7°', 8: '8°', 9: '9°', 10: '10°', 11: '11°', 12: '12°',
};

const STATUS_LABEL: Record<string, string> = {
  Pending:    'Pendiente',
  Generating: 'Generando…',
  Ready:      'Listo',
  Failed:     'Error',
};

const STATUS_COLOR: Record<string, string> = {
  Pending:    '#92400e',
  Generating: '#1e40af',
  Ready:      '#166534',
  Failed:     '#991b1b',
};

const STATUS_BG: Record<string, string> = {
  Pending:    '#fef3c7',
  Generating: '#dbeafe',
  Ready:      '#dcfce7',
  Failed:     '#fee2e2',
};

export default function PlaneamientosScreen({ navigation }: Props) {
  const { data: rows, isLoading, error } = useQuery<LessonPlanRow>(
    'SELECT * FROM lesson_plans ORDER BY created_at DESC',
  );

  const renderItem = useCallback(
    ({ item }: { item: LessonPlanRow }) => {
      const canOpen = item.status === 'Ready' && item.contenido_generado;
      return (
        <Pressable
          style={[styles.card, !canOpen && styles.cardDisabled]}
          onPress={() => {
            if (!canOpen) return;
            navigation.navigate('PlaneamientoDetalle', {
              planId:     item.id,
              asignatura: item.asignatura,
              nivel:      item.nivel,
              trimestre:  item.trimestre,
            });
          }}
          accessibilityRole="button"
          accessibilityLabel={`${item.asignatura} ${NIVEL_LABEL[item.nivel] ?? item.nivel}° — Trimestre ${item.trimestre}`}
          accessibilityState={{ disabled: !canOpen }}
        >
          <View style={styles.cardHeader}>
            <Text style={styles.cardTitle}>
              {item.asignatura} · {NIVEL_LABEL[item.nivel] ?? `${item.nivel}°`}
            </Text>
            <View style={[
              styles.badge,
              { backgroundColor: STATUS_BG[item.status] ?? '#f3f4f6' },
            ]}>
              <Text style={[
                styles.badgeText,
                { color: STATUS_COLOR[item.status] ?? '#374151' },
              ]}>
                {STATUS_LABEL[item.status] ?? item.status}
              </Text>
            </View>
          </View>
          <Text style={styles.cardSub}>
            Trimestre {item.trimestre} · {item.anio_lectivo}
          </Text>
          {item.fecha_inicio && item.fecha_fin ? (
            <Text style={styles.cardDate}>{item.fecha_inicio} — {item.fecha_fin}</Text>
          ) : null}
        </Pressable>
      );
    },
    [navigation],
  );

  if (isLoading) {
    return (
      <View style={styles.center}>
        <ActivityIndicator size="large" color="#1a56db" />
      </View>
    );
  }

  return (
    <SafeAreaView style={styles.container} edges={['bottom']}>
      {error ? <Text style={styles.error}>{String(error)}</Text> : null}
      <FlatList
        data={rows}
        keyExtractor={r => r.id}
        contentInsetAdjustmentBehavior="automatic"
        contentContainerStyle={rows.length === 0 ? styles.emptyContainer : undefined}
        ListEmptyComponent={
          <View style={styles.emptyBox}>
            <Text style={styles.emptyTitle}>Sin planeamientos</Text>
            <Text style={styles.emptySub}>
              Creá uno desde la app web en mep.ezekl.com.{'\n'}
              Aparecerá aquí una vez sincronizado.
            </Text>
          </View>
        }
        renderItem={renderItem}
      />
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container:      { flex: 1, backgroundColor: '#f9fafb' },
  center:         { flex: 1, justifyContent: 'center', alignItems: 'center' },
  error:          { color: '#ef4444', textAlign: 'center', margin: 16 },
  emptyContainer: { flex: 1, justifyContent: 'center', alignItems: 'center' },
  emptyBox:       { alignItems: 'center', paddingHorizontal: 32 },
  emptyTitle:     { fontSize: 16, fontWeight: '600', color: '#374151', marginBottom: 8 },
  emptySub:       { fontSize: 13, color: '#9ca3af', textAlign: 'center', lineHeight: 20 },
  card:           {
    backgroundColor: '#fff',
    marginHorizontal: 16,
    marginTop: 12,
    padding: 16,
    borderRadius: 10,
    elevation: 2,
    shadowColor: '#000',
    shadowOpacity: 0.06,
    shadowRadius: 4,
    shadowOffset: { width: 0, height: 2 },
  },
  cardDisabled:   { opacity: 0.65 },
  cardHeader:     { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'flex-start', gap: 8 },
  cardTitle:      { flex: 1, fontSize: 15, fontWeight: '600', color: '#111827' },
  cardSub:        { fontSize: 13, color: '#6b7280', marginTop: 4 },
  cardDate:       { fontSize: 12, color: '#9ca3af', marginTop: 2 },
  badge:          { paddingHorizontal: 8, paddingVertical: 3, borderRadius: 20 },
  badgeText:      { fontSize: 11, fontWeight: '600' },
});
