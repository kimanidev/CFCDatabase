using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CfCServiceTester.SVC.DataObjects;
using System.Security.Cryptography;
using Microsoft.SqlServer.Management.Smo;
using System.Data.SqlClient;
using System.Data;
using CfCServiceTester.WEBservice.DataObjects;
using System.Text.RegularExpressions;

namespace CfCServiceTester.WEBservice
{
    /// <summary>
    /// Utilities that are not callable from outside world. These method may be used from server side only.
    /// </summary>
    public partial class CfcWebService
    {
        /// <summary>
        /// The application generates public/private key individual for every session and stores it
        /// in <code>Session[CertificateKey]</code>
        /// </summary>
        public static readonly string CertificateKey = "{4908DA1F-CC08-4816-8971-1166CD3B03DB}";
        /// <summary>
        /// List with disconnected hosts is stored in the session
        /// </summary>
        public static readonly string HostsListKey = "{AEEA6FE4-1129-4108-B0CD-B0243FCADCAB}";

        /// <summary>
        /// The application generates public/private key individual for every session and stores it
        /// in <code>Session[CertificateKey]</code>
        /// </summary>
        public static string MyRSA
        {
            get { return (string)HttpContext.Current.Session[CertificateKey]; }
            set { HttpContext.Current.Session[CertificateKey] = value; }
        }

        /// <summary>
        /// List with disconnected hosts is stored in the session
        /// </summary>
        public static List<string> DisconnectedHosts
        {
            get 
            {
                object tmp = HttpContext.Current.Session[HostsListKey];
                return tmp == null ? new List<string>() : (List<string>)tmp;
            }
            set { HttpContext.Current.Session[HostsListKey] = value; }
        }

        /// <summary>
        /// Creates public/private RSA keys, individual pair for every session.
        /// </summary>
        /// <returns></returns>
        public static RsaParametersDbo GetPublicKey()
        {
            CspParameters cspParams = new CspParameters();
            cspParams.Flags = CspProviderFlags.UseMachineKeyStore;
            var myRSA = new RSACryptoServiceProvider(cspParams);
            if ( String.IsNullOrEmpty(MyRSA))
                MyRSA = myRSA.ToXmlString(true);

            myRSA.FromXmlString(MyRSA);

            RSAParameters param = myRSA.ExportParameters(false);
            var rzlt = new RsaParametersDbo()
            {
                Exponent = ToHexString(param.Exponent),
                Modulus = ToHexString(param.Modulus)
            };
            return rzlt;
        }

        /// <summary>
        /// Converts byte array to string
        /// </summary>
        /// <param name="byteValue">Byte array</param>
        /// <returns>Converted string</returns>
        public static string ToHexString(byte[] byteValue)
        {
            char[] lookup = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
            int i = 0, p = 0, l = byteValue.Length;
            char[] c = new char[l * 2];
            while (i < l)
            {
                byte d = byteValue[i++];
                c[p++] = lookup[d / 0x10];
                c[p++] = lookup[d % 0x10];
            }
            return new string(c, 0, c.Length);
        }
        /// <summary>
        /// Restores byte array from string.
        /// </summary>
        /// <param name="str">String with hexadecimal digits</param>
        /// <returns>Compressed byte array</returns>
        public static byte[] ToHexByte(string str)
        {
            byte[] b = new byte[str.Length / 2];
            for (int y = 0, x = 0; x < str.Length; ++y, x++)
            {
                byte c1 = (byte)str[x];
                if (c1 > 0x60) c1 -= 0x57;
                else if (c1 > 0x40) c1 -= 0x37;
                else c1 -= 0x30;
                byte c2 = (byte)str[++x];
                if (c2 > 0x60) c2 -= 0x57;
                else if (c2 > 0x40) c2 -= 0x37;
                else c2 -= 0x30;
                b[y] = (byte)((c1 << 4) + c2);
            }
            return b;
        }

