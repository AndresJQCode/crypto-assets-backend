# .NET Core DDD + CQRS Skills

## 📚 Overview

This collection provides **three specialized skills** for building enterprise-grade .NET Core applications with Domain-Driven Design, CQRS.

---

## 🎯 Skills Included

### 1. **dotnet-ddd-architecture** (Main Skill)
**File**: `dotnet-ddd-architecture/SKILL.md`

**Use When**:
- Starting a new feature or module
- Need comprehensive architecture guidance
- Reviewing overall design
- Learning DDD + CQRS patterns

**Contains**:
- Complete layer definitions and rules
- CQRS enforcement
- Technology separation (EF Core vs Dapper)
- Event handling patterns
- Detailed code examples for each layer

**Size**: Comprehensive (~400 lines)

---

### 2. **dotnet-code-validation** (Patterns & Validation)
**File**: `dotnet-code-validation/SKILL.md`

**Use When**:
- Implementing specific patterns (Result, Repository, UoW)
- Setting up validation
- Configuring EF Core
- Writing tests
- Need concrete code templates

**Contains**:
- Code review checklist
- Naming conventions
- Common patterns (Result, Specification, UoW)
- FluentValidation examples
- EF Core configuration templates
- Testing patterns
- Dependency injection setup

**Size**: Detailed (~350 lines)

---

### 3. **dotnet-ddd-quickref** (Quick Reference)
**File**: `dotnet-ddd-quickref/SKILL.md`

**Use When**:
- Need fast lookup
- Quick violation check
- Copy-paste templates
- Making quick decisions

**Contains**:
- Instant violation detector
- Ready-to-use code templates
- Layer cheat sheet
- CQRS decision tree
- Common mistakes & fixes
- 5-second architecture check

**Size**: Concise (~200 lines)

---

## 📁 Installation

### Option 1: Individual Skills (Recommended)
Copy each skill folder to your project's `.claude/skills/` directory:

```
your-project/
└── .claude/
    └── skills/
        ├── dotnet-ddd-architecture/
        │   └── SKILL.md
        ├── dotnet-code-validation/
        │   └── SKILL.md
        └── dotnet-ddd-quickref/
            └── SKILL.md
```

### Option 2: Use All Three Together
All three skills complement each other and can be used simultaneously:
- **Main skill** for architecture decisions
- **Validation skill** for implementation details
- **Quick ref** for fast lookups

---

## 🚀 Usage Guide

### Scenario 1: Creating a New Aggregate
1. **Read**: `dotnet-ddd-architecture` → Domain Layer section
2. **Copy Template**: `dotnet-ddd-quickref` → Domain Entity Template
3. **Validate**: `dotnet-code-validation` → Domain Layer checklist

### Scenario 2: Implementing a Query
1. **Read**: `dotnet-ddd-architecture` → CQRS Rules → Queries
2. **Copy Template**: `dotnet-ddd-quickref` → Query Template
3. **Performance**: `dotnet-code-validation` → Performance Patterns

### Scenario 3: Quick Architecture Decision
1. **Use**: `dotnet-ddd-quickref` → CQRS Decision Tree
2. **Confirm**: `dotnet-ddd-architecture` → Relevant section

### Scenario 4: Code Review
1. **Check**: `dotnet-code-validation` → Code Review Checklist
2. **Violations**: `dotnet-ddd-quickref` → Instant Violation Detector

---

## 🎓 Learning Path

### For Beginners
1. Start with `dotnet-ddd-quickref` to get familiar with patterns
2. Read `dotnet-ddd-architecture` layer by layer
3. Use `dotnet-code-validation` for specific implementations

### For Experienced Developers
1. Keep `dotnet-ddd-quickref` as your daily reference
2. Consult `dotnet-ddd-architecture` for architecture decisions
3. Use `dotnet-code-validation` for advanced patterns

---

## 🔍 Quick Decision Matrix

| Question | Skill to Use |
|----------|--------------|
| Which layer does this belong to? | `dotnet-ddd-quickref` → Layer Cheat Sheet |
| How do I structure my aggregate? | `dotnet-ddd-architecture` → Domain Layer |
| What's the correct pattern for X? | `dotnet-code-validation` → Patterns section |
| Is this code violating a rule? | `dotnet-ddd-quickref` → Violation Detector |
| How do I configure EF Core? | `dotnet-code-validation` → EF Core Configuration |
| Command or Query? | `dotnet-ddd-quickref` → CQRS Decision Tree |
| How to implement pagination? | `dotnet-code-validation` → Performance Patterns |

---

## ✅ Validation Workflow

Before committing code, validate against all three skills:

