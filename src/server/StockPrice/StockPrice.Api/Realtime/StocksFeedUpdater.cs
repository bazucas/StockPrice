using Microsoft.AspNetCore.SignalR; // Provides real-time communication functionality with SignalR
using Microsoft.Extensions.Options; // Used to access configuration options
using StockPrice.Api.Stocks; // Imports Stock-related services

namespace StockPrice.Api.Realtime;

/// <summary>
/// Background service that periodically updates stock prices and sends real-time updates to connected clients using SignalR.
/// </summary>
internal sealed class StocksFeedUpdater(
    ActiveTickerManager activeTickerManager,                  // Manages the active stock tickers
    IServiceScopeFactory serviceScopeFactory,                 // Factory to create service scopes for scoped services like StockService
    IHubContext<StocksFeedHub, IStockUpdateClient> hubContext, // Provides access to SignalR for sending updates to clients
    IOptions<StockUpdateOptions> options,                     // Contains configuration options for stock updates (e.g., update interval)
    ILogger<StocksFeedUpdater> logger                         // Logger for logging information and errors
) : BackgroundService // Inherits from BackgroundService, so it runs in the background
{
    // Random number generator for simulating stock price changes
    private readonly Random _random = new();

    // Store the configuration options for updating stock prices
    private readonly StockUpdateOptions _options = options.Value;

    /// <summary>
    /// Main method that runs when the background service starts. It continuously updates stock prices
    /// and sends the updates to clients at a defined interval.
    /// </summary>
    /// <param name="stoppingToken">A token that signals when the service should stop</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Continue updating stock prices until the cancellation token is triggered
        while (!stoppingToken.IsCancellationRequested)
        {
            // Update stock prices and notify clients
            await UpdateStockPrices();

            // Wait for the defined interval before updating again
            await Task.Delay(_options.UpdateInterval, stoppingToken);
        }
    }

    /// <summary>
    /// Updates stock prices for all active tickers and sends the new prices to connected clients.
    /// </summary>
    private async Task UpdateStockPrices()
    {
        // Create a new scope to retrieve scoped services like StockService
        using var scope = serviceScopeFactory.CreateScope();
        var stockService = scope.ServiceProvider.GetRequiredService<StockService>();

        // Loop through all active tickers managed by ActiveTickerManager
        foreach (var ticker in activeTickerManager.GetAllTickers())
        {
            // Fetch the latest price for the current ticker
            var currentPrice = await stockService.GetLatestStockPrice(ticker);
            if (currentPrice == null)
            {
                continue; // Skip if no price is available for the ticker
            }

            // Calculate a new stock price based on the current price
            var newPrice = CalculateNewPrice(currentPrice);

            // Create a new stock price update to send to clients
            var update = new StockPriceUpdate(ticker, newPrice);

            // Notify all clients subscribed to this ticker group via SignalR
            await hubContext.Clients.Group(ticker).ReceiveStockPriceUpdate(update);

            // Log the updated price
            logger.LogInformation("Updated {Ticker} price to {Price}", ticker, newPrice);
        }
    }

    /// <summary>
    /// Calculates a new stock price by applying a random percentage change to the current price.
    /// </summary>
    /// <param name="currentPrice">The current stock price</param>
    /// <returns>The new stock price</returns>
    private decimal CalculateNewPrice(StockPriceResponse currentPrice)
    {
        // Get the maximum percentage change allowed from the configuration options
        var change = _options.MaxPercentageChange;

        // Generate a random price factor within the range [-change, change]
        var priceFactor = (decimal)((_random.NextDouble() * change * 2) - change);

        // Calculate the price change based on the current price
        var priceChange = currentPrice.Price * priceFactor;

        // Ensure the new price is non-negative, then round to two decimal places
        var newPrice = Math.Max(0, currentPrice.Price + priceChange);
        newPrice = Math.Round(newPrice, 2);

        return newPrice; // Return the new calculated price
    }
}
