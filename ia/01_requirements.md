# 01 — Requisitos del Sistema

> **Última actualización:** 2026-05-07
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
9. **El marco legal de evaluación es el REAC 2026** (Reglamento de Evaluación de los Aprendizajes del MEP). La evaluación tiene tres tipos: diagnóstica (inicio del período), formativa (proceso continuo) y sumativa (nota final). El sistema debe soportar los tres.
10. **El marco legal de adecuaciones es la Ley 7600** (Igualdad de Oportunidades para Personas con Discapacidad, 1996). Las adecuaciones significativas (AS) requieren registro formal en el SIMED del MEP; AulaIA genera el documento de soporte pero no registra en SIMED directamente.
11. **En Artes Plásticas se evalúa el proceso, no solo el producto final.** El disfrute y la participación son criterios de evaluación legítimos según el programa oficial del MEP. La IA debe generar rúbricas e instrumentos que reflejen esto.

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

#### Marco institucional de adecuaciones (contexto obligatorio)

- **CAE** (Comité de Apoyo Educativo): equipo interdisciplinario institucional (orientador, docente de apoyo, dirección) que gestiona el proceso de adecuaciones. AulaIA genera documentos para el expediente del CAE.
- **CENAREC** (Centro Nacional de Recursos para la Educación Inclusiva): institución del MEP de referencia para educación inclusiva.
- **SIMED**: las adecuaciones significativas deben registrarse en el SIMED del MEP. AulaIA genera el plan individual (PI) como documento exportable; el docente lo registra manualmente en SIMED.
- **Proceso completo (5 etapas):** identificación → evaluación diagnóstica por CAE → elaboración del plan → registro en SIMED (solo AS) → implementación → **seguimiento periódico**. AulaIA soporta la elaboración del plan y el seguimiento; las etapas 1–2 son previas a la app.

| Tipo | Qué modifica | Qué no modifica | Registro SIMED |
|------|-------------|-----------------|----------------|
| **ANS** (no significativa) | Metodología, tiempos, materiales, ambiente | Contenidos, objetivos, evaluación sumativa | No requerido |
| **AS** (significativa) | Contenidos, objetivos, criterios de evaluación, nivel de exigencia | Pertenencia al grupo, socialización | **Sí requerido** |

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
- Para ANS: estrategias de metodología diferenciada (tiempo extra, apoyos visuales, instrucciones claras, posición estratégica en el aula)
- Para AS: actividades con menor complejidad, objetivos ajustados, plan individual (PI) exportable
- Estrategias concretas de apoyo en el aula

#### RF-32 — Integrar adecuación en el planeamiento
- El docente puede solicitar que el planeamiento de una sección incluya adaptaciones para uno o más estudiantes con adecuación registrada.
- El planeamiento muestra las adaptaciones diferenciadas por actividad.

#### RF-33 — Seguimiento de adecuación
- El docente puede registrar observaciones de seguimiento periódico por estudiante con adecuación activa.
- El sistema genera un informe de seguimiento exportable en PDF para el expediente del CAE.

---

### Módulo 9: Suscripciones y Pagos (SINPE Móvil + Verificación Manual)

> **Pasarela de pago:** SINPE Móvil — sistema interoperble del Banco Central de Costa Rica (BCCR). No tiene API pública; toda verificación es manual. El pagador transfiere desde su app bancaria al número SINPE de AulaIA; el admin confirma el pago y activa la suscripción.
>
> **Consecuencia de diseño:** no hay renovación automática. El usuario debe pagar cada mes y el admin verificar. Esto es viable para la fase inicial (< 200 usuarios). Si el volumen crece, evaluar integración con un agregador de pagos CR que soporte SINPE (ej. CincoDigitos, Kushki).

#### Planes disponibles

| Plan | Precio/mes | Precio/mes (CRC referencial ~₡530/$) | Límite grupos | Funciones incluidas |
|------|-----------|--------------------------------------|---------------|---------------------|
| **Básico** | $6 USD | ₡3,200 | 5 grupos | Planeamiento + asistencia + notas básicas |
| **Profesional** | $15 USD | ₡8,000 | Sin límite | Todo + adecuaciones + reportes exportables |
| **Institucional** | $100 USD/mes | ₡53,000 | Sin límite (todos los docentes) | Todo Profesional + panel de director + reportes institucionales |

