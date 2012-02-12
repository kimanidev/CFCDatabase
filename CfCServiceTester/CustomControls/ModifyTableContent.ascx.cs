using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CfCServiceTester.WEBservice;
using CfCServiceTester.WEBservice.DataObjects;

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
    }
}