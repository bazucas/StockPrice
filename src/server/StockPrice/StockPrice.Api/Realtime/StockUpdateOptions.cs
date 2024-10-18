namespace StockPrice.Api.Realtime;

/// <summary>
/// Configuration options for controlling the stock price updates in the application.
/// </summary>
internal sealed class StockUpdateOptions
{
    /// <summary>
    /// Specifies how often stock prices should be updated (default is every 5 seconds).
    /// </summary>
    public TimeSpan UpdateInterval { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Specifies the maximum percentage change allowed for stock price updates (default is 2%).
    /// This defines how much the stock price can increase or decrease during each update.
    /// </summary>
    public double MaxPercentageChange { get; set; } = 0.02;
}