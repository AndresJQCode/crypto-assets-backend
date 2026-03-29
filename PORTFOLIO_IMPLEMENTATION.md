# Portfolio Feature Implementation

## ✅ Implementación Completa del Agregado Portfolio

Se ha implementado completamente la funcionalidad de **Portfolio** para gestionar capital inicial, depósitos, retiros y tracking de P&L de trading.

---

## 📁 Archivos Creados

### Domain Layer (`Domain/AggregatesModel/PortfolioAggregate/`)

1. **Portfolio.cs** - Aggregate root principal
   - Propiedades:
     - `InitialCapital`: Capital inicial del usuario
     - `CurrentBalance`: Balance actual
     - `TotalDeposits`, `TotalWithdrawals`: Tracking de movimientos
     - `TotalTradingProfit`, `TotalTradingLoss`, `TotalFees`: Métricas de trading
     - `Currency`: Moneda del portfolio (default: USDT)
     - `IsActive`: Estado del portfolio
   
   - Métodos principales:
     - `Create()`: Factory method para crear portfolio con capital inicial
     - `AddDeposit()`: Agregar depósito
     - `AddWithdrawal()`: Agregar retiro (con validación de balance)
     - `RecordTradingProfit()`: Registrar ganancia de un trade
     - `RecordTradingLoss()`: Registrar pérdida de un trade
     - `RecordFee()`: Registrar comisión
     - `UpdateInitialCapital()`: Actualizar capital inicial (solo si no hay transacciones)
     - `GetNetProfitLoss()`: Calcular P&L neto
     - `GetROI()`: Calcular retorno de inversión en %

2. **PortfolioTransaction.cs** - Entidad de transacciones
   - Registra cada movimiento del portfolio
   - Incluye: amount, balance after, type, notes, trading order reference

3. **TransactionType.cs** - Enumeration
   - `Deposit`, `Withdrawal`, `TradingProfit`, `TradingLoss`, `Fee`

4. **IPortfolioRepository.cs** - Interface del repositorio
   - `GetByUserIdAsync()`: Obtener portfolio por usuario
   - `GetWithTransactionsAsync()`: Obtener con historial completo
   - `UserHasPortfolioAsync()`: Verificar existencia

### Infrastructure Layer

5. **PortfolioEntityConfiguration.cs** - EF Core configuration
   - Tabla: `Portfolios`
   - Índices: UserId (unique), IsActive
   - Precision: 18,8 para todos los decimales

6. **PortfolioTransactionEntityConfiguration.cs** - EF Core configuration
   - Tabla: `PortfolioTransactions`
   - Índices: PortfolioId, TransactionDate, TradingOrderId

7. **PortfolioRepository.cs** - Implementación del repositorio
   - Ubicación: `Infrastructure/Repositories/`

8. **ApiContext.cs** - Actualizado
   - Agregados DbSets: `Portfolios`, `PortfolioTransactions`
   - Aplicadas configuraciones

### API Layer

#### Commands (`Api/Application/Commands/Portfolio/`)

9. **CreatePortfolioCommand** + Handler
   - Crea portfolio con capital inicial
   - Valida que el usuario no tenga un portfolio existente

10. **AddDepositCommand** + Handler
    - Agrega depósito al portfolio

11. **AddWithdrawalCommand** + Handler
    - Agrega retiro con validación de balance

12. **UpdateInitialCapitalCommand** + Handler
    - Actualiza capital inicial (solo permitido sin transacciones)

#### Queries (`Api/Application/Queries/Portfolio/`)

13. **GetUserPortfolioQuery** + Handler
    - Obtiene portfolio del usuario actual

14. **GetPortfolioTransactionsQuery** + Handler
    - Obtiene historial de transacciones ordenado por fecha

#### DTOs (`Api/Application/Dtos/Portfolio/`)

15. **PortfolioDto**
    - Incluye campos calculados: `NetProfitLoss`, `ROI`

16. **PortfolioTransactionDto**

#### Validators (`Api/Application/Validators/Portfolio/`)

17. **CreatePortfolioCommandValidator**
18. **AddDepositCommandValidator**
19. **AddWithdrawalCommandValidator**
20. **UpdateInitialCapitalCommandValidator**

#### Endpoints (`Api/Apis/Portfolio/`)

21. **PortfolioEndpoints.cs**
    - `GET /api/portfolio` - Obtener portfolio del usuario
    - `GET /api/portfolio/transactions` - Obtener historial
    - `POST /api/portfolio` - Crear portfolio
    - `POST /api/portfolio/deposit` - Agregar depósito
    - `POST /api/portfolio/withdrawal` - Agregar retiro
    - `PUT /api/portfolio/initial-capital` - Actualizar capital inicial

