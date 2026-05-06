namespace SVFDriveLibrary.Models.Operations;

public class SettingsModel
{
	public string Key { get; set; }
	public string Value { get; set; }
	public string Description { get; set; }
}

public static class SettingsKeys
{
	public static string EnableLoginWithCode => "EnableLoginWithCode";
	public static string MaxLoginAttempts => "MaxLoginAttempts";
	public static string EnableUsersToResetPassword => "EnableUsersToResetPassword";
	public static string CodeResendLimit => "CodeResendLimit";
	public static string CodeExpiryMinutes => "CodeExpiryMinutes";
	
	public static string MainDriveFolder => "MainDriveFolder";
	public static string FileManagerApiBase => "FileManagerApiBase";

	public static string AutoRefreshReportTimer => "AutoRefreshReportTimer";
}
