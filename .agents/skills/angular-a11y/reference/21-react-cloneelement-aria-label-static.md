# Reference 21 — React/Next.js: cloneElement/useId no resuelven el accessible name en análisis estático

**Regla Axe:** `select-name` / `label` — "Select element must have an accessible name"  
**Herramienta que reporta:** Microsoft Edge Tools (axe/forms)  
**Stack:** React 18 / Next.js (aplica también a cualquier framework con análisis estático de JSX)

---

## Problema

Un componente wrapper (`InputField`) intentaba asociar el `<label>` con su control hijo
usando `React.cloneElement` + `useId()` para inyectar el `id` en runtime:

```tsx
// ❌ No funciona para analizadores estáticos
function InputField({ label, children }) {
  const fieldId = useId();
  return (
    <div>
      <label htmlFor={fieldId}>{label}</label>
      {React.cloneElement(children, { id: fieldId })}
    </div>
  );
}

<InputField label="Asignatura">
  <select ...>   {/* Edge Tools / axe ven este select sin id ni aria-label */}
```

Edge Tools y axe realizan **análisis estático del árbol JSX** antes de ejecutar el código.
`cloneElement` inyecta el `id` únicamente en runtime, por lo que el analizador estático
ve el `<select>` sin ningún nombre accesible y reporta la violación.

El mismo problema aplica a:
- `useId()` inyectado via `cloneElement`
- `context` que resuelve el `id` en un componente padre
- Cualquier mecanismo que calcule el `id` fuera del JSX visible del control

---

## Fix

Añadir `aria-label` **directamente** en cada `<select>` / `<input>`, visible
estáticamente en el JSX del control:

```tsx
// ✅ aria-label directo — visible para el analizador estático
function InputField({ label, children }) {
  return (
    <div>
      <label className="...">{label}</label>   {/* label visual */}
      {children}
    </div>
  );
}

<InputField label="Asignatura">
  <select aria-label="Asignatura" ...>
```

La `<label>` sigue siendo útil como texto visual para el usuario;
el `aria-label` en el control es lo que el lector de pantalla y Edge Tools consumen.

---

## Regla de oro extraída

> En componentes wrapper de React/Next.js, **nunca depender de `cloneElement` o `useId`
> para inyectar el nombre accesible de un control**. El `aria-label` debe estar escrito
> directamente en el elemento `<select>` / `<input>` / `<textarea>`.

---

## Archivos donde se aplicó

- `src/aulaia-web/src/app/planeamiento/nuevo/page.tsx` (2026-05-06)
  - Controles: Grupo/Sección, Asignatura, Nivel, Trimestre, Año lectivo, Lecciones/semana, Fecha inicio, Fecha fin
