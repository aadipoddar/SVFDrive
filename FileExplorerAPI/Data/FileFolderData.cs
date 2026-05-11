using SVFDriveLibrary.Data.Common;
using SVFDriveLibrary.Data.Operations;
using SVFDriveLibrary.Data.Permissions;
using SVFDriveLibrary.DataAccess;
using SVFDriveLibrary.Models.FileExplorer;
using SVFDriveLibrary.Models.Operations;
using SVFDriveLibrary.Models.Permissions;

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

	private static string NormalizeDirPath(string path) =>
		Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;

	private static async Task<bool> ValidatePermission(string path, int userId, Func<UserPermissionModel, bool> flag)
	{
		var user = await CommonData.LoadTableDataById<UserModel>(OperationNames.User, userId)
			?? throw new Exception($"User not found: {userId}");
		//if (user.Admin)
		//	return true;

		var perms = await UserPermissionData.LoadUserPermissionByUserId(userId);
		var target = NormalizeDirPath(path);

		bool Covers(UserPermissionModel p) =>
			target.StartsWith(NormalizeDirPath(p.Path), StringComparison.OrdinalIgnoreCase);

		if (perms.Any(p => p.Deny && Covers(p)))
			return false;

		return perms.Any(p => !p.Deny && flag(p) && Covers(p));
	}

	internal static Task<bool> ValidateReadPermission(string path, int userId) =>
		ValidatePermission(path, userId, _ => true);

	internal static Task<bool> ValidateWritePermission(string path, int userId) =>
		ValidatePermission(path, userId, p => p.Write);

	internal static Task<bool> ValidateDeletePermission(string path, int userId) =>
		ValidatePermission(path, userId, p => p.Delete);
	#endregion

	#region Lists
	internal static FileFolderModel ConvertFileFolderInfoToFileFolderModel(FileInfo fileInfo = null, DirectoryInfo folderInfo = null)
	{
		if (fileInfo is not null)
			return new()
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

	internal static async Task<List<FileFolderModel>> LoadFileFoldersFromPath(string path, int userId)
	{
		var dir = new DirectoryInfo(path);

		List<FileFolderModel> items = [];

		foreach (var d in dir.GetDirectories())
			items.Add(ConvertFileFolderInfoToFileFolderModel(folderInfo: d));

		foreach (var f in dir.GetFiles())
			items.Add(ConvertFileFolderInfoToFileFolderModel(fileInfo: f));

		if (items.Count == 0)
			return items;

		return await FilterFileFoldersPermissions(items, path, userId);
	}

	private static async Task<List<FileFolderModel>> FilterFileFoldersPermissions(List<FileFolderModel> items, string currentPath, int userId)
	{
		var user = await CommonData.LoadTableDataById<UserModel>(OperationNames.User, userId)
			?? throw new Exception($"User not found: {userId}");
		//if (user.Admin)
		//	return items;

		var userPermissions = await UserPermissionData.LoadUserPermissionByUserId(userId);

		var current = NormalizeDirPath(currentPath);
		var allowed = new Dictionary<string, FileFolderModel>(StringComparer.OrdinalIgnoreCase);

		// Allowed Files / Folders
		foreach (var p in userPermissions.Where(_ => !_.Deny))
		{
			var perm = NormalizeDirPath(p.Path);

			// Permission is at or above current folder → user can see everything here
			if (current.StartsWith(perm, StringComparison.OrdinalIgnoreCase))
				foreach (var item in items)
					allowed[item.FullName] = item;

			// Permission is below current folder → expose only the child on the path to it
			if (perm.StartsWith(current, StringComparison.OrdinalIgnoreCase))
				foreach (var item in items)
					if (perm.StartsWith(NormalizeDirPath(item.FullName), StringComparison.OrdinalIgnoreCase))
						allowed[item.FullName] = item;
		}

		// Denied Files / Folders
		foreach (var p in userPermissions.Where(_ => _.Deny))
		{
			var perm = NormalizeDirPath(p.Path);

			// Deny covers current folder or an ancestor → hide everything
			if (current.StartsWith(perm, StringComparison.OrdinalIgnoreCase))
				return [];

			// Deny covers an item (or anything inside it) → remove it
			foreach (var item in allowed.Values.ToList())
				if (NormalizeDirPath(item.FullName).StartsWith(perm, StringComparison.OrdinalIgnoreCase))
					allowed.Remove(item.FullName);
		}

		// Hidden Files / Folders
		var sortedAllows = userPermissions
			.Where(_ => !_.Deny)
			.OrderByDescending(_ => _.Path.Length)
			.ToList();

		foreach (var item in allowed.Values.ToList())
		{
			if (!item.Attributes.HasFlag(FileAttributes.Hidden))
				continue;

			var itemPath = NormalizeDirPath(item.FullName);
			var covering = sortedAllows.FirstOrDefault(p =>
			{
				var perm = NormalizeDirPath(p.Path);
				return itemPath.StartsWith(perm, StringComparison.OrdinalIgnoreCase)
					|| perm.StartsWith(itemPath, StringComparison.OrdinalIgnoreCase);
			});

			if (covering is null || !covering.ShowHidden)
				allowed.Remove(item.FullName);
		}

		return [.. allowed.Values];
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

	internal static async Task StreamFolderAsZip(string folderPath, int userId, Stream output, CancellationToken cancellationToken)
	{
		var user = await CommonData.LoadTableDataById<UserModel>(OperationNames.User, userId)
			?? throw new Exception($"User not found: {userId}");

		var denies = user.Admin
			? []
			: (await UserPermissionData.LoadUserPermissionByUserId(userId))
				.Where(p => p.Deny)
				.Select(p => NormalizeDirPath(p.Path))
				.ToList();

		bool IsDenied(string path)
		{
			var target = NormalizeDirPath(path);
			return denies.Any(d => target.StartsWith(d, StringComparison.OrdinalIgnoreCase));
		}

		await using var archive = await System.IO.Compression.ZipArchive.CreateAsync(
			output, System.IO.Compression.ZipArchiveMode.Create, leaveOpen: true, entryNameEncoding: null, cancellationToken);

		var rootLength = folderPath.Length + 1;
		foreach (var file in Directory.EnumerateFiles(folderPath, "*", SearchOption.AllDirectories))
		{
			cancellationToken.ThrowIfCancellationRequested();

			if (IsDenied(file))
				continue;

			var entryName = file[rootLength..].Replace('\\', '/');
			var entry = archive.CreateEntry(entryName, System.IO.Compression.CompressionLevel.NoCompression);

			await using var entryStream = await entry.OpenAsync(cancellationToken);
			await using var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 81920, useAsync: true);
			await fileStream.CopyToAsync(entryStream, cancellationToken);
		}
	}
	#endregion

	#region Actions
	internal static async Task CreateFolder(string parentPath, string name, int userId)
	{
		ValidateName(name);
		parentPath = await ValidateRootPath(parentPath);
		if (!await ValidateWritePermission(parentPath, userId))
			throw new UnauthorizedAccessException("You do not have permission to create a folder here.");

		if (!Directory.Exists(parentPath))
			throw new DirectoryNotFoundException($"Parent folder not found: {parentPath}");

		var destination = Path.Combine(parentPath, name);

		if (Directory.Exists(destination) || File.Exists(destination))
			throw new IOException($"An item named '{name}' already exists.");

		Directory.CreateDirectory(destination);
	}

	internal static async Task CreateFile(string parentPath, string name, int userId)
	{
		ValidateName(name);
		parentPath = await ValidateRootPath(parentPath);
		if (!await ValidateWritePermission(parentPath, userId))
			throw new UnauthorizedAccessException("You do not have permission to create a file here.");

		if (!Directory.Exists(parentPath))
			throw new DirectoryNotFoundException($"Parent folder not found: {parentPath}");

		var destination = Path.Combine(parentPath, name);

		if (Directory.Exists(destination) || File.Exists(destination))
			throw new IOException($"An item named '{name}' already exists.");

		using (File.Create(destination)) { }
	}

	internal static async Task MoveFileFolder(string source, string destinationFolder, int userId)
	{
		source = await ValidateRootPath(source);
		destinationFolder = await ValidateRootPath(destinationFolder);

		var sourceParent = Path.GetDirectoryName(source)
			?? throw new DirectoryNotFoundException("Cannot move: source parent not found.");

		if (!await ValidateReadPermission(source, userId))
			throw new UnauthorizedAccessException("You do not have permission to move this item.");

		if (!await ValidateDeletePermission(sourceParent, userId))
			throw new UnauthorizedAccessException("You do not have permission to move from the source folder.");

		if (!await ValidateWritePermission(destinationFolder, userId))
			throw new UnauthorizedAccessException("You do not have permission to move into the destination folder.");

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

	internal static async Task CopyFileFolder(string source, string destinationFolder, int userId)
	{
		source = await ValidateRootPath(source);
		destinationFolder = await ValidateRootPath(destinationFolder);

		if (!await ValidateWritePermission(destinationFolder, userId))
			throw new UnauthorizedAccessException("You do not have permission to copy into the destination folder.");

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

	internal static async Task RenameFileFolder(string path, string newName, int userId)
	{
		ValidateName(newName);
		path = await ValidateRootPath(path);

		if (!File.Exists(path) && !Directory.Exists(path))
			throw new FileNotFoundException($"Path not found: {path}");

		var parent = Path.GetDirectoryName(path)
			?? throw new Exception("Cannot rename: parent folder not found.");

		if (!await ValidateReadPermission(path, userId))
			throw new UnauthorizedAccessException("You do not have permission to rename this item.");

		if (!await ValidateWritePermission(parent, userId))
			throw new UnauthorizedAccessException("You do not have permission to rename this item.");

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

	internal static async Task DeleteFileFolder(string path, int userId)
	{
		path = await ValidateRootPath(path);

		if (!File.Exists(path) && !Directory.Exists(path))
			throw new FileNotFoundException($"Path not found: {path}");

		var parent = Path.GetDirectoryName(path)
			?? throw new Exception("Cannot delete: parent folder not found.");

		if (!await ValidateReadPermission(path, userId))
			throw new UnauthorizedAccessException("You do not have permission to delete this item.");

		if (!await ValidateDeletePermission(parent, userId))
			throw new UnauthorizedAccessException("You do not have permission to delete this item.");

		if (Directory.Exists(path))
			Directory.Delete(path, recursive: true);

		else if (File.Exists(path))
			File.Delete(path);
	}
	#endregion
}
