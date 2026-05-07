import { Suspense } from 'react';
import DashboardClient from './DashboardClient';

export function generateStaticParams() {
  return [];
}

export default function DashboardPage() {
  return (
    <Suspense fallback={
      <div className="min-h-screen flex items-center justify-center">
        <div role="status" aria-label="Cargando dashboard" className="w-8 h-8 border-4 border-blue-600 border-t-transparent rounded-full animate-spin" />
      </div>
    }>
      <DashboardClient />
    </Suspense>
  );
}
