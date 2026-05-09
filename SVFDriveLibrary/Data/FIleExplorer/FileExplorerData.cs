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
		if (response is not null && response.IsSuccessStatusCode)
		{
			var json = await response.Content.ReadAsStringAsync();
			return json;
		}
		throw new Exception($"Failed to load data from API. Status code: {response.StatusCode}, Reason: {response.ReasonPhrase}");
	}
	#endregion

	#region Info
	public static async Task<FileFolderModel> LoadFileFolderInfoFromAPI(string path)
	{
		var encodedPath = Uri.EscapeDataString(path);
		var urlSuffix = $"FileFolderManager/LoadFileFolderInfo?path={encodedPath}";
		var json = await CallAPI(HttpMethod.Get, urlSuffix);
		return JsonSerializer.Deserialize<FileFolderModel>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new FileFolderModel();
	}
	#endregion

	#region Lists
	public static async Task<List<FileFolderModel>> LoadFileFoldersFromAPI(string path)
	{
		var encodedPath = Uri.EscapeDataString(path);
		var urlSuffix = $"FileFolderManager/LoadFileFolders?path={encodedPath}";
		var json = await CallAPI(HttpMethod.Get, urlSuffix);
		return JsonSerializer.Deserialize<List<FileFolderModel>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
	}
	#endregion

	#region Actions
	public static async Task DeleteFileFolderFromAPI(string path)
	{
		var encodedPath = Uri.EscapeDataString(path);
		var urlSuffix = $"FileFolderManager/DeleteFileFolder?path={encodedPath}";
		await CallAPI(HttpMethod.Delete, urlSuffix);
	}
	#endregion
}
