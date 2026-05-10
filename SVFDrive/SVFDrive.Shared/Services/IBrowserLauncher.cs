namespace SVFDrive.Shared.Services;

public interface IBrowserLauncher
{
	Task OpenAsync(string url);
}
