# ✅ Integración Frontend Implementada

## Resumen

Se han implementado los endpoints backend necesarios para soportar el frontend `crypto-assets-frontend`, adaptando la respuesta al formato TypeScript esperado por los servicios del frontend.

---

## 📦 Endpoints Implementados

### 1. Trading Orders - `/api/orders`

#### `GET /api/orders`
**Descripción:** Lista paginada de órdenes de trading con filtros

**Query Parameters:**
- `page` (int, default: 1)
- `limit` (int, default: 10)
- `connectorInstanceId` (Guid?, optional) - Exchange ID
- `status` (string?, optional) - "open" | "closed"
- `state` (string?, optional) - "Created" | "Submitted" | "PartiallyFilled" | "Filled" | "Cancelled" | "Rejected"
- `side` (string?, optional) - "buy" | "sell"
- `type` (string?, optional) - "market" | "limit" | "stop_loss" | "take_profit"
- `pair` (string?, optional) - Symbol filter (e.g., "BTC", "BTCUSDT")
- `dateFrom` (DateTime?, optional)
- `dateTo` (DateTime?, optional)

**Response:**
```json
{
  "data": [
    {
      "id": "guid",
      "exchangeId": "guid",
      "exchangeOrderId": "bybit-12345",
      "pair": {
        "base": "BTC",
        "quote": "USDT",
        "symbol": "BTC/USDT"
      },
      "type": "limit",
      "side": "buy",
      "state": "Filled",
      "status": "closed",
      "quantity": 0.5,
      "filledQuantity": 0.5,
      "remainingQuantity": 0,
      "price": 60000,
      "averageFilledPrice": 59950,
      "totalValue": 29975,
      "currentPrice": null,
      "pnl": null,
      "pnlPercentage": null,
      "createdAt": "2026-02-05T08:00:00Z",
      "updatedAt": "2026-02-05T08:15:00Z",
      "submittedAt": "2026-02-05T08:00:00Z",
      "completedAt": "2026-02-05T08:15:00Z"
    }
  ],
  "totalCount": 45,
  "totalPages": 5,
  "page": 1,
  "limit": 10
}
```

#### `GET /api/orders/{id}`
**Descripción:** Obtener una orden específica por ID

**Response:** Same as individual order object above

---

### 2. PnL Metrics - `/api/pnl-metrics`

#### `GET /api/pnl-metrics`
**Descripción:** Métricas de Profit & Loss del usuario actual

**Response:**
```json
{
  "totalPnL": 1250.50,
  "realizedPnL": 800.00,
  "unrealizedPnL": 450.50,
  "winRate": 65.5,
  "totalTrades": 32
}
```

---

## 🔧 Implementación Técnica

### DTOs Creados

1. **TradingOrderDto** (`Api/Application/Dtos/TradingOrder/TradingOrderDto.cs`)
   - Formato compatible con frontend TypeScript `Order` interface
   - Incluye campo `pair` como objeto `{ base, quote, symbol }`
   - Mapeo de `state` (frontend) ← `OrderStatus` (backend)
   - Cálculo de `status` derivado ("open" | "closed")

2. **TradingPairDto** (`Api/Application/Dtos/TradingOrder/TradingPairDto.cs`)
   - Parse automático de símbolos: "BTCUSDT" → `{ base: "BTC", quote: "USDT", symbol: "BTC/USDT" }`
   - Soporta formatos: "BTCUSDT", "BTC/USDT", "BTC-USDT"

3. **PnLMetricsDto** (`Api/Application/Dtos/TradingOrder/PnLMetricsDto.cs`)
   - totalPnL, realizedPnL, unrealizedPnL, winRate, totalTrades

4. **PaginatedDataDto&lt;T&gt;** (`Api/Application/Dtos/Common/PaginatedDataDto.cs`)
   - Genérico para paginación
   - Compatible con frontend `PaginatedData<T>`

### Queries Implementadas

1. **GetOrdersQuery** + Handler
   - Filtrado avanzado por múltiples criterios
   - Paginación
   - Mapeo automático Backend → Frontend format
   - Ordenamiento por fecha (más reciente primero)

