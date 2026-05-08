namespace SVFDriveLibrary.Models.FileExplorer;

public class FolderFileModel
{
	public bool IsFile { get; set; }
	public string Name { get; set; }
	public string FullName { get; set; }
	public string Extension { get; set; }

	public long Length { get; set; }
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
