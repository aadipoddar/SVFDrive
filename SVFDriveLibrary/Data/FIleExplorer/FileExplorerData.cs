using SVFDriveLibrary.Data.Operations;
using SVFDriveLibrary.Models.FileExplorer;
using SVFDriveLibrary.Models.Operations;
using System.Text.Json;

namespace SVFDriveLibrary.Data.FIleExplorer;

public static class FileExplorerData
{
	public static async Task<List<FolderFileModel>> LoadFoldersFileFromAPI(string path)
	{
		var fileManagerApiBase = (await SettingsData.LoadSettingsByKey(SettingsKeys.FileManagerApiBase)).Value;
		var encodedPath = Uri.EscapeDataString(path);

		using var client = new HttpClient();
		var request = new HttpRequestMessage(HttpMethod.Get, $"{fileManagerApiBase}api/FolderFileManager?path={encodedPath}");
		using var response = await client.SendAsync(request);
		if (response.IsSuccessStatusCode)
		{
			var json = await response.Content.ReadAsStringAsync();
			var res = JsonSerializer.Deserialize<List<FolderFileModel>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
			return res;
		}

		throw new Exception($"Failed to load folders and files from API. Status code: {response.StatusCode}, Reason: {response.ReasonPhrase}");
	}
}
