CREATE PROCEDURE [dbo].[Insert_UserPermission]
    @Id INT OUTPUT,
	@UserId INT,
	@Path VARCHAR(MAX),
	@IsFile BIT,
	@Read BIT,
	@Write BIT
AS
BEGIN
	IF @Id = 0
	BEGIN
		INSERT INTO [dbo].[UserPermission]
		(
			[UserId],
			[Path],
			[IsFile],
			[Read],
			[Write]
		)
		VALUES
		(
			@UserId,
			@Path,
			@IsFile,
			@Read,
			@Write
		);

		SET @Id = SCOPE_IDENTITY();
	END

	ELSE
	BEGIN
		UPDATE [dbo].[UserPermission]
		SET 
			[UserId] = @UserId,
			[Path] = @Path,
			[IsFile] = @IsFile,
			[Read] = @Read,
			[Write] = @Write
		WHERE
			[Id] = @Id
	END

	SELECT @Id AS Id;
END