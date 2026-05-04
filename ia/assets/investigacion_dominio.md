# Investigación de Dominio: Asistente Pedagógico MEP

> **Documento preparado por:** GitHub Copilot  
> **Fecha:** Mayo 2026  
> **Propósito:** Investigación profunda del dominio para la construcción de una aplicación educativa asistida por IA para docentes del MEP de Costa Rica  
> **Fuentes:** PDFs oficiales del MEP (Programas de Estudio de Artes Plásticas), Wikipedia, sitio oficial del MEP (mep.go.cr), CENAREC, referentes internacionales de EdTech

---

## Tabla de Contenidos

1. [El Sistema Educativo Costarricense](#1-el-sistema-educativo-costarricense)
2. [El MEP: Ministerio de Educación Pública](#2-el-mep-ministerio-de-educación-pública)
3. [El Planeamiento Didáctico en Costa Rica](#3-el-planeamiento-didáctico-en-costa-rica)
4. [Los Programas de Estudio de Artes Plásticas](#4-los-programas-de-estudio-de-artes-plásticas)
   - 4.1 [I y II Ciclo (Primaria: 1º a 6º año)](#41-i-y-ii-ciclo-primaria-1º-a-6º-año)
   - 4.2 [III Ciclo y Diversificada (Secundaria: 7º a 11º año)](#42-iii-ciclo-y-diversificada-secundaria-7º-a-11º-año)
   - 4.3 [Estructura por Grado (7mo año - caso de uso principal)](#43-estructura-por-grado-7mo-año---caso-de-uso-principal)
5. [Evaluación en el Sistema MEP](#5-evaluación-en-el-sistema-mep)
6. [Adecuaciones Curriculares](#6-adecuaciones-curriculares)
7. [El Rol del Docente y sus Necesidades Reales](#7-el-rol-del-docente-y-sus-necesidades-reales)
8. [Referentes Tecnológicos: Apps Educativas con IA](#8-referentes-tecnológicos-apps-educativas-con-ia)
9. [Análisis del Proyecto Propuesto](#9-análisis-del-proyecto-propuesto)
10. [Conceptos Clave del Dominio (Glosario Técnico)](#10-conceptos-clave-del-dominio-glosario-técnico)
11. [Conclusiones e Implicaciones para el Desarrollo](#11-conclusiones-e-implicaciones-para-el-desarrollo)

---

## 1. El Sistema Educativo Costarricense

### Estructura General

El sistema educativo de Costa Rica se divide en **cuatro niveles**:

| Nivel | Nombre | Rango de edad | Obligatorio |
|---|---|---|---|
| 1 | Preescolar (Educación Inicial) | 0–6 años | Sí (transición) |
| 2 | Primaria (I y II Ciclo de la EGB) | 6–12 años | Sí |
| 3 | Secundaria (III Ciclo + Educación Diversificada) | 12–17/18 años | Parcialmente |
| 4 | Universitaria | 17/18+ años | No |

### Ciclos del Sistema Formal

- **I Ciclo:** 1°, 2° y 3° año de primaria  
- **II Ciclo:** 4°, 5° y 6° año de primaria  
- **III Ciclo (EGB):** 7°, 8° y 9° año (secundaria básica)  
- **Ciclo Diversificado:** 10° y 11° año (o 10°, 11° y 12° en colegios técnicos)

### Modalidades en Secundaria

- **Educación Académica:** 2 años del ciclo diversificado, orientación científico-humanista
- **Educación Técnica Profesional:** 3 años del ciclo diversificado, especialidades como Diseño Gráfico, Informática, Agronomía, entre otras
- **Bachillerato Internacional (BI):** presente en algunos centros educativos privados y públicos
- Otras modalidades: artística, ambiental, tecnológica, científica, etc.

### Sistema de Calificación

- Escala del **1 al 100**
- Nota mínima para aprobar en primaria y secundaria básica: **65**
- Nota mínima en educación diversificada: **70**

### Regencia Nacional

El ente rector es el **Ministerio de Educación Pública (MEP)**, que define programas, calendarios, plantillas de planeamiento, evaluación y normativas docentes.

---

## 2. El MEP: Ministerio de Educación Pública

### Qué es

El Ministerio de Educación Pública es la entidad estatal costarricense encargada de:
- Definir los **programas de estudio oficiales** para todas las asignaturas
- Establecer los **lineamientos de evaluación** (Reglamento de Evaluación de los Aprendizajes - REAC)
- Regular el **calendario escolar** (habitualmente 200 días lectivos)
- Orientar las **políticas curriculares** y pedagógicas
- Emitir las **plantillas institucionales de planeamiento**
- Capacitar al cuerpo docente a través del **Instituto de Desarrollo Profesional Uladislao Gámez Solano (IDP)**

### Marco Filosófico Educativo

El MEP fundamenta su propuesta curricular en tres pilares filosóficos:

1. **Humanismo:** busca la plena realización del ser humano como persona con dignidad y valor
2. **Racionalismo:** reconoce la capacidad racional del ser humano para construir conocimiento
3. **Constructivismo:** el aprendizaje se construye activamente, basado en experiencias previas

Adicionalmente, en 2008 se incorporó el eje transversal **"Ética, Estética y Ciudadanía"**, que permea las asignaturas de Artes Plásticas, Artes Musicales, Educación Cívica, Educación Física, Artes Industriales y Educación para el Hogar.

### Ejes Transversales del Currículo Nacional

El MEP define los siguientes ejes transversales que deben cruzar todos los programas:

- Educación ambiental para el desarrollo sostenible
- Educación para la salud
- Educación para la expresión integral de la sexualidad humana
- Educación para los derechos humanos y la paz
- Valores éticos, estéticos y ciudadanos

### Calendario Escolar

Costa Rica opera en año lectivo de febrero a diciembre (aproximadamente), con tres trimestres:
- **I Trimestre:** febrero–abril
- **II Trimestre:** mayo–julio
- **III Trimestre:** agosto–noviembre

Existen dos años de transición en los niveles (algunas instituciones trabajan semestres), por lo que la app debe manejar con flexibilidad períodos: **semanal, mensual, trimestral, semestral y anual**.

---

## 3. El Planeamiento Didáctico en Costa Rica

### ¿Qué es el Planeamiento Didáctico?

El **planeamiento didáctico** es el proceso mediante el cual un docente organiza y estructura con antelación las experiencias de aprendizaje que llevará al aula. No es un simple cronograma: es un documento técnico-pedagógico que refleja la intencionalidad educativa, las estrategias de mediación y los criterios de evaluación.

En Costa Rica, el MEP ha definido una **plantilla oficial** que los docentes deben completar para cada período lectivo. Aunque existen variaciones institucionales, la estructura general incluye los siguientes elementos:

### Datos Generales del Planeamiento

| Campo | Descripción |
|---|---|
| Nombre del docente | Identificación del profesor(a) responsable |
| Institución | Centro educativo (nombre y circuito) |
| Asignatura | Materia que se planea |
| Nivel / Grado | Año o nivel educativo (ej. 7° año) |
| Sección | Grupo de estudiantes (ej. 7-3) |
| Año lectivo | El año en curso |
| Período | Semanal, mensual, trimestral, semestral o anual |
| Cantidad de lecciones | Lecciones semanales asignadas (ej. 2 lecciones de 40 min.) |
| Fechas | Fechas de inicio y cierre del período planificado |

### Componentes del Planeamiento (Cuerpo Técnico)

Un planeamiento MEP completo contiene los siguientes elementos estructurales:

#### 1. Tema o Título de la Unidad
Nombre descriptivo de la unidad de aprendizaje, derivado del programa oficial de la asignatura.

#### 2. Propósito u Objetivo General
Redacción amplia que describe qué se espera que el estudiante aprenda o logre durante el período. En los programas de Artes Plásticas se denomina también **"intención pedagógica"**.

#### 3. Aprendizajes Esperados (o "por lograr")
Son los **resultados de aprendizaje concretos** que el estudiante debe alcanzar. En los programas del MEP de 2013 se expresan como verbos de acción medibles:

> Ejemplo (7° año, Artes Plásticas, II Trimestre):
> - "Desarrollo de la capacidad de observación y percepción del mundo visual."
> - "Valoración de la cultura visual a través de nuestra historia."
> - "Aplicación de los elementos plásticos que simulan la percepción de movimientos."

#### 4. Contenidos Curriculares (en tres dimensiones)

| Dimensión | Descripción | Ejemplo |
|---|---|---|
| **Conceptual** | Conceptos, teorías, definiciones que el estudiante debe conocer | "El individuo como generador de expresiones plásticas", "El retrato y sus características" |
| **Procedimental** | Habilidades, procesos, técnicas que el estudiante debe ejecutar | "Reproducción de la figura humana mediante diversas técnicas", "Identificación de la estética y emociones en el retrato" |
| **Actitudinal** | Valores, actitudes, comportamientos que se busca desarrollar | "Aprecio por la persona como ser integral", "Valoración de las diferencias en los arquetipos de belleza" |

#### 5. Valores, Actitudes y Comportamientos Éticos, Estéticos y Ciudadanos
Sección específica que indica cuáles valores del eje transversal se trabajan en la unidad (libertad, respeto, cooperación, disfrute de la diversidad, etc.)

#### 6. Estrategias de Mediación y Aprendizaje
Actividades, técnicas y metodologías concretas que el docente utilizará para guiar el proceso. En los programas MEP estas siguen una secuencia:
- **Discusión inicial / Activación de conocimientos previos**
- **Actividades de desarrollo** (ejercicios prácticos, proyectos, talleres)
- **Conclusión** (montaje de exposición, reflexión crítica, portafolio)

#### 7. Recursos y Materiales
Lista de materiales necesarios para las clases: papel, lápices, pinturas, materiales reciclables, recursos digitales, libros de referencia, etc.

#### 8. Estrategias de Evaluación
Técnicas e instrumentos que se usarán para medir el aprendizaje:
- Rúbricas
- Listas de cotejo
- Portafolios
- Coevaluación / Autoevaluación
- Evaluaciones formativas continuas
- Proyectos finales

#### 9. Evidencias de Aprendizaje
Productos concretos que demuestran el logro de los aprendizajes (ej. una obra artística, un portafolio, una exposición, una rúbrica completada).

#### 10. Instrumentos de Evaluación
Los documentos formales usados para evaluar: rúbricas, escala de calificación, lista de cotejo, etc.

#### 11. Cronograma de Lecciones
Distribución de los contenidos y actividades en el tiempo disponible, considerando el número de lecciones semanales y el calendario escolar.

### Importante: La Plantilla Institucional

Cada institución puede tener variantes sobre la plantilla base del MEP. Por eso el proyecto prevé que el docente pueda **subir su plantilla institucional** para que la app la respete.

---

## 4. Los Programas de Estudio de Artes Plásticas

### Contexto de la Reforma

En 2013, el MEP publicó nuevos programas de Artes Plásticas bajo el marco de **"Ética, Estética y Ciudadanía"**, reformando completamente los currículos anteriores que se centraban demasiado en la labor técnica sin contexto estético ni ciudadano.

La reforma se basó en:
- Teorías cognitivas del desarrollo (Piaget, Parsons, Gardner - Inteligencias Múltiples)
- Semiótica y análisis del lenguaje visual
- Enfoque constructivista y socio-reconstructivista
- Arte como herramienta de formación ética, estética y ciudadana

### 4.1 I y II Ciclo (Primaria: 1° a 6° año)

#### Estructura Programática

El currículo de primaria está organizado en:
- **Eje Horizontal:** tres grandes temas que se trabajan en cada trimestre
  - **I Trimestre:** Identidad (lo individual/intrapersonal)
  - **II Trimestre:** Naturaleza (relación con el entorno)
  - **III Trimestre:** Sociedad (lo colectivo/interpersonal)

- **Eje Vertical:** progresión cognitiva por grado

#### Matriz Temática por Grado (Primaria)

| Grado | Tema Central | Trimestre 1 (Identidad) | Trimestre 2 (Naturaleza) | Trimestre 3 (Sociedad) |
|---|---|---|---|---|
| **1° año** | Favoritismo | La identidad en el descubrimiento de mis favoritos | La naturaleza y su relación con el arte | Reconociendo el arte en mi comunidad |
| **2° año** | Belleza | El concepto de belleza y la identidad | La belleza en la naturaleza | La sociedad y el concepto de belleza |
| **3° año** | Expresión | La expresión de la identidad | La naturaleza en la expresión | Los contextos sociales y la expresión |
| **4° año** | Lenguaje visual | El lenguaje estético en la identidad artística | Las formas del arte y la naturaleza | El lenguaje del arte: forma, estilo, contexto y técnica |
| **5° año** | Significados | La expresión personal a partir de la naturaleza | La sociedad y la subjetividad | Valoración y juicio personal en la creación |
| **6° año** | Integración | Integración: identidad, salud, afectividad | La sostenibilidad y la naturaleza en la creación | Integración: identidad cultural y reflexión en la expresión |

#### Sintaxis Visual en Primaria

La sintaxis visual es la "gramática" del lenguaje plástico. En primaria se trabaja de forma gradual:

| Grado | Color | Forma | Composición | Diseño/Técnicas |
|---|---|---|---|---|
| 1° | Colores primarios, secundarios, cálidos/fríos, intensidad | Punto, figura-fondo | Figura-fondo | Sesgado, bodoques, títeres |
| 2° | Primarios, secundarios, complementarios, terciarios | Línea | Simetría y asimetría | Collage, puntillismo, mancha |
| 3° | Matiz, armonías (análogos, monocromía) | Planos | Balance y proporción | Impresiones, ilustración de cuentos |
| 4° | Armonías complementarias, triadas | Volumen y textura | Ritmo y armonía | Fotografía/imagen impresa, mascaras, cartel |
| 5° | Tono, luz, sombra, psicología del color | Espacio y perspectiva | Diseño y proporción | Artesanía, cartel, paisaje |
| 6° | Interpretación, alfabetidad visual | Sintaxis formal | Expresión y comunicación visual | Expresión bi/tridimensional, escultura |

#### Historia del Arte en Primaria

La historia del arte se trabaja desde primaria como parte del contenido curricular, organizando el conocimiento en tres grandes períodos históricos:
- **Arte Premoderno** (antigüedad hasta revolución industrial)
- **Arte Moderno** (siglo XIX y XX)
- **Arte Contemporáneo** (a partir del siglo XX tardío/XXI)

#### Evaluación en Primaria (Artes Plásticas)

- Enfocada en el **proceso**, no solo en el resultado final
- Carácter **formativo y cualitativo**
- Se valoran: la experiencia vivida, la participación activa, los productos artísticos creados
- Herramientas: **portafolios, rúbricas, coevaluación, observación directa**
- Se prioriza el disfrute y la apreciación sobre la destreza técnica

---

### 4.2 III Ciclo y Diversificada (Secundaria: 7° a 11° año)

#### Eje Filosófico Central

El programa de secundaria se organiza en torno a **tres paradigmas artísticos históricos** que funcionan como "ejes verticales":

| Eje Vertical | Paradigma | Años | Descripción |
|---|---|---|---|
| **Pre-moderno** | Observación, mímesis y representación | **7° año** | Arte que imita la realidad: retrato, paisaje, bodegón. Técnicas figurativas. |
| **Moderno** | Abstracción, estilización y fundamentos del diseño | **8° año** | Arte que rompe con la representación literal. Abstracción, vanguardias. |
| **Contemporáneo** | Conceptualización, proceso y desmaterialización | **9° año** | Arte conceptual. Instalaciones, arte digital, performance. Ruptura del objeto. |
| **Integración** | Pre-moderno + Moderno + Contemporáneo | **10° y 11°** | Arte costarricense en relación con los tres paradigmas. |

#### Eje Horizontal en Secundaria

Los tres temas del eje horizontal (también llamados "inspiraciones artísticas") se mantienen para todos los grados de secundaria:

1. **La Naturaleza:** el entorno natural como punto de partida artístico
2. **La Imagen Humana:** el ser humano, el retrato, la figura humana, la identidad
3. **La Comunidad y los Objetos Cotidianos:** cultura material, identidad colectiva, lo social

Cada trimestre se aborda uno de estos tres temas, pero desde el paradigma del año correspondiente.

#### Historia del Arte en Secundaria

| Año | Eje Vertical | Arte Universal | Arte Costarricense |
|---|---|---|---|
| 7° | Premoderno | Arte antiguo y premoderno | Arte precolombino, colonial, republicano, inicios siglo XX |
| 8° | Moderno | Arte moderno | Arte costarricense moderno: siglo XX hasta 1980 |
| 9° | Contemporáneo | Arte contemporáneo | Arte costarricense: 1980 hasta presente |
| 10° | Integración | Claves universales antiguo + premoderno | Arte costarricense siglo XX |
| 11° | Integración | Claves universales moderno + contemporáneo | Arte costarricense contemporáneo |

---

### 4.3 Estructura por Grado (7mo año - caso de uso principal)

El requerimiento menciona explícitamente **7mo año de Artes Plásticas**, que corresponde al **paradigma Pre-moderno** con técnicas figurativas (mímesis y representación).

#### Unidades de 7° año

**UNIDAD 1 (I Trimestre): La naturaleza como inspiración artística figurativa**
- Observación y representación de la naturaleza
- Técnicas: dibujo de contorno, perspectiva, composición
- Contenido conceptual: arte premoderno, paisajismo, naturaleza muerta (bodegón)
- Historia del arte: paisajistas, artistas naturalistas

**UNIDAD 2 (II Trimestre): La imagen humana como inspiración artística figurativa**
- El retrato: características físicas y emocionales
- Técnicas: dibujo (grafito, sanguina, carbón, tinta china, marcadores), pintura, escultura, collage
- Contenido conceptual: proporción, unidad, simetría, cánones de belleza
- Historia del arte: escultura griega y romana, pinturas de Miguel Ángel, Rembrandt, artistas latinoamericanos y costarricenses
- Valores: respeto a la diversidad, superación de estereotipos, valoración del otro

**UNIDAD 3 (III Trimestre): La comunidad y los objetos cotidianos como inspiración artística figurativa**
- La cultura material, lo cotidiano como arte
- Técnicas: dibujo figurativo (línea, plano, volumen, luz y sombra), pintura, escultura (modelado, alto relieve)
- Contenido conceptual: relativismo cultural, cultura material, espacio físico y comunidad
- Historia del arte: naturalezas muertas, arte costumbrista, cultura visual

#### Perfil de Salida del Estudiante de Artes Plásticas (Secundaria Completa)

Al finalizar el ciclo diversificado, el estudiante podrá:
- Disfrutar, apreciar y comprender manifestaciones artísticas
- Expresarse mediante técnicas artísticas
- Distinguir y contextualizar el arte premoderno, moderno y contemporáneo
- Reconocer exponentes de la plástica nacional e internacional
- Respetar la diversidad cultural y artística
- Comunicarse a través del lenguaje visual

---

## 5. Evaluación en el Sistema MEP

### Marco Legal: REAC (Reglamento de Evaluación de los Aprendizajes)

En 2026, el MEP publicó un nuevo Reglamento de Evaluación de los Aprendizajes. La evaluación en Costa Rica es:
- **Diagnóstica:** al inicio para conocer el punto de partida
- **Formativa:** continua, durante el proceso
- **Sumativa:** al final de cada período para asignar notas

### Instrumentos de Evaluación Más Usados

| Instrumento | Descripción | Uso típico |
|---|---|---|
| **Rúbrica** | Tabla con criterios y niveles de desempeño descriptivos | Evaluación de proyectos y producciones artísticas |
| **Lista de cotejo** | Registro de presencia/ausencia de criterios específicos | Verificación de procesos o técnicas |
| **Portafolio** | Colección organizada de trabajos del estudiante | Evidencia del proceso de aprendizaje |
| **Coevaluación** | Los estudiantes se evalúan entre sí | Fomenta pensamiento crítico y criterio estético |
| **Autoevaluación** | El propio estudiante valora su trabajo | Reflexión metacognitiva |
| **Escala de calificación** | Escala numérica con descriptores cualitativos | Cuantificar el desempeño |

### Evaluación en Artes Plásticas (Particularidades)

- Se evalúa principalmente el **proceso**, no solo el producto terminado
- El **disfrute y la participación** son criterios de valoración legítimos
- No existe un único estándar de "bello": se valoran diversas expresiones
- Las obras no deben desecharse; se valora el esfuerzo y la calidad relativa
- Se fomenta la reflexión sobre el propio proceso creativo

---

## 6. Adecuaciones Curriculares

### ¿Qué son?

Las **adecuaciones curriculares** son ajustes que se realizan al currículo oficial para atender a estudiantes con necesidades educativas especiales (NEE), ya sean de origen cognitivo, sensorial, físico, social o emocional. En Costa Rica, el marco legal es la **Ley 7600** (Igualdad de Oportunidades para Personas con Discapacidad, 1996).

### Marco Institucional

El sistema de soporte para adecuaciones curriculares en Costa Rica incluye:

- **Comité de Apoyo Educativo (CAE):** equipo interdisciplinario en cada institución (incluye orientador, docente de apoyo, dirección) que gestiona el proceso
- **CENAREC (Centro Nacional de Recursos para la Educación Inclusiva):** institución del MEP que apoya la educación de personas con discapacidad
- **CIAD-MEP:** Centro de Investigación y Adaptaciones Didácticas

### Tipos de Adecuaciones Curriculares

#### 1. Adecuación No Significativa (ANS)

- **Concepto:** Ajustes en el proceso de enseñanza que **no modifican** los aprendizajes esperados del programa oficial
- **Qué cambia:** La forma de presentar los contenidos, los tiempos, los materiales, el ambiente
- **Qué no cambia:** Los contenidos, los objetivos, la evaluación sumativa es la misma
- **Casos típicos:** Estudiantes con dificultades de aprendizaje leves, problemas de atención, dificultades sensoriales manejables, problemas emocionales transitorios

**Estrategias típicas para ANS:**
- Ampliar el tiempo para actividades y evaluaciones
- Ofrecer instrucciones más claras y visuales
- Sentar al estudiante en posición estratégica
- Utilizar apoyos visuales adicionales
- Dividir las tareas en partes más pequeñas
- Permitir evaluaciones orales en vez de escritas
- Ofrecer textos con letra más grande

#### 2. Adecuación Significativa (AS)

- **Concepto:** Ajustes que **sí modifican** los contenidos, los aprendizajes esperados y la forma de evaluar
- **Qué cambia:** Los objetivos, los contenidos, los criterios de evaluación, el nivel de exigencia
- **Qué no cambia:** La pertenencia al grupo, la socialización, la dignidad del estudiante
- **Casos típicos:** Estudiantes con discapacidad intelectual, autismo severo, síndromes específicos, parálisis cerebral, etc.
- **Requiere:** Diagnóstico formal, registro en SIMED (sistema del MEP), Plan Individual

**Estrategias típicas para AS:**
- Reducción de contenidos al mínimo funcional
- Objetivos más simples y concretos
- Actividades guiadas paso a paso
- Evaluación con criterios propios del estudiante
- Materiales adaptados (pictogramas, materiales manipulativos)
- Mayor apoyo del docente durante la clase
- Coordinación con el docente de apoyo o especialista
- Plan Individual (PI) personalizado

### Proceso de Implementación

1. **Identificación:** El docente, la familia o un especialista detecta la necesidad
2. **Evaluación diagnóstica:** El CAE reúne información sobre el estudiante
3. **Elaboración del Plan:** Se define el tipo de adecuación, los ajustes específicos, las metas
4. **Registro:** Se documenta en el sistema SIMED del MEP
5. **Implementación:** El docente aplica la adecuación en el aula
6. **Seguimiento:** El CAE revisa el progreso periódicamente

### Información que un Docente Necesita para Planear con Adecuaciones

Para adaptar correctamente un planeamiento, el docente necesita saber:
- Tipo de adecuación (AS o ANS)
- Diagnóstico o características educativas del estudiante
- Nivel de funcionamiento académico actual
- Dificultades específicas observadas en el aula
- Fortalezas y áreas de interés del estudiante
- Recomendaciones del CAE o especialistas externos
- Contexto familiar relevante

---

## 7. El Rol del Docente y sus Necesidades Reales

### El Docente de Artes Plásticas en Costa Rica

Un docente de Artes Plásticas en secundaria costarricense:
- Atiende múltiples grupos (puede tener entre 8 y 15 secciones distintas)
- Cada sección tiene entre 30 y 40 estudiantes
- Tiene **2 lecciones de 40 minutos por semana** con cada grupo
- Debe hacer un **planeamiento por período** (generalmente trimestral)
- Debe documentar adecuaciones curriculares por cada estudiante que las tenga
- Asiste a reuniones del CAE cuando hay estudiantes con NEE
- Debe rendir informes a la dirección del colegio

### El Problema del Planeamiento

El planeamiento es una de las tareas más **demandantes en tiempo** para los docentes costarricenses. Los problemas más comunes:

1. **Falta de tiempo:** Los docentes tienen muchos grupos y poco tiempo de preparación
2. **Desconocimiento del programa:** Especialmente los docentes nuevos o los que cambian de nivel
3. **Dificultad para contextualizar:** Adaptar los contenidos del programa a la realidad del aula
4. **Complejidad de la plantilla:** La plantilla institucional puede variar de colegio en colegio
5. **Integración de adecuaciones:** Adaptar el planeamiento para estudiantes con NEE es complejo
6. **Falta de ejemplos concretos:** El programa indica qué enseñar, pero no cómo hacerlo en detalle

### Lo que el Docente Necesita

- Un punto de partida sólido: no quiere una hoja en blanco
- Lenguaje técnico correcto (aprendizajes esperados, indicadores, mediación)
- Actividades concretas y aplicables en el aula real
- Sugerencias de materiales accesibles (no todos los colegios tienen recursos)
- Instrumentos de evaluación listos para usar o adaptar
- Ejemplos de cómo ejecutar la clase paso a paso
- Orientación sobre cómo adaptar para estudiantes con NEE

---

## 8. Referentes Tecnológicos: Apps Educativas con IA

### Panorama Internacional

La intersección entre IA y planeamiento educativo es un campo en crecimiento acelerado. Existen varios referentes relevantes:

### MagicSchool.ai (EE.UU.)

- **Descripción:** Plataforma #1 de IA para docentes en EE.UU. Tiene más de 80 herramientas para profesores
- **Funcionalidades clave:** Generador de planes de lección, generador de rúbricas, generador de IEP (Individualized Education Plan), retroalimentación de escritura, generador de presentaciones
- **Fortaleza relevante:** Tiene un módulo específico para **IEP Generator** (equivalente al plan de adecuaciones curriculares), que es directamente análogo a la función de adecuaciones del proyecto
- **Debilidad para el contexto costarricense:** No está contextualizado para el sistema MEP, no conoce los programas oficiales costarricenses, está en inglés
- **Estadística:** Ahorra un promedio de **7-10 horas semanales** por docente
- **Impacto:** 88% de docentes reporta que les ayuda a llegar a todos sus estudiantes

### LessonPlans.ai

- **Descripción:** Generador de planes de lección basado en IA, desarrollado por docentes
- **Funcionalidades:** Generación rápida de planes, personalización, reutilización
- **Debilidad:** No contextualizado para sistemas educativos específicos latinoamericanos

### Education.com (EE.UU.)

- **Descripción:** Biblioteca masiva de recursos educativos con planes de lección para K-8
- **Debilidad:** No es IA generativa; no se adapta al currículo MEP

### Lo que Diferencia este Proyecto

Lo que hace **único** este proyecto es:

| Aspecto | Apps Genéricas | Este Proyecto |
|---|---|---|
| **Contexto curricular** | Genérico | Basado en programas **oficiales del MEP** |
| **Idioma y terminología** | Inglés / genérico en español | Terminología exacta del MEP costarricense |
| **Plantilla de salida** | Formato propio | Plantilla **institucional del docente** |
| **Módulo de adecuaciones** | Genérico (IEP en inglés) | Adecuaciones MEP (AS y ANS) con marco legal costarricense |
| **Asignatura** | Cualquier materia | Comienza con **Artes Plásticas** (y puede expandirse) |
| **Calendario** | No considera calendarios | Integra el **calendario escolar costarricense** |

---

## 9. Análisis del Proyecto Propuesto

### Qué es el Proyecto

Una **aplicación web/móvil de asistencia pedagógica** basada en IA generativa (tipo LLM), capaz de:

1. Recibir parámetros básicos de un docente (asignatura, nivel, período, cantidad de lecciones)
2. Consultar o integrar el programa oficial del MEP correspondiente
3. Generar automáticamente un **planeamiento didáctico completo** usando la plantilla oficial o la del docente
4. Adaptar el planeamiento para estudiantes con **adecuaciones curriculares** (AS o ANS)

### Módulos Principales

#### Módulo 1: Generador de Planeamiento

**Entrada (inputs):**
- Nombre del docente
- Institución
- Año lectivo
- Asignatura (ej. Artes Plásticas)
- Nivel/Grado (ej. 7° año)
- Sección
- Período (semanal / mensual / trimestral / semestral / anual)
- Cantidad de lecciones por semana
- Fechas del calendario (inicio–fin del período)
- Plantilla institucional (opcional, upload de archivo)

**Salida (outputs):**
- Planeamiento mensual completo
- Desarrollo por semana
- Desarrollo por clase (cada lección)
- Materiales necesarios
- Explicación teórica breve por tema
- Actividades paso a paso
- Evaluación sugerida (rúbricas, listas de cotejo)
- Anexos imprimibles

#### Módulo 2: Adecuaciones Curriculares

**Entrada (inputs):**
- Tipo de adecuación (significativa / no significativa)
- Diagnóstico o características educativas
- Nivel de funcionamiento académico
- Dificultades observadas
- Fortalezas del estudiante
- Recomendaciones del CAE o especialistas

**Salida (outputs):**
- Recomendaciones pedagógicas para el aula
- Ajustes metodológicos al planeamiento
- Ajustes en la evaluación
- Material adaptado
- Actividades con menor nivel de complejidad (en AS)
- Objetivos ajustados (en AS)
- Plan individual específico
- Estrategias de apoyo en el aula

### Flujo de Usuario (User Journey)

```
Docente entra a la app
    ↓
Selecciona asignatura + nivel + período
    ↓
App carga programa oficial MEP correspondiente
    ↓
App presenta propuesta de planeamiento (basado en el programa)
    ↓
Docente ajusta o aprueba → Planeamiento generado
    ↓
[Opcional] Docente añade información de estudiante con NEE
    ↓
App adapta el planeamiento con adecuaciones
    ↓
Docente descarga / exporta el planeamiento
```

### Retos Técnicos del Dominio

1. **Base de conocimiento curricular:** La app necesita conocer todos los programas de estudio del MEP por asignatura y nivel. Hay que estructurar estos datos
2. **Generación fiel al programa:** La IA debe generar aprendizajes esperados que coincidan con los del programa oficial (no inventar contenidos)
3. **Variabilidad de plantillas:** Cada institución tiene ligeras variaciones en su plantilla. La app debe ser flexible
4. **Lenguaje técnico correcto:** La terminología debe ser la del MEP (aprendizajes esperados, contenidos conceptuales/procedimentales/actitudinales, mediación, etc.)
5. **Calendario escolar:** Debe calcular correctamente el número de lecciones disponibles en un período dado
6. **Adecuaciones personalizadas:** Cada estudiante con NEE es único; la app debe generar propuestas personalizadas, no genéricas

---

## 10. Conceptos Clave del Dominio (Glosario Técnico)

Para desarrollar la aplicación, el equipo debe dominar estos términos del contexto MEP:

| Término | Definición en contexto MEP |
|---|---|
| **Aprendizajes esperados** | Resultados concretos de aprendizaje redactados como logros del estudiante, derivados del programa oficial |
| **Mediación pedagógica** | La intervención del docente para facilitar el aprendizaje; incluye estrategias, actividades y recursos |
| **Contenidos conceptuales** | Conceptos, hechos y principios que el estudiante debe conocer |
| **Contenidos procedimentales** | Habilidades, destrezas y procesos que el estudiante debe ejecutar |
| **Contenidos actitudinales** | Valores, normas y actitudes que el estudiante debe desarrollar |
| **Ejes transversales** | Temas que cruzan horizontalmente todo el currículo (salud, ambiente, ciudadanía, etc.) |
| **Planeamiento didáctico** | Documento técnico-pedagógico que organiza la práctica docente para un período |
| **Lección** | Una unidad de tiempo de clase (40 minutos en secundaria) |
| **Período lectivo** | El tramo de tiempo que se planifica (trimestre, semestre, etc.) |
| **REAC** | Reglamento de Evaluación de los Aprendizajes; norma la evaluación en el sistema MEP |
| **CAE** | Comité de Apoyo Educativo; equipo institucional para atender a estudiantes con NEE |
| **CENAREC** | Centro Nacional de Recursos para la Educación Inclusiva |
| **SIMED** | Sistema de Información para el Mejoramiento y la Evaluación de la Calidad de la Educación; registros administrativos del MEP |
| **Adecuación significativa (AS)** | Modificación de los contenidos y objetivos curriculares para un estudiante con NEE grave |
| **Adecuación no significativa (ANS)** | Ajuste en la metodología sin modificar los contenidos ni objetivos curriculares |
| **Plan Individual (PI)** | Documento personalizado para estudiantes con adecuación significativa |
| **Indicadores de evaluación** | Criterios específicos que permiten verificar si se logró un aprendizaje esperado |
| **Rúbrica** | Instrumento de evaluación con criterios y niveles de desempeño descritos cualitativamente |
| **Lista de cotejo** | Instrumento que registra la presencia o ausencia de características o comportamientos |
| **Portafolio** | Colección organizada de evidencias del aprendizaje del estudiante |
| **Sintaxis visual** | La "gramática" del lenguaje plástico: punto, línea, plano, color, composición, textura |
| **Mímesis** | Imitación de la naturaleza como base del arte premoderno; eje del 7° año |
| **Retrato** | Representación visual de una persona; tema central del II trimestre de 7° año |
| **Arte premoderno** | Paradigma artístico anterior al siglo XIX; base del currículo de 7° año |
| **Arte moderno** | Paradigma artístico del siglo XIX–XX; base del currículo de 8° año |
| **Arte contemporáneo** | Paradigma artístico del siglo XX tardío al presente; base del 9° año |
| **Disfrute artístico** | El placer y gozo estético como objetivo educativo legítimo y prioritario |

---

## 11. Conclusiones e Implicaciones para el Desarrollo

### Lo que el Equipo Necesita Entender

1. **El planeamiento no es solo llenar un formulario.** Es un documento técnico pedagógico que requiere dominio del programa oficial. La mayoría de los docentes dedica horas a esto, y muchos docentes nuevos no saben bien cómo hacerlo.

2. **El programa de Artes Plásticas es mucho más rico de lo que parece.** No es solo "pintar en clase". Es un sistema filosófico, histórico, técnico y ético que requiere que la app genere contenido fundamentado en conceptos como mímesis, sintaxis visual, arte premoderno/moderno/contemporáneo, identidad, ciudadanía y valoración de la diversidad.

3. **Las adecuaciones curriculares son un tema delicado y técnico.** La diferencia entre AS y ANS es crucial; modificar incorrectamente los contenidos puede tener consecuencias legales y administrativas. La app debe orientar correctamente.

4. **El docente costarricense trabaja con terminología específica del MEP.** Palabras como "aprendizajes esperados", "mediación", "contenidos conceptuales/procedimentales/actitudinales" no son intercambiables con "objetivos de aprendizaje", "actividades" o "destrezas" tal como las usaría una app genérica en inglés.

5. **La variabilidad institucional es real.** La plantilla oficial del MEP existe, pero cada institución la adapta ligeramente. El docente debe poder subir su plantilla para que la app respete el formato.

6. **El calendario escolar determina todo.** La cantidad de lecciones disponibles en un período depende del calendario escolar oficial y los días no lectivos de cada institución. Sin este dato, los planeamientos no serían realistas.

### Estrategias de Implementación Sugeridas

1. **Comenzar con Artes Plásticas.** Tiene programas bien documentados, lenguaje claro, y es una asignatura donde el docente especialmente necesita ayuda con la parte teórica.

2. **Estructurar el conocimiento del MEP en una base de datos.** Los programas de estudio deben parsearse y estructurarse: unidades por grado y trimestre, aprendizajes esperados, contenidos en las tres dimensiones.

3. **Construir un motor de planeamiento,** no solo un chatbot. La app debe ser capaz de calcular el número de lecciones disponibles, distribuir los contenidos en el tiempo, y generar el documento con la estructura correcta.

4. **El módulo de adecuaciones debe preguntar bien.** La calidad del output dependerá de la calidad de la información que el docente ingrese sobre el estudiante.

5. **Diseñar para la usabilidad.** Un docente no tiene tiempo para aprender interfaces complejas. La app debe ser directa, rápida y clara. El flujo ideal es: pocas preguntas → planeamiento generado.

6. **Exportación en formatos estándar.** Los docentes necesitan documentos en Word o PDF que puedan imprimir o enviar a la dirección del colegio.

### Alcance Inicial Recomendado

Para una primera versión funcional (MVP), se recomienda:

- **Asignatura:** Artes Plásticas
- **Nivel:** III Ciclo (7°, 8°, 9°) como prioridad
- **Período:** Trimestral (el más solicitado)
- **Output:** Planeamiento trimestral completo con desarrollo semanal
- **Adecuaciones:** Soporte básico para AS y ANS (recomendaciones generales)

En versiones posteriores se puede expandir a:
- Otras asignaturas del eje de Ética, Estética y Ciudadanía
- Más asignaturas del currículo MEP
- Módulo de adecuaciones más detallado con PI completo
- Integración con calendario escolar oficial
- Biblioteca de planeamientos guardados

---

## Referencias

### Fuentes Oficiales

- **MEP (2013).** *Programa de Estudio de Artes Plásticas, I y II Ciclos de la Educación General Básica.* San José, Costa Rica. (Archivo: `aplasticas1y2ciclo.pdf`)
- **MEP (2008/2013).** *Programas de Estudio de Artes Plásticas, Tercer Ciclo de la Educación General Básica y Educación Diversificada.* San José, Costa Rica. (Archivo: `artesplasticas3cicloydiversificada.pdf`)
- **Garnier, Leonardo.** "Ética, estética y ciudadanía: educar para la vida." Prólogo en ambos programas de estudio.
- **MEP (2026).** *Reglamento de Evaluación de los Aprendizajes (REAC).* Gaceta Nº 39, Alcance Nº 20.
- **CENAREC.** Centro Nacional de Recursos para la Educación Inclusiva. [https://www.cenarec.go.cr](https://www.cenarec.go.cr)

### Referentes Tecnológicos

- **MagicSchool.ai.** [https://www.magicschool.ai](https://www.magicschool.ai) - Plataforma #1 de IA para docentes
- **LessonPlans.ai.** [https://www.lessonplans.ai](https://www.lessonplans.ai) - Generador de planes de lección con IA

### Fuentes Informativas

- **Wikipedia.** *Educación en Costa Rica.* [https://es.wikipedia.org/wiki/Educación_en_Costa_Rica](https://es.wikipedia.org/wiki/Educaci%C3%B3n_en_Costa_Rica)
- **Wikipedia.** *Artes plásticas.* [https://es.wikipedia.org/wiki/Artes_plásticas](https://es.wikipedia.org/wiki/Artes_pl%C3%A1sticas)
- **MEP.** Sitio web oficial. [https://www.mep.go.cr](https://www.mep.go.cr)

---

*Documento elaborado con base en análisis profundo de los programas oficiales del MEP y fuentes secundarias. Fecha: Mayo 2026.*
