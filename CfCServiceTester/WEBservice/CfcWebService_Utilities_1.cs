using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using Microsoft.SqlServer.Management.Smo;
using System.Data;
using System.Text;
using CfCServiceTester.WEBservice.DataObjects;

namespace CfCServiceTester.WEBservice
{
    /// <summary>
    /// Listed below methods are designed for service only.
    /// </summary>
    public partial class CfcWebService
    {
        private delegate void ProcessColumnDelegate(Table aTable, DataColumnDbo currentColumn);

        /// <summary>
        /// Removes is null condition from the column
        /// </summary>
        /// <param name="aTable">Table, <see cref="Table"/></param>
        /// <param name="currentColumn">Column, <see cref="Column"/></param>
        private static void RemoveNullCondition(Table aTable, Column currentColumn)
        {
            const string query = "SELECT count(1) AS NullCounter FROM [{0}] WHERE [{1}] IS NULL";
            string sql = String.Format(query, aTable.Name, currentColumn.Name);
            int nullCounter;

            using (var connection = new SqlConnection(ConnectionString))
            {
                SqlCommand cmd = new SqlCommand(sql, connection);
                connection.Open();
                nullCounter = (Int32)cmd.ExecuteScalar();
            }
            if (nullCounter > 0)
                throw new Exception(String.Format("Table {0} contains NULLs in the {1} column.", aTable.Name, currentColumn.Name));

            currentColumn.Nullable = false;
            currentColumn.Alter();
        }

        /// <summary>
        /// Returns <code>true</code> if table contains no repeating values in defined set of columns. 
        /// </summary>
        /// <param name="aTable">Table to be tested, <see cref="Table"/></param>
        /// <param name="columnNames">Set of columns that needs to be verified</param>
        /// <returns>
        ///     <list type="bullet">
        ///         <item><code>true</code> - all rows contains different values of the set,</item>
        ///         <item><code>false</code> - no colums or set contains repeating values in the table</item>
        ///     </list>
        /// </returns>
        private static bool AreValuesUnique(Table aTable, params string[] columnNames)
        {
            const string query =
                "WITH tmp ({0}, [repo]) " +
                "AS " +
                "( " +
                    "SELECT {0}, COUNT(1) AS [repo] " +
                    "FROM [{1}] " +
                    "GROUP BY {0} " +
                ") " +
                "SELECT COUNT(*) AS [BadRowCounter] FROM tmp WHERE [repo] > 1";

            int badRowCounter;
            if (columnNames.Length < 1)
                return false;

            var sb = new StringBuilder();
            foreach (string name in columnNames)
            {
                if (sb.Length > 0)
                    sb.Append(", ");
                sb.AppendFormat("[{0}]", name);
            }
            string sql = String.Format(query, sb.ToString(), aTable.Name);

            using (var connection = new SqlConnection(ConnectionString))
            {
                SqlCommand cmd = new SqlCommand(sql, connection);
                connection.Open();
                badRowCounter = (Int32)cmd.ExecuteScalar();
            }
            
            return badRowCounter < 1;
        }

        private static string[] GetPrimaryKeyColumns(Table aTable, string excludeColumn, out Index primaryKey)
        {
            var rzlt = new List<string>();
            foreach (Index ind in aTable.Indexes)
            {
                if (ind.IndexKeyType == IndexKeyType.DriPrimaryKey)
                {
                    foreach (IndexedColumn clm in ind.IndexedColumns)
                    {
                        if (String.Compare(clm.Name, excludeColumn, true) != 0)
                            rzlt.Add(clm.Name);
                    }
                    primaryKey = ind;
                    return rzlt.ToArray();
                }
            }

            primaryKey = null;
            return rzlt.ToArray();
        }

        private static string[] GetIndexColumns(Index ind)
        {
            var rzlt = new List<string>();
            if (ind != null)
            {
                foreach (IndexedColumn clm in ind.IndexedColumns)
                    rzlt.Add(clm.Name);
            }
            return rzlt.ToArray();
        }

        private static List<DroppedDependencyDbo> RemoveColumnFromPrimarykey(
                                Table aTable, Column column, bool disableDependencies, Database db)
        {
            Index primaryKey;
            var rzlt = new List<DroppedDependencyDbo>();
            IndexKeyType oldIndexKeyType;
            bool oldIsClustered;
            string oldIndexName;

            string[] newColumns = GetPrimaryKeyColumns(aTable, column.Name, out primaryKey);
            if (disableDependencies)
                DropDependentForeignKeys(primaryKey.Name, db, rzlt);

            oldIndexName = primaryKey.Name;
            oldIndexKeyType = primaryKey.IndexKeyType;
            oldIsClustered = primaryKey.IsClustered;
            primaryKey.Drop();

            if (AreValuesUnique(aTable, newColumns))
                CreateNewPrimaryKey(aTable, oldIndexName, oldIndexKeyType, newColumns);
            return rzlt;
        }

        private static List<DroppedDependencyDbo> RestoreNullCondition(
                                Table aTable, Column column, bool disableDependencies, Database db)
        {
            var rzlt = new List<DroppedDependencyDbo>();
            IndexKeyType oldIndexKeyType;
            bool oldIsClustered;
            string oldIndexName;

            for (int i = aTable.Indexes.Count - 1; i >= 0; i--)
            {
                Index currentInd = aTable.Indexes[i];
                string[] indexColumns = GetIndexColumns(currentInd);
                if (indexColumns.Contains(column.Name))
                {
                    if (disableDependencies)
                        DropDependentForeignKeys(currentInd.Name, db, rzlt);

                    oldIndexName = currentInd.Name;
                    oldIndexKeyType = currentInd.IndexKeyType;
                    oldIsClustered = currentInd.IsClustered;
                    currentInd.Drop();

                    column.Nullable = true;
                    column.Alter();
                    CreateNewPrimaryKey(aTable, oldIndexName, oldIndexKeyType, indexColumns);
                    return rzlt;
                }
            }

            // No index contains column
            column.Nullable = true;
            column.Alter();
            return rzlt;
        }

