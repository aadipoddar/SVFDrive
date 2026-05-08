using SVFDrive.Shared.Components.Dialog;
using SVFDriveLibrary.Data.FIleExplorer;
using SVFDriveLibrary.Data.Operations;
using SVFDriveLibrary.Models.FileExplorer;
using SVFDriveLibrary.Models.Operations;

namespace SVFDrive.Shared.Pages;

public partial class FileExplorer
{
	private UserModel _user;
	private bool _isLoading = true;

	private string _mainDriveFolder;
	private string _currentPath;

	private List<FolderFileModel> _folderFiles = [];

	private ToastNotification _toastNotification;

	#region Load Data
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
		_mainDriveFolder = (await SettingsData.LoadSettingsByKey(SettingsKeys.MainDriveFolder)).Value;
		_currentPath = _mainDriveFolder;
		await LoadFilesFolders();
	}

	private async Task LoadFilesFolders(string path = null)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(path))
				path = _currentPath;

			_folderFiles = await FileExplorerData.LoadFoldersFileFromAPI(path);
			_currentPath = path;
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("Error", $"Failed to load file explorer data: {ex.Message}", ToastType.Error);
		}
	}
	#endregion
}
