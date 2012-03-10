using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Configuration.Install;
using System.Configuration;
using System.Xml.Linq;
using System.Xml.XPath;
using System.IO;

namespace CustomWindow
{
    public partial class Form1 : Form
    {
        private InstallContext context;
        private string sConfigFileName;

        public Form1(InstallContext context)
        {
            this.context = context;
            InitializeComponent();
        }

        public Form1()
        {
            InitializeComponent();
        }

        private string GetConfigFileName(System.Reflection.Assembly ExecutingAssembly)
        {
            string sPath = GetWebRootFolder(ExecutingAssembly);
            sPath = sPath + @"\web.config";
            return sPath;
        }
        public static string GetWebRootFolder(System.Reflection.Assembly ExecutableAssembly)
        {
            string sPath = ExecutableAssembly.Location;
            sPath = sPath.Remove(sPath.LastIndexOf(@"\"), sPath.Length - sPath.LastIndexOf(@"\"));
            if ((sPath.ToUpper().EndsWith(@"\BIN")))//15/6/2006
                sPath = sPath.Remove(sPath.LastIndexOf(@"\"), sPath.Length - sPath.LastIndexOf(@"\"));
            
            return sPath;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
            this.sConfigFileName = GetConfigFileName(asm);
            LoadWebConfig(this.sConfigFileName);
        }

        private void LoadWebConfig(string fileName)
        {
            using (StreamReader sr = new StreamReader(fileName))
            {
                XDocument document = XDocument.Load(sr);
                var found = document.XPathEvaluate("/configuration/appSettings/add") as IEnumerable<object>;
                foreach (var obj in found)
                {
                    var element = (XElement)obj;
                    XAttribute keyName = element.Attribute("key");
                    XAttribute keyValue = element.Attribute("value");
                    switch (keyName.Value)
                    {
                        case "LocalServersOnly":
                            this.checkBoxLocalServersOnly.Checked = String.Compare(keyValue.Value, "true", true) == 0;
                            break;
                        case "AccessibleOnly":
                            this.checkBoxAccessibleOnly.Checked = String.Compare(keyValue.Value, "true", true) == 0;
                            break;
                        case "BackupDirectory":
                            this.textBoxBackupDirectory.Text = keyValue.Value;
                            break;
                    }
                }

            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            XDocument document;
            using (StreamReader sr = new StreamReader(this.sConfigFileName))
            {
                document = XDocument.Load(sr, LoadOptions.PreserveWhitespace);
                var found = document.XPathEvaluate("/configuration/appSettings/add") as IEnumerable<object>;
                foreach (var obj in found)
                {
                    var element = (XElement)obj;
                    XAttribute keyName = element.Attribute("key");
                    XAttribute keyValue = element.Attribute("value");
                    switch (keyName.Value)
                    {
                        case "LocalServersOnly":
                            keyValue.Value = this.checkBoxLocalServersOnly.Checked.ToString().ToLower();
                            break;
                        case "AccessibleOnly":
                            keyValue.Value = this.checkBoxAccessibleOnly.Checked.ToString().ToLower();
                            break;
                        case "BackupDirectory":
                            keyValue.Value = this.textBoxBackupDirectory.Text;
                            break;
                    }
                }
            }

            using (FileStream fileStream = new FileStream(this.sConfigFileName, FileMode.Create))
            using (StreamWriter sw = new StreamWriter(fileStream, Encoding.UTF8))
            {
                document.Save(sw, SaveOptions.DisableFormatting);
            }

            this.Close();
        }
    }
}