```
1. Quick Check (30 seconds)
   └─ dotnet-ddd-quickref → Red Flags section

2. Layer Validation (2 minutes)
   └─ dotnet-code-validation → Code Review Checklist

3. Architecture Review (5 minutes)
   └─ dotnet-ddd-architecture → Validation Checklist
```

---

## 🎯 Common Use Cases

### Use Case: Create New Command
```
Step 1: Check decision tree
        → dotnet-ddd-quickref

Step 2: Copy command template
        → dotnet-ddd-quickref → Command Template

Step 3: Read CQRS rules
        → dotnet-ddd-architecture → CQRS Rules → Commands

Step 4: Add validation
        → dotnet-code-validation → FluentValidation section
```

### Use Case: Optimize Query Performance
```
Step 1: Confirm using Dapper
        → dotnet-ddd-quickref → CQRS Decision Tree

Step 2: Check performance requirements
        → dotnet-ddd-architecture → Performance Requirements

Step 3: Implement pagination
        → dotnet-code-validation → Performance Patterns
```

---

## 🛡️ Rules Hierarchy

**MANDATORY** (Never break):
- Domain layer has zero dependencies
- Commands use EF Core, Queries use Dapper
- No business logic outside Domain
- Pagination on all lists

**STRONGLY RECOMMENDED** (Break only with justification):
- Strongly-typed IDs
- Result pattern for commands
- Value Objects for domain concepts
- Minimal APIs over Controllers

**BEST PRACTICES** (Follow when possible):
- Specification pattern for complex queries
- FluentValidation for commands
- Unit of Work pattern
- Domain events for cross-aggregate communication

---

## 📖 Skill Dependencies

```
dotnet-ddd-quickref (Quick Reference)
    ↓ references
dotnet-ddd-architecture (Architecture Rules)
    ↓ implemented by
dotnet-code-validation (Implementation Patterns)
```

**Use Together**: All three skills are designed to work together. Quick ref points to detailed docs, which link to implementation patterns.

---

## 🚨 When Claude Should Use These Skills

Claude should automatically read these skills when:

1. **Architecture Questions**
   - "How should I structure this?"
   - "Which layer does X belong to?"
   - "Should this be a command or query?"

2. **Code Generation**
   - Creating new aggregates
   - Implementing commands/queries
   - Setting up repositories
   - Configuring EF Core

3. **Code Review**
   - User asks "Is this correct?"
   - Reviewing uploaded code
   - Refactoring requests

4. **Pattern Questions**
   - "How do I implement pagination?"
   - "Show me the Result pattern"
   - "How to configure value objects?"

---

## 💡 Tips for Effective Use

### Do's ✅
- Read the quick reference first for fast decisions
- Use templates as starting points, not final code
- Combine insights from multiple skills
- Reference the checklist before code reviews

### Don'ts ❌
- Don't skip the architecture skill for complex features
- Don't blindly copy templates without understanding
- Don't violate "MANDATORY" rules
- Don't mix patterns from different paradigms

---

## 🔄 Skill Update Strategy

As your project evolves, you may need to:

1. **Add Project-Specific Rules**
   - Create a fourth skill: `dotnet-ddd-[projectname]`
   - Reference these base skills
   - Add company/project-specific conventions

2. **Customize Templates**
   - Copy templates to your own skill
   - Adjust naming conventions
   - Add project-specific validations

3. **Extend Patterns**
   - Document new patterns you discover
   - Share across team via custom skills

---

## 📞 Support

If you encounter situations not covered by these skills:

1. Check all three skills for relevant sections
2. Consult official DDD resources (Eric Evans, Vaughn Vernon)
3. Review .NET documentation for EF Core/Dapper specifics
4. Create a custom skill for recurring patterns in your project

---

## 🎓 Additional Resources

**Books**:
- Domain-Driven Design by Eric Evans
- Implementing Domain-Driven Design by Vaughn Vernon
- .NET Microservices Architecture by Microsoft

**Patterns**:
- CQRS Pattern (Martin Fowler)
- Repository Pattern
- Unit of Work Pattern
- Specification Pattern

**Technologies**:
- [MediatR Documentation](https://github.com/jbogard/MediatR)
- [EF Core Documentation](https://docs.microsoft.com/ef/core)
- [Dapper Documentation](https://github.com/DapperLib/Dapper)
- [FluentValidation](https://fluentvalidation.net/)

---

## 📝 License

These skills are provided as authoritative guidance for .NET Core projects using DDD + CQRS architecture. Feel free to customize for your project's needs while maintaining the core architectural principles.

---

**Version**: 1.0.0  
**Last Updated**: 2025-01-25  
**Maintained by**: Your Team
