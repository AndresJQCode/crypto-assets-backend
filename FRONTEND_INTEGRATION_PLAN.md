# Plan de Integración Frontend-Backend

## Análisis del Frontend (crypto-assets-frontend)

### Servicios esperados por el frontend:

#### 1. Assets (Balances de Exchange)
**Tipos esperados:**
```typescript
interface Asset {
  id: string;
  exchangeId: ExchangeType; // "bybit", etc
  symbol: string; // BTC, ETH, USDT
  name: string;
  totalQuantity: number;
  availableQuantity: number;
  lockedQuantity: number;
  estimatedValueUSD?: number;
  lastUpdated: string; // ISO date
  metadata?: Record<string, unknown>;
}

interface AssetBalance {
  assets: Asset[];
  totalValueUSD: number;
  lastUpdated: string;
}
```

**Endpoints esperados:**
- `GET /assets?exchangeId={id}` → `Asset[]`
- `GET /asset-balance?exchangeId={id}` → `AssetBalance`

#### 2. Orders (Órdenes de Trading)
**Tipos esperados:**
```typescript
type OrderState = "Created" | "Submitted" | "PartiallyFilled" | "Filled" | "Cancelled" | "Rejected";
type OrderStatus = "open" | "closed";
type OrderType = "market" | "limit" | "stop_loss" | "take_profit";
type OrderSide = "buy" | "sell";

interface Order {
  id: string;
  exchangeId: ExchangeType;
  exchangeOrderId?: string;
  pair: { base: string; quote: string; symbol: string };
  type: OrderType;
  side: OrderSide;
  state: OrderState;
  quantity: number;
  filledQuantity: number;
  remainingQuantity: number;
  price?: number;
  averageFilledPrice?: number;
  totalValue?: number;
  currentPrice?: number;
  pnl?: number;
  pnlPercentage?: number;
  createdAt: string;
  updatedAt: string;
  submittedAt?: string;
  completedAt?: string;
}

interface OrderFilters {
  exchangeId?: ExchangeType;
  status?: OrderStatus; // open | closed
  state?: OrderState;
  side?: OrderSide;
  type?: OrderType;
  pair?: string;
  dateFrom?: string;
  dateTo?: string;
}
```

**Endpoints esperados:**
- `GET /orders?page=1&limit=10&filters={...}` → `PaginatedData<Order>`
- `GET /orders/{id}` → `Order`
- `GET /orders/{id}/events` → `OrderEvent[]`
- `GET /orders/{id}/trades` → `Trade[]`

#### 3. PnL Metrics
```typescript
interface PnLMetrics {
  totalPnL: number;
  realizedPnL: number;
  unrealizedPnL: number;
  winRate: number;
  totalTrades: number;
}
```

**Endpoint esperado:**
- `GET /pnl-metrics` → `PnLMetrics`

---

## Mapeo Backend Actual → Frontend

### ✅ Ya existe en el backend:

1. **TradingOrder** entity (Domain)
   - Mapea a `Order` del frontend
   - Ya tiene: symbol, side, type, status, quantity, price, etc.
   - **Diferencias:**
     - Backend usa `OrderStatus` enum (New, PartiallyFilled, Filled, Cancelled, etc.)
     - Frontend espera `state` (OrderState) y `status` ("open"|"closed")
     - Frontend espera `pair` como objeto `{ base, quote, symbol }`
     - Frontend espera `pnl` y `pnlPercentage` calculados

2. **ConnectorInstance** (Bybit)
   - Mapea a `exchangeId` del frontend

### ❌ Falta en el backend:

1. **Assets/Balances** - NO existe
   - Necesitamos obtener balances del exchange vía API
   - Cachear los resultados

2. **PnL calculations** - Parcialmente implementado
   - Portfolio tiene tracking básico de P&L
   - Falta calcular PnL por orden individual
   - Falta calcular unrealized PnL (órdenes abiertas)
   - Falta win rate

3. **Order Events/Trades** - NO implementado
   - Falta tracking de eventos de la orden
   - Falta detalle de trades ejecutados

---

## Plan de Implementación

### Fase 1: Adaptar TradingOrder para el frontend ✅

**Backend ya tiene:**
- TradingOrder entity
- ITradingOrderRepository
- Basic CRUD

**Agregar:**
1. **Query: GetOrdersWithFilters** (paginado)
   - Filtros: exchangeId, status, state, side, type, pair, dateFrom, dateTo
   - Mapeo a PaginatedData

2. **Query: GetOrderById**
   - Incluir cálculo de PnL si aplicable

3. **DTOs adaptados al frontend:**
   - `TradingOrderDto` → `Order` (frontend)
   - Agregar campo `pair` como objeto
   - Agregar `state` y `status` derivados
   - Calcular `remainingQuantity`, `pnl`, `pnlPercentage`

### Fase 2: Assets/Balances (nuevo agregado)