### Configuración

22. **Extensions.cs** - Actualizado
    - Agregado: `services.AddScoped<IPortfolioRepository, PortfolioRepository>()`

23. **Program.cs** - Actualizado
    - Agregado: `tenantGroup.MapPortfolioEndpoints()`

---

## 🚀 Pasos Siguientes (en el ambiente de desarrollo)

### 1. Crear la migración de base de datos

```bash
cd /home/andres/.openclaw/workspace/crypto-assets-backend

# Crear migración
dotnet ef migrations add AddPortfolioAggregate --project Infrastructure --startup-project Api

# Aplicar migración
dotnet ef database update --project Infrastructure --startup-project Api
```

### 2. Agregar permisos en la base de datos

Agregar los siguientes permisos al sistema (vía seed data o manualmente):

```sql
INSERT INTO "Permissions" ("Id", "Resource", "Action", "Description", "CreatedOn")
VALUES
  (gen_random_uuid(), 'Portfolio', 'Create', 'Create portfolio', NOW()),
  (gen_random_uuid(), 'Portfolio', 'Read', 'View portfolio', NOW()),
  (gen_random_uuid(), 'Portfolio', 'Update', 'Update portfolio (deposits/withdrawals)', NOW()),
  (gen_random_uuid(), 'Portfolio', 'Delete', 'Delete portfolio', NOW());
```

### 3. Asignar permisos a roles

```sql
-- Ejemplo: asignar todos los permisos de Portfolio al rol de usuario normal
INSERT INTO "PermissionRoles" ("PermissionId", "RoleId")
SELECT p."Id", r."Id"
FROM "Permissions" p
CROSS JOIN "Roles" r
WHERE p."Resource" = 'Portfolio' 
  AND r."Name" = 'User';
```

---

## 📋 Casos de Uso

### 1. Crear Portfolio con Capital Inicial

**Request:**
```http
POST /api/portfolio
Content-Type: application/json
Authorization: Bearer {token}

{
  "initialCapital": 10000,
  "currency": "USDT"
}
```

**Response:**
```json
{
  "id": "...",
  "userId": "...",
  "initialCapital": 10000,
  "currentBalance": 10000,
  "totalDeposits": 0,
  "totalWithdrawals": 0,
  "totalTradingProfit": 0,
  "totalTradingLoss": 0,
  "totalFees": 0,
  "netProfitLoss": 0,
  "roi": 0,
  "currency": "USDT",
  "isActive": true,
  "lastUpdatedAt": "2026-03-28T20:00:00Z",
  "createdOn": "2026-03-28T20:00:00Z"
}
```

### 2. Agregar Depósito

```http
POST /api/portfolio/deposit
Content-Type: application/json
Authorization: Bearer {token}

{
  "amount": 5000,
  "notes": "Depósito adicional"
}
```

### 3. Agregar Retiro

```http
POST /api/portfolio/withdrawal
Content-Type: application/json
Authorization: Bearer {token}

{
  "amount": 2000,
  "notes": "Retiro parcial"
}
```

### 4. Ver Portfolio

```http
GET /api/portfolio
Authorization: Bearer {token}
```

### 5. Ver Historial de Transacciones

```http
GET /api/portfolio/transactions
Authorization: Bearer {token}
```

### 6. Actualizar Capital Inicial (solo sin transacciones)

```http
PUT /api/portfolio/initial-capital
Content-Type: application/json
Authorization: Bearer {token}

{
  "newInitialCapital": 15000
}
```

---

## 🔐 Permisos Requeridos

Los endpoints están protegidos con los siguientes permisos:

- `Portfolio.Create` - Para crear portfolio
- `Portfolio.Read` - Para ver portfolio y transacciones
- `Portfolio.Update` - Para depósitos, retiros y actualizar capital
- `Portfolio.Delete` - Para eliminar portfolio (endpoint aún no implementado)

---

## 🎯 Características Implementadas

