using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace CfCServiceTester.WEBservice.DataObjects
{
    public enum DbObjectType { StoredProcedure, View, Trigger, UserDefinedFunction };

    [DataContract(Namespace = "CfCServiceNS")]
    public class AlteredDependencyDbo
    {
        /// <summary>
        /// Dependency type
        /// </summary>
        [DataMember]
        public DbObjectType ObjectType { get; set; }

        /// <summary>
        /// Name of the dependency
        /// </summary>
        [DataMember]
        public string Name { get; set; }
    }
}