**Crear:**
1. **BalanceSnapshot** entity (snapshot periódico de balances)
   - UserId
   - ConnectorInstanceId
   - Symbol
   - TotalQuantity, AvailableQuantity, LockedQuantity
   - EstimatedValueUSD
   - SnapshotDate

2. **Services:**
   - `BybitBalanceService` - Llama API de Bybit para obtener balances
   - `BalanceSnapshotService` - Cachea y persiste snapshots

3. **Queries:**
   - `GetAssetsByConnectorQuery` → `Asset[]`
   - `GetAssetBalanceQuery` → `AssetBalance`

### Fase 3: PnL Metrics

**Agregar:**
1. **Query: GetPnLMetricsQuery**
   - Calcular realizedPnL (órdenes cerradas Filled)
   - Calcular unrealizedPnL (órdenes abiertas con currentPrice)
   - Calcular totalPnL
   - Calcular winRate
   - Contar totalTrades

2. **Service: PnLCalculationService**
   - Lógica de cálculo de PnL por orden
   - Obtener precios actuales de mercado (vía Bybit API)
   - Calcular unrealized PnL

### Fase 4: Order Events & Trades (opcional - puede ser futuro)

**Crear:**
1. **OrderEvent** entity
   - TradingOrderId
   - EventType (Created, Submitted, PartiallyFilled, Filled, Cancelled)
   - Timestamp
   - Metadata

2. **Trade** entity (trades ejecutados)
   - TradingOrderId
   - TradeId (del exchange)
   - Price
   - Quantity
   - Fee
   - Timestamp

---

## Priorización

### 🚀 Alta Prioridad (implementar ahora):
1. ✅ Adaptar TradingOrder queries/DTOs al formato frontend
2. ✅ Implementar GetOrdersWithFilters (paginado)
3. ✅ PnL Metrics básico

### 🔶 Media Prioridad (próxima sesión):
1. Assets/Balances (requiere llamadas a Bybit API)
2. PnL calculation service con precios en tiempo real

### 🔵 Baja Prioridad (futuro):
1. Order Events tracking
2. Trades detallados

---

## Endpoints a Implementar AHORA

### 1. Orders (adaptado al frontend)

```csharp
// GET /api/orders?page=1&limit=10&status=open&exchangeId=xxx
public record GetOrdersQuery(
    int Page = 1,
    int Limit = 10,
    Guid? ConnectorInstanceId = null,
    string? Status = null, // "open" | "closed"
    string? Side = null,
    string? Type = null,
    string? Pair = null,
    DateTime? DateFrom = null,
    DateTime? DateTo = null
) : IRequest<PaginatedData<TradingOrderDto>>;

// GET /api/orders/{id}
public record GetOrderByIdQuery(Guid OrderId) : IRequest<TradingOrderDto>;

// GET /api/pnl-metrics
public record GetPnLMetricsQuery : IRequest<PnLMetricsDto>;
```

### 2. DTOs adaptados

```csharp
public class TradingOrderDto
{
    public Guid Id { get; set; }
    public Guid ExchangeId { get; set; } // ConnectorInstanceId
    public string? ExchangeOrderId { get; set; }
    
    // Pair como objeto
    public TradingPairDto Pair { get; set; }
    
    public string Type { get; set; } // market, limit, etc
    public string Side { get; set; } // buy, sell
    public string State { get; set; } // Created, Submitted, PartiallyFilled, Filled, Cancelled, Rejected
    public string Status { get; set; } // open, closed (derivado de State)
    
    public decimal Quantity { get; set; }
    public decimal FilledQuantity { get; set; }
    public decimal RemainingQuantity { get; set; } // Quantity - FilledQuantity
    
    public decimal? Price { get; set; }
    public decimal? AverageFilledPrice { get; set; }
    public decimal? TotalValue { get; set; }
    public decimal? CurrentPrice { get; set; } // Precio de mercado actual
    public decimal? Pnl { get; set; } // Profit/Loss calculado
    public decimal? PnlPercentage { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class TradingPairDto
{
    public string Base { get; set; } // BTC
    public string Quote { get; set; } // USDT
    public string Symbol { get; set; } // BTC/USDT
}

public class PnLMetricsDto
{
    public decimal TotalPnL { get; set; }
    public decimal RealizedPnL { get; set; }
    public decimal UnrealizedPnL { get; set; }
    public decimal WinRate { get; set; }
    public int TotalTrades { get; set; }
}

public class PaginatedData<T>
{
    public List<T> Data { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public int Page { get; set; }
    public int Limit { get; set; }
}
```

---

## Siguientes Pasos

1. ✅ Implementar TradingPairDto
2. ✅ Adaptar TradingOrderDto al formato frontend
3. ✅ Implementar GetOrdersQuery con filtros
4. ✅ Implementar GetPnLMetricsQuery
5. ✅ Crear endpoints en TradingOrderEndpoints.cs
6. ✅ Actualizar Program.cs para mapear endpoints

