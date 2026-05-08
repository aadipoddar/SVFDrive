using SVFDriveLibrary.Data.Operations;
using SVFDriveLibrary.Models.Operations;

namespace SVFDrive.Shared.Pages;

public partial class FileExplorer
{
	private UserModel _user;
	private bool _isLoading = true;
	private string _fileManagerApiBase;

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (!firstRender)
			return;

		_user = await AuthenticationService.ValidateUser(DataStorageService, NavigationManager, VibrationService);

		var fileManagerApiBase = await SettingsData.LoadSettingsByKey(SettingsKeys.FileManagerApiBase);
		_fileManagerApiBase = fileManagerApiBase.Value;

		// TODO - NEW API
		_fileManagerApiBase = "https://localhost:7250/";

		_isLoading = false;
		StateHasChanged();
	}
}
