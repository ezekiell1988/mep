# 01 — Requisitos del Sistema

> **Última actualización:** 2026-05-05
> **Fuentes:** `ia/assets/requerimiento.txt`, `ia/assets/investigacion_dominio.md`, `ia/assets/vision_producto_integral.md`, `ia/assets/20260505-Whatsapp.txt`, `ia/assets/sea-manual-docente.pdf`

---

## Propósito del sistema

Asistente pedagógico digital para docentes del MEP de Costa Rica que automatiza el planeamiento didáctico con IA, la toma de asistencia con QR, el registro de notas y trabajo cotidiano, las adecuaciones curriculares y la generación de reportes exportables en los formatos requeridos por el MEP.

---

## Reglas fundamentales de negocio

> Estas reglas son invariantes. **Nunca deben romperse sin un ADR en `06_decisions.md`.**

1. **La IA no inventa contenido curricular.** Todo aprendizaje esperado, indicador de evaluación y contenido generado debe estar anclado al programa oficial del MEP. No se generan objetivos ni indicadores que no existan en el programa.
2. **El docente es el único dueño de sus datos.** Un docente no puede ver ni modificar datos de grupos, notas o asistencia de otro docente, salvo que tenga rol de director o admin.
3. **Offline es obligatorio para operaciones de aula.** Tomar asistencia, registrar notas y consultar grupos/planeamientos guardados deben funcionar sin internet.
4. **Las notas de menores de edad son datos sensibles.** No se exponen en URLs públicas, no se comparten sin autenticación, no se incluyen en logs.
5. **La escala de calificación del MEP es 1–100.** Aprobatorio: 65 (primaria y III Ciclo), 70 (diversificado). No se puede cambiar por defecto; es configurable solo a nivel institucional si el director lo autoriza.
6. **El sistema nunca sube datos automáticamente al SEA** (Sistema de Evaluación Ágil — `sea.mep.go.cr`). La integración con plataformas del MEP es solo por exportación de archivos. El docente decide cuándo y qué exportar.
7. **Las adecuaciones curriculares son por estudiante, no por grupo.** Un plan de adecuación es individual y no debe aplicarse masivamente sin revisión del docente.
8. **La ponderación de evaluación es configurable por institución**, pero el sistema provee la ponderación por defecto del MEP (Trabajo cotidiano 20% / Pruebas 45% / Trabajo extraclase 20% / Otros 15%).

---

## Roles del sistema

| Rol | Descripción |
|-----|-------------|
| `docente` | Usuario principal. Gestiona sus propios grupos, planeamientos, asistencia y notas. |
| `director` | Ve reportes de todos los docentes de su institución. No edita datos de docentes. |
| `admin` | Administrador de plataforma. Gestiona instituciones, usuarios y configuración global. |

---

## Módulos y flujos principales

---

### Módulo 1: Gestión de Grupos y Estudiantes

**Es el núcleo del sistema. Todos los demás módulos dependen de este.**

#### RF-01 — Crear y gestionar institución
- El docente registra su institución con: nombre, circuito educativo, dirección regional del MEP.
- Un `admin` puede crear instituciones; un `docente` puede asociarse a una institución existente.

#### RF-02 — Crear grupos/secciones
- El docente crea grupos con: asignatura, nivel (ej. 7°), sección (ej. 7-3), año lectivo, cantidad de lecciones semanales.
- Un docente puede tener múltiples grupos activos simultáneamente.

#### RF-03 — Gestionar lista de estudiantes
- El docente agrega estudiantes a un grupo: nombre completo, número de cédula/expediente.
- Importación masiva desde archivo CSV.
- Digitación manual uno a uno.

#### RF-04 — Generar código QR por estudiante
- Al agregar un estudiante, el sistema genera automáticamente un UUID único que se convierte en código QR.
- El QR es descargable/imprimible en PDF (individual o en hoja con toda la sección).
- El QR no expone datos personales; solo un identificador interno.

#### RF-05 — Perfil del estudiante
- Datos básicos: nombre, expediente, sección.
- Historial: asistencia por fecha, notas por período, trabajos entregados, observaciones del docente.
- Adecuación curricular activa (si aplica): tipo, diagnóstico, observaciones.

