using SVFDrive.Shared.Components.Dialog;
using SVFDriveLibrary.Data.Common;
using SVFDriveLibrary.Data.Operations;
using SVFDriveLibrary.Models.Operations;

namespace SVFDrive.Shared.Pages.Operations;

public partial class SettingsPage
{
    #region Fields

    private bool _isLoading = true;
    private bool _isProcessing = false;

    private ToastNotification _toastNotification;
    private ResetConfirmationDialog _resetConfirmationDialog = default!;

    // Login Settings
    private bool _enableLoginWithCode = true;
    private int _maxLoginAttempts = 5;
    private bool _enableUsersToResetPassword = true;
    private int _codeResendLimit = 3;
    private int _codeExpiryMinutes = 10;

	// File System Settings
	private string _mainDriveFolder = @"C:\";
	private string _fileManagerApiBase = string.Empty;

	// Report Settings
	private int _autoRefreshReportTimer = 5;

	#endregion

	#region Load Data

	protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        await AuthenticationService.ValidateUser(DataStorageService, NavigationManager, VibrationService, [UserRoles.Admin]);
        await LoadData();
        _isLoading = false;
        StateHasChanged();
    }

    private async Task LoadData()
    {
        try
        {
            await LoadAllSettings();
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("Error", $"Failed to load settings: {ex.Message}", ToastType.Error);
        }
    }

    private async Task LoadAllSettings()
    {
        var s = await SettingsData.LoadSettingsByKey(SettingsKeys.EnableLoginWithCode);
        _enableLoginWithCode = !bool.TryParse(s?.Value, out var v1) || v1;

        s = await SettingsData.LoadSettingsByKey(SettingsKeys.MaxLoginAttempts);
        _maxLoginAttempts = int.TryParse(s?.Value, out var v2) ? v2 : 5;

        s = await SettingsData.LoadSettingsByKey(SettingsKeys.EnableUsersToResetPassword);
        _enableUsersToResetPassword = !bool.TryParse(s?.Value, out var v3) || v3;

        s = await SettingsData.LoadSettingsByKey(SettingsKeys.CodeResendLimit);
        _codeResendLimit = int.TryParse(s?.Value, out var v4) ? v4 : 3;

        s = await SettingsData.LoadSettingsByKey(SettingsKeys.CodeExpiryMinutes);
        _codeExpiryMinutes = int.TryParse(s?.Value, out var v5) ? v5 : 10;

        s = await SettingsData.LoadSettingsByKey(SettingsKeys.MainDriveFolder);
        _mainDriveFolder = string.IsNullOrWhiteSpace(s?.Value) ? @"C:\" : s.Value;

		s = await SettingsData.LoadSettingsByKey(SettingsKeys.FileManagerApiBase);
		_fileManagerApiBase = s?.Value ?? string.Empty;

		s = await SettingsData.LoadSettingsByKey(SettingsKeys.AutoRefreshReportTimer);
		_autoRefreshReportTimer = int.TryParse(s?.Value, out var v6) ? v6 : 5;
	}
    #endregion

    #region Save Settings

    private async Task SaveSettings()
    {
        if (_isProcessing) return;

        try
        {
            _isProcessing = true;

            await _toastNotification.ShowAsync("Saving", "Processing settings...", ToastType.Info);

            var settings = await CommonData.LoadTableData<SettingsModel>(OperationNames.Settings);

            await UpdateSetting(SettingsKeys.EnableLoginWithCode, _enableLoginWithCode.ToString().ToLower());
            await UpdateSetting(SettingsKeys.MaxLoginAttempts, _maxLoginAttempts.ToString());
            await UpdateSetting(SettingsKeys.EnableUsersToResetPassword, _enableUsersToResetPassword.ToString().ToLower());
            await UpdateSetting(SettingsKeys.CodeResendLimit, _codeResendLimit.ToString());
            await UpdateSetting(SettingsKeys.CodeExpiryMinutes, _codeExpiryMinutes.ToString());
            
            await UpdateSetting(SettingsKeys.MainDriveFolder, _mainDriveFolder);
			await UpdateSetting(SettingsKeys.FileManagerApiBase, _fileManagerApiBase);

			await UpdateSetting(SettingsKeys.AutoRefreshReportTimer, _autoRefreshReportTimer.ToString());

			await _toastNotification.ShowAsync("Saved", "Settings saved successfully.", ToastType.Success);
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("Error", $"Failed to save settings: {ex.Message}", ToastType.Error);
        }
        finally
        {
            _isProcessing = false;
            StateHasChanged();
        }
    }

    private static async Task UpdateSetting(string key, string value)
    {
        await SettingsData.UpdateSettings(new ()
        {
            Key = key,
            Value = value ?? string.Empty,
            Description = ""
        });
    }

    #endregion

    #region Reset Settings

    private async Task ShowResetConfirmation() => await _resetConfirmationDialog.ShowAsync();

    private async Task CancelReset() => await _resetConfirmationDialog.HideAsync();

    private async Task ConfirmReset()
    {
        try
        {
            await _resetConfirmationDialog.HideAsync();
            _isProcessing = true;

            await _toastNotification.ShowAsync("Resetting", "Restoring default settings...", ToastType.Info);
            await SettingsData.ResetSettings();
            await LoadData();
            await _toastNotification.ShowAsync("Reset", "Settings restored to defaults.", ToastType.Success);
        }
        catch (Exception ex)
        {
            await _toastNotification.ShowAsync("Error", $"Failed to reset settings: {ex.Message}", ToastType.Error);
        }
        finally
        {
            _isProcessing = false;
            StateHasChanged();
        }
    }

    #endregion

    #region Utilities

    private async Task OnMenuSelected(Syncfusion.Blazor.Navigations.MenuEventArgs<Syncfusion.Blazor.Navigations.MenuItem> args)
    {
        switch (args.Item.Id)
        {
            case "SaveSettings":
                await SaveSettings();
                break;
            case "ResetSettings":
                await ShowResetConfirmation();
                break;
        }
    }

    private void NavigateBack() =>
        NavigationManager.NavigateTo(PageRouteNames.OperationsDashboard);

    #endregion
}
