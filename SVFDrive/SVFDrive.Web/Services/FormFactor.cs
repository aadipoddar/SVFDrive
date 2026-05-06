using SVFDrive.Shared.Services;

namespace SVFDrive.Web.Services;

public class FormFactor : IFormFactor
{
	public string GetFormFactor() => "Web";

	public string GetPlatform() => Environment.OSVersion.ToString();
}
