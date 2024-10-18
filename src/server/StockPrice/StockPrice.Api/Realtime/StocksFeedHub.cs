using Microsoft.AspNetCore.SignalR;

namespace StockPrice.Api.Realtime;

internal sealed class StocksFeedHub : Hub<IStockUpdateClient>
{
    public async Task JoinStockGroup(string ticker)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, ticker);
    }
};