2. **GetOrderByIdQuery** + Handler
   - Obtener orden individual
   - Mismo mapeo que lista

3. **GetPnLMetricsQuery** + Handler
   - Cálculo de realized PnL (órdenes cerradas/filled)
   - Cálculo de unrealized PnL (órdenes abiertas) - placeholder
   - Win rate basado en órdenes profitable
   - Total de trades cerrados

### Endpoints

**TradingOrderEndpoints.cs**
- `GET /api/orders` → GetOrdersQuery
- `GET /api/orders/{id}` → GetOrderByIdQuery

**PnLMetricsEndpoints.cs**
- `GET /api/pnl-metrics` → GetPnLMetricsQuery

Ambos grupos mapeados en `Program.cs`:
```csharp
tenantGroup.MapTradingOrderEndpoints();
tenantGroup.MapPnLMetricsEndpoints();
```

---

## 🔄 Mapeo Backend ↔ Frontend

### OrderStatus (Backend) → State (Frontend)

| Backend OrderStatus | Frontend State |
|---------------------|----------------|
| New                 | Created        |
| PartiallyFilled     | PartiallyFilled|
| Filled              | Filled         |
| Cancelled           | Cancelled      |
| Rejected            | Rejected       |
| Untriggered         | Created        |
| Triggered           | Submitted      |
| Deactivated         | Cancelled      |

### Status Derivado (Frontend)

- **"open"**: New, PartiallyFilled, Untriggered, Triggered
- **"closed"**: Filled, Cancelled, Rejected, Deactivated

### OrderType Mapping

| Backend          | Frontend      |
|------------------|---------------|
| Market           | market        |
| Limit            | limit         |
| StopLoss         | stop_loss     |
| TakeProfit       | take_profit   |
| StopLossLimit    | stop_loss     |
| TakeProfitLimit  | take_profit   |

### OrderSide Mapping

| Backend | Frontend |
|---------|----------|
| Buy     | buy      |
| Sell    | sell     |

---

## ⚠️ Pendientes / Limitaciones Actuales

### 1. Cálculo de PnL

**Implementado:**
- ✅ Realized PnL básico (basado en fees)
- ✅ Win rate estimado
- ✅ Total trades count

**Pendiente:**
- ⚠️ Cálculo real de PnL por orden (requiere matching buy/sell pairs)
- ⚠️ Unrealized PnL (requiere precios de mercado en tiempo real)
- ⚠️ Campo `currentPrice` en TradingOrderDto (requiere integración con precio de mercado)
- ⚠️ Campos `pnl` y `pnlPercentage` por orden

**Solución futura:**
Crear un servicio `IMarketDataService` que obtenga precios actuales de Bybit API y calcule PnL real.

### 2. Assets/Balances

**NO IMPLEMENTADO AÚN**

Los endpoints esperados por el frontend:
- `GET /api/assets?exchangeId={id}`
- `GET /api/asset-balance?exchangeId={id}`

Requieren:
1. Llamar a Bybit API para obtener balances
2. Cachear resultados
3. Posiblemente crear un agregado `BalanceSnapshot`

### 3. Order Events & Trades

**NO IMPLEMENTADO AÚN**

Endpoints esperados:
- `GET /api/orders/{id}/events`
- `GET /api/orders/{id}/trades`

Requieren:
1. Crear entidad `OrderEvent` (tracking de cambios de estado)
2. Crear entidad `Trade` (trades ejecutados)
3. Poblar estos datos al sincronizar con Bybit

---

## 🚀 Próximos Pasos

### Alta Prioridad
1. **Market Data Service**
   - Integrar con Bybit API para obtener precios actuales
   - Calcular `currentPrice`, `pnl`, `pnlPercentage` en tiempo real
   - Mejorar cálculo de `GetPnLMetricsQuery`

2. **Assets/Balances Endpoints**
   - Crear servicios para obtener balances de Bybit
   - Implementar cache (Redis/Memory)
   - Exponer endpoints `/api/assets` y `/api/asset-balance`

