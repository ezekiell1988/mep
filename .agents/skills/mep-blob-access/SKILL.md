---
name: mep-blob-access
description: >
  Acceso directo a Azure Blob Storage del proyecto MEP (AulaIA) usando az CLI.
  Documenta cómo leer el connection string, listar blobs, verificar existencia,
  descargar y limpiar blobs en desarrollo. Usar cuando se necesite inspeccionar
  archivos subidos (PDFs de curriculum, reportes, planeamientos, etc.), verificar
  si un blob existe después de un job fallido, o diagnosticar errores de storage.
  Triggers: blob, storage, az storage, container, curriculum pdf, blob not found,
  BlobNotFound, 404 blob, listar blobs, descargar blob, subir blob.
applyTo: "**"
---

# Skill: MEP Blob Access

## Fuente de credenciales

**Siempre** leer el connection string desde:

```
src/AulaIA.Api/appsettings.Development.json → "Storage" → "ConnectionString"
```

```bash
STORAGE_CS=$(python3 -c "import json; d=json.load(open('src/AulaIA.Api/appsettings.Development.json')); print(d['Storage']['ConnectionString'])")
```

### Forma abreviada (hardcoded, válida solo para dev)

Consultar el archivo `credentials/appservice.txt` para el connection string actual.

---

## Contenedores disponibles

| Container | Uso |
|-----------|-----|
| `curriculum` | PDFs de programas del MEP (SyncCurriculumJob / ExtractCurriculumJob) |
| `planeamientos` | Archivos de planeamiento generados |
| `reportes` | Reportes exportados |
| `exportaciones` | Exportaciones varias |
| `adjuntos` | Adjuntos de usuarios |
| `plantillas` | Plantillas de documentos |
| `pagos` | Comprobantes de pago |

---

## Comandos frecuentes

### Listar blobs de un container

```bash
STORAGE_CS=$(python3 -c "import json; d=json.load(open('src/AulaIA.Api/appsettings.Development.json')); print(d['Storage']['ConnectionString'])")

az storage blob list \
  --connection-string "$STORAGE_CS" \
  --container-name curriculum \
  --output table
```

### Listar blobs de un prefijo (carpeta virtual)

```bash
az storage blob list \
  --connection-string "$STORAGE_CS" \
  --container-name curriculum \
  --prefix "educacion-fisica/" \
  --output table
```

### Verificar si un blob existe

```bash
az storage blob exists \
  --connection-string "$STORAGE_CS" \
  --container-name curriculum \
  --name "educacion-fisica/20260508190108.pdf" \
  --output table
```

> ⚠️ Los nombres de blob son rutas literales. Si fueron subidos con acentos (bug previo),
> usar el nombre exacto con acento: `"educación-física/..."`.

### Descargar un blob a disco

```bash
az storage blob download \
  --connection-string "$STORAGE_CS" \
  --container-name curriculum \
  --name "educacion-fisica/20260508190108.pdf" \
  --file /tmp/edufisica.pdf
```

### Eliminar un blob

```bash
az storage blob delete \
  --connection-string "$STORAGE_CS" \
  --container-name curriculum \
  --name "educacion-fisica/20260508190108.pdf"
```

### Eliminar todos los blobs de un prefijo (limpieza)

```bash
az storage blob delete-batch \
  --connection-string "$STORAGE_CS" \
  --source curriculum \
  --pattern "educacion-fisica/*"
```

### Ver propiedades de un blob (tamaño, ETag, Content-Type)

```bash
az storage blob show \
  --connection-string "$STORAGE_CS" \
  --container-name curriculum \
  --name "educacion-fisica/20260508190108.pdf" \
  --output json
```

---

## Convención de nombres de blob (post-fix)

Desde el fix aplicado el 2026-05-08, `SyncCurriculumJob` genera slugs ASCII sin acentos:

```
{asignatura-slug}/{yyyyMMddHHmmss}.pdf
```

Ejemplos:
| Asignatura | Slug |
|---|---|
| Educación Física | `educacion-fisica` |
| Artes Plásticas | `artes-plasticas` |
| Matemáticas | `matematicas` |
| Español | `espanol` |
| Francés | `frances` |
| Orientación | `orientacion` |
| Inglés | `ingles` |

> Blobs anteriores al fix pueden tener acentos en el nombre (ej. `educación-física/`).
> `ExtractCurriculumJob` los maneja correctamente con `Uri.UnescapeDataString`.

---

## Diagnóstico: BlobNotFound en ExtractCurriculumJob

Si un job falla con `Azure.RequestFailedException: BlobNotFound`:

1. Obtener la URL del blob desde Hangfire BD:
```sql
SELECT invocationdata->>'Arguments' FROM hangfire.job WHERE id = <job_id>;
```

2. Extraer el blob name (primer elemento del array JSON de argumentos).

3. Verificar existencia con `az storage blob exists` (ver arriba).

4. Si existe pero con nombre diferente (ej. con/sin acento), el fix de `Uri.UnescapeDataString`
   en `ExtractCurriculumJob.cs` line ~128 debe resolverlo tras reiniciar la API.

5. Si no existe, el PDF no fue subido correctamente — re-ejecutar `SyncCurriculumJob`
   desde el dashboard de Hangfire.
