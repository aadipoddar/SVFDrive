CREATE TABLE [dbo].[UserFolderPermission]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
	[UserId] INT NOT NULL, 
	[FolderPath] VARCHAR(MAX) NOT NULL,
	[Read] BIT NOT NULL,
	[Write] BIT NOT NULL,
    CONSTRAINT [FK_UserFolderPermission_ToUser] FOREIGN KEY ([UserId]) REFERENCES [dbo].[User]([Id])
)
