using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace CfCServiceTester.WEBservice.DataObjects
{
    [DataContract(Namespace = "CfCServiceNS")]
    public class EnumerateColumnsResponse : RestoreStatus
    {
        [DataMember]
        public List<DataColumnDbo> Columns { get; set; }

        public EnumerateColumnsResponse() : base()
        {
            this.Columns = new List<DataColumnDbo>();
        }
    }

    [DataContract(Namespace = "CfCServiceNS")]
    public class InsertColumnResponseBase : RestoreStatus
    {
        [DataMember]
        public DataColumnDbo Column { get; set; }
    }

    [DataContract(Namespace = "CfCServiceNS")]
    public class InsertColumnResponse : InsertColumnResponseBase
    {
        [DataMember]
        public List<DroppedDependencyDbo> DroppedForeignKeys { get; set; }

        public InsertColumnResponse() : base()
        {
            this.DroppedForeignKeys = new List<DroppedDependencyDbo>();
        }
    }

    [DataContract(Namespace = "CfCServiceNS")]
    public class RenameColumnResponse : InsertColumnResponseBase
    {
        [DataMember]
        public List<AlteredDependencyDbo> AlteredDependencies { get; set; }

        public RenameColumnResponse() : base()
        {
            this.AlteredDependencies = new List<AlteredDependencyDbo>();
        }
    }

    [DataContract(Namespace = "CfCServiceNS")]
    public class DeleteColumnResponse : InsertColumnResponseBase
    {
        [DataMember]
        public List<DroppedDependencyDbo> DroppedDependencies { get; set; }

        public DeleteColumnResponse()
            : base()
        {
            this.DroppedDependencies = new List<DroppedDependencyDbo>();
        }
    }

    [DataContract(Namespace = "CfCServiceNS")]
    public class DeleteTableResponse : RestoreStatus
    {
        [DataMember]
        public List<DroppedDependencyDbo> DroppedDependencies { get; set; }

        public DeleteTableResponse(): base()
        {
            this.DroppedDependencies = new List<DroppedDependencyDbo>();
        }
    }

    [DataContract(Namespace = "CfCServiceNS")]
    public class GetIndexResponse : RestoreStatus
    {
        [DataMember]
        public IndexDbo Dbo { get; set; }
    }

    [DataContract(Namespace = "CfCServiceNS")]
    public class EnumerateIndexesResponse : RestoreStatus
    {
        [DataMember]
        public List<IndexDbo> Indexes { get; set; }

        public EnumerateIndexesResponse(): base()
        {
            this.Indexes = new List<IndexDbo>();
        }
    }
}