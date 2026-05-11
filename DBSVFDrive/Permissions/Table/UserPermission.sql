CREATE TABLE [dbo].[UserPermission]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
	[UserId] INT NOT NULL, 
	[Path] VARCHAR(MAX) NOT NULL,
	[IsFile] BIT NOT NULL,
	[Read] BIT NOT NULL,
	[Write] BIT NOT NULL,
    CONSTRAINT [FK_UserPermission_ToUser] FOREIGN KEY ([UserId]) REFERENCES [dbo].[User]([Id])
)
