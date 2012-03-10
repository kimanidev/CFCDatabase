using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;

namespace CfCServiceTester.CustomControls
{
    public partial class StartPageContent : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            this.spnConnectionError.Style[HtmlTextWriterStyle.Display] = "none";
        }

        public void setDefaultLoginText()
        {
            this.spnConnectionError.InnerText = "Login to database, please.";
            this.spnConnectionError.Style[HtmlTextWriterStyle.Display] = "block";
        }

        public void setConnectedLoginText(List<string> roles)
        {
            Func<List<string>, string> ulItems = delegate(List<string> options)
            {
                var sb = new StringBuilder();
                options.ForEach(x => { sb.AppendFormat("<li>{0}</li>", x); });
                return sb.ToString();
            };

            this.UserRolesList.InnerHtml = ulItems(roles);
            this.LoginTooltip.InnerText = "Enter credentials and connect to database again if you want another roles.";
            this.spnConnectionOK.Style[HtmlTextWriterStyle.Display] = "block";
        }

    }
}