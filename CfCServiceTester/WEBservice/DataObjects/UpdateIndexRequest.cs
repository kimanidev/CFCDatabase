using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace CfCServiceTester.WEBservice.DataObjects
{
    /// <summary>
    /// Request for updating index in the table
    /// </summary>
    [DataContract(Namespace = "CfCServiceNS")]
    public class UpdateIndexRequest : DbModifyRequest
    {
/*
        /// <summary>
        /// Operation type, <see cref="UpdateColumnOperation"/>
        /// </summary>
        [DataMember]
        public UpdateColumnOperation Operation { get; set; }

        /// <summary>
        /// Table name
        /// </summary>
        [DataMember]
        public string Table { get; set; }
*/
        /// <summary>
        /// Old name, for renaming operation only
        /// </summary>
        [DataMember]
        public string OldIndexName { get; set; }

        /// <summary>
        /// New name, for renaming operation only
        /// </summary>
        [DataMember]
        public string IndexName { get; set; }

        /// <summary>
        /// <code>true></code> - disable dependencies before deleting (for delete and modify operations only)
        /// </summary>
        [DataMember]
        public bool DisableDependencies { get; set; }

        /// <summary>
        /// Index descriptor is used in Insert and Modify operations
        /// </summary>
        [DataMember]
        public IndexDbo IndexDescriptor { get; set; }
    }
}