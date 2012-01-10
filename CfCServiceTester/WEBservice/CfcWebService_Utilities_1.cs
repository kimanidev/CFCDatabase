using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using Microsoft.SqlServer.Management.Smo;
using System.Data;
using System.Text;

namespace CfCServiceTester.WEBservice
{
    /// <summary>
    /// Listed below methods are designed for service only.
    /// </summary>
    public partial class CfcWebService
    {
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
        /// Returns <code>true</code> if table contains no repeating values in defined set of columns
        /// </summary>
        /// <param name="aTable">Table to be tested, <see cref="Table"/></param>
        /// <param name="columnNames">Set of columns that needs to be verified</param>
        /// <returns><code>true</code> - all rows contains different value of the set.</returns>
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
    }
}