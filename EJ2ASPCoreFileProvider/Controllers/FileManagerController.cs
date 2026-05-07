using Syncfusion.EJ2.FileManager.PhysicalFileProvider;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Syncfusion.EJ2.FileManager.Base;
using System.Text.Json;
using SVFDriveLibrary.Data.Operations;
using SVFDriveLibrary.Models.Operations;

namespace EJ2APIServices.Controllers;

[Route("api/[controller]")]
[EnableCors("AllowAllOrigins")]
public class FileManagerController : Controller
{
	private readonly PhysicalFileProvider _operation = new();

	private async Task SetRoot()
	{
		var setting = await SettingsData.LoadSettingsByKey(SettingsKeys.MainDriveFolder);
		_operation.RootFolder(setting.Value);
	}

	[Route("FileOperations")]
	public async Task<object> FileOperations([FromBody] FileManagerDirectoryContent args)
	{
		await SetRoot();

		if ((args.Action == "delete" || args.Action == "rename") && args.TargetPath == null && args.Path == "")
			return _operation.ToCamelCase(new FileManagerResponse
			{
				Error = new ErrorDetails { Code = "401", Message = "Restricted to modify the root folder." }
			});

		return args.Action switch
		{
			"read" => _operation.ToCamelCase(_operation.GetFiles(args.Path, args.ShowHiddenItems)),
			"delete" => _operation.ToCamelCase(_operation.Delete(args.Path, args.Names)),
			"copy" => _operation.ToCamelCase(_operation.Copy(args.Path, args.TargetPath, args.Names, args.RenameFiles, args.TargetData)),
			"move" => _operation.ToCamelCase(_operation.Move(args.Path, args.TargetPath, args.Names, args.RenameFiles, args.TargetData)),
			"details" => _operation.ToCamelCase(_operation.Details(args.Path, args.Names, args.Data)),
			"create" => _operation.ToCamelCase(_operation.Create(args.Path, args.Name)),
			"search" => _operation.ToCamelCase(_operation.Search(args.Path, args.SearchString, args.ShowHiddenItems, args.CaseSensitive)),
			"rename" => _operation.ToCamelCase(_operation.Rename(args.Path, args.Name, args.NewName, false, args.ShowFileExtension, args.Data)),
			_ => null
		};
	}

	[Route("Upload")]
	[DisableRequestSizeLimit]
	public async Task<IActionResult> Upload(string path, long size, IList<IFormFile> uploadFiles, string action)
	{
		await SetRoot();
		var setting = await SettingsData.LoadSettingsByKey(SettingsKeys.MainDriveFolder);
		var basePath = setting.Value;
		try
		{
			foreach (var file in uploadFiles)
			{
				var folders = file.FileName.Split('/');
				if (folders.Length > 1)
				{
					for (var i = 0; i < folders.Length - 1; i++)
					{
						string newDirectoryPath = Path.Combine(basePath + path, folders[i]);
						if (Path.GetFullPath(newDirectoryPath) != Path.GetDirectoryName(newDirectoryPath) + Path.DirectorySeparatorChar + folders[i])
							throw new UnauthorizedAccessException("Access denied for Directory-traversal");
						if (!Directory.Exists(newDirectoryPath))
							_operation.ToCamelCase(_operation.Create(path, folders[i]));
						path += folders[i] + "/";
					}
				}
			}

			var uploadResponse = _operation.Upload(path, uploadFiles, action, size, null);
			if (uploadResponse.Error != null)
			{
				Response.Clear();
				Response.ContentType = "application/json; charset=utf-8";
				Response.StatusCode = Convert.ToInt32(uploadResponse.Error.Code);
				Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = uploadResponse.Error.Message;
			}
		}
		catch
		{
			Response.Clear();
			Response.ContentType = "application/json; charset=utf-8";
			Response.StatusCode = 417;
			Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = "Access denied for Directory-traversal";
		}
		return Content("");
	}

	[Route("Download")]
	public async Task<IActionResult> Download(string downloadInput)
	{
		await SetRoot();
		var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
		var args = JsonSerializer.Deserialize<FileManagerDirectoryContent>(downloadInput, options);
		return _operation.Download(args.Path, args.Names, args.Data);
	}

	[Route("GetImage")]
	public async Task<IActionResult> GetImage([FromQuery] string path)
	{
		await SetRoot();
		return _operation.GetImage(path, null, false, null, null);
	}
}
