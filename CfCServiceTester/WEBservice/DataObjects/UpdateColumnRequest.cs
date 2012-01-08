using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace CfCServiceTester.WEBservice.DataObjects
{
    public enum UpdateColumnOperation { Insert, Delete, Rename, Modify };

    /// <summary>
    /// Request for updating column in the table
    /// </summary>
    [DataContract(Namespace = "CfCServiceNS")]
    public class UpdateColumnRequest
    {
        /// <summary>
        /// Update operation, <see cref="UpdateColumnOperation"/>
        /// </summary>
        [DataMember]
        public UpdateColumnOperation Operation { get; set; }

        /// <summary>
        /// Table name
        /// </summary>
        [DataMember]
        public string Table { get; set; }

        /// <summary>
        /// Old name, for renaming operation only
        /// </summary>
        [DataMember]
        public string OldColumnName { get; set; }

        /// <summary>
        /// Definition of new or updated column
        /// </summary>
        [DataMember]
        public DataColumnDbo Column { get; set; }

        /// <summary>
        /// <code>true</code> - switch database into single user mode
        /// </summary>
        [DataMember]
        public bool SingleUserMode { get; set; }

        /// <summary>
        /// <code>true</code> - disable dependencies before deleting the column
        /// </summary>
        [DataMember]
        public bool DisableDependencies { get; set; }
    }
}