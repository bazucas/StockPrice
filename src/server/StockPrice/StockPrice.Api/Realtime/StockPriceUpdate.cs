namespace StockPrice.Api.Realtime;

public sealed record StockPriceUpdate(string Ticker, decimal Price);
