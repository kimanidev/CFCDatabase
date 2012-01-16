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
    public class UpdateIndexRequest
    {
        /// <summary>
        /// Operation type, <see cref="UpdateColumnOperation"/>
        /// </summary>
        [DataMember]
        public UpdateColumnOperation OperationType { get; set; }

        /// <summary>
        /// Table name
        /// </summary>
        [DataMember]
        public string TableName { get; set; }

        /// <summary>
        /// Old name, for renaming operation only
        /// </summary>
        [DataMember]
        public string OldIndexName { get; set; }

        /// <summary>
        /// Old name, for renaming operation only
        /// </summary>
        [DataMember]
        public string IndexName { get; set; }
    }
}