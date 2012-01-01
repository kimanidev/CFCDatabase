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
    public class InsertColumnResponse : RestoreStatus
    {
        [DataMember]
        public DataColumnDbo Column { get; set; }
    }
}