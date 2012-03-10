using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CfCServiceTester.WEBservice.DataObjects;
using System.Data.SqlClient;

namespace CfCServiceTester.WEBservice
{
    public partial class CfcWebService
    {
        public static Guid LogTableOperation(string tableName, string message, short majorVersion, short MinorVersion)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                var cmd = new SqlCommand("CFC_DB_ChangesHistory", connection) { CommandType = System.Data.CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@DatabaseName", DatabaseName);
                cmd.Parameters.AddWithValue("@TableName", tableName);
                cmd.Parameters.AddWithValue("@MajorVersion", majorVersion);
                cmd.Parameters.AddWithValue("@MinorVersion", MinorVersion);
                cmd.Parameters.AddWithValue("@ChangeDescription", message);
                connection.Open();
                var rzlt = (Guid)cmd.ExecuteScalar();
                connection.Close();
                return rzlt;
            }
        }
    }
}