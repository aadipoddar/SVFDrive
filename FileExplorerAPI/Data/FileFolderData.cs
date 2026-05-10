using SVFDriveLibrary.Data.Operations;
using SVFDriveLibrary.Models.FileExplorer;
using SVFDriveLibrary.Models.Operations;

namespace FileExplorerAPI.Data;

public static class FileFolderData
{
	#region Validation
	internal static async Task<string> ValidateRootPath(string path)
	{
		var rootFolder = (await SettingsData.LoadSettingsByKey(SettingsKeys.MainDriveFolder)).Value;

		if (string.IsNullOrWhiteSpace(path))
			return rootFolder;

		if (!path.StartsWith(rootFolder, StringComparison.OrdinalIgnoreCase))
			return rootFolder;

		return path;
	}

	private static void ValidateName(string name)
	{
		if (string.IsNullOrWhiteSpace(name))
			throw new ArgumentException("Name cannot be empty.");

		if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || name.Contains('/') || name.Contains('\\') || name.Contains(".."))
			throw new ArgumentException($"Invalid name: '{name}'.");
	}
	#endregion

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

	internal static void CreateFolder(string parentPath, string name)
	{
		ValidateName(name);

		if (!Directory.Exists(parentPath))
			throw new DirectoryNotFoundException($"Parent folder not found: {parentPath}");

		var destination = Path.Combine(parentPath, name);

		if (Directory.Exists(destination) || File.Exists(destination))
			throw new IOException($"An item named '{name}' already exists.");

		Directory.CreateDirectory(destination);
	}

	internal static void CreateFile(string parentPath, string name)
	{
		ValidateName(name);

		if (!Directory.Exists(parentPath))
			throw new DirectoryNotFoundException($"Parent folder not found: {parentPath}");

		var destination = Path.Combine(parentPath, name);

		if (Directory.Exists(destination) || File.Exists(destination))
			throw new IOException($"An item named '{name}' already exists.");

		using (File.Create(destination)) { }
	}

	internal static void RenameFileFolder(string path, string newName)
	{
		ValidateName(newName);

		var parent = Path.GetDirectoryName(path)
			?? throw new Exception("Cannot rename: parent folder not found.");

		var destination = Path.Combine(parent, newName);

		if (File.Exists(destination) || Directory.Exists(destination))
			throw new IOException($"An item named '{newName}' already exists.");

		if (Directory.Exists(path))
			Directory.Move(path, destination);

		else if (File.Exists(path))
			File.Move(path, destination, overwrite: false);

		else
			throw new FileNotFoundException($"Path not found: {path}");
	}
}
