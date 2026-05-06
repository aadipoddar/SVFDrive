using SVFDriveLibrary.Data.Common;
using SVFDriveLibrary.Exports.Utils;
using SVFDriveLibrary.Models.Operations;

namespace SVFDriveLibrary.Exports.Operations;

public static class UserExport
{
	public static async Task<(MemoryStream stream, string fileName)> ExportMaster(
		IEnumerable<UserModel> userData,
		ReportExportType exportType)
	{
		var enrichedData = userData.Select(user => new
		{
			user.Id,
			user.Name,
			user.Phone,
			user.Email,
			Admin = user.Admin ? "Yes" : "No",
			user.Remarks,
			Status = user.Status ? "Active" : "Deleted"
		});

		var columnSettings = new Dictionary<string, ReportColumnSetting>
		{
			[nameof(UserModel.Id)] = new() { DisplayName = "ID", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(UserModel.Name)] = new() { DisplayName = "Name", Alignment = CellAlignment.Left, IsRequired = true },
			[nameof(UserModel.Phone)] = new() { DisplayName = "Phone", Alignment = CellAlignment.Left, IsRequired = true, IncludeInTotal = false },
			[nameof(UserModel.Email)] = new() { DisplayName = "Email", Alignment = CellAlignment.Left },
			[nameof(UserModel.Admin)] = new() { DisplayName = "Admin", Alignment = CellAlignment.Center, IncludeInTotal = false },
			[nameof(UserModel.Remarks)] = new() { DisplayName = "Remarks", Alignment = CellAlignment.Left },
			[nameof(UserModel.Status)] = new() { DisplayName = "Status", Alignment = CellAlignment.Center, IncludeInTotal = false }
		};

		List<string> columnOrder =
		[
			nameof(UserModel.Id),
			nameof(UserModel.Name),
			nameof(UserModel.Phone),
			nameof(UserModel.Email),
			nameof(UserModel.Admin),
			nameof(UserModel.Remarks),
			nameof(UserModel.Status)
		];

		var currentDateTime = await CommonData.LoadCurrentDateTime();
		var fileName = $"User_Master_{currentDateTime:yyyyMMdd_HHmmss}";

		if (exportType == ReportExportType.PDF)
		{
			var stream = await PDFReportExportUtil.ExportToPdf(
				enrichedData,
				"USER MASTER",
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
				"USER MASTER",
				"User Data",
				null,
				null,
				columnSettings,
				columnOrder
			);

			return (stream, fileName + ".xlsx");
		}
	}
}