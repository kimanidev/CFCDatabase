using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CfCServiceTester.WEBservice;
using CfCServiceTester.WEBservice.DataObjects;
using System.Data.SqlClient;
using System.Data;
using System.Threading;

namespace CfCServiceTester.CustomControls
{
    public partial class ModifyTableContent : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        public void SetupPage()
        {
            this.txtServerName2.Text = CfcWebService.SqlServerName;
            this.txtDatabaseName2.Text = CfcWebService.DatabaseName;
            CfcDbChangesDbo dbo = CfcWebService.GetFirstCfcDbChanges();
            txtMajorDbVersion2.Text = Math.Max((short)1, dbo.CFC_DB_Major_Version).ToString();
            txtMinorDbVersion2.Text = dbo.CFC_DB_Minor_Version.ToString();
            txtTable2.Text = dbo.Table_Name;
        }
/*
        private CfcDbChangesDbo GetFirstCfcDbChanges(int counter = 3)
        {
            const string queryString = "EXEC GetFirst_CFC_DB_Changes";
            CfcDbChangesDbo rzlt = null;

            try
            {
                //var options = new TransactionOptions()
                //{
                //    IsolationLevel = System.Transactions.IsolationLevel.Serializable,
                //    Timeout = new TimeSpan(0, TransactionTimeout, 0)
                //};
                //using (var trScope = new TransactionScope(TransactionScopeOption.Required, options))
                //{
                using (var connection = new SqlConnection(CfcWebService.ConnectionString))
                {
                    connection.Open();
                    var da = new SqlDataAdapter(queryString, connection);
                    da.TableMappings.Add("Table", "CFC_DB_Changes");

                    var ds = new DataSet();
                    da.Fill(ds);
                    DataTable changeList = ds.Tables["CFC_DB_Changes"];
                    rzlt = (
                        from chng in changeList.AsEnumerable()
                        select new CfcDbChangesDbo()
                        {
                            DB_Change_GUID = chng.Field<Guid>("DB_Change_GUID"),
                            CFC_DB_Name = chng.Field<string>("CFC_DB_Name"),
                            CFC_DB_Major_Version = chng.Field<short>("CFC_DB_Major_Version"),
                            CFC_DB_Minor_Version = chng.Field<short>("CFC_DB_Minor_Version"),
                            Seq_No = chng.Field<int>("Seq_No"),
                            Table_Name = chng.Field<string>("Table_Name"),
                            Change_Description = chng.Field<string>("Change_Description"),
                            Created_By = chng.Field<string>("Created_By"),
                            Created_Date = chng.Field<DateTime>("Created_Date"),
                            Last_Update_By = chng.Field<string>("Last_Update_By"),
                            Last_Update = chng.Field<DateTime>("Last_Update"),
                        }).FirstOrDefault();
                    connection.Close();
                }
                //    trScope.Complete();
                //}
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                if (counter-- > 0)
                {
                    Thread.Sleep(100);
                    return GetFirstCfcDbChanges(counter);
                }
            }
            return rzlt ?? new CfcDbChangesDbo()
            {
                DB_Change_GUID = Guid.NewGuid(),
                CFC_DB_Name = CfcWebService.DatabaseName,
                CFC_DB_Major_Version = 1,
                CFC_DB_Minor_Version = 0,
                Seq_No = 0,
                Table_Name = String.Empty,
                Change_Description = String.Empty,
                Created_By = CfcWebService.UserName,
                Created_Date = DateTime.Now,
                Last_Update_By = CfcWebService.UserName,
                Last_Update = DateTime.Now,
            };
        }
 */ 
    }
}