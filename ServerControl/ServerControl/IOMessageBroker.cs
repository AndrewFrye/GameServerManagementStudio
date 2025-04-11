using System.Collections.Concurrent;
using System.Threading.Channels;
using Microsoft.AspNetCore.SignalR;

namespace ServerControl;

public class IOMessageBroker
{
    private readonly Channel<string> _inputChannel = Channel.CreateUnbounded<string>();
    private readonly ConcurrentDictionary<string, IClientProxy> _clients = new();
    
    public async Task QueueInputAsync(string message)
    {
        await _inputChannel.Writer.WriteAsync(message);
    }

    public async Task<string> DequeueInputAsync(CancellationToken token)
    {
        return await _inputChannel.Reader.ReadAsync(token);
    }

    public void RegisterClient(string connectionId, IClientProxy client)
    {
        _clients[connectionId] = client;
        client.SendAsync("Registered");
    }

    public void UnregisterClient(string connectionId)
    {
        _clients.TryRemove(connectionId, out _);
    }

    public async Task BroadcastOutputAsync(string message)
    {
        Console.WriteLine($"Broadcasting message: {message}");
        Console.WriteLine($"Client count: {_clients.Count}");
        foreach (var client in _clients.Values)
        {
            await client.SendAsync("ReceiveData", message);
        }
    }
    
}