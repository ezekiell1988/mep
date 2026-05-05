import React from 'react';
import { NavigationContainer } from '@react-navigation/native';
import { createNativeStackNavigator } from '@react-navigation/native-stack';
import { useAuth } from '../auth/AuthContext';
import LoginScreen from '../screens/LoginScreen';
import GruposScreen from '../screens/GruposScreen';
import EstudiantesScreen from '../screens/EstudiantesScreen';
import TomarListaScreen from '../screens/TomarListaScreen';
import { RootStackParamList } from './types';
import { ActivityIndicator, View } from 'react-native';

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
        <Stack.Screen name="Grupos" component={GruposScreen} options={{ title: 'Mis Grupos' }} />
        <Stack.Screen
          name="Estudiantes"
          component={EstudiantesScreen}
          options={({ route }) => ({ title: route.params.grupo.name })}
        />
        <Stack.Screen
          name="TomarLista"
          component={TomarListaScreen}
          options={{ title: 'Tomar Lista' }}
        />
      </Stack.Navigator>
    </NavigationContainer>
  );
}
