using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Syncfusion.Blazor.Inputs;
using Syncfusion.Blazor.Popups;

namespace SVFDrive.Shared.Components.Dialog;

public partial class FileUploadDialog
{
	private SfDialog _dialog;
	private bool _isVisible;
	private bool _pendingMarked;

	[Inject] private IJSRuntime JS { get; set; }

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

		if (!_pendingMarked)
		{
			await JS.InvokeVoidAsync("svfBeginPending");
			_pendingMarked = true;
		}

		StateHasChanged();
	}

	public async Task HideAsync()
	{
		_isVisible = false;

		if (_pendingMarked)
		{
			await JS.InvokeVoidAsync("svfEndPending");
			_pendingMarked = false;
		}

		StateHasChanged();
	}

	private async Task HandleSuccess(SuccessEventArgs args)
	{
		if (OnAnyUploadFinished.HasDelegate)
			await OnAnyUploadFinished.InvokeAsync();
	}
}
