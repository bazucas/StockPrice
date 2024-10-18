using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using StockPrice.Api.Stocks;

namespace StockPrice.Api.Realtime;

internal sealed class StocksFeedUpdater(
    ActiveTickerManager activeTickerManager,
    IServiceScopeFactory serviceScopeFactory,
    IHubContext<StocksFeedHub, IStockUpdateClient> hubContext,
    IOptions<StockUpdateOptions> options,
    ILogger<StocksFeedUpdater> logger)
    : BackgroundService
{
    private readonly Random _random = new();
    private readonly StockUpdateOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await UpdateStockPrices();

            await Task.Delay(_options.UpdateInterval, stoppingToken);
        }
    }

    private async Task UpdateStockPrices()
    {
        using var scope = serviceScopeFactory.CreateScope();
        var stockService = scope.ServiceProvider.GetRequiredService<StockService>();

        foreach (var ticker in activeTickerManager.GetAllTickers())
        {
            var currentPrice = await stockService.GetLatestStockPrice(ticker);
            if (currentPrice == null)
            {
                continue;
            }

            var newPrice = CalculateNewPrice(currentPrice);

            var update = new StockPriceUpdate(ticker, newPrice);

            await hubContext.Clients.Group(ticker).ReceiveStockPriceUpdate(update);

            logger.LogInformation("Updated {Ticker} price to {Price}", ticker, newPrice);
        }
    }

    private decimal CalculateNewPrice(StockPriceResponse currentPrice)
    {
        var change = _options.MaxPercentageChange;
        var priceFactor = (decimal)((_random.NextDouble() * change * 2) - change);
        var priceChange = currentPrice.Price * priceFactor;
        var newPrice = Math.Max(0, currentPrice.Price + priceChange);
        newPrice = Math.Round(newPrice, 2);
        return newPrice;
    }
}
