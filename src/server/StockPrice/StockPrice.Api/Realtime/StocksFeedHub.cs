using Microsoft.AspNetCore.SignalR; // Provides real-time web functionality with SignalR

namespace StockPrice.Api.Realtime;

/// <summary>
/// The SignalR Hub that allows clients to join real-time stock price updates.
/// Implements the IStockUpdateClient interface, meaning that it can send updates to clients implementing this interface.
/// </summary>
internal sealed class StocksFeedHub : Hub<IStockUpdateClient>
{
    /// <summary>
    /// Allows clients to join a specific stock ticker group to receive real-time updates for that ticker.
    /// Each client connection can subscribe to updates for a given stock symbol (ticker).
    /// </summary>
    /// <param name="ticker">The stock ticker symbol (e.g., AAPL for Apple, MSFT for Microsoft) to join the group for</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task JoinStockGroup(string ticker)
    {
        // Adds the current client connection to a SignalR group based on the stock ticker symbol
        // Each ticker represents a group, and clients in that group will receive updates for that specific stock
        await Groups.AddToGroupAsync(Context.ConnectionId, ticker);
    }
};