> Los precios en CRC se calculan usando el **tipo de cambio de venta USD del BCCR del día**, actualizado automáticamente cada día hábil mediante `UpdateExchangeRateJob` (Hangfire). El precio oficial cotizado al usuario es en USD; el equivalente en CRC es informativo para saber cuánto transferir por SINPE.

#### RF-34 — Trial gratuito
- Todo usuario nuevo recibe 30 días de acceso completo (plan Profesional) sin pago.
- Al vencer el trial, el acceso se degrada automáticamente al plan Básico (con límite de grupos) hasta que el admin confirme el primer pago.
- El sistema avisa al usuario en la app: 7 días antes, 3 días antes y el día del vencimiento del trial.

#### RF-35 — Paywalls por plan
- Las funciones exclusivas de planes superiores muestran un banner/modal con descripción del plan requerido y las instrucciones de pago SINPE. No errores genéricos.
- El límite de 5 grupos en el plan Básico se verifica en el backend, no solo en el frontend.
- Un docente con plan institucional no paga suscripción individual; el acceso lo activa el admin al confirmar el pago del colegio.

#### RF-36 — Flujo de pago SINPE Móvil
El flujo completo es:
1. El usuario selecciona un plan en la app (o es llevado al paywall).
2. La app muestra la pantalla de pago con:
   - Número SINPE de AulaIA (configurado en `AppSettings`, no hardcodeado).
   - Monto exacto en **CRC** (SINPE Móvil solo opera en colones costarricenses). Se muestra el equivalente en USD del plan como referencia. El monto CRC se calcula con el tipo de cambio de venta BCCR del día.
   - Un **código de referencia único** generado por el sistema (ej. `APL-20260507-8F3K`) que el usuario debe incluir en el mensaje/descripción de la transferencia SINPE.
   - Instrucciones paso a paso: "Abre tu app del banco → SINPE Móvil → ingresa el número → pon el código de referencia en la descripción → confirma".
3. El usuario transfiere desde su app bancaria (fuera de AulaIA — no hay integración técnica).
4. El usuario regresa a AulaIA y presiona **"Ya pagué"**, adjuntando opcionalmente una captura de pantalla del comprobante.
5. La solicitud queda en estado `pending_verification` en el sistema.
6. El admin recibe una notificación (email + badge en el panel admin) con los datos de la solicitud.
7. El admin verifica el pago en la app bancaria usando el código de referencia y el monto.
8. El admin aprueba → el sistema activa la suscripción por 30 días desde la fecha de aprobación.
9. El usuario recibe una notificación push/email confirmando la activación.

> **El sistema nunca procesa ni almacena datos bancarios.** Solo almacena: código de referencia, monto solicitado, captura opcional subida por el usuario (almacenada en Azure Blob Storage como evidencia).

#### RF-37 — Renovación mensual
- 7 días antes de que venza el período activo, el sistema notifica al usuario (push + email) con las instrucciones de pago SINPE para renovar.
- El día del vencimiento, si no hay pago confirmado, el acceso se degrada al plan Básico automáticamente.
- El historial de pagos previos del usuario no se borra al degradar; se reactiva completo al confirmar el siguiente pago.
- El admin tiene un panel de "suscripciones por vencer esta semana" para hacer seguimiento proactivo.

#### RF-38 — Panel de administración de pagos
El admin puede:
- Ver lista de solicitudes de pago pendientes de verificación, con: usuario, plan, monto, código de referencia, fecha de solicitud, captura adjunta.
- Aprobar o rechazar cada solicitud con una nota (ej. "código no coincide", "monto incorrecto").
- Ver historial completo de pagos aprobados y rechazados.
- Ver panel de suscripciones activas con fecha de vencimiento de cada una.
- Exportar reporte mensual de ingresos (PDF/XLSX) para contabilidad.
- Configurar: número SINPE de AulaIA (el TC se obtiene automáticamente del BCCR; no requiere configuración manual).
- Ver el tipo de cambio de venta BCCR vigente (leído de `exchange_rates`) y la fecha de la última actualización del job.

