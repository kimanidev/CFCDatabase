using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace CfCServiceTester.SVC.DataObjects
{
    /// <summary>
    /// Service public/private encryption follows article 
    /// <see cref="http://www.codeproject.com/KB/ajax/SecureAjaxAuthentication.aspx?display=Print"/>
    /// </summary>
    [DataContract(Namespace = "CfCServiceNS")]
    public class RsaParametersDbo
    {
        /// <summary>
        /// String representation of the <code>Exponent</code> parameter, <see cref="RSAParameters"/>
        /// </summary>
        [DataMember]
        public string Exponent { get; set; }

        /// <summary>
        /// String representation of the <code>Modulus</code> parameter, <see cref="RSAParameters"/>
        /// </summary>
        [DataMember]
        public string Modulus { get; set; }
    }
}