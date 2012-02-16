using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CfCServiceTester.WEBservice;
using System.Web.Configuration;

namespace CfCServiceTester.CustomControls
{
    public partial class DatabaseBackupContent : System.Web.UI.UserControl
    {
        private bool emptyFileTemplate;

        protected void Page_Load(object sender, EventArgs e)
        {
            this.txtBackupDirectory.Text = (string)WebConfigurationManager.AppSettings["BackupDirectory"];
            this.hdnKillUserProcedure.Value = (string)WebConfigurationManager.AppSettings["KillUserProcedure"];
            var fileTemplate = (string)WebConfigurationManager.AppSettings["BackupFileTemplate"];
            this.emptyFileTemplate = !String.IsNullOrEmpty(fileTemplate) && String.Compare(fileTemplate, "None", true) == 0;
        }

        public void SetDefaultFileName()
        {
            if (!this.emptyFileTemplate && String.IsNullOrEmpty(this.txtBackupFileName.Text))
            {
                this.txtBackupFileName.Text = String.Format("{0}_{1}_{2}.bak",
                        CfcWebService.SqlServerName, CfcWebService.DatabaseName, DateTime.Now.ToString("yyyyMMdd_HHmm"));
            }
            if (String.IsNullOrEmpty(this.txtServerName1.Text))
            {
                this.txtServerName1.Text = CfcWebService.SqlServerName;
                this.txtCurrentDatabaseName1.Text = CfcWebService.DatabaseName;
            }
        }
    }
}