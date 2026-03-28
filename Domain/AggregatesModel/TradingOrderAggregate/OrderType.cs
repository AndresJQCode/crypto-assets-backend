namespace Domain.AggregatesModel.TradingOrderAggregate;

/// <summary>
/// Trading order types supported by exchanges.
/// </summary>
public enum OrderType
{
    None = 0,
    Market = 1,
    Limit = 2,
    StopLoss = 3,
    TakeProfit = 4,
    StopLossLimit = 5,
    TakeProfitLimit = 6
}
