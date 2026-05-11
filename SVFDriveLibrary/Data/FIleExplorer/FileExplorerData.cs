using SVFDriveLibrary.Data.Operations;
using SVFDriveLibrary.Models.FileExplorer;
using SVFDriveLibrary.Models.Operations;
using System.Text.Json;

namespace SVFDriveLibrary.Data.FileExplorer;

public static class FileExplorerData
{
	#region API Calls
	private static async Task<string> CallAPI(HttpMethod method, string urlSuffix)
	{
		var fileManagerApiBase = (await SettingsData.LoadSettingsByKey(SettingsKeys.FileManagerApiBase)).Value
			?? throw new Exception("FileManagerApiBase setting is not configured.");

		using var client = new HttpClient();
		var request = new HttpRequestMessage(method, $"{fileManagerApiBase}api/{urlSuffix}");
		using var response = await client.SendAsync(request);

		var body = response.Content is not null ? await response.Content.ReadAsStringAsync() : string.Empty;

		if (response.IsSuccessStatusCode)
			return body;

		var message = string.IsNullOrWhiteSpace(body) ? response.ReasonPhrase : body;
		throw new Exception($"API error ({(int)response.StatusCode} {response.StatusCode}): {message}");
	}
	#endregion

	#region Info
	public static async Task<FileFolderModel> LoadFileFolderInfoFromAPI(string path, int userId)
	{
		var encodedPath = Uri.EscapeDataString(path);
		var urlSuffix = $"FileFolderManager/LoadFileFolderInfo?path={encodedPath}&userId={userId}";
		var json = await CallAPI(HttpMethod.Get, urlSuffix);
		return JsonSerializer.Deserialize<FileFolderModel>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new FileFolderModel();
	}
	#endregion

	#region Lists
	public static async Task<List<FileFolderModel>> LoadFileFoldersFromAPI(string path, int userId)
	{
		var encodedPath = Uri.EscapeDataString(path);
		var urlSuffix = $"FileFolderManager/LoadFileFolders?path={encodedPath}&userId={userId}";
		var json = await CallAPI(HttpMethod.Get, urlSuffix);
		return JsonSerializer.Deserialize<List<FileFolderModel>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
	}
	#endregion

	#region Download Upload
	public static async Task<string> GetDownloadUrl(string path, bool isFolder, int userId)
	{
		var encodedPath = Uri.EscapeDataString(path);
		var endpoint = isFolder ? "DownloadFolder" : "DownloadFile";
		var urlSuffix = $"FileFolderManager/{endpoint}?path={encodedPath}&userId={userId}";
		var fileManagerApiBase = (await SettingsData.LoadSettingsByKey(SettingsKeys.FileManagerApiBase)).Value
			?? throw new Exception("FileManagerApiBase setting is not configured.");

		return $"{fileManagerApiBase}api/{urlSuffix}";
	}
	#endregion

	#region Actions
	public static async Task CreateFolderFromAPI(string parentPath, string name, int userId)
	{
		var encodedParentPath = Uri.EscapeDataString(parentPath);
		var encodedName = Uri.EscapeDataString(name);
		var urlSuffix = $"FileFolderManager/CreateFolder?parentPath={encodedParentPath}&name={encodedName}&userId={userId}";
		await CallAPI(HttpMethod.Post, urlSuffix);
	}

	public static async Task CreateFileFromAPI(string parentPath, string name, int userId)
	{
		var encodedParentPath = Uri.EscapeDataString(parentPath);
		var encodedName = Uri.EscapeDataString(name);
		var urlSuffix = $"FileFolderManager/CreateFile?parentPath={encodedParentPath}&name={encodedName}&userId={userId}";
		await CallAPI(HttpMethod.Post, urlSuffix);
	}

	public static async Task RenameFileFolderFromAPI(string path, string newName, int userId)
	{
		var encodedPath = Uri.EscapeDataString(path);
		var encodedNewName = Uri.EscapeDataString(newName);
		var urlSuffix = $"FileFolderManager/RenameFileFolder?path={encodedPath}&newName={encodedNewName}&userId={userId}";
		await CallAPI(HttpMethod.Put, urlSuffix);
	}

	public static async Task DeleteFileFolderFromAPI(string path, int userId)
	{
		var encodedPath = Uri.EscapeDataString(path);
		var urlSuffix = $"FileFolderManager/DeleteFileFolder?path={encodedPath}&userId={userId}";
		await CallAPI(HttpMethod.Delete, urlSuffix);
	}

	public static async Task MoveFileFolderFromAPI(string source, string destinationFolder, int userId)
	{
		var urlSuffix = $"FileFolderManager/MoveFileFolder?source={Uri.EscapeDataString(source)}&destinationFolder={Uri.EscapeDataString(destinationFolder)}&userId={userId}";
		await CallAPI(HttpMethod.Put, urlSuffix);
	}

	public static async Task CopyFileFolderFromAPI(string source, string destinationFolder, int userId)
	{
		var urlSuffix = $"FileFolderManager/CopyFileFolder?source={Uri.EscapeDataString(source)}&destinationFolder={Uri.EscapeDataString(destinationFolder)}&userId={userId}";
		await CallAPI(HttpMethod.Post, urlSuffix);
	}
	#endregion
}
