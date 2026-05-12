using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;

namespace SVFDriveRelay;

public class RelayHub : Hub
{
    // serverId -> connectionId of the API host that registered under that ID.
    private static readonly ConcurrentDictionary<string, string> Servers = new();

    public Task RegisterAsServer(string serverId)
    {
        Servers[serverId] = Context.ConnectionId;
        return Task.CompletedTask;
    }

    // Client → Server RPC. Routed to the registered server, response returned to caller.
    public async Task<string> Invoke(string serverId, string method, string jsonPayload)
    {
        if (!Servers.TryGetValue(serverId, out var serverConnId))
            throw new HubException($"Server '{serverId}' is not connected.");

        return await Clients.Client(serverConnId)
            .InvokeAsync<string>("Handle", method, jsonPayload, Context.ConnectionAborted);
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        foreach (var kv in Servers)
        {
            if (kv.Value == Context.ConnectionId)
                Servers.TryRemove(kv.Key, out _);
        }
        return base.OnDisconnectedAsync(exception);
    }
}
