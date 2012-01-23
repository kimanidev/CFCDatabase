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
        public byte? FillFactor { get; set; }

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

        /// <summary>
        /// Verifies is the obj equals to this.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (!typeof(IndexDbo).IsInstanceOfType(obj))
                return false;
            
            IndexDbo other = obj as IndexDbo;
            bool rzlt = this.CompactLargeObjects == other.CompactLargeObjects && this.DisallowPageLocks == other.DisallowPageLocks     &&
                        this.DisallowRowLocks == other.DisallowRowLocks       && this.FillFactor        == other.FillFactor            &&
                        this.FilterDefinition == other.FilterDefinition       && this.IgnoreDuplicateKeys == other.IgnoreDuplicateKeys &&
                        this.IndexedColumns.Count == other.IndexedColumns.Count &&
                        this.IndexedColumns.TrueForAll(x => other.IndexedColumns.Contains(x)) &&
                        this.IndexKeyType == other.IndexKeyType               && this.IsClustered == other.IsClustered                 &&
                        this.IsDisabled == other.IsDisabled                   && this.IsUnique == other.IsUnique                       &&
                        this.Name == other.Name;
            return rzlt;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        /// <summary>
        /// <see cref="http://msdn.microsoft.com/en-us/library/ms173147(v=vs.80).aspx"/>
        /// </summary>
        /// <param name="a">Left operand</param>
        /// <param name="b">Right operand</param>
        /// <returns><code>true</code> - operands are equal</returns>
        public static bool operator ==(IndexDbo a, IndexDbo b)
        {
            if (System.Object.ReferenceEquals(a, b))
                return true;
            if ((object)a == null || (object)b == null)
                return false;

            return a.Equals(b);
        }
        public static bool operator !=(IndexDbo a, IndexDbo b)
        {
            return !(a == b);
        }
    }
}