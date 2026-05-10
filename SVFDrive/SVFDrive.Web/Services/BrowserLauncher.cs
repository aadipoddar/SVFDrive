using Microsoft.JSInterop;
using SVFDrive.Shared.Services;

namespace SVFDrive.Web.Services;

public class BrowserLauncher(IJSRuntime jsRuntime) : IBrowserLauncher
{
	public async Task OpenAsync(string url)
	{
		await jsRuntime.InvokeVoidAsync("svfDownload", url);
	}
}
