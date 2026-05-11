using SVFDriveLibrary.Data.Common;
using SVFDriveLibrary.Data.Operations;
using SVFDriveLibrary.DataAccess;
using SVFDriveLibrary.Models.Operations;
using SVFDriveLibrary.Models.Permissions;

namespace SVFDriveLibrary.Data.Permissions;

public static class UserPermissionData
{
	private static async Task<int> InsertUserPermission(UserPermissionModel userPermissionModel, SqlDataAccessTransaction transaction = null) =>
		(await SqlDataAccess.LoadData<int, dynamic>(PermissionsNames.InsertUserPermission, userPermissionModel, transaction)).FirstOrDefault()
			is var id and > 0 ? id : throw new InvalidOperationException("Failed to Insert User Permission.");

	public static async Task<List<UserPermissionModel>> LoadUserPermissionByUserId(int UserId) =>
		await SqlDataAccess.LoadData<UserPermissionModel, dynamic>(PermissionsNames.LoadUserPermissionByUserId, new { UserId });

	public static async Task DeleteUserPermissionById(int Id) =>
		await SqlDataAccess.SaveData(PermissionsNames.DeleteUserPermissionById, new { Id });

	public static async Task DeleteTransaction(UserPermissionModel userPermission, int userId, string platform) =>
		await SqlDataAccessTransaction.Run(async transaction =>
		{
			await DeleteUserPermissionById(userPermission.Id);

			var user = await CommonData.LoadTableDataById<UserModel>(OperationNames.User, userPermission.UserId, transaction);

			await AuditTrailData.SaveAuditTrail(new()
			{
				Action = AuditTrailActionTypes.Delete.ToString(),
				TableName = PermissionsNames.UserPermission,
				RecordNo = user.Name + " - " + userPermission.Path,
				CreatedBy = userId,
				CreatedFromPlatform = platform
			}, transaction);
		});

	private static async Task ValidateTransaction(UserPermissionModel userPermission)
	{
		userPermission.Path = userPermission.Path?.Trim() ?? string.Empty;

		if (string.IsNullOrWhiteSpace(userPermission.Path))
			throw new Exception("Folder path is required. Please enter a valid folder path.");

		if (userPermission.UserId <= 0)
			throw new Exception("User ID is required. Please enter a valid user ID.");

		var user = await CommonData.LoadTableDataById<UserModel>(OperationNames.User, userPermission.UserId);
		if (user is null || !user.Status)
			throw new Exception("User not found or inactive. Please select a valid user.");

		var allUserPermissions = await CommonData.LoadTableData<UserPermissionModel>(PermissionsNames.UserPermission);

		var existingPermission = allUserPermissions.FirstOrDefault(existing =>
			existing.Id != userPermission.Id &&
			existing.UserId == userPermission.UserId &&
			existing.Path.Equals(userPermission.Path, StringComparison.OrdinalIgnoreCase));

		if (existingPermission is not null)
			throw new Exception($"Folder path '{userPermission.Path}' already exists for this user. Please use a different folder path.");
	}

	public static async Task<int> SaveTransaction(UserPermissionModel userPermission, int userId, string platform)
	{
		await ValidateTransaction(userPermission);

		var isUpdate = userPermission.Id > 0;
		var previous = isUpdate
			? await CommonData.LoadTableDataById<UserPermissionModel>(PermissionsNames.UserPermission, userPermission.Id)
			: null;

		return await SqlDataAccessTransaction.Run(async transaction =>
		{
			var id = await InsertUserPermission(userPermission, transaction);
			var diff = AuditTrailData.GetDifference(previous, userPermission);
			var user = await CommonData.LoadTableDataById<UserModel>(OperationNames.User, userPermission.UserId, transaction);

			await AuditTrailData.SaveAuditTrail(new()
			{
				Action = isUpdate ? AuditTrailActionTypes.Update.ToString() : AuditTrailActionTypes.Insert.ToString(),
				TableName = PermissionsNames.UserPermission,
				RecordNo = user.Name + " - " + userPermission.Path,
				RecordValue = diff,
				CreatedBy = userId,
				CreatedFromPlatform = platform
			}, transaction);
			return id;
		});
	}
}
