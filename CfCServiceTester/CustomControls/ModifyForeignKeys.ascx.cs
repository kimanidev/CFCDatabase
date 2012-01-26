using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CfCServiceTester.WEBservice;

namespace CfCServiceTester.CustomControls
{
    public partial class ModifyForeignKeys : System.Web.UI.UserControl
    {
        private string currentDatabase;
        private string currentTable;
        private string currentForeignKey;
        private List<string> tableList;
        private List<string> fKeyList;

        protected void Page_Load(object sender, EventArgs e)
        {

        }
        public void SetupPage()
        {
            this.txtServerName6.Text = CfcWebService.SqlServerName;
            this.txtDatabaseName6.Text = currentDatabase = CfcWebService.DatabaseName;
            this.lstTableList6.DataSource = tableList = CfcWebService.GetAllTables();
            this.lblErrorMessage6.Text = String.Empty;
            this.lstTableList6.DataBind();
        }

        protected void TableList6_OnDataBound(Object sender, EventArgs e)
        {
            Action<int> setDefaultItem = delegate(int i)
            {
                this.lstTableList6.SelectedIndex = i;
                this.hdnSelectedTable6.Value = this.currentTable = tableList[i];
            };

            if (this.lstTableList6.Items.Count < 1)
                this.lblErrorMessage6.Text = String.Format("Database '{0}' has no tables.", currentDatabase);
            else
            {
                if (!String.IsNullOrEmpty(this.hdnSelectedTable6.Value))
                {
                    if (tableList.Contains(this.hdnSelectedTable6.Value))
                        this.lstTableList6.SelectedValue = this.currentTable = this.hdnSelectedTable6.Value;
                    else
                        setDefaultItem(0);
                }
                else
                    setDefaultItem(0);
                ShowForeignKeys();
            }
        }
        private void ShowForeignKeys()
        {
            lstForeignKeyList6.DataSource = fKeyList = CfcWebService.GetForeignKeys(this.currentTable);
            lstForeignKeyList6.DataBind();
        }

        protected void lstIndexList6_OnDataBound(Object sender, EventArgs e)
        {
            Action<int> setDefaultItem = delegate(int i)
            {
                this.lstForeignKeyList6.SelectedIndex = i;
                this.hdnSelectedForeignKey6.Value = this.currentForeignKey = fKeyList[i];
            };
            if (lstForeignKeyList6.Items.Count > 0)
            {
                if (!String.IsNullOrEmpty(this.hdnSelectedForeignKey6.Value))
                {
                    if (fKeyList.Contains(this.hdnSelectedForeignKey6.Value))
                        this.lstForeignKeyList6.SelectedValue = this.currentForeignKey = this.hdnSelectedForeignKey6.Value;
                    else
                        setDefaultItem(0);
                }
                else
                    setDefaultItem(0);
                ShowFields(this.currentForeignKey);
            }
        }

        /// <summary>
        /// <see cref="http://msdn.microsoft.com/en-us/library/ms219599%28v=sql.90%29.aspx"/>
        /// <seealso cref="http://msdn.microsoft.com/en-us/library/microsoft.sqlserver.management.smo.foreignkey.aspx"/>
        /// <seealso cref="http://msdn.microsoft.com/en-us/library/ms162566.aspx"/>
        /// </summary>
        /// <param name="fKeyName"></param>
        protected void ShowFields(string fKeyName)
        {
            foreach (KeyValuePair<string, string> pair in CfcWebService.GetForeignKeyColumns(this.currentTable, this.currentForeignKey))
            {
                lstSourceColumnList6.Items.Add(new ListItem(pair.Key, pair.Key));
                lstTargetColumnList6.Items.Add(new ListItem(pair.Value, pair.Value));
            }
        }
    }
}