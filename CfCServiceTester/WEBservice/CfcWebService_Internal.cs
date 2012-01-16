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
        /// </summary>
        public static bool SetSingleMode(string dbName)
        {
            var srv = new Server(SqlServerName);
            var db = srv.Databases[dbName];
            // db == null for new databases; there is no need for switching to single mode in this case
            return db == null ? true : SetSingleMode(db);
        }
        private static bool SetSingleMode(Database db)
        {
            try
            {
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
            var srv = new Server(SqlServerName);
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
            var srv = new Server(SqlServerName);
            var db = srv.Databases[DatabaseName];
            Table aTable = db.Tables[newName];
            if (aTable != null)
                throw new Exception(String.Format("Table {0} exists in the {1} database.", newName, DatabaseName));
            aTable = db.Tables[oldName];
            if (aTable == null)
                throw new Exception(String.Format("There isn no table {0} in the {1} database.", newName, DatabaseName));

            aTable.Rename(newName);
            return CorrectStoredProcedure(db, oldName, newName);
        }

        /// <summary>
        /// Renames index or primary key.
        /// <param name="oldName">Old name</param>
        /// <param name="newName">New name</param>
        /// </summary>
        public static IndexDbo RenameTheIndex(string tableName, string oldName, string newName)
        {
            var srv = new Server(SqlServerName);
            var db = srv.Databases[DatabaseName];
            Table aTable = db.Tables[tableName];
            if (aTable == null)
                throw new Exception(String.Format("There is no table {0} in the {1} database.", newName, DatabaseName));

            Index ind = aTable.Indexes[oldName];
            if (ind == null)
                throw new Exception(String.Format("There is no index {0} in the {1} table.", oldName, tableName));

            ind.Rename(newName);
            ind.Alter();
            return GetIndexDescription(aTable, newName); ;
        }

        public static IEnumerable<DataColumnDbo> GetTableColumns(string tableName, bool createNewTable = false)
        {
            var srv = new Server(SqlServerName);
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
           var srv = new Server(SqlServerName);
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
            var srv = new Server(SqlServerName);
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
            var srv = new Server(SqlServerName);
            if (srv == null)
                throw new Exception(String.Format("Server '{0}' is not accessible.", SqlServerName));
            var db = srv.Databases[DatabaseName];
            if (db == null)
                throw new Exception(String.Format("Database '{0}' is not accessible.", DatabaseName));
            Table table = db.Tables[tableName];
            if (table == null)
                throw new Exception(String.Format("Tabase '{0}' has no table '{1}'.", DatabaseName, tableName));

            return GetAllIndexes(table);
        }

        private static List<string> GetAllIndexes(Table table)
        {
            var rzlt = new List<string>();
            foreach (Index currentIndex in table.Indexes)
                rzlt.Add(currentIndex.Name);
            return rzlt;
        }

        public static List<IndexDbo> GetTableIndexes(string tableName)
        {
            var srv = new Server(SqlServerName);
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
            var srv = new Server(SqlServerName);
            if (srv == null)
                throw new Exception(String.Format("Server '{0}' is not accessible.", SqlServerName));
            var db = srv.Databases[DatabaseName];
            if (db == null)
                throw new Exception(String.Format("Database '{0}' is not accessible.", DatabaseName)); ;
            Table table = db.Tables[tableName];
            if (table == null)
                throw new Exception(String.Format("Dabase '{0}' has no table '{1}'.", DatabaseName, tableName)); ;

            return GetIndexDescription(table, indexName);
        }
    }
}