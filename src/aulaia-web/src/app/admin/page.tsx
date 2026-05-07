import { Suspense } from 'react';
import AdminClient from './AdminClient';

export function generateStaticParams() {
  return [];
}

export default function AdminPage() {
  return (
    <Suspense fallback={
      <div className="min-h-screen flex items-center justify-center">
        <div className="w-8 h-8 border-4 border-blue-600 border-t-transparent rounded-full animate-spin" />
      </div>
    }>
      <AdminClient />
    </Suspense>
  );
}
