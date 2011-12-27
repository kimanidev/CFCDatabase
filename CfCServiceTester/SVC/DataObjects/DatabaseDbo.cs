using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace CfCServiceTester.SVC.DataObjects
{
    /// <summary>
    /// <see cref="http://msdn.microsoft.com/en-us/library/microsoft.sqlserver.management.smo.database.aspx"/>
    /// Only scalar properties are downloaded to JavaScript.
    /// </summary>
    [DataContract(Namespace = "CfCServiceNS")]
    public class DatabaseDbo
    {
        /// <summary>
        /// The name of the instance of SQL Server.
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets the database ID value that uniquely identifies the database.
        /// </summary>
        [DataMember]
        public int ID { get; set; }

        /// <summary>
        /// Gets a Boolean property value that specifies whether the database can be accessed.
        /// </summary>
        [DataMember]
        public bool IsAccessible { get; set; }

        /// <summary>
        /// Gets the size of the database in MB.
        /// </summary>
        [DataMember]
        public double Size { get; set; }
    }
}