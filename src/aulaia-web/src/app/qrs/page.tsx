'use client';

import { useAuth0 } from '@auth0/auth0-react';
import { useEffect, useState, useRef, useCallback, Suspense } from 'react';
import { useSearchParams, useRouter } from 'next/navigation';
import QRCode from 'qrcode';
import { getEstudiantes, type Estudiante } from '../../lib/api';

// ── Componente interno (necesita useSearchParams, debe estar en Suspense) ────

function QrsContent() {
  const { isAuthenticated, isLoading, loginWithRedirect, getAccessTokenSilently } = useAuth0();
  const searchParams = useSearchParams();
  const router = useRouter();

  const groupId   = searchParams.get('groupId')   ?? '';
  const groupName = searchParams.get('groupName') ?? '';
  const level     = searchParams.get('level')     ?? '';

  const [estudiantes, setEstudiantes] = useState<Estudiante[]>([]);
  const [qrDataUrls, setQrDataUrls]   = useState<Record<string, string>>({});
  const [loading, setLoading]         = useState(true);
  const [error, setError]             = useState<string | null>(null);
  const printRef = useRef<HTMLDivElement>(null);

  const load = useCallback(async () => {
    if (!groupId) return;
    try {
      setError(null);
      const token = await getAccessTokenSilently();
      const data  = await getEstudiantes(token, groupId);
      setEstudiantes(data);

      // Generar QR data URLs para cada estudiante usando su qrCode (UUID)
      const urls: Record<string, string> = {};
      await Promise.all(
        data.map(async (e) => {
          urls[e.studentId] = await QRCode.toDataURL(e.qrCode, {
            width: 180,
            margin: 2,
            errorCorrectionLevel: 'M',
          });
        }),
      );
      setQrDataUrls(urls);
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Error al cargar estudiantes');
    } finally {
      setLoading(false);
    }
  }, [groupId, getAccessTokenSilently]);

  useEffect(() => {
    if (!isLoading && !isAuthenticated) { loginWithRedirect(); return; }
    if (!isLoading && isAuthenticated)  { load(); }
  }, [isAuthenticated, isLoading, loginWithRedirect, load]);

  if (isLoading || loading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="w-8 h-8 border-4 border-blue-600 border-t-transparent rounded-full animate-spin" />
      </div>
    );
  }

  return (
    <div className="max-w-4xl mx-auto px-4 py-8">
      {/* Controles — se ocultan al imprimir */}
      <div className="flex items-center gap-4 mb-8 print:hidden">
        <button
          onClick={() => router.back()}
          className="text-sm text-gray-500 hover:text-gray-700 transition-colors"
        >
          ← Volver
        </button>
        <div className="flex-1">
          <h1 className="text-xl font-bold text-gray-900">{groupName}</h1>
          <p className="text-sm text-gray-500">{level} · {estudiantes.length} estudiantes</p>
        </div>
        <button
          onClick={() => window.print()}
          className="bg-blue-600 hover:bg-blue-700 text-white font-semibold px-6 py-2 rounded-xl text-sm transition-colors"
        >
          🖨 Imprimir / Guardar PDF
        </button>
      </div>

      {error !== null ? (
        <div className="bg-red-50 border border-red-200 text-red-700 rounded-xl p-4">{error}</div>
      ) : null}

      {/* Hoja de QRs — se imprime */}
      <div ref={printRef}>
        {/* Encabezado visible solo en impresión */}
        <div className="hidden print:block mb-6 text-center">
          <h2 className="text-xl font-bold">{groupName}</h2>
          <p className="text-sm text-gray-600">{level} — Códigos QR de asistencia</p>
        </div>

        <div className="grid grid-cols-3 gap-4 sm:grid-cols-4">
          {estudiantes.map((e) => (
            <div
              key={e.studentId}
              className="bg-white border border-gray-200 rounded-xl p-3 flex flex-col items-center gap-2 shadow-sm print:shadow-none print:border-gray-300"
            >
              {qrDataUrls[e.studentId] ? (
                // eslint-disable-next-line @next/next/no-img-element
                <img
                  src={qrDataUrls[e.studentId]}
                  alt={`QR ${e.fullName}`}
                  className="w-32 h-32"
                />
              ) : (
                <div className="w-32 h-32 bg-gray-100 rounded flex items-center justify-center text-xs text-gray-400">
                  Generando…
                </div>
              )}
              <p className="text-xs font-medium text-center text-gray-800 leading-tight">{e.fullName}</p>
              <p className="text-xs text-gray-400">Exp. {e.studentCode}</p>
            </div>
          ))}
        </div>
      </div>

      {/* Estilos de impresión */}
      <style>{`
        @media print {
          body { background: white; }
          .print\\:hidden { display: none !important; }
          .print\\:block { display: block !important; }
          .print\\:shadow-none { box-shadow: none !important; }
          .print\\:border-gray-300 { border-color: #d1d5db !important; }
        }
      `}</style>
    </div>
  );
}

// ── Page con Suspense (requerido por useSearchParams en output: export) ──────

export default function QrsPage() {
  return (
    <Suspense fallback={
      <div className="min-h-screen flex items-center justify-center">
        <div className="w-8 h-8 border-4 border-blue-600 border-t-transparent rounded-full animate-spin" />
      </div>
    }>
      <QrsContent />
    </Suspense>
  );
}
