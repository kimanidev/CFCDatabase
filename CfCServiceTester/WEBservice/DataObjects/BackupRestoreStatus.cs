using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace CfCServiceTester.WEBservice.DataObjects
{
    [DataContract(Namespace = "CfCServiceNS")]
    public class BackupStatus : RestoreStatus
    {
        /// <summary>
        /// File size (<code>)0L</code> in case of error)
        /// </summary>
        [DataMember]
        public long FileSize { get; set; }
    }

    [DataContract(Namespace = "CfCServiceNS")]
    public class RestoreStatus
    {
        /// <summary>
        /// <code>true</code> - backup was created
        /// </summary>
        [DataMember]
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Error message (empty for succesful operation).
        /// </summary>
        [DataMember]
        public string ErrorMessage { get; set; }
    }

    [DataContract(Namespace = "CfCServiceNS")]
    public class RenameTableStatus : RestoreStatus
    {
        /// <summary>
        /// List of database objects that were modified (views, procedures, functions, triggers)
        /// </summary>
        [DataMember]
        public IList<AlteredDependencyDbo> AlteredDependencies { get; set; }

        public RenameTableStatus()
        {
            this.AlteredDependencies = new List<AlteredDependencyDbo>();
        }
    }

}