import { expect, test } from '@playwright/test';

const groupId = '11111111-1111-1111-1111-111111111111';
const planId = '22222222-2222-2222-2222-222222222222';

test('crea un planeamiento desde el formulario web', async ({ page }) => {
  let planeamientoPayload: unknown = null;

  await page.route('**/api/**', async (route) => {
    const request = route.request();
    const url = new URL(request.url());

    if (request.method() === 'GET' && url.pathname === '/api/grupos') {
      await route.fulfill({
        contentType: 'application/json',
        body: JSON.stringify([
          {
            id: groupId,
            name: '7-3',
            level: '7',
            subject: 'Artes Plásticas',
            schoolYear: 2026,
            teacherId: 'teacher-e2e',
            pctCotidiano: 20,
            pctPruebas: 45,
            pctExtraclase: 20,
            pctOtros: 15,
          },
        ]),
      });
      return;
    }

    if (request.method() === 'GET' && url.pathname === '/api/planeamiento/curriculum-check') {
      expect(url.searchParams.get('asignatura')).toBe('Artes Plásticas');
      expect(url.searchParams.get('nivel')).toBe('7');
      expect(url.searchParams.get('trimestre')).toBe('1');
      await route.fulfill({
        contentType: 'application/json',
        body: JSON.stringify({ disponible: true, unidades: 3 }),
      });
      return;
    }

    if (request.method() === 'POST' && url.pathname === '/api/planeamiento') {
      planeamientoPayload = request.postDataJSON();
      await route.fulfill({
        status: 202,
        contentType: 'application/json',
        body: JSON.stringify({
          id: planId,
          status: 'Pending',
          contenido: null,
        }),
      });
      return;
    }

    await route.abort();
  });

  await page.goto('/planeamiento/nuevo/');

  await expect(page.getByRole('heading', { name: 'Nuevo planeamiento' })).toBeVisible();
  await expect(page.getByLabel('Grupo / Sección')).toHaveValue(groupId);
  await expect(page.getByText('Programa MEP validado')).toBeVisible();

  await page.getByLabel('Fecha de inicio').fill('2026-02-09');
  await page.getByLabel('Fecha de fin').fill('2026-04-24');
  await page.getByLabel('Lecciones por semana').fill('2');

  await page.getByRole('button', { name: 'Generar planeamiento con IA' }).click();

  await expect(page).toHaveURL(`/planeamiento/detalle/?id=${planId}`);
  expect(planeamientoPayload).toEqual({
    groupId,
    asignatura: 'Artes Plásticas',
    nivel: 7,
    trimestre: 1,
    anioLectivo: 2026,
    fechaInicio: '2026-02-09',
    fechaFin: '2026-04-24',
    leccionesPorSemana: 2,
  });
});
