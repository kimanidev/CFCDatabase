﻿using System;
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
        /// Makes DB connection
        /// </summary>
        /// <param name="serverName">SQL server's name</param>
        /// <param name="namePattern">Name pattern</param>
        /// <param name="accessibleOnly"><code>true</code> - return accessible databases only</param>
        /// <returns>List of available databases</returns>
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
        /// <returns></returns>
        [WebMethod(EnableSession = true)]
        public BackupStatus BackupDatabase(string directory, string file, bool overWriteMode)
        {
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
                long fileSize = MakeBackup(fileName);
                return new BackupStatus() { IsSuccess = true, FileSize = fileSize };
            }
            catch (Exception ex)
            {
                return new BackupStatus() { IsSuccess = false, FileSize = 0L, ErrorMessage = ex.Message };
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
        /// <returns><see cref="RestoreStatus"/></returns>
        [WebMethod(EnableSession = true)]
        public RestoreStatus RestoreDatabase(string dbName, string directory, string file, bool withReplace)
        {
            try
            {
                if (!directory.EndsWith(@"\"))
                    directory += @"\";
                string fileName = Path.Combine(directory, file);

                Restore(fileName, dbName, withReplace);
                return new RestoreStatus() { IsSuccess = true };
            }
            catch (Exception ex)
            {
                return new RestoreStatus() { IsSuccess = false, ErrorMessage = ex.Message };
            }
        }
    }
}