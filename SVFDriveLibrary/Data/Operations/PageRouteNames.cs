namespace SVFDriveLibrary.Data.Operations;

public static class PageRouteNames
{
	#region Operations
	public const string Login = "/login";
	public const string LoginWithCode = "/login-with-code";
	public const string LoginWithCodeRedirect = "login-with-code-redirect"; // Do not put leading slash

	public const string Dashboard = "/";
	public const string OperationsDashboard = "/operations";

	public const string User = "/operations/user";
	public const string AuditTrailReport = "/operations/audit-trail-report";
	public const string Settings = "/operations/settings";
	#endregion

	#region Permissions
	public const string UserFolderPermission = "/operations/user-folder-permission";
	#endregion
}