---

### Módulo 2: Planeamiento Didáctico con IA

#### RF-06 — Ingresar parámetros del planeamiento
El docente provee:
- Nombre del docente, institución, año lectivo
- Asignatura, nivel/grado, sección
- Período: semanal / mensual / trimestral / semestral / anual
- Cantidad de lecciones por semana
- Fechas del calendario (inicio y fin del período)
- Plantilla institucional (opcional — upload de archivo DOCX/PDF)

#### RF-07 — Generación automática del planeamiento
El sistema debe generar:
- Datos generales completos
- Aprendizajes esperados (tomados del programa oficial del MEP)
- Indicadores de evaluación
- Contenidos en tres dimensiones: conceptual, procedimental, actitudinal
- Valores, actitudes y comportamientos éticos, estéticos y ciudadanos (ejes transversales)
- Estrategias de mediación y aprendizaje (secuenciadas: activación → desarrollo → cierre)
- **Actividades clase por clase** — secuencia detallada de cada lección dentro del período
- Cronograma de lecciones (distribución por semana y por clase)
- Recursos y materiales necesarios
- Estrategias e instrumentos de evaluación
- Evidencias de aprendizaje
- Rúbricas o listas de cotejo listos para usar
- **Tareas o prácticas sugeridas** — listas para asignar directamente
- **Ejemplos listos para aplicar en el aula** — concretos y contextualizados
- Anexos imprimibles

#### RF-08 — Anclar al programa oficial del MEP
- El sistema tiene una base de conocimiento interna con los programas de estudio oficiales del MEP por asignatura y nivel.
- El planeamiento generado usa la terminología exacta del MEP.
- El docente puede ver qué sección del programa corresponde a cada aprendizaje generado.
- **Programa inicial cubierto:** Artes Plásticas — III Ciclo (7°, 8°, 9°).
- **Meta a largo plazo:** base de conocimiento completa para todas las asignaturas y todos los niveles del MEP (I Ciclo, II Ciclo, III Ciclo, Diversificado). Cada nueva materia se incorpora en iteraciones posteriores.

#### RF-09 — Plantilla institucional
- Si el docente sube su plantilla, el planeamiento se genera respetando el formato de esa plantilla.
- Si no sube plantilla, se usa la plantilla base del MEP.

#### RF-10 — Exportar planeamiento
- El planeamiento se exporta en: PDF, DOCX.
- El docente puede editar el planeamiento antes de exportar (editor en línea básico).
- Los planeamientos exportados se almacenan en Azure Blob Storage y están disponibles offline una vez descargados.

#### RF-11 — Caché de planeamientos
- Un planeamiento con los mismos parámetros (asignatura + nivel + período + trimestre) no debe generar una nueva llamada al LLM si ya existe uno generado previamente.
- El docente puede regenerar explícitamente si lo desea.

---

### Módulo 3: Asistencia con QR

#### RF-12 — Tomar asistencia por QR
- El docente selecciona sección y fecha.
- Activa el modo "tomar lista con QR".
- Cada estudiante pasa su código QR frente a la cámara del dispositivo.
- El sistema registra: presente, hora de entrada.
- Los estudiantes que no pasen QR quedan marcados como ausentes.
- Funciona completamente **offline** (se sincroniza al recuperar conexión).

#### RF-13 — Tomar asistencia manual
- El docente puede marcar asistencia manualmente por tap en la lista de estudiantes.
- Estados disponibles: Presente / Ausente / Tardío / Salida temprana / Ausencia justificada.

#### RF-14 — Modo híbrido
- QR para los que llegaron a tiempo; marcado manual para tardíos o quienes olvidaron el QR.

#### RF-15 — Justificaciones y observaciones
- El docente puede agregar una nota de justificación a una ausencia.
- Las justificaciones son texto libre.

#### RF-16 — Historial de asistencia
- Vista por sección: listado de fechas con conteo de presentes/ausentes.
- Vista por estudiante: historial cronológico de asistencias.
- Filtros: por período, por sección, por estudiante.

