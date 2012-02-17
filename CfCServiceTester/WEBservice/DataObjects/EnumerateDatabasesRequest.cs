using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace CfCServiceTester.WEBservice.DataObjects
{
    [DataContract(Namespace = "CfCServiceNS")]
    public class DatabaseRequestBase
    {
        /// <summary>
        /// Server's name
        /// </summary>
        [DataMember]
        public string ServerName { get; set; }
        
        /// <summary>
        /// Encrypted login name
        /// </summary>
        [DataMember]
        public string LoginName { get; set; }
        
        /// <summary>
        /// Encrypted password
        /// </summary>
        [DataMember]
        public string Password { get; set; }
    }

    [DataContract(Namespace = "CfCServiceNS")]
    public class EnumerateDatabasesRequest: DatabaseRequestBase
    {
        /// <summary>
        /// Starting symbols in database name
        /// </summary>
        [DataMember]
        public string NamePattern { get; set; }
        
        /// <summary>
        /// <code>true</code> - return accessible databases only
        /// </summary>
        [DataMember]
        public bool AccessibleOnly { get; set; }
    }

    [DataContract(Namespace = "CfCServiceNS")]
    public class CreateDbConnectionRequest : DatabaseRequestBase
    {
        /// <summary>
        /// Database name
        /// </summary>
        [DataMember]
        public string InitialCatalog { get; set; }
    }
}