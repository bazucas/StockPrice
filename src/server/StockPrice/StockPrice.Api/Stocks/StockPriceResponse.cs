namespace StockPrice.Api.Stocks;

/// <summary>
/// Represents a response containing stock price information for a given ticker.
/// </summary>
/// <param name="Ticker">The stock ticker symbol (e.g., AAPL for Apple)</param>
/// <param name="Price">The latest stock price associated with the ticker</param>
public sealed record StockPriceResponse(string Ticker, decimal Price);