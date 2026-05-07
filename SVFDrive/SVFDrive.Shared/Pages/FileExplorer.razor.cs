using SVFDriveLibrary.Data.Operations;
using SVFDriveLibrary.Data.Permissions;
using SVFDriveLibrary.Models.Operations;
using SVFDriveLibrary.Models.Permissions;

namespace SVFDrive.Shared.Pages;

public partial class FileExplorer
{
	private UserModel _user;
	private bool _isLoading = true;
	private string _mainDriveFolder;
	private string _fileManagerApiBase;

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (!firstRender)
			return;

		_user = await AuthenticationService.ValidateUser(DataStorageService, NavigationManager, VibrationService);
		await InitializePage();

		_isLoading = false;
		StateHasChanged();
	}

	private async Task InitializePage()
	{ 
		await LoadDriveSettings();
		await UserPermission();
	}

	private async Task LoadDriveSettings()
	{
		var mainDriveFolder = await SettingsData.LoadSettingsByKey(SettingsKeys.MainDriveFolder);
		_mainDriveFolder = mainDriveFolder.Value;

		var fileManagerApiBase = await SettingsData.LoadSettingsByKey(SettingsKeys.FileManagerApiBase);
		_fileManagerApiBase = fileManagerApiBase.Value;
	}

	private async Task UserPermission()
	{
		var permissions = await UserFolderPermissionData.LoadUserFolderPermissionByUserId(_user.Id);
	}
}