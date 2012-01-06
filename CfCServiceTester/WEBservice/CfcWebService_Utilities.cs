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
        /// <summary>
        /// SQL server's name
        /// </summary>
        public static readonly string SqlServerNameKey = "{0B568310-9686-4CF9-B38C-99ED7FE48D87}";
        /// <summary>
        /// Database name
        /// </summary>
        public static readonly string DatabaseNameKey = "{93CA56A2-3CE6-456D-86E6-0C9849CF363E}";

        internal static string ConnectionString
        {
            get { return (string)HttpContext.Current.Session[ConnectionStringKey]; }
            set { HttpContext.Current.Session[ConnectionStringKey] = value; }
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
                        rzlt.Add(new AlteredDependencyDbo() { ObjectType = DbObjectType.StoredProcedure, Name = vw.Name });
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
                        rzlt.Add(new AlteredDependencyDbo() { ObjectType = DbObjectType.StoredProcedure, Name = udf.Name });
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

        private static void InsertColumnIntoPrimarykey(Table table, Column column)
        {
            var query = (
                from Index ind in table.Indexes
                where ind.IndexKeyType == IndexKeyType.DriPrimaryKey
                select ind).FirstOrDefault();
            if (query == null)
            {
                var primaryKeyIndex = new Index(table, String.Concat("PK_", table.Name)) { IndexKeyType = IndexKeyType.DriPrimaryKey };
                primaryKeyIndex.IndexedColumns.Add(new IndexedColumn(primaryKeyIndex, column.Name));
                table.Indexes.Add(primaryKeyIndex);
            }
            else
                query.IndexedColumns.Add(new IndexedColumn(query, column.Name));
        }

        public static DataColumnDbo CreateDataColumnDbo(Column clmn, List<string> primaryKeyColumns)
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
            var srv = new Server(SqlServerName);
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
    }
}