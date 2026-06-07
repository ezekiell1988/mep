'use client';

import { useAuth0 } from '@auth0/auth0-react';
import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';

export default function HomePage() {
  const { isAuthenticated, isLoading, loginWithRedirect, getAccessTokenSilently } = useAuth0();
  const router = useRouter();
  const [hangfireMsg, setHangfireMsg] = useState<string | null>(null);

  useEffect(() => {
    if (isLoading) return;
    const params = typeof window !== 'undefined'
      ? new URLSearchParams(window.location.search)
      : null;
    const isHangfireReturn = params?.get('hangfire_return') === '1';

    if (isHangfireReturn) {
      if (!isAuthenticated) {
        // Sin sesión → ir a Auth0 y volver aquí
        void loginWithRedirect({
          authorizationParams: {
            redirect_uri: window.location.origin + '/callback',
            audience:     'https://api.aulaia.mep.go.cr',
            scope:        'openid profile email',
          },
          appState: { returnTo: '/?hangfire_return=1' },
        });
        return;
      }
      // Ya autenticado: obtener token y crear cookie de Hangfire directamente
      setHangfireMsg('Accediendo al dashboard…');
      getAccessTokenSilently({ authorizationParams: { audience: 'https://api.aulaia.mep.go.cr' } })
        .then(token => fetch('/hangfire-session', {
          method: 'POST',
          headers: { Authorization: 'Bearer ' + token },
        }))
        .then(resp => {
          if (resp.ok) {
            window.location.replace('/hangfire');
          } else if (resp.status === 403) {
            setHangfireMsg('Acceso denegado: se requiere rol admin.');
          } else {
            setHangfireMsg('Error al verificar sesión (' + resp.status + ').');
          }
        })
        .catch(() => setHangfireMsg('Error de red al conectar con Hangfire.'));
      return;
    }

    if (isAuthenticated) {
      router.replace('/dashboard');
    }
  }, [isAuthenticated, isLoading, loginWithRedirect, getAccessTokenSilently, router]);

  if (hangfireMsg) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <p className="text-gray-400">{hangfireMsg}</p>
      </div>
    );
  }

  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="w-8 h-8 border-4 border-blue-600 border-t-transparent rounded-full animate-spin" />
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-white">
      {/* Nav */}
      <header className="flex items-center justify-between px-6 py-4 max-w-6xl mx-auto">
        <span className="text-xl font-bold text-gray-900">AulaIA</span>
        <button
          onClick={() => loginWithRedirect()}
          className="text-sm text-gray-600 hover:text-gray-900 font-medium transition-colors"
        >
          Iniciar sesión
        </button>
      </header>

      {/* Hero */}
      <section className="text-center px-6 py-20 max-w-4xl mx-auto">
        <div className="inline-flex items-center gap-2 bg-blue-50 text-blue-700 text-sm font-medium px-4 py-1.5 rounded-full mb-6">
          <span>Para docentes del MEP de Costa Rica</span>
        </div>
        <h1 className="text-5xl font-extrabold text-gray-900 leading-tight mb-5">
          Tu planeamiento didáctico<br />
          <span className="text-blue-600">listo en minutos</span>
        </h1>
        <p className="text-xl text-gray-500 mb-10 max-w-2xl mx-auto">
          AulaIA genera planeamientos completos alineados al programa oficial del MEP,
          toma lista con QR, registra notas y exporta informes listos para entregar.
        </p>
        <div className="flex flex-col sm:flex-row gap-3 justify-center">
          <button
            onClick={() => loginWithRedirect({ authorizationParams: { screen_hint: 'signup' } })}
            className="bg-blue-600 hover:bg-blue-700 text-white font-semibold px-8 py-3.5 rounded-xl text-base transition-colors shadow-sm"
          >
            Comenzar gratis
          </button>
          <button
            onClick={() => loginWithRedirect()}
            className="bg-white hover:bg-gray-50 text-gray-700 font-semibold px-8 py-3.5 rounded-xl text-base transition-colors border border-gray-200"
          >
            Ya tengo cuenta
          </button>
        </div>
      </section>

      {/* Features */}
      <section className="bg-gray-50 py-16 px-6">
        <div className="max-w-5xl mx-auto">
          <h2 className="text-2xl font-bold text-gray-900 text-center mb-10">Todo lo que necesitás en un solo lugar</h2>
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-5">
            {[
              { icon: '🤖', title: 'Planeamiento con IA', desc: 'Genera planeamientos por semana, mes o trimestre con actividades clase por clase, anclados al programa oficial del MEP.' },
              { icon: '📋', title: 'Asistencia con QR', desc: 'Tomá lista en segundos escaneando los QR de tus estudiantes desde el celular. Funciona sin internet.' },
              { icon: '📊', title: 'Registro de notas', desc: 'Calcula promedios automáticamente con la ponderación del MEP. Libro de notas por grupo y período.' },
              { icon: '📝', title: 'Adecuaciones curriculares', desc: 'Genera planes de adecuación individuales con IA, con contexto del programa y del estudiante.' },
              { icon: '📄', title: 'Exportar informes MEP', desc: 'Descargá reportes de asistencia, notas e informes en los formatos requeridos por el MEP.' },
              { icon: '📅', title: 'Calendario escolar', desc: 'Reagrupa lecciones automáticamente al agregar feriados, exámenes o actos cívicos.' },
            ].map(f => (
              <div key={f.title} className="bg-white rounded-2xl p-6 border border-gray-100 shadow-sm">
                <div className="text-3xl mb-3" aria-hidden="true">{f.icon}</div>
                <h3 className="font-semibold text-gray-900 mb-1">{f.title}</h3>
                <p className="text-sm text-gray-500 leading-relaxed">{f.desc}</p>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Pricing teaser */}
      <section className="py-16 px-6 text-center">
        <div className="max-w-2xl mx-auto">
          <h2 className="text-2xl font-bold text-gray-900 mb-3">Sin compromisos</h2>
          <p className="text-gray-500 mb-8">Empezá con una prueba gratuita. Planes desde <strong className="text-gray-900">$6/mes</strong> — menos que una lección particular.</p>
          <button
            onClick={() => loginWithRedirect({ authorizationParams: { screen_hint: 'signup' } })}
            className="bg-blue-600 hover:bg-blue-700 text-white font-semibold px-10 py-3.5 rounded-xl text-base transition-colors shadow-sm"
          >
            Comenzar gratis
          </button>
        </div>
      </section>

      {/* Footer */}
      <footer className="border-t border-gray-100 py-6 px-6 text-center text-sm text-gray-400">
        © 2026 AulaIA · Para docentes del MEP de Costa Rica
      </footer>
    </div>
  );
}
