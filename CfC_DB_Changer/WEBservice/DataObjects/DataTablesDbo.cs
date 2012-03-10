using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace CfCServiceTester.WEBservice.DataObjects
{
    /// <summary>
    /// List with table names
    /// </summary>
    [DataContract(Namespace = "CfCServiceNS")]
    public class DataTableListDbo : RestoreStatus
    {
        /// <summary>
        /// List with fetched table names. Empty in error case.
        /// </summary>
        [DataMember]
        public IList<string> TableNames { get; set; }

        public DataTableListDbo(): base()
        {
            TableNames = new List<string>();
        }
    }
}