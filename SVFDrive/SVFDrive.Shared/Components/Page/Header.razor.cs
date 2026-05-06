using Microsoft.AspNetCore.Components;
using SVFDriveLibrary.Data.Operations;
using SVFDriveLibrary.Models.Operations;

namespace SVFDrive.Shared.Components.Page;

public partial class Header
{
	#region Load Data
	[Parameter]
	public string Title { get; set; } = string.Empty;

	[Parameter]
	public RenderFragment? LeftContent { get; set; }

	[Parameter]
	public RenderFragment? RightContent { get; set; }

	private UserModel _user;

	protected override async Task OnInitializedAsync()
	{
		_user = await AuthenticationService.ValidateUser(DataStorageService, NavigationManager, VibrationService);
	}

	private string GetMobileUserName()
	{
		if (_user is null || string.IsNullOrWhiteSpace(_user.Name))
			return string.Empty;

		var userName = _user.Name.Trim();
		return userName.Length > 5 ? $"{userName[..5]}..." : userName;
	}

	private void NavigateToHome() =>
		NavigationManager.NavigateTo(PageRouteNames.Dashboard);

	private async Task Logout() =>
		await AuthenticationService.Logout(DataStorageService, NavigationManager, VibrationService);
	#endregion
}