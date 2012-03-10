using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace CfCServiceTester.WEBservice.DataObjects
{
    public enum DbObjectType { StoredProcedure, View, Trigger, UserDefinedFunction, index, primaryKey, foreignKey };

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

    [DataContract(Namespace = "CfCServiceNS")]
    public class DroppedDependencyDbo : AlteredDependencyDbo
    {
        /// <summary>
        /// Name of the dependency
        /// </summary>
        [DataMember]
        public string TableName { get; set; }

        /// <summary>
        /// Inner text
        /// </summary>
        [DataMember]
        public IList<string> Columns { get; set; }

        public DroppedDependencyDbo() : base()
        {
            Columns = new List<string>();
        }

    }
}