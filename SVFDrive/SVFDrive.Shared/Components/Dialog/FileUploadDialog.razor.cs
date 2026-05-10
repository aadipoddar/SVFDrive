using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Inputs;
using Syncfusion.Blazor.Popups;

namespace SVFDrive.Shared.Components.Dialog;

public partial class FileUploadDialog
{
	private SfDialog _dialog;
	private bool _isVisible;

	[Parameter] public string DestinationPath { get; set; } = "";
	[Parameter] public string SaveUrl { get; set; } = "";
	[Parameter] public string RemoveUrl { get; set; } = "";
	[Parameter] public EventCallback OnAnyUploadFinished { get; set; }

	public async Task ShowAsync(string destinationPath, string saveUrl, string removeUrl)
	{
		DestinationPath = destinationPath;
		SaveUrl = saveUrl;
		RemoveUrl = removeUrl;
		_isVisible = true;
		StateHasChanged();
		await Task.CompletedTask;
	}

	public async Task HideAsync()
	{
		_isVisible = false;
		StateHasChanged();
		await Task.CompletedTask;
	}

	private async Task HandleSuccess(SuccessEventArgs args)
	{
		if (OnAnyUploadFinished.HasDelegate)
			await OnAnyUploadFinished.InvokeAsync();
	}
}
