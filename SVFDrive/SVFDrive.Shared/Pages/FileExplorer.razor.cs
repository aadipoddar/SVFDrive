using SVFDrive.Shared.Components.Dialog;
using SVFDriveLibrary.Data.FileExplorer;
using SVFDriveLibrary.Data.Operations;
using SVFDriveLibrary.Models.FileExplorer;
using SVFDriveLibrary.Models.Operations;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Navigations;

namespace SVFDrive.Shared.Pages;

public partial class FileExplorer
{
	private UserModel _user;
	private bool _isLoading = true;

	private FileFolderModel _mainDriveFolder;
	private FileFolderModel _currentPath;

	private List<FileFolderModel> _folderFiles = [];

	private SfGrid<FileFolderModel> _sfGrid;
	private ToastNotification _toastNotification;

	private List<object> ToolbarItems = [
		new ItemModel() { Id = "GoBack", TooltipText = "Go back", PrefixIcon = "e-arrow-left" },
		new ItemModel() { Id = "Home", TooltipText = "Home", PrefixIcon = "e-home" },
		new ItemModel() { Id = "Refresh", TooltipText = "Refresh", PrefixIcon = "e-refresh" },
		new ItemModel() { Type = ItemType.Separator},
		new ItemModel() { Id = "Path" },
		new ItemModel() { Type = ItemType.Separator, Align = ItemAlign.Right},
		new ItemModel() { Id = "Delete", TooltipText = "Delete", PrefixIcon = "e-delete", Align = ItemAlign.Right},
		new ItemModel() { Type = ItemType.Separator, Align = ItemAlign.Right},
		"Search"
	];

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
		await LoadFileFoldersFromAPI();
	}

	private async Task LoadRootFolder()
	{
		var rootFolder = (await SettingsData.LoadSettingsByKey(SettingsKeys.MainDriveFolder)).Value;
		_mainDriveFolder = await FileExplorerData.LoadFileFolderInfoFromAPI(rootFolder);
		_currentPath = _mainDriveFolder;
	}
	#endregion

	#region Info
	private async Task<FileFolderModel> LoadFileFolderInfoFromAPI(string path)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(path))
				path = _currentPath?.FullName;

			return await FileExplorerData.LoadFileFolderInfoFromAPI(path);
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("Error", $"Failed to get file info: {ex.Message}", ToastType.Error);
			return null;
		}
	}
	#endregion

	#region Lists
	private async Task LoadFileFoldersFromAPI(string path = null)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(path))
				path = _currentPath?.FullName;

			_folderFiles = await FileExplorerData.LoadFileFoldersFromAPI(path);
			_currentPath = await FileExplorerData.LoadFileFolderInfoFromAPI(path);
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("Error", $"Failed to load file explorer data: {ex.Message}", ToastType.Error);
		}
		finally
		{
			await DataGridRefresh();
		}
	}

	private async Task LoadParentFileFoldersFromAPI(string path = null)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(path))
				path = _currentPath?.FullName;

			_folderFiles = await FileExplorerData.LoadParentFileFoldersFromAPI(path);
			_currentPath = await FileExplorerData.LoadFileFolderInfoFromAPI(_currentPath.ParentFullName);
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("Error", $"Failed to load parent folder: {ex.Message}", ToastType.Error);
		}
		finally
		{
			await DataGridRefresh();
		}
	}

	#endregion

	#region Actions
	private async Task DeleteFileFolderFromAPI(string path = null)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(path))
			{
				if (_sfGrid is null || _sfGrid.SelectedRecords.Count == 0)
					throw new Exception("No file selected for deletion.");

				foreach (var selected in _sfGrid.SelectedRecords)
					if (selected is not null)
						await FileExplorerData.DeleteFileFolderFromAPI(selected.FullName);
			}

			else
				await FileExplorerData.DeleteFileFolderFromAPI(path);
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("Error", $"Failed to delete file: {ex.Message}", ToastType.Error);
		}
		finally
		{
			await LoadFileFoldersFromAPI();
		}
	}
	#endregion

	#region Utilities
	public async Task ToolbarClickHandler(ClickEventArgs args)
	{
		switch (args.Item.Id)
		{
			case "GoBack": await LoadParentFileFoldersFromAPI(); break;
			case "Home": await LoadFileFoldersFromAPI(_mainDriveFolder.FullName); break;
			case "Refresh": await LoadFileFoldersFromAPI(); break;
			case "Delete": await DeleteFileFolderFromAPI(); break;
		}
	}

	public async Task RecordDoubleClickHandler(RecordDoubleClickEventArgs<FileFolderModel> args)
	{
		if (!args.RowData.IsFile)
			await LoadFileFoldersFromAPI(args.RowData.FullName);
	}

	private async Task DataGridRefresh()
	{
		ToolbarItems[4] = new ItemModel() { Id = "Path", Text = _currentPath?.FullName ?? string.Empty };

		if (_sfGrid is not null)
			await _sfGrid.Refresh();
	}
	#endregion
}
