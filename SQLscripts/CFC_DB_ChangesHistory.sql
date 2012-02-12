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
