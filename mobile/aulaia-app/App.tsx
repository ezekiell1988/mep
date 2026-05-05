import 'react-native-get-random-values';
import React from 'react';
import { AuthProvider } from './src/auth/AuthContext';
import { PowerSyncProvider } from './src/powersync/PowerSyncContext';
import AppNavigator from './src/navigation/AppNavigator';

export default function App() {
  return (
    <AuthProvider>
      <PowerSyncProvider>
        <AppNavigator />
      </PowerSyncProvider>
    </AuthProvider>
  );
}
