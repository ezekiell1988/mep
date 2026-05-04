# Visión del Producto: Plataforma Educativa Integral para Docentes MEP

> **Tipo de documento:** Análisis de viabilidad y visión de producto  
> **Fecha:** Mayo 2026  
> **Propósito:** Evaluar la viabilidad técnica, comercial y pedagógica de evolucionar de un generador de planeamientos a una plataforma educativa integral de uso diario para docentes del MEP de Costa Rica  
> **Pregunta central:** ¿Tiene sentido y viabilidad construir un sistema todo-en-uno que acompañe al docente en su día a día?

---

## Mi opinión directa: sí es viable, y hay algo más importante

Antes de entrar en el análisis técnico, quiero decir esto claramente:

> **El problema que describes no tiene una solución local en Costa Rica.** Los docentes del MEP hoy usan Excel para las notas, WhatsApp para comunicarse con padres, Word para el planeamiento, libros físicos para la lista de asistencia, y Google Drive para "guardar todo". No existe ningún sistema unificado, construido para el docente costarricense, que resuelva todo eso. Eso es una oportunidad real.

El riesgo no es que no haya mercado. El riesgo es **construir demasiado al mismo tiempo** y no terminar nada bien. Ese es el único peligro serio.

---

## Tabla de Contenidos

