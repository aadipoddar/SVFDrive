namespace SVFDriveLibrary.DataAccess;

public static class OperationNames
{
	#region Common
	public static string LoadTableData => "Load_TableData";
	public static string LoadTableDataById => "Load_TableData_By_Id";
	public static string LoadTableDataByStatus => "Load_TableData_By_Status";
	public static string LoadTableDataByMasterId => "Load_TableData_By_MasterId";
	public static string LoadTableDataByCode => "Load_TableData_By_Code";
	public static string LoadTableDataByDate => "Load_TableData_By_Date";
	public static string LoadLastTableData => "Load_LastTableData";
	public static string LoadCurrentDateTime => "Load_CurrentDateTime";
	#endregion

	#region Settings
	public static string Settings => "Settings";

	public static string UpdateSettings => "Update_Settings";
	public static string LoadSettingsByKey => "Load_Settings_By_Key";
	public static string ResetSettings => "Reset_Settings";
	#endregion

	#region User
	public static string User => "User";
	public static string InsertUser => "Insert_User";
	#endregion

	#region Audit Trail
	public static string AuditTrail => "AuditTrail";
	public static string InsertAuditTrail => "Insert_AuditTrail";
	#endregion
}

public static class PermissionsNames
{
	#region Permissions
	public static string InsertUserFolderPermission => "Insert_UserFolderPermission";

	public static string DeleteUserFolderPermissionById => "Delete_UserFolderPermission_By_Id";

	public static string LoadUserFolderPermissionByUserId => "Load_UserFolderPermission_By_UserId";

	public static string UserFolderPermission => "UserFolderPermission";
	#endregion
}