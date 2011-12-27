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

        public _Default() : base()
        {
            this.PreInit += new EventHandler(_Default_PreInit);
        }

        protected void _Default_PreInit(object sender, EventArgs e)
        {
                rsaParam = CfcWebService.GetPublicKey();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
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

            List<string> roles = CfcWebService.GetUsersRoles();
            if (roles == null || roles.Count < 1)
            {
                wizard.ActiveStepIndex = 0;
                this.StartPageContent.setDefaultLoginText();
            }
            else
            {
                if (wizard.ActiveStepIndex == 0)
                {
                    this.StartPageContent.setConnectedLoginText(roles);
                }
                else if (wizard.ActiveStepIndex == 1)
                {
                    this.BackupPageContent.SetDefaultFileName();
                }
            }
        }
    }
}