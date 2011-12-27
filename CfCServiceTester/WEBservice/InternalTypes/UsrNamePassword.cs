using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography;
using System.Text;

namespace CfCServiceTester.WEBservice.InternalTypes
{
    public class UsrNamePassword
    {
        /// <summary>
        /// User's name
        /// </summary>
        public string UserName { get; private set; }

        /// <summary>
        /// Password
        /// </summary>
        public string Password { get; private set; }

        public UsrNamePassword(string encUsername, string encPassword)
        {
            CspParameters cspParams = new CspParameters();
            cspParams.Flags = CspProviderFlags.UseMachineKeyStore;
            var myRSA = new RSACryptoServiceProvider(cspParams);

            if (HttpContext.Current.Session == null || HttpContext.Current.Session[CfcWebService.CertificateKey] == null)
                this.UserName = this.Password = null;
            else
            {
                myRSA.FromXmlString((string)HttpContext.Current.Session[CfcWebService.CertificateKey]);
                this.UserName = Encoding.UTF8.GetString(myRSA.Decrypt(CfcWebService.ToHexByte(encUsername), false));
                this.Password = Encoding.UTF8.GetString(myRSA.Decrypt(CfcWebService.ToHexByte(encPassword), false));
            }
        }
    }
}