### Media Prioridad
3. **Order Events Tracking**
   - Crear agregado OrderEvent
   - Registrar eventos al sincronizar con Bybit
   - Endpoint `/api/orders/{id}/events`

4. **Trades Detallados**
   - Crear agregado Trade
   - Obtener trades de Bybit API
   - Endpoint `/api/orders/{id}/trades`

### Baja Prioridad
5. **Real-time Updates**
   - WebSockets para actualizaciones en tiempo real
   - SignalR para notificaciones de cambios de estado

---

## 🧪 Testing

### Endpoints para probar

```bash
# 1. Get all orders (paginated)
GET http://localhost:5000/api/orders?page=1&limit=10

# 2. Get open orders only
GET http://localhost:5000/api/orders?status=open

# 3. Get buy orders
GET http://localhost:5000/api/orders?side=buy

# 4. Get BTC orders
GET http://localhost:5000/api/orders?pair=BTC

# 5. Get order by ID
GET http://localhost:5000/api/orders/{orderId}

# 6. Get PnL metrics
GET http://localhost:5000/api/pnl-metrics
```

### Permisos requeridos

Agregar permiso `TradingOrders.Read` al rol del usuario:

```sql
INSERT INTO "Permissions" ("Id", "Resource", "Action", "Description", "CreatedOn")
VALUES (gen_random_uuid(), 'TradingOrders', 'Read', 'View trading orders', NOW());

-- Asignar a rol User
INSERT INTO "PermissionRoles" ("PermissionId", "RoleId")
SELECT p."Id", r."Id"
FROM "Permissions" p
CROSS JOIN "Roles" r
WHERE p."Resource" = 'TradingOrders' 
  AND p."Action" = 'Read'
  AND r."Name" = 'User';
```

---

## 📚 Archivos Modificados/Creados

### Nuevos archivos:

**DTOs:**
- `Api/Application/Dtos/TradingOrder/TradingOrderDto.cs`
- `Api/Application/Dtos/TradingOrder/TradingPairDto.cs`
- `Api/Application/Dtos/TradingOrder/PnLMetricsDto.cs`
- `Api/Application/Dtos/Common/PaginatedDataDto.cs`

**Queries:**
- `Api/Application/Queries/TradingOrder/GetOrdersQuery.cs`
- `Api/Application/Queries/TradingOrder/GetOrdersQueryHandler.cs`
- `Api/Application/Queries/TradingOrder/GetOrderByIdQuery.cs`
- `Api/Application/Queries/TradingOrder/GetOrderByIdQueryHandler.cs`
- `Api/Application/Queries/TradingOrder/GetPnLMetricsQuery.cs`
- `Api/Application/Queries/TradingOrder/GetPnLMetricsQueryHandler.cs`

**Endpoints:**
- `Api/Apis/TradingOrder/TradingOrderEndpoints.cs`

### Archivos modificados:
- `Api/Program.cs` - Registro de endpoints
- `Api/Application/Queries/BybitQueries/GetOpenOrdersQueryHandler.cs` - Fix namespace conflict

---

## ✅ Checklist de Implementación

- [x] DTOs adaptados al formato frontend
- [x] TradingPairDto parser
- [x] GetOrdersQuery con filtros múltiples
- [x] GetOrderByIdQuery
- [x] GetPnLMetricsQuery (básico)
- [x] Paginación compatible con frontend
- [x] Mapeo OrderStatus ↔ State/Status
- [x] Endpoints REST creados
- [x] Endpoints registrados en Program.cs
- [x] Compilación exitosa
- [ ] Permisos en base de datos
- [ ] Testing manual de endpoints
- [ ] Market data service (PnL real)
- [ ] Assets/Balances endpoints
- [ ] Order Events tracking
- [ ] Trades detallados

---

**Fecha:** 2026-03-28  
**Estado:** ✅ Compilado y listo para testing  
**Próximo paso:** Agregar permisos y probar endpoints
