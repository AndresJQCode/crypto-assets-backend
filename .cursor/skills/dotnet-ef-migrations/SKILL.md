---
name: dotnet-ef-migrations
description: Crea migraciones de Entity Framework Core en .NET 10. Pide el nombre de la migración si no se indica y ejecuta el comando desde la raíz del repo. Usar cuando el usuario pida crear migraciones, add migration, agregar migración o modificar el esquema de base de datos con EF Core.
---

# Migraciones EF Core (.NET 10)

## Cuándo aplicar

Aplica esta skill cuando el usuario:
- Pida **crear una migración**, **agregar migración** o **add migration**
- Mencione migraciones de Entity Framework o EF Core
- Quiera generar cambios de esquema para la base de datos

## Flujo obligatorio

1. **Obtener el nombre de la migración**
   - Si el usuario **no** indicó un nombre: preguntar explícitamente: *"¿Qué nombre quieres para la migración?"* (o similar) y esperar la respuesta antes de ejecutar.
   - Si el usuario **sí** indicó un nombre: usarlo tal cual (por ejemplo "AddContactTags", "ContactTags", "AddNewTable").

2. **Ejecutar el comando**
   - Ejecutar **desde la raíz del repositorio** (donde está la solución .sln):
   ```bash
   dotnet ef migrations add NOMBRE_MIGRACION --project Infrastructure --startup-project Api
   ```
   - Sustituir `NOMBRE_MIGRACION` por el nombre indicado por el usuario (sin espacios; PascalCase recomendado).

3. **No editar archivos de migración**
   - Los archivos bajo `Infrastructure/Migrations/` son generados por EF Core. No modificarlos manualmente. Si algo sale mal, usar `dotnet ef migrations remove` y corregir el modelo, luego volver a crear la migración.

## Comando de referencia

```bash
dotnet ef migrations add <NombreMigracion> --project Infrastructure --startup-project Api
```

## Aplicar migraciones a la base de datos (solo si el usuario lo pide)

```bash
dotnet ef database update --project Infrastructure --startup-project Api
```

## Eliminar la última migración (solo si el usuario lo pide)

```bash
dotnet ef migrations remove --project Infrastructure --startup-project Api
```
