CREATE PROCEDURE [dbo].[Insert_UserFolderPermission]
    @Id INT OUTPUT,
	@UserId INT,
	@FolderPath VARCHAR(MAX),
	@Read BIT,
	@Write BIT
AS
BEGIN
	IF @Id = 0
	BEGIN
		INSERT INTO [dbo].[UserFolderPermission]
		(
			[UserId],
			[FolderPath],
			[Read],
			[Write]
		)
		VALUES
		(
			@UserId,
			@FolderPath,
			@Read,
			@Write
		);

		SET @Id = SCOPE_IDENTITY();
	END

	ELSE
	BEGIN
		UPDATE [dbo].[UserFolderPermission]
		SET 
			[UserId] = @UserId,
			[FolderPath] = @FolderPath,
			[Read] = @Read,
			[Write] = @Write
		WHERE
			[Id] = @Id
	END

	SELECT @Id AS Id;
END