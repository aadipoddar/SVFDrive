using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Inputs;
using Syncfusion.Blazor.Popups;

namespace SVFDrive.Shared.Components.Dialog;

public partial class RenameDialog
{
	private SfDialog _dialog;
	private bool _isVisible;

	[Parameter]
	public string CurrentName { get; set; } = "";

	[Parameter]
	public string NewName { get; set; } = "";

	[Parameter]
	public EventCallback<string> NewNameChanged { get; set; }

	[Parameter]
	public bool Disabled { get; set; }

	[Parameter]
	public EventCallback<string> OnConfirm { get; set; }

	[Parameter]
	public EventCallback OnCancel { get; set; }

	public async Task ShowAsync(string currentName)
	{
		CurrentName = currentName;
		NewName = currentName;
		await NewNameChanged.InvokeAsync(NewName);
		_isVisible = true;
		StateHasChanged();
	}

	public async Task HideAsync()
	{
		_isVisible = false;
		StateHasChanged();
		await Task.CompletedTask;
	}

	private async Task HandleConfirm()
	{
		if (string.IsNullOrWhiteSpace(NewName) || NewName == CurrentName)
			return;

		await OnConfirm.InvokeAsync(NewName);
	}

	private async Task HandleCancel()
	{
		_isVisible = false;
		await OnCancel.InvokeAsync();
	}
}
