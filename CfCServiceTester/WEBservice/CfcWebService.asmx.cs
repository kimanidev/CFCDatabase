using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using CfCServiceTester.SVC.DataObjects;
using Microsoft.SqlServer.Management.Smo;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using CfCServiceTester.WEBservice.InternalTypes;
using CfCServiceTester.WEBservice.DataObjects;
using System.IO;

namespace CfCServiceTester.WEBservice
{
    /// <summary>
    /// Summary description for CfcWebService
    /// </summary>
    [WebService(Namespace = "http://CfCService.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public partial class CfcWebService : System.Web.Services.WebService
    {
        /// <summary>
        /// Ping method for testing service
        /// </summary>
        /// <returns>String</returns>
        [WebMethod]
        public string HelloWorld(string client)
        {
            return String.Format("Hello dear {0}. I am CfC WEB service.", client);
        }

        /// <summary>
        /// The method returns list of SQL servers that are visible for the service. The function will select names with phrase inside the name when
        /// parameter <code>namePattern</code> is not empty.
        /// <remarks>
        ///     Ensure that <code>SQL Server Browser</code> service is started on computers with installed SQL servers.
        ///     SmoApplication.EnumAvailableSqlServers is lying otherwise. 
        ///     SQLEXPRESS instance is not visible on local computer when <code>SQL Server Browser</code> is stopped.
        /// </remarks>
        /// </summary>
        /// <param name="localOnly"><code>true</code> - enumerate local computers only, <code>false</code> - all available SQL servers</param>
        /// <param name="namePattern">Phrase in the server's name.</param>
        /// <returns>List with server's names</returns>
        [WebMethod]
        public IEnumerable<SqlServerDbo> EnumerateSqlServers(bool localOnly, string namePattern)
        {
            DataTable dtSmo = SmoApplication.EnumAvailableSqlServers(localOnly);
            var rzlt = new List<SqlServerDbo>();
            SqlServerDbo dbo;

            foreach (DataRow dr in dtSmo.Rows)
            {
                dbo = new SqlServerDbo()
                {
                    Name = dr.Field<string>("Name"),
                    Server = dr.Field<string>("Server"),
                    Instance = dr.Field<string>("Instance"),
                    IsClustered = dr.Field<bool>("IsClustered"),
                    Version = dr.Field<string>("Version"),
                    IsLocal = dr.Field<bool>("IsLocal")
                };
                if (String.IsNullOrEmpty(namePattern) || dbo.Name.ToUpper().Contains(namePattern.ToUpper()))
                    rzlt.Add(dbo);
            }

            return rzlt;
        }
        /// <summary>
        /// Returns all available databases in selected server
        /// </summary>
        /// <param name="serverName">SQL server's name</param>
        /// <param name="namePattern">Name pattern</param>
        /// <param name="accessibleOnly"><code>true</code> - return accessible databases only</param>
        /// <returns>List of available databases</returns>
        [WebMethod]
        public IEnumerable<DatabaseDbo> EnumerateDatabases(string serverName, string namePattern, bool accessibleOnly)
        {
            try
            {
                Server server = new Server(serverName);
                var rzlt = new List<DatabaseDbo>();
                if (server.Databases == null || server.Databases.Count < 1)
                    return rzlt;
                DatabaseDbo dbo;
                string dbsName;

                foreach (Database dbs in server.Databases)
                {
                    dbsName = dbs.Name;
                    if ((!String.IsNullOrEmpty(dbsName) || String.IsNullOrEmpty(namePattern) || dbsName.ToUpper().Contains(namePattern.ToUpper())) &&
                        (!accessibleOnly || dbs.IsAccessible))
                    {
                        dbo = new DatabaseDbo()
                        {
                            Name = dbsName,
                            ID = dbs.ID,
                            IsAccessible = dbs.IsAccessible,
                            Size = dbs.Size
                        };
                        rzlt.Add(dbo);
                    }
                }
                return rzlt;
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return new List<DatabaseDbo>(); ;
            }
        }

        /// <summary>
        /// List of tables in selected database
        /// </summary>
        /// <param name="serverName">Server's name</param>
        /// <param name="databaseName">Name of selected database</param>
        /// <returns><see cref="DataTableListDbo"/></returns>
        [WebMethod]
        public DataTableListDbo EnumerateTables(string serverName, string databaseName)
        {
            try
            {
                Server server = new Server(serverName);
                if (server == null)
                    return new DataTableListDbo() { IsSuccess = false, ErrorMessage = "Invalid server's name." };
                Database db = server.Databases[databaseName];
                if (db == null)
                    return new DataTableListDbo() { IsSuccess = false, ErrorMessage = "Invalid name of database." };

                var rzlt = new List<string>();
                foreach (Table currentTable in db.Tables)
                    rzlt.Add(currentTable.Name);

                return new DataTableListDbo() { IsSuccess = true, TableNames = rzlt };
            }
            catch (Exception ex)
            {
                return new DataTableListDbo() { IsSuccess = false, ErrorMessage = ex.Message };
            }
        }

        /// <summary>
        /// Makes DB connection
        /// </summary>
        /// <param name="dataSource">SQL server's name</param>
        /// <param name="initialCatalog">Database name</param>
        /// <param name="encUsername">Encrypted login name</param>
        /// <param name="encPassword">Encrypted password</param>
        /// <returns>List with roles, <see cref="CreateDbConnectionResponse"/>.</returns>
        [WebMethod(EnableSession = true)]
        public CreateDbConnectionResponse CreateDbConnection(string dataSource, string initialCatalog, string encUsername, string encPassword)
        {
            var credentials = new UsrNamePassword(encUsername, encPassword);
            bool loginIsOk;

            string message = BuildSqlConnection(dataSource, initialCatalog, credentials.UserName, credentials.Password, out loginIsOk);
            if (!loginIsOk)
                return new CreateDbConnectionResponse() { Connected = false, ErrorMessage = message };
            else
            {
                var rzlt = new CreateDbConnectionResponse() { Connected = true };
                try
                {
                    rzlt.Roles = GetUsersRoles(credentials.UserName);
                    return rzlt;
                }
                catch (Exception ex)
                {
                    return new CreateDbConnectionResponse() { Connected = false, ErrorMessage = ex.Message };
                }
            }
        }

        /// <summary>
        /// The function makes backup file of the database.
        /// There is no need for database locking because SQL 2008 performs all tasks itself.
        /// <see cref="http://home.clara.net/drdsl/MSSQL/backups.html"/>
        /// <seealso cref="http://msdn.microsoft.com/en-us/library/aa225964(v=SQL.80).aspx"/>
        /// <see cref="http://msdn.microsoft.com/en-us/library/microsoft.sqlserver.management.smo.backup.aspx"/>
        /// </summary>
        /// <param name="directory">Directory where backup file would be stored</param>
        /// <param name="file">File name of the backuo file</param>
        /// <param name="overWriteMode"><code>true</code> - delete existing file</param>
        /// <param name="singleUserMode"><code>true</code> - put database to single user mode</param>
        /// <returns><see cref="BackupStatus"/></returns>
        [WebMethod(EnableSession = true)]
        public BackupStatus BackupDatabase(string directory, string file, bool overWriteMode, bool singleUserMode)
        {
            bool isSingleMode = false;
            try
            {
                if (!directory.EndsWith(@"\"))
                    directory += @"\";
                string fileName = Path.Combine(directory, file);

                if (overWriteMode)
                {
                    if (File.Exists(fileName))
                        File.Delete(fileName);
                }
                else
                    RenameFile(fileName);

                long fileSize;
                if (singleUserMode)
                {
                    isSingleMode = SetSingleMode(DatabaseName);
                    fileSize = MakeBackup(fileName);
                }
                else
                    fileSize = MakeBackup(fileName);

                return new BackupStatus() { IsSuccess = true, FileSize = fileSize };
            }
            catch (Exception ex)
            {
                return new BackupStatus() { IsSuccess = false, FileSize = 0L, ErrorMessage = ex.Message };
            }
            finally
            {
                if (isSingleMode)
                    SetMultiUserMode(DatabaseName);
            }
        }

        /// <summary>
        /// Returns list with file names from backup directory
        /// </summary>
        /// <param name="backupDirectory">Name of directory with backuo files</param>
        /// <param name="template">Phrase in backup file name</param>
        /// <returns><see cref="EnumerateBackupFilesResponse"/></returns>
        [WebMethod(EnableSession = true)]
        public EnumerateBackupFilesResponse EnumerateBackupFiles(string backupDirectory, string template)
        {
            if (String.IsNullOrEmpty(backupDirectory))
                return new EnumerateBackupFilesResponse() { IsSuccess = false, ErrorMessage = "Backup directory is not defined." };
            if (!backupDirectory.EndsWith(@"\"))
                backupDirectory += @"\";
            try
            {
                IList<string> fileList = GetBackupFilenames(backupDirectory, template);
                var rzlt = new EnumerateBackupFilesResponse() { IsSuccess = true };
                rzlt.NameList = fileList;
                return rzlt;
            }
            catch (Exception ex)
            {
                return new EnumerateBackupFilesResponse() { IsSuccess = false, ErrorMessage = ex.Message };
            }
        }

        /// <summary>
        /// Restores database from file.
        /// <see cref="http://msdn.microsoft.com/en-us/library/aa238405(v=sql.80).aspx"/>
        /// <seealso cref="http://msdn.microsoft.com/en-us/library/microsoft.sqlserver.management.smo.restore.aspx"/>
        /// </summary>
        /// <param name="dbName">Dabase name</param>
        /// <param name="directory">Directory with with backup files</param>
        /// <param name="file">Restore database from this file</param>
        /// <param name="withReplace"><code>true</code> - replace existing database</param>
        /// <param name="singleUserMode"><code>true</code> - put database to single user mode</param>
        /// <returns><see cref="RestoreStatus"/></returns>
        [WebMethod(EnableSession = true)]
        public RestoreStatus RestoreDatabase(string dbName, string directory, string file, bool withReplace, bool singleUserMode)
        {
            try
            {
                if (!directory.EndsWith(@"\"))
                    directory += @"\";
                string fileName = Path.Combine(directory, file);

                if (singleUserMode)
                    SetSingleMode(dbName);

                Restore(fileName, dbName, withReplace);
                SetMultiUserMode(dbName);

                return new RestoreStatus() { IsSuccess = true };
            }
            catch (Exception ex)
            {
                return new RestoreStatus() { IsSuccess = false, ErrorMessage = ex.Message };
            }
        }

        /// <summary>
        /// <see cref="http://msdn.microsoft.com/en-us/library/microsoft.sqlserver.management.smo.table.rename.aspx"/>
        /// <seealso cref="http://www.youdidwhatwithtsql.com/altering-database-objects-with-powershell/119"/>
        /// </summary>
        /// <param name="oldName">Old table name</param>
        /// <param name="newName">New table name</param>
        /// <param name="singleUserMode"><code>true</code> - set single user mode</param>
        /// <returns><see cref="RenameTableStatus"/></returns>
        [WebMethod(EnableSession = true)]
        public RenameTableStatus RenameTable(string oldName, string newName, bool singleUserMode)
        {
            try
            {
                if (singleUserMode)
                    SetSingleMode(DatabaseName);
                List<AlteredDependencyDbo> dependecies = RenameTheTable(oldName, newName);
                if (singleUserMode)
                    SetMultiUserMode(DatabaseName);
                return new RenameTableStatus() { IsSuccess = true, AlteredDependencies = dependecies };
            }
            catch (Exception ex)
            {
                return new RenameTableStatus() { IsSuccess = false, ErrorMessage = ex.Message };
            }
        }

        /// <summary>
        /// Returns description of all columns in the table
        /// </summary>
        /// <param name="serverName">Server name</param>
        /// <param name="dbName">database name</param>
        /// <param name="tableName">table name</param>
        /// <returns><see cref="EnumerateColumnsResponse"/></returns>
        [WebMethod(EnableSession = true)]
        public EnumerateColumnsResponse EnumerateColumns(string serverName, string dbName, string tableName)
        {
            try
            {
                var rzlt = new EnumerateColumnsResponse();
                rzlt.Columns.AddRange(GetTableColumns(tableName));
                rzlt.IsSuccess = true;
                return rzlt;
            }
            catch (Exception ex)
            {
                return new EnumerateColumnsResponse() { IsSuccess = false, ErrorMessage = ex.Message };
            }
        }

        /// <summary>
        /// Inserts new column into the table defined in the parameter
        /// <see cref="http://www.mssqltips.com/sqlservertip/1826/getting-started-with-sql-server-management-objects-smo/"/>
        /// </summary>
        /// <param name="columnRequest"><see cref="UpdateColumnRequest"/></param>
        /// <returns>Definition of the new column</returns>
        [WebMethod(EnableSession = true)]
        public InsertColumnResponse InsertColumn(UpdateColumnRequest columnRequest)
        {
            try
            {
                DataColumnDbo column = InsertColumn(columnRequest.Table, columnRequest.Column);
                return new InsertColumnResponse() { IsSuccess = true, Column = column };
            }
            catch (Exception ex)
            {
                var msg = new StringBuilder(ex.Message);
                for (Exception inner = ex.InnerException; inner != null; inner = inner.InnerException)
                {
                    msg.Append("\n");
                    msg.Append(inner.Message);
                }
                return new InsertColumnResponse() { IsSuccess = false, ErrorMessage = msg.ToString() };
            }
        }
    }
}
