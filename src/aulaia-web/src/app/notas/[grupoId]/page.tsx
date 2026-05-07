import { Suspense } from 'react';
import PageClient from './PageClient';

export function generateStaticParams() {
  return [{ grupoId: '_' }];
}

export default function Page({ params }: { params: Promise<{ grupoId: string }> }) {
  return (
    <Suspense fallback={null}>
      <PageClient params={params} />
    </Suspense>
  );
}
