using SVFDriveLibrary.DataAccess;
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
}
