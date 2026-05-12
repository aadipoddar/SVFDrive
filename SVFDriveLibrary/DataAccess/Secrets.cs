using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace SVFDriveLibrary.DataAccess;

public static partial class Secrets
{
	public static string DatabaseName => "SVFDrive";

	public static string AzureConnectionString = GetSecret(nameof(AzureConnectionString));
	public static string LocalConnectionString = "Data Source=AADILAPIKIIT;Initial Catalog=SVFDrive;Integrated Security=True;Connect Timeout=300;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False";

	public static string SyncfusionLicense = GetSecret(nameof(SyncfusionLicense));

	public static string Email => "softaadi@gmail.com";
	public static string EmailPassword = GetSecret(nameof(EmailPassword));

	public static string ToEmail = GetSecret(nameof(ToEmail));
	public static string ToName => "SVF Drive";

	public static string OnlineFullLogoPath => "https://raw.githubusercontent.com/aadipoddar/SVFDrive/refs/heads/main/SVFDrive/SVFDrive.Web/wwwroot/images/logo.png";
	public static string AppWebsite => "https://svfdrive.azurewebsites.net";
	public static string CompanyWebsite => "https://svf.in/";
	
	private static string GetSecret(string key) =>
		new ConfigurationBuilder()
			.AddUserSecrets(Assembly.GetExecutingAssembly())
			.AddEnvironmentVariables()
			.Build()
			.GetSection(key).Value;
}