import React from 'react';
import { NavigationContainer } from '@react-navigation/native';
import { createNativeStackNavigator } from '@react-navigation/native-stack';
import { Pressable, Text, ActivityIndicator, View } from 'react-native';
import { useAuth } from '../auth/AuthContext';
import LoginScreen from '../screens/LoginScreen';
import GruposScreen from '../screens/GruposScreen';
import EstudiantesScreen from '../screens/EstudiantesScreen';
import TomarListaScreen from '../screens/TomarListaScreen';
import PlaneamientosScreen from '../screens/PlaneamientosScreen';
import PlaneamientoDetalleScreen from '../screens/PlaneamientoDetalleScreen';
import NotasScreen from '../screens/NotasScreen';
import { RootStackParamList } from './types';

const Stack = createNativeStackNavigator<RootStackParamList>();

export default function AppNavigator() {
  const { isAuthenticated, isLoading } = useAuth();

  if (isLoading) {
    return <View style={{ flex: 1, justifyContent: 'center', alignItems: 'center' }}><ActivityIndicator size="large" color="#1a56db" /></View>;
  }

  if (!isAuthenticated) {
    return <LoginScreen />;
  }

  return (
    <NavigationContainer>
      <Stack.Navigator
        screenOptions={{
          headerStyle: { backgroundColor: '#1a56db' },
          headerTintColor: '#fff',
          headerTitleStyle: { fontWeight: '700' },
        }}
      >
        <Stack.Screen
          name="Grupos"
          component={GruposScreen}
          options={({ navigation }) => ({
            title: 'Mis Grupos',
            headerRight: () => (
              <Pressable
                onPress={() => navigation.navigate('Planeamientos')}
                accessibilityLabel="Ver planeamientos"
                style={{ marginRight: 4, paddingHorizontal: 8, paddingVertical: 4 }}
              >
                <Text style={{ color: '#fff', fontSize: 13, fontWeight: '600' }}>Planeamientos</Text>
              </Pressable>
            ),
          })}
        />
        <Stack.Screen
          name="Estudiantes"
          component={EstudiantesScreen}
          options={({ route, navigation }) => ({
            title: route.params.grupo.name,
            headerRight: () => (
              <Pressable
                onPress={() => navigation.navigate('Notas', { grupo: route.params.grupo })}
                accessibilityLabel="Ver notas del grupo"
                style={{ marginRight: 4, paddingHorizontal: 8, paddingVertical: 4 }}
              >
                <Text style={{ color: '#fff', fontSize: 13, fontWeight: '600' }}>Notas</Text>
              </Pressable>
            ),
          })}
        />
        <Stack.Screen
          name="TomarLista"
          component={TomarListaScreen}
          options={{ title: 'Tomar Lista' }}
        />
        <Stack.Screen
          name="Planeamientos"
          component={PlaneamientosScreen}
          options={{ title: 'Planeamientos' }}
        />
        <Stack.Screen
          name="PlaneamientoDetalle"
          component={PlaneamientoDetalleScreen}
          options={({ route }) => ({
            title: `${route.params.asignatura} ${route.params.nivel}° — T${route.params.trimestre}`,
          })}
        />
        <Stack.Screen
          name="Notas"
          component={NotasScreen}
          options={({ route }) => ({
            title: `Notas · ${route.params.grupo.name}`,
          })}
        />
      </Stack.Navigator>
    </NavigationContainer>
  );
}
