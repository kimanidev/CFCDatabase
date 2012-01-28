using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace CfCServiceTester.WEBservice.DataObjects
{
    /// <summary>
    /// Request for updating foreign key in the table
    /// </summary>
    [DataContract(Namespace = "CfCServiceNS")]
    public class UpdateForeignKeyRequest
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
        public string OldForeignKeyName { get; set; }

        /// <summary>
        /// New name, for renaming and delete operation only
        /// </summary>
        [DataMember]
        public string ForeignKeyName { get; set; }
    }
}