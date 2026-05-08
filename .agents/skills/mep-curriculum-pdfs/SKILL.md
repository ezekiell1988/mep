# Skill: mep-curriculum-pdfs

> Cómo localizar, descargar y subir al API los PDFs de programas de estudio del MEP de Costa Rica.
> Usar cuando se necesite agregar una nueva asignatura al sistema de curriculum de AulaIA.
>
> **Flujo de trabajo:** Solo el admin (Ezequiel) sube PDFs al sistema, siempre desde esta máquina usando el agente con este skill. Los docentes nunca tocan el endpoint de curriculum.

---

## 1. Contexto

El MEP publica los programas de estudio oficiales en `mep.go.cr`. No hay una API pública — los PDFs se sirven como archivos estáticos bajo `/sites/default/files/media/`. La navegación del sitio usa filtros de Drupal que generan parámetros `academico` por nivel.

**URL base de los PDFs:**
```
https://www.mep.go.cr/sites/default/files/media/<nombre-archivo>.pdf
```

---

## 2. URLs de búsqueda por nivel (para encontrar nuevos PDFs)

| Nivel | URL de búsqueda |
|-------|----------------|
| I Ciclo (1°–3°) | `https://www.mep.go.cr/programas-estudio?texto-programas-academicos=<búsqueda>&academico=8080` |
| II Ciclo (4°–6°) | `https://www.mep.go.cr/programas-estudio?texto-programas-academicos=<búsqueda>&academico=8081` |
| III Ciclo (7°–9°) | `https://www.mep.go.cr/programas-estudio?texto-programas-academicos=<búsqueda>&academico=8082` |
| Diversificada (10°–12°) | `https://www.mep.go.cr/programas-estudio?texto-programas-academicos=<búsqueda>&academico=8083` |
| Técnica | `https://www.mep.go.cr/programas-estudio-educacion-tecnica` |
| Talleres exploratorios | `https://www.mep.go.cr/programas-estudio-talleres-exploratorios` |

> ⚠️ Las búsquedas con caracteres especiales (tildes, ñ) a veces generan error 422. Usar términos simples sin acentos: `musicales` en lugar de `artes musicales`, `hogar` en lugar de `educación para el hogar`.

---

## 3. Cómo extraer el link directo del PDF con curl

```bash
curl -s "https://www.mep.go.cr/programas-estudio?texto-programas-academicos=<término>&academico=<nivel>" \
  | grep -o 'href="[^"]*\.pdf"' \
  | grep -iv "inf-estrategia\|guia-mep"
```

Ejemplo — buscar Educación Musical en III Ciclo:
```bash
curl -s "https://www.mep.go.cr/programas-estudio?texto-programas-academicos=musicales&academico=8082" \
  | grep -o 'href="[^"]*\.pdf"' \
  | grep -iv "inf-estrategia\|guia-mep"
# → href="/sites/default/files/media/musica3cicloydiversificada.pdf"
```

Para listar TODOS los PDFs de un nivel (sin filtro de texto):
```bash
curl -s "https://www.mep.go.cr/programas-estudio?texto-programas-academicos=&academico=8082&page=1" \
  | grep -o 'href="[^"]*\.pdf"' \
  | grep -iv "inf-estrategia\|guia-mep"
```

> Hay hasta 3 páginas por nivel (`page=0`, `page=1`, `page=2`). Iterar si no aparece el PDF buscado.

---

## 4. PDFs descargados al 2026-05-08 — `assets/` (raíz del repo)

| Archivo local | URL original MEP | Asignatura AulaIA | Ciclos |
|---------------|-----------------|-------------------|--------|
| `artes-plasticas-3ciclo-div.pdf` | `/media/artesplasticas3cicloydiversificada.pdf` | Artes Plásticas | III + Diversificada |
| `musica-3ciclo-div.pdf` | `/media/musica3cicloydiversificada.pdf` | Artes Musicales | III + Diversificada |
| `educacion-vida-cotidiana-3ciclo.pdf` | `/media/educacion-vida-cotidiana.pdf` | Educación para el Hogar | III ciclo |
| `educacion-vida-cotidiana-1y2ciclo.pdf` | `/media/vida-cotidiana1y2ciclos.pdf` | Educación para el Hogar | I + II ciclo |
| `educacion-fisica-3ciclo-div.pdf` | `/media/educfisica3cicloydiversificada.pdf` | Educación Física | III + Diversificada |
| `matematica.pdf` | `/media/matematica.pdf` | Matemáticas | Todos |
| `espanol-3ciclo-div.pdf` | `/media/espanol3ciclo_diversificada.pdf` | Español | III + Diversificada |
| `ciencias-3ciclo.pdf` | `/media/ciencias3ciclo.pdf` | Ciencias | III ciclo |
| `estudios-sociales-3ciclo-div.pdf` | `/media/esociales3ciclo_diversificada.pdf` | Estudios Sociales | III + Diversificada |
| `ingles-3ciclo-div.pdf` | `/media/ingles3ciclo_diversificada.pdf` | Inglés | III + Diversificada |
| `ingles-2ciclo.pdf` | `/media/ingles_2ciclo.pdf` | Inglés | II ciclo |
| `frances-3ciclo-div.pdf` | `/media/frances3ciclo_diversificada.pdf` | Francés | III + Diversificada |
| `orientacion.pdf` | `/media/orientacion-nuevo.pdf` | Orientación | Todos |

> **Nombres MEP vs AulaIA:** El MEP llama "Educación para la Vida Cotidiana" a lo que AulaIA llama "Educación para el Hogar". El MEP llama "Educación Musical" a lo que AulaIA llama "Artes Musicales". Los PDFs son los mismos.

