using SVFDrive.Shared.Services;

namespace SVFDrive.Services;

public class BrowserLauncher : IBrowserLauncher
{
	public async Task OpenAsync(string url)
	{
		await Browser.Default.OpenAsync(new Uri(url), BrowserLaunchMode.External);
	}

	// On native hosts the system browser already opens the URL in its own tab/window,
	// rendering inline content — same call as OpenAsync.
	public async Task OpenInNewTabAsync(string url)
	{
		await Browser.Default.OpenAsync(new Uri(url), BrowserLaunchMode.External);
	}
}
