# 07 — Issues Conocidos

> **Última actualización:** 2026-05-08

---

## ✅ ISSUE-001: BlobNotFound en ExtractCurriculumJob para asignaturas con acentos

**Detectado:** 2026-05-08 (job 38 en Hangfire)
**Estado:** ✅ Resuelto
**Componentes:** `SyncCurriculumJob.cs`, `ExtractCurriculumJob.cs`

### Síntoma
`ExtractCurriculumJob` falla con `Azure.RequestFailedException: BlobNotFound (404)` al intentar descargar el PDF de asignaturas cuyo nombre contiene acentos (ej. _Educación Física_, _Artes Plásticas_, _Matemáticas_).

### Causa raíz
Dos problemas combinados:

1. **`SyncCurriculumJob`** generaba el blob name con `asignatura.ToLowerInvariant().Replace(" ", "-")`, que preserva acentos: `educación-física/20260508190108.pdf`.
2. **`ExtractCurriculumJob`** extraía el blob name desde `Uri.AbsolutePath`, que percent-encodea caracteres no-ASCII: `educaci%C3%B3n-f%C3%ADsica/...`. Ese string encoded se usaba literalmente como nombre de blob → no coincidía → 404.

### Fix aplicado
- `ExtractCurriculumJob.cs` línea ~128: `Uri.UnescapeDataString(blobUri.AbsolutePath)` antes de construir el blob name. Resuelve jobs con blobs ya subidos con acentos.
- `SyncCurriculumJob.cs`: nuevo método `ToAsciiSlug()` (NFD normalization + strip NonSpacingMark) para generar slugs ASCII puros en subidas futuras.

### Verificación
- Blob `educación-física/20260508190108.pdf` existe en container `curriculum` (confirmado con `az storage blob list`).
- Build compila sin errores ni advertencias.
- Retry del job 38 en Hangfire dashboard debería pasar tras reiniciar la API con el fix.
