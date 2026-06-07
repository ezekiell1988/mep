'use client';

import { useCallback } from 'react';
import type { TourKey } from '../lib/tours';

interface TourButtonProps {
  tourKey: TourKey;
  className?: string;
}

export function TourButton({ tourKey, className }: TourButtonProps) {
  const handleStart = useCallback(async () => {
    const [{ driver }, { getTourSteps }] = await Promise.all([
      import('driver.js'),
      import('../lib/tours'),
    ]);

    const steps = getTourSteps(tourKey);
    if (!steps.length) return;

    const d = driver({
      showProgress: true,
      progressText: '{{current}} de {{total}}',
      nextBtnText: 'Siguiente →',
      prevBtnText: '← Anterior',
      doneBtnText: '¡Listo!',
      allowClose: true,
      steps,
    });

    d.drive();
  }, [tourKey]);

  return (
    <button
      type="button"
      onClick={handleStart}
      className={
        className ??
        'text-sm border border-gray-200 text-gray-400 hover:text-gray-600 hover:bg-gray-50 font-medium px-3 py-2 rounded-lg transition-colors flex items-center gap-1'
      }
      aria-label="Iniciar guía interactiva de la página"
      title="Guía interactiva"
    >
      <span aria-hidden="true">❓</span>
      <span className="hidden sm:inline">Guía</span>
    </button>
  );
}
