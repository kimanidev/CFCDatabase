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
    public class UpdateForeignKeyRequest: DbModifyRequest
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
        public string OldForeignKeyName { get; set; }

        /// <summary>
        /// New name, for renaming and delete operation only. 
        /// Both names are equal for the Insert and Delete operations.
        /// </summary>
        [DataMember]
        public string ForeignKeyName { get; set; }

        /// <summary>
        /// Description of the new foreign key
        /// </summary>
        [DataMember]
        public ForeignKeyDbo Dbo { get; set; }
    }
}