#### RF-39 — Entidades de BD necesarias
- `subscriptions`: `id`, `user_id`, `plan` (basic/professional/institutional), `status` (trialing/active/past_due/cancelled), `trial_ends_at`, `current_period_end`, `activated_by_admin_id`, `activated_at`.
- `payment_requests`: `id`, `user_id`, `plan`, `amount_usd`, `amount_crc`, `exchange_rate_used` (TC venta BCCR del día de la solicitud, para auditoría), `reference_code` (único), `status` (pending/approved/rejected), `screenshot_url`, `admin_note`, `created_at`, `reviewed_at`, `reviewed_by`.
- `exchange_rates`: `id`, `date` (único, clave natural), `sell_rate` (DECIMAL, tipo de cambio de venta BCCR USD→CRC), `fetched_at`. Poblada por `UpdateExchangeRateJob`. Si el job falla un día, se reutiliza el último valor disponible.
- El campo `plan` en `subscriptions` determina los permisos en todo el sistema.
- El `reference_code` se genera automáticamente al crear la solicitud: formato `AUI-{YYYYMMDD}-{4 chars aleatorios en mayúsculas}`.

---

### Módulo 10: Sistema de Referidos

> Implementa el modelo de comisiones definido en ADR-008. Adriana Guido es la primera y principal referidora. Las comisiones se pagan manualmente por SINPE Móvil al cierre de cada mes.

#### RF-40 — Códigos de referido
- Todo usuario con rol `docente` puede solicitar su código de referido personal (link único: `mep.ezekl.com/registro?ref=CODIGO`).
- Adriana recibe su código al activar su cuenta. El código es permanente y no cambia.
- El código es alfanumérico de 8 caracteres, generado aleatoriamente, único en el sistema.

#### RF-41 — Tracking de origen al registrarse
- Durante el registro, si el usuario llega desde un link con `?ref=CODIGO`, ese código queda asociado a su cuenta permanentemente.
- El usuario también puede ingresar un código de referido manualmente en el formulario de registro.
- Una vez guardado el referidor, no se puede cambiar.
- Entidades: `referral_codes` (`id`, `user_id`, `code`, `created_at`) y campo `referred_by_code` en `users`.

#### RF-42 — Cálculo de comisiones
- La comisión se genera cuando un usuario referido tiene un pago **aprobado** por el admin (no durante el trial).
- Tasa: **20% del monto neto** (ingresos brutos del mes − costos de infraestructura Azure del mes) proporcional a la suscripción del usuario referido, durante **12 meses** desde su primer pago aprobado.
- El cálculo de comisión se ejecuta en un job mensual (`CalculateCommissionsJob`, Hangfire) al inicio de cada mes para el mes anterior.
- El admin ingresa manualmente el costo de infraestructura Azure del mes (obtenido del portal Azure) antes de ejecutar el cierre.
- La comisión se registra en la tabla `commissions`: `id`, `referrer_user_id`, `referred_user_id`, `month`, `subscription_amount_usd`, `infra_deduction_usd`, `commission_amount_usd`, `status` (pending/paid), `paid_at`, `sinpe_confirmation`.

#### RF-43 — Panel de referidos y comisiones
- El referidor (Adriana) puede ver en su perfil:
  - Lista de usuarios que registró (nombre y fecha de registro, sin datos sensibles de terceros).
  - Por cada referido que es suscriptor activo: meses restantes de comisión.
  - Total de comisiones acumuladas en el mes actual y en meses anteriores.
  - Estado del pago de cada mes: pendiente / pagado.
  - Detalle por mes exportable en PDF/CSV.
- El admin ve el panel completo de todos los referidores con totales globales y botón "Marcar como pagado" por comisión (después de hacer el SINPE al referidor).
- El docente referido **no puede ver** que está generando una comisión para alguien — solo el referidor y el admin.

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
