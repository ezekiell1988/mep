import { Suspense } from 'react';
import SuscripcionClient from './SuscripcionClient';

export function generateStaticParams() {
  return [];
}

export default function SuscripcionPage() {
  return (
    <Suspense fallback={
      <div className="min-h-screen flex items-center justify-center">
        <div className="w-8 h-8 border-4 border-blue-600 border-t-transparent rounded-full animate-spin" />
      </div>
    }>
      <SuscripcionClient />
    </Suspense>
  );
}
