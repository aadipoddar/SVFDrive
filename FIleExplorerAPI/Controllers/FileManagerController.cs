using Microsoft.AspNetCore.Mvc;

namespace FIleExplorerAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FileManagerController : ControllerBase
{
	[Route("FileOperations")]
	public async Task FileOperations([FromQuery] int userId)
	{
		var deserializedArgs = userId;
	}
}
