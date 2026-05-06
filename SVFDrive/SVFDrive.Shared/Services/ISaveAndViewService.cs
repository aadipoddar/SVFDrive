namespace SVFDrive.Shared.Services;

public interface ISaveAndViewService
{
    public Task<string> SaveAndView(string fileName, MemoryStream stream);
}
