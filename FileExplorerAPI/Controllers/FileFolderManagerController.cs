using FileExplorerAPI.Data;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using SVFDriveLibrary.Data.Operations;
using SVFDriveLibrary.DataAccess;
using SVFDriveLibrary.Models.Operations;

namespace FileExplorerAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FileFolderManagerController : ControllerBase
{
	#region Info
	[HttpGet]
	[Route("LoadFileFolderInfo")]
	public async Task<IActionResult> LoadFileFolderInfo([FromQuery] string path, [FromQuery] int userId)
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
		catch (Exception ex) { return StatusCode(500, $"Error loading info: {ex.Message}"); }
	}
	#endregion

	#region Lists
	[HttpGet]
	[Route("LoadFileFolders")]
	public async Task<IActionResult> LoadFileFolders([FromQuery] string path, [FromQuery] int userId)
	{
		try
		{
			path = await FileFolderData.ValidateRootPath(path);

			if (!Directory.Exists(path))
				return NotFound($"Folder not found: {path}");

			return Ok(await FileFolderData.LoadFileFoldersFromPath(path, userId));
		}
		catch (Exception ex) { return StatusCode(500, $"Error loading folder: {ex.Message}"); }
	}
	#endregion

	#region Download Upload
	[HttpPost]
	[Route("UploadFile")]
	[DisableRequestSizeLimit]
	[RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue, ValueLengthLimit = int.MaxValue)]
	public async Task<IActionResult> UploadFile([FromQuery] string parentPath, [FromQuery] int userId, [FromQuery] string platform)
	{
		try
		{
			parentPath = await FileFolderData.ValidateRootPath(parentPath);

			if (!await FileFolderData.ValidateWritePermission(parentPath, userId))
				return StatusCode(403, "You do not have permission to upload here.");

			var form = await Request.ReadFormAsync(HttpContext.RequestAborted);
			var file = form.Files.FirstOrDefault()
				?? throw new ArgumentException("No file in form.");

			int.TryParse(form["chunk-index"].FirstOrDefault(), out var chunkIndex);
			int.TryParse(form["total-chunk"].FirstOrDefault(), out var totalChunks);
			if (totalChunks == 0) totalChunks = 1;

			await using var stream = file.OpenReadStream();
			await FileFolderData.AppendChunkToFile(parentPath, file.FileName, chunkIndex, totalChunks, stream, HttpContext.RequestAborted);

			// Audit once, on the final chunk
			if (chunkIndex + 1 >= totalChunks)
				await AuditTrailData.SaveAuditTrail(new()
				{
					Action = AuditTrailActionTypes.Upload.ToString(),
					TableName = OperationNames.FileFolder,
					RecordNo = file.FileName,
					RecordValue = Path.Combine(parentPath, file.FileName),
					CreatedBy = userId,
					CreatedFromPlatform = platform
				});

			return Ok();
		}
		catch (Exception ex) { return StatusCode(500, $"Error uploading file: {ex.Message}"); }
	}

	[HttpPost]
	[Route("RemoveUploadedFile")]
	public async Task<IActionResult> RemoveUploadedFile([FromQuery] string parentPath, [FromQuery] int userId)
	{
		try
		{
			parentPath = await FileFolderData.ValidateRootPath(parentPath);

			if (!await FileFolderData.ValidateWritePermission(parentPath, userId))
				return StatusCode(403, "You do not have permission to remove uploads here.");

			var form = await Request.ReadFormAsync(HttpContext.RequestAborted);
			var name = form.Files.FirstOrDefault()?.FileName ?? form["filename"].FirstOrDefault();

			if (string.IsNullOrWhiteSpace(name)) return BadRequest("No filename.");

			var target = Path.Combine(parentPath, name);
			if (System.IO.File.Exists(target))
				System.IO.File.Delete(target);

			return Ok();
		}
		catch (Exception ex) { return StatusCode(500, $"Error removing: {ex.Message}"); }
	}

	[HttpGet]
	[Route("DownloadFile")]
	public async Task<IActionResult> DownloadFile([FromQuery] string path, [FromQuery] int userId, [FromQuery] string platform)
	{
		try
		{
			path = await FileFolderData.ValidateRootPath(path);

			if (!System.IO.File.Exists(path))
				return NotFound($"File not found: {path}");

			if (!await FileFolderData.ValidateReadPermission(path, userId))
				return StatusCode(403, "You do not have permission to download this file.");

			var fileName = Path.GetFileName(path);

			// Audit only on the initial request — range-continuation requests skip
			if (string.IsNullOrEmpty(Request.Headers.Range.ToString()))
				await AuditTrailData.SaveAuditTrail(new()
				{
					Action = AuditTrailActionTypes.Download.ToString(),
					TableName = OperationNames.FileFolder,
					RecordNo = fileName,
					RecordValue = path,
					CreatedBy = userId,
					CreatedFromPlatform = platform
				});

			return PhysicalFile(path, "application/octet-stream", fileName, enableRangeProcessing: true);
		}
		catch (Exception ex) { return StatusCode(500, $"Error downloading file: {ex.Message}"); }
	}

	[HttpGet]
	[Route("DownloadFolder")]
	public async Task<IActionResult> DownloadFolder([FromQuery] string path, [FromQuery] int userId, [FromQuery] string platform)
	{
		string validatedPath;
		try
		{
			validatedPath = await FileFolderData.ValidateRootPath(path);
		}
		catch (Exception ex) { return StatusCode(500, $"Error validating path: {ex.Message}"); }

		if (!Directory.Exists(validatedPath))
			return NotFound($"Folder not found: {validatedPath}");

		if (!await FileFolderData.ValidateReadPermission(validatedPath, userId))
			return StatusCode(403, "You do not have permission to download this folder.");

		// ZipArchiveEntry's internal data-descriptor flush on dispose still calls sync Write.
		// Async APIs reduce blocking but don't eliminate it — opt in for this endpoint only.
		var bodyControl = HttpContext.Features.Get<IHttpBodyControlFeature>();
		bodyControl?.AllowSynchronousIO = true;

		var folderName = new DirectoryInfo(validatedPath).Name;

		await AuditTrailData.SaveAuditTrail(new()
		{
			Action = AuditTrailActionTypes.Download.ToString(),
			TableName = OperationNames.FileFolder,
			RecordNo = $"{folderName}.zip",
			RecordValue = validatedPath,
			CreatedBy = userId,
			CreatedFromPlatform = platform
		});

		Response.Headers.Append("Content-Disposition", $"attachment; filename=\"{folderName}.zip\"");
		Response.ContentType = "application/zip";

		try
		{
			await FileFolderData.StreamFolderAsZip(validatedPath, userId, Response.Body, HttpContext.RequestAborted);
		}
		catch (OperationCanceledException) { }

		return new EmptyResult();
	}
	#endregion

	#region Actions
	[HttpPost]
	[Route("CreateFolder")]
	public async Task<IActionResult> CreateFolder([FromQuery] string parentPath, [FromQuery] string name, [FromQuery] int userId, [FromQuery] string platform)
	{
		try
		{
			await FileFolderData.CreateFolder(parentPath, name, userId, platform);
			return NoContent();
		}
		catch (Exception ex) { return StatusCode(500, $"Error creating folder: {ex.Message}"); }
	}

	[HttpPost]
	[Route("CreateFile")]
	public async Task<IActionResult> CreateFile([FromQuery] string parentPath, [FromQuery] string name, [FromQuery] int userId, [FromQuery] string platform)
	{
		try
		{
			await FileFolderData.CreateFile(parentPath, name, userId, platform);
			return NoContent();
		}
		catch (Exception ex) { return StatusCode(500, $"Error creating file: {ex.Message}"); }
	}

	[HttpPut]
	[Route("MoveFileFolder")]
	public async Task<IActionResult> MoveFileFolder([FromQuery] string source, [FromQuery] string destinationFolder, [FromQuery] int userId, [FromQuery] string platform)
	{
		try
		{
			await Task.Run(async () => await FileFolderData.MoveFileFolder(source, destinationFolder, userId, platform), HttpContext.RequestAborted);
			return NoContent();
		}
		catch (Exception ex) { return StatusCode(500, $"Error moving: {ex.Message}"); }
	}

	[HttpPost]
	[Route("CopyFileFolder")]
	public async Task<IActionResult> CopyFileFolder([FromQuery] string source, [FromQuery] string destinationFolder, [FromQuery] int userId, [FromQuery] string platform)
	{
		try
		{
			await Task.Run(async () => await FileFolderData.CopyFileFolder(source, destinationFolder, userId, platform), HttpContext.RequestAborted);
			return NoContent();
		}
		catch (Exception ex) { return StatusCode(500, $"Error copying: {ex.Message}"); }
	}

	[HttpPut]
	[Route("RenameFileFolder")]
	public async Task<IActionResult> RenameFileFolder([FromQuery] string path, [FromQuery] string newName, [FromQuery] int userId, [FromQuery] string platform)
	{
		try
		{
			await FileFolderData.RenameFileFolder(path, newName, userId, platform);
			return NoContent();
		}
		catch (Exception ex) { return StatusCode(500, $"Error renaming path: {ex.Message}"); }
	}

	[HttpDelete]
	[Route("DeleteFileFolder")]
	public async Task<IActionResult> DeleteFileFolder([FromQuery] string path, [FromQuery] int userId, [FromQuery] string platform)
	{
		try
		{
			await Task.Run(async () => await FileFolderData.DeleteFileFolder(path, userId, platform), HttpContext.RequestAborted);
			return NoContent();
		}
		catch (Exception ex) { return StatusCode(500, $"Error deleting path: {ex.Message}"); }
	}
	#endregion
}