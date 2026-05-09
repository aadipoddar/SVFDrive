using Syncfusion.EJ2.FileManager.PhysicalFileProvider;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Syncfusion.EJ2.FileManager.Base;
using System.Text.Json;

namespace EJ2APIServices.Controllers;

[Route("api/[controller]")]
[EnableCors("AllowAllOrigins")]
public class VirtualizationController : Controller
{
	public PhysicalFileProvider operation;
	public string basePath;
	string root = Path.Combine("wwwroot", "FileBrowser");
	public VirtualizationController(IWebHostEnvironment hostingEnvironment)
	{
		basePath = hostingEnvironment.ContentRootPath;
		operation = new PhysicalFileProvider();
		string physicalPath = Path.Combine(basePath, root);
		operation.RootFolder(physicalPath);
	}
	[Route("FileOperations")]
	public object FileOperations([FromBody] FileManagerDirectoryContent args)
	{
		if (args.Action == "delete" || args.Action == "rename")
		{
			if ((args.TargetPath == null) && (args.Path == ""))
			{
				FileManagerResponse response = new()
				{
					Error = new ErrorDetails { Code = "401", Message = "Restricted to modify the root folder." }
				};
				return operation.ToCamelCase(response);
			}
		}
		switch (args.Action)
		{
			case "read":
				// reads the file(s) or folder(s) from the given path.
				return operation.ToCamelCase(operation.GetFiles(args.Path, args.ShowHiddenItems));
			case "delete":
				// deletes the selected file(s) or folder(s) from the given path.
				return operation.ToCamelCase(operation.Delete(args.Path, args.Names));
			case "copy":
				// copies the selected file(s) or folder(s) from a path and then pastes them into a given target path.
				return operation.ToCamelCase(operation.Copy(args.Path, args.TargetPath, args.Names, args.RenameFiles, args.TargetData));
			case "move":
				// cuts the selected file(s) or folder(s) from a path and then pastes them into a given target path.
				return operation.ToCamelCase(operation.Move(args.Path, args.TargetPath, args.Names, args.RenameFiles, args.TargetData));
			case "details":
				// gets the details of the selected file(s) or folder(s).
				return operation.ToCamelCase(operation.Details(args.Path, args.Names, args.Data));
			case "create":
				// creates a new folder in a given path.
				return operation.ToCamelCase(operation.Create(args.Path, args.Name));
			case "search":
				// gets the list of file(s) or folder(s) from a given path based on the searched key string.
				return operation.ToCamelCase(operation.Search(args.Path, args.SearchString, args.ShowHiddenItems, args.CaseSensitive));
			case "rename":
				// renames a file or folder.
				return operation.ToCamelCase(operation.Rename(args.Path, args.Name, args.NewName, false, args.ShowFileExtension, args.Data));
		}
		return null;
	}

	// uploads the file(s) into a specified path
	[Route("Upload")]
	public IActionResult Upload(string path, long size, IList<IFormFile> uploadFiles, string action)
	{
		try
		{
			FileManagerResponse uploadResponse;
			foreach (var file in uploadFiles)
			{
				var folders = file.FileName.Split('/');
				// checking the folder upload
				if (folders.Length > 1)
				{
					for (var i = 0; i < folders.Length - 1; i++)
					{
						string newDirectoryPath = Path.Combine(basePath + path, folders[i]);
						if (Path.GetFullPath(newDirectoryPath) != (Path.GetDirectoryName(newDirectoryPath) + Path.DirectorySeparatorChar + folders[i]))
						{
							throw new UnauthorizedAccessException("Access denied for Directory-traversal");
						}
						if (!Directory.Exists(newDirectoryPath))
						{
							operation.ToCamelCase(operation.Create(path, folders[i]));
						}
						path += folders[i] + "/";
					}
				}
			}
			uploadResponse = operation.Upload(path, uploadFiles, action, size, null);
			if (uploadResponse.Error != null)
			{
				Response.Clear();
				Response.ContentType = "application/json; charset=utf-8";
				Response.StatusCode = Convert.ToInt32(uploadResponse.Error.Code);
				Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = uploadResponse.Error.Message;
			}
		}
		catch (Exception e)
		{
			ErrorDetails er = new()
			{
				Message = e.Message.ToString(),
				Code = "417"
			};
			er.Message = "Access denied for Directory-traversal";
			Response.Clear();
			Response.ContentType = "application/json; charset=utf-8";
			Response.StatusCode = Convert.ToInt32(er.Code);
			Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = er.Message;
			return Content("");
		}
		return Content("");
	}

	// downloads the selected file(s) and folder(s)
	[Route("Download")]
	public IActionResult Download(string downloadInput)
	{
		var options = new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		};
		FileManagerDirectoryContent args = JsonSerializer.Deserialize<FileManagerDirectoryContent>(downloadInput, options);
		return operation.Download(args.Path, args.Names, args.Data);
	}

	// gets the image(s) from the given path
	[Route("GetImage")]
	public IActionResult GetImage(FileManagerDirectoryContent args)
	{
		return operation.GetImage(args.Path, args.Id, false, null, null);
	}
}
