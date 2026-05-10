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

	#region Lists
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
	#endregion

	#region Download Upload
	internal static async Task AppendChunkToFile(string parentPath, string name, int chunkIndex, int totalChunks, Stream chunk, CancellationToken cancellationToken)
	{
		ValidateName(name);

		if (!Directory.Exists(parentPath))
			throw new DirectoryNotFoundException($"Parent folder not found: {parentPath}");

		var destination = Path.Combine(parentPath, name);
		var mode = chunkIndex == 0 ? FileMode.Create : FileMode.Append;

		await using var fs = new FileStream(destination, mode, FileAccess.Write, FileShare.None, bufferSize: 81920, useAsync: true);
		await chunk.CopyToAsync(fs, cancellationToken);
	}

	internal static async Task StreamUploadToFile(string parentPath, string name, bool overwrite, Stream input, CancellationToken cancellationToken)
	{
		ValidateName(name);

		if (!Directory.Exists(parentPath))
			throw new DirectoryNotFoundException($"Parent folder not found: {parentPath}");

		var destination = Path.Combine(parentPath, name);

		if (!overwrite && (File.Exists(destination) || Directory.Exists(destination)))
			throw new IOException($"An item named '{name}' already exists.");

		await using var fs = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 81920, useAsync: true);
		await input.CopyToAsync(fs, cancellationToken);
	}

	internal static async Task StreamFolderAsZip(string folderPath, Stream output, CancellationToken cancellationToken)
	{
		await using var archive = await System.IO.Compression.ZipArchive.CreateAsync(
			output, System.IO.Compression.ZipArchiveMode.Create, leaveOpen: true, entryNameEncoding: null, cancellationToken);

		var rootLength = folderPath.Length + 1;
		foreach (var file in Directory.EnumerateFiles(folderPath, "*", SearchOption.AllDirectories))
		{
			cancellationToken.ThrowIfCancellationRequested();

			var entryName = file[rootLength..].Replace('\\', '/');
			var entry = archive.CreateEntry(entryName, System.IO.Compression.CompressionLevel.NoCompression);

			await using var entryStream = await entry.OpenAsync(cancellationToken);
			await using var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 81920, useAsync: true);
			await fileStream.CopyToAsync(entryStream, cancellationToken);
		}
	}
	#endregion

	#region Actions
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

	internal static void MoveFileFolder(string source, string destinationFolder)
	{
		if (!Directory.Exists(destinationFolder))
			throw new DirectoryNotFoundException($"Destination folder not found: {destinationFolder}");

		var name = Path.GetFileName(source.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
		var destination = Path.Combine(destinationFolder, name);

		if (string.Equals(source, destination, StringComparison.OrdinalIgnoreCase))
			throw new IOException("Source and destination are the same.");

		if (File.Exists(destination) || Directory.Exists(destination))
			throw new IOException($"An item named '{name}' already exists in destination.");

		if (Directory.Exists(source))
			Directory.Move(source, destination);

		else if (File.Exists(source))
			File.Move(source, destination, overwrite: false);

		else
			throw new FileNotFoundException($"Source not found: {source}");
	}

	internal static void CopyFileFolder(string source, string destinationFolder)
	{
		if (!Directory.Exists(destinationFolder))
			throw new DirectoryNotFoundException($"Destination folder not found: {destinationFolder}");

		var name = Path.GetFileName(source.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
		var destination = Path.Combine(destinationFolder, name);

		if (File.Exists(destination) || Directory.Exists(destination))
			throw new IOException($"An item named '{name}' already exists in destination.");

		if (Directory.Exists(source))
		{
			if (destination.StartsWith(source + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
				throw new IOException("Cannot copy a folder into itself.");

			CopyDirectoryRecursive(source, destination);
		}

		else if (File.Exists(source))
			File.Copy(source, destination, overwrite: false);

		else
			throw new FileNotFoundException($"Source not found: {source}");
	}

	private static void CopyDirectoryRecursive(string source, string destination)
	{
		Directory.CreateDirectory(destination);

		foreach (var file in Directory.EnumerateFiles(source))
			File.Copy(file, Path.Combine(destination, Path.GetFileName(file)), overwrite: false);

		foreach (var dir in Directory.EnumerateDirectories(source))
			CopyDirectoryRecursive(dir, Path.Combine(destination, Path.GetFileName(dir)));
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
	#endregion
}
