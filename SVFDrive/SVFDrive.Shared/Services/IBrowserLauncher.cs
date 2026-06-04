namespace SVFDrive.Shared.Services;

public interface IBrowserLauncher
{
	Task OpenAsync(string url);

	/// <summary>
	/// Opens the URL in a new browser tab/window so inline content (e.g. an image preview)
	/// is shown to the user, rather than triggering a background download.
	/// </summary>
	Task OpenInNewTabAsync(string url);
}
