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
    public partial class ModifyForeignKeys : System.Web.UI.UserControl
    {
        private string currentDatabase;
        private string currentTable;
        private string currentForeignKey;
        private List<string> tableList;

        private List<ForeignKeyDbo> foreignKeys;
//        private List<string> fKeyList;

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

                this.SourceFieldLabel.InnerText = String.Format("Fields ({0})", this.currentTable);
                ShowForeignKeys();
            }
        }
        private void ShowForeignKeys()
        {
            this.foreignKeys = CfcWebService.GetForeignKeys(this.currentTable);
            var fKeyNames = new List<string>();
            foreach (ForeignKeyDbo dbo in foreignKeys)
                fKeyNames.Add(dbo.Name);
            lstForeignKeyList6.DataSource = fKeyNames;
            lstForeignKeyList6.DataBind();
        }

        protected void lstIndexList6_OnDataBound(Object sender, EventArgs e)
        {
            int selectedIndex = 0;
            Func<string, ForeignKeyDbo> currentKey = delegate(string fKeyName)
            {
                selectedIndex = this.foreignKeys.Count - 1;
                while(selectedIndex > 0 && this.foreignKeys[selectedIndex].Name != fKeyName) 
                    selectedIndex--;
                return this.foreignKeys[selectedIndex];
            };

            if (this.foreignKeys.Count > 0)
            {
                ForeignKeyDbo selectedItem = null;
                if (!String.IsNullOrEmpty(this.hdnSelectedForeignKey6.Value))
                {
                    selectedItem = currentKey(this.hdnSelectedForeignKey6.Value);
                    this.lstForeignKeyList6.SelectedIndex = selectedIndex;
                    this.hdnSelectedForeignKey6.Value = this.currentForeignKey = selectedItem.Name;
                }
                else
                {
                    selectedItem = this.foreignKeys[0];
                    this.lstForeignKeyList6.SelectedIndex = 0;
                    this.hdnSelectedForeignKey6.Value = this.currentForeignKey = selectedItem.Name;
                }
                ShowFields(selectedItem);
            }
            else
            {
                this.hdnSelectedForeignKey6.Value = this.currentForeignKey = String.Empty;
            }
            if (String.IsNullOrEmpty(this.currentForeignKey))
            {
                this.btnRenameFkey6.Attributes["disabled"] = "disabled";
                this.btnModifyFkey6.Attributes["disabled"] = "disabled";
                this.btnDeleteFkey6.Attributes["disabled"] = "disabled";
            }
        }

        /// <summary>
        /// <see cref="http://msdn.microsoft.com/en-us/library/ms219599%28v=sql.90%29.aspx"/>
        /// <seealso cref="http://msdn.microsoft.com/en-us/library/microsoft.sqlserver.management.smo.foreignkey.aspx"/>
        /// <seealso cref="http://msdn.microsoft.com/en-us/library/ms162566.aspx"/>
        /// </summary>
        /// <param name="fKeyName"></param>
        protected void ShowFields(ForeignKeyDbo selectedItem)
        {
            foreach (ForeignKeyColumnDbo dbo in selectedItem.Columns)
            {
                lstSourceColumnList6.Items.Add(new ListItem(dbo.Name, dbo.Name));
                lstTargetColumnList6.Items.Add(new ListItem(dbo.ReferencedColumn, dbo.ReferencedColumn));
            }
            this.TargetFieldLabel6.InnerText = String.Format("Fields ({0})", selectedItem.ReferencedTable);
        }
    }
}