using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using Microsoft.SqlServer.Management.Smo;
using CfCServiceTester.WEBservice.DataObjects;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading;

namespace CfCServiceTester.WEBservice
{
    /// <summary>
    /// Listed below methods are designed for service only.
    /// </summary>
    public partial class CfcWebService
    {
        /// <summary>
        /// Wait handle for finishing backup operation
        /// </summary>
        private static ManualResetEvent mre = new ManualResetEvent(false);

        /// <summary>
        /// The application stores connection string in <code>Session[ConnectionStringKey]</code> and verifies credentials.
        /// </summary>
        public static readonly string ConnectionStringKey = "{61F8CB8C-9AE6-429D-8900-0BCE6AB4C956}";
        /// <summary>
        /// List of roles for the connected user.
        /// </summary>
        public static readonly string RoleListKey = "{12FB9A22-7C13-4338-881E-62D24367B6E5}";
        /// <summary>
        /// SQL server's name
        /// </summary>
        public static readonly string SqlServerNameKey = "{0B568310-9686-4CF9-B38C-99ED7FE48D87}";
        /// <summary>
        /// Database name
        /// </summary>
        public static readonly string DatabaseNameKey = "{93CA56A2-3CE6-456D-86E6-0C9849CF363E}";
        /// <summary>
        /// Connected user's name
        /// </summary>
        public static string UserNameKey = "{9635AA12-EEA1-486D-A510-3E536BD0617E}";
        /// <summary>
        /// Current password
        /// </summary>
        public static string PasswordKey = "{E06CA12A-F31C-4193-A0C3-62047750CD17}";

        internal static string ConnectionString
        {
            get { return (string)HttpContext.Current.Session[ConnectionStringKey]; }
            set { HttpContext.Current.Session[ConnectionStringKey] = value; }
        }
        public static bool IsConnected
        {
            get { return !String.IsNullOrEmpty(ConnectionString); }
        }

