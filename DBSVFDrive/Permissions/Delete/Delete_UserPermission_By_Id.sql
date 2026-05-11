CREATE PROCEDURE [dbo].[Delete_UserPermission_By_Id]
	@Id INT
AS
BEGIN

	DELETE FROM UserPermission
	WHERE Id = @Id

END