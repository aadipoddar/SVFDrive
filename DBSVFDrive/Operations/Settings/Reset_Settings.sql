CREATE PROCEDURE [dbo].[Reset_Settings]
AS
BEGIN
	DELETE FROM [Settings]

	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'EnableLoginWithCode'				, N'true'	, N'Enable or disable login with code feature')
	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'MaxLoginAttempts'				, N'5'		, N'Maximum number of login attempts before lockout')
	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'EnableUsersToResetPassword'		, N'true'	, N'Allow users to reset their passwords')
	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'CodeResendLimit'					, N'3'		, N'Maximum number of code resends allowed')
	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'CodeExpiryMinutes'				, N'10'		, N'Expiry time for codes in minutes')

	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'MainDriveFolder'					, N'/mnt/mni'	, N'Primary root folder path used by the application')
	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'FileManagerApiBase'				, N'http://103.170.167.11:5033/'	, N'Base URL of the EJ2 File Manager API')

	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'AutoRefreshReportTimer'			, N'5', N'Auto refresh interval for reports in minutes')

END
