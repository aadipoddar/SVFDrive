using System.Text.Json;
using FileExplorerAPI.Data;
using Microsoft.AspNetCore.SignalR.Client;
using SVFDriveLibrary.DataAccess;

namespace FileExplorerAPI.Services;

public class RelayConnectionService(ILogger<RelayConnectionService> logger) : BackgroundService
{
	private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

	private HubConnection _connection;

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		var url = Secrets.RelayUrl
			?? throw new InvalidOperationException("RelayUrl secret is not configured.");
		var serverId = Secrets.RelayServerId;

		_connection = new HubConnectionBuilder()
			.WithUrl(url.TrimEnd('/') + "/relay")
			.WithAutomaticReconnect()
			.Build();

		_connection.On<string, string, string>("Handle", Dispatch);

		_connection.Reconnected += async _ =>
		{
			await _connection.InvokeAsync("RegisterAsServer", serverId, stoppingToken);
			logger.LogInformation("Re-registered with relay as {ServerId}", serverId);
		};

		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				await _connection.StartAsync(stoppingToken);
				await _connection.InvokeAsync("RegisterAsServer", serverId, stoppingToken);
				logger.LogInformation("Connected to relay {Url} as {ServerId}", url, serverId);
				break;
			}
			catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
			{
				logger.LogWarning(ex, "Failed to connect to relay; retrying in 5s");
				await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
			}
		}
	}

	public override async Task StopAsync(CancellationToken cancellationToken)
	{
		if (_connection is not null)
			await _connection.DisposeAsync();
		await base.StopAsync(cancellationToken);
	}

	private static async Task<string> Dispatch(string method, string payloadJson)
	{
		var p = JsonSerializer.Deserialize<Dictionary<string, string>>(payloadJson, JsonOptions) ?? [];

		string Get(string key) => p.TryGetValue(key, out var v) ? v : null;
		int GetInt(string key) => int.TryParse(Get(key), out var v) ? v : 0;

		switch (method)
		{
			case "LoadFileFolderInfo":
				{
					var path = await FileFolderData.ValidateRootPath(Get("path"));
					if (!File.Exists(path) && !Directory.Exists(path))
						throw new Exception($"Path not found: {path}");

					var model = File.GetAttributes(path).HasFlag(FileAttributes.Directory)
						? FileFolderData.ConvertFileFolderInfoToFileFolderModel(folderInfo: new DirectoryInfo(path))
						: FileFolderData.ConvertFileFolderInfoToFileFolderModel(fileInfo: new FileInfo(path));
					return JsonSerializer.Serialize(model);
				}
			case "LoadFileFolders":
				{
					var path = await FileFolderData.ValidateRootPath(Get("path"));
					if (!Directory.Exists(path))
						throw new Exception($"Folder not found: {path}");

					var list = await FileFolderData.LoadFileFoldersFromPath(path, GetInt("userId"));
					return JsonSerializer.Serialize(list);
				}
			case "CreateFolder":
				await FileFolderData.CreateFolder(Get("parentPath"), Get("name"), GetInt("userId"), Get("platform"));
				return "";
			case "CreateFile":
				await FileFolderData.CreateFile(Get("parentPath"), Get("name"), GetInt("userId"), Get("platform"));
				return "";
			case "RenameFileFolder":
				await FileFolderData.RenameFileFolder(Get("path"), Get("newName"), GetInt("userId"), Get("platform"));
				return "";
			case "DeleteFileFolder":
				await FileFolderData.DeleteFileFolder(Get("path"), GetInt("userId"), Get("platform"));
				return "";
			case "MoveFileFolder":
				await FileFolderData.MoveFileFolder(Get("source"), Get("destinationFolder"), GetInt("userId"), Get("platform"));
				return "";
			case "CopyFileFolder":
				await FileFolderData.CopyFileFolder(Get("source"), Get("destinationFolder"), GetInt("userId"), Get("platform"));
				return "";
			default:
				throw new Exception($"Unknown relay method: {method}");
		}
	}
}
