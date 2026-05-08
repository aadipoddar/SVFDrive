using FileExplorerAPI.Data;
using Microsoft.AspNetCore.Mvc;
using SVFDriveLibrary.Models.FileExplorer;

namespace FileExplorerAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FileFolderManagerController : ControllerBase
{
	[HttpGet]
	[Route("GetFileFolders")]
	public async Task<object> GetFileFolders([FromQuery] string path)
	{
		path = await FileFolderData.ValidateRootPath(path);
		return FileFolderData.LoadFileFoldersFromPath(path);
	}

	[HttpGet]
	[Route("GetParentFileFolders")]
	public async Task<object> GetParentFileFolders([FromQuery] string path)
	{
		var dir = new DirectoryInfo(path);
		var parentDir = dir.Parent;

		path = await FileFolderData.ValidateRootPath(parentDir.FullName);
		return FileFolderData.LoadFileFoldersFromPath(path);
	}

	[HttpGet]
	[Route("GetFileInfo")]
	public async Task<object> GetFileInfo([FromQuery] string path)
	{
		path = await FileFolderData.ValidateRootPath(path);
		
		var fileInfo = new FileInfo(path);

		return FileFolderData.ConvertFileFolderInfoToFileFolderModel(fileInfo: fileInfo);
	}

	[HttpGet]
	[Route("GetFolderInfo")]
	public async Task<object> GetFolderInfo([FromQuery] string path)
	{
		path = await FileFolderData.ValidateRootPath(path);

		var dir = new DirectoryInfo(path);

		return FileFolderData.ConvertFileFolderInfoToFileFolderModel(folderInfo: dir);
	}
}