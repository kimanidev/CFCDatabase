using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace CfCServiceTester.WEBservice.DataObjects
{
    /// <summary>
    /// Contains list with names of backup files.
    /// </summary>
    [DataContract(Namespace = "CfCServiceNS")]
    public class EnumerateBackupFilesResponse : RestoreStatus
    {
        /// <summary>
        /// List with file names. Empty in error case.
        /// </summary>
        [DataMember]
        public IList<string> NameList { get; set; }

        public EnumerateBackupFilesResponse() : base()
        {
            NameList = new List<string>();
        }
    }
}