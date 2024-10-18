namespace StockPrice.Api.Realtime;

/// <summary>
/// Represents a real-time stock price update.
/// This is a record type, which means it's an immutable data structure used to transfer stock price updates.
/// </summary>
/// <param name="Ticker">The stock ticker symbol (e.g., AAPL for Apple, MSFT for Microsoft)</param>
/// <param name="Price">The updated stock price for the given ticker</param>
public sealed record StockPriceUpdate(string Ticker, decimal Price);