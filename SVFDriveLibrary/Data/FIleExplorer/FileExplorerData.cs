using SVFDriveLibrary.Data.Operations;
using SVFDriveLibrary.Models.FileExplorer;
using SVFDriveLibrary.Models.Operations;
using System.Text.Json;

namespace SVFDriveLibrary.Data.FileExplorer;

public static class FileExplorerData
{
	private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

	#region Info
	public static async Task<FileFolderModel> LoadFileFolderInfoFromAPI(string path, int userId)
	{
		var json = await RelayClient.InvokeAsync("LoadFileFolderInfo", new()
		{
			["path"] = path,
			["userId"] = userId.ToString()
		});
		return JsonSerializer.Deserialize<FileFolderModel>(json, JsonOptions) ?? new FileFolderModel();
	}
	#endregion

	#region Lists
	public static async Task<List<FileFolderModel>> LoadFileFoldersFromAPI(string path, int userId)
	{
		var json = await RelayClient.InvokeAsync("LoadFileFolders", new()
		{
			["path"] = path,
			["userId"] = userId.ToString()
		});
		return JsonSerializer.Deserialize<List<FileFolderModel>>(json, JsonOptions) ?? [];
	}
	#endregion

	#region Download
	// Upload (Syncfusion multipart) and Download (browser stream / range) still go over direct HTTP.
	// They need streaming refactors before they can move onto the relay.
	public static async Task<string> GetDownloadUrl(string path, bool isFolder, int userId, string platform)
	{
		var encodedPath = Uri.EscapeDataString(path);
		var encodedPlatform = Uri.EscapeDataString(platform);
		var endpoint = isFolder ? "DownloadFolder" : "DownloadFile";
		var urlSuffix = $"FileFolderManager/{endpoint}?path={encodedPath}&userId={userId}&platform={encodedPlatform}";
		var fileManagerApiBase = (await SettingsData.LoadSettingsByKey(SettingsKeys.FileManagerApiBase)).Value
			?? throw new Exception("FileManagerApiBase setting is not configured.");

		return $"{fileManagerApiBase}api/{urlSuffix}";
	}
	#endregion

	#region Actions
	public static Task CreateFolderFromAPI(string parentPath, string name, int userId, string platform)
		=> RelayClient.InvokeAsync("CreateFolder", new()
		{
			["parentPath"] = parentPath,
			["name"] = name,
			["userId"] = userId.ToString(),
			["platform"] = platform
		});

	public static Task CreateFileFromAPI(string parentPath, string name, int userId, string platform)
		=> RelayClient.InvokeAsync("CreateFile", new()
		{
			["parentPath"] = parentPath,
			["name"] = name,
			["userId"] = userId.ToString(),
			["platform"] = platform
		});

	public static Task RenameFileFolderFromAPI(string path, string newName, int userId, string platform)
		=> RelayClient.InvokeAsync("RenameFileFolder", new()
		{
			["path"] = path,
			["newName"] = newName,
			["userId"] = userId.ToString(),
			["platform"] = platform
		});

	public static Task DeleteFileFolderFromAPI(string path, int userId, string platform)
		=> RelayClient.InvokeAsync("DeleteFileFolder", new()
		{
			["path"] = path,
			["userId"] = userId.ToString(),
			["platform"] = platform
		});

	public static Task MoveFileFolderFromAPI(string source, string destinationFolder, int userId, string platform)
		=> RelayClient.InvokeAsync("MoveFileFolder", new()
		{
			["source"] = source,
			["destinationFolder"] = destinationFolder,
			["userId"] = userId.ToString(),
			["platform"] = platform
		});

	public static Task CopyFileFolderFromAPI(string source, string destinationFolder, int userId, string platform)
		=> RelayClient.InvokeAsync("CopyFileFolder", new()
		{
			["source"] = source,
			["destinationFolder"] = destinationFolder,
			["userId"] = userId.ToString(),
			["platform"] = platform
		});
	#endregion
}
