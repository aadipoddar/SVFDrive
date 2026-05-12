using Microsoft.AspNetCore.SignalR.Client;
using SVFDriveLibrary.DataAccess;

namespace SVFDriveLibrary.Data.FileExplorer;

public static class RelayClient
{
	private static HubConnection _connection;
	private static readonly SemaphoreSlim _gate = new(1, 1);

	public static async Task<string> InvokeAsync(string method, Dictionary<string, string> payload)
	{
		var conn = await EnsureConnected();
		var json = System.Text.Json.JsonSerializer.Serialize(payload);
		return await conn.InvokeAsync<string>("Invoke", Secrets.RelayServerId, method, json);
	}

	private static async Task<HubConnection> EnsureConnected()
	{
		if (_connection is { State: HubConnectionState.Connected })
			return _connection;

		await _gate.WaitAsync();
		try
		{
			if (_connection is { State: HubConnectionState.Connected })
				return _connection;

			if (string.IsNullOrEmpty(Secrets.RelayUrl))
				throw new Exception("RelayUrl secret is not configured.");

			_connection ??= new HubConnectionBuilder()
				.WithUrl(Secrets.RelayUrl.TrimEnd('/') + "/relay")
				.WithAutomaticReconnect()
				.Build();

			if (_connection.State != HubConnectionState.Connected)
				await _connection.StartAsync();

			return _connection;
		}
		finally { _gate.Release(); }
	}
}
