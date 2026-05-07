CREATE PROCEDURE [dbo].[Delete_UserFolderPermission_By_Id]
	@Id INT
AS
BEGIN

	DELETE FROM UserFolderPermission
	WHERE Id = @Id

END