import React, { useCallback } from 'react';
import {
  View, Text, FlatList, Pressable,
  StyleSheet, ActivityIndicator,
} from 'react-native';
import { NativeStackScreenProps } from '@react-navigation/native-stack';
import { useQuery } from '@powersync/react-native';
import type { StudentRow } from '../powersync/schema';
import { RootStackParamList } from '../navigation/types';

type Props = NativeStackScreenProps<RootStackParamList, 'Estudiantes'>;

export default function EstudiantesScreen({ navigation, route }: Props) {
  const { grupo } = route.params;

  // Lee del SQLite local: funciona offline y se actualiza en tiempo real al sincronizar.
  const { data: rows, isLoading, error } = useQuery<StudentRow>(
    'SELECT * FROM students WHERE group_id = ? ORDER BY full_name',
    [grupo.id],
  );

  const today = new Date().toISOString().split('T')[0];

  const renderItem = useCallback(
    ({ item }: { item: StudentRow }) => (
      <View style={styles.card}>
        <Text style={styles.cardName}>{item.full_name}</Text>
        <Text style={styles.cardCode}>Exp. {item.student_code}</Text>
      </View>
    ),
    [],
  );

  if (isLoading) {
    return <View style={styles.center}><ActivityIndicator size="large" color="#1a56db" /></View>;
  }

  return (
    <View style={styles.container}>
      {error ? <Text style={styles.error}>{String(error)}</Text> : null}
      <Pressable
        style={styles.tomarListaBtn}
        onPress={() => navigation.navigate('TomarLista', { grupo, date: today })}
      >
        <Text style={styles.tomarListaText}>📋 Tomar Lista — Hoy</Text>
      </Pressable>
      <FlatList
        data={rows}
        keyExtractor={e => e.id}
        contentInsetAdjustmentBehavior="automatic"
        contentContainerStyle={rows.length === 0 ? styles.center : { paddingBottom: 24 }}
        ListEmptyComponent={<Text style={styles.empty}>No hay estudiantes en este grupo.</Text>}
        renderItem={renderItem}
      />
    </View>
  );
}

const styles = StyleSheet.create({
  container:       { flex: 1, backgroundColor: '#f9fafb' },
  center:          { flex: 1, justifyContent: 'center', alignItems: 'center' },
  error:           { color: '#ef4444', textAlign: 'center', margin: 16 },
  empty:           { color: '#9ca3af', textAlign: 'center' },
  tomarListaBtn:   { backgroundColor: '#1a56db', margin: 16, padding: 14, borderRadius: 10, alignItems: 'center' },
  tomarListaText:  { color: '#fff', fontSize: 15, fontWeight: '600' },
  card:            { backgroundColor: '#fff', marginHorizontal: 16, marginTop: 10, padding: 14, borderRadius: 10, elevation: 1, shadowColor: '#000', shadowOpacity: 0.04, shadowRadius: 3, shadowOffset: { width: 0, height: 1 } },
  cardName:        { fontSize: 15, fontWeight: '500', color: '#111827' },
  cardCode:        { fontSize: 12, color: '#9ca3af', marginTop: 2 },
});

