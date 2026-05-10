import { Suspense } from 'react';
import DirectorClient from './DirectorClient';

export function generateStaticParams() {
  return [];
}

export default function DirectorPage() {
  return (
    <Suspense fallback={
      <div className="min-h-screen flex items-center justify-center">
        <div role="status" aria-label="Cargando panel de director" className="w-8 h-8 border-4 border-blue-600 border-t-transparent rounded-full animate-spin" />
      </div>
    }>
      <DirectorClient />
    </Suspense>
  );
}