1. [El problema real del docente costarricense hoy](#1-el-problema-real-del-docente-costarricense-hoy)
2. [Visión del Producto Integral](#2-visión-del-producto-integral)
3. [Módulos Propuestos (desglose funcional)](#3-módulos-propuestos-desglose-funcional)
4. [Módulo estrella: Asistencia con QR](#4-módulo-estrella-asistencia-con-qr)
5. [Integración con plataformas del MEP](#5-integración-con-plataformas-del-mep)
6. [Modelo de negocio: SaaS por suscripción](#6-modelo-de-negocio-saas-por-suscripción)
7. [Análisis de mercado y competencia en Costa Rica](#7-análisis-de-mercado-y-competencia-en-costa-rica)
8. [Estrategia de Go-to-Market y Ventas](#8-estrategia-de-go-to-market-y-ventas)
9. [Análisis de viabilidad técnica](#9-análisis-de-viabilidad-técnica)
10. [Riesgos reales del proyecto](#10-riesgos-reales-del-proyecto)
11. [Estrategia recomendada (fases de desarrollo)](#11-estrategia-recomendada-fases-de-desarrollo)
12. [Veredicto final](#12-veredicto-final)

---

## 1. El Problema Real del Docente Costarricense Hoy

### Cómo trabaja un docente de secundaria del MEP actualmente

Un docente promedio de secundaria en Costa Rica maneja entre **8 y 15 secciones** con **30–40 estudiantes cada una**. Eso significa potencialmente **400–600 estudiantes** a los que debe hacerles seguimiento simultáneo.

Su herramienta de trabajo actual:

| Tarea | Herramienta actual | Problema |
|---|---|---|
| Tomar asistencia | Libro físico o cuaderno de papel | Se pierde, se moja, no hay respaldo digital |
| Registrar notas de trabajos cotidianos | Excel / hoja de papel | No está conectado con nada |
| Gestionar tareas | WhatsApp o verbal | No hay trazabilidad |
| Registrar notas de exámenes | Excel o papel | Calcular promedios es manual y propenso a error |
| Elaborar planeamiento | Word + copia del año anterior | Horas de trabajo para cumplir con la entrega |
| Subir notas al sistema del MEP | SIMAR / SIRC | Transcripción manual de Excel al sistema; doble trabajo |
| Comunicarse con padres | WhatsApp personal | Expone el número personal del docente |
| Registrar adecuaciones curriculares | Documentos aparte | No integrado con nada |
| Generar informes de período | Manualmente | Tarda horas por sección |

**El docente está atrapado en trabajo administrativo que no es enseñar.** Según estudios de la OEI para Costa Rica, los docentes dedican en promedio entre **3 y 5 horas semanales adicionales** solo a tareas administrativas fuera del horario de clase.

### El dolor más grande: doble trabajo

El problema estructural número uno es que el docente **registra la misma información dos veces**:
1. Primero en su sistema propio (Excel, papel)
2. Luego la transcribe a la plataforma del MEP

Si la app resuelve ese doble trabajo, ya justifica su existencia por sí sola.

---

## 2. Visión del Producto Integral

### Nombre tentativo de concepto
**"AulaIA"** / **"ProfeBot"** / **"MEPDesk"** — (el nombre final es secundario, es para ilustrar)

### Propuesta de valor en una frase

> Un asistente digital que acompaña al docente costarricense desde que planea la clase hasta que sube las notas al MEP, eliminando el doble trabajo y el papeleo innecesario.

### Qué es y qué no es

**SÍ es:**
- Una herramienta de uso diario para docentes
- Un generador de planeamientos didácticos basados en los programas del MEP
- Un sistema de registro de asistencia, notas y trabajo cotidiano
- Un generador de reportes e informes en los formatos que el MEP requiere
- Una plataforma SaaS accesible desde web y móvil

**NO es:**
- Una plataforma para estudiantes (al menos no en V1)
- Un sistema de comunicación con padres (en V1)
- Un sustituto del SIMAR, SIRC u otros sistemas del MEP
- Un LMS (Learning Management System) al estilo Google Classroom

---

## 3. Módulos Propuestos (Desglose Funcional)

### Vista general de módulos

```
┌─────────────────────────────────────────────────────────────────┐
│                    PLATAFORMA EDUCATIVA INTEGRAL                 │
├─────────────────┬────────────────┬───────────────┬──────────────┤
│  PLANEAMIENTO   │   ASISTENCIA   │  EVALUACIÓN   │  REPORTES    │
│  (con IA)       │   (QR + Manual)│  (notas, tareas│  (MEP-ready) │
│                 │                │  cotidiano)   │              │
├─────────────────┴────────────────┴───────────────┴──────────────┤
│            GESTIÓN DE GRUPOS Y ESTUDIANTES                       │
│         (nómina, adecuaciones, perfil del estudiante)           │
└─────────────────────────────────────────────────────────────────┘
```

---

### Módulo 1: Gestión de Grupos y Estudiantes

Es el **núcleo de la plataforma**. Todo lo demás depende de este.

**Funcionalidades:**
- Crear institución y asignar datos básicos
- Crear grupos/secciones por año lectivo
- Importar lista de estudiantes (desde CSV o digitación manual)
- Generar código QR único por estudiante
- Perfil del estudiante: datos básicos, adecuación curricular (tipo, diagnóstico, observaciones)
- Historial por estudiante: asistencia, notas, trabajos, observaciones

**Datos que se manejan:**
- Nombre completo
- Número de cédula o expediente
- Sección a la que pertenece
- Código QR personal
- Tipo de adecuación (si aplica)
- Observaciones docentes

---

### Módulo 2: Planeamiento Didáctico con IA

*(Este es el módulo ya investigado en `investigacion_dominio.md`)*

**Funcionalidades:**
- Generación automática de planeamiento anual / trimestral / mensual / semanal / por clase
- Basado en programas oficiales del MEP (Artes Plásticas y otras materias)
- Adapta el planeamiento si hay estudiantes con adecuaciones
- Respeta la plantilla institucional del docente
- Calcula automáticamente el número de lecciones disponibles según el calendario escolar

**Diferencial de IA:**
- No genera contenidos inventados; está anclado al programa oficial del MEP
- Sugiere actividades y recursos contextualizados al nivel y materia
- Genera los tres tipos de contenidos (conceptual, procedimental, actitudinal)
- Redacta con la terminología exacta del MEP

---

### Módulo 3: Asistencia con QR

*(Descrito en detalle en la sección 4)*

---

### Módulo 4: Trabajo Cotidiano y Tareas

**El problema que resuelve:** los docentes tienen múltiples "notas" de cosas pequeñas que los estudiantes hacen en clase (un ejercicio del día, participación, trabajo en clase) que no tienen dónde registrar fácilmente.

**Funcionalidades:**
- Crear una actividad de trabajo cotidiano (nombre, fecha, valor en porcentaje o puntos)
- Registrar la nota de cada estudiante (puede ser por escala numérica o cualitativa)
- Crear tareas (nombre, descripción, fecha de entrega, valor)
- Registrar entrega / no entrega + calificación
- Visualizar rápidamente quién no ha entregado, quién está en rojo
- Filtrar por sección, por estudiante, por período

**Tipos de evaluación que debe soportar:**
| Tipo | Descripción |
|---|---|
| Trabajo cotidiano | Ejercicios, participación, trabajos en clase |
| Tarea | Asignación para hacer fuera del aula |
| Proyecto | Actividad extensa, con rúbrica |
| Prueba o examen | Evaluación formal |
| Autoevaluación / coevaluación | El estudiante evalúa su propio trabajo o el de un compañero |
| Portafolio | Evidencias acumuladas del período |

---

### Módulo 5: Gestión de Notas y Promedio

**El problema que resuelve:** calcular el promedio trimestral según la ponderación del MEP es tedioso y propenso a errores. Los docentes pasan horas con Excel haciendo esto.

**Funcionalidades:**
- Configurar la ponderación de evaluación según la normativa del MEP (porcentajes por tipo de evaluación)
- Calcular automáticamente el promedio acumulado por período
- Alertas automáticas: estudiantes con promedio en riesgo (bajo 65 en primaria, bajo 70 en diversificada)
- Vista de calificaciones por sección (visualización tipo libro de notas)
- Edición rápida de notas

**Ponderación típica en el MEP (secundaria):**

| Componente | Porcentaje |
|---|---|
| Trabajo cotidiano | 20% |
| Pruebas | 45% |
| Trabajo extraclase | 20% |
| Otros (proyectos, portafolios) | 15% |

*(Nota: La ponderación puede variar según el Departamento de Evaluación del MEP por año y reforma. La app debe ser configurable.)*

---

### Módulo 6: Generador de Reportes e Informes

**El problema que resuelve:** al final de cada período, los docentes deben subir notas al sistema del MEP, entregar informes a la dirección y llenar registros de asistencia. Todo eso hoy se hace manualmente.

**Funcionalidades:**
- Generar **acta de notas** (formato compatible con el MEP)
- Generar **reporte de asistencia** (ausencias por período, por estudiante)
- Generar **informe de adecuaciones** (para el CAE / expediente del estudiante)
- Generar **informe del docente** para la dirección
- Exportar en formatos: PDF, Excel (.xlsx), CSV
- Exportar en el formato específico para subir al SIMAR / SIRC u otras plataformas del MEP

---

### Módulo 7: Calendario Escolar Integrado

**Funcionalidades:**
- Visualización del calendario escolar oficial del MEP
- Marcado de días no lectivos (feriados, vacaciones, actividades institucionales)
- Cálculo automático del número de lecciones disponibles por período
- Alertas de fechas de entrega de planeamientos y actas de notas

---

## 4. Módulo Estrella: Asistencia con QR

Este módulo merece análisis especial porque es el que más impacta la vida diaria y el que más diferencia esta app de cualquier otra herramienta que usa el docente hoy.

### Cómo Funciona

**Preparación (una vez):**
1. El docente ingresa los estudiantes de su grupo
2. El sistema genera un **código QR único por estudiante**
3. Cada estudiante imprime o guarda en su teléfono su QR personal

**En cada clase:**
1. El docente abre la app en su teléfono (o en una tableta frente a la clase)
2. Selecciona la sección y fecha
3. Activa el modo "Tomar lista con QR"
4. Cada estudiante pasa su QR frente a la cámara al entrar
5. El sistema registra automáticamente: presente, hora de entrada
6. Los que no pasan QR quedan marcados como ausentes o tardíos

**Variantes de uso:**
| Modalidad | Cuándo usar |
|---|---|
| QR individual | El estudiante pasa su código; ideal para clases con pocos grupos |
| Lista manual rápida | El docente marca con tap; para clases donde no hay tiempo |
| Modo híbrido | QR para los que llegan a tiempo, manual para los que llegan tarde |

### Por qué el QR es mejor que las alternativas

| Método | Tiempo estimado por clase | Problemas |
|---|---|---|
| Pasar lista oral | 3–7 minutos | Interrumpe la clase, el docente se distrae |
| Lista en papel | 2–5 minutos | No hay respaldo digital, riesgo de perderla |
| Lista en Excel | 2–4 minutos | No se hace en tiempo real, hay que transcribir |
| QR con esta app | < 1 minuto | Automático, digital, con historial inmediato |

**En un colegio con 10 grupos de 35 estudiantes, el docente pasa lista unos 50 veces al trimestre. El QR puede ahorrarle entre 3 y 4 horas por trimestre solo en ese proceso.**

### Consideraciones técnicas del QR

- El QR puede ser un **UUID único del estudiante** generado al momento de la inscripción
- No necesita conexión a internet si la app tiene modo offline con sincronización posterior
- El código QR es imprimible en papel o puede mostrarse desde el celular del estudiante
- Consideración de privacidad: el QR no debe exponer datos sensibles del menor; solo un identificador

### Casos Edge que la App Debe Manejar

- Estudiante olvidó el QR → el docente puede marcarlo manualmente
- El estudiante llegó tarde → opción "tarde" además de "presente"
- Justificación de ausencia → el docente puede agregar nota de justificación después
- Salida temprana → opción adicional
- Días no lectivos que no deben contarse en el registro

---

## 5. Integración con Plataformas del MEP

### Las Plataformas Digitales del MEP

El MEP de Costa Rica tiene varios sistemas digitales que los docentes deben usar:

| Sistema | Propósito | Quién lo usa |
|---|---|---|
| **SIMAR** (Sistema de Matrícula y Registro) | Matrícula de estudiantes, registro oficial de notas | Docentes, directores |
| **SIRC** | Reportes de calificaciones | Docentes |
| **SIMED** | Información estadística del sistema educativo | Gestión institucional |
| **Expediente Digital del Estudiante** | Historial del estudiante (en implementación) | Equipos de apoyo, dirección |
| **Plataforma de planeamiento** | Subida del planeamiento didáctico (varía por institución) | Docentes |

### El Problema de la Integración

Actualmente no existe una **API pública del MEP** que permita integración directa con estos sistemas. Esto es una limitante importante.

**¿Qué se puede hacer en su lugar?**

| Estrategia | Descripción | Viabilidad |
|---|---|---|
| **Exportar en formato compatible** | La app genera un Excel o CSV en el exacto formato que el SIMAR acepta para importación | Alta — muchos sistemas aceptan importaciones masivas |
| **Instrucciones de pegado asistido** | La app genera la tabla de notas y guía al docente para copiar/pegar en el sistema del MEP paso a paso | Alta — no requiere API |
| **Lobby para integración oficial** | Proponer al MEP una integración formal (largo plazo) | Baja en corto plazo, posible en 2–3 años |
| **Web scraping** | Automatizar el ingreso de datos al sistema del MEP (técnicamente posible) | No recomendado — viola términos de uso, frágil ante cambios |

### Recomendación

La estrategia más viable es generar **archivos de exportación en el formato exacto del MEP**. Esto significa:
1. Investigar qué formato acepta el SIMAR para importación de notas
2. Generar ese archivo con un clic
3. El docente solo descarga y sube el archivo al sistema del MEP

Esto resuelve el problema del doble trabajo sin requerir integración técnica compleja.

---

## 6. Modelo de Negocio: SaaS por Suscripción

### Por qué SaaS es el modelo correcto

- El docente usa la app **todos los días del año lectivo** → justifica un pago recurrente
- El costo de adquisición (onboarding) es alto; el SaaS premia la retención
- Permite mejora continua del producto sin reinstalaciones
- Los docentes ya están familiarizados con pagar suscripciones (Netflix, Spotify, etc.)

### Estructura de Precios Sugerida

| Plan | Precio/mes | Para quién | Qué incluye |
|---|---|---|---|
| **Básico** | $5–8 USD | Docente individual | Planeamiento + asistencia + notas básicas (máx. 5 grupos) |
| **Profesional** | $12–18 USD | Docente con muchos grupos | Todo + adecuaciones + reportes exportables + sin límite de grupos |
| **Institucional** | $80–150 USD/mes | Colegio completo | Todos los docentes + panel de dirección + reportes institucionales |

### Contexto para la Fijación de Precios en Costa Rica

- El salario de un docente del MEP en Costa Rica varía según grado profesional; un docente con grado 1 puede ganar alrededor de **600,000–900,000 colones/mes** (~$1,100–1,700)
- Un precio de **$10–15/mes** equivale al **0.7–1.3% de su salario mensual** → comparable con lo que muchos pagan por Spotify
- Si el sistema les ahorra 3–5 horas semanales, el retorno es inmediato

### Proyección de Mercado (estimación conservadora)

| Segmento | Docentes estimados en CR | Penetración año 1 | Ingresos mensuales proyectados |
|---|---|---|---|
| Docentes secundaria pública | ~20,000 | 0.5% (100 usuarios) | $1,200–1,800 |
| Docentes secundaria privada | ~8,000 | 1% (80 usuarios) | $960–1,440 |
| Docentes primaria pública | ~22,000 | 0.2% (44 usuarios) | $220–350 |
| **Total conservador año 1** | — | **~224 docentes** | **~$2,380–3,590/mes** |

Esto puede sonar pequeño, pero:
- El crecimiento en EdTech es exponencial si el producto es bueno (docentes se recomiendan entre sí)
- El plan institucional (colegios completos) puede triplicar el ingreso
- Si se llega al 5% del mercado total (~2,500 docentes), los ingresos serían **$30,000–45,000/mes**

### Estrategia de Adquisición

1. **Prueba gratuita de 30 días** (sin necesidad de tarjeta de crédito) → baja el riesgo percibido
2. **Embajadores docentes:** identificar docentes influyentes en redes, comunidades de WhatsApp, Facebook
3. **Grupos de Facebook de docentes del MEP:** hay grupos activos con decenas de miles de miembros
4. **Demostraciones en colegios:** una demo de 20 minutos a los docentes de una institución puede convertir todo el staff
5. **Plan institucional como palanca:** si el director compra, todos los docentes del colegio usan

---

## 7. Análisis de Mercado y Competencia en Costa Rica

### Competencia Directa (en Costa Rica)

**Respuesta corta: no hay competencia directa real.**

No existe actualmente en Costa Rica ninguna plataforma que combine:
- Generación de planeamientos según programas del MEP
- Asistencia digital con QR
- Gestión de notas integrada
- Exportación a formatos del MEP

### Lo que usan hoy (competencia indirecta)

| Herramienta | Qué resuelve | Lo que no resuelve |
|---|---|---|
| **Google Classroom** | Tareas, comunicación, recursos | No tiene planeamiento MEP, no genera reportes para el MEP, no hace asistencia |
| **Microsoft Teams** | Comunicación y colaboración | No pedagógico, no contextualizado al MEP |
| **Excel** | Notas y asistencia | Manual, no integrado, sin IA, sin exportación al MEP |
| **Word/Google Docs** | Planeamiento | Desde cero, sin IA, sin plantillas del MEP |
| **WhatsApp** | Comunicación con padres y estudiantes | Sin trazabilidad, mezcla vida personal y profesional |
| **ClassDojo** | Comunicación con padres, recompensas | En inglés, no contextualizado al MEP, sin planeamiento |

### Competencia Internacional con Posibilidad de Entrar

- **MagicSchool.ai:** No conoce el MEP, está en inglés, no tiene QR, no genera reportes para el MEP
- **Additio App (España):** Es la app más completa de Europa para docentes; tiene libro de calificaciones, asistencia y diario. **Es la competencia más cercana en concepto.** Sin embargo, no está contextualizada para Costa Rica ni para el MEP
- **iDoceo (España):** Similar a Additio, muy completa, pero solo iOS, sin IA, sin integración MEP

**Conclusión:** hay un nicho claro. El que primero llegue con una herramienta bien ejecutada y adaptada al MEP costarricense, **se convierte en el estándar**.

---

## 8. Estrategia de Go-to-Market y Ventas

### El problema de vender a docentes del MEP

Vender tecnología a docentes del sector público costarricense tiene una dinámica particular que no se parece a vender un SaaS B2B corporativo. Los docentes:

- **No tienen presupuesto institucional para herramientas digitales** (lo pagan de su bolsillo)
- **Toman decisiones por confianza, no por marketing** — un docente prueba algo porque un colega de confianza se lo recomendó
- **Son altamente escépticos ante promesas tecnológicas** — han visto muchas modas que no duran
- **Tienen redes sociales muy activas entre sí** — grupos de WhatsApp por circuito, por materia, por región; grupos de Facebook con decenas de miles de miembros
- **Respetan la autoridad del par con trayectoria** — un docente con 24 años de experiencia y alto grado académico tiene más influencia que cualquier campaña publicitaria

Esto significa que la estrategia de ventas no es de publicidad masiva. Es de **confianza, credibilidad y boca a boca dirigido**.

---

### La Alianza Estratégica: Adriana Guido

#### Por qué Adriana es el activo más valioso del proyecto

Adriana Guido, docente de Artes Plásticas con 24 años de experiencia y plaza en el Colegio de Aserrí, no es solo una fuente de información del dominio. Es **la llave de entrada al mercado**.

Análisis de su valor estratégico:

| Atributo | Por qué importa para el proyecto |
|---|---|
| **24 años de experiencia** | Credibilidad máxima ante otros docentes. Lo que ella avale, los demás escuchan |
| **Altamente titulada** | En el mundo del MEP, el grado académico equivale a autoridad profesional |
| **Plaza en Colegio de Aserrí** | Acceso directo a un colegio real con docentes reales; laboratorio de prueba ideal |
| **Red de pares construida en 24 años** | Conoce a docentes de toda la región, de múltiples colegios, de asociaciones gremiales |
| **Usuaria del producto** | No es una asesora externa; ella misma usará la app en su trabajo diario |
| **Conocimiento del dolor** | Sabe exactamente qué problema resuelve la app porque lo vive todos los días |

#### El rol formal que debe tener Adriana en el proyecto

Adriana no debe ser simplemente una "docente que prueba la app". Debe tener un **rol formal y remunerado** que reconozca su contribución y la comprometa con el éxito del producto.

**Rol sugerido: Co-fundadora / Directora Pedagógica**

Esto implica:
- Participar en decisiones de producto (qué funciones son útiles de verdad, qué terminología usar, qué flujos tienen sentido para un docente real)
- Ser la primera usuaria real del MVP en condiciones de producción real
- Ser el rostro pedagógico del producto (la cara técnica es el desarrollador; la cara educativa es Adriana)
- Abrir puertas en su red de contactos del MEP

**¿Qué recibe Adriana a cambio?**

| Beneficio | Descripción |
|---|---|
| **Cuenta gratuita de por vida** | No paga suscripción nunca |
| **Participación en ingresos** | Un porcentaje de las suscripciones generadas por docentes que ella refiera o que vengan de su red directa |
| **Crédito como co-fundadora** | En la app, en el sitio web, en medios — su nombre y trayectoria dan credibilidad |
| **Acceso anticipado a nuevas funciones** | Ella las prueba primero y da retroalimentación antes del lanzamiento |

---

### Estrategia de Entrada al Mercado: El Modelo Beachhead

Un **beachhead** (cabeza de playa) es el concepto de conquistar un segmento pequeño y específico primero, y desde ahí expandirse. Es la estrategia correcta para este producto.

#### Beachhead elegido: Docentes de Artes Plásticas de secundaria, zona sur de San José

**¿Por qué este segmento específico?**
- Adriana está en Aserrí → acceso directo al Circuito 06 de la Dirección Regional de San José
- Artes Plásticas es el programa con el que ya se construyó el módulo de IA
- Los docentes de una misma materia tienen canales de comunicación propios (reuniones de área, grupos de WhatsApp por materia)
- Son pocos (~500 docentes de Artes Plásticas en secundaria pública en todo CR) → más manejable que un lanzamiento masivo

#### Expansión en ondas concéntricas

```
         Onda 1 (mes 1–3)
    ┌────────────────────────┐
    │  Colegio de Aserrí     │  ← Adriana usa la app; sus colegas la ven
    │  (~10–15 docentes)     │
    └────────────┬───────────┘
                 │
         Onda 2 (mes 3–6)
    ┌────────────────────────┐
    │  Circuito 06 / región  │  ← Adriana lo recomienda en reunión de área
    │  sur de San José       │    o en su grupo de WhatsApp de docentes
    │  (~50–80 docentes)     │
    └────────────┬───────────┘
                 │
         Onda 3 (mes 6–12)
    ┌────────────────────────┐
    │  Docentes de Artes     │  ← Publicación en grupos de Facebook MEP,
    │  Plásticas a nivel     │    presentación en jornada pedagógica,
    │  nacional (~500)       │    video testimonio de Adriana
    └────────────┬───────────┘
                 │
         Onda 4 (mes 12+)
    ┌────────────────────────┐
    │  Otras materias del    │  ← El producto ya tiene reputación;
    │  MEP (Música, Ed.      │    se agregan nuevos programas de estudio
    │  para el Hogar, etc.)  │
    │  (~50,000 docentes)    │
    └────────────────────────┘
```

---

### Canales de Venta y Comunicación

#### Canal 1: WhatsApp (el canal más poderoso en CR)

Los docentes del MEP tienen grupos de WhatsApp organizados por:
- Materia (ej. "Docentes Artes Plásticas CR")
- Circuito educativo
- Institución
- Año de graduación universitaria
- Asociaciones gremiales

Una recomendación de Adriana en esos grupos vale más que cualquier anuncio de Facebook.

**Estrategia:** Adriana comparte en sus grupos un video corto de 60 segundos mostrando cómo usa la app para tomar asistencia. No es publicidad: es una docente mostrando su herramienta de trabajo. Eso genera conversación orgánica.

#### Canal 2: Grupos de Facebook de docentes del MEP

Existen grupos activos con decenas de miles de docentes:
- "Docentes de Costa Rica" (grupos generales con +30,000 miembros)
- Grupos por asignatura
- Grupos por región educativa

**Estrategia:** Publicaciones de Adriana (no de la empresa) mostrando el producto. El formato más efectivo: "Llevo 24 años pasando lista en papel. La semana pasada lo cambié por esto."

#### Canal 3: Reuniones de área y jornadas pedagógicas

El MEP organiza reuniones periódicas por materia ("reuniones de área") donde los docentes de una misma asignatura de un circuito se reúnen. También hay jornadas pedagógicas regionales y nacionales.

**Estrategia:** Adriana hace una demostración de 15 minutos en la próxima reunión de área de Artes Plásticas del Circuito 06. Con 20 docentes en la sala, si 5 se suscriben, el boca a boca comienza.

#### Canal 4: Universidad de Costa Rica / UNED / UNA

Los docentes en formación (estudiantes de Enseñanza de las Artes Plásticas) son un canal de adopción temprana muy valioso:
- Son jóvenes con mayor disposición tecnológica
- Adoptarán la herramienta antes de tener malos hábitos establecidos
- Cuando se gradúen, llevan la app con ellos a sus colegios

**Estrategia:** Contactar a profesores de las carreras de Enseñanza de las Artes en las universidades públicas para incluir la app como herramienta de práctica docente.

#### Canal 5: IDP (Instituto de Desarrollo Profesional Uladislao Gámez Solano)

El IDP es la entidad del MEP que capacita a los docentes. Ofrece cursos de actualización que los docentes toman para acumular puntos en su carrera profesional.

**Estrategia a mediano plazo:** Proponer al IDP la inclusión de la app en algún curso de actualización sobre herramientas tecnológicas para docentes. Si el IDP la avala, la adopción se multiplica instantáneamente.

---

### El Discurso de Ventas (Pitch)

El error más común al vender a docentes es hablar de tecnología. Al docente no le importa la IA ni el QR. Le importa **su tiempo y su tranquilidad**.

#### Lo que NO funciona como argumento de venta:
> "Nuestra app usa inteligencia artificial para generar planeamientos con modelos de lenguaje de última generación..."

#### Lo que SÍ funciona:
> **"¿Cuántas horas te tomó hacer el planeamiento del trimestre pasado? Con esta app, lo haces en 15 minutos. Y tomar lista en cada clase te toma menos de un minuto."**

#### El pitch en tres oraciones (para WhatsApp):
> *"Hice el planeamiento de Artes Plásticas de 7° para todo el trimestre en 15 minutos. La app conoce el programa del MEP y lo genera completa, con actividades, evaluación y todo. Pruébala gratis un mes."*

---

### Estructura de la Alianza con Adriana: Acuerdo Formal

Para que la alianza funcione bien y no haya malentendidos a futuro, es importante formalizar el acuerdo desde el inicio.

**Puntos clave del acuerdo:**

| Punto | Descripción |
|---|---|
| **Rol** | Co-fundadora / Directora de Contenido Pedagógico |
| **Dedicación esperada** | 3–5 horas semanales en fase de desarrollo; 1–2 horas semanales en fase de operación |
| **Responsabilidades** | Validación pedagógica, pruebas de usuario, recomendación en su red, retroalimentación continua |
| **Compensación en desarrollo** | Cuenta gratuita de por vida + participación en equity o revenue share (a definir) |
| **Compensación en operación** | Porcentaje de ingresos de usuarios referidos por su red (sugerido: 10–15% de la suscripción mensual de cada usuario referido, por 12 meses) |
| **Confidencialidad** | Acuerdo de no divulgar información técnica o de negocio sensible |
| **Duración** | Mínimo 2 años; renovable |

> **Nota importante:** Este acuerdo debe hacerse por escrito, con firma, aunque sea entre conocidos. No por desconfianza, sino para proteger la relación. Un acuerdo claro evita malentendidos cuando el dinero empieza a fluir.

---

### Métricas de Éxito de la Estrategia de Ventas

| Métrica | Meta mes 3 | Meta mes 6 | Meta mes 12 |
|---|---|---|---|
| Docentes usando el MVP | 20 | 100 | 500 |
| Docentes pagando suscripción | 5 | 40 | 200 |
| Colegios con plan institucional | 0 | 1 | 5 |
| NPS (satisfacción del usuario) | N/A | > 50 | > 60 |
| Fuente de adquisición principal | Red de Adriana | Red de Adriana + FB | Múltiples canales |

---

## 9. Análisis de Viabilidad Técnica

### Plataformas Objetivo: iOS + Android + Web

La app **debe existir en tres formatos simultáneos**. No es opcional: la realidad del docente costarricense exige las tres plataformas.

| Plataforma | Por qué es necesaria | Casos de uso principales |
|---|---|---|
| **App iOS (iPhone/iPad)** | Gran parte de los docentes con mayor poder adquisitivo usa iPhone; también directores y colegios privados | Asistencia con QR en el aula, consulta rápida de notas, notificaciones |
| **App Android** | La plataforma dominante en Costa Rica por costo accesible; la mayoría de docentes del sector público usa Android | Idéntico al iOS; es la plataforma más crítica por volumen |
| **Web (navegador)** | Para tareas de escritorio: crear planeamientos, gestionar grupos, generar reportes, exportar archivos | Planeamiento con IA, libro de notas completo, descarga de PDFs y reportes |

#### Estrategia de desarrollo: un solo código para las tres plataformas

Desarrollar tres apps por separado (una en Swift para iOS, una en Kotlin para Android, una en React) triplicaría el costo y el tiempo. La estrategia correcta es:

**Opción A — React Native + Next.js (recomendada)**
- React Native genera apps nativas para iOS y Android desde un único código TypeScript/JavaScript
- Next.js genera la versión web usando los mismos componentes lógicos
- Comparte lógica de negocio (cálculo de promedios, sincronización, modelos de datos)
- Equipos pequeños pueden mantener las tres plataformas con 2–3 desarrolladores
- Acceso a la cámara nativa del dispositivo (necesario para el QR)

**Opción B — Flutter**
- Un solo código Dart compila a iOS, Android y web
- Muy buen rendimiento en móvil
- La versión web de Flutter es funcional pero menos optimizada que React para web complejas
- Mejor opción si el equipo prefiere un solo lenguaje

| Criterio | React Native + Next.js | Flutter |
|---|---|---|
| Madurez del ecosistema | Alta (React es el estándar web) | Alta (Google lo mantiene activamente) |
| Rendimiento móvil | Muy bueno | Excelente |
| Versión web | Excelente | Buena (mejorando) |
| Tamaño de la comunidad | Muy grande | Grande |
| Acceso a cámara / QR | Sí (Expo Camera) | Sí (mobile_scanner) |
| Curva de aprendizaje | Media si se conoce JS | Media si se aprende Dart |

---

### Stack Tecnológico Recomendado

| Capa | Tecnología sugerida | Razón |
|---|---|---|
| **App iOS y Android** | React Native (Expo) | Un solo código TypeScript para ambas plataformas; acceso a cámara, notificaciones push y almacenamiento local nativo |
| **App Web** | Next.js (React) | Comparte componentes con React Native vía librerías compartidas; SSR para SEO; ideal para las tareas de escritorio |
| **Backend** | Node.js + NestJS / .NET 10 | APIs REST robustas; .NET si el equipo ya lo conoce |
| **Base de datos (servidor)** | PostgreSQL | Relacional, robusta para notas y asistencia; fácil escalar |
| **Base de datos (local/offline)** | SQLite (móvil) + IndexedDB (web) | Almacenamiento estructurado sin internet |
| **Sincronización offline** | WatermelonDB o RxDB | Librerías diseñadas para sincronización offline-first con el servidor |
| **IA / LLM** | Azure OpenAI o Gemini API | Para la generación de planeamientos con IA (requiere internet; funciona cuando hay conexión) |
| **Lector de QR** | Expo Camera / mobile_scanner | Acceso a la cámara nativa; funciona completamente offline |
| **Autenticación** | Auth0 / Supabase Auth | OAuth, JWT, múltiples proveedores; soporte para sesiones persistentes |
| **Almacenamiento** | Azure Blob Storage / S3 | Para PDFs de planeamientos y archivos exportados |
| **Hosting** | Azure App Service o Vercel | Escalable, confiable |
| **Notificaciones push** | Firebase Cloud Messaging (FCM) | Para iOS y Android; gratuito |

---

### Modo Offline — Análisis de Viabilidad Detallado

**¿Es viable? Sí. ¿Es complejo? Sí también. Pero es absolutamente necesario.**

#### La realidad de la conectividad en Costa Rica

Costa Rica tiene una brecha de conectividad significativa:
- Colegios en zonas rurales (Limón, zonas indígenas, Guanacaste rural) pueden tener conexión muy lenta o nula
- Incluso en la GAM (Gran Área Metropolitana), muchos colegios tienen WiFi solo en la sala de cómputo, no en las aulas
- El docente en el aula típicamente depende de sus datos móviles del celular
- Operadoras como Claro y Movistar tienen cobertura 4G en áreas urbanas, pero 2G/3G o sin señal en zonas rurales
- **Una app que requiere internet constante simplemente no es usable en el contexto real del docente costarricense**

#### Qué debe funcionar offline vs. qué puede requerir internet

| Función | ¿Funciona offline? | Justificación |
|---|---|---|
| Tomar asistencia (QR o manual) | **Sí, siempre** | Es la función más usada; no puede depender de internet |
| Registrar notas y trabajos | **Sí, siempre** | El docente registra datos en el aula en tiempo real |
| Ver lista de estudiantes y grupos | **Sí, siempre** | Datos ya descargados al dispositivo |
| Ver planeamientos guardados | **Sí, siempre** | Se descargan cuando hay conexión |
| Calcular promedios | **Sí, siempre** | Es lógica local, no requiere servidor |
| Ver calendario y fechas | **Sí, siempre** | Datos estáticos del año lectivo |
| Generar nuevo planeamiento con IA | **No — requiere internet** | La IA (LLM) corre en la nube |
| Exportar PDF / reportes | **Parcial** | Puede generarse localmente; subir requiere internet |
| Sincronizar datos al servidor | **No — requiere internet** | Se hace automáticamente cuando hay conexión |
| Recibir notificaciones push | **No — requiere internet** | — |

#### Arquitectura Offline-First

El principio es: **la app almacena todo localmente primero y sincroniza cuando hay internet**, no al revés.

```
┌─────────────────────────────────────────────────────────┐
│                  DISPOSITIVO DEL DOCENTE                 │
│                                                         │
│  ┌─────────────────┐     ┌─────────────────────────┐   │
│  │   App (UI)       │────▶│  Base de datos local     │   │
│  │  React Native /  │     │  SQLite (móvil)          │   │
│  │  Next.js         │     │  IndexedDB (web)         │   │
│  └─────────────────┘     └──────────┬──────────────┘   │
│                                      │                   │
│                          Cola de cambios pendientes      │
│                          (asistencia, notas, etc.)       │
└──────────────────────────────────────┼──────────────────┘
                                       │
                            ¿Hay internet?
                           /              \
                         SÍ               NO
                          │               │
                    Sincroniza      Guarda en cola
                    al servidor     para después
                          │
              ┌───────────▼───────────┐
              │   BACKEND (nube)      │
              │   PostgreSQL          │
              │   API REST            │
              └───────────────────────┘
```

#### El problema técnico más difícil: conflictos de sincronización

Si el docente usa la app en su teléfono sin internet y otro docente (o el mismo docente en otro dispositivo) modifica los mismos datos con internet, al sincronizar puede haber **conflictos**.

Ejemplo concreto:
- El docente registra asistencia en su celular sin internet
- Alguien modifica la lista de estudiantes desde la web
- Al sincronizar, ¿cuál versión gana?

**Estrategia de resolución:**

| Estrategia | Descripción | Para este caso |
|---|---|---|
| **Last-write-wins** | El dato más reciente siempre gana | Para notas y asistencia (el docente es el único que edita sus datos) |
| **Merge automático** | Se fusionan los cambios no conflictivos | Para listas de estudiantes |
| **Notificación al usuario** | Se le muestra al docente el conflicto para que decida | Para casos raros de edición simultánea |

En la práctica, los conflictos son raros porque **cada docente solo edita sus propios datos**. Un docente no edita la asistencia de otro docente. Esto simplifica enormemente la lógica de conflictos.

#### Librerías que resuelven esto

No hay que construir la sincronización desde cero:
- **WatermelonDB:** Librería de base de datos offline-first para React Native y React. Diseñada específicamente para este problema. Usada en Nozbe, Droplr y otras apps de producción.
- **PowerSync:** Servicio que sincroniza PostgreSQL con SQLite local de forma automática. Permite offline-first sin construir la lógica de sincronización manualmente.
- **ElectricSQL:** Similar a PowerSync; sincronización PostgreSQL ↔ SQLite local.

**Recomendación:** PowerSync o ElectricSQL, porque permiten tener PostgreSQL en el servidor (que ya era la elección para la base de datos principal) y SQLite en el dispositivo, con sincronización automática y resolución de conflictos integrada.

#### Tamaño de datos a almacenar localmente

Estimación por docente con 10 grupos de 35 estudiantes:

| Datos | Tamaño estimado |
|---|---|
| Lista de estudiantes (350 registros) | < 1 MB |
| Asistencia de un trimestre (350 × 60 clases) | < 5 MB |
| Notas de un año completo | < 10 MB |
| Planeamientos del año | < 20 MB |
| **Total por docente** | **< 40 MB** |

Esto es completamente manejable en cualquier celular moderno (incluso uno de gama baja con 32 GB de almacenamiento).

---

### Complejidad Técnica por Módulo

| Módulo | Complejidad | Riesgo | Offline |
|---|---|---|---|
| Gestión de estudiantes | Baja | Bajo | Sí |
| Planeamiento con IA | Alta | Medio (calidad del output) | Solo lectura (ver planeamientos guardados) |
| Asistencia con QR | Media | Bajo | Sí, 100% |
| Trabajo cotidiano y notas | Media | Bajo | Sí |
| Cálculo de promedios | Media | Bajo | Sí (lógica local) |
| Generador de reportes / PDF | Media | Medio | Parcial |
| Exportación compatible MEP | Alta | Alto | No (requiere conexión para subir) |
| Modo offline + sincronización | Alta | Alto (conflictos de datos) | Es el módulo base |
| App nativa iOS + Android | Media | Bajo con Expo | Incluido en React Native |

### Tiempo de Desarrollo Estimado (con equipo pequeño de 2–3 personas)

| Fase | Módulos | Duración estimada |
|---|---|---|
| **MVP (V1)** | Estudiantes + Asistencia QR + Notas básicas + Offline base | 4–5 meses |
| **V2** | Planeamiento con IA + Trabajo cotidiano + App iOS/Android en stores | 3–4 meses adicionales |
| **V3** | Reportes exportables + Adecuaciones + Calendario + Sincronización avanzada | 2–3 meses adicionales |
| **Producto completo** | Todo lo anterior + refinamientos + pruebas de campo en colegios | ~12–14 meses total |

---

## 10. Riesgos Reales del Proyecto

### Riesgo 1: Cambios en los programas del MEP ⚠️ ALTO

**Descripción:** El MEP puede actualizar los programas de estudio. En 2013 hubo una reforma completa de Artes Plásticas. Puede volver a ocurrir.

**Mitigación:** Estructurar el conocimiento curricular en una base de datos administrable, no en código fijo. Crear un proceso de actualización cuando el MEP publica nuevos programas.

---

### Riesgo 2: Cambios en las plataformas del MEP ⚠️ ALTO

**Descripción:** Si la exportación depende del formato del SIMAR y ese formato cambia, hay que actualizar.

**Mitigación:** No hacer integración directa. Exportar formatos estándar (Excel, CSV, PDF) y documentar claramente cómo el docente los usa. Menos dependencia del MEP.

---

### Riesgo 3: Adopción del docente 🔶 MEDIO

**Descripción:** Los docentes mayores pueden resistirse a aprender una nueva herramienta. "Siempre lo he hecho en papel y funciona."

**Mitigación:** La interfaz debe ser **extraordinariamente simple**. El QR debe funcionar en el primer intento. Crear tutoriales en video cortos. El boca a boca entre docentes es el canal de adopción más poderoso.

---

### Riesgo 4: Privacidad de datos de menores 🔶 MEDIO

**Descripción:** La app maneja datos de estudiantes menores de edad (nombres, notas, asistencia). Esto tiene implicaciones legales en Costa Rica (Ley 8968, Ley de Protección de la Persona frente al Tratamiento de sus Datos Personales).

**Mitigación:**
- Los datos de estudiantes NO son del docente, son de la institución / MEP
- La app no debe compartir datos de estudiantes con terceros
- Implementar las medidas del RGPD / Ley 8968
- Acuerdos de procesamiento de datos con las instituciones educativas
- El QR no debe contener información personal, solo un identificador interno

---

### Riesgo 5: Monetización vs. Sector Público 🔶 MEDIO

**Descripción:** Muchos docentes del sector público pueden resistirse a pagar por una herramienta que "el MEP debería proveer gratis". Existe la percepción de que el Estado debería dar estas herramientas.

**Mitigación:** Esto se trabaja con el argumento del tiempo ahorrado. Si la app les ahorra 4 horas semanales, un precio de $10/mes es trivial comparado con su tiempo. Además, la app puede ofrecerse con plan gratuito básico y cobrar por las funciones avanzadas.

---

### Riesgo 6: Calidad del planeamiento generado por IA 🔶 MEDIO

**Descripción:** Si la IA genera planeamientos con errores de contenido, terminología incorrecta o desapegados del programa del MEP, los docentes perderán confianza rápidamente.

**Mitigación:** El módulo de IA debe estar **anclado a los datos del programa oficial**, no generar libremente. El docente siempre debe poder editar el resultado antes de usarlo.

---

### Riesgo 7: Competencia del MEP mismo 🟡 BAJO

**Descripción:** En teoría, el MEP podría desarrollar o licenciar su propia herramienta.

**Mitigación:** El MEP históricamente ha sido muy lento en digitalización. Llevan años intentando digitalizar el expediente del estudiante. No es una amenaza real en el corto-mediano plazo. Si el MEP decide comprar o licenciar la app, eso es una victoria.

---

## 11. Estrategia Recomendada (Fases de Desarrollo)

### Principio rector: lanzar rápido, aprender de los docentes reales

El error más común en EdTech es construir el producto ideal antes de tener usuarios reales. Los docentes costarricenses tienen necesidades muy específicas que solo se descubren con uso real.

---

### Fase 1: MVP — "El diario digital del docente" (3–4 meses)

**Objetivo:** Tener algo que los docentes quieran usar cada día, aunque sea básico.

**Qué incluye:**
- Registro de grupos y estudiantes (importación simple por CSV o digitación)
- Asistencia: lista manual rápida (tap en la pantalla) + QR básico
- Registro de trabajo cotidiano y notas (entrada simple de números)
- Cálculo automático de promedio trimestral
- Vista del libro de notas por sección
- Exportación básica a Excel

**Lo que NO incluye (deliberadamente):**
- IA para planeamiento (deja esto para la Fase 2)
- Adecuaciones curriculares
- Reportes complejos

**Precio en Fase 1:** Gratis o $3–5/mes (ganar usuarios, no dinero todavía)

**Meta de validación:** 50 docentes usando la app diariamente durante un mes. Si lo usan, el producto funciona.

---

### Fase 2: El asistente pedagógico (3–4 meses después del MVP)

**Qué incluye:**
- Módulo de planeamiento con IA (el núcleo original)
  - Artes Plásticas I y II Ciclo
  - Artes Plásticas III Ciclo (7°, 8°, 9°)
- Gestión de tareas con fecha de entrega
- Alertas de estudiantes con bajo rendimiento
- Exportación mejorada (PDF de libro de notas, acta de notas)
- Plan de asistencia mensual exportable

**Precio en Fase 2:** Plan básico gratuito + Plan profesional $10–15/mes

---

### Fase 3: Plataforma completa (4–5 meses después de la Fase 2)

**Qué incluye:**
- Módulo de adecuaciones curriculares
- Calendario escolar oficial integrado
- Más materias en el módulo de planeamiento (Educación Musical, Educación para el Hogar, etc.)
- Generador de reportes para el MEP (formato compatible con SIMAR)
- Panel de dirección para el plan institucional
- Notificaciones push en móvil
- Modo offline completo

**Precio en Fase 3:** Activar plan institucional ($80–150/mes por colegio)

---

## 12. Veredicto Final

### Puntaje de Viabilidad por Dimensión

| Dimensión | Puntaje | Observación |
|---|---|---|
| **Necesidad real del mercado** | 9/10 | El dolor del docente es real y está subatendido |
| **Tamaño de mercado** | 7/10 | Costa Rica es pequeño; hay que pensar en expansión regional (Panamá, Guatemala, etc.) |
| **Diferenciación** | 9/10 | No hay competencia directa contextualizada al MEP |
| **Viabilidad técnica** | 8/10 | Todo es construible con tecnología existente |
| **Modelo de negocio** | 7/10 | El SaaS funciona, pero requiere masa crítica de usuarios para ser rentable |
| **Riesgos** | 6/10 | Los riesgos son manejables pero hay que planificar |
| **Tiempo al mercado** | 8/10 | Un MVP básico es alcanzable en 3–4 meses |

### Mi opinión directa sobre qué hacer

**Construye el MVP de uso diario primero (asistencia + notas), no el módulo de IA.**

Esta es la recomendación más importante. La razón:

1. El módulo de uso diario (asistencia, notas) **engancha al docente** con la app. Lo hace volver todos los días.
2. El módulo de planeamiento con IA es **de alto valor pero de bajo uso frecuente** (una vez al trimestre o mes).
3. Si construyes el planeamiento primero, el docente lo usa una vez y se olvida. No hay retención.
4. Si construyes el diario digital primero, el docente lo usa **cada día de clase** → retención natural → disposición a pagar.

**Después, el módulo de IA se convierte en el "plus premium"** que justifica pagar el plan profesional.

### La visión correcta del producto

No es una app de planeamiento con IA. Es una **herramienta de acompañamiento diario al docente costarricense**, que además tiene IA para hacerle la vida más fácil.

El planeamiento con IA es el diferenciador técnico y el argumento de venta. El diario digital (asistencia, notas, tareas) es lo que hace que el docente **no pueda vivir sin la app**.

### Potencial de escala regional

Costa Rica es el mercado piloto ideal: educación de calidad, docentes con acceso a tecnología, sistema educativo homogéneo (un solo MEP). Pero una vez validado el modelo, **el mismo producto puede adaptarse para:**
- Panamá (MEDUCA)
- Guatemala (MINEDUC)
- Honduras (SEDUC)
- El Salvador (MINED)
- Nicaragua (MINED)

Cada uno tiene su propio currículo, pero la arquitectura del producto es la misma. El diferenciador en cada país es la base de datos del currículo oficial.

---

## Resumen Ejecutivo

| Aspecto | Evaluación |
|---|---|
| **¿Existe el problema?** | Sí, es real y diario |
| **¿Hay solución hoy en Costa Rica?** | No (nicho desatendido) |
| **¿Es técnicamente posible?** | Sí, con tecnología estándar |
| **¿Hay mercado suficiente?** | Sí, ~50,000 docentes solo en CR |
| **¿El modelo SaaS funciona?** | Sí, si se logra retención por uso diario |
| **¿Cuándo lanzar el primer MVP?** | En 3–4 meses con equipo pequeño |
| **¿Qué construir primero?** | Asistencia + notas (uso diario), DESPUÉS planeamiento con IA |
| **Mayor riesgo?** | Calidad del planeamiento generado y cambios en formatos del MEP |
| **Potencial de escala?** | Regional (Centroamérica y más) |

---

*Documento elaborado como insumo para la planificación del producto. Mayo 2026.*
