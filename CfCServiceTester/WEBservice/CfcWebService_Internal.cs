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
            try
            {
                DisconnectedHosts = GetConnectedHosts();
                SendNotification(String.Format("Access to the [{0}].[{1}] database is locked.", SqlServerName, dbName));    //DatabaseName

                var srv = new Server(SqlServerName);
                var db = srv.Databases[dbName];
                if (db != null)     // db == null for new databases; there is no need for switching to single mode in this case
                {
                    db.DatabaseOptions.UserAccess = DatabaseUserAccess.Single;
                    db.Alter(TerminationClause.RollbackTransactionsImmediately);
                }
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
            db.DatabaseOptions.UserAccess = DatabaseUserAccess.Multiple;
            db.Alter(TerminationClause.RollbackTransactionsImmediately);

            SendNotification(String.Format("Access to the [{0}].[{1}] database is free now.", SqlServerName, dbName));
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

        public static IEnumerable<DataColumnDbo> GetTableColumns(/*string serverName, string dbName, */string tableName)
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
            List<string> primaryKeyColumns = GetPrimaryKeyColumns(aTable);

            foreach (Column clmn in aTable.Columns)
            {
                yield return CreateDataColumnDbo(clmn, primaryKeyColumns);
            }
        }

        /// <summary>
        /// Inserts new column into the table
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="column">Column definition, <see cref="DataColumnDbo"/></param>
        /// <returns>Column description, <see cref="DataColumnDbo"/></returns>
        public static DataColumnDbo InsertColumn(string tableName, DataColumnDbo column)
        {
            Database db;
            Table aTable = GetTable(tableName, out db);

            Column newColumn = CreateColumn(aTable, column);
            if (column.IsPrimaryKey)
                InsertColumnIntoPrimarykey(aTable, newColumn);
            aTable.Alter();

            List<string> primaryKeyColumns = GetPrimaryKeyColumns(aTable);
            return CreateDataColumnDbo(newColumn, primaryKeyColumns);
        }

        /// <summary>
        /// Inserts new column into the table
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="oldColumnName">Old column name</param>
        /// <param name="newColumnName">New column name</param>
        /// <param name="alteredDependencies">List with altered procedures, functions, triggers and views.</param>
        /// <returns>Column description, <see cref="DataColumnDbo"/></returns>
        public static DataColumnDbo RenameColumn(string tableName, string oldColumnName, string newColumnName, 
                                                 out List<AlteredDependencyDbo> alteredDependencies)
        {
            Database db;
            Table aTable = GetTable(tableName, out db);

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

        private static void RenameDatabase(string newName)
        {
            string oldName = DatabaseName;
            DatabaseName = newName;
            string connString = ConnectionString;

            string regexTemplate = String.Format(@"\b{0}\b", oldName);        
            var rg = new Regex(regexTemplate, RegexOptions.Compiled | RegexOptions.IgnoreCase);

            ConnectionString = rg.Replace(connString, newName);
        }
    }
}