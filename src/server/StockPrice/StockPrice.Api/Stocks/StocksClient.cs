using Microsoft.Extensions.Caching.Memory; // Provides in-memory caching functionality
using Newtonsoft.Json; // Used for JSON serialization and deserialization
using System.Globalization; // Provides information about specific cultures, used here for number formatting/parsing

namespace StockPrice.Api.Stocks;

// StocksClient is responsible for fetching stock price data from an external API and caching the results in memory
internal sealed class StocksClient(
    HttpClient httpClient,          // HttpClient for making HTTP requests to external APIs
    IConfiguration configuration,   // Configuration to retrieve settings like API keys and URLs
    IMemoryCache memoryCache,       // In-memory caching to store stock data and reduce external API calls
    ILogger<StocksClient> logger    // Logger to log informational messages and warnings
)
{
    /// <summary>
    /// Fetches stock price information for a given ticker, caching the results to optimize performance.
    /// First checks the memory cache, and if the data is not available, fetches it from an external API.
    /// </summary>
    /// <param name="ticker">The stock ticker symbol (e.g., AAPL for Apple)</param>
    /// <returns>A StockPriceResponse object if successful, otherwise null</returns>
    public async Task<StockPriceResponse?> GetDataForTicker(string ticker)
    {
        // Log the start of the process to fetch stock price information
        logger.LogInformation("Getting stock price information for {Ticker}", ticker);

        // Try to get the stock price from the memory cache or fetch it from the API if not cached
        var stockPriceResponse = await memoryCache.GetOrCreateAsync($"stocks-{ticker}", async entry =>
        {
            // Set cache expiration time to 5 minutes
            entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

            // Fetch the stock price from the external API
            return await GetStockPrice(ticker);
        });

        // Log a warning if the stock price data was not found
        if (stockPriceResponse is null)
        {
            logger.LogWarning("Failed to get stock price information for {Ticker}", ticker);
        }
        else
        {
            // Log that the stock price information was successfully retrieved
            logger.LogInformation(
                "Completed getting stock price information for {Ticker}, {@Stock}",
                ticker,
                stockPriceResponse);
        }

        // Return the stock price response (or null if not found)
        return stockPriceResponse;
    }

    /// <summary>
    /// Fetches stock price data from an external API for a given ticker.
    /// </summary>
    /// <param name="ticker">The stock ticker symbol (e.g., AAPL)</param>
    /// <returns>A StockPriceResponse object containing the latest price, or null if data is unavailable</returns>
    private async Task<StockPriceResponse?> GetStockPrice(string ticker)
    {
        // Construct the API URL using the ticker and the API key from the configuration
        var tickerDataString = await httpClient.GetStringAsync(
            $"?function=TIME_SERIES_INTRADAY&symbol={ticker}&interval=15min&apikey={configuration["Stocks:ApiKey"]}");

        // Deserialize the JSON response into an AlphaVantageData object
        var tickerData = JsonConvert.DeserializeObject<AlphaVantageData>(tickerDataString);

        // Get the most recent stock price data by fetching the first item from the TimeSeries
        var lastPrice = tickerData?.TimeSeries.FirstOrDefault().Value;

        // If the last price is found, return a new StockPriceResponse object; otherwise, return null
        return lastPrice is null ? null : new StockPriceResponse(ticker, decimal.Parse(lastPrice.High, CultureInfo.InvariantCulture));
    }
}
