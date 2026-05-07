using SVFDriveLibrary.Data.Common;
using SVFDriveLibrary.DataAccess;
using SVFDriveLibrary.Exports.Utils;
using SVFDriveLibrary.Models.Operations;
using SVFDriveLibrary.Models.Permissions;

namespace SVFDriveLibrary.Exports.Permissions;

public static class UserFolderPermissionExport
{
	public static async Task<(MemoryStream stream, string fileName)> ExportMaster(
		IEnumerable<UserFolderPermissionModel> userData,
		ReportExportType exportType)
	{
		var users = await CommonData.LoadTableData<UserModel>(OperationNames.User);

		var enrichedData = userData.Select(user => new
		{
			user.Id,
			User = users.FirstOrDefault(u => u.Id == user.UserId)?.Name ?? "Unknown",
			user.FolderPath,
			Read = user.Read ? "Yes" : "No",
			Write = user.Write ? "Yes" : "No"
		});

		var columnSettings = new Dictionary<string, ReportColumnSetting>
		{
			[nameof(UserFolderPermissionModel.Id)] = new() { DisplayName = "ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(UserFolderPermissionModel.FolderPath)] = new() { DisplayName = "Folder Path", Alignment = CellAlignment.Left, IsRequired = true },
			["User"] = new() { DisplayName = "User", Alignment = CellAlignment.Left, IsRequired = true },
			[nameof(UserFolderPermissionModel.Read)] = new() { DisplayName = "Read", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(UserFolderPermissionModel.Write)] = new() { DisplayName = "Write", Alignment = CellAlignment.Center, IncludeInTotal = false }
		};

		List<string> columnOrder =
		[
			nameof(UserFolderPermissionModel.Id),
			"User",
			nameof(UserFolderPermissionModel.FolderPath),
			nameof(UserFolderPermissionModel.Read),
			nameof(UserFolderPermissionModel.Write)
		];

		var currentDateTime = await CommonData.LoadCurrentDateTime();
		var fileName = $"User_Folder_Permission_Master_{currentDateTime:yyyyMMdd_HHmmss}";

		if (exportType == ReportExportType.PDF)
		{
			var stream = await PDFReportExportUtil.ExportToPdf(
				enrichedData,
				"USER FOLDER PERMISSION MASTER",
				null,
				null,
				columnSettings,
				columnOrder,
				useBuiltInStyle: false,
				useLandscape: true
			);

			return (stream, fileName + ".pdf");
		}
		else
		{
			var stream = await ExcelReportExportUtil.ExportToExcel(
				enrichedData,
				"USER FOLDER PERMISSION MASTER",
				"User Folder Permission Data",
				null,
				null,
				columnSettings,
				columnOrder
			);

			return (stream, fileName + ".xlsx");
		}
	}
}
