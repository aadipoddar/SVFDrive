using Microsoft.AspNetCore.Mvc;
using SVFDriveLibrary.Models.FileExplorer;

namespace FileExplorerAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FolderFileManagerController : ControllerBase
{
	[HttpGet]
	public async Task<object> GetFoldersFiles([FromQuery] string path)
	{
		var dir = new DirectoryInfo(path);
		var folders = dir.GetDirectories();
		var files = dir.GetFiles();

		List<FolderFileModel> items = [];

		foreach (var d in folders)
			items.Add(new()
			{
				IsFile = false,
				Name = d.Name,
				FullName = d.FullName,
				Extension = string.Empty,
				Length = 0,
				IsReadOnly = false,
				Exists = d.Exists,
				Attributes = d.Attributes,
				CreationTime = d.CreationTime,
				CreationTimeUtc = d.CreationTimeUtc,
				LastAccessTime = d.LastAccessTime,
				LastAccessTimeUtc = d.LastAccessTimeUtc,
				LastWriteTime = d.LastWriteTime,
				LastWriteTimeUtc = d.LastWriteTimeUtc
			});

		foreach (var f in files)
			items.Add(new()
			{
				IsFile = true,
				Name = f.Name,
				FullName = f.FullName,
				Extension = f.Extension,
				Length = f.Length,
				IsReadOnly = f.IsReadOnly,
				Exists = f.Exists,
				Attributes = f.Attributes,
				CreationTime = f.CreationTime,
				CreationTimeUtc = f.CreationTimeUtc,
				LastAccessTime = f.LastAccessTime,
				LastAccessTimeUtc = f.LastAccessTimeUtc,
				LastWriteTime = f.LastWriteTime,
				LastWriteTimeUtc = f.LastWriteTimeUtc
			});

		return items;
	}
}