using FileExplorerAPI.Data;
using Microsoft.AspNetCore.Mvc;

namespace FileExplorerAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FileFolderManagerController : ControllerBase
{
	#region Info
	[HttpGet]
	[Route("LoadFileFolderInfo")]
	public async Task<object> LoadFileFolderInfo([FromQuery] string path)
	{
		try
		{
			path = await FileFolderData.ValidateRootPath(path);

			if (System.IO.File.GetAttributes(path).HasFlag(FileAttributes.Directory))
			{
				var dir = new DirectoryInfo(path);
				return FileFolderData.ConvertFileFolderInfoToFileFolderModel(folderInfo: dir);
			}

			else
			{
				var fileInfo = new FileInfo(path);
				return FileFolderData.ConvertFileFolderInfoToFileFolderModel(fileInfo: fileInfo);
			}
		}
		catch { return null; }
	}
	#endregion

	#region Lists
	[HttpGet]
	[Route("LoadFileFolders")]
	public async Task<object> LoadFileFolders([FromQuery] string path)
	{
		path = await FileFolderData.ValidateRootPath(path);
		return FileFolderData.LoadFileFoldersFromPath(path);
	}
	#endregion

	#region Actions
	[HttpDelete]
	[Route("DeleteFileFolder")]
	public async Task DeleteFileFolder([FromQuery] string path)
	{
		try
		{
			path = await FileFolderData.ValidateRootPath(path);

			if (System.IO.File.GetAttributes(path).HasFlag(FileAttributes.Directory))
				Directory.Delete(path, recursive: true);

			else
				System.IO.File.Delete(path);
		}
		catch { }
	}
	#endregion
}