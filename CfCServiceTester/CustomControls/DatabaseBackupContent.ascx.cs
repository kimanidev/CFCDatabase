using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CfCServiceTester.WEBservice;

namespace CfCServiceTester.CustomControls
{
    public partial class DatabaseBackupContent : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        public void SetDefaultFileName()
        {
            if (String.IsNullOrEmpty(this.txtBackupFileName.Text))
            {
                this.txtBackupFileName.Text = String.Format("{0}_{1}_{2}.bak",
                        CfcWebService.SqlServerName, CfcWebService.DatabaseName, DateTime.Now.ToString("yyyyMMdd_HHmm"));
            }
            if (String.IsNullOrEmpty(this.txtServerName1.Text))
            {
                this.txtServerName1.Text = CfcWebService.SqlServerName;
            }
        }
    }
}