using SVFDriveLibrary.Data.Operations;
using SVFDriveLibrary.Models.FileExplorer;
using SVFDriveLibrary.Models.Operations;
using System.Text.Json;

namespace SVFDriveLibrary.Data.FileExplorer;

public static class FileExplorerData
{
	public static async Task<List<FileFolderModel>> LoadFileFoldersFromAPI(string path)
	{
		var fileManagerApiBase = (await SettingsData.LoadSettingsByKey(SettingsKeys.FileManagerApiBase)).Value;
		var encodedPath = Uri.EscapeDataString(path);

		using var client = new HttpClient();
		var request = new HttpRequestMessage(HttpMethod.Get, $"{fileManagerApiBase}api/FileFolderManager/GetFileFolders?path={encodedPath}");
		using var response = await client.SendAsync(request);
		if (response.IsSuccessStatusCode)
		{
			var json = await response.Content.ReadAsStringAsync();
			return JsonSerializer.Deserialize<List<FileFolderModel>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
		}

		throw new Exception($"Failed to load folders and files from API. Status code: {response.StatusCode}, Reason: {response.ReasonPhrase}");
	}

	public static async Task<List<FileFolderModel>> LoadParentFileFoldersFromAPI(string path)
	{
		var fileManagerApiBase = (await SettingsData.LoadSettingsByKey(SettingsKeys.FileManagerApiBase)).Value;
		var encodedPath = Uri.EscapeDataString(path);

		using var client = new HttpClient();
		var request = new HttpRequestMessage(HttpMethod.Get, $"{fileManagerApiBase}api/FileFolderManager/GetParentFileFolders?path={encodedPath}");
		using var response = await client.SendAsync(request);
		if (response.IsSuccessStatusCode)
		{
			var json = await response.Content.ReadAsStringAsync();
			return JsonSerializer.Deserialize<List<FileFolderModel>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
		}

		throw new Exception($"Failed to load folders and files from API. Status code: {response.StatusCode}, Reason: {response.ReasonPhrase}");
	}

	public static async Task<FileFolderModel> GetFileInfoFromAPI(string path)
	{
		var fileManagerApiBase = (await SettingsData.LoadSettingsByKey(SettingsKeys.FileManagerApiBase)).Value;
		var encodedPath = Uri.EscapeDataString(path);

		using var client = new HttpClient();
		var request = new HttpRequestMessage(HttpMethod.Get, $"{fileManagerApiBase}api/FileFolderManager/GetFileInfo?path={encodedPath}");
		using var response = await client.SendAsync(request);
		if (response.IsSuccessStatusCode)
		{
			var json = await response.Content.ReadAsStringAsync();
			return JsonSerializer.Deserialize<FileFolderModel>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new FileFolderModel();
		}

		throw new Exception($"Failed to load file info from API. Status code: {response.StatusCode}, Reason: {response.ReasonPhrase}");
	}

	public static async Task<FileFolderModel> GetFolderInfoFromAPI(string path)
	{
		var fileManagerApiBase = (await SettingsData.LoadSettingsByKey(SettingsKeys.FileManagerApiBase)).Value;
		var encodedPath = Uri.EscapeDataString(path);

		using var client = new HttpClient();
		var request = new HttpRequestMessage(HttpMethod.Get, $"{fileManagerApiBase}api/FileFolderManager/GetFolderInfo?path={encodedPath}");
		using var response = await client.SendAsync(request);
		if (response.IsSuccessStatusCode)
		{
			var json = await response.Content.ReadAsStringAsync();
			return JsonSerializer.Deserialize<FileFolderModel>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new FileFolderModel();
		}

		throw new Exception($"Failed to load folder info from API. Status code: {response.StatusCode}, Reason: {response.ReasonPhrase}");
	}
}
