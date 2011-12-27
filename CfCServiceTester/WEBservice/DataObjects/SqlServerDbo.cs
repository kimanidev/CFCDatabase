using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace CfCServiceTester.SVC.DataObjects
{
    /// <summary>
    /// <see cref="http://msdn.microsoft.com/en-us/library/ms210350.aspx"/>
    /// </summary>
    [DataContract(Namespace = "CfCServiceNS")]
    public class SqlServerDbo
    {
        /// <summary>
        /// The name of the instance of SQL Server.
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// The name of the server on which the instance of SQL Server is installed.
        /// </summary>
        [DataMember]
        public string Server { get; set; }

        /// <summary>
        /// The instance of SQL Server.
        /// </summary>
        [DataMember]
        public string Instance { get; set; }

        /// <summary>
        /// A Boolean value that is True if the instance is participating in failover clustering, or False if it is not.
        /// </summary>
        [DataMember]
        public bool IsClustered { get; set; }

        /// <summary>
        /// The version of the instance of SQL Server.
        /// </summary>
        [DataMember]
        public string Version { get; set; }

        /// <summary>
        /// A Boolean value that is True if the instance is local, or False if the instance is remote.
        /// </summary>
        [DataMember]
        public bool IsLocal { get; set; }
    }
}