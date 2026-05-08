using SVFDriveLibrary.Data.Operations;
using SVFDriveLibrary.Models.FileExplorer;
using SVFDriveLibrary.Models.Operations;

namespace FileExplorerAPI.Data;

public static class FileFolderData
{
	internal static async Task<string> ValidateRootPath(string path)
	{
		var rootFolder = (await SettingsData.LoadSettingsByKey(SettingsKeys.MainDriveFolder)).Value;

		if (string.IsNullOrWhiteSpace(path))
			return rootFolder;

		if (!path.StartsWith(rootFolder, StringComparison.OrdinalIgnoreCase))
			return rootFolder;

		return path;
	}

	internal static List<FileFolderModel> LoadFileFoldersFromPath(string path)
	{
		var dir = new DirectoryInfo(path);
		var folders = dir.GetDirectories();
		var files = dir.GetFiles();

		List<FileFolderModel> items = [];

		foreach (var d in folders)
			items.Add(ConvertFileFolderInfoToFileFolderModel(folderInfo: d));

		foreach (var f in files)
			items.Add(ConvertFileFolderInfoToFileFolderModel(fileInfo: f));

		return items;
	}

	internal static FileFolderModel ConvertFileFolderInfoToFileFolderModel(FileInfo fileInfo = null, DirectoryInfo folderInfo = null)
	{
		if (fileInfo is not null)
			return new ()
			{
				IsFile = true,
				Name = fileInfo.Name,
				FullName = fileInfo.FullName,
				Extension = fileInfo.Extension,
				ParentFullName = fileInfo.Directory?.FullName ?? string.Empty,
				Length = fileInfo.Length,
				IsReadOnly = fileInfo.IsReadOnly,
				Exists = fileInfo.Exists,
				Attributes = fileInfo.Attributes,
				CreationTime = fileInfo.CreationTime,
				CreationTimeUtc = fileInfo.CreationTimeUtc,
				LastAccessTime = fileInfo.LastAccessTime,
				LastAccessTimeUtc = fileInfo.LastAccessTimeUtc,
				LastWriteTime = fileInfo.LastWriteTime,
				LastWriteTimeUtc = fileInfo.LastWriteTimeUtc
			};

		if (folderInfo is not null)
			return new()
			{
				IsFile = false,
				Name = folderInfo.Name,
				FullName = folderInfo.FullName,
				Extension = string.Empty,
				ParentFullName = folderInfo.Parent?.FullName ?? string.Empty,
				Length = 0,
				IsReadOnly = false,
				Exists = folderInfo.Exists,
				Attributes = folderInfo.Attributes,
				CreationTime = folderInfo.CreationTime,
				CreationTimeUtc = folderInfo.CreationTimeUtc,
				LastAccessTime = folderInfo.LastAccessTime,
				LastAccessTimeUtc = folderInfo.LastAccessTimeUtc,
				LastWriteTime = folderInfo.LastWriteTime,
				LastWriteTimeUtc = folderInfo.LastWriteTimeUtc
			};

		return null;
	}
}
