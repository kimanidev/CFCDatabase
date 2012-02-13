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
    public partial class ModifyIndexesContent : System.Web.UI.UserControl
    {
        private List<string> tableList;
        private List<string> indexList;
        private string currentDatabase;
        private string currentTable;
        private string currentIndex;
        private IndexDbo currentIndexDbo;

        protected void Page_Load(object sender, EventArgs e)
        {

        }
        public void SetupPage()
        {
            this.txtServerName4.Text = CfcWebService.SqlServerName;
            this.txtDatabaseName4.Text = currentDatabase = CfcWebService.DatabaseName;
            this.lstTableList4.DataSource = tableList = CfcWebService.GetAllTables();
            this.lblErrorMessage4.Text = String.Empty;
            this.lstTableList4.DataBind();
            CfcDbChangesDbo dbo = CfcWebService.GetFirstCfcDbChanges();
            txtMajorDbVersion4.Text = Math.Max((short)1, dbo.CFC_DB_Major_Version).ToString();
            txtMinorDbVersion4.Text = dbo.CFC_DB_Minor_Version.ToString();
        }

        protected void TableList4_OnDataBound(Object sender, EventArgs e)
        {
            Action<int> setDefaultItem = delegate(int i)
            {
                this.lstTableList4.SelectedIndex = i;
                this.hdnSelectedTable4.Value = this.currentTable = tableList[i];
            };

            if (this.lstTableList4.Items.Count < 1)
                this.lblErrorMessage4.Text = String.Format("Database '{0}' has no tables.", currentDatabase);
            else
            {
                if (!String.IsNullOrEmpty(this.hdnSelectedTable4.Value))
                {
                    if (tableList.Contains(this.hdnSelectedTable4.Value))
                        this.lstTableList4.SelectedValue = this.currentTable = this.hdnSelectedTable4.Value;
                    else
                        setDefaultItem(0);
                }
                else
                    setDefaultItem(0);
                ShowIndexes();
            }
        }
        private void ShowIndexes()
        {
            lstIndexList4.DataSource = indexList = CfcWebService.GetAllIndexes(this.currentTable);
            lstIndexList4.DataBind();
        }

        protected void lstIndexList4_OnDataBound(Object sender, EventArgs e)
        {
            Action<int> setDefaultItem = delegate(int i)
            {
                this.lstIndexList4.SelectedIndex = i;
                this.hdnSelectedIndex4.Value = this.currentIndex = indexList[i];
            };
            if (lstIndexList4.Items.Count > 0)
            {
                if (!String.IsNullOrEmpty(this.hdnSelectedIndex4.Value))
                {
                    if (indexList.Contains(this.hdnSelectedIndex4.Value))
                        this.lstIndexList4.SelectedValue = this.currentIndex = this.hdnSelectedIndex4.Value;
                    else
                        setDefaultItem(0);
                }
                else
                    setDefaultItem(0);
                ShowFields(this.currentIndex);
            }
        }
        private void ShowFields(string indexName)
        {
            this.currentIndexDbo = CfcWebService.GetIndexDescription(this.currentTable, this.currentIndex);
            this.lstFieldList4.DataSource = this.currentIndexDbo.IndexedColumns;
            this.lstFieldList4.DataBind();

            this.chkCompactLargeObjects4.Checked = currentIndexDbo.CompactLargeObjects; // 1
            this.chkDisallowPageLocks4.Checked = currentIndexDbo.DisallowPageLocks;     // 2
            this.chkDisallowRowLocks4.Checked = currentIndexDbo.DisallowRowLocks;       // 3
            this.txtFillFactor4.Text = currentIndexDbo.FillFactor.ToString();           // 4
            this.txtFilterDefinition4.Text = currentIndexDbo.FilterDefinition;          // 5
            this.chkIgnoreDuplicateKeys4.Checked = currentIndexDbo.IgnoreDuplicateKeys; // 6
            
            string tmp = currentIndexDbo.IndexKeyType.ToString();
            int i = ddlIndexKeyType4.Items.Count - 1;
            while (i > 0)
            {
                if (String.Compare(ddlIndexKeyType4.Items[i].Value, tmp, true) == 0)
                    break;
                else
                    i--;
            }
            ddlIndexKeyType4.SelectedIndex = i;                                         // 7

            this.chkIsClustered4.Checked = currentIndexDbo.IsClustered;                 // 8
            this.chkIsDisabled4.Checked = currentIndexDbo.IsDisabled;                   // 9
            this.chkIsUnique4.Checked = currentIndexDbo.IsUnique;                       // 10
        }
    }
}