using Dapper; // Dapper is a simple object mapper for .NET that allows executing SQL queries and mapping results to objects
using Npgsql; // Npgsql is a .NET driver for PostgreSQL, used to interact with a PostgreSQL database
using StockPrice.Api.Realtime; // Import for real-time stock updates

namespace StockPrice.Api.Stocks;

// StockService handles the logic for fetching and storing stock prices, either from the database or an external API
internal sealed class StockService(
    ActiveTickerManager activeTickerManager, // Manages active stock tickers
    NpgsqlDataSource dataSource,             // Provides the PostgreSQL data source for database interaction
    StocksClient stocksClient,               // Client to fetch stock prices from an external API
    ILogger<StockService> logger             // Logger to log information and errors
)
{
    /// <summary>
    /// Fetches the latest stock price for a given ticker. Tries to get it from the database first,
    /// and if not found, fetches it from an external API and stores it in the database.
    /// </summary>
    /// <param name="ticker">The stock ticker symbol (e.g., MSFT for Microsoft)</param>
    /// <returns>Latest stock price information or null if not found</returns>
    public async Task<StockPriceResponse?> GetLatestStockPrice(string ticker)
    {
        try
        {
            // First, try to get the latest stock price from the database
            var dbPrice = await GetLatestPriceFromDatabase(ticker);
            if (dbPrice is not null)
            {
                // If found in the database, add it to the active ticker manager
                activeTickerManager.AddTicker(ticker);
                return dbPrice;
            }

            // If not found in the database, fetch the price from the external API
            var apiPrice = await stocksClient.GetDataForTicker(ticker);

            if (apiPrice == null)
            {
                // Log a warning if no data is returned from the API
                logger.LogWarning("No data returned from external API for ticker: {Ticker}", ticker);
                return null;
            }

            // Save the newly fetched price to the database
            await SavePriceToDatabase(apiPrice);

            // Add the ticker to the active ticker manager
            activeTickerManager.AddTicker(ticker);

            // Return the fetched price
            return apiPrice;
        }
        catch (Exception ex)
        {
            // Log any exception that occurs while fetching the stock price
            logger.LogError(ex, "Error occurred while fetching stock price for ticker: {Ticker}", ticker);
            throw; // Re-throw the exception to propagate it
        }
    }

    /// <summary>
    /// Tries to get the latest stock price for a given ticker from the database.
    /// </summary>
    /// <param name="ticker">The stock ticker symbol (e.g., MSFT)</param>
    /// <returns>The latest stock price information from the database or null if not found</returns>
    private async Task<StockPriceResponse?> GetLatestPriceFromDatabase(string ticker)
    {
        // SQL query to get the latest price for a given ticker, ordered by timestamp in descending order
        const string sql =
            """
            SELECT ticker, price, timestamp
            FROM public.stock_prices
            WHERE ticker = @Ticker
            ORDER BY timestamp DESC
            LIMIT 1
            """;

        // Open a connection to the database
        await using var connection = await dataSource.OpenConnectionAsync();

        // Execute the query and map the result to a StockPriceRecord object
        var result = await connection.QueryFirstOrDefaultAsync<StockPriceRecord>(sql, new
        {
            Ticker = ticker
        });

        // If a result is found, return a new StockPriceResponse object, otherwise return null
        return result is not null ? new StockPriceResponse(result.Ticker, result.Price) : null;
    }

    /// <summary>
    /// Saves the latest stock price information to the database.
    /// </summary>
    /// <param name="price">The stock price information to save</param>
    private async Task SavePriceToDatabase(StockPriceResponse price)
    {
        // SQL query to insert a new stock price record into the database
        const string sql =
            """
            INSERT INTO public.stock_prices (ticker, price, timestamp)
            VALUES (@Ticker, @Price, @Timestamp)
            """;

        // Open a connection to the database
        await using var connection = await dataSource.OpenConnectionAsync();

        // Execute the insert query, passing the ticker, price, and current timestamp
        await connection.ExecuteAsync(sql, new
        {
            price.Ticker,
            price.Price,
            Timestamp = DateTime.UtcNow // Use the current UTC time as the timestamp
        });
    }

    // Internal record to represent a stock price retrieved from the database
    private sealed record StockPriceRecord(string Ticker, decimal Price, DateTime Timestamp);
}
