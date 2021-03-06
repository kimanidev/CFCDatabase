/*
exec dbo.usp_KillUsers 'Northwind_new1';
*/
/****** Object:  StoredProcedure [dbo].[usp_KillUsers]    Script Date: 02/08/2012 22:16:08 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[usp_KillUsers] @dbname varchar(50) as
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