---

### Módulo 4: Trabajo Cotidiano y Tareas

#### RF-17 — Crear actividad de evaluación
El docente crea actividades con:
- Nombre, tipo (trabajo cotidiano / tarea / proyecto / prueba / autoevaluación / coevaluación / portafolio)
- Fecha, valor en puntos o porcentaje
- Descripción (opcional)

#### RF-18 — Registrar calificaciones por actividad
- El docente ingresa la nota de cada estudiante (escala 1–100 o cualitativa según el tipo).
- Vista rápida: quién no ha entregado, quién está en rojo.
- Edición en línea directamente en la tabla.

#### RF-19 — Alertas de rendimiento
- Alerta automática cuando un estudiante tiene promedio por debajo del mínimo aprobatorio.
- Vista de "estudiantes en riesgo" por sección.

---

### Módulo 5: Gestión de Notas y Promedios

#### RF-20 — Configurar ponderación por sección
- El docente configura el porcentaje por tipo de evaluación para cada sección/período.
- El sistema provee la ponderación por defecto del MEP.

#### RF-21 — Cálculo automático de promedios
- El sistema calcula el promedio acumulado por período de forma automática.
- El cálculo se actualiza en tiempo real al ingresar o editar notas.

#### RF-22 — Libro de notas
- Vista tipo hoja de cálculo: filas = estudiantes, columnas = actividades.
- Visualización de promedio final por estudiante.
- Exportable a Excel (.xlsx) y CSV.

---

### Módulo 6: Generador de Reportes e Informes

#### RF-23 — Acta de notas
- Genera el acta de notas por sección y período, equivalente al acta que produce el SEA y que el docente debe entregar firmada a la dirección del centro.
- El acta incluye: período, institución, asignatura, grupo, lista de estudiantes con calificaciones finales.
- Exportable en PDF (para imprimir y firmar).

#### RF-23b — Informe descriptivo de logro
- El SEA genera y envía por correo a cada estudiante un "Informe descriptivo de logro unificado".
- AulaIA genera el equivalente: informe individual por estudiante con su desempeño en el período (notas, asistencia, observaciones del docente).
- El docente puede revisarlo y enviarlo desde la app antes de subirlo al SEA.
- Exportable en PDF.

#### RF-24 — Reporte de asistencia
- Genera reporte de ausencias por período: total de días, ausencias, porcentaje de asistencia.
- Por sección y por estudiante.
- Exportable en PDF y Excel.

#### RF-25 — Informe de adecuaciones
- Genera informe individual por estudiante con adecuación activa.
- Incluye: tipo, ajustes aplicados, notas del período con criterios adaptados.
- Exportable en PDF (para expediente del CAE).

#### RF-26 — Exportación para el SEA
- El SEA (Sistema de Evaluación Ágil — `sea.mep.go.cr`) permite al docente **descargar un archivo** con la plantilla de calificaciones por período + institución + asignatura + grupo, completarlo offline y **subirlo** de vuelta al sistema.
- AulaIA genera ese mismo archivo ya pre-completado con las notas registradas en la app, eliminando el trabajo de transcripción manual.
- El docente descarga el archivo desde AulaIA y lo sube manualmente al SEA. El sistema **nunca interactúa directamente con el SEA**.
- **Flujo SEA confirmado (manual oficial, abril 2023):** período → nombramiento/institución → asignatura → grupo → ingreso de calificaciones por rubros.
- **Los rubros y sus porcentajes los configura la dirección del centro** en el SEA-Institucional. AulaIA debe respetar esa configuración al generar el archivo.
- El SEA maneja dos tipos de evaluación: **sumativa** (nota numérica 1–100) y **formativa/cualitativa** (observaciones por rubro). AulaIA debe soportar ambos.
- El SEA también tiene un módulo de **Ampliación** (pruebas de recuperación). RF pendiente de detalle para Fase 3.
- **Formato exacto del archivo:** confirmar con acceso real al SEA. El manual indica descarga directa desde la pantalla de calificaciones — presumiblemente Excel (.xlsx).

---

