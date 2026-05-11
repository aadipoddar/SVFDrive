CREATE PROCEDURE [dbo].[Insert_UserPermission]
    @Id INT OUTPUT,
	@UserId INT,
	@Path VARCHAR(MAX),
	@IsFile BIT,
	@Deny BIT,
	@ShowHidden BIT,
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
			[Deny],
			[ShowHidden],
			[Write]
		)
		VALUES
		(
			@UserId,
			@Path,
			@IsFile,
			@Deny,
			@ShowHidden,
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
			[Deny] = @Deny,
			[ShowHidden] = @ShowHidden,
			[Write] = @Write
		WHERE
			[Id] = @Id
	END

	SELECT @Id AS Id;
END