✅ Capital inicial configurable  
✅ Tracking automático de balance  
✅ Depósitos y retiros con validación  
✅ Historial completo de transacciones  
✅ Cálculo automático de P&L neto  
✅ Cálculo de ROI en porcentaje  
✅ Soporte para múltiples monedas  
✅ Soft delete incorporado  
✅ Audit trail (CreatedOn, LastModifiedOn)  
✅ Validación de negocio en dominio  
✅ Arquitectura DDD completa  
✅ Primary constructors (C# 12)  
✅ File-scoped namespaces  
✅ FluentValidation  

---

## 🔮 Extensiones Futuras

### 1. Integración con TradingOrder

Cuando una orden se complete, automáticamente actualizar el portfolio:

```csharp
// En TradingOrderCompletedHandler
public class TradingOrderCompletedHandler : INotificationHandler<TradingOrderCompletedEvent>
{
    private readonly IPortfolioRepository _portfolioRepository;
    
    public async Task Handle(TradingOrderCompletedEvent notification, CancellationToken ct)
    {
        var portfolio = await _portfolioRepository.GetByUserIdAsync(notification.UserId, ct);
        
        if (notification.ProfitLoss > 0)
            portfolio.RecordTradingProfit(notification.ProfitLoss, notification.TradingOrderId);
        else
            portfolio.RecordTradingLoss(Math.Abs(notification.ProfitLoss), notification.TradingOrderId);
            
        if (notification.Fee > 0)
            portfolio.RecordFee(notification.Fee, notification.TradingOrderId);
            
        await _portfolioRepository.UnitOfWork.SaveEntitiesAsync(ct);
    }
}
```

### 2. Dashboard Metrics

```csharp
// Query adicional para métricas
public record GetPortfolioMetricsQuery : IRequest<PortfolioMetricsDto>;

public class PortfolioMetricsDto
{
    public decimal TotalProfitLoss { get; set; }
    public decimal WinRate { get; set; }
    public decimal AverageWin { get; set; }
    public decimal AverageLoss { get; set; }
    public decimal SharpeRatio { get; set; }
    public List<DailyBalanceDto> BalanceHistory { get; set; }
}
```

### 3. Multiple Portfolios

Si se desea soportar múltiples portfolios por usuario (ej: una cuenta demo y una real):

- Remover índice UNIQUE de `UserId`
- Agregar campo `Name` o `Type` a Portfolio
- Actualizar queries para filtrar por tipo

---

## 🧪 Testing

### Unit Tests Sugeridos

```csharp
[Fact]
public void Create_ShouldCreatePortfolioWithInitialCapital()
{
    var portfolio = Portfolio.Create(Guid.NewGuid(), 10000, "USDT");
    
    Assert.Equal(10000, portfolio.InitialCapital);
    Assert.Equal(10000, portfolio.CurrentBalance);
    Assert.Single(portfolio.Transactions);
}

[Fact]
public void AddWithdrawal_ShouldThrowException_WhenInsufficientBalance()
{
    var portfolio = Portfolio.Create(Guid.NewGuid(), 1000, "USDT");
    
    Assert.Throws<DomainException>(() => portfolio.AddWithdrawal(2000));
}

[Fact]
public void GetROI_ShouldCalculateCorrectly()
{
    var portfolio = Portfolio.Create(Guid.NewGuid(), 10000, "USDT");
    portfolio.AddDeposit(5000); // Total invested: 15000
    portfolio.RecordTradingProfit(3000, Guid.NewGuid()); // Balance: 18000
    
    var roi = portfolio.GetROI();
    
    Assert.Equal(20, roi); // (18000 - 15000) / 15000 * 100 = 20%
}
```

---

## 📚 Recursos

- **DDD Pattern**: Aggregate Root con invariantes de negocio
- **CQRS**: Comandos para escritura, Queries para lectura
- **Repository Pattern**: Abstracción de persistencia
- **Primary Constructors**: C# 12 feature
- **FluentValidation**: Validación declarativa

---

## ✅ Checklist de Implementación

- [x] Entidad de dominio Portfolio
- [x] Entidad de dominio PortfolioTransaction
- [x] TransactionType enumeration
- [x] IPortfolioRepository interface
- [x] Entity configurations (EF Core)
- [x] Repository implementation
- [x] DbContext updates
- [x] Commands (Create, Deposit, Withdrawal, UpdateInitialCapital)
- [x] Queries (GetUserPortfolio, GetPortfolioTransactions)
- [x] DTOs
- [x] Validators
- [x] API Endpoints
- [x] Dependency Injection registration
- [x] Endpoint mapping in Program.cs
- [ ] Database migration (requiere .NET SDK)
- [ ] Permissions seed data
- [ ] Integration tests
- [ ] API documentation updates

---

**Implementado por:** Claude (OpenClaw)  
**Fecha:** 2026-03-28  
**Stack:** .NET 10, PostgreSQL, DDD, CQRS, Clean Architecture
