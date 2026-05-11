using SVFDrive.Shared.Components.Dialog;
using SVFDriveLibrary.Data.FileExplorer;
using SVFDriveLibrary.Data.Operations;
using SVFDriveLibrary.Models.FileExplorer;
using SVFDriveLibrary.Models.Operations;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Navigations;

namespace SVFDrive.Shared.Components.FileExplorer;

public partial class FileExplorer
{
	private UserModel _user;
	private bool _isLoading = true;

	private FileFolderModel _mainDriveFolder;
	private FileFolderModel _currentPath;

	private List<FileFolderModel> _folderFiles = [];

	private FileFolderModel _renameTarget;
	private EditDialog _editDialog;
	private EditDialogMode _editDialogMode;

	private List<string> _clipboardPaths = [];
	private ClipboardMode _clipboardMode = ClipboardMode.None;

	private string _deleteItemName = string.Empty;
	private DeleteConfirmationDialog _deleteConfirmationDialog;

	private FileUploadDialog _fileUploadDialog;

	private PropertiesDialog _propertiesDialog;

	private SfGrid<FileFolderModel> _sfGrid;
	private ToastNotification _toastNotification;

	private readonly List<object> ToolbarItems = [
		new ItemModel() { Id = "GoBack", TooltipText = "Go back", PrefixIcon = "e-arrow-left" },
		new ItemModel() { Id = "Home", TooltipText = "Home", PrefixIcon = "e-home" },
		new ItemModel() { Id = "Refresh", TooltipText = "Refresh", PrefixIcon = "e-refresh" },
		new ItemModel() { Type = ItemType.Separator},
		new ItemModel() { Id = "Path" },
		new ItemModel() { Type = ItemType.Separator, Align = ItemAlign.Right},
		new ItemModel() { Id = "NewFile", TooltipText = "New File (Ctrl + M)", PrefixIcon = "e-plus" , Align = ItemAlign.Right},
		new ItemModel() { Id = "NewFolder", TooltipText = "New Folder (Ctrl + N)", PrefixIcon = "e-folder", Align = ItemAlign.Right},
		new ItemModel() { Type = ItemType.Separator,Align = ItemAlign.Right},
		new ItemModel() { Id = "CutItem", TooltipText = "Cut (Ctrl + X)", PrefixIcon = "e-cut", Align = ItemAlign.Right},
		new ItemModel() { Id = "CopyItem", TooltipText = "Copy (Ctrl + C)", PrefixIcon = "e-copy", Align = ItemAlign.Right},
		new ItemModel() { Id = "PasteItem", TooltipText = "Paste (Ctrl + V)", PrefixIcon = "e-paste", Align = ItemAlign.Right},
		new ItemModel() { Type = ItemType.Separator,Align = ItemAlign.Right},
		new ItemModel() { Id = "UploadItem", TooltipText = "Upload Files", PrefixIcon = "e-upload-1", Align = ItemAlign.Right},
		new ItemModel() { Id = "DownloadItem", TooltipText = "Download", PrefixIcon = "e-download", Align = ItemAlign.Right},
		new ItemModel() { Id = "RenameItem", TooltipText = "Rename (F2)", PrefixIcon = "e-rename", Align = ItemAlign.Right},
		new ItemModel() { Id = "DeleteItem", TooltipText = "Delete (Del)", PrefixIcon = "e-delete", Align = ItemAlign.Right},
		new ItemModel() { Id = "PropertiesItem", TooltipText = "Properties", PrefixIcon = "e-circle-info", Align = ItemAlign.Right},
		new ItemModel() { Type = ItemType.Separator, Align = ItemAlign.Right},
		"Search"
	];
	private readonly List<ContextMenuItemModel> _gridContextMenuItems =
	[
		new() { Text = "New File (Ctrl + M)", Id = "NewFile", IconCss = "e-icons e-plus", Target = ".e-content" },
		new() { Text = "New Folder (Ctrl + N)", Id = "NewFolder", IconCss = "e-icons e-folder", Target = ".e-content" },
		new() { Separator = true },
		new() { Text = "Cut (Ctrl + X)", Id = "CutItem", IconCss = "e-icons e-cut", Target = ".e-content" },
		new() { Text = "Copy (Ctrl + C)", Id = "CopyItem", IconCss = "e-icons e-copy", Target = ".e-content" },
		new() { Text = "Paste (Ctrl + V)", Id = "PasteItem", IconCss = "e-icons e-paste", Target = ".e-content" },
		new() { Separator = true },
		new() { Text = "Upload Files", Id = "UploadItem", IconCss = "e-icons e-upload-1", Target = ".e-content" },
		new() { Text = "Download", Id = "DownloadItem", IconCss = "e-icons e-download", Target = ".e-content" },
		new() { Text = "Rename (F2)", Id = "RenameItem", IconCss = "e-icons e-rename", Target = ".e-content" },
		new() { Text = "Delete (Del)", Id = "DeleteItem", IconCss = "e-icons e-trash", Target = ".e-content" },
		new() { Separator = true },
		new() { Text = "Properties", Id = "PropertiesItem", IconCss = "e-icons e-circle-info", Target = ".e-content" }
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
		_mainDriveFolder = await FileExplorerData.LoadFileFolderInfoFromAPI(rootFolder, _user.Id);
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

			return await FileExplorerData.LoadFileFolderInfoFromAPI(path, _user.Id);
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

			_folderFiles = await FileExplorerData.LoadFileFoldersFromAPI(path, _user.Id);
			_currentPath = await FileExplorerData.LoadFileFolderInfoFromAPI(path, _user.Id);
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("Error", ex.Message, ToastType.Error);
		}
		finally
		{
			await DataGridRefresh();
		}
	}
	#endregion

	#region Rename New
	private async Task HandleEditDialogConfirm(string value)
	{
		try
		{
			await _editDialog.HideAsync();

			switch (_editDialogMode)
			{
				case EditDialogMode.NewFolder:
					await FileExplorerData.CreateFolderFromAPI(_currentPath.FullName, value, _user.Id);
					await _toastNotification.ShowAsync("Created", $"Folder '{value}' created.", ToastType.Success);
					break;

				case EditDialogMode.NewFile:
					await FileExplorerData.CreateFileFromAPI(_currentPath.FullName, value, _user.Id);
					await _toastNotification.ShowAsync("Created", $"File '{value}' created.", ToastType.Success);
					break;

				case EditDialogMode.Rename:
					if (_renameTarget is null) return;
					await FileExplorerData.RenameFileFolderFromAPI(_renameTarget.FullName, value, _user.Id);
					await _toastNotification.ShowAsync("Renamed", $"Renamed to '{value}' successfully.", ToastType.Success);
					break;
			}
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("Error", ex.Message, ToastType.Error);
		}
		finally
		{
			_renameTarget = null;
			await LoadFileFoldersFromAPI();
		}
	}

	private async Task ShowRenameDialog()
	{
		if (_sfGrid is null || _sfGrid.SelectedRecords.Count == 0)
		{
			await _toastNotification.ShowAsync("Error", "No item selected to rename.", ToastType.Error);
			return;
		}

		if (_sfGrid.SelectedRecords.Count > 1)
		{
			await _toastNotification.ShowAsync("Error", "Select only one item to rename.", ToastType.Error);
			return;
		}

		_renameTarget = _sfGrid.SelectedRecords[0];
		_editDialogMode = EditDialogMode.Rename;
		await _editDialog.ShowAsync(_renameTarget.Name, _renameTarget.Name);
	}

	private async Task ShowNewFolderDialog()
	{
		_renameTarget = null;
		_editDialogMode = EditDialogMode.NewFolder;
		await _editDialog.ShowAsync();
	}

	private async Task ShowNewFileDialog()
	{
		_renameTarget = null;
		_editDialogMode = EditDialogMode.NewFile;
		await _editDialog.ShowAsync();
	}

	private async Task HandleEditDialogCancel()
	{
		_renameTarget = null;
		await _editDialog.HideAsync();
	}
	#endregion

	#region Download Upload
	private async Task DownloadSelected()
	{
		if (_sfGrid is null || _sfGrid.SelectedRecords.Count == 0)
		{
			await _toastNotification.ShowAsync("Error", "No item selected to download.", ToastType.Error);
			return;
		}

		try
		{
			await _toastNotification.ShowAsync("Download Will Start", "Your download will start shortly.", ToastType.Info);

			foreach (var item in _sfGrid.SelectedRecords)
			{
				if (item is null) continue;
				var url = await FileExplorerData.GetDownloadUrl(item.FullName, isFolder: !item.IsFile, _user.Id);
				await BrowserLauncher.OpenAsync(url);
			}
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("Error", $"Failed to start download: {ex.Message}", ToastType.Error);
		}
	}

	private async Task StartUpload()
	{
		if (_currentPath is null)
			return;

		try
		{
			var apiBase = (await SettingsData.LoadSettingsByKey(SettingsKeys.FileManagerApiBase)).Value
				?? throw new Exception("FileManagerApiBase not configured.");

			var encodedParent = Uri.EscapeDataString(_currentPath.FullName);
			var saveUrl = $"{apiBase}api/FileFolderManager/UploadFile?parentPath={encodedParent}&userId={_user.Id}";
			var removeUrl = $"{apiBase}api/FileFolderManager/RemoveUploadedFile?parentPath={encodedParent}&userId={_user.Id}";

			await _fileUploadDialog.ShowAsync(_currentPath.FullName, saveUrl, removeUrl);
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("Error", $"Failed to start upload: {ex.Message}", ToastType.Error);
		}
	}
	#endregion

	#region Cut / Copy / Paste
	private async Task CutSelected()
	{
		if (_sfGrid is null || _sfGrid.SelectedRecords.Count == 0)
		{
			await _toastNotification.ShowAsync("Error", "No item selected.", ToastType.Error);
			return;
		}

		_clipboardPaths = [.. _sfGrid.SelectedRecords.Where(r => r is not null).Select(r => r.FullName)];
		_clipboardMode = ClipboardMode.Cut;
		await _toastNotification.ShowAsync("Cut", $"{_clipboardPaths.Count} item(s) ready to move. Paste at destination.", ToastType.Info);
	}

	private async Task CopySelected()
	{
		if (_sfGrid is null || _sfGrid.SelectedRecords.Count == 0)
		{
			await _toastNotification.ShowAsync("Error", "No item selected.", ToastType.Error);
			return;
		}

		_clipboardPaths = [.. _sfGrid.SelectedRecords.Where(r => r is not null).Select(r => r.FullName)];
		_clipboardMode = ClipboardMode.Copy;
		await _toastNotification.ShowAsync("Copied", $"{_clipboardPaths.Count} item(s) ready to copy. Paste at destination.", ToastType.Info);
	}

	private async Task PasteHere()
	{
		if (_clipboardMode == ClipboardMode.None || _clipboardPaths.Count == 0)
		{
			await _toastNotification.ShowAsync("Nothing to Paste", "Cut or copy items first.", ToastType.Info);
			return;
		}

		if (_currentPath is null) return;

		var mode = _clipboardMode;
		var items = _clipboardPaths;
		int succeeded = 0, failed = 0;

		try
		{
			await _toastNotification.ShowAsync("Working", $"{(mode == ClipboardMode.Cut ? "Moving" : "Copying")} {items.Count} item(s)...", ToastType.Info);

			foreach (var path in items)
			{
				try
				{
					if (mode == ClipboardMode.Cut)
						await FileExplorerData.MoveFileFolderFromAPI(path, _currentPath.FullName, _user.Id);
					else
						await FileExplorerData.CopyFileFolderFromAPI(path, _currentPath.FullName, _user.Id);

					succeeded++;
				}
				catch
				{
					failed++;
				}
			}

			if (failed == 0)
				await _toastNotification.ShowAsync("Done", $"{succeeded} item(s) {(mode == ClipboardMode.Cut ? "moved" : "copied")}.", ToastType.Success);
			else
				await _toastNotification.ShowAsync("Finished", $"{succeeded} succeeded, {failed} failed.", ToastType.Error);

			if (mode == ClipboardMode.Cut)
			{
				_clipboardPaths = [];
				_clipboardMode = ClipboardMode.None;
			}
		}
		finally
		{
			await LoadFileFoldersFromAPI();
		}
	}
	#endregion

	#region Delete
	private async Task DeleteFileFolderFromAPI()
	{
		try
		{
			await _deleteConfirmationDialog.HideAsync();

			foreach (var selected in _sfGrid.SelectedRecords)
				if (selected is not null)
					await FileExplorerData.DeleteFileFolderFromAPI(selected.FullName, _user.Id);
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("Error", ex.Message, ToastType.Error);
		}
		finally
		{
			_deleteItemName = string.Empty;
			await LoadFileFoldersFromAPI();
		}
	}

	private async Task ShowDeleteConfirmation()
	{
		if (_sfGrid is null || _sfGrid.SelectedRecords.Count == 0)
		{
			await _toastNotification.ShowAsync("Error", "No file selected for deletion.", ToastType.Error);
			return;
		}

		_deleteItemName = _sfGrid.SelectedRecords.Count == 1
			? _sfGrid.SelectedRecords[0].Name
			: $"{_sfGrid.SelectedRecords.Count} items";

		await _deleteConfirmationDialog.ShowAsync();
	}

	private async Task CancelDelete()
	{
		_deleteItemName = string.Empty;
		await _deleteConfirmationDialog.HideAsync();
	}
	#endregion

	#region Utilities
	public async Task ToolbarClickHandler(ClickEventArgs args)
	{
		switch (args.Item.Id)
		{
			case "GoBack": await LoadFileFoldersFromAPI(_currentPath.ParentFullName); break;
			case "Home": await LoadFileFoldersFromAPI(_mainDriveFolder.FullName); break;
			case "Refresh": await LoadFileFoldersFromAPI(); break;
			case "NewFolder": await ShowNewFolderDialog(); break;
			case "NewFile": await ShowNewFileDialog(); break;
			case "CutItem": await CutSelected(); break;
			case "CopyItem": await CopySelected(); break;
			case "PasteItem": await PasteHere(); break;
			case "UploadItem": await StartUpload(); break;
			case "DownloadItem": await DownloadSelected(); break;
			case "RenameItem": await ShowRenameDialog(); break;
			case "DeleteItem": await ShowDeleteConfirmation(); break;
			case "PropertiesItem": await ShowProperties(); break;
		}
	}

	private async Task OnGridContextMenuItemClicked(ContextMenuClickEventArgs<FileFolderModel> args)
	{
		switch (args.Item.Id)
		{
			case "NewFile": await ShowNewFileDialog(); break;
			case "NewFolder": await ShowNewFolderDialog(); break;
			case "CutItem": await CutSelected(); break;
			case "CopyItem": await CopySelected(); break;
			case "PasteItem": await PasteHere(); break;
			case "UploadItem": await StartUpload(); break;
			case "DownloadItem": await DownloadSelected(); break;
			case "RenameItem": await ShowRenameDialog(); break;
			case "DeleteItem": await ShowDeleteConfirmation(); break;
			case "PropertiesItem": await ShowProperties(); break;
		}
	}

	private async Task ShowProperties()
	{
		if (_sfGrid is null || _sfGrid.SelectedRecords.Count == 0)
		{
			await _toastNotification.ShowAsync("Error", "No item selected.", ToastType.Error);
			return;
		}

		if (_sfGrid.SelectedRecords.Count > 1)
		{
			await _toastNotification.ShowAsync("Error", "Select only one item to view properties.", ToastType.Error);
			return;
		}

		var info = await LoadFileFolderInfoFromAPI(_sfGrid.SelectedRecords[0].FullName);
		if (info is not null)
			await _propertiesDialog.ShowAsync(info);
	}

	public async Task RecordDoubleClickHandler(RecordDoubleClickEventArgs<FileFolderModel> args)
	{
		if (!args.RowData.IsFile)
			await LoadFileFoldersFromAPI(args.RowData.FullName);
	}

	private async Task DataGridRefresh()
	{
		var pathIndex = ToolbarItems.FindIndex(i => i is ItemModel m && m.Id == "Path");
		if (pathIndex >= 0)
			ToolbarItems[pathIndex] = new ItemModel() { Id = "Path", Text = _currentPath?.FullName ?? string.Empty };

		if (_sfGrid is not null)
			await _sfGrid.Refresh();
	}
	#endregion
}
