﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Configuration;
using System.Web.Configuration;
using System.Security.Cryptography;
using CfCServiceTester.SVC.DataObjects;
using CfCServiceTester.WEBservice;
using System.Text;
using CfCServiceTester.CustomControls;

namespace CfCServiceTester
{
    public partial class _Default : System.Web.UI.Page
    {
        private RsaParametersDbo rsaParam;
        public string RsaExponent
        {
            get 
            {
                return rsaParam.Exponent; 
            }
        }
        public string RsaModulus
        {
            get
            {
                return rsaParam.Modulus;
            }
        }

        protected string GetFirstPageControlId(string controlName)
        {
            var ctrl = this.StartPageContent.FindControl(controlName);
            return ctrl == null ? String.Empty : ctrl.ClientID;
        }
        protected string GetSecondPageControlId(string controlName)
        {
            var ctrl = this.BackupPageContent.FindControl(controlName);
            return ctrl == null ? String.Empty : ctrl.ClientID;
        }
        protected string GetThirdPageControlId(string controlName)
        {
            var ctrl = this.ModifyTablePageContent.FindControl(controlName);
            return ctrl == null ? String.Empty : ctrl.ClientID;
        }
        protected string GetEditColumnBoxControlId(string controlName)
        {
            var ctrlRoot = this.ModifyTablePageContent.FindControl("ColumnEditorBox2");
            if (ctrlRoot == null)
                return String.Empty;

            var ctrl = ((ColumnEditBox)ctrlRoot).FindControl(controlName);
            return ctrl == null ? String.Empty : ctrl.ClientID;
        }
        protected string GetFourthPageControlId(string controlName)
        {
            var ctrl = this.ModifyIndexesContent.FindControl(controlName);
            return ctrl == null ? String.Empty : ctrl.ClientID;
        }
        protected string GetIndexEditColumnBoxControlId(string controlName)
        {
            var ctrlRoot = this.ModifyIndexesContent.FindControl("IndexEditorBox4");
            if (ctrlRoot == null)
                return String.Empty;

            var ctrl = ((IndexEditorBox)ctrlRoot).FindControl(controlName);
            return ctrl == null ? String.Empty : ctrl.ClientID;
        }
        protected string GetFifthPageControlId(string controlName)
        {
            var ctrl = this.ModifyForeignKeysContent.FindControl(controlName);
            return ctrl == null ? String.Empty : ctrl.ClientID;
        }
        protected string GetForeignKeyEditBoxControlId(string controlName)
        {
            var ctrlRoot = this.ModifyForeignKeysContent.FindControl("ForeignKeyEditorBox6");
            if (ctrlRoot == null)
                return String.Empty;

            var ctrl = ((ForeignKeyEditor)ctrlRoot).FindControl(controlName);
            return ctrl == null ? String.Empty : ctrl.ClientID;
        }
        public static string IsUserConnected
        {
            get
            {
                string rzlt = CfcWebService.IsConnected.ToString().ToLower();
                return rzlt;
            }
        }

        public _Default() : base()
        {
            this.PreInit += new EventHandler(_Default_PreInit);
            this.PreRender += new System.EventHandler(_Default_PreRender);
        }

        protected void _Default_PreInit(object sender, EventArgs e)
        {
            rsaParam = CfcWebService.GetPublicKey();
        }
        protected void _Default_PreRender(object sender, EventArgs e)
        {
            Wizard1.StepNextButtonStyle.CssClass = "oculto";
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Expires = 0;
            Response.Cache.SetNoStore();
            Response.AddHeader("Pragma", "no-cache");
        }
        
        public string LocalServersOnly
        {
            get
            {
                bool rzlt;
                if (!Boolean.TryParse((string)WebConfigurationManager.AppSettings["LocalServersOnly"], out rzlt))
                    rzlt = false;
                return rzlt.ToString().ToLower();
            }
        }
        public string AccessibleDatabasesOnly
        {
            get
            {
                bool rzlt;
                if (!Boolean.TryParse((string)WebConfigurationManager.AppSettings["AccessibleOnly"], out rzlt))
                    rzlt = false;
                return rzlt.ToString().ToLower();
            }
        }

        protected void Wizard1_OnActiveStepChanged(Object sender, EventArgs e)
        {
            var wizard = (Wizard)sender;
            Func<List<string>, string> ulItems = delegate(List<string> options)
            {
                var sb = new StringBuilder();
                options.ForEach(x => { sb.AppendFormat("<li>{0}</li>", x); });
                return sb.ToString();
            };

            bool noSession = HttpContext.Current.Session == null || String.IsNullOrEmpty(HttpContext.Current.Session.SessionID);
            List<string> roles = noSession ? new List<string>() : CfcWebService.GetUsersRoles();
            if (roles == null || roles.Count < 1)
            {
                wizard.ActiveStepIndex = 0;
                this.StartPageContent.setDefaultLoginText();
            }
            else
            {
                switch (wizard.ActiveStepIndex)// == 0)
                {
                    case 0:
                        this.StartPageContent.setConnectedLoginText(roles);
                        break;
                    case 1:
                        this.BackupPageContent.SetDefaultFileName();
                        break;
                    case 2:
                        this.ModifyTablePageContent.SetupPage();
                        break;
                    case 3:
                        this.ModifyIndexesContent.SetupPage();
                        break;
                    case 4:
                        this.ModifyForeignKeysContent.SetupPage();
                        break;
                }
            }
        }
    }
}