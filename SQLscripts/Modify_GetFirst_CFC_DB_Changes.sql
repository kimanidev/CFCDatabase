ALTER PROCEDURE dbo.GetFirst_CFC_DB_Changes
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @TABLE_NAME NVARCHAR(128) = 'CFC_DB_Changes';
	DECLARE @TargetTable NVARCHAR(128);
	
    -- Insert statements for procedure here
	IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @TABLE_NAME)
	BEGIN
		SELECT TOP 1 @TargetTable = TABLE_NAME
		FROM INFORMATION_SCHEMA.TABLES
		WHERE TABLE_TYPE = 'BASE TABLE'
		ORDER BY TABLE_NAME;
		
		CREATE TABLE [dbo].[CFC_DB_Changes](
			[DB_Change_GUID] [uniqueidentifier] NOT NULL,
			[CFC_DB_Name] [nvarchar](50) NOT NULL,
			[CFC_DB_Major_Version] [smallint] NOT NULL DEFAULT (0),
			[CFC_DB_Minor_Version] [smallint] NOT NULL DEFAULT (0),
			[Seq_No] [int] NOT NULL,
			[Table_Name] [nvarchar](100) NOT NULL,
			[Change_Description] [nvarchar](max) NULL,
			[Created_By] [nvarchar](50) NOT NULL,
			[Created_Date] [datetime] NOT NULL,
			[Last_Update_By] [nvarchar](50) NOT NULL,
			[Last_Update] [datetime] NOT NULL,
		 CONSTRAINT [PK_CFC_DB_Changes] PRIMARY KEY CLUSTERED 
		(
			[DB_Change_GUID] ASC
		)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
		) ON [PRIMARY];
		
		INSERT INTO [CFC_DB_Changes]
			   ([DB_Change_GUID], [CFC_DB_Name], [Seq_No], [Table_Name]
			   ,[Change_Description], [Created_By], [Created_Date], [Last_Update_By], [Last_Update])
		 VALUES
			   (NEWID(), DB_NAME(), 1, @TargetTable
			   ,N'', SUSER_SNAME(), GETDATE(), SUSER_SNAME(), GETDATE()
			   );
	END
	
	SELECT TOP 1 * 
	FROM [dbo].[CFC_DB_Changes]
	ORDER BY [CFC_DB_Major_Version] DESC, [CFC_DB_Minor_Version] DESC;
END
GO
