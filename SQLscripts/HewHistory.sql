DECLARE @TABLE_NAME NVARCHAR(128) = 'CFC_DB_Changes';

IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @TABLE_NAME)
BEGIN
	CREATE TABLE [dbo].[CFC_DB_Changes](
		[DB_Change_GUID] [uniqueidentifier] NOT NULL,
		[CFC_DB_Name] [nvarchar](50) NOT NULL,
		[CFC_DB_Major_Version] [smallint] NOT NULL DEFAULT (1),
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
           (NEWID(), DB_NAME(), 1, @TABLE_NAME
           ,N'', SUSER_SNAME(), GETDATE(), SUSER_SNAME(), GETDATE()
           );
END
ELSE
BEGIN
	IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'CFC_DB_Version'  
            AND Object_ID = Object_ID(@TABLE_NAME))
    BEGIN
		ALTER TABLE CFC_DB_Changes DROP COLUMN CFC_DB_Version;
		ALTER TABLE CFC_DB_Changes ADD
			[CFC_DB_Major_Version] [smallint] NOT NULL DEFAULT (1),
			[CFC_DB_Minor_Version] [smallint] NOT NULL DEFAULT (0);
    END
END
GO
