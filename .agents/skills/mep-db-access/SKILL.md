---
name: mep-db-access
description: >
  Acceso directo a la base de datos PostgreSQL del proyecto MEP (AulaIA).
  Documenta cómo conectarse con psql, de dónde tomar las credenciales y los
  comandos SQL más usados para inspeccionar y depurar datos en desarrollo.
  Usar cuando se necesite consultar tablas, verificar resultados de jobs,
  limpiar datos de prueba, o diagnosticar errores de persistencia.
applyTo: "**"
---

# Skill: MEP DB Access

## Fuente de credenciales

**Siempre** leer las credenciales desde:

```
src/AulaIA.Api/appsettings.Development.json → "Database" → "ConnectionString"
```

Formato del connection string (Npgsql):
```
Host=<host>;Port=<port>;Database=<db>;Username=<user>;Password=<password>;SslMode=Prefer
```

### Extraer credenciales con Python

```bash
python3 -c "
import json, re
cs = json.load(open('src/AulaIA.Api/appsettings.Development.json'))['Database']['ConnectionString']
parts = dict(p.split('=',1) for p in cs.split(';') if '=' in p)
print(f\"host={parts['Host']} port={parts['Port']} db={parts['Database']} user={parts['Username']} pass={parts['Password']}\")
"
```

## Comando de conexión psql

```bash
# Desde la raíz del repo
DB_CS=$(python3 -c "import json; print(json.load(open('src/AulaIA.Api/appsettings.Development.json'))['Database']['ConnectionString'])")
HOST=$(echo $DB_CS | python3 -c "import sys; cs=sys.stdin.read(); parts=dict(p.split('=',1) for p in cs.split(';') if '=' in p); print(parts['Host'])")
PORT=$(echo $DB_CS | python3 -c "import sys; cs=sys.stdin.read(); parts=dict(p.split('=',1) for p in cs.split(';') if '=' in p); print(parts['Port'])")
DBNAME=$(echo $DB_CS | python3 -c "import sys; cs=sys.stdin.read(); parts=dict(p.split('=',1) for p in cs.split(';') if '=' in p); print(parts['Database'])")
USER=$(echo $DB_CS | python3 -c "import sys; cs=sys.stdin.read(); parts=dict(p.split('=',1) for p in cs.split(';') if '=' in p); print(parts['Username'])")
PASS=$(echo $DB_CS | python3 -c "import sys; cs=sys.stdin.read(); parts=dict(p.split('=',1) for p in cs.split(';') if '=' in p); print(parts['Password'])")

PGPASSWORD="$PASS" psql -h $HOST -p $PORT -U $USER -d $DBNAME
```

### Forma abreviada (hardcoded, válida para dev)

```bash
PGPASSWORD='198zklA8!' psql -h 172.191.128.24 -p 5432 -U demoadmin -d aulaia
```

> ⚠️ La forma abreviada es solo para dev. Siempre preferir leer de `appsettings.Development.json`.

### Verificar que psql está disponible

```bash
which psql || (brew install libpq && export PATH="/opt/homebrew/opt/libpq/bin:$PATH")
```

Si el PATH no persiste, agregar a `~/.zshrc`:
```bash
echo 'export PATH="/opt/homebrew/opt/libpq/bin:$PATH"' >> ~/.zshrc
```

---

## Comandos SQL frecuentes

### Inspeccionar curriculum_units

```sql
-- Resumen por ciclo
SELECT "Ciclo", COUNT(*) AS unidades, SUM("TokensUsed") AS tokens_total
FROM curriculum_units
GROUP BY "Ciclo"
ORDER BY "Ciclo";

-- Ver todas las unidades con conteo de arrays JSONB
SELECT
  "Ciclo", "Nivel", "Trimestre", "UnidadNumero", "UnidadNombre",
  "TokensUsed",
  jsonb_array_length("AprendizajesEsperados") AS aprendizajes,
  jsonb_array_length("IndicadoresEvaluacion") AS indicadores,
  "ValidatedAt" IS NOT NULL AS validada
FROM curriculum_units
ORDER BY "Ciclo", "Nivel", "Trimestre", "UnidadNumero";

-- Borrar unidades no validadas (limpieza para re-extracción)
DELETE FROM curriculum_units WHERE "ValidatedAt" IS NULL;

-- Ver el detalle JSONB de una unidad
SELECT "UnidadNombre", "AprendizajesEsperados", "IndicadoresEvaluacion"
FROM curriculum_units
WHERE "UnidadNumero" = 1 AND "Nivel" = 7;
```

### Inspeccionar planeamientos

```sql
SELECT id, "DocenteId", "Asignatura", "Nivel", "Trimestre", "Estado", "CreatedAt"
FROM planeamientos
ORDER BY "CreatedAt" DESC
LIMIT 20;
```

### Inspeccionar asistencia

```sql
SELECT g."Nombre" AS grupo, COUNT(a.id) AS registros
FROM asistencia a
JOIN grupos g ON g."Id" = a."GrupoId"
GROUP BY g."Nombre";
```

### Tablas del schema

```sql
-- Listar todas las tablas
\dt

-- Ver columnas de una tabla
\d curriculum_units
```

---

## Integración con dotnet ef (migraciones)

Ejecutar siempre desde `src/AulaIA.Api/`:

```bash
cd src/AulaIA.Api

# Crear migración
dotnet ef migrations add <NombreMigracion>

# Aplicar migraciones pendientes
dotnet ef database update

# Ver historial de migraciones
dotnet ef migrations list
```
