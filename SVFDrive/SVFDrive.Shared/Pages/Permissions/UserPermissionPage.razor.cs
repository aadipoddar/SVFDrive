using SVFDrive.Shared.Components.Dialog;
using SVFDrive.Shared.Components.Input;
using SVFDriveLibrary.Data.Common;
using SVFDriveLibrary.Data.Operations;
using SVFDriveLibrary.Data.Permissions;
using SVFDriveLibrary.Exports.Permissions;
using SVFDriveLibrary.Exports.Utils;
using SVFDriveLibrary.Models.Operations;
using SVFDriveLibrary.Models.Permissions;
using Syncfusion.Blazor.Grids;

namespace SVFDrive.Shared.Pages.Permissions;

public partial class UserPermissionPage
{
	private UserModel _loggedInUser;
	private bool _isLoading = true;
	private bool _isProcessing = false;

	private UserPermissionModel _userPermission = new();
	private UserModel _selectedUser;

	private List<UserModel> _users = [];
	private List<UserPermissionModel> _userPermissions = [];
	private readonly List<ContextMenuItemModel> _gridContextMenuItems =
	[
		new() { Text = "Edit (Insert)", Id = "EditSelectedItem", IconCss = "e-icons e-edit", Target = ".e-content" },
		new() { Text = "Delete (Del)", Id = "DeleteSelectedItem", IconCss = "e-icons e-trash", Target = ".e-content" }
	];

	private SfGrid<UserPermissionModel> _sfGrid;
	private DeleteConfirmationDialog _deleteConfirmationDialog;

	private int _deleteTransactionId = 0;
	private string _deleteTransactionName = string.Empty;

	private ToastNotification _toastNotification;
	private AutoCompleteWithAdd<UserModel, UserModel> _sfFirstFocus;

	#region Load Data
	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (!firstRender)
			return;

