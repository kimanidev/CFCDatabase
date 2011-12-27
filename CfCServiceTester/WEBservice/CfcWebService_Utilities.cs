using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using Microsoft.SqlServer.Management.Smo;
using CfCServiceTester.WEBservice.DataObjects;

namespace CfCServiceTester.WEBservice
{
    /// <summary>
    /// Listed below methods are designed for service only.
    /// </summary>
    public partial class CfcWebService
    {
        /// <summary>
        /// The application stores connection string in <code>Session[ConnectionStringKey]</code> and verifies credentials.
        /// </summary>
        public static readonly string ConnectionStringKey = "{61F8CB8C-9AE6-429D-8900-0BCE6AB4C956}";
        /// <summary>
        /// List of roles for the connected user.
        /// </summary>
        public static readonly string RoleListKey = "{12FB9A22-7C13-4338-881E-62D24367B6E5}";
        public static readonly string SqlServerNameKey = "{0B568310-9686-4CF9-B38C-99ED7FE48D87}";
        public static readonly string DatabaseNameKey = "{93CA56A2-3CE6-456D-86E6-0C9849CF363E}";

        internal static string ConnectionString
        {
            get { return (string)HttpContext.Current.Session[ConnectionStringKey]; }
        }
        internal static List<string> RoleList
        {
            get { return (List<string>)HttpContext.Current.Session[RoleListKey]; }
        }
        public static string SqlServerName
        {
            get { return (string)HttpContext.Current.Session[SqlServerNameKey]; }
        }
        public static string DatabaseName
        {
            get { return (string)HttpContext.Current.Session[DatabaseNameKey]; }
        }

        /// <summary>
        /// The function creates connection string and verifies credentials.
        /// </summary>
        /// <param name="dataSource">SQL server's name</param>
        /// <param name="initialCatalog">Name of the database </param>
        /// <param name="userName">Login name</param>
        /// <param name="password">Password</param>
        /// <param name="isValid"><code>true</code> - connection is valid, <code>false</code> - connection failed</param>
        /// <returns>
        ///     Connection string (when <code>isValid == true</code> or error message (when <code>isValid == false</code>).
        /// </returns>
        private static string BuildSqlConnection(string dataSource, string initialCatalog, string userName, string password, 
                                                 out bool isValid)
        {
            var csb = new SqlConnectionStringBuilder() 
            { 
                DataSource = dataSource,
                InitialCatalog = initialCatalog,
                UserID = userName,
                Password = password
            };
            try
            {
                var sqlConnection = new SqlConnection(csb.ConnectionString);
                sqlConnection.Open();   // Verify connection
                sqlConnection.Close();
                HttpContext.Current.Session[ConnectionStringKey] = csb.ConnectionString;
                HttpContext.Current.Session[SqlServerNameKey] = dataSource;
                HttpContext.Current.Session[DatabaseNameKey] = initialCatalog;

                isValid = true;
                return csb.ConnectionString;
            }
            catch (Exception ex)
            {
                isValid = false;
                return ex.Message;
            }
        }
        
        /// <summary>
        /// The function returns List of roles for user defined in the parameter.
        /// The function quries data with Linq to datable 
        /// <see cref="http://msdn.microsoft.com/en-us/library/system.data.datatableextensions_methods.aspx"/>
        /// </summary>
        /// <param name="userName">User's name</param>
        /// <returns>List of roles for the user</returns>
        public static List<string> GetUsersRoles(string userName = null)
        {
            const string queryString =
                "select rls.name as RoleName " +
                "from sys.database_role_members m " +
                    "join sys.database_principals usr on usr.principal_id = m.member_principal_id " +
                    "join sys.database_principals rls on rls.principal_id = m.role_principal_id " +
                "where usr.name = @UserName;";
            if (String.IsNullOrEmpty(userName))
                return RoleList;
            else
            {
                using (var connection = new SqlConnection(ConnectionString))
                {
                    var da = new SqlDataAdapter(queryString, connection);
                    da.SelectCommand.Parameters.AddWithValue("@UserName", userName);
                    da.TableMappings.Add("Table", "RoleList");
                
                    var ds = new DataSet();
                    da.Fill(ds);
                    DataTable roles = ds.Tables["RoleList"];
                    var rzlt = (
                        from role in roles.AsEnumerable()
                        select role.Field<string>("RoleName")
                        ).ToList();
                    HttpContext.Current.Session[RoleListKey] = rzlt;

                    return rzlt;
                }
            }
        }
/*
        public static long MakeBackup(string fileName)
        {
            const string BackupCommand = @"BACKUP DATABASE {0} TO DISK='{1}'";
            string sql = String.Format(BackupCommand, DatabaseName, fileName);
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand(sql, connection);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
            var fInfo = new FileInfo(fileName);
            return fInfo.Length;
        }
*/
        /// <summary>
        /// Writes SQL backup into file defined in the parameter. Procedure is using SMO data objects.
        /// <see cref="http://msdn.microsoft.com/en-us/library/microsoft.sqlserver.management.smo.backup.aspx"/>
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <returns>File size</returns>
        public static long MakeBackup(string fileName)
        {
            var server = new Server(SqlServerName);
            var backup = new Backup()
                {
                    Database = DatabaseName,
                    Action = BackupActionType.Database,
                    Initialize = true,
                };
            
            backup.Devices.AddDevice(fileName, DeviceType.File);
            backup.SqlBackup(server);

            var fInfo = new FileInfo(fileName);
            return fInfo.Length;
        }