        private static List<DroppedDependencyDbo> ModifyColumn(ProcessColumnDelegate processor,
                                    Table aTable, DataColumnDbo column, bool disableDependencies, Database db)
        {
            var rzlt = new List<DroppedDependencyDbo>();
            IndexKeyType oldIndexKeyType;
            bool oldIsClustered;
            string oldIndexName;

            for (int i = aTable.Indexes.Count - 1; i >= 0; i--)
            {
                Index currentInd = aTable.Indexes[i];
                string[] indexColumns = GetIndexColumns(currentInd);
                if (indexColumns.Contains(column.Name))
                {
                    if (disableDependencies)
                        DropDependentForeignKeys(currentInd.Name, db, rzlt);

                    oldIndexName = currentInd.Name;
                    oldIndexKeyType = currentInd.IndexKeyType;
                    oldIsClustered = currentInd.IsClustered;
                    currentInd.Drop();

                    processor(aTable, column);
                    CreateNewPrimaryKey(aTable, oldIndexName, oldIndexKeyType, indexColumns);
                    return rzlt;
                }
            }

            // No index contains column
            processor(aTable, column);
            return rzlt;
        }

        /// <summary>
        /// Removes Identity constraint
        /// </summary>
        /// <param name="aTable">Table, <see cref="Table"/></param>
        /// <param name="column">Column, <see cref="DataColumnDbo"/></param>
        /// <param name="disableDependencies"><code>true></code> - drop dependencies</param>
        /// <param name="db">Current database, <see cref="Database"/></param>
        public static List<DroppedDependencyDbo> RemoveOrRestoreIdentity(
                                    Table aTable, DataColumnDbo column, bool disableDependencies, Database db)
        {
            return ModifyColumn(new ProcessColumnDelegate(RecreateColumn), aTable, column, disableDependencies, db);
        }
        private static void RecreateColumn(Table aTable, DataColumnDbo column)
        {

            // Create new column
            bool oldIsNullable = column.IsNullable;
            string oldName = column.Name;
            column.IsNullable = true;
            column.Name = String.Format("{0}_{1}", oldName, CreateUniqueAppendix());
            Column newColumn = CreateColumn(aTable, column);
            aTable.Alter();
            
            // Copy data into new column
            string queryString = String.Format("UPDATE [{0}] SET [{1}] = [{2}]", aTable.Name, column.Name, oldName);
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                var command = new SqlCommand(queryString, connection);
                command.Connection.Open();
                command.ExecuteNonQuery();
            }

            // Drop old column and rename new one.
            aTable.Columns[oldName].Drop();
            aTable.Alter();
            Column clmn = aTable.Columns[column.Name];
            if (!oldIsNullable)
                clmn.Nullable = false;
            column.IsNullable = oldIsNullable;

            clmn.Rename(oldName);
            clmn.Alter();
            aTable.Alter();
            column.Name = oldName;
        }
        private static string CreateUniqueAppendix()
        {
            string tmp = Guid.NewGuid().ToString();
            tmp = tmp.Substring(1, tmp.Length - 2);
            return tmp.Replace("-", String.Empty);
        }

        private static void DeleteTable(Table aTable, bool disableDependencies, Database db, List<DroppedDependencyDbo> droppedForeignKeys)
        {
            if (disableDependencies)
            {
                DataTable foreignKeys = aTable.EnumForeignKeys();
                List<KeyValuePair<string, string>> lstForeignKeys = (
                    from fKey in foreignKeys.AsEnumerable()
                    select new KeyValuePair<string, string>(fKey.Field<string>("Name"), fKey.Field<string>("Table_Name"))
                    ).ToList();
                DropCorrentForeignKey(lstForeignKeys, db, droppedForeignKeys);
            }
            aTable.Drop();
        }

        /// <summary>
        /// Drops foreign keys, listed in lstForeignKeys
        /// </summary>
        /// <param name="lstForeignKeys">List with foreign keys</param>
        /// <param name="db">Current database</param>
        /// <param name="droppedForeignKeys">Result of the dropping</param>
        private static void DropCorrentForeignKey(List<KeyValuePair<string, string>> lstForeignKeys,
                                                   Database db, List<DroppedDependencyDbo> droppedForeignKeys)
        {
            foreach (KeyValuePair<string, string> pair in lstForeignKeys)
            {
                Table currentTable = db.Tables[pair.Value]; // Value - table name
                if (currentTable != null)
                {
                    ForeignKey fKey = currentTable.ForeignKeys[pair.Key];   // Key - name of the foreign key
                    if (fKey != null)
                    {
                        droppedForeignKeys.Add(new DroppedDependencyDbo()
                        {
                            Name = fKey.Name,
                            ObjectType = DbObjectType.foreignKey,
                            TableName = currentTable.Name,
                            Columns = GetColumnNames(fKey)
                        });
                        fKey.Drop();
                    }
                    currentTable.Alter();
                }
            }
        }
    }
}