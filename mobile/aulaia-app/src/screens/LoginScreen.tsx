import React from 'react';
import { View, Text, TouchableOpacity, StyleSheet, ActivityIndicator } from 'react-native';
import { useAuth } from '../auth/AuthContext';

export default function LoginScreen() {
  const { login, isLoading } = useAuth();
  const [loading, setLoading] = React.useState(false);

  const handleLogin = async () => {
    setLoading(true);
    try {
      await login();
    } finally {
      setLoading(false);
    }
  };

  if (isLoading) {
    return (
      <View style={styles.center}>
        <ActivityIndicator size="large" color="#1a56db" />
      </View>
    );
  }

  return (
    <View style={styles.container}>
      <Text style={styles.title}>AulaIA</Text>
      <Text style={styles.subtitle}>Asistente pedagógico para docentes del MEP</Text>
      <TouchableOpacity style={styles.button} onPress={handleLogin} disabled={loading}>
        {loading
          ? <ActivityIndicator color="#fff" />
          : <Text style={styles.buttonText}>Ingresar con Auth0</Text>}
      </TouchableOpacity>
    </View>
  );
}

const styles = StyleSheet.create({
  center:    { flex: 1, justifyContent: 'center', alignItems: 'center' },
  container: { flex: 1, justifyContent: 'center', alignItems: 'center', padding: 32, backgroundColor: '#f9fafb' },
  title:     { fontSize: 36, fontWeight: '700', color: '#1a56db', marginBottom: 8 },
  subtitle:  { fontSize: 15, color: '#6b7280', textAlign: 'center', marginBottom: 48 },
  button:    { backgroundColor: '#1a56db', paddingVertical: 14, paddingHorizontal: 40, borderRadius: 10 },
  buttonText:{ color: '#fff', fontSize: 16, fontWeight: '600' },
});
