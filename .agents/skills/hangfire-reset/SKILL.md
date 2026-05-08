---
name: hangfire-reset
description: >
  Diagnóstico y reset del estado de Hangfire en PostgreSQL cuando los recurring jobs no
  aparecen en el dashboard, o cuando se necesita limpiar jobs encolados/fallidos para
  volver a ejecutarlos. Usar cuando: la pestaña Recurring del dashboard aparece vacía,
  los jobs no se re-registran tras reiniciar la API, se quiere limpiar curriculum_sources
  + jobs para re-run, o cualquier estado inconsistente de Hangfire en dev.
  Triggers: hangfire recurring vacío, jobs no aparecen, dashboard sin jobs, reset hangfire,
  limpiar hangfire, volver a correr jobs, hangfire.set vacío.
applyTo: "src/AulaIA.Api/**"
---

# Hangfire Reset — Diagnóstico y limpieza

## Causa raíz descubierta (mayo 2026)

Hangfire usa **dos tablas** para recurring jobs:

| Tabla | Propósito |
|---|---|
| `hangfire.hash` | Definición del job (cron, queue, serialización del método) |
| `hangfire.set` | Índice que usa el dashboard para listar jobs en `/hangfire/recurring` |

Cuando se hace `TRUNCATE hangfire.set` (o se borran filas de `recurring-jobs`),
el dashboard queda vacío **aunque los jobs sigan en `hangfire.hash`**.

Al reiniciar la API, `IRecurringJobManager.AddOrUpdate` detecta que el hash ya existe
y solo lo actualiza — **sin re-insertar en el set**. El dashboard permanece vacío
indefinidamente aunque los jobs se ejecuten normalmente por el scheduler.

---

## Diagnóstico rápido

```bash
# ¿Están los jobs en el hash?
PGPASSWORD="..." psql -h HOST -p PORT -U USER -d DB \
  -c "SELECT key, field FROM hangfire.hash WHERE key LIKE 'recurring-job:%' LIMIT 10;"

# ¿Está el índice del dashboard?
PGPASSWORD="..." psql -h HOST -p PORT -U USER -d DB \
  -c "SELECT key, value FROM hangfire.set WHERE key = 'recurring-jobs';"
```

**Si hay filas en `hash` pero ninguna en `set` con `key = 'recurring-jobs'`**
→ aplicar el fix de abajo.

---

## Fix — Reset de recurring jobs

```bash
# 1. Eliminar las definiciones del hash (fuerza inserción limpia en el próximo arranque)
PGPASSWORD="..." psql -h HOST -p PORT -U USER -d DB \
  -c "DELETE FROM hangfire.hash WHERE key LIKE 'recurring-job:%';"

# 2. Reiniciar la API (Shift+F5 → F5 en VS Code, o Ctrl+C → dotnet run)
#    AddAulaIARecurringJobs() escribirá en AMBAS tablas porque no encuentra el hash.
```

Tras el reinicio verificar:

```bash
PGPASSWORD="..." psql -h HOST -p PORT -U USER -d DB \
  -c "SELECT value FROM hangfire.set WHERE key = 'recurring-jobs';"
# Debe listar: update-exchange-rate, check-expired-subscriptions, sync-curriculum-mep
```

---

## Limpieza completa para re-run de jobs de curriculum

Útil cuando se quiere volver a correr `SyncCurriculumJob` desde cero:

```bash
PGPASSWORD="..." psql -h HOST -p PORT -U USER -d DB << 'SQL'
-- 1. Limpiar toda la BD de Hangfire
TRUNCATE TABLE
  hangfire.job,
  hangfire.jobparameter,
  hangfire.jobqueue,
  hangfire.state,
  hangfire.set,
  hangfire.hash,
  hangfire.list,
  hangfire.counter,
  hangfire.aggregatedcounter
RESTART IDENTITY CASCADE;

-- 2. Limpiar datos de curriculum para que SyncCurriculumJob vuelva a sembrar y procesar
TRUNCATE TABLE curriculum_sources RESTART IDENTITY CASCADE;
SQL
```

Luego reiniciar la API — el startup registra los recurring jobs limpios.

---

## Regla de oro

> **Nunca truncar `hangfire.set` o `hangfire.hash` de forma aislada.**
> Si se va a limpiar Hangfire en dev, truncar **todas** las tablas del schema
> de una sola vez, o ninguna. De lo contrario el estado queda inconsistente.

---

## Credenciales de BD (dev)

Leer de `src/AulaIA.Api/appsettings.Development.json` → clave `Database.ConnectionString`.

Ejemplo rápido para extraer y conectar:

```bash
DB_CS=$(python3 -c "import json; print(json.load(open('src/AulaIA.Api/appsettings.Development.json'))['Database']['ConnectionString'])")
HOST=$(echo $DB_CS | python3 -c "import sys; cs=sys.stdin.read(); parts=dict(p.split('=',1) for p in cs.split(';') if '=' in p); print(parts['Host'])")
PORT=$(echo $DB_CS | python3 -c "import sys; cs=sys.stdin.read(); parts=dict(p.split('=',1) for p in cs.split(';') if '=' in p); print(parts['Port'])")
DBNAME=$(echo $DB_CS | python3 -c "import sys; cs=sys.stdin.read(); parts=dict(p.split('=',1) for p in cs.split(';') if '=' in p); print(parts['Database'])")
USER=$(echo $DB_CS | python3 -c "import sys; cs=sys.stdin.read(); parts=dict(p.split('=',1) for p in cs.split(';') if '=' in p); print(parts['Username'])")
PASS=$(echo $DB_CS | python3 -c "import sys; cs=sys.stdin.read(); parts=dict(p.split('=',1) for p in cs.split(';') if '=' in p); print(parts['Password'])")
PGPASSWORD="$PASS" psql -h $HOST -p $PORT -U $USER -d $DBNAME
```

---

## Contexto: registro de recurring jobs en este repo

`AddAulaIARecurringJobs()` en `ModulesExtensions.cs` usa `IRecurringJobManager` desde DI
(no `RecurringJob.AddOrUpdate` estático — ese depende de `JobStorage.Current` que puede
no estar disponible en startup):

```csharp
public void AddAulaIARecurringJobs()
{
    var manager = app.Services.GetRequiredService<IRecurringJobManager>();
    manager.AddOrUpdate<UpdateExchangeRateJob>(
        "update-exchange-rate", j => j.ExecuteAsync(CancellationToken.None), "0 12 * * *");
    manager.AddOrUpdate<CheckExpiredSubscriptionsJob>(
        "check-expired-subscriptions", j => j.ExecuteAsync(CancellationToken.None), "0 8 * * *");
    manager.AddOrUpdate<SyncCurriculumJob>(
        "sync-curriculum-mep", j => j.ExecuteAsync(null, CancellationToken.None), "0 0 30 2 *");
}
```
