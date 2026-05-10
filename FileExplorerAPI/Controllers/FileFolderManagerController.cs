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
		try
		{
			path = await FileFolderData.ValidateRootPath(path);

			if (!System.IO.File.Exists(path) && !Directory.Exists(path))
				return NotFound($"Path not found: {path}");

			if (System.IO.File.GetAttributes(path).HasFlag(FileAttributes.Directory))
				return Ok(FileFolderData.ConvertFileFolderInfoToFileFolderModel(folderInfo: new DirectoryInfo(path)));

			return Ok(FileFolderData.ConvertFileFolderInfoToFileFolderModel(fileInfo: new FileInfo(path)));
		}
		catch (Exception ex)
		{
			return StatusCode(500, $"Error loading info: {ex.Message}");
		}
	}
	#endregion

	#region Lists
	[HttpGet]
	[Route("LoadFileFolders")]
	public async Task<IActionResult> LoadFileFolders([FromQuery] string path)
	{
		try
		{
			path = await FileFolderData.ValidateRootPath(path);

			if (!Directory.Exists(path))
				return NotFound($"Folder not found: {path}");

			return Ok(FileFolderData.LoadFileFoldersFromPath(path));
		}
		catch (Exception ex)
		{
			return StatusCode(500, $"Error loading folder: {ex.Message}");
		}
	}
	#endregion

	#region Actions
	[HttpPut]
	[Route("RenameFileFolder")]
	public async Task<IActionResult> RenameFileFolder([FromQuery] string path, [FromQuery] string newName)
	{
		try
		{
			path = await FileFolderData.ValidateRootPath(path);

			if (!System.IO.File.Exists(path) && !Directory.Exists(path))
				return NotFound($"Path not found: {path}");

			FileFolderData.RenameFileFolder(path, newName);
			return NoContent();
		}
		catch (Exception ex)
		{
			return StatusCode(500, $"Error renaming path: {ex.Message}");
		}
	}

	[HttpDelete]
	[Route("DeleteFileFolder")]
	public async Task<IActionResult> DeleteFileFolder([FromQuery] string path)
	{
		try
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
		catch (Exception ex)
		{
			return StatusCode(500, $"Error deleting path: {ex.Message}");
		}
	}
	#endregion
}