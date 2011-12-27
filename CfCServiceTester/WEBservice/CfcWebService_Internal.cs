using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CfCServiceTester.SVC.DataObjects;
using System.Security.Cryptography;

namespace CfCServiceTester.WEBservice
{
    /// <summary>
    /// Utilities that are not callable from outside world. These method may be used from server side only.
    /// </summary>
    public partial class CfcWebService
    {
        /// <summary>
        /// The application generates public/private key individual for every session and stores it
        /// in <code>Session[CertificateKey]</code>
        /// </summary>
        public static readonly string CertificateKey = "{4908DA1F-CC08-4816-8971-1166CD3B03DB}";
        public static string MyRSA
        {
            get { return (string)HttpContext.Current.Session[CertificateKey]; }
            set { HttpContext.Current.Session[CertificateKey] = value; }
        }

        /// <summary>
        /// Creates public/private RSA keys, individual pair for every session.
        /// </summary>
        /// <returns></returns>
        public static RsaParametersDbo GetPublicKey()
        {
            CspParameters cspParams = new CspParameters();
            cspParams.Flags = CspProviderFlags.UseMachineKeyStore;
            var myRSA = new RSACryptoServiceProvider(cspParams);
            if ( String.IsNullOrEmpty(MyRSA))
                MyRSA = myRSA.ToXmlString(true);

            myRSA.FromXmlString(MyRSA);

            RSAParameters param = myRSA.ExportParameters(false);
            var rzlt = new RsaParametersDbo()
            {
                Exponent = ToHexString(param.Exponent),
                Modulus = ToHexString(param.Modulus)
            };
            return rzlt;
        }

        /// <summary>
        /// Converts byte array to string
        /// </summary>
        /// <param name="byteValue">Byte array</param>
        /// <returns>Converted string</returns>
        public static string ToHexString(byte[] byteValue)
        {
            char[] lookup = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
            int i = 0, p = 0, l = byteValue.Length;
            char[] c = new char[l * 2];
            while (i < l)
            {
                byte d = byteValue[i++];
                c[p++] = lookup[d / 0x10];
                c[p++] = lookup[d % 0x10];
            }
            return new string(c, 0, c.Length);
        }
        /// <summary>
        /// Restores byte array from string.
        /// </summary>
        /// <param name="str">String with hexadecimal digits</param>
        /// <returns>Compressed byte array</returns>
        public static byte[] ToHexByte(string str)
        {
            byte[] b = new byte[str.Length / 2];
            for (int y = 0, x = 0; x < str.Length; ++y, x++)
            {
                byte c1 = (byte)str[x];
                if (c1 > 0x60) c1 -= 0x57;
                else if (c1 > 0x40) c1 -= 0x37;
                else c1 -= 0x30;
                byte c2 = (byte)str[++x];
                if (c2 > 0x60) c2 -= 0x57;
                else if (c2 > 0x40) c2 -= 0x37;
                else c2 -= 0x30;
                b[y] = (byte)((c1 << 4) + c2);
            }
            return b;
        }
    }
}