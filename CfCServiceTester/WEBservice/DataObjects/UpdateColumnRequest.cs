using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace CfCServiceTester.WEBservice.DataObjects
{
    public enum UpdateColumnOperation { Insert, Delete, Rename, Modify };

    /// <summary>
    /// Base request for DB updating
    /// </summary>
    [DataContract(Namespace = "CfCServiceNS")]
    public class DbModifyRequest
    {
        /// <summary>
        /// Update operation, <see cref="UpdateColumnOperation"/>
        /// </summary>
        [DataMember]
        public UpdateColumnOperation Operation { get; set; }

        /// <summary>
        /// <code>[CFC_DB_Major_Version] [smallint] NOT NULL DEFAULT (0)</code>
        /// </summary>
        [DataMember]
        public short CFC_DB_Major_Version { get; set; }

        /// <summary>
        /// <code>[CFC_DB_Minor_Version] [smallint] NOT NULL DEFAULT (0)</code>
        /// </summary>
        [DataMember]
        public short CFC_DB_Minor_Version { get; set; }

        /// <summary>
        /// Table name
        /// </summary>
        [DataMember]
        public string Table { get; set; }
    }

    /* ******************* Requests for updating tables ******************* */
    [DataContract(Namespace = "CfCServiceNS")]
    public class DeleteTableRequest : DbModifyRequest
    {
        /// <summary>
        /// <code>true</code> remove references to the table
        /// </summary>
        [DataMember]
        public bool disableDependencies { get; set; }
    }

    [DataContract(Namespace = "CfCServiceNS")]
    public class RenameTableRequest : DbModifyRequest
    {
        /// <summary>
        /// Old name of the table
        /// </summary>
        [DataMember]
        public string OldName { get; set; }

        /// <summary>
        /// <code>true</code> Single user mode
        /// </summary>
        [DataMember]
        public bool SingleUserMode { get; set; }
    }


    /* ******************* Requests for updating columns ******************* */
    /// <summary>
    /// Request for updating column in the table
    /// </summary>
    [DataContract(Namespace = "CfCServiceNS")]
    public class UpdateColumnRequest : DbModifyRequest
    {

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