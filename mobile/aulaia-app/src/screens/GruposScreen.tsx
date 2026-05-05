import React, { useCallback, useEffect, useState } from 'react';
import {
  View, Text, FlatList, Pressable,
  StyleSheet, ActivityIndicator, RefreshControl,
} from 'react-native';
import { NativeStackScreenProps } from '@react-navigation/native-stack';
import { SafeAreaView } from 'react-native-safe-area-context';
import { useAuth } from '../auth/AuthContext';
import { getGrupos, Grupo } from '../api/endpoints';
import { RootStackParamList } from '../navigation/types';

type Props = NativeStackScreenProps<RootStackParamList, 'Grupos'>;

export default function GruposScreen({ navigation }: Props) {
  const { accessToken, logout } = useAuth();
  const [grupos, setGrupos] = useState<Grupo[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    if (!accessToken) return;
    try {
      setError(null);
      const data = await getGrupos(accessToken);
      setGrupos(data);
    } catch (e: any) {
      setError(e.message ?? 'Error al cargar grupos');
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, [accessToken]);

  useEffect(() => { load(); }, [load]);

  const onRefresh = () => { setRefreshing(true); load(); };

  if (loading) {
    return <View style={styles.center}><ActivityIndicator size="large" color="#1a56db" /></View>;
  }

  return (
    <SafeAreaView style={styles.container} edges={['bottom']}>
      {error ? <Text style={styles.error}>{error}</Text> : null}
      <FlatList
        data={grupos}
        keyExtractor={g => g.id}
        refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} />}
        contentInsetAdjustmentBehavior="automatic"
        contentContainerStyle={grupos.length === 0 ? styles.center : undefined}
        ListEmptyComponent={<Text style={styles.empty}>No hay grupos activos.</Text>}
        renderItem={({ item }) => (
          <Pressable
            style={styles.card}
            onPress={() => navigation.navigate('Estudiantes', { grupo: item })}
          >
            <Text style={styles.cardTitle}>{item.name}</Text>
            <Text style={styles.cardSub}>{item.subject} · {item.level} · {item.schoolYear}</Text>
          </Pressable>
        )}
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
