---
name: git-github-flow
description: Git workflow following GitHub Flow (main always deployable, feature branches, pull requests). Use when creating branches, merging, doing pull requests, rebasing, or when the user asks about Git workflow or GitHub flow.
---

# Git con GitHub Flow

## Principios

- **main** es la rama principal y siempre debe estar en estado desplegable.
- Todo el trabajo nuevo se hace en **ramas de corta vida** creadas desde main.
- Las integraciones se hacen mediante **Pull Requests** (PR) hacia main.
- No hay rama `develop` ni ramas de release; se despliega desde main.

## Flujo de trabajo

### 1. Crear rama desde main

Antes de crear una rama, asegurar que main está actualizada:

```bash
git checkout main
git pull origin main
git checkout -b <tipo>/<descripcion-corta>
```

**Convención de nombres de rama:**

| Prefijo   | Uso |
|-----------|-----|
| `feature/` | Nueva funcionalidad (ej. `feature/auth-jwt`) |
| `fix/`     | Corrección de bug (ej. `fix/login-timeout`) |
| `docs/`    | Solo documentación |
| `refactor/`| Refactor sin cambiar comportamiento |

### 2. Trabajar en la rama

- Hacer commits atómicos y con mensajes claros.
- Hacer push con frecuencia para respaldo y para abrir el PR cuando esté listo:

```bash
git add .
git commit -m "tipo(ámbito): descripción breve"
git push -u origin <nombre-rama>
```

### 3. Abrir Pull Request

- Crear el PR desde la rama de feature/fix hacia **main**.
- El PR debe tener descripción, revisión (si aplica) y pasar CI si existe.
- Mantener el PR pequeño y enfocado; varios PRs pequeños mejor que uno enorme.

### 4. Actualizar la rama con main (antes de merge)

Si main ha avanzado, actualizar la rama (recomendado: rebase para historial lineal):

```bash
git fetch origin
git rebase origin/main
# Resolver conflictos si los hay, luego:
git push --force-with-lease
```

Alternativa con merge (historial con merges):

```bash
git fetch origin
git merge origin/main
git push
```

### 5. Merge a main

- Hacer **merge** del PR en GitHub (merge commit o squash según política del repo).
- Tras el merge, la rama puede borrarse en remoto.
- Actualizar main local: `git checkout main && git pull origin main`.

## Comandos rápidos

| Acción | Comando |
|--------|---------|
| Ver ramas | `git branch -a` |
| Cambiar a main y actualizar | `git checkout main && git pull origin main` |
| Crear y cambiar a nueva rama | `git checkout -b feature/nombre` |
| Ver estado y rama actual | `git status` |
| Subir rama y definir upstream | `git push -u origin <rama>` |
| Rebase sobre main actual | `git fetch origin && git rebase origin/main` |

## Reglas prácticas

1. No hacer commit directo en main en repos compartidos; todo por PR.
2. No hacer push de main con `--force` en repos compartidos.
3. Resolver conflictos en la rama de feature/fix, no en main.
4. Borrar ramas locales después de merge: `git branch -d <rama>`.
5. En rebase, usar `--force-with-lease` en lugar de `--force` para no sobrescribir trabajo remoto por error.

## Mensajes de commit (opcional)

Formato convencional breve:

```
tipo(ámbito): descripción en imperativo

- tipo: feat, fix, docs, refactor, test, chore
- ámbito: módulo o archivo afectado (opcional)
- descripción: una línea, sin punto final
```

Ejemplos: `feat(auth): add JWT login`, `fix(api): handle null in paged result`.
