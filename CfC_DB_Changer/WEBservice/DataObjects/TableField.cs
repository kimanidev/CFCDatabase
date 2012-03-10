using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace CfCServiceTester.WEBservice.DataObjects
{
    [DataContract(Namespace = "CfCServiceNS")]
    public struct TableField
    {
        /// <summary>
        /// Name of the field
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// <code>true</code> - field is included into index or foreign key
        /// </summary>
        [DataMember] 
        public bool IsIncluded { get; set; }
    }
}