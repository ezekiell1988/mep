import type { DriveStep } from 'driver.js';

export type TourKey =
  | 'dashboard'
  | 'grupos'
  | 'planeamiento'
  | 'planeamiento-nuevo'
  | 'asistencia'
  | 'notas'
  | 'calendario'
  | 'adecuaciones'
  | 'qrs'
  | 'suscripcion'
  | 'perfil';

const TOURS: Record<TourKey, DriveStep[]> = {
  dashboard: [
    {
      popover: {
        title: '👋 Bienvenido a AulaIA',
        description:
          'Este es tu <b>dashboard</b>. Aquí encontrarás un resumen de toda tu actividad como docente del MEP de Costa Rica.',
      },
    },
    {
      element: '#tour-stats',
      popover: {
        title: '📊 Estadísticas rápidas',
        description:
          'De un vistazo: cuántos grupos activos tienes, cuántos estudiantes, planeamientos listos y adecuaciones vigentes. El badge rojo indica estudiantes con promedio menor a 65 (en riesgo de reprobación).',
        side: 'bottom',
        align: 'center',
      },
    },
    {
      element: '#tour-eventos',
      popover: {
        title: '📅 Próximos eventos',
        description:
          'Aquí aparecen los eventos del calendario escolar en los próximos 14 días: feriados, exámenes, consejos de profesores, actos cívicos y más.',
        side: 'top',
      },
    },
    {
      element: '#tour-nav-grupos',
      popover: {
        title: '🏫 Mis Grupos',
        description:
          'Haz clic aquí para ver y gestionar todos tus grupos. Desde cada grupo accedes a asistencia, notas, calendario, adecuaciones y códigos QR.',
        side: 'bottom',
      },
    },
  ],

  grupos: [
    {
      popover: {
        title: '🏫 Mis Grupos',
        description:
          'Aquí aparecen todos tus grupos activos del año lectivo. Cada grupo agrupa a los estudiantes de una sección y te da acceso a todas las herramientas de AulaIA.',
      },
    },
    {
      element: '#tour-grupos-list',
      popover: {
        title: '📋 Lista de grupos',
        description:
          'Cada tarjeta muestra el nombre del grupo, la asignatura, el nivel y el año lectivo. Haz clic en los botones de acción para gestionar cada grupo.',
        side: 'top',
      },
    },
    {
      element: '#tour-grupos-acciones',
      popover: {
        title: '⚡ Acciones del grupo',
        description:
          '<b>📅 Asistencia</b> — Historial de asistencia del grupo<br/>' +
          '<b>🗓️ Calendario</b> — Gestiona el calendario escolar del grupo<br/>' +
          '<b>📊 Notas</b> — Libro de calificaciones y promedios<br/>' +
          '<b>♿ Adecuaciones</b> — Planes de adecuación curricular por estudiante<br/>' +
          '<b>🖨 QRs</b> — Genera e imprime los códigos QR de cada estudiante',
        side: 'left',
      },
    },
    {
      element: '#tour-grupos-planeamiento',
      popover: {
        title: '📋 Planeamientos con IA',
        description:
          'Ve a Planeamientos para generar con inteligencia artificial el planeamiento didáctico completo de cualquiera de tus grupos, alineado al programa oficial del MEP.',
        side: 'bottom',
      },
    },
  ],

  planeamiento: [
    {
      popover: {
        title: '📋 Planeamientos Didácticos',
        description:
          'Genera y consulta tus planeamientos didácticos con IA, alineados al programa oficial del MEP de Costa Rica. Incluye aprendizajes esperados, indicadores de evaluación, estrategias de mediación y actividades clase por clase.',
      },
    },
    {
      element: '#tour-plan-nuevo',
      popover: {
        title: '✨ Nuevo planeamiento',
        description:
          'Haz clic aquí para iniciar la generación de un planeamiento completo con IA. Solo necesitas elegir la asignatura, nivel y trimestre. La IA hace el resto en 1–2 minutos.',
        side: 'bottom',
        align: 'end',
      },
    },
    {
      element: '#tour-plan-list',
      popover: {
        title: '📄 Tus planeamientos',
        description:
          'Aquí aparecen todos tus planeamientos generados. Los badges de color indican el estado: <b>Pendiente</b> (en cola), <b>Generando…</b> (IA trabajando), <b>Listo</b> (disponible) o <b>Error</b> (falló).',
        side: 'top',
      },
    },
  ],

  'planeamiento-nuevo': [
    {
      popover: {
        title: '✨ Nuevo Planeamiento con IA',
        description:
          'Completa este formulario para que la IA genere un planeamiento didáctico completo y alineado al programa oficial del MEP. Tarda 1–2 minutos.',
      },
    },
    {
      element: '#tour-pnuevo-asignatura',
      popover: {
        title: '📚 Asignatura',
        description:
          'Selecciona la asignatura. La IA usará el programa oficial del MEP correspondiente para generar los aprendizajes esperados, indicadores de evaluación y contenidos.',
        side: 'bottom',
      },
    },
    {
      element: '#tour-pnuevo-nivel',
      popover: {
        title: '🎓 Nivel y Trimestre',
        description:
          'El nivel determina qué unidades del programa corresponden. El trimestre indica el período del año lectivo (I: feb–abr, II: may–jul, III: ago–nov).',
        side: 'bottom',
      },
    },
    {
      element: '#tour-pnuevo-lecciones',
      popover: {
        title: '⏱️ Lecciones y fechas',
        description:
          'Indica las lecciones por semana y el rango de fechas. La IA calculará las lecciones reales disponibles y distribuirá las actividades clase por clase dentro del período.',
        side: 'bottom',
      },
    },
    {
      element: '#tour-pnuevo-submit',
      popover: {
        title: '🚀 Generar con IA',
        description:
          'Al hacer clic, la IA comienza a generar el planeamiento completo. Serás redirigido automáticamente al resultado cuando esté listo.',
        side: 'top',
      },
    },
  ],

  asistencia: [
    {
      popover: {
        title: '📅 Historial de Asistencia',
        description:
          'Consulta y exporta el historial de asistencia de tu grupo para cualquier período. Los datos se actualizan en tiempo real desde la app móvil.',
      },
    },
    {
      element: '#tour-asist-filtros',
      popover: {
        title: '🔍 Filtros de fecha',
        description:
          'Selecciona el rango de fechas que quieres consultar y haz clic en <b>Buscar</b>. Por defecto muestra el mes actual.',
        side: 'bottom',
      },
    },
    {
      element: '#tour-asist-leyenda',
      popover: {
        title: '🏷️ Leyenda de estados',
        description:
          '<b>P</b> = Presente &nbsp; <b>A</b> = Ausente &nbsp; <b>T</b> = Tardanza &nbsp; <b>J</b> = Justificada<br/>Los guiones (—) indican que no se registró asistencia ese día.',
        side: 'bottom',
      },
    },
    {
      element: '#tour-asist-tabla',
      popover: {
        title: '📊 Tabla de asistencia',
        description:
          'Cada fila es un estudiante. Las columnas muestran el estado por fecha. Al final de cada fila aparece el resumen total de P/A/T/J del período consultado.',
        side: 'top',
      },
    },
    {
      element: '#tour-asist-descargar',
      popover: {
        title: '📥 Exportar',
        description:
          'Descarga el historial en <b>Excel (XLSX)</b> o <b>PDF</b> para presentarlo en reuniones de padres, consejos de profesores o como evidencia institucional.',
        side: 'left',
      },
    },
  ],

  notas: [
    {
      popover: {
        title: '📊 Libro de Calificaciones',
        description:
          'Registra y gestiona todas las calificaciones de tu grupo. El sistema calcula automáticamente los promedios según la ponderación del MEP (o la ponderación personalizada de tu institución).',
      },
    },
    {
      element: '#tour-notas-ponderacion',
      popover: {
        title: '⚖️ Ponderación MEP',
        description:
          'La ponderación por defecto del MEP es: <b>Trabajo Cotidiano 20%</b> / <b>Pruebas 45%</b> / <b>Trabajo Extraclase 20%</b> / <b>Otros 15%</b>. Haz clic aquí para ajustarla si tu institución usa una diferente.',
        side: 'bottom',
      },
    },
    {
      element: '#tour-notas-nueva',
      popover: {
        title: '➕ Nueva actividad',
        description:
          'Agrega pruebas escritas, trabajos cotidianos, proyectos, portafolios y más. Cada actividad genera una columna de calificaciones en la tabla.',
        side: 'bottom',
      },
    },
    {
      element: '#tour-notas-actividades',
      popover: {
        title: '📝 Actividades evaluadas',
        description:
          'Cada chip muestra el nombre de la actividad y su peso porcentual. La suma debe ser 100%. Haz clic en <b>Editar</b> dentro de la tabla para ingresar o modificar notas.',
        side: 'bottom',
      },
    },
    {
      element: '#tour-notas-tabla',
      popover: {
        title: '👩‍🎓 Tabla de estudiantes',
        description:
          'Cada fila es un estudiante con su promedio actual. El badge <span style="color:#15803d">verde</span> indica aprobado y <span style="color:#b91c1c">rojo</span> indica en riesgo. El ícono ⚠ señala a los estudiantes bajo el umbral de aprobación.',
        side: 'top',
      },
    },
  ],

  calendario: [
    {
      popover: {
        title: '🗓️ Calendario Escolar',
        description:
          'Gestiona el calendario escolar de tu grupo: registra feriados, exámenes, actos cívicos y otros eventos no lectivos. El calendario alimenta el módulo de planeamiento.',
      },
    },
    {
      element: '#tour-cal-nav',
      popover: {
        title: '◀ ▶ Navegación por meses',
        description:
          'Usa los botones de flecha para moverte entre meses. El encabezado muestra el mes y año que estás viendo.',
        side: 'bottom',
      },
    },
    {
      element: '#calendar-grid',
      popover: {
        title: '📆 Calendario visual',
        description:
          'Los días con eventos aparecen marcados con chips de colores según el tipo: <span style="color:#b91c1c">rojo</span> feriado, <span style="color:#c2410c">naranja</span> examen, <span style="color:#1d4ed8">azul</span> consejo de profesores, <span style="color:#15803d">verde</span> deportivo, <span style="color:#7e22ce">morado</span> acto cívico.',
        side: 'top',
      },
    },
    {
      element: '#tour-cal-form',
      popover: {
        title: '➕ Agregar evento',
        description:
          'Registra un nuevo evento indicando la fecha, tipo y título. Puedes incluir una fecha fin para eventos de varios días (ej. semana de exámenes).',
        side: 'left',
      },
    },
    {
      element: '#tour-cal-lecciones',
      popover: {
        title: '⏱️ Lecciones disponibles',
        description:
          'Calcula cuántas lecciones reales tienes en un período, descontando automáticamente los días no lectivos registrados. Útil para saber cuántas clases tendrás antes de generar un planeamiento.',
        side: 'left',
      },
    },
  ],

  adecuaciones: [
    {
      popover: {
        title: '♿ Adecuaciones Curriculares',
        description:
          'Gestiona los planes de adecuación curricular de los estudiantes de tu grupo, según la <b>Ley 7600</b> y las directrices del MEP. Las adecuaciones son individuales y no se aplican masivamente.',
      },
    },
    {
      element: '#tour-adec-lista',
      popover: {
        title: '👥 Lista de estudiantes',
        description:
          'Aparecen todos los estudiantes del grupo. El tipo (ANS, AS, AA) y el estado (Borrador, Lista, etc.) se muestran cuando el estudiante tiene una adecuación activa.',
        side: 'top',
      },
    },
    {
      element: '#tour-adec-tipos',
      popover: {
        title: '📋 Tipos de adecuación',
        description:
          '<b>ANS</b> = Adecuación No Significativa — ajustes menores de acceso<br/>' +
          '<b>AS</b> = Adecuación Significativa — modificaciones al currículo (requiere registro en SIMED)<br/>' +
          '<b>AA</b> = Adecuación de Acceso — recursos físicos o tecnológicos<br/><br/>' +
          'Haz clic en <b>+ Agregar</b> o <b>Ver / Editar</b> para abrir el panel de detalle.',
        side: 'top',
      },
    },
  ],

  qrs: [
    {
      popover: {
        title: '🖨️ Códigos QR de Asistencia',
        description:
          'Aquí están los códigos QR de todos los estudiantes del grupo. Cada QR es único e identifica al estudiante para registrar asistencia con la app móvil.',
      },
    },
    {
      element: '#tour-qrs-grid',
      popover: {
        title: '📇 Grilla de QRs',
        description:
          'Cada tarjeta muestra el nombre del estudiante y su código QR único. Los QRs se generan automáticamente cuando agregas al estudiante al grupo.',
        side: 'top',
      },
    },
    {
      element: '#tour-qrs-print',
      popover: {
        title: '🖨️ Imprimir',
        description:
          'Imprime todos los QRs en formato A4. Puedes recortarlos y entregarlos a los estudiantes para que los peguen en su cuaderno o carnet. La app móvil los escanea para registrar asistencia.',
        side: 'bottom',
      },
    },
  ],

  suscripcion: [
    {
      popover: {
        title: '💳 Mi Suscripción',
        description:
          'Aquí gestionas tu suscripción a AulaIA. Puedes activar un trial gratuito de 30 días o suscribirte a un plan pagado vía SINPE Móvil.',
      },
    },
    {
      element: '#tour-sus-estado',
      popover: {
        title: '📋 Estado actual',
        description:
          'Muestra tu plan activo, el estado (Activo / Expirado) y la fecha de vencimiento. Durante el trial verás los días restantes.',
        side: 'bottom',
      },
    },
    {
      element: '#tour-sus-trial',
      popover: {
        title: '🎁 Trial gratuito',
        description:
          'Si aún no tienes suscripción, activa un <b>trial gratuito de 30 días</b> que incluye todas las funciones del plan Profesional sin necesidad de pago.',
        side: 'bottom',
      },
    },
    {
      element: '#plan-select',
      popover: {
        title: '📦 Planes disponibles',
        description:
          '<b>Básico ($6/mes)</b> — Planeamiento, asistencia y notas (máx. 5 grupos)<br/>' +
          '<b>Profesional ($15/mes)</b> — Todo + adecuaciones, reportes exportables, grupos ilimitados<br/>' +
          '<b>Institucional ($100/mes)</b> — Todos los docentes del colegio + panel de director',
        side: 'bottom',
      },
    },
  ],

  perfil: [
    {
      popover: {
        title: '👤 Mi Perfil',
        description:
          'Aquí encuentras tu información de cuenta, el estado de tu suscripción y tu código de referidos para invitar a otros docentes.',
      },
    },
    {
      element: '#tour-perfil-suscripcion',
      popover: {
        title: '💳 Suscripción activa',
        description:
          'Muestra tu plan actual y fecha de vencimiento. Haz clic para ir a la página de suscripción y renovar o cambiar de plan.',
        side: 'bottom',
      },
    },
    {
      element: '#tour-perfil-referido',
      popover: {
        title: '🔗 Código de referidos',
        description:
          'Comparte este enlace con otros docentes del MEP. Por cada docente que se suscriba usando tu código, recibirás beneficios especiales durante 12 meses.',
        side: 'bottom',
      },
    },
  ],
};

export function getTourSteps(key: TourKey): DriveStep[] {
  return TOURS[key] ?? [];
}
