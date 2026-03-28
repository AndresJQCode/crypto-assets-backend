# Contributing Guidelines

## 🚨 REGLAS CRÍTICAS - NO NEGOCIABLES

### 📏 EditorConfig
**⚠️ NUNCA eliminar o desactivar reglas del archivo `.editorconfig`**

El archivo `.editorconfig` contiene estándares de código que mantienen la calidad y consistencia del proyecto.

**Prohibido:**
- ❌ Eliminar reglas existentes
- ❌ Cambiar severidad de `error` a `warning` o `none`
- ❌ Desactivar analyzers (CA*, SA*, IDE*)
- ❌ Agregar excepciones globales sin justificación documentada

**Permitido:**
- ✅ Agregar **nuevas** reglas más estrictas
- ✅ Documentar excepciones en archivos específicos con comentarios
- ✅ Mejorar configuraciones existentes (más restrictivo = mejor)

**Excepción única aceptada:**
- `CA1873` - Falso positivo conocido en .NET 10 para logging (ya configurado)

### 🔧 Si encuentras problemas de compilación por reglas de código:

1. **NO desactives la regla**
2. **Arregla el código** para cumplir con el estándar
3. Si es imposible cumplir, documenta por qué y solicita aprobación

### 💡 Ejemplo de excepción documentada (caso extremo):

```csharp
// EXCEPCIÓN JUSTIFICADA: Código autogenerado por Entity Framework
// REGLA AFECTADA: CA1062 - Null validation
#pragma warning disable CA1062
public class MigrationSnapshot : Migration
{
    // ... código autogenerado
}
#pragma warning restore CA1062
```

## 📋 Checklist antes de commit

- [ ] Código compila sin errores
- [ ] No se modificó `.editorconfig` para desactivar reglas
- [ ] Tests pasan (cuando existan)
- [ ] Código sigue convenciones de naming
- [ ] XML documentation en APIs públicas

## 🎯 Filosofía

> El código de calidad no es negociable. Las reglas existen para mantener un estándar alto que beneficia a todo el equipo a largo plazo.

---

**Si tienes dudas sobre una regla específica, pregunta antes de desactivarla.**