        internal static List<string> RoleList
        {
            get { return (List<string>)HttpContext.Current.Session[RoleListKey]; }
        }
        public static string SqlServerName
        {
            get { return (string)HttpContext.Current.Session[SqlServerNameKey]; }
            set { HttpContext.Current.Session[SqlServerNameKey] = value; }
        }
        public static string DatabaseName
        {
            get { return (string)HttpContext.Current.Session[DatabaseNameKey]; }
            set { HttpContext.Current.Session[DatabaseNameKey] = value; }
        }
        public static string UserName
        {
            get { return (string)HttpContext.Current.Session[UserNameKey]; }
            set { HttpContext.Current.Session[UserNameKey] = value; }
        }
        public static string Password
        {
            get { return (string)HttpContext.Current.Session[PasswordKey]; }
            set { HttpContext.Current.Session[PasswordKey] = value; }
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
                ConnectionString = csb.ConnectionString;
                SqlServerName = dataSource;
                DatabaseName = initialCatalog;
                UserName = userName;
                Password = password;

                isValid = true;
                sqlConnection.Close();
                return csb.ConnectionString;
            }
            catch (Exception ex)
            {
                isValid = false;
                return ex.Message.StartsWith("Login failed for user") ? "Incorrect username or password entered." : ex.Message;
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

        /// <summary>
        /// Writes SQL backup into file defined in the parameter. Procedure is using SMO data objects.
        /// <see cref="http://msdn.microsoft.com/en-us/library/microsoft.sqlserver.management.smo.backup.aspx"/>
        /// <see cref="http://www.codeproject.com/Articles/127065/SMO-Tutorial-1-of-n-Programming-data-storage-objec#DBBackup"/>
        /// First call to SQL server Express is crashing after backup . Production and developer's editions are working.
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <returns>File size</returns>
        public static long MakeBackup(string fileName)
        {
            var server = GetConnectedServer(SqlServerName, UserName, Password);
            var backup = new Backup()
                {
                    Database = DatabaseName,
                    Action = BackupActionType.Database,
                    Initialize = true,
                    ContinueAfterError = true,
                    PercentCompleteNotification = 20,
                };

            backup.Devices.Clear();
            backup.Devices.AddDevice(fileName, DeviceType.File);
            backup.PercentComplete += new PercentCompleteEventHandler(ProgressEventHandler);

            mre.Reset();
            backup.SqlBackup(server);
            mre.WaitOne(new TimeSpan(0, 1, 0));

            try
            {
                var fInfo = new FileInfo(fileName);
                return fInfo.Length;
            }
            catch (Exception)   // There is no need for processing error here
            {
                return 0L;
            }
        }
        static void ProgressEventHandler(object sender, PercentCompleteEventArgs e)
        {
            if (e.Percent > 90)
                mre.Set();
        }
        /// <summary>
        /// /// Restores database from file.
        /// <see cref="http://msdn.microsoft.com/en-us/library/microsoft.sqlserver.management.smo.restore.sqlrestore.aspx"/>
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="databaseName">Database name</param>
        /// <param name="withReplace"><code>true</code> - with replace</param>
        public static void Restore(string fileName, string databaseName, bool withReplace)
        {

            string sourceLogFileName = databaseName + "_log";
            var server = GetConnectedServer(SqlServerName, UserName, Password);
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
            System.Data.SqlClient.SqlConnection.ClearAllPools();
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

        /// <summary>
        /// Returns list with names of backup files.
        /// </summary>
        /// <param name="backupDirectory">Directory where backup files are stored</param>
        /// <param name="template">Phrase in the name of backup file</param>
        /// <returns>List with file names</returns>
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

        /// <summary>
        /// Returns list with name of hosts, connected to actual server
        /// </summary>
        /// <returns>List with host names.</returns>
        private static List<string> GetConnectedHosts()
        {
            const string queryString =
                "SELECT  DISTINCT hostname " +
                "FROM [master].[dbo].sysprocesses (nolock) " +
                "WHERE status <> 'background' AND hostname IS NOT NULL AND hostname <> ' ';";

            // Get names of connected hosts. 
            using (var connection = new SqlConnection(ConnectionString))
            {
                var da = new SqlDataAdapter(queryString, connection);
                da.TableMappings.Add("Table", "HostNames");

                var ds = new DataSet();
                da.Fill(ds);
                DataTable hosts = ds.Tables["HostNames"];
                List<string> rzlt = (
                    from host in hosts.AsEnumerable()
                    select host.Field<string>("hostname")
                    ).ToList();
                return rzlt;
            }
        }

        /// <summary>
        /// <see cref="http://www.eggheadcafe.com/community/csharp/2/10085799/how-to-send-message-via-lan.aspx"/>
        /// <seealso cref="http://www.dotnetspider.com/resources/35565-Send-Message-all-hosts-connected-LAN.aspx"/>
        /// <seealso cref="http://bytes.com/topic/c-sharp/answers/262560-net-send-using-c"/>
        /// </summary>
        /// <param name="message"></param>
        private static void SendNotification(string message)
        {
            foreach (string host in DisconnectedHosts)
            {
                var objProcess = new Process();
                objProcess.StartInfo.FileName = "cmd.exe";
                objProcess.StartInfo.Arguments = String.Format("/C NET SEND {0} {1}", host, message); 
                objProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                objProcess.StartInfo.CreateNoWindow = true;
                objProcess.Start();
                objProcess.WaitForExit();
            }
        }

        /// <summary>
        /// Renames all references to old table in body of stored procedure
        /// <see cref="http://www.youdidwhatwithtsql.com/altering-database-objects-with-powershell/119"/>
        /// </summary>
        /// <param name="oldName">Old table name</param>
        /// <param name="newName">New table name</param>
        /// <param name="db">Active database</param>
        private static List<AlteredDependencyDbo> CorrectStoredProcedure(Database db, string oldName, string newName)
        {
            string regexTemplate = String.Format(@"\b{0}\b", oldName);        
            var rg = new Regex(regexTemplate, RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);
//            string body;
            var rzlt = new List<AlteredDependencyDbo>();

            CorrectStoredProcedures(db, rg, newName, rzlt);
            CorrectViews(db, rg, newName, rzlt);
            CorrectUserDefinedFunctions(db, rg, newName, rzlt);
            CorrectTriggers(db, rg, newName, rzlt);

            //foreach (Trigger trg in db.Triggers)
            //{
            //    bool isForbidden = trg.ImplementationType == ImplementationType.SqlClr || trg.IsEncrypted || trg.IsSystemObject;
            //    if (!isForbidden)
            //    {
            //        body = trg.TextBody;
            //        if (rg.IsMatch(body))
            //        {
            //            trg.TextBody = rg.Replace(body, newName);
            //            trg.Alter();
            //            rzlt.Add(new AlteredDependencyDbo() { ObjectType = DbObjectType.Trigger, Name = trg.Name });
            //        }
            //    }
            //}

            return rzlt;
        }

        private static void CorrectTriggers(Database db, Regex rg, string newName, List<AlteredDependencyDbo> rzlt)
        {
            string body;
            foreach (Trigger trg in db.Triggers)
            {
                bool isForbidden = trg.ImplementationType == ImplementationType.SqlClr || trg.IsEncrypted || trg.IsSystemObject;
                if (!isForbidden)
                {
                    body = trg.TextBody;
                    if (rg.IsMatch(body))
                    {
                        trg.TextBody = rg.Replace(body, newName);
                        trg.Alter();
                        rzlt.Add(new AlteredDependencyDbo() { ObjectType = DbObjectType.Trigger, Name = trg.Name });
                    }
                }
            }
        }

        private static List<AlteredDependencyDbo> CorrectFieldNames(Database db, string tblName, string oldName, string newName)
        {
            string regexTemplate = String.Format(@"\b({0}|\[{0}\])\.({1}|\[{1}\])\b", tblName, oldName);
            var rg = new Regex(regexTemplate, RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);
            var rzlt = new List<AlteredDependencyDbo>();
            string newFieldName = String.Format(@"[{0}].[{1}]", tblName, newName);

            CorrectStoredProcedures(db, rg, newFieldName, rzlt);
            CorrectViews(db, rg, newFieldName, rzlt);
            CorrectUserDefinedFunctions(db, rg, newFieldName, rzlt);
            CorrectTriggers(db, rg, newFieldName, rzlt);

            return rzlt;
        }

        /// <summary>
        /// Procedure processes normal procedures only excluding encrypted, system and CLR procedures
        /// </summary>
        /// <param name="db">database</param>
        /// <param name="rg">Regular expression for replacing text</param>
        /// <param name="newName">New name of renamed table (old one is stored in regular expression</param>
        /// <param name="rzlt">List with names modified objects.</param>
        private static void CorrectStoredProcedures(Database db, Regex rg, string newName, List<AlteredDependencyDbo> rzlt)
        {
            DataTable dt = db.EnumObjects(DatabaseObjectTypes.StoredProcedure);
            var procList = 
                from sp in dt.AsEnumerable()
                where String.Compare(sp.Field<string>("Schema"), "sys", true) != 0
                select sp.Field<string>("Name");

            foreach (string name in procList)
            {
                StoredProcedure sp = db.StoredProcedures[name];
                if (sp == null)
                    continue;

                if (!(sp.ImplementationType == ImplementationType.SqlClr || sp.IsEncrypted || sp.IsSystemObject))
                {
                    string body = sp.TextBody;
                    if (String.IsNullOrEmpty(body))
                        continue;
                    if (rg.IsMatch(body))
                    {
                        sp.TextBody = rg.Replace(body, newName);
                        sp.Alter();
                        rzlt.Add(new AlteredDependencyDbo() { ObjectType = DbObjectType.StoredProcedure, Name = sp.Name });
                    }
                }
            }
        }

        /// <summary>
        /// Procedure processes normal views only excluding encrypted, system views.
        /// </summary>
        /// <param name="db">database</param>
        /// <param name="rg">Regular expression for replacing text</param>
        /// <param name="newName">New name of renamed table (old one is stored in regular expression</param>
        /// <param name="rzlt">List with names modified objects.</param>
        private static void CorrectViews(Database db, Regex rg, string newName, List<AlteredDependencyDbo> rzlt)
        {
            DataTable dt = db.EnumObjects(DatabaseObjectTypes.View);
            var viewList =
                from sp in dt.AsEnumerable()
                where String.Compare(sp.Field<string>("Schema"), "sys", true) != 0 &&
                        String.Compare(sp.Field<string>("Schema"), "INFORMATION_SCHEMA", true) != 0
                select sp.Field<string>("Name");

            foreach (string name in viewList)
            {
                View vw = db.Views[name];
                if (vw == null)
                    continue;

                if (!(vw.IsEncrypted || vw.IsSystemObject))
                {
                    string body = vw.TextBody;
                    if (String.IsNullOrEmpty(body))
                        continue;

                    if (rg.IsMatch(body))
                    {
                        vw.TextBody = rg.Replace(body, newName);
                        vw.Alter();
                        rzlt.Add(new AlteredDependencyDbo() { ObjectType = DbObjectType.View, Name = vw.Name });
                    }
                }
            }
        }

        /// <summary>
        /// Procedure processes normal UDF only excluding encrypted, system and CLR UDF
        /// </summary>
        /// <param name="db">database</param>
        /// <param name="rg">Regular expression for replacing text</param>
        /// <param name="newName">New name of renamed table (old one is stored in regular expression</param>
        /// <param name="rzlt">List with names modified objects.</param>
        private static void CorrectUserDefinedFunctions(Database db, Regex rg, string newName, List<AlteredDependencyDbo> rzlt)
        {
            DataTable dt = db.EnumObjects(DatabaseObjectTypes.UserDefinedFunction);
            var funcList =
                from udf in dt.AsEnumerable()
                where String.Compare(udf.Field<string>("Schema"), "sys", true) != 0
                select udf.Field<string>("Name");

            foreach (string name in funcList)
            {
                UserDefinedFunction udf = db.UserDefinedFunctions[name];
                if (udf == null)
                    continue;

                if (!(udf.ImplementationType == ImplementationType.SqlClr || udf.IsEncrypted || udf.IsSystemObject))
                {
                    string body = udf.TextBody;
                    if (String.IsNullOrEmpty(body))
                        continue;

                    if (rg.IsMatch(body))
                    {
                        udf.TextBody = rg.Replace(body, newName);
                        udf.Alter();
                        rzlt.Add(new AlteredDependencyDbo() { ObjectType = DbObjectType.UserDefinedFunction, Name = udf.Name });
                    }
                }
            }
        }

        /// <summary>
        /// Returns list with columns that represents primary key
        /// </summary>
        /// <param name="aTable">Current table, <see cref="Table"/></param>
        /// <returns>List with names of columns in the primary key.</returns>
        private static List<string> GetPrimaryKeyColumns(Table aTable)
        {
            var keyQuery = (
                from Index ind in aTable.Indexes
                where ind.IndexKeyType == IndexKeyType.DriPrimaryKey
                select ind
                ).FirstOrDefault();
            if (keyQuery == null)
                return new List<string>();
            else
            {
                var rzlt = new List<string>();
                foreach (IndexedColumn clmn in keyQuery.IndexedColumns)
                    rzlt.Add(clmn.Name);
                return rzlt;
            }
        }

        /// <summary>
        /// Creates column in the table
        /// </summary>
        /// <param name="aTable">Table, <see cref="Table"/></param>
        /// <param name="column">Column description <see cref="DataColumnDbo"/></param>
        /// <returns>SMO column, <see cref="Column"/></returns>
        private static Column CreateColumn(Table aTable, DataColumnDbo column)
        {
           SqlDataType sqlType = (SqlDataType)Enum.Parse(typeof(SqlDataType), column.SqlDataType, true);
            int length;
            if (column.MaximumLength.HasValue && column.MaximumLength.Value > 0)
                length = column.MaximumLength.Value;
            else
                length = column.NumericPrecision ?? 0;

            var newColumn = new Column(aTable, column.Name, 
                                       GetSmoType(sqlType, length, column.NumericPrecision ?? 0, column.NumericScale ?? 0))
            {
                Identity = column.IsIdentity,
                Nullable = column.IsNullable,
            };
            if (!String.IsNullOrEmpty(column.Default))
            {
                newColumn.AddDefaultConstraint();
                newColumn.DefaultConstraint.Text = column.Default;
            }
            if (newColumn.Identity)
            {
                newColumn.IdentityIncrement = 1;
                newColumn.IdentitySeed = 1;
            }

            aTable.Columns.Add(newColumn);
            return newColumn;
        }

        private static Microsoft.SqlServer.Management.Smo.DataType GetSmoType(SqlDataType sqlType, int length, int precision, int scale)
        {
            switch (sqlType)
            {
                case SqlDataType.BigInt:
                    return DataType.BigInt;
                case SqlDataType.Binary:
                    return new DataType(SqlDataType.Binary, length);
                case SqlDataType.Bit:
                    return DataType.Bit;
                case SqlDataType.Char:
                    return new DataType(SqlDataType.Char, length);
                case SqlDataType.Date:
                    return DataType.Date;
                case SqlDataType.DateTime:
                    return DataType.DateTime;
                case SqlDataType.Decimal:
                    return new DataType(SqlDataType.Decimal, scale, precision);
                case SqlDataType.Float:
                    return DataType.Float;
                case SqlDataType.Geography:
                    return DataType.Geography;
                case SqlDataType.Geometry:
                    return DataType.Geometry;
                case SqlDataType.Image:
                    return DataType.Image;
                case SqlDataType.Int:
                    return DataType.Int;
                case SqlDataType.Money:
                    return DataType.Money;
                case SqlDataType.NChar:
                    return new DataType(SqlDataType.NChar, length);
                case SqlDataType.NText:
                    return DataType.NText;
                case SqlDataType.NVarChar:
                    return new DataType(SqlDataType.NVarChar, length);
                case SqlDataType.NVarCharMax:
                    return DataType.NVarCharMax;
                case SqlDataType.Real:
                    return DataType.Real;
                case SqlDataType.SmallDateTime:
                    return DataType.SmallDateTime;
                case SqlDataType.SmallInt:
                    return DataType.SmallInt;
                case SqlDataType.SmallMoney:
                    return DataType.SmallMoney;
                case SqlDataType.SysName:
                    return DataType.SysName;
                case SqlDataType.Text:
                    return DataType.Text;
                case SqlDataType.Timestamp:
                    return DataType.Timestamp;
                case SqlDataType.TinyInt:
                    return DataType.TinyInt;
                case SqlDataType.UniqueIdentifier:
                    return DataType.UniqueIdentifier;
                case SqlDataType.VarBinary:
                    return new DataType(SqlDataType.VarBinary, length);
                case SqlDataType.VarBinaryMax:
                    return DataType.VarBinaryMax;
                case SqlDataType.VarChar:
                    return new DataType(SqlDataType.VarChar, length);
                case SqlDataType.VarCharMax:
                    return DataType.VarCharMax;
                case SqlDataType.Variant:
                    return DataType.Variant;
                case SqlDataType.Xml:
                    return new DataType(SqlDataType.Xml);
                case SqlDataType.DateTimeOffset:
                    return new DataType(SqlDataType.DateTimeOffset, scale);
                case SqlDataType.DateTime2:
                    return new DataType(SqlDataType.DateTime2, scale);
                default:
                    return DataType.Int;
            }
        }

        private static List<DroppedDependencyDbo> InsertColumnIntoPrimarykey(
                                Table table, Column column, bool disableDependencies, Database db)
        {
            var droppedForeignKeys = new List<DroppedDependencyDbo>();
            var query = (
                from Index ind in table.Indexes
                where ind.IndexKeyType == IndexKeyType.DriPrimaryKey
                select ind).FirstOrDefault();
            if (query == null)
                CreateNewPrimaryKey(table, String.Concat("PK_", table.Name), IndexKeyType.DriPrimaryKey, column.Name);
            else
            {
                if (disableDependencies)
                {
                    DropDependentForeignKeys(query.Name, db, droppedForeignKeys);
                }
                AddColumnToIndex(table, column, query);
            }
            return droppedForeignKeys;
        }

        /// <summary>
        /// Drops foreign keys that points to selected primary key
        /// </summary>
        /// <param name="primaryKeyName">Name of the priamry key</param>
        /// <param name="db">Current database</param>
        /// <param name="droppedForeignKeys">List with dropped foreign keys</param>
        private static void DropDependentForeignKeys(string primaryKeyName, Database db, List<DroppedDependencyDbo> droppedForeignKeys)
        {
            const string queryString =
                "SELECT own.name AS TableName, fk.name AS ForeignKeyName " +
                "FROM sys.foreign_keys fk " +
	                "JOIN sys.key_constraints kc ON fk.referenced_object_id = kc.parent_object_id " +
		                "AND kc.unique_index_id = fk.key_index_id " +
	                "JOIN sys.objects own ON own.object_id = fk.parent_object_id " +
                "WHERE kc.name = @PrimaryKeyName";

            List<KeyValuePair<string, string>> lstForeignKeys;
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                var da = new SqlDataAdapter(queryString, connection);
                da.SelectCommand.Parameters.AddWithValue("@PrimaryKeyName", primaryKeyName);
                da.TableMappings.Add("Table", "ForeignKeyList");

                var ds = new DataSet();
                da.Fill(ds);
                DataTable foreignKeys = ds.Tables["ForeignKeyList"];
                lstForeignKeys = (
                    from fKey in foreignKeys.AsEnumerable()
                    select new KeyValuePair<string, string>(fKey.Field<string>("ForeignKeyName"), fKey.Field<string>("TableName"))
                    ).ToList();
                connection.Close();
            }
            DropCorrentForeignKey(lstForeignKeys, db, droppedForeignKeys);
        }

        public static void AddColumnToIndex(Table table, Column column, Index ind)
        {
            string oldName = ind.Name;
            var idx = new Index(table, oldName)
            {
                BoundingBoxYMax = ind.BoundingBoxYMax,
                BoundingBoxYMin = ind.BoundingBoxYMin,
                BoundingBoxXMax = ind.BoundingBoxXMax,
                BoundingBoxXMin = ind.BoundingBoxXMin,
                CompactLargeObjects = ind.CompactLargeObjects,
                DisallowRowLocks = ind.DisallowRowLocks,
                FileGroup = ind.FileGroup,
                FileStreamFileGroup = ind.FileStreamFileGroup,
                FileStreamPartitionScheme = ind.FileStreamPartitionScheme,
                FillFactor = ind.FillFactor,
                FilterDefinition = ind.FilterDefinition,
                IgnoreDuplicateKeys = ind.IgnoreDuplicateKeys,
                IndexKeyType = ind.IndexKeyType,
                IsClustered = ind.IsClustered,
                IsFullTextKey = ind.IsFullTextKey,
                IsUnique = ind.IsUnique,
                Level1Grid = ind.Level1Grid,
                Level2Grid = ind.Level2Grid,
                Level3Grid = ind.Level3Grid,
                Level4Grid = ind.Level4Grid,
                MaximumDegreeOfParallelism = ind.MaximumDegreeOfParallelism,
                NoAutomaticRecomputation = ind.NoAutomaticRecomputation,
                OnlineIndexOperation = ind.OnlineIndexOperation,
                PadIndex = ind.PadIndex,
                ParentXmlIndex = ind.ParentXmlIndex,
                PartitionScheme = ind.PartitionScheme,
                SecondaryXmlIndexType = ind.SecondaryXmlIndexType,
                SortInTempdb = ind.SortInTempdb,
                SpatialIndexType = ind.SpatialIndexType,
            };

            foreach (IndexedColumn iColumn in ind.IndexedColumns)
            {
                var newIdxColumn = new IndexedColumn(idx, iColumn.Name)
                {
                    Descending = iColumn.Descending,
                    IsIncluded = iColumn.IsIncluded,
                };
                idx.IndexedColumns.Add(newIdxColumn);
            }
            idx.IndexedColumns.Add(new IndexedColumn(idx, column.Name));
            
            bool oldDisallowPageLocks = ind.DisallowPageLocks;
            ind.Drop();
            idx.Create();
            idx.DisallowPageLocks = oldDisallowPageLocks;
            idx.Alter();
        }

        public static DataColumnDbo CreateDataColumnDbo(Column clmn, IList<string> primaryKeyColumns)
        {
            var rzlt = new DataColumnDbo()
            {
                Name = clmn.Name,
                SqlDataType = clmn.DataType.Name,
                MaximumLength = clmn.DataType.MaximumLength,
                NumericPrecision = clmn.DataType.NumericPrecision,
                NumericScale = clmn.DataType.NumericScale,
                IsNullable = clmn.Nullable,
                IsIdentity = clmn.Identity,
                IsPrimaryKey = primaryKeyColumns.Contains(clmn.Name),
            };
            if (clmn.DefaultConstraint != null)
            {
                string dfText = clmn.DefaultConstraint.Text;
                while (dfText.StartsWith("(") && dfText.EndsWith(")"))
                    dfText = dfText.Substring(1, dfText.Length - 2);
                rzlt.Default = dfText;
            }
            
            return rzlt;
        }

        /// <summary>
        /// Packs all exception messages into single string.
        /// </summary>
        /// <param name="ex"><see cref="Exception"/></param>
        /// <returns>String with messages from the exception and inner ones.</returns>
        public static string ParseErrorMessage(Exception ex)
        {
            var msg = new StringBuilder(ex.Message);
            for (Exception inner = ex.InnerException; inner != null; inner = inner.InnerException)
            {
                msg.Append("\n");
                msg.Append(inner.Message);
            }
            return msg.ToString();
        }

        /// <summary>
        /// Returns table object
        /// </summary>
        /// <param name="tableName">String with table name</param>
        /// <param name="db">Curent database</param>
        /// <returns><see cref="Table"/></returns>
        public static Table GetTable(string tableName, out Database db)
        {
            var srv = GetConnectedServer(SqlServerName, UserName, Password);
            if (srv == null)
                throw new Exception(String.Format("Server '{0}' was not found.", SqlServerName));

            db = srv.Databases[DatabaseName];
            if (srv == null)
                throw new Exception(String.Format("Database '{0}' was not found.", DatabaseName));

            Table aTable = db.Tables[tableName];
            if (aTable == null)
                throw new Exception(String.Format("Database '{0}' has no table '{1}'.", DatabaseName, tableName));

            return aTable;
        }
        
        private static IList<string> GetColumnNames(Index ind)
        {
            var rzlt = new List<string>();
            foreach (IndexedColumn clm in ind.IndexedColumns)
                rzlt.Add(clm.Name);
            return rzlt;
        }
        private static IList<string> GetColumnNames(ForeignKey fKey)
        {
            var rzlt = new List<string>();
            foreach (ForeignKeyColumn clm in fKey.Columns)
                rzlt.Add(clm.Name);
            return rzlt;
        }

        // Delete index from the table if it contains column that needs to be deleted
        private static void DeleteColumnFromIndexes(Table aTable, Column aColumn, List<DroppedDependencyDbo> alteredDependencies)
        {
            Func<Index, string, bool> indexContainsColumn = delegate(Index ind, string name)
            {
                foreach (IndexedColumn clm in ind.IndexedColumns)
                {
                    if (String.Compare(clm.Name, name, true) == 0)
                        return true;
                }
                return false;
            };

            for (int i = aTable.Indexes.Count - 1; i >= 0; i--)
            {
                Index ind = aTable.Indexes[i];
                if (indexContainsColumn(ind, aColumn.Name))
                {
                    alteredDependencies.Add(new DroppedDependencyDbo()
                    {
                        Name = ind.Name,
                        ObjectType = ind.IndexKeyType == IndexKeyType.DriPrimaryKey ? DbObjectType.primaryKey : DbObjectType.index,
                        TableName = aTable.Name,
                        Columns = GetColumnNames(ind)
                    });
                    ind.Drop();
                }
            }
        }

        /// <summary>
        /// The function returns List of foreign keys that references a column in the table.
        /// The function quries data with Linq to datable 
        /// <see cref="http://msdn.microsoft.com/en-us/library/system.data.datatableextensions_methods.aspx"/>
        /// </summary>
        /// <param name="tableName">Name of target table </param>
        /// <param name="columnName">Name of target column</param>
        /// <returns>
        ///     <para>List of pairs <see cref="KeyValuePair"/></para>
        ///     <list type="bullet">
        ///         <item>key - name of the foreign key</item>,
        ///         <item>value - name of the table</item>
        ///     </list>
        /// </returns>
        private static List<KeyValuePair<string, string>> GetForeignKeys(string tableName, string columnName)
        {
            const string queryString =
                "SELECT own.name AS TableName, fk.name AS ForeignKeyName " +
                "FROM sys.foreign_key_columns fkc " +
                    "JOIN sys.objects trg ON trg.object_id = fkc.referenced_object_id " +
                    "JOIN sys.objects fk ON fk.object_id = fkc.constraint_object_id " +
                    "JOIN sys.objects own ON own.object_id = fkc.parent_object_id " +
                "WHERE trg.name LIKE @TableName " +
                    "AND EXISTS ( " +
                        "SELECT * " +
                        "FROM sys.columns clm " +
                        "WHERE fkc.referenced_object_id = clm.object_id " +
                            "AND fkc.referenced_column_id = clm.column_id " +
                            "AND clm.name LIKE @ColumnName " +
                ")";
            using (var connection = new SqlConnection(ConnectionString))
            {
                var da = new SqlDataAdapter(queryString, connection);
                da.SelectCommand.Parameters.AddWithValue("@TableName", tableName);
                da.SelectCommand.Parameters.AddWithValue("@ColumnName", columnName);
                da.TableMappings.Add("Table", "ForeignKeyList");

                var ds = new DataSet();
                da.Fill(ds);
                DataTable foreignKeys = ds.Tables["ForeignKeyList"];
                var rzlt = (
                    from fKey in foreignKeys.AsEnumerable()
                    select new KeyValuePair<string, string>(fKey.Field<string>("ForeignKeyName"), fKey.Field<string>("TableName"))
                    ).ToList();
                return rzlt;
            }
        }

        /// <summary>
        /// Delete foreign keys from the database if it contains column that needs to be deleted
        /// </summary>
        /// <param name="db">Current database <see cref="Database"/></param>
        /// <param name="aTable">Current table, <see cref="Table"/></param>
        /// <param name="aColumn">Current column, <see cref="Column"/></param>
        /// <param name="alteredDependencies">List with dropped dependecies</param>
        private static void DeleteColumnFromForeignKeys(Database db, Table aTable, Column aColumn, 
                                                        List<DroppedDependencyDbo> alteredDependencies)
        {
            Func<ForeignKey, IList<string>> getColumnNames = delegate(ForeignKey foreignKey)
            {
                var rzlt = new List<string>();
                foreach (ForeignKeyColumn clm in foreignKey.Columns)
                    rzlt.Add(clm.Name);
                return rzlt;
            };

            List<KeyValuePair<string, string>> foreignKeys = GetForeignKeys(aTable.Name, aColumn.Name);
            foreach (KeyValuePair<string, string> pair in foreignKeys)
            {
                Table currentTable = db.Tables[pair.Value];
                if (currentTable != null)
                {
                    ForeignKey fKey = currentTable.ForeignKeys[pair.Key];
                    if (fKey != null)
                    {
                        alteredDependencies.Add(new DroppedDependencyDbo()
                        {
                            Name = fKey.Name,
                            ObjectType = DbObjectType.foreignKey,
                            TableName = currentTable.Name,
                            Columns = getColumnNames(fKey)
                        });
                        fKey.Drop();
                    }
                    currentTable.Alter();
                }
            }
        }
    }
}