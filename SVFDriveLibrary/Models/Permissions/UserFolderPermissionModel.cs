namespace SVFDriveLibrary.Models.Permissions;

public class UserFolderPermissionModel
{
	public int Id { get; set; }
	public int UserId { get; set; }
	public string FolderPath { get; set; }
	public bool Read { get; set; }
	public bool Write { get; set; }
}
