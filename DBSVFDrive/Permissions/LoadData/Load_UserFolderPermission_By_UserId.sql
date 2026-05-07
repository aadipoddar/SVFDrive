CREATE PROCEDURE [dbo].[Load_UserFolderPermission_By_UserId]
	@UserId INT
AS
BEGIN

	SELECT *
	FROM UserFolderPermission
	WHERE UserId = @UserId

END