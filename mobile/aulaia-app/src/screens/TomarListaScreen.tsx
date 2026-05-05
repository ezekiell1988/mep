import React, { useCallback, useEffect, useRef, useState } from 'react';
import {
  View, Text, FlatList, Pressable,
  StyleSheet, ActivityIndicator, Alert, Switch,
} from 'react-native';
import { CameraView, useCameraPermissions } from 'expo-camera';
import { SafeAreaView } from 'react-native-safe-area-context';
import { NativeStackScreenProps } from '@react-navigation/native-stack';
import { useAuth } from '../auth/AuthContext';
import {
  getAsistencia, upsertAsistencia, scanQr,
  AsistenciaDia, AttendanceStatus,
} from '../api/endpoints';
import { RootStackParamList } from '../navigation/types';

type Props = NativeStackScreenProps<RootStackParamList, 'TomarLista'>;

const STATUS_LABELS: Record<AttendanceStatus, string> = {
  Present:   '✅ Presente',
  Absent:    '❌ Ausente',
  Late:      '🕐 Tardío',
  Justified: '📄 Justificado',
};

const STATUS_CYCLE: AttendanceStatus[] = ['Present', 'Absent', 'Late', 'Justified'];

export default function TomarListaScreen({ route }: Props) {
  const { grupo, date } = route.params;
  const { accessToken } = useAuth();

  const [records, setRecords] = useState<AsistenciaDia[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [modoQr, setModoQr] = useState(false);
  const [permission, requestPermission] = useCameraPermissions();
  const [scanning, setScanning] = useState(false);
  const lastScanned = useRef<string | null>(null);

  const load = useCallback(async () => {
    if (!accessToken) return;
    try {
      const data = await getAsistencia(accessToken, grupo.id, date);
      // Alumnos sin registro → Present por defecto al abrir lista
      setRecords(data.students.map(s => ({ ...s, status: s.status ?? 'Present' })));
    } catch {
      setRecords([]);
    } finally {
      setLoading(false);
    }
  }, [accessToken, grupo.id, date]);

  useEffect(() => { load(); }, [load]);

  // Ciclar estado al tocar: Present → Absent → Late → Justified → Present
  const cycleStatus = (studentId: string) => {
    setRecords(prev => prev.map(r => {
      if (r.studentId !== studentId) return r;
      const idx = STATUS_CYCLE.indexOf((r.status ?? 'Present') as AttendanceStatus);
      return { ...r, status: STATUS_CYCLE[(idx + 1) % STATUS_CYCLE.length] };
    }));
  };

  const handleSave = async () => {
    if (!accessToken) return;
    setSaving(true);
    try {
      await upsertAsistencia(
        accessToken,
        grupo.id,
        date,
        records.map(r => ({ studentId: r.studentId, status: (r.status ?? 'Present') as AttendanceStatus })),
      );
      Alert.alert('✅ Guardado', 'Lista de asistencia guardada.');
    } catch (e: any) {
      Alert.alert('Error', e.message ?? 'No se pudo guardar.');
    } finally {
      setSaving(false);
    }
  };

  const handleQrScan = useCallback(async ({ data: qrCode }: { data: string }) => {
    if (scanning || lastScanned.current === qrCode || !accessToken) return;
    lastScanned.current = qrCode;
    setScanning(true);
    try {
      const result = await scanQr(accessToken, grupo.id, qrCode, date);
      // Actualizar el estado local del estudiante escaneado
      setRecords(prev => prev.map(r =>
        r.studentId === result.studentId ? { ...r, status: result.status } : r,
      ));
      Alert.alert('✅ Registrado', `${result.fullName}\n${STATUS_LABELS[result.status]}`);
    } catch (e: any) {
      Alert.alert('QR no reconocido', e.message ?? 'Este código no pertenece al grupo.');
    } finally {
      setScanning(false);
      // Cooldown de 2s entre scans
      setTimeout(() => { lastScanned.current = null; }, 2000);
    }
  }, [accessToken, grupo.id, date, scanning]);

  const enableQr = async () => {
    if (!permission?.granted) {
      const result = await requestPermission();
      if (!result.granted) {
        Alert.alert('Permiso denegado', 'Se necesita acceso a la cámara para escanear QR.');
        return;
      }
    }
    setModoQr(true);
  };

  if (loading) {
    return <View style={styles.center}><ActivityIndicator size="large" color="#1a56db" /></View>;
  }

  return (
    <View style={styles.container}>
      {/* Header */}
      <View style={styles.header}>
        <Text style={styles.headerTitle}>{grupo.name}</Text>
        <Text style={styles.headerDate}>{date}</Text>
        <View style={styles.modeToggle}>
          <Text style={styles.modeLabel}>Modo QR</Text>
          <Switch value={modoQr} onValueChange={v => v ? enableQr() : setModoQr(false)} />
        </View>
      </View>

      {/* Cámara QR */}
      {modoQr && (
        <View style={styles.camera}>
          <CameraView
            style={StyleSheet.absoluteFill}
            facing="back"
            onBarcodeScanned={scanning ? undefined : handleQrScan}
          />
          {scanning && (
            <View style={styles.scanOverlay}>
              <ActivityIndicator color="#fff" size="large" />
            </View>
          )}
          <View style={styles.scanFrame} />
        </View>
      )}

      {/* Lista manual */}
      <FlatList
        data={records}
        keyExtractor={r => r.studentId}
        contentInsetAdjustmentBehavior="automatic"
        contentContainerStyle={{ paddingBottom: 100 }}
        renderItem={({ item }) => {
          const status = (item.status ?? 'Present') as AttendanceStatus;
          return (
            <Pressable style={styles.row} onPress={() => cycleStatus(item.studentId)}>
              <View style={styles.rowInfo}>
                <Text style={styles.rowName}>{item.fullName}</Text>
                <Text style={styles.rowCode}>Exp. {item.studentCode}</Text>
              </View>
              <View style={[styles.badge, styles[`badge_${status}`]]}>
                <Text style={styles.badgeText}>{STATUS_LABELS[status]}</Text>
              </View>
            </Pressable>
          );
        }}
      />

      {/* Botón guardar */}
      <SafeAreaView edges={['bottom']} style={styles.footer}>
        <Pressable style={styles.saveBtn} onPress={handleSave} disabled={saving}>
          {saving
            ? <ActivityIndicator color="#fff" />
            : <Text style={styles.saveBtnText}>Guardar lista</Text>}
        </Pressable>
      </SafeAreaView>
    </View>
  );
}

const styles = StyleSheet.create({
  container:        { flex: 1, backgroundColor: '#f9fafb' },
  center:           { flex: 1, justifyContent: 'center', alignItems: 'center' },
  header:           { backgroundColor: '#1a56db', padding: 16, paddingTop: 20 },
  headerTitle:      { color: '#fff', fontSize: 17, fontWeight: '700' },
  headerDate:       { color: '#bfdbfe', fontSize: 13, marginTop: 2 },
  modeToggle:       { flexDirection: 'row', alignItems: 'center', marginTop: 10, gap: 8 },
  modeLabel:        { color: '#fff', fontSize: 14 },
  camera:           { height: 220, position: 'relative', backgroundColor: '#000' },
  scanOverlay:      { ...StyleSheet.absoluteFillObject, backgroundColor: 'rgba(0,0,0,0.5)', justifyContent: 'center', alignItems: 'center' },
  scanFrame:        { position: 'absolute', top: '25%', left: '25%', width: '50%', height: '50%', borderWidth: 2, borderColor: '#22c55e', borderRadius: 8 },
  row:              { flexDirection: 'row', alignItems: 'center', backgroundColor: '#fff', marginHorizontal: 12, marginTop: 8, padding: 12, borderRadius: 10, elevation: 1, shadowColor: '#000', shadowOpacity: 0.04, shadowRadius: 2, shadowOffset: { width: 0, height: 1 } },
  rowInfo:          { flex: 1 },
  rowName:          { fontSize: 15, fontWeight: '500', color: '#111827' },
  rowCode:          { fontSize: 11, color: '#9ca3af' },
  badge:            { paddingHorizontal: 10, paddingVertical: 5, borderRadius: 20 },
  badge_Present:    { backgroundColor: '#dcfce7' },
  badge_Absent:     { backgroundColor: '#fee2e2' },
  badge_Late:       { backgroundColor: '#fef9c3' },
  badge_Justified:  { backgroundColor: '#dbeafe' },
  badgeText:        { fontSize: 12, fontWeight: '500' },
  footer:           { position: 'absolute', bottom: 0, left: 0, right: 0, padding: 16, backgroundColor: '#f9fafb', borderTopWidth: 1, borderTopColor: '#e5e7eb' },
  saveBtn:          { backgroundColor: '#1a56db', padding: 15, borderRadius: 10, alignItems: 'center' },
  saveBtnText:      { color: '#fff', fontSize: 16, fontWeight: '600' },
});
