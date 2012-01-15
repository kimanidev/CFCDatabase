using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace CfCServiceTester.WEBservice.DataObjects
{
//    public enum IndexKeyType { None, DriPrimaryKey, DriPrimaryKey };

    /// <summary>
    /// Index descriptor, <see cref="http://msdn.microsoft.com/en-us/library/microsoft.sqlserver.management.smo.index.aspx"/>
    /// </summary>
    [DataContract(Namespace = "CfCServiceNS")]
    public class IndexDbo
    {
        /// <summary>
        /// Gets or sets the Boolean property value that specifies whether to compact the large object (LOB) data in the index.
        /// </summary>
        [DataMember]
        public bool CompactLargeObjects { get; set; }

        /// <summary>
        /// Gets or sets the Boolean property value that specifies whether the index allows page locks.
        /// </summary>
        [DataMember]
        public bool DisallowPageLocks { get; set; }

        /// <summary>
        /// Gets or sets the Boolean property value that specifies whether the index allows row locks.
        /// </summary>
        [DataMember]
        public bool DisallowRowLocks { get; set; }

        /// <summary>
        /// Gets or sets the percentage of an index page to fill when the index is created or re-created.
        /// </summary>
        [DataMember]
        public byte FillFactor { get; set; }

        /// <summary>
        /// Gets or sets the String value that contains the definition for the filter.
        /// </summary>
        [DataMember]
        public string FilterDefinition { get; set; }

        /// <summary>
        /// Gets or sets the Boolean property value that specifies whether the index ignores duplicate keys.
        /// </summary>
        [DataMember]
        public bool IgnoreDuplicateKeys { get; set; }

        /// <summary>
        /// Gets a collection of IndexedColumn objects that represent all the columns participating in the index.
        /// </summary>
        [DataMember]
        public List<string> IndexedColumns { get; set; }

        /// <summary>
        /// Gets or sets the index key type.
        /// Valid values are: "None", "DriPrimaryKey", "DriUniqueKey"
        /// </summary>
        [DataMember]
        public string IndexKeyType { get; set; }

        /// <summary>
        /// Gets or sets the Boolean property value that specifies whether the index is clustered.
        /// </summary>
        [DataMember]
        public bool IsClustered { get; set; }

        /// <summary>
        /// Gets or sets the Boolean property value that specifies whether the index is disabled.
        /// </summary>
        [DataMember]
        public bool IsDisabled { get; set; }

        /// <summary>
        /// Gets or sets the Boolean property value that specifies whether the index is unique or not.
        /// </summary>
        [DataMember]
        public bool IsUnique { get; set; }

        /// <summary>
        /// Gets or sets the name of the index.
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        public IndexDbo()
        {
            this.IndexedColumns = new List<string>();
        }
    }
}