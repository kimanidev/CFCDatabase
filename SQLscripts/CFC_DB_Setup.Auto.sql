-- ================================================
-- Template generated from Template Explorer using:
-- Create Procedure (New Menu).SQL
--
-- Use the Specify Values for Template Parameters 
-- command (Ctrl-Shift-M) to fill in the parameter 
-- values below.
--
-- This block of comments will not be included in
-- the definition of the procedure.
-- ================================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CFC_DB_ChangesHistory]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[CFC_DB_ChangesHistory]
GO

-- =============================================
-- Author:		Gediminas Bukauskas
-- Create date: 2012-02-11
-- Description:	Writes new history record into the database
-- =============================================
CREATE PROCEDURE dbo.CFC_DB_ChangesHistory
	@DatabaseName [nvarchar](50),
	@TableName [nvarchar](128),
	@MajorVersion [smallint] = 1,
	@MinorVersion [smallint] = 0,
	@ChangeDescription [nvarchar](max)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	DECLARE @DbChangeGuid [uniqueidentifier];
	DECLARE @SeqNo [int];
	DECLARE @OldChangeDescription [nvarchar](max);
	DECLARE @NewLine [char](2) = CHAR(13) + CHAR(10);
	
	IF NOT EXISTS (
		SELECT * FROM [dbo].[CFC_DB_Changes]
		WHERE CFC_DB_Name = @DatabaseName AND
		  CFC_DB_Major_Version = @MajorVersion AND CFC_DB_Minor_Version = @MinorVersion
		)
	BEGIN	
		SET @DbChangeGuid = NEWID();	-- Create new data base version
		INSERT INTO [dbo].[CFC_DB_Changes]
           ([DB_Change_GUID], [CFC_DB_Name], [CFC_DB_Major_Version], [CFC_DB_Minor_Version],
            [Seq_No], [Table_Name], [Change_Description], [Created_By], [Created_Date],
            [Last_Update_By], [Last_Update])
		VALUES
           (@DbChangeGuid, @DatabaseName, @MajorVersion, @MinorVersion,
            1, @TableName, @ChangeDescription, SUSER_SNAME(), GETDATE(),
            SUSER_SNAME(), GETDATE());
	END
	ELSE
	BEGIN
		SELECT @DbChangeGuid = DB_Change_GUID, @OldChangeDescription = Change_Description
		FROM dbo.CFC_DB_Changes
		WHERE CFC_DB_Name = @DatabaseName AND
			  CFC_DB_Major_Version = @MajorVersion AND CFC_DB_Minor_Version = @MinorVersion AND
			  Table_Name = @TableName;
		IF @@ROWCOUNT < 1
		BEGIN
			SET @DbChangeGuid = NEWID();	-- Create new table description in the data base version
			SELECT @SeqNo = MAX(Seq_No) FROM dbo.CFC_DB_Changes
			WHERE CFC_DB_Name = @DatabaseName AND
				  CFC_DB_Major_Version = @MajorVersion AND CFC_DB_Minor_Version = @MinorVersion;
			INSERT INTO [dbo].[CFC_DB_Changes]
			   ([DB_Change_GUID], [CFC_DB_Name], [CFC_DB_Major_Version], [CFC_DB_Minor_Version],
				[Seq_No], [Table_Name], [Change_Description], [Created_By], [Created_Date],
				[Last_Update_By], [Last_Update])
			VALUES
			   (@DbChangeGuid, @DatabaseName, @MajorVersion, @MinorVersion,
				@SeqNo + 1, @TableName, @ChangeDescription, SUSER_SNAME(), GETDATE(),
				SUSER_SNAME(), GETDATE());
		END
		ELSE
		BEGIN
			UPDATE [dbo].[CFC_DB_Changes]
			   SET [Change_Description] = @OldChangeDescription + @NewLine + @ChangeDescription, 
				   [Last_Update_By] = SUSER_SNAME(), [Last_Update] = GETDATE()
			WHERE DB_Change_GUID = @DbChangeGuid;
		END
	END
	SELECT @DbChangeGuid AS DB_Change_GUID;
END
GO

-- ================================================
-- Template generated from Template Explorer using:
-- Create Procedure (New Menu).SQL
--
-- Use the Specify Values for Template Parameters 
-- command (Ctrl-Shift-M) to fill in the parameter 
-- values below.
--
-- This block of comments will not be included in
-- the definition of the procedure.
-- ================================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
	dbo.GetFirst_CFC_DB_Changes
*/

-- =============================================
-- Author:		Gediminas Bukauskas
-- Create date: 2012-02-11
-- Description:	Returns latest record in the CFC_DB_Changes
-- =============================================
IF  EXISTS (SELECT * FROM sys.objects 
			WHERE object_id = OBJECT_ID(N'[dbo].[GetFirst_CFC_DB_Changes]') AND 
			      type in (N'P', N'PC')
			)
	DROP PROCEDURE [dbo].[GetFirst_CFC_DB_Changes]
GO

CREATE PROCEDURE dbo.GetFirst_CFC_DB_Changes
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
	ORDER BY [Last_Update] DESC;
END
GO

/*
exec dbo.usp_KillUsers 'Northwind_new1';
*/
/****** Object:  StoredProcedure [dbo].[usp_KillUsers]    Script Date: 02/08/2012 22:16:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF  EXISTS (SELECT * FROM sys.objects 
			WHERE object_id = OBJECT_ID(N'[dbo].[usp_KillUsers]') AND 
			      type in (N'P', N'PC')
			)
	DROP PROCEDURE [dbo].[usp_KillUsers]
GO

CREATE PROCEDURE [dbo].[usp_KillUsers] @dbname varchar(50) as
BEGIN
	SET NOCOUNT ON
	DECLARE @strSQL varchar(255)
	PRINT 'Killing Users'
	PRINT '-----------------'
	CREATE table #tmpUsers(
	 spid int,
	 eid int,
	 status varchar(30),
	 loginname varchar(50),
	 hostname varchar(50),
	 blk int,
	 dbname varchar(50),
	 cmd varchar(30),
	 request_id [int])
	INSERT INTO #tmpUsers EXEC SP_WHO
	DECLARE LoginCursor CURSOR
	READ_ONLY
	FOR SELECT spid, dbname FROM #tmpUsers WHERE dbname = @dbname
	DECLARE @spid varchar(10)
	DECLARE @dbname2 varchar(40)
	OPEN LoginCursor
	FETCH NEXT FROM LoginCursor INTO @spid, @dbname2
	WHILE (@@fetch_status <> -1)
	BEGIN
			IF (@@fetch_status <> -2)
			BEGIN
			PRINT 'Killing ' + @spid
			SET @strSQL = 'KILL ' + @spid
			EXEC (@strSQL)
			END
			FETCH NEXT FROM LoginCursor INTO  @spid, @dbname2
	END
	CLOSE LoginCursor
	DEALLOCATE LoginCursor
	DROP table #tmpUsers
	PRINT 'Done'
END
GO

DECLARE @TABLE_NAME NVARCHAR(128) = 'CFC_DB_Changes';

IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @TABLE_NAME)
	DROP TABLE [dbo].[CFC_DB_Changes];

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
GO
