using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CfCServiceTester.WEBservice;

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
        }
    }
}