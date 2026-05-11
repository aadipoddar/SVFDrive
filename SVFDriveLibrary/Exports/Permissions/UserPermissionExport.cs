using SVFDriveLibrary.Data.Common;
using SVFDriveLibrary.DataAccess;
using SVFDriveLibrary.Exports.Utils;
using SVFDriveLibrary.Models.Operations;
using SVFDriveLibrary.Models.Permissions;

namespace SVFDriveLibrary.Exports.Permissions;

public static class UserPermissionExport
{
	public static async Task<(MemoryStream stream, string fileName)> ExportMaster(
		IEnumerable<UserPermissionModel> userData,
		ReportExportType exportType)
	{
		var users = await CommonData.LoadTableData<UserModel>(OperationNames.User);

		var enrichedData = userData.Select(user => new
		{
			user.Id,
			User = users.FirstOrDefault(u => u.Id == user.UserId)?.Name ?? "Unknown",
			user.Path,
			IsFile = user.IsFile ? "Yes" : "No",
			Deny = user.Deny ? "Yes" : "No",
			ShowHidden = user.ShowHidden ? "Yes" : "No",
			Write = user.Write ? "Yes" : "No"
		});

		var columnSettings = new Dictionary<string, ReportColumnSetting>
		{
			[nameof(UserPermissionModel.Id)] = new() { DisplayName = "ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(UserPermissionModel.Path)] = new() { DisplayName = "Path", Alignment = CellAlignment.Left, IsRequired = true },
			["User"] = new() { DisplayName = "User", Alignment = CellAlignment.Left, IsRequired = true },
			[nameof(UserPermissionModel.IsFile)] = new() { DisplayName = "Is File", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(UserPermissionModel.Deny)] = new() { DisplayName = "Deny", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(UserPermissionModel.ShowHidden)] = new() { DisplayName = "Show Hidden", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(UserPermissionModel.Write)] = new() { DisplayName = "Write", Alignment = CellAlignment.Center, IncludeInTotal = false }
		};

		List<string> columnOrder =
		[
			nameof(UserPermissionModel.Id),
			"User",
			nameof(UserPermissionModel.Path),
			nameof(UserPermissionModel.IsFile),
			nameof(UserPermissionModel.Deny),
			nameof(UserPermissionModel.ShowHidden),
			nameof(UserPermissionModel.Write)
		];

		var currentDateTime = await CommonData.LoadCurrentDateTime();
		var fileName = $"User_Permission_Master_{currentDateTime:yyyyMMdd_HHmmss}";

		if (exportType == ReportExportType.PDF)
		{
			var stream = await PDFReportExportUtil.ExportToPdf(
				enrichedData,
				"USER PERMISSION MASTER",
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
				"USER PERMISSION MASTER",
				"User Permission Data",
				null,
				null,
				columnSettings,
				columnOrder
			);

			return (stream, fileName + ".xlsx");
		}
	}
}