        /// <summary>
        /// /// Resdtores database from file.
        /// <see cref="http://msdn.microsoft.com/en-us/library/microsoft.sqlserver.management.smo.restore.sqlrestore.aspx"/>
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="databaseName">Database name</param>
        /// <param name="withReplace"><code>true</code> - with replace</param>
        public static void Restore(string fileName, string databaseName, bool withReplace)
        {

            string sourceLogFileName = databaseName + "_log";
            var server = new Server(SqlServerName);
            string rootPath = server.MasterDBPath;
            if (!rootPath.EndsWith(@"\"))
                rootPath += @"\";

            var dbFile = Path.Combine(rootPath, String.Format("{0}.mdf", databaseName));
            var logFile = Path.Combine(rootPath, String.Format("{0}.ldf", sourceLogFileName));
            var restore = new Restore()
                {
                    Database = databaseName,
                    NoRecovery = false,
                    FileNumber = 1,
                    ReplaceDatabase = withReplace,
                };
            restore.Devices.AddDevice(fileName, DeviceType.File);

            DbLogicalFileNames logicalFileNames = GetDbLogicalFileNames(fileName);
            restore.RelocateFiles.Add(new RelocateFile(logicalFileNames.LogicalNameData, dbFile));
            restore.RelocateFiles.Add(new RelocateFile(logicalFileNames.LogicalNameLog, logFile));
            
            restore.SqlRestore(server);
        }

        /// <summary>
        /// Get logical file names from the backup file.
        /// <see cref="http://stackoverflow.com/questions/7089627/how-can-i-retrieve-the-logical-file-name-of-the-database-from-backup-file"/>
        /// </summary>
        /// <param name="fileName">Full path to Backup file</param>
        /// <returns>Structure with logical file names.</returns>
        private static DbLogicalFileNames GetDbLogicalFileNames(string fileName)
        {
            const string query = 
                @"USE {0}; " +
                 "DECLARE @Table TABLE ( " +
	                "LogicalName varchar(128), " +
	                "[PhysicalName] varchar(128), " +
	                "[Type] varchar, " +
	                "[FileGroupName] varchar(128), " +
	                "[Size] varchar(128), " +
                    "[MaxSize] varchar(128), " +
                    "[FileId]varchar(128), " +
                    "[CreateLSN]varchar(128), " +
                    "[DropLSN]varchar(128), " +
                    "[UniqueId]varchar(128), " +
                    "[ReadOnlyLSN]varchar(128), " +
                    "[ReadWriteLSN]varchar(128), " +
                    "[BackupSizeInBytes]varchar(128), " +
                    "[SourceBlockSize]varchar(128), " +
                    "[FileGroupId]varchar(128), " +
                    "[LogGroupGUID]varchar(128), " +
                    "[DifferentialBaseLSN]varchar(128), " +
                    "[DifferentialBaseGUID]varchar(128), " +
                    "[IsReadOnly]varchar(128), " +
                    "[IsPresent]varchar(128), " +
                    "[TDEThumbprint]varchar(128) " +
                "); " +
                "DECLARE @Path varchar(1000)=N'{1}'; " +
                "DECLARE @LogicalNameData varchar(128), @LogicalNameLog varchar(128); " +
                "INSERT INTO @table " +
                "EXEC(' " +
	                "RESTORE FILELISTONLY " +
	                "FROM DISK=''' + @Path + ''' " +
                "'); " +
                "SET @LogicalNameData=(SELECT LogicalName FROM @Table WHERE Type='D'); " +
                "SET @LogicalNameLog=(SELECT LogicalName FROM @Table WHERE Type='L'); " +
                "SELECT @LogicalNameData AS [LogicalNameData], @LogicalNameLog AS [LogicalNameLog]; ";

            using (var connection = new SqlConnection(ConnectionString))
            {
                var da = new SqlDataAdapter(String.Format(query, DatabaseName, fileName), connection);
                da.TableMappings.Add("Table", "LogicalFileNames");

                var ds = new DataSet();
                da.Fill(ds);
                DataTable logicalFileNames = ds.Tables["LogicalFileNames"];
                var rzlt = (
                    from names in logicalFileNames.AsEnumerable()
                    select new DbLogicalFileNames()
                        {
                            LogicalNameData = names.Field<string>("LogicalNameData"),
                            LogicalNameLog = names.Field<string>("LogicalNameLog")
                        }
                    ).FirstOrDefault();

                return rzlt; 
            }
        }

        /// <summary>
        /// Renames file adding suffix .bak
        /// </summary>
        /// <param name="fileName">File name (absolute path)</param>
        public static void RenameFile(string fileName)
        {
            if (File.Exists(fileName))
            {
                string newFileName = fileName + ".bak";
                RenameFile(newFileName);
                File.Move(fileName, newFileName);
            }
        }

        public static IList<string> GetBackupFilenames(string backupDirectory, string template)
        {
            string[] filePaths = Directory.GetFiles(backupDirectory, "*.bak");
            for (int i = 0; i < filePaths.Length; i++)
                filePaths[i] = Path.GetFileName(filePaths[i]);

            if (String.IsNullOrEmpty(template))
                return new List<string>(filePaths);
            else
            {
                string uTemplate = template.ToUpper();
                var query = (
                    from string fileName in filePaths
                    where fileName.ToUpper().Contains(uTemplate)
                    select fileName
                    ).ToList();
                return query;
            }
        }
    }
}