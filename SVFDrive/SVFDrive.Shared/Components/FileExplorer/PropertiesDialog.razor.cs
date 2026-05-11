using Microsoft.AspNetCore.Components;
using SVFDriveLibrary.Models.FileExplorer;
using Syncfusion.Blazor.Popups;

namespace SVFDrive.Shared.Components.FileExplorer;

public partial class PropertiesDialog
{
    private SfDialog _dialog;
    private bool _isVisible;

    [Parameter] public FileFolderModel Item { get; set; }

    public async Task ShowAsync(FileFolderModel item)
    {
        Item = item;
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
}
