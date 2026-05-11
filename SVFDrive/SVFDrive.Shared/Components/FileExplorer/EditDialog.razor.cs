using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Popups;

namespace SVFDrive.Shared.Components.FileExplorer;

public partial class EditDialog
{
	private SfDialog _dialog;
	private bool _isVisible;

	[Parameter] public string CurrentName { get; set; } = "";
	[Parameter] public string Value { get; set; } = "";
	[Parameter] public EventCallback<string> ValueChanged { get; set; }
	[Parameter] public EventCallback<string> OnConfirm { get; set; }
	[Parameter] public EventCallback OnCancel { get; set; }

	public async Task ShowAsync(string initialValue = "", string currentName = "")
	{
		CurrentName = currentName;
		Value = initialValue;
		await ValueChanged.InvokeAsync(Value);
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
		if (string.IsNullOrWhiteSpace(Value) || Value == CurrentName)
			return;

		await OnConfirm.InvokeAsync(Value);
	}

	private async Task HandleCancel()
	{
		_isVisible = false;
		await OnCancel.InvokeAsync();
	}
}
