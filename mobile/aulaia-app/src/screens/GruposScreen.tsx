import React, { useCallback } from 'react';
import {
  View, Text, FlatList, Pressable,
  StyleSheet, ActivityIndicator,
} from 'react-native';
import { NativeStackScreenProps } from '@react-navigation/native-stack';
import { SafeAreaView } from 'react-native-safe-area-context';
import { useQuery } from '@powersync/react-native';
import { useAuth } from '../auth/AuthContext';
import type { GroupRow } from '../powersync/schema';
import { RootStackParamList } from '../navigation/types';
import { Grupo } from '../api/endpoints';
import { useRendimientoNotifications } from '../lib/useRendimientoNotifications';

type Props = NativeStackScreenProps<RootStackParamList, 'Grupos'>;

/** Convierte una fila del SQLite local al tipo Grupo que espera la navegación */
function rowToGrupo(row: GroupRow): Grupo {
  return {
    id:         row.id,
    name:       row.name,
    level:      row.level,
    subject:    row.subject,
    schoolYear: row.school_year,
    teacherId:  row.teacher_id,
  };
}

export default function GruposScreen({ navigation }: Props) {
  const { logout } = useAuth();

  // Notificaciones de alerta de rendimiento: se disparan cuando hay estudiantes con promedio < 65
  useRendimientoNotifications();

  // useQuery suscribe al SQLite local y se actualiza automáticamente cuando PowerSync sincroniza.
  const { data: rows, isLoading, error } = useQuery<GroupRow>(
    'SELECT * FROM groups WHERE is_active = 1 ORDER BY name',
  );

  const grupos = rows.map(rowToGrupo);

  const renderItem = useCallback(
    ({ item }: { item: Grupo }) => (
      <Pressable
        style={styles.card}
        onPress={() => navigation.navigate('Estudiantes', { grupo: item })}
      >
        <Text style={styles.cardTitle}>{item.name}</Text>
        <Text style={styles.cardSub}>{item.subject} · {item.level} · {item.schoolYear}</Text>
      </Pressable>
    ),
    [navigation],
  );

  if (isLoading) {
    return <View style={styles.center}><ActivityIndicator size="large" color="#1a56db" /></View>;
  }

  return (
    <SafeAreaView style={styles.container} edges={['bottom']}>
      {error ? <Text style={styles.error}>{String(error)}</Text> : null}
      <FlatList
        data={grupos}
        keyExtractor={g => g.id}
        contentInsetAdjustmentBehavior="automatic"
        contentContainerStyle={grupos.length === 0 ? styles.center : undefined}
        ListEmptyComponent={<Text style={styles.empty}>No hay grupos activos.</Text>}
        renderItem={renderItem}
      />
      <Pressable style={styles.logoutBtn} onPress={logout}>
        <Text style={styles.logoutText}>Cerrar sesión</Text>
      </Pressable>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container:   { flex: 1, backgroundColor: '#f9fafb' },
  center:      { flex: 1, justifyContent: 'center', alignItems: 'center' },
  error:       { color: '#ef4444', textAlign: 'center', margin: 16 },
  empty:       { color: '#9ca3af', textAlign: 'center' },
  card:        { backgroundColor: '#fff', marginHorizontal: 16, marginTop: 12, padding: 16, borderRadius: 10, elevation: 2, shadowColor: '#000', shadowOpacity: 0.06, shadowRadius: 4, shadowOffset: { width: 0, height: 2 } },
  cardTitle:   { fontSize: 16, fontWeight: '600', color: '#111827' },
  cardSub:     { fontSize: 13, color: '#6b7280', marginTop: 2 },
  logoutBtn:   { margin: 16, padding: 12, alignItems: 'center' },
  logoutText:  { color: '#ef4444', fontSize: 14 },
});

