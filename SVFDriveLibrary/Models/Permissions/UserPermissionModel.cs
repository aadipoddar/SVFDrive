namespace SVFDriveLibrary.Models.Permissions;

public class UserPermissionModel
{
	public int Id { get; set; }
	public int UserId { get; set; }
	public string Path { get; set; }
	public bool IsFile { get; set; }
	public bool Deny { get; set; }
	public bool Write { get; set; }
}