        /// <summary>
        /// Sends message to connected users and switches databas to single user mode.
        /// <see cref="http://www.codeproject.com/KB/database/SqlServer_Backup_Restore.aspx?msg=3221753"/>
        /// <param name="dbName">Database name</param>
        /// <param name="procedureName">Call the procedure before switching to single user mode</param>
        /// </summary>
        public static bool SetSingleMode(string dbName, string procedureName = null)
        {
            var srv = GetConnectedServer(SqlServerName, UserName, Password);
            var db = srv.Databases[dbName];
            // db == null for new databases; there is no need for switching to single mode in this case
            return db == null ? true : SetSingleMode(db, procedureName);
        }
        private static bool SetSingleMode(Database db, string procedureName = null)
        {
            try
            {
                if (!String.IsNullOrEmpty(procedureName))
                    KillUsers(procedureName);
                DisconnectedHosts = GetConnectedHosts();
                SendNotification(String.Format("Access to the [{0}].[{1}] database is locked.", SqlServerName, db.Name));
                db.DatabaseOptions.UserAccess = DatabaseUserAccess.Single;
                db.Alter(TerminationClause.RollbackTransactionsImmediately);
                
                return true;
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Sends message to connected users and switches databas to normal, multiuser mode.
        /// <see cref="http://www.codeproject.com/KB/database/SqlServer_Backup_Restore.aspx?msg=3221753"/>
        /// <param name="dbName">Database name</param>
        /// </summary>
        public static void SetMultiUserMode(string dbName)
        {
            var srv = GetConnectedServer(SqlServerName, UserName, Password);
            var db = srv.Databases[dbName];
            if (db != null)
                SetMultiUserMode(db);
        }
        private static void SetMultiUserMode(Database db)
        {
            db.DatabaseOptions.UserAccess = DatabaseUserAccess.Multiple;
            db.Alter(TerminationClause.RollbackTransactionsImmediately);

            SendNotification(String.Format("Access to the [{0}].[{1}] database is free now.", SqlServerName, db.Name));
        }

        /// <summary>
        /// Renames table.
        /// <see cref="http://www.codeproject.com/KB/database/SqlServer_Backup_Restore.aspx?msg=3221753"/>
        /// Foreign keys that are referenced to renamed table are correct but it is neccessary to replace views, triggers, stored procedures
        /// and user defined functions. 
        /// <see cref=" http://www.youdidwhatwithtsql.com/altering-database-objects-with-powershell/119"/>
        /// <strong>This algorithm does not change CLR functions. You must to recompile them!</strong>
        /// <param name="oldName">Old name</param>
        /// <param name="newName">New name</param>
        /// </summary>
        public static List<AlteredDependencyDbo> RenameTheTable(string oldName, string newName)
        {
            var srv = GetConnectedServer(SqlServerName, UserName, Password);
            var db = srv.Databases[DatabaseName];
            Table aTable = db.Tables[newName];
            if (aTable != null)
                throw new Exception(String.Format("Table {0} exists in the {1} database.", newName, DatabaseName));
            aTable = db.Tables[oldName];
            if (aTable == null)
                throw new Exception(String.Format("There isn no table {0} in the {1} database.", newName, DatabaseName));

            RenameAllIndexes(aTable, newName);
            RenameThisForeignKeys(aTable, newName);
            RenameOtherForeignKeys(db, aTable, newName);

            aTable.Rename(newName);
            return CorrectStoredProcedure(db, oldName, newName);
        }

        private static void RenameOtherForeignKeys(Database db, Table aTable, string newTableName)
        {
            string pattern = String.Format(@"_{0}\b|_{0}_", aTable.Name);
            var rg = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            DataTable tbForeignKeys = aTable.EnumForeignKeys();

            var foreignKeyList = 
                from fKey in tbForeignKeys.AsEnumerable()
                where rg.IsMatch(fKey.Field<string>("Name"))
                select new {
                    TableName = fKey.Field<string>("Table_Name"),
                    ForeignKeyName = fKey.Field<string>("Name")
                };

            foreach (var fk in foreignKeyList)
            {
                Table currentTable = db.Tables[fk.TableName];
                ForeignKey key = currentTable.ForeignKeys[fk.ForeignKeyName];
                string newKeyName = rg.Replace(key.Name, delegate(Match m)
                {
                    string mString = m.ToString();
                    return String.Format("_{0}{1}", newTableName, mString.EndsWith("_") ? "_" : String.Empty);
                });
                key.Rename(newKeyName);
                key.Alter();
            }
        }

        private static void RenameThisForeignKeys(Table aTable, string newTableName)
        {
            string pattern = String.Format(@"_{0}\b|_{0}_", aTable.Name);
            var rg = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            for (int i = aTable.ForeignKeys.Count - 1; i >= 0; i--)
            {
                ForeignKey key = aTable.ForeignKeys[i];
                string newKeyName = rg.Replace(key.Name, delegate(Match m)
                {
                    string mString = m.ToString();
                    return String.Format("_{0}{1}", newTableName, mString.EndsWith("_") ? "_" : String.Empty);
                });
                if (String.Compare(key.Name, newKeyName, true) != 0)
                {
                    key.Rename(newKeyName);
                    key.Alter();
                }
            }
        }

        private static void RenameAllIndexes(Table aTable, string newTableName)
        {
            string pattern = String.Format(@"_{0}\b|_{0}_", aTable.Name);
            var rg = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            for (int i = aTable.Indexes.Count - 1; i >= 0; i--)
            {
                Index ind = aTable.Indexes[i];
                string newIndexName = rg.Replace(ind.Name, delegate(Match m)
                {
                    string mString = m.ToString();
                    return String.Format("_{0}{1}", newTableName, mString.EndsWith("_") ? "_" : String.Empty);
                });
                if (String.Compare(ind.Name, newIndexName, true) != 0)
                {
                    ind.Rename(newIndexName);
                    ind.Alter();
                }
            }
        }

        /// <summary>
        /// Renames index or primary key.
        /// <param name="tableName">Table name</param>
        /// <param name="oldName">Old name</param>
        /// <param name="newName">New name</param>
        /// </summary>
        public static IndexDbo RenameTheIndex(string tableName, string oldName, string newName)
        {
            var srv = GetConnectedServer(SqlServerName, UserName, Password);
            var db = srv.Databases[DatabaseName];
            Table aTable = db.Tables[tableName];
            if (aTable == null)
                throw new Exception(String.Format("There is no table {0} in the {1} database.", tableName, DatabaseName));

            Index ind = aTable.Indexes[oldName];
            if (ind == null)
                throw new Exception(String.Format("There is no index {0} in the {1} table.", oldName, tableName));

            ind.Rename(newName);
            ind.Alter();
            return GetIndexDescription(aTable, newName); ;
        }

        /// <summary>
        /// Renames foreign key.
        /// <param name="tableName">Table name</param>
        /// <param name="oldName">Old name</param>
        /// <param name="newName">New name</param>
        /// </summary>
        public static ForeignKeyDbo RenameTheForeignKey(string tableName, string oldName, string newName)
        {
            var srv = GetConnectedServer(SqlServerName, UserName, Password);
            var db = srv.Databases[DatabaseName];
            Table aTable = db.Tables[tableName];
            if (aTable == null)
                throw new Exception(String.Format("There is no table {0} in the {1} database.", tableName, DatabaseName));

            ForeignKey fKey = aTable.ForeignKeys[oldName];
            if (fKey == null)
                throw new Exception(String.Format("There is no foreign key {0} in the {1} table.", oldName, tableName));

            fKey.Rename(newName);
            fKey.Alter();
            return CreateForeignKeyDbo(fKey);
        }

        /// <summary>
        /// Modifies trhe index
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="dbo">Index description, <see cref="IndexDbo"/></param>
        /// <param name="disableDependencies"><code>true</code> - drop dependencies</param>
        /// <param name="newDbo">New, updated index</param>
        /// <returns>List of dropped foreign keys</returns>
        private static List<DroppedDependencyDbo> UpdateTheIndex(string tableName, IndexDbo dbo, bool disableDependencies,
                                                                 out IndexDbo newDbo)
        {
            var srv = GetConnectedServer(SqlServerName, UserName, Password);
            var db = srv.Databases[DatabaseName];
            Table aTable = db.Tables[tableName];
            if (aTable == null)
                throw new Exception(String.Format("There is no table {0} in the {1} database.", tableName, DatabaseName));
            IndexDbo oldIndex = GetIndexDescription(aTable, dbo.Name);
            if (oldIndex == dbo)
                throw new Exception("There is nothing to change.");

            List<DroppedDependencyDbo> droppedDependencies =  DeleteTheIndex(db, aTable, dbo.Name, disableDependencies);
            newDbo = CreateTheIndex(aTable, dbo, aTable.Name);
            return droppedDependencies;
        }

        /// <summary>
        /// Creates index or primary key.
        /// <param name="tableName">Table name</param>
        /// <param name="dbo">Index descriptor, <see cref="IndexDbo"/></param>
        /// <param name="uniqueInTable">If the value is not empty - look for equal names in this table only </param>
        /// </summary>
        public static IndexDbo CreateTheIndex(string tableName, IndexDbo dbo, string uniqueInTable = null)
        {
            var srv = GetConnectedServer(SqlServerName, UserName, Password);
            var db = srv.Databases[DatabaseName];
            Table aTable = db.Tables[tableName];
            if (aTable == null)
                throw new Exception(String.Format("There is no table {0} in the {1} database.", tableName, DatabaseName));

            return CreateTheIndex(aTable, dbo, uniqueInTable);
        }
        public static IndexDbo CreateTheIndex(Table aTable, IndexDbo dbo, string uniqueInTable)
        {
            string errMessage = IsIndexUnique(dbo.Name, uniqueInTable);
            if (!String.IsNullOrEmpty(errMessage))
                throw new Exception(errMessage);

            if (dbo.IsUnique && !AreValuesUnique(aTable, dbo.IndexedColumns.ToArray()))
                throw new Exception(String.Format("Table '{0}' contains duplicates in selected set of columns.", aTable.Name));

            // See CreateNewPrimaryKey in the Utilities_1
            CreateNewPrimaryKey(aTable, dbo);
            return GetIndexDescription(aTable, dbo.Name);
        }

        /// <summary>
        /// Renames index or primary key.
        /// <param name="tableName">Table name</param>
        /// <param name="indexName">Index Name</param>
        /// <param name="disableDependencies"><code>true</code> - remove foreign keys that target is current index</param>
        /// <returns>List of dropped foreign keys, <see cref="DroppedDependencyDbo"/></returns>
        /// </summary>
        public static List<DroppedDependencyDbo> DeleteTheIndex(string tableName, string indexName, bool disableDependencies)
        {
            var srv = GetConnectedServer(SqlServerName, UserName, Password);
            var db = srv.Databases[DatabaseName];
            Table aTable = db.Tables[tableName];
            if (aTable == null)
                throw new Exception(String.Format("There is no table {0} in the {1} database.", tableName, DatabaseName));
            return DeleteTheIndex(db, aTable, indexName, disableDependencies);
        }
        public static List<DroppedDependencyDbo> DeleteTheIndex(Database db, Table aTable, string indexName, bool disableDependencies)
        {
            Index ind = aTable.Indexes[indexName];
            if (ind == null)
                throw new Exception(String.Format("There is no index {0} in the {1} table.", indexName, aTable.Name));

            var droppedForeignKeys = new List<DroppedDependencyDbo>();
            if (disableDependencies)
                DropDependentForeignKeys(indexName, db, droppedForeignKeys);
            ind.Drop();
            aTable.Alter();

            return droppedForeignKeys;
        }

        /// <summary>
        /// Renames index or primary key.
        /// <param name="tableName">Table name</param>
        /// <param name="fKeyName">Index Name</param>
        /// </summary>
        public static void DeleteTheForeignKey(string tableName, string fKeyName)
        {
            var srv = GetConnectedServer(SqlServerName, UserName, Password);
            var db = srv.Databases[DatabaseName];
            Table aTable = db.Tables[tableName];
            if (aTable == null)
                throw new Exception(String.Format("There is no table {0} in the {1} database.", tableName, DatabaseName));
            DeleteTheForeignKey(db, aTable, fKeyName);
        }
        public static void DeleteTheForeignKey(Database db, Table aTable, string fKeyName)
        {
            ForeignKey fKey = aTable.ForeignKeys[fKeyName];
            if (fKey == null)
                throw new Exception(String.Format("There is no foreign key {0} in the {1} table.", fKeyName, aTable.Name));

            fKey.Drop();
            aTable.Alter();
        }

        /// <summary>
        /// Creates new foreign key
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="dbo">Foreign key description <see cref="ForeignKeyDbo"/></param>
        /// <returns>Description of created foreign key</returns>
        public static ForeignKeyDbo CreateForeignKey(string tableName, ForeignKeyDbo dbo)
        {
            var srv = GetConnectedServer(SqlServerName, UserName, Password);
            var db = srv.Databases[DatabaseName];
            Table aTable = db.Tables[tableName];
            if (aTable == null)
                throw new Exception(String.Format("There is no table {0} in the {1} database.", tableName, DatabaseName));
            if (aTable.ForeignKeys[dbo.Name] != null)
                throw new Exception(String.Format("There is {0} foreign key in the table {1}.", dbo.Name, tableName));
            
            return CreateForeignKey(aTable, dbo);
        }

        public static IEnumerable<DataColumnDbo> GetTableColumns(string tableName, bool createNewTable = false)
        {
            var srv = GetConnectedServer(SqlServerName, UserName, Password);
            if (srv == null)
                throw new Exception(String.Format("Server '{0}' was not found.", SqlServerName));
            var db = srv.Databases[DatabaseName];
            if (srv == null)
                throw new Exception(String.Format("Database '{0}' was not found.", DatabaseName));

            Table aTable = db.Tables[tableName];
            if (aTable == null)
            {
                if (createNewTable)
                {
                    aTable = new Table(db, tableName);  // It is impossible to create table without columns
                    var col = new Column(aTable, "ID", DataType.Int) { Nullable = false };   
                    aTable.Columns.Add(col);
                    aTable.Create();
                }
                else
                    throw new Exception(String.Format("Database '{0}' has no table '{1}'.", DatabaseName, tableName));
            }
            List<string> primaryKeyColumns = GetPrimaryKeyColumns(aTable);

            foreach (Column clmn in aTable.Columns)
            {
                yield return CreateDataColumnDbo(clmn, primaryKeyColumns);
            }
        }

        /// <summary>
        /// Removes table from database
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="disableDependencies"><code>true</code> - remove foreign keys that references the table</param>
        public static List<DroppedDependencyDbo> DeleteTheTable(string tableName, bool disableDependencies)
        {
            var srv = GetConnectedServer(SqlServerName, UserName, Password);
            if (srv == null)
                throw new Exception(String.Format("Server '{0}' was not found.", SqlServerName));
            var db = srv.Databases[DatabaseName];
            if (srv == null)
                throw new Exception(String.Format("Database '{0}' was not found.", DatabaseName));

            Table aTable = db.Tables[tableName];
            if (aTable == null)
                throw new Exception(String.Format("Database '{0}' has no table '{1}'.", DatabaseName, tableName));

            var droppedForeignKeys = new List<DroppedDependencyDbo>();
            DeleteTable(aTable, disableDependencies, db, droppedForeignKeys);
            return droppedForeignKeys;
         }

        /// <summary>
        /// Inserts new column into the table
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="column">Column definition, <see cref="DataColumnDbo"/></param>
        /// <param name="singleUserMode"><code>true</code> - switch to single user mode</param>
        /// <param name="disableDependencies">
        ///     <code>true</code> - drop foreign keys that references primary key if column is new member of priamry key
        /// </param>
        /// <param name="droppedKeys">Foreign keys, that were removed for recreating primary key.</param>
        /// <returns>Column description, <see cref="DataColumnDbo"/></returns>
        public static DataColumnDbo InsertColumn(string tableName, DataColumnDbo column, bool singleUserMode, bool disableDependencies,
                                    out List<DroppedDependencyDbo> droppedKeys)
        {
            Database db = null;
            bool isSingleUserMode = false;
            try
            {
                Table aTable = GetTable(tableName, out db);
                if (singleUserMode && !column.IsPrimaryKey)
                    isSingleUserMode = SetSingleMode(db);

                Column newColumn = CreateColumn(aTable, column);
                aTable.Alter();
                if (column.IsPrimaryKey)
                {
                    droppedKeys = InsertColumnIntoPrimarykey(aTable, newColumn, disableDependencies, db);
                    aTable.Alter();
                }
                else
                    droppedKeys = new List<DroppedDependencyDbo>();

                List<string> primaryKeyColumns = GetPrimaryKeyColumns(aTable);
                return CreateDataColumnDbo(newColumn, primaryKeyColumns);
            }
            finally
            {
                if (isSingleUserMode && db != null)
                    SetMultiUserMode(db);
            }
        }

        private static DataColumnDbo GetDataColumnDbo(Table aTable, string columnName, out Column currentColumn)
        {
            Column column = aTable.Columns[columnName];
            if (column == null)
                throw new Exception(String.Format("Column '{0}' was not found in the '{1}' table.", columnName, aTable.Name));
            List<string> primaryKeyColumns = GetPrimaryKeyColumns(aTable);
            currentColumn = column;
            return CreateDataColumnDbo(column, primaryKeyColumns);
        }

        /// <summary>
        /// The procedure manages processing of the column updating.
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="column">Column description </param>
        /// <param name="disableDependencies"><code>true</code> - remove dependencies</param>
        /// <param name="singleUserMode"><code>true</code> - single user mode</param>
        /// <param name="droppedKeys">List of dropped foreign keys</param>
        /// <returns>Description of the updated column</returns>
        public static DataColumnDbo UpdateColumn(string tableName, DataColumnDbo column, bool disableDependencies, bool singleUserMode,
                                        out List<DroppedDependencyDbo> droppedKeys)
        {
            Database db = null;
            bool isSingleUserMode = false;
            var droppedForeignKeys = new List<DroppedDependencyDbo>();
            try
            {
                Table aTable = GetTable(tableName, out db);
                if (singleUserMode && !column.IsPrimaryKey)
                    isSingleUserMode = SetSingleMode(db);
                Column currentColumn;
                DataColumnDbo oldValues = GetDataColumnDbo(aTable, column.Name, out currentColumn);

                if (oldValues.IsNullable && !column.IsNullable)
                    RemoveNullCondition(aTable, currentColumn);

                
                #region Primary Key block
                if (!oldValues.IsPrimaryKey && column.IsPrimaryKey)
                {
                    // Include into primary key
                    droppedForeignKeys.AddRange(InsertColumnIntoPrimarykey(aTable, currentColumn, disableDependencies, db));
                    aTable.Alter();
                }
                else if (oldValues.IsPrimaryKey && !column.IsPrimaryKey)
                {
                    // Remove from primary key
                    droppedForeignKeys.AddRange(RemoveColumnFromPrimarykey(aTable, currentColumn, disableDependencies, db));
                    aTable.Alter();
                }
                #endregion

                if (!oldValues.IsNullable && column.IsNullable)
                {
                    droppedForeignKeys.AddRange(RestoreNullCondition(aTable, currentColumn, disableDependencies, db));
                    aTable.Alter();
                }
                if (DataColumnDbo.RequiresRecreating(oldValues, column))
                    droppedForeignKeys.AddRange(RemoveOrRestoreIdentity(aTable, column, disableDependencies, db));

                droppedKeys = droppedForeignKeys;
                return GetDataColumnDbo(aTable, column.Name, out currentColumn);
            }
            finally
            {
                if (isSingleUserMode && db != null)
                    SetMultiUserMode(db);
            }
        }
        /// <summary>
        /// Inserts new column into the table
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="oldColumnName">Old column name</param>
        /// <param name="newColumnName">New column name</param>
        /// <param name="singleUserMode"><code>true</code> - switch to single user mode</param>
        /// <param name="alteredDependencies">List with altered procedures, functions, triggers and views.</param>
        /// <returns>Column description, <see cref="DataColumnDbo"/></returns>
        public static DataColumnDbo RenameColumn(string tableName, string oldColumnName, string newColumnName, 
                                                 bool singleUserMode, out List<AlteredDependencyDbo> alteredDependencies)
        {
            Database db = null;
            bool isSingleUserMode = false;
            try
            {
                Table aTable = GetTable(tableName, out db);
                if (singleUserMode)
                    isSingleUserMode = SetSingleMode(db);

                Column aColumn = aTable.Columns[oldColumnName];
                if (aColumn == null)
                    throw new Exception(String.Format("Table '{0}' has no column '{1}'.", tableName, oldColumnName));

                aColumn.Rename(newColumnName);
                aTable.Alter();
                Column newColumn = aTable.Columns[newColumnName];

                alteredDependencies = CorrectFieldNames(db, tableName, oldColumnName, newColumnName);
                List<string> primaryKeyColumns = GetPrimaryKeyColumns(aTable);
                return CreateDataColumnDbo(newColumn, primaryKeyColumns);
            }
            finally
            {
                if (isSingleUserMode && db != null)
                    SetMultiUserMode(db);
            }
        }

        private static void RenameDatabase(string newName)
        {
            string oldName = DatabaseName;
            DatabaseName = newName;
            string connString = ConnectionString;

            string regexTemplate = String.Format(@"\b{0}\b", oldName);        
            var rg = new Regex(regexTemplate, RegexOptions.Compiled | RegexOptions.IgnoreCase);

            ConnectionString = rg.Replace(connString, newName);
        }

        public static void DeleteColumn(string tableName, string columnName, bool DisableDependencies,
                                                 bool singleUserMode, out List<DroppedDependencyDbo> alteredDependencies)
        {
            Database db = null;
            var droppedDependencies = new List<DroppedDependencyDbo>();
            bool isSingleUserMode = false;
            try
            {
                Table aTable = GetTable(tableName, out db);
                if (singleUserMode)
                    isSingleUserMode = SetSingleMode(db);

                Column aColumn = aTable.Columns[columnName];
                if (aColumn == null)
                    throw new Exception(String.Format("Table '{0}' has no column '{1}'.", tableName, columnName));

                if (DisableDependencies)
                {
                    DeleteColumnFromForeignKeys(db, aTable, aColumn, droppedDependencies);
                    DeleteColumnFromIndexes(aTable, aColumn, droppedDependencies);
                }

                aColumn.Drop();
                aTable.Alter();
            }
            finally
            {
                if (isSingleUserMode && db != null)
                    SetMultiUserMode(db);

                alteredDependencies = droppedDependencies;
            }
        }

        public static List<string> GetAllTables()
        {
            var srv = GetConnectedServer(SqlServerName, UserName, Password);
            if (srv == null)
                throw new Exception(String.Format("Server '{0}' is not accessible.", SqlServerName));
            var db = srv.Databases[DatabaseName];
            if (db == null)
                throw new Exception(String.Format("Database '{0}' is not accessible.", DatabaseName)); ;

            var rzlt = new List<string>();
            foreach (Table currentTable in db.Tables)
                rzlt.Add(currentTable.Name);
            return rzlt;
        }

        public static List<string> GetAllIndexes(string tableName)
        {
            var srv = GetConnectedServer(SqlServerName, UserName, Password);
            if (srv == null)
                throw new Exception(String.Format("Server '{0}' is not accessible.", SqlServerName));
            var db = srv.Databases[DatabaseName];
            if (db == null)
                throw new Exception(String.Format("Database '{0}' is not accessible.", DatabaseName));
            Table table = db.Tables[tableName];
            if (table == null)
                throw new Exception(String.Format("Database '{0}' has no table '{1}'.", DatabaseName, tableName));

            return GetAllIndexes(table);
        }
        private static List<string> GetAllIndexes(Table table)
        {
            var rzlt = new List<string>();
            foreach (Index currentIndex in table.Indexes)
                rzlt.Add(currentIndex.Name);
            return rzlt;
        }

        public static List<ForeignKeyDbo> GetForeignKeys(string tableName)
        {
            var srv = GetConnectedServer(SqlServerName, UserName, Password);
            if (srv == null)
                throw new Exception(String.Format("Server '{0}' is not accessible.", SqlServerName));
            var db = srv.Databases[DatabaseName];
            if (db == null)
                throw new Exception(String.Format("Database '{0}' is not accessible.", DatabaseName));
            Table table = db.Tables[tableName];
            if (table == null)
                throw new Exception(String.Format("Database '{0}' has no table '{1}'.", DatabaseName, tableName));

            return GetForeignKeys(table);
        }
        private static List<ForeignKeyDbo> GetForeignKeys(Table table)
        {
            var rzlt = new List<ForeignKeyDbo>();
            foreach (ForeignKey currentKey in table.ForeignKeys)
                rzlt.Add(CreateForeignKeyDbo(currentKey));
            return rzlt;
        }

        public static ForeignKeyDbo GetThisForeignKey(string tableName, string foreignKeyName)
        {
            var srv = GetConnectedServer(SqlServerName, UserName, Password);
            if (srv == null)
                throw new Exception(String.Format("Server '{0}' is not accessible.", SqlServerName));
            var db = srv.Databases[DatabaseName];
            if (db == null)
                throw new Exception(String.Format("Database '{0}' is not accessible.", DatabaseName));
            Table table = db.Tables[tableName];
            if (table == null)
                throw new Exception(String.Format("Database '{0}' has no table '{1}'.", DatabaseName, tableName));
            ForeignKey aKey = table.ForeignKeys[foreignKeyName];
            if (aKey == null)
                throw new Exception(String.Format("Table '{0}' has no key '{1}'.", tableName, foreignKeyName));

            ForeignKeyDbo foreignKey = CreateForeignKeyDbo(aKey);
            foreignKey.AvailableTargetColumns = GetTargetColumns(foreignKey.ReferencedTable);
            return foreignKey;
        }

        public static List<IndexDbo> GetTableIndexes(string tableName)
        {
            var srv = GetConnectedServer(SqlServerName, UserName, Password);
            if (srv == null)
                throw new Exception(String.Format("Server '{0}' is not accessible.", SqlServerName));
            var db = srv.Databases[DatabaseName];
            if (db == null)
                throw new Exception(String.Format("Database '{0}' is not accessible.", DatabaseName));
            Table table = db.Tables[tableName];
            if (table == null)
                throw new Exception(String.Format("Tabase '{0}' has no table '{1}'.", DatabaseName, tableName));

            var rzlt = new List<IndexDbo>();
            foreach (string indexName in GetAllIndexes(table))
                rzlt.Add(GetIndexDescription(table, indexName));
            return rzlt;
        }

        public static IndexDbo GetIndexDescription(Table table, string indexName)
        {
            Index ind = table.Indexes[indexName];
            if (ind == null)
                throw new Exception(String.Format("Table '{0}' has no index '{1}'.", table.Name, indexName)); ;

            var dbo = new IndexDbo()
            {
                CompactLargeObjects = ind.CompactLargeObjects,
                DisallowPageLocks = ind.DisallowPageLocks,
                DisallowRowLocks = ind.DisallowRowLocks,
                FillFactor = ind.FillFactor,
                FilterDefinition = ind.FilterDefinition,
                IgnoreDuplicateKeys = ind.IgnoreDuplicateKeys,
                IndexKeyType = ind.IndexKeyType.ToString(),
                IsClustered = ind.IsClustered,
                IsDisabled = ind.IsDisabled,
                IsUnique = ind.IsUnique,
                Name = ind.Name,
            };
            foreach (IndexedColumn clmn in ind.IndexedColumns)
                dbo.IndexedColumns.Add(clmn.Name);

            return dbo;
        }

        public static IndexDbo GetIndexDescription(string tableName, string indexName)
        {
            var srv = GetConnectedServer(SqlServerName, UserName, Password);
            if (srv == null)
                throw new Exception(String.Format("Server '{0}' is not accessible.", SqlServerName));
            var db = srv.Databases[DatabaseName];
            if (db == null)
                throw new Exception(String.Format("Database '{0}' is not accessible.", DatabaseName)); ;
            Table table = db.Tables[tableName];
            if (table == null)
                throw new Exception(String.Format("Database '{0}' has no table '{1}'.", DatabaseName, tableName)); ;

            return GetIndexDescription(table, indexName);
        }

        public static List<string> GetTargetColumns(string tableName)
        {
            var srv = GetConnectedServer(SqlServerName, UserName, Password);
            if (srv == null)
                throw new Exception(String.Format("Server '{0}' is not accessible.", SqlServerName));
            var db = srv.Databases[DatabaseName];
            if (db == null)
                throw new Exception(String.Format("Database '{0}' is not accessible.", DatabaseName));
            Table table = db.Tables[tableName];
            if (table == null)
                throw new Exception(String.Format("Tabase '{0}' has no table '{1}'.", DatabaseName, tableName));

            var rzlt = new List<string>(GetPrimaryKeyColumns(table));
            rzlt.AddRange(GetColumnsInUniqueConstraints(table));

            return rzlt;
        }

        public static IEnumerable<DatabaseDbo> EnumerateDatabases(string serverName,  string loginName, string password,
                                                                  string namePattern, bool accessibleOnly)
        {
            Server server = GetConnectedServer(serverName, loginName, password);
            
            var rzlt = new List<DatabaseDbo>();
            if (server.Databases != null || server.Databases.Count > 0)
            {
                DatabaseDbo dbo;
                string dbsName;

                foreach (Database dbs in server.Databases)
                {
                    if (!dbs.IsAccessible || dbs.IsSystemObject)
                        continue;
                    if (dbs.Users.Contains(loginName))
                    {
                        dbsName = dbs.Name;
                        if (String.IsNullOrEmpty(namePattern) || dbsName.ToUpper().Contains(namePattern.ToUpper()))
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
                }
            }
            return rzlt;
        }
    }
}