		_loggedInUser = await AuthenticationService.ValidateUser(DataStorageService, NavigationManager, VibrationService, [UserRoles.Admin]);
		await LoadData();
	}

	private async Task LoadData()
	{
		_users = await CommonData.LoadTableDataByStatus<UserModel>(OperationNames.User);
		_userPermissions = await CommonData.LoadTableData<UserPermissionModel>(PermissionsNames.UserPermission);

		if (_sfGrid is not null)
			await _sfGrid.Refresh();

		_isLoading = false;
		StateHasChanged();

		if (_sfFirstFocus is not null)
			await _sfFirstFocus.FocusAsync();
	}
	#endregion

	#region Saving
	private async Task SaveTransaction()
	{
		if (_isProcessing)
			return;

		try
		{
			_isProcessing = true;
			StateHasChanged();

			if (!_loggedInUser.Admin)
				throw new Exception("You do not have permission to perform this action.");

			await _toastNotification.ShowAsync("Processing", "Please wait while the transaction is being saved...", ToastType.Info);

			_userPermission.UserId = _selectedUser.Id;
			await UserPermissionData.SaveTransaction(_userPermission, _loggedInUser.Id, FormFactor.GetFormFactor() + FormFactor.GetPlatform());

			await _toastNotification.ShowAsync("Saved", "Transaction has been saved successfully.", ToastType.Success);
			ResetPage();
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("Error While Saving", ex.Message, ToastType.Error);
		}
		finally
		{
			_isProcessing = false;
		}
	}
	#endregion

	#region Exporting
	private async Task ExportExcel()
	{
		if (_isProcessing)
			return;

		try
		{
			_isProcessing = true;
			StateHasChanged();
			await _toastNotification.ShowAsync("Processing", "Generating the Export...", ToastType.Info);

			var (stream, fileName) = await UserPermissionExport.ExportMaster(_userPermissions, ReportExportType.Excel);
			await SaveAndViewService.SaveAndView(fileName, stream);

			await _toastNotification.ShowAsync("Exported", "The export has been downloaded successfully.", ToastType.Success);
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("Error While Exporting", ex.Message, ToastType.Error);
		}
		finally
		{
			_isProcessing = false;
			StateHasChanged();
		}
	}

	private async Task ExportPdf()
	{
		if (_isProcessing)
			return;

		try
		{
			_isProcessing = true;
			StateHasChanged();
			await _toastNotification.ShowAsync("Processing", "Generating the Export...", ToastType.Info);

			var (stream, fileName) = await UserPermissionExport.ExportMaster(_userPermissions, ReportExportType.PDF);
			await SaveAndViewService.SaveAndView(fileName, stream);

			await _toastNotification.ShowAsync("Exported", "The export has been downloaded successfully.", ToastType.Success);
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("Error While Exporting", ex.Message, ToastType.Error);
		}
		finally
		{
			_isProcessing = false;
			StateHasChanged();
		}
	}
	#endregion

	#region Actions
	private async Task ConfirmDelete()
	{
		try
		{
			_isProcessing = true;
			await _deleteConfirmationDialog.HideAsync();

			if (!_loggedInUser.Admin)
				throw new Exception("You do not have permission to perform this action.");

			var userPermission = await CommonData.LoadTableDataById<UserPermissionModel>(PermissionsNames.UserPermission, _deleteTransactionId)
				?? throw new Exception("Transaction not found.");

			await UserPermissionData.DeleteTransaction(userPermission, _loggedInUser.Id, FormFactor.GetFormFactor() + FormFactor.GetPlatform());

			await _toastNotification.ShowAsync("Deleted", "Transaction has been deleted successfully.", ToastType.Success);
			ResetPage();
		}
		catch (Exception ex)
		{
			await _toastNotification.ShowAsync("Error While Deleting", ex.Message, ToastType.Error);
		}
		finally
		{
			_isProcessing = false;
			_deleteTransactionId = 0;
			_deleteTransactionName = string.Empty;
		}
	}
	#endregion

	#region Utilities
	private async Task OnMenuSelected(Syncfusion.Blazor.Navigations.MenuEventArgs<Syncfusion.Blazor.Navigations.MenuItem> args)
	{
		switch (args.Item.Id)
		{
			case "NewTransaction":
				ResetPage();
				break;
			case "SaveTransaction":
				await SaveTransaction();
				break;
			case "ExportExcel":
				await ExportExcel();
				break;
			case "ExportPdf":
				await ExportPdf();
				break;
			case "EditSelectedItem":
				await EditSelectedItem();
				break;
			case "DeleteSelectedItem":
				await DeleteSelectedItem();
				break;
		}
	}

	private async Task OnGridContextMenuItemClicked(ContextMenuClickEventArgs<UserPermissionModel> args)
	{
		switch (args.Item.Id)
		{
			case "EditSelectedItem":
				await EditSelectedItem();
				break;
			case "DeleteSelectedItem":
				await DeleteSelectedItem();
				break;
		}
	}

	private async Task EditSelectedItem()
	{
		var selectedRecords = await _sfGrid.GetSelectedRecordsAsync();
		if (selectedRecords.Count == 0)
			return;

		_selectedUser = _users.FirstOrDefault(u => u.Id == selectedRecords[0].UserId);
		_userPermission = await CommonData.LoadTableDataById<UserPermissionModel>(PermissionsNames.UserPermission, selectedRecords[0].Id);
		if (_userPermission is null)
			await _toastNotification.ShowAsync("Error while Editing", "Transaction Not Found.", ToastType.Error);

		await _sfFirstFocus.FocusAsync();

		StateHasChanged();
	}

	private async Task DeleteSelectedItem()
	{
		var selectedRecords = await _sfGrid.GetSelectedRecordsAsync();
		if (selectedRecords.Count > 0)
			await ShowDeleteConfirmation(selectedRecords[0].Id, selectedRecords[0].Path);
	}

	private async Task ShowDeleteConfirmation(int id, string name)
	{
		_deleteTransactionId = id;
		_deleteTransactionName = name;
		await _deleteConfirmationDialog.ShowAsync();
	}

	private async Task CancelDelete()
	{
		_deleteTransactionId = 0;
		_deleteTransactionName = string.Empty;
		await _deleteConfirmationDialog.HideAsync();
	}
	
	private void ResetPage() =>
		NavigationManager.NavigateTo(PageRouteNames.UserPermission, true);

	private void NavigateBack() =>
		NavigationManager.NavigateTo(PageRouteNames.OperationsDashboard);
	#endregion
}