### Módulo 7: Calendario Escolar

> **Prioridad elevada** (retroalimentación Adriana 2026-05-05): el calendario es crítico porque alimenta y reorganiza automáticamente el planeamiento.

#### RF-27 — Cargar y personalizar el calendario escolar
- El sistema incluye el calendario escolar oficial del MEP (200 días lectivos, tres trimestres, año lectivo febrero–diciembre).
- El docente puede personalizar el calendario según su institución. Operaciones soportadas:
  - **Eliminar feriados** o días no hábiles locales.
  - **Agregar actos cívicos** o eventos especiales.
  - **Marcar semanas de exámenes** (se excluyen como lecciones disponibles).
  - **Marcar días de consejo de profesores.**
  - **Marcar FEA** (Festivales de Expresión Artistica).
  - **Marcar semana del deporte.**
  - **Marcar congresos, capacitaciones y actividades institucionales.**
  - **Ajustar la cantidad de lecciones por semana** para un período dado (ej. reducción por actividades).
- Cada modificación al calendario **reorganiza automáticamente** el cronograma de todas las lecciones del planeamiento asociado.

#### RF-28 — Cálculo automático de lecciones disponibles
- Dado un período (inicio–fin) y la cantidad de lecciones semanales configurada, el sistema calcula automáticamente cuántas lecciones hay disponibles, descontando todos los eventos marcados como no lectivos.
- Este cálculo alimenta directamente el cronograma del planeamiento didáctico.

#### RF-29 — Vista de calendario
- El docente puede ver el calendario del período con los días lectivos, no lectivos y eventos resaltados por tipo.
- Filtrable por grupo (para ver cuántas lecciones tiene cada sección en el período).

---

### Módulo 8: Adecuaciones Curriculares

#### RF-30 — Registrar adecuación de un estudiante
El docente registra:
- Tipo: significativa (AS) o no significativa (ANS)
- Diagnóstico o características educativas
- Nivel de funcionamiento académico actual
- Dificultades observadas en el aula
- Fortalezas e intereses del estudiante
- Recomendaciones del CAE o especialistas externos

#### RF-31 — Generar propuesta pedagógica de adecuación
El sistema genera:
- Recomendaciones pedagógicas para el aula
- Ajustes metodológicos específicos
- Ajustes en la evaluación (criterios adaptados)
- Material adaptado sugerido
- Para AS: actividades con menor complejidad, objetivos ajustados, plan individual
- Estrategias concretas de apoyo en el aula

#### RF-32 — Integrar adecuación en el planeamiento
- El docente puede solicitar que el planeamiento de una sección incluya adaptaciones para uno o más estudiantes con adecuación registrada.
- El planeamiento muestra las adaptaciones diferenciadas por actividad.

---

## Requisitos no funcionales

| # | Requisito | Criterio de aceptación |
|---|-----------|------------------------|
| RNF-01 | **Offline obligatorio** | Asistencia, notas y consulta de grupos funcionan sin internet. Sincronización automática al recuperar conexión. |
| RNF-02 | **Tiempo de respuesta del LLM** | La generación de un planeamiento mensual completa en menos de 60 segundos con buena conexión. |
| RNF-03 | **Seguridad de datos de menores** | Las notas y datos de estudiantes viajan cifrados (HTTPS). No se exponen en URLs. Acceso solo con JWT válido. |
| RNF-04 | **Multiplataforma** | La app funciona en iOS, Android y navegador web. Sin funcionalidades exclusivas de una plataforma. |
| RNF-05 | **Exportación de archivos** | PDF y DOCX generados correctamente en todos los navegadores y dispositivos sin pérdida de formato. |
| RNF-06 | **Roles y permisos** | Un `docente` solo accede a sus propios datos. Un `director` solo ve datos de su institución. |
| RNF-07 | **Disponibilidad** | El backend tiene uptime ≥ 99.5% en horario escolar (lunes–viernes, 6am–8pm hora CR). |
| RNF-08 | **Privacidad** | No se comparte ningún dato de estudiantes con terceros. Sin analytics de comportamiento sobre datos de menores. |
