using SVFDrive.Shared.Services;

namespace SVFDrive.Services;

public class BrowserLauncher : IBrowserLauncher
{
	public async Task OpenAsync(string url)
	{
		await Browser.Default.OpenAsync(new Uri(url), BrowserLaunchMode.External);
	}
}
