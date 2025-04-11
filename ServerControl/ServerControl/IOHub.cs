using Microsoft.AspNetCore.SignalR;

namespace ServerControl;

public class IOHub : Hub
{
    private readonly IOMessageBroker _broker;

    public IOHub(IOMessageBroker broker)
    {
        _broker = broker;
    }

    public async Task SendToService(string message)
    {
        await _broker.QueueInputAsync(message);
    }

    public async Task Register()
    {
        _broker.RegisterClient(Context.ConnectionId, Clients.Client(Context.ConnectionId));
    }
    
    public async Task OnConnectedAsync()
    {
        _broker.RegisterClient(Context.ConnectionId, Clients.Client(Context.ConnectionId));
        await base.OnConnectedAsync();
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _broker.UnregisterClient(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}