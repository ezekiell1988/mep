import React, { useCallback, useEffect, useState } from 'react';
import {
  View, Text, FlatList, Pressable,
  StyleSheet, ActivityIndicator, RefreshControl,
} from 'react-native';
import { NativeStackScreenProps } from '@react-navigation/native-stack';
import { useAuth } from '../auth/AuthContext';
import { getEstudiantes, Estudiante } from '../api/endpoints';
import { RootStackParamList } from '../navigation/types';

type Props = NativeStackScreenProps<RootStackParamList, 'Estudiantes'>;

export default function EstudiantesScreen({ navigation, route }: Props) {
  const { grupo } = route.params;
  const { accessToken } = useAuth();
  const [estudiantes, setEstudiantes] = useState<Estudiante[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    if (!accessToken) return;
    try {
      setError(null);
      const data = await getEstudiantes(accessToken, grupo.id);
      setEstudiantes(data);
    } catch (e: any) {
      setError(e.message ?? 'Error al cargar estudiantes');
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, [accessToken, grupo.id]);

  useEffect(() => { load(); }, [load]);

  const onRefresh = () => { setRefreshing(true); load(); };

  const today = new Date().toISOString().split('T')[0];

  if (loading) {
    return <View style={styles.center}><ActivityIndicator size="large" color="#1a56db" /></View>;
  }

  return (
    <View style={styles.container}>
      {error ? <Text style={styles.error}>{error}</Text> : null}
      <Pressable
        style={styles.tomarListaBtn}
        onPress={() => navigation.navigate('TomarLista', { grupo, date: today })}
      >
        <Text style={styles.tomarListaText}>📋 Tomar Lista — Hoy</Text>
      </Pressable>
      <FlatList
        data={estudiantes}
        keyExtractor={e => e.studentId}
        refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} />}
        contentInsetAdjustmentBehavior="automatic"
        contentContainerStyle={estudiantes.length === 0 ? styles.center : { paddingBottom: 24 }}
        ListEmptyComponent={<Text style={styles.empty}>No hay estudiantes en este grupo.</Text>}
        renderItem={({ item }) => (
          <View style={styles.card}>
            <Text style={styles.cardName}>{item.fullName}</Text>
            <Text style={styles.cardCode}>Exp. {item.studentCode}</Text>
          </View>
        )}
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
