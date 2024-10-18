namespace StockPrice.Api.Realtime;

/// <summary>
/// Interface that defines the contract for receiving stock price updates.
/// Any class implementing this interface should handle receiving stock price updates in real-time.
/// </summary>
public interface IStockUpdateClient
{
    /// <summary>
    /// Method to be implemented by clients for receiving real-time stock price updates.
    /// </summary>
    /// <param name="update">The stock price update containing ticker, price, and other relevant information</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task ReceiveStockPriceUpdate(StockPriceUpdate update);
}