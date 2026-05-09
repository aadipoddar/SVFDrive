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
	public async Task<IActionResult> LoadFileFolderInfo([FromQuery] string path)
	{
		path = await FileFolderData.ValidateRootPath(path);

		if (!System.IO.File.Exists(path) && !Directory.Exists(path))
			return NotFound($"Path not found: {path}");

		if (System.IO.File.GetAttributes(path).HasFlag(FileAttributes.Directory))
			return Ok(FileFolderData.ConvertFileFolderInfoToFileFolderModel(folderInfo: new DirectoryInfo(path)));

		return Ok(FileFolderData.ConvertFileFolderInfoToFileFolderModel(fileInfo: new FileInfo(path)));
	}
	#endregion

	#region Lists
	[HttpGet]
	[Route("LoadFileFolders")]
	public async Task<IActionResult> LoadFileFolders([FromQuery] string path)
	{
		path = await FileFolderData.ValidateRootPath(path);

		if (!Directory.Exists(path))
			return NotFound($"Folder not found: {path}");

		return Ok(FileFolderData.LoadFileFoldersFromPath(path));
	}
	#endregion

	#region Actions
	[HttpDelete]
	[Route("DeleteFileFolder")]
	public async Task<IActionResult> DeleteFileFolder([FromQuery] string path)
	{
		path = await FileFolderData.ValidateRootPath(path);

		if (Directory.Exists(path))
			Directory.Delete(path, recursive: true);

		else if (System.IO.File.Exists(path))
			System.IO.File.Delete(path);

		else
			return NotFound($"Path not found: {path}");

		return NoContent();
	}
	#endregion
}