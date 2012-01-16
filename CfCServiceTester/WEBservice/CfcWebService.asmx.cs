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
using System.Transactions;
using System.Web.Configuration;

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
        /// Transaction timeout in minutes. 
        /// </summary>
        private static readonly int TransactionTimeout = 10;


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
        [WebMethod(EnableSession = true)]
        public IEnumerable<SqlServerDbo> EnumerateSqlServers(bool localOnly, string namePattern)
        {
            var options = new TransactionOptions()
            {
                IsolationLevel = System.Transactions.IsolationLevel.Serializable,
                Timeout = new TimeSpan(0, TransactionTimeout, 0)
            };
            using (var trScope = new TransactionScope(TransactionScopeOption.Required, options))
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

                trScope.Complete();
                return rzlt;
            }
        }
        /// <summary>
        /// Returns all available databases in selected server
        /// </summary>
        /// <param name="serverName">SQL server's name</param>
        /// <param name="namePattern">Name pattern</param>
        /// <param name="accessibleOnly"><code>true</code> - return accessible databases only</param>
        /// <returns>List of available databases</returns>
        [WebMethod(EnableSession = true)]
        public IEnumerable<DatabaseDbo> EnumerateDatabases(string serverName, string namePattern, bool accessibleOnly)
        {
            try
            {
                var options = new TransactionOptions()
                {
                    IsolationLevel = System.Transactions.IsolationLevel.Serializable,
                    Timeout = new TimeSpan(0, TransactionTimeout, 0)
                };
                using (var trScope = new TransactionScope(TransactionScopeOption.Required, options))
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

                    trScope.Complete();
                    return rzlt;
                }
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
        /// <returns><see cref="DataTableListDbo"/></returns>
        [WebMethod(EnableSession = true)]
        public DataTableListDbo EnumerateTables()
        {
            try
            {
                var options = new TransactionOptions()
                {
                    IsolationLevel = System.Transactions.IsolationLevel.Serializable,
                    Timeout = new TimeSpan(0, TransactionTimeout, 0)
                };
                using (var trScope = new TransactionScope(TransactionScopeOption.Required, options))
                {
                    var rzlt = GetAllTables();
                    trScope.Complete();
                    return new DataTableListDbo() { IsSuccess = true, TableNames = rzlt };
                }
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
                var rzlt = new CreateDbConnectionResponse() {
                    Connected = true,
                    CurrentServer = dataSource,
                    CurrentDatabase = initialCatalog,
                };
                try
                {
                    var options = new TransactionOptions()
                    {
                        IsolationLevel = System.Transactions.IsolationLevel.Serializable,
                        Timeout = new TimeSpan(0, TransactionTimeout, 0)
                    };
                    using (var trScope = new TransactionScope(TransactionScopeOption.Required, options))
                    {
                        rzlt.Roles = GetUsersRoles(credentials.UserName);
                        trScope.Complete();
                        return rzlt;
                    }
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
                if (!Directory.Exists(directory))
                {
                    string error = String.Format("Backup directory '{0}' was not found.", directory);
                    return new BackupStatus() { IsSuccess = false, FileSize = 0L, ErrorMessage = error };
                }

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
            if (!Directory.Exists(backupDirectory))
            {
                string error = String.Format("Backup directory '{0}' was not found.", backupDirectory);
                return new EnumerateBackupFilesResponse() { IsSuccess = false, ErrorMessage = error };
            }
            try
            {
                var options = new TransactionOptions() 
                { 
                    IsolationLevel = System.Transactions.IsolationLevel.Serializable,
                    Timeout = new TimeSpan(0, TransactionTimeout, 0)
                };
                using (var trScope = new TransactionScope(TransactionScopeOption.Required, options))
                {
                    IList<string> fileList = GetBackupFilenames(backupDirectory, template);
                    var rzlt = new EnumerateBackupFilesResponse() { IsSuccess = true };
                    rzlt.NameList = fileList;

                    trScope.Complete();
                    return rzlt;
                }
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
        public RestoreStatus RestoreDatabase(string dbName, string directory, string file, bool withReplace, 
                                             bool singleUserMode, bool switchDatabase)
        {
            bool isSingleMode = false;
            try
            {
                if (!directory.EndsWith(@"\"))
                    directory += @"\";
                if (!Directory.Exists(directory))
                {
                    string error = String.Format("Backup directory '{0}' was not found.", directory);
                    return new EnumerateBackupFilesResponse() { IsSuccess = false, ErrorMessage = error };
                }
                string fileName = Path.Combine(directory, file);

                if (singleUserMode)
                    isSingleMode = SetSingleMode(dbName);

                Restore(fileName, dbName, withReplace);
                if (switchDatabase)
                    RenameDatabase(dbName);

                return new RestoreStatus() { IsSuccess = true };
            }
            catch (Exception ex)
            {
                return new RestoreStatus() { IsSuccess = false, ErrorMessage = ex.Message };
            }
            finally
            {
                SetMultiUserMode(dbName);
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
                var options = new TransactionOptions() 
                { 
                    IsolationLevel = System.Transactions.IsolationLevel.Serializable,
                    Timeout = new TimeSpan(0, TransactionTimeout, 0)
                };
                using (var trScope = new TransactionScope(TransactionScopeOption.Required, options))
                {
                    if (singleUserMode)
                        SetSingleMode(DatabaseName);
                    List<AlteredDependencyDbo> dependecies = RenameTheTable(oldName, newName);

                    trScope.Complete();
                    return new RenameTableStatus() { IsSuccess = true, AlteredDependencies = dependecies };
                }
            }
            catch (Exception ex)
            {
                return new RenameTableStatus() { IsSuccess = false, ErrorMessage = ex.Message };
            }
            finally
            {
                if (singleUserMode)
                    SetMultiUserMode(DatabaseName);
            }
        }

        /// <summary>
        /// Returns description of all columns in the table
        /// </summary>
        /// <param name="tableName">table name</param>
        /// <returns><see cref="EnumerateColumnsResponse"/></returns>
        [WebMethod(EnableSession = true)]
        public EnumerateColumnsResponse EnumerateColumns(string tableName)
        {
            try
            {
                var options = new TransactionOptions() 
                { 
                    IsolationLevel = System.Transactions.IsolationLevel.Serializable,
                    Timeout = new TimeSpan(0, TransactionTimeout, 0)
                };
                using (var trScope = new TransactionScope(TransactionScopeOption.Required, options))
                {
                    var rzlt = new EnumerateColumnsResponse();
                    rzlt.Columns.AddRange(GetTableColumns(tableName));
                    rzlt.IsSuccess = true;

                    trScope.Complete();
                    return rzlt;
                }
            }
            catch (Exception ex)
            {
                return new EnumerateColumnsResponse() { IsSuccess = false, ErrorMessage = ex.Message };
            }
        }

        /// <summary>
        /// Creates new table and returns  empty list or description of all columns in the table when the table exists.
        /// </summary>
        /// <param name="tableName">table name</param>
        /// <returns><see cref="EnumerateColumnsResponse"/></returns>
        [WebMethod(EnableSession = true)]
        public EnumerateColumnsResponse CreateTable(string tableName)
        {
            try
            {
                var options = new TransactionOptions()
                {
                    IsolationLevel = System.Transactions.IsolationLevel.Serializable,
                    Timeout = new TimeSpan(0, TransactionTimeout, 0)
                };
                using (var trScope = new TransactionScope(TransactionScopeOption.Required, options))
                {
                    var rzlt = new EnumerateColumnsResponse();
                    rzlt.Columns.AddRange(GetTableColumns(tableName, true));
                    rzlt.IsSuccess = true;

                    trScope.Complete();
                    return rzlt;
                }
            }
            catch (Exception ex)
            {
                return new EnumerateColumnsResponse() { IsSuccess = false, ErrorMessage = ex.Message };
            }
        }

        /// <summary>
        /// Removes table from the database
        /// </summary>
        /// <param name="tableName">table name</param>
        /// <param name="disableDependencies"><code>true</code> - remove foreign keys that references the table</param>
        /// <returns><see cref="DeleteTableResponse"/></returns>
        [WebMethod(EnableSession = true)]
        public DeleteTableResponse DeleteTable(string tableName, bool disableDependencies)
        {
            try
            {
                var options = new TransactionOptions()
                {
                    IsolationLevel = System.Transactions.IsolationLevel.Serializable,
                    Timeout = new TimeSpan(0, TransactionTimeout, 0)
                };
                using (var trScope = new TransactionScope(TransactionScopeOption.Required, options))
                {
                    List<DroppedDependencyDbo> droppedKeys = DeleteTheTable(tableName, disableDependencies);
                    var rzlt = new DeleteTableResponse() 
                    { 
                        DroppedDependencies = droppedKeys ,
                        ErrorMessage = String.Format("Table '{0}' is deleted.", tableName),
                        IsSuccess = true
                    };

                    trScope.Complete();
                    return rzlt;
                }
            }
            catch (Exception ex)
            {
                return new DeleteTableResponse() { IsSuccess = false, ErrorMessage = ex.Message };
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
            List<DroppedDependencyDbo> droppedForeignKeys;
            try
            {
                var options = new TransactionOptions() 
                { 
                    IsolationLevel = System.Transactions.IsolationLevel.Serializable,
                    Timeout = new TimeSpan(0, TransactionTimeout, 0)
                };
                using (var trScope = new TransactionScope(TransactionScopeOption.Required, options))
                {
                    DataColumnDbo column = InsertColumn(columnRequest.Table, columnRequest.Column, columnRequest.SingleUserMode,
                                                        columnRequest.DisableDependencies, out droppedForeignKeys);

                    trScope.Complete();
                    return new InsertColumnResponse() { IsSuccess = true, Column = column, DroppedForeignKeys = droppedForeignKeys };
                }
            }
            catch (Exception ex)
            {
                return new InsertColumnResponse() { IsSuccess = false, ErrorMessage = ParseErrorMessage(ex) };
            }
        }

        /// <summary>
        /// Renames column in the table
        /// </summary>
        /// <param name="columnRequest"><see cref="UpdateColumnRequest"/></param>
        /// <returns><see cref="RenameColumnResponse"/></returns>
        [WebMethod(EnableSession = true)]
        public RenameColumnResponse RenameColumn(UpdateColumnRequest columnRequest)
        {
            List<AlteredDependencyDbo> alteredDependencies;
            try
            {
                var options = new TransactionOptions() 
                { 
                    IsolationLevel = System.Transactions.IsolationLevel.Serializable,
                    Timeout = new TimeSpan(0, TransactionTimeout, 0)
                };
                using (var trScope = new TransactionScope(TransactionScopeOption.Required, options))
                {
                    DataColumnDbo column = RenameColumn(columnRequest.Table, columnRequest.OldColumnName, columnRequest.Column.Name,
                                                        columnRequest.SingleUserMode, out alteredDependencies);
                    var rzlt = new RenameColumnResponse() { IsSuccess = true, Column = column };
                    rzlt.AlteredDependencies.AddRange(alteredDependencies);

                    trScope.Complete();
                    return rzlt;
                }
            }
            catch (Exception ex)
            {
                return new RenameColumnResponse() { IsSuccess = false, ErrorMessage = ParseErrorMessage(ex) };
            }

        }

        /// <summary>
        /// Removes column from the table
        /// </summary>
        /// <param name="columnRequest">Request for deleting the column</param>
        /// <returns><see cref="DeleteColumnResponse"/></returns>
        [WebMethod(EnableSession = true)]
        public DeleteColumnResponse DeleteColumn(UpdateColumnRequest columnRequest)
        {
            List<DroppedDependencyDbo> droppedDependencies;
            try
            {
                var options = new TransactionOptions() 
                { 
                    IsolationLevel = System.Transactions.IsolationLevel.Serializable,
                    Timeout = new TimeSpan(0, TransactionTimeout, 0)
                };
                using (var trScope = new TransactionScope(TransactionScopeOption.Required, options))
                {
                    DeleteColumn(columnRequest.Table, columnRequest.Column.Name, columnRequest.DisableDependencies,
                                                        columnRequest.SingleUserMode, out droppedDependencies);
                    var rzlt = new DeleteColumnResponse() { IsSuccess = true, DroppedDependencies = droppedDependencies };

                    trScope.Complete();
                    return rzlt;
                }
            }
            catch (Exception ex)
            {
                return new DeleteColumnResponse() { IsSuccess = false, ErrorMessage = ParseErrorMessage(ex) };
            }
        }

        /// <summary>
        /// Removes column from the table
        /// </summary>
        /// <param name="columnRequest">Request for deleting the column</param>
        /// <returns><see cref="InsertColumnResponse"/></returns>
        [WebMethod(EnableSession = true)]
        public InsertColumnResponse UpdateColumn(UpdateColumnRequest columnRequest)
        {
            try
            {
                var options = new TransactionOptions() 
                { 
                    IsolationLevel = System.Transactions.IsolationLevel.Serializable,
                    Timeout = new TimeSpan(0, TransactionTimeout, 0)
                };
                using (var trScope = new TransactionScope(TransactionScopeOption.Required, options))
                {
                    List<DroppedDependencyDbo> droppedDependencies;
                    DataColumnDbo dbo = UpdateColumn(columnRequest.Table, columnRequest.Column, columnRequest.DisableDependencies,
                                                        columnRequest.SingleUserMode, out droppedDependencies);
                    var rzlt = new InsertColumnResponse()
                    {
                        IsSuccess = true,
                        Column = dbo,
                        DroppedForeignKeys = droppedDependencies
                    };
                    
                    trScope.Complete();
                    return rzlt;
                }
            }
            catch (Exception ex)
            {
                return new InsertColumnResponse() { IsSuccess = false, ErrorMessage = ParseErrorMessage(ex) };
            }
        }

        /// <summary>
        /// Returns index description
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="indexName">Index name</param>
        /// <returns><see cref="GetIndexResponse"/></returns>
        [WebMethod(EnableSession = true)]
        public GetIndexResponse GetIndex(string tableName, string indexName)
        {
            try
            {
                var options = new TransactionOptions()
                {
                    IsolationLevel = System.Transactions.IsolationLevel.Serializable,
                    Timeout = new TimeSpan(0, TransactionTimeout, 0)
                };
                using (var trScope = new TransactionScope(TransactionScopeOption.Required, options))
                {
                    IndexDbo dbo = GetIndexDescription(tableName, indexName);
                    var rzlt = new GetIndexResponse() { IsSuccess = true, Dbo = dbo };

                    trScope.Complete();
                    return rzlt;
                }
            }
            catch (Exception ex)
            {
                return new GetIndexResponse() { IsSuccess = false, ErrorMessage = ParseErrorMessage(ex) };
            }
        }

        /// <summary>
        /// Returns descriptions for indexes that belongs to selected table
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <returns><see cref="EnumerateIndexesResponse"/></returns>
        [WebMethod(EnableSession = true)]
        public EnumerateIndexesResponse EnumerateIndexes(string tableName)
        {
            try
            {
                var options = new TransactionOptions()
                {
                    IsolationLevel = System.Transactions.IsolationLevel.Serializable,
                    Timeout = new TimeSpan(0, TransactionTimeout, 0)
                };
                using (var trScope = new TransactionScope(TransactionScopeOption.Required, options))
                {
                    List<IndexDbo> indexes = GetTableIndexes(tableName);
                    var rzlt = new EnumerateIndexesResponse() { IsSuccess = true, Indexes = indexes };

                    trScope.Complete();
                    return rzlt;
                }
            }
            catch (Exception ex)
            {
                return new EnumerateIndexesResponse() { IsSuccess = false, ErrorMessage = ParseErrorMessage(ex) };
            }
        }

        /// <summary>
        /// Performs CRUD operations on indexes
        /// </summary>
        /// <param name="request">Request for updating the index, <see cref="UpdateIndexRequest"/></param>
        /// <param name="singleUserMode"><code>true</code> - set single user mode</param>
        /// <returns><see cref="UpdateIndexResponse"/></returns>
        [WebMethod(EnableSession = true)]
        public UpdateIndexResponse UpdateIndex(UpdateIndexRequest request, bool singleUserMode)
        {
            try
            {
                var options = new TransactionOptions()
                {
                    IsolationLevel = System.Transactions.IsolationLevel.Serializable,
                    Timeout = new TimeSpan(0, TransactionTimeout, 0)
                };
                using (var trScope = new TransactionScope(TransactionScopeOption.Required, options))
                {
                    if (singleUserMode)
                        SetSingleMode(DatabaseName);

                    var dependecies = new List<DroppedDependencyDbo>();
                    IndexDbo dbo = null;

                    switch (request.OperationType)
                    {
                        case UpdateColumnOperation.Rename:
                            dbo = RenameTheIndex(request.TableName, request.OldIndexName, request.IndexName);
                            break;
                    }

                    trScope.Complete();
                    return new UpdateIndexResponse() { IsSuccess = true, DroppedDependencies = dependecies, Dbo = dbo };
                }
            }
            catch (Exception ex)
            {
                return new UpdateIndexResponse() { IsSuccess = false, ErrorMessage = ex.Message };
            }
            finally
            {
                if (singleUserMode)
                    SetMultiUserMode(DatabaseName);
            }
        }
    }
}
