using System.Collections.Concurrent; // Provides thread-safe collections such as ConcurrentBag

namespace StockPrice.Api.Realtime;

/// <summary>
/// Manages a collection of active stock tickers in a thread-safe manner.
/// </summary>
internal sealed class ActiveTickerManager
{
    // ConcurrentBag is used here to store the active tickers in a thread-safe manner
    private readonly ConcurrentBag<string> _activeTickers = []; // Initializes an empty ConcurrentBag for tickers

    /// <summary>
    /// Adds a ticker to the active tickers collection if it is not already present.
    /// This ensures thread-safe addition.
    /// </summary>
    /// <param name="ticker">The stock ticker to add (e.g., AAPL)</param>
    public void AddTicker(string ticker)
    {
        // Check if the ticker is not already in the bag, then add it.
        // Note: ConcurrentBag does not have a native Contains method, so this check could lead to race conditions
        // in highly concurrent scenarios.
        if (!_activeTickers.Contains(ticker))
        {
            _activeTickers.Add(ticker); // Add the ticker to the collection if it's not already there
        }
    }

    /// <summary>
    /// Retrieves all the tickers in the active collection.
    /// </summary>
    /// <returns>A read-only collection of active tickers</returns>
    public IReadOnlyCollection<string> GetAllTickers()
    {
        // Convert the ConcurrentBag to an array and return as a read-only collection
        return _activeTickers.ToArray();
    }
}