---

## 5. Comando completo para descargar en batch

```bash
BASE="https://www.mep.go.cr/sites/default/files/media"
DIR="assets"

declare -A PDFS=(
  ["artes-plasticas-3ciclo-div.pdf"]="artesplasticas3cicloydiversificada.pdf"
  ["musica-3ciclo-div.pdf"]="musica3cicloydiversificada.pdf"
  ["educacion-vida-cotidiana-3ciclo.pdf"]="educacion-vida-cotidiana.pdf"
  ["educacion-vida-cotidiana-1y2ciclo.pdf"]="vida-cotidiana1y2ciclos.pdf"
  ["educacion-fisica-3ciclo-div.pdf"]="educfisica3cicloydiversificada.pdf"
  ["matematica.pdf"]="matematica.pdf"
  ["espanol-3ciclo-div.pdf"]="espanol3ciclo_diversificada.pdf"
  ["ciencias-3ciclo.pdf"]="ciencias3ciclo.pdf"
  ["estudios-sociales-3ciclo-div.pdf"]="esociales3ciclo_diversificada.pdf"
  ["ingles-3ciclo-div.pdf"]="ingles3ciclo_diversificada.pdf"
  ["ingles-2ciclo.pdf"]="ingles_2ciclo.pdf"
  ["frances-3ciclo-div.pdf"]="frances3ciclo_diversificada.pdf"
  ["orientacion.pdf"]="orientacion-nuevo.pdf"
)

for DEST in "${(@k)PDFS}"; do
  SRC="${PDFS[$DEST]}"
  curl -s -L -o "$DIR/$DEST" "$BASE/$SRC"
  echo "✅ $DEST ($(wc -c < "$DIR/$DEST" | tr -d ' ') bytes)"
done
```

> ⚠️ Usar `"${(@k)PDFS}"` (sintaxis zsh) para iterar asociative arrays. En bash usar `"${!PDFS[@]}"`.

---

## 6. Subir PDFs al API — solo admin, desde esta máquina

El endpoint `POST /api/curriculum/upload` requiere token JWT con rol `admin`. Este proceso lo ejecuta el agente con instrucciones de Ezequiel — nunca los docentes.

**Obtener el token admin** (ver skill `mep-db-access` para credenciales Auth0):
```bash
TOKEN=$(curl -s -X POST "https://aulaia-mep.us.auth0.com/oauth/token" \
  -H "Content-Type: application/json" \
  -d '{"client_id":"<M2M_CLIENT_ID>","client_secret":"<M2M_SECRET>","audience":"https://api.aulaia.mep.go.cr","grant_type":"client_credentials"}' \
  | grep -o '"access_token":"[^"]*"' | cut -d'"' -f4)
```

O copiar el token de la consola del navegador (DevTools → Application → Auth0 token).

**Subir un PDF:**
```bash
API="https://mep.ezekl.com"

curl -s -X POST "$API/api/curriculum/upload" \
  -H "Authorization: Bearer $TOKEN" \
  -F "file=@assets/musica-3ciclo-div.pdf;type=application/pdf" \
  -F "asignatura=Artes Musicales" \
  -F "ciclo=III Ciclo"
```

Parámetros:
- `asignatura`: nombre exacto del dropdown del UI (`Artes Musicales`, `Educación para el Hogar`, `Artes Plásticas`, etc.)
- `ciclo`: `I y II Ciclo` | `III Ciclo` | `Diversificada`

Respuesta exitosa: `202 Accepted` con `{ "jobId": "...", "blobUrl": "..." }`. El job corre en cola `curriculum` con Hangfire y extrae las unidades curriculares usando GPT-5.5.

**Uploads pendientes al 2026-05-08** (PDFs están en `assets/`, falta subirlos al API):

| Archivo | asignatura | ciclo | Estado |
|---------|-----------|-------|--------|
| `musica-3ciclo-div.pdf` | `Artes Musicales` | `III Ciclo` | ⏳ |
| `educacion-vida-cotidiana-3ciclo.pdf` | `Educación para el Hogar` | `III Ciclo` | ⏳ |
| `educacion-vida-cotidiana-1y2ciclo.pdf` | `Educación para el Hogar` | `I y II Ciclo` | ⏳ |
| `artes-plasticas-3ciclo-div.pdf` | `Artes Plásticas` | `III Ciclo` | ✅ (ya procesado) |
| `educacion-fisica-3ciclo-div.pdf` | `Educación Física` | `III Ciclo` | ⏳ |
| `matematica.pdf` | `Matemáticas` | `III Ciclo` | ⏳ |
| `espanol-3ciclo-div.pdf` | `Español` | `III Ciclo` | ⏳ |
| `ciencias-3ciclo.pdf` | `Ciencias` | `III Ciclo` | ⏳ |
| `estudios-sociales-3ciclo-div.pdf` | `Estudios Sociales` | `III Ciclo` | ⏳ |
| `ingles-3ciclo-div.pdf` | `Inglés` | `III Ciclo` | ⏳ |
| `frances-3ciclo-div.pdf` | `Francés` | `III Ciclo` | ⏳ |
| `orientacion.pdf` | `Orientación` | `III Ciclo` | ⏳ |

---

## 7. Verificar que la extracción funcionó

```bash
# Ver el audit log del API
cat src/AulaIA.Api/logs/llm-audit.md | grep "ExtractCurriculumJob" | tail -20

# O consultar la BD directamente
psql <conexion> -c "SELECT asignatura, ciclo, unidad_count, created_at FROM curriculum_extractions ORDER BY created_at DESC LIMIT 10;"
```
