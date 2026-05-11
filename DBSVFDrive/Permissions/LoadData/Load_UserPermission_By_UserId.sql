CREATE PROCEDURE [dbo].[Load_UserPermission_By_UserId]
	@UserId INT
AS
BEGIN

	SELECT *
	FROM UserPermission
	WHERE UserId = @UserId

END