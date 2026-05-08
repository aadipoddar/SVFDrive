using SVFDrive.Shared.Components.Dialog;
using SVFDriveLibrary.Data.FileExplorer;
using SVFDriveLibrary.Data.Operations;
using SVFDriveLibrary.Models.FileExplorer;
using SVFDriveLibrary.Models.Operations;

namespace SVFDrive.Shared.Pages;

public partial class FileExplorer
{
	private UserModel _user;
	private bool _isLoading = true;

	private FileFolderModel _mainDriveFolder;
	private FileFolderModel _currentPath;

	private List<FileFolderModel> _folderFiles = [];

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
		await LoadRootFolder();
		await LoadFolderFiles();
	}

	private async Task LoadRootFolder()
	{
		var rootFolder = (await SettingsData.LoadSettingsByKey(SettingsKeys.MainDriveFolder)).Value;
		_mainDriveFolder = await FileExplorerData.GetFolderInfoFromAPI(rootFolder);
		_currentPath = _mainDriveFolder;
	}
	#endregion

	#region Folder File
	private async Task LoadFolderFiles(string path = null)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(path))
				path = _currentPath?.FullName;

			_folderFiles = await FileExplorerData.LoadFileFoldersFromAPI(path);
			_currentPath = await FileExplorerData.GetFolderInfoFromAPI(path);
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("Error", $"Failed to load file explorer data: {ex.Message}", ToastType.Error);
		}
	}

	private async Task LoadParentFolderFiles(string path = null)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(path))
				path = _currentPath?.FullName;

			_folderFiles = await FileExplorerData.LoadParentFileFoldersFromAPI(path);
			_currentPath = await FileExplorerData.GetFolderInfoFromAPI(_currentPath.ParentFullName);
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("Error", $"Failed to load parent folder: {ex.Message}", ToastType.Error);
		}
	}

	private async Task<FileFolderModel> GetFileInfoFromAPI(string path)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(path))
				path = _currentPath?.FullName;

			return await FileExplorerData.GetFileInfoFromAPI(path);
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("Error", $"Failed to get file info: {ex.Message}", ToastType.Error);
			return null;
		}
	}

	private async Task<FileFolderModel> GetFolderInfoFromAPI(string path)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(path))
				path = _currentPath?.FullName;

			return await FileExplorerData.GetFolderInfoFromAPI(path);
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("Error", $"Failed to get folder info: {ex.Message}", ToastType.Error);
			return null;
		}
	}
	#endregion
}
