using SVFDriveLibrary.Data.Common;
using SVFDriveLibrary.Data.Operations;
using SVFDriveLibrary.DataAccess;
using SVFDriveLibrary.Models.Operations;
using SVFDriveLibrary.Models.Permissions;

namespace SVFDriveLibrary.Data.Permissions;

public static class UserFolderPermissionData
{
	private static async Task<int> InsertUserFolderPermission(UserFolderPermissionModel userFolderPermissionModel, SqlDataAccessTransaction transaction = null) =>
		(await SqlDataAccess.LoadData<int, dynamic>(PermissionsNames.InsertUserFolderPermission, userFolderPermissionModel, transaction)).FirstOrDefault()
			is var id and > 0 ? id : throw new InvalidOperationException("Failed to Insert User Folder Permission.");

	public static async Task<List<UserFolderPermissionModel>> LoadUserFolderPermissionByUserId(int UserId) =>
		await SqlDataAccess.LoadData<UserFolderPermissionModel, dynamic>(PermissionsNames.LoadUserFolderPermissionByUserId, new { UserId });

	public static async Task DeleteUserFolderPermissionById(int Id) =>
		await SqlDataAccess.SaveData(PermissionsNames.DeleteUserFolderPermissionById, new { Id });

	public static async Task DeleteTransaction(UserFolderPermissionModel userFolderPermission, int userId, string platform) =>
		await SqlDataAccessTransaction.Run(async transaction =>
		{
			await DeleteUserFolderPermissionById(userFolderPermission.Id);

			var user = await CommonData.LoadTableDataById<UserModel>(OperationNames.User, userFolderPermission.UserId, transaction);

			await AuditTrailData.SaveAuditTrail(new()
			{
				Action = AuditTrailActionTypes.Delete.ToString(),
				TableName = PermissionsNames.UserFolderPermission,
				RecordNo = user.Name + " - " + userFolderPermission.FolderPath,
				CreatedBy = userId,
				CreatedFromPlatform = platform
			}, transaction);
		});

	private static async Task ValidateTransaction(UserFolderPermissionModel userFolderPermission)
	{
		userFolderPermission.FolderPath = userFolderPermission.FolderPath?.Trim() ?? string.Empty;

		if (string.IsNullOrWhiteSpace(userFolderPermission.FolderPath))
			throw new Exception("Folder path is required. Please enter a valid folder path.");

		if (userFolderPermission.UserId <= 0)
			throw new Exception("User ID is required. Please enter a valid user ID.");

		var user = await CommonData.LoadTableDataById<UserModel>(OperationNames.User, userFolderPermission.UserId);
		if (user is null || !user.Status)
			throw new Exception("User not found or inactive. Please select a valid user.");

		var allUserFolderPermissions = await CommonData.LoadTableData<UserFolderPermissionModel>(PermissionsNames.UserFolderPermission);

		var existingPermission = allUserFolderPermissions.FirstOrDefault(existing =>
			existing.Id != userFolderPermission.Id &&
			existing.UserId == userFolderPermission.UserId &&
			existing.FolderPath.Equals(userFolderPermission.FolderPath, StringComparison.OrdinalIgnoreCase));

		if (existingPermission is not null)
			throw new Exception($"Folder path '{userFolderPermission.FolderPath}' already exists for this user. Please use a different folder path.");
	}

	public static async Task<int> SaveTransaction(UserFolderPermissionModel userFolderPermission, int userId, string platform)
	{
		await ValidateTransaction(userFolderPermission);

		var isUpdate = userFolderPermission.Id > 0;
		var previous = isUpdate
			? await CommonData.LoadTableDataById<UserFolderPermissionModel>(PermissionsNames.UserFolderPermission, userFolderPermission.Id)
			: null;

		return await SqlDataAccessTransaction.Run(async transaction =>
		{
			var id = await InsertUserFolderPermission(userFolderPermission, transaction);
			var diff = AuditTrailData.GetDifference(previous, userFolderPermission);
			var user = await CommonData.LoadTableDataById<UserModel>(OperationNames.User, userFolderPermission.UserId, transaction);

			await AuditTrailData.SaveAuditTrail(new()
			{
				Action = isUpdate ? AuditTrailActionTypes.Update.ToString() : AuditTrailActionTypes.Insert.ToString(),
				TableName = PermissionsNames.UserFolderPermission,
				RecordNo = user.Name + " - " + userFolderPermission.FolderPath,
				RecordValue = diff,
				CreatedBy = userId,
				CreatedFromPlatform = platform
			}, transaction);
			return id;
		});
	}
}
