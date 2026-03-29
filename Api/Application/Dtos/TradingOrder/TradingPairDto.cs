namespace Api.Application.Dtos.TradingOrder;

/// <summary>
/// Trading pair data transfer object.
/// Represents a cryptocurrency trading pair (e.g., BTC/USDT).
/// </summary>
public class TradingPairDto
{
    /// <summary>
    /// Base currency (e.g., BTC).
    /// </summary>
    public string Base { get; set; } = default!;

    /// <summary>
    /// Quote currency (e.g., USDT).
    /// </summary>
    public string Quote { get; set; } = default!;

    /// <summary>
    /// Trading pair symbol (e.g., BTC/USDT or BTCUSDT).
    /// </summary>
    public string Symbol { get; set; } = default!;

    /// <summary>
    /// Parse a symbol string into a TradingPairDto.
    /// Supports formats: "BTCUSDT", "BTC/USDT", "BTC-USDT".
    /// </summary>
    public static TradingPairDto Parse(string symbol)
    {
        // Normalize symbol (remove separators)
        var normalizedSymbol = symbol.Replace("/", "").Replace("-", "").ToUpperInvariant();

        // Common quote currencies to try
        string[] quotestrings = ["USDT", "USDC", "USD", "BUSD", "BTC", "ETH"];

        foreach (var quote in quotestrings)
        {
            if (normalizedSymbol.EndsWith(quote, StringComparison.Ordinal))
            {
                var baseSymbol = normalizedSymbol[..^quote.Length];
                return new TradingPairDto
                {
                    Base = baseSymbol,
                    Quote = quote,
                    Symbol = $"{baseSymbol}/{quote}"
                };
            }
        }

        // If no match, assume last 4 characters are quote
        if (normalizedSymbol.Length > 4)
        {
            var baseSymbol = normalizedSymbol[..^4];
            var quoteSymbol = normalizedSymbol[^4..];
            return new TradingPairDto
            {
                Base = baseSymbol,
                Quote = quoteSymbol,
                Symbol = $"{baseSymbol}/{quoteSymbol}"
            };
        }

        // Fallback: return as-is
        return new TradingPairDto
        {
            Base = normalizedSymbol,
            Quote = "",
            Symbol = normalizedSymbol
        };
    }
}
