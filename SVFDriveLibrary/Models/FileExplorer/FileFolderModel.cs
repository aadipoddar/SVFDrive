namespace SVFDriveLibrary.Models.FileExplorer;

public class FileFolderModel
{
	public bool IsFile { get; set; }
	public string Name { get; set; }
	public string FullName { get; set; }
	public string Extension { get; set; }

	public string ParentFullName { get; set; }

	public long Length { get; set; }
	public string LengthDisplay => IsFile ? FileFolderHelper.FormatLength(Length) : string.Empty;
	public bool IsReadOnly { get; set; }
	public bool Exists { get; set; }
	public FileAttributes Attributes { get; set; }

	public DateTime CreationTime { get; set; }
	public DateTime CreationTimeUtc { get; set; }
	public DateTime LastAccessTime { get; set; }
	public DateTime LastAccessTimeUtc { get; set; }
	public DateTime LastWriteTime { get; set; }
	public DateTime LastWriteTimeUtc { get; set; }
}

public static class FileFolderHelper
{
	public static string FormatLength(long length)
	{
		if (length < 1024) return $"{length} B";
		if (length < 1024 * 1024) return $"{length / 1024.0:F2} KB";
		if (length < 1024 * 1024 * 1024) return $"{length / (1024.0 * 1024.0):F2} MB";
		if (length < 1024L * 1024L * 1024L * 1024L) return $"{length / (1024.0 * 1024.0 * 1024.0):F2} GB";
		return $"{length / (1024.0 * 1024.0 * 1024.0 * 1024.0):F2} TB";
	}
}

public enum ClipboardMode
{
	None,
	Cut,
	Copy
}

public enum EditDialogMode
{
	Rename,
	NewFolder,
	NewFile
}