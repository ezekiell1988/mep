'use client';

import { useAuth0 } from '@auth0/auth0-react';
import { useEffect, useState, useCallback, useRef, Suspense } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
import { getPlaneamiento, getPlaneamientoPdfUrl, type PlaneamientoResponse } from '../../../lib/api';

const POLL_INTERVAL_MS = 3000;

function downloadMarkdown(contenido: string, filename: string) {
  const blob = new Blob([contenido], { type: 'text/markdown;charset=utf-8' });
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = filename;
  document.body.appendChild(a);
  a.click();
  a.remove();
  window.setTimeout(() => URL.revokeObjectURL(url), 0);
}

function getDownloadFilename(contentDisposition: string | null, fallback: string) {
  const match = contentDisposition?.match(/filename\*?=(?:UTF-8'')?["']?([^"';]+)["']?/i);
  return match?.[1] ? decodeURIComponent(match[1]) : fallback;
}

function DetalleContent() {
  const { isAuthenticated, isLoading, loginWithRedirect, getAccessTokenSilently } = useAuth0();
  const router = useRouter();
  const searchParams = useSearchParams();
  const id = searchParams.get('id');

  const [plan, setPlan] = useState<PlaneamientoResponse | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [downloadError, setDownloadError] = useState<string | null>(null);
  const [downloadingPdf, setDownloadingPdf] = useState(false);
  const pollRef = useRef<ReturnType<typeof setInterval> | null>(null);

  const fetchPlan = useCallback(async () => {
    if (!id) return;
    try {
      const token = await getAccessTokenSilently();
      const data = await getPlaneamiento(token, id);
      setPlan(data);
      if (data.status === 'Ready' || data.status === 'Failed') {
        if (pollRef.current) clearInterval(pollRef.current);
      }
    } catch (e: unknown) {
      setError(e instanceof Error ? e.message : 'Error al cargar el planeamiento');
      if (pollRef.current) clearInterval(pollRef.current);
    }
  }, [id, getAccessTokenSilently]);

  useEffect(() => {
    if (!isLoading && !isAuthenticated) loginWithRedirect();
  }, [isAuthenticated, isLoading, loginWithRedirect]);

  useEffect(() => {
    if (!isAuthenticated || isLoading || !id) return;
    fetchPlan();
    pollRef.current = setInterval(() => {
      setPlan(prev => {
        if (prev?.status === 'Ready' || prev?.status === 'Failed') {
          if (pollRef.current) clearInterval(pollRef.current);
        }
        return prev;
      });
      fetchPlan();
    }, POLL_INTERVAL_MS);
    return () => { if (pollRef.current) clearInterval(pollRef.current); };
  }, [isAuthenticated, isLoading, id, fetchPlan]);

  const downloadPdf = useCallback(async () => {
    if (!id) return;
    setDownloadError(null);
    setDownloadingPdf(true);
    try {
      const token = await getAccessTokenSilently();
      const res = await fetch(getPlaneamientoPdfUrl(id), {
        headers: { Authorization: `Bearer ${token}` },
      });
      if (!res.ok) {
        const text = await res.text().catch(() => '');
        throw new Error(text || `Error ${res.status}`);
      }
      const blob = await res.blob();
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = getDownloadFilename(res.headers.get('content-disposition'), `planeamiento-${id.slice(0, 8)}.pdf`);
      document.body.appendChild(a);
      a.click();
      a.remove();
      window.setTimeout(() => URL.revokeObjectURL(url), 0);
    } catch (e: unknown) {
      setDownloadError(e instanceof Error ? e.message : 'No se pudo descargar el PDF');
    } finally {
      setDownloadingPdf(false);
    }
  }, [id, getAccessTokenSilently]);

  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div role="status" aria-label="Cargando" className="w-8 h-8 border-4 border-blue-600 border-t-transparent rounded-full animate-spin" />
      </div>
    );
  }

  if (error !== null) {
    return (
      <div className="max-w-3xl mx-auto px-4 py-8">
        <button type="button" onClick={() => router.push('/planeamiento')} className="text-sm text-gray-400 hover:text-gray-600 mb-4" aria-label="Volver a Planeamientos"><span aria-hidden="true">←</span> Planeamientos</button>
        <div role="alert" className="bg-red-50 border border-red-200 text-red-700 rounded-xl p-4">{error}</div>
      </div>
    );
  }

  if (!plan || plan.status === 'Pending' || plan.status === 'Generating') {
    return (
      <div className="max-w-3xl mx-auto px-4 py-8">
        <button type="button" onClick={() => router.push('/planeamiento')} className="text-sm text-gray-400 hover:text-gray-600 mb-4" aria-label="Volver a Planeamientos"><span aria-hidden="true">←</span> Planeamientos</button>
        <div className="bg-white rounded-2xl border border-gray-200 shadow-sm p-10 flex flex-col items-center gap-4">
          <div role="status" aria-label="Generando planeamiento" className="w-10 h-10 border-4 border-blue-600 border-t-transparent rounded-full animate-spin" />
          <p className="text-gray-600 font-medium">La IA está generando el planeamiento…</p>
          <p className="text-sm text-gray-400">Esto puede tomar entre 30 y 90 segundos. La página se actualizará automáticamente.</p>
        </div>
      </div>
    );
  }

  if (plan.status === 'Failed') {
    return (
      <div className="max-w-3xl mx-auto px-4 py-8">
        <button type="button" onClick={() => router.push('/planeamiento')} className="text-sm text-gray-400 hover:text-gray-600 mb-4" aria-label="Volver a Planeamientos"><span aria-hidden="true">←</span> Planeamientos</button>
        <div role="alert" className="bg-red-50 border border-red-200 text-red-700 rounded-xl p-6 text-center">
          <p className="font-semibold mb-1">El planeamiento falló</p>
          <p className="text-sm">Intenta crear uno nuevo. Si el problema persiste, revisa los logs del servidor.</p>
        </div>
      </div>
    );
  }

  const contenido = plan.contenido ?? '';
  const filename = `planeamiento-${id?.slice(0, 8)}.md`;

  return (
    <div className="max-w-4xl mx-auto px-4 py-8">
      {/* Header */}
      <div className="flex items-center justify-between mb-6 flex-wrap gap-3">
        <button type="button" onClick={() => router.push('/planeamiento')} className="text-sm text-gray-400 hover:text-gray-600 flex items-center gap-1" aria-label="Volver a Planeamientos">
          <span aria-hidden="true">←</span> Planeamientos
        </button>
        <div className="flex gap-2">
          <button
            type="button"
            onClick={() => downloadMarkdown(contenido, filename)}
            aria-label="Descargar planeamiento como Markdown"
            className="text-sm border border-gray-300 hover:bg-gray-50 text-gray-700 font-medium px-4 py-2 rounded-lg transition-colors"
          >
            <span aria-hidden="true">⬇</span> Descargar .md
          </button>
          <button
            type="button"
            onClick={downloadPdf}
            disabled={downloadingPdf}
            aria-label="Descargar planeamiento como PDF"
            className="text-sm bg-blue-600 hover:bg-blue-700 disabled:bg-blue-300 text-white font-medium px-4 py-2 rounded-lg transition-colors"
          >
            <span aria-hidden="true">⬇</span> {downloadingPdf ? 'Generando PDF…' : 'Descargar PDF'}
          </button>
        </div>
      </div>

      {downloadError !== null ? (
        <div role="alert" className="bg-red-50 border border-red-200 text-red-700 rounded-xl p-4 mb-6 text-sm">
          {downloadError}
        </div>
      ) : null}

      {/* Contenido Markdown */}
      <div
        className="bg-white rounded-2xl border border-gray-200 shadow-sm p-8 prose prose-gray max-w-none print:shadow-none print:border-none print:rounded-none print:p-0"
      >
        <ReactMarkdown remarkPlugins={[remarkGfm]}>
          {contenido}
        </ReactMarkdown>
      </div>
    </div>
  );
}

export default function DetallePlaneamientoPage() {
  return (
    <Suspense fallback={
      <div className="min-h-screen flex items-center justify-center">
        <div role="status" aria-label="Cargando" className="w-8 h-8 border-4 border-blue-600 border-t-transparent rounded-full animate-spin" />
      </div>
    }>
      <DetalleContent />
    </Suspense>
  );
}
