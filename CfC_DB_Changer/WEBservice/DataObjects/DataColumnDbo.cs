using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace CfCServiceTester.WEBservice.DataObjects
{
    /// <summary>
    /// Column in the table
    /// </summary>
    [DataContract(Namespace = "CfCServiceNS")]
    public class DataColumnDbo
    {
        /// <summary>
        /// Column name
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// SQL data type
        /// </summary>
        [DataMember]
        public string SqlDataType { get; set; }

        /// <summary>
        /// Maximum length of the data type.
        /// </summary>
        [DataMember]
        public int? MaximumLength { get; set; }

        /// <summary>
        /// Numeric precision of the data type.
        /// </summary>
        [DataMember]
        public int? NumericPrecision { get; set; }

        /// <summary>
        /// Numeric scale of the data type.
        /// </summary>
        [DataMember]
        public int? NumericScale { get; set; }

        /// <summary>
        /// Specifies whether the column can accept null values..
        /// </summary>
        [DataMember]
        public bool IsNullable { get; set; }

        /// <summary>
        /// Specifies whether the column can accept null values.
        /// </summary>
        [DataMember]
        public bool IsIdentity { get; set; }

        /// <summary>
        /// Column is included in the primary key.
        /// </summary>
        [DataMember]
        public bool IsPrimaryKey { get; set; }

        /// <summary>
        /// Default value for the column.
        /// </summary>
        [DataMember]
        public string Default { get; set; }

        public static bool RequiresRecreating(DataColumnDbo item1, DataColumnDbo item2)
        {
            return item1.SqlDataType != item2.SqlDataType ||
                   item1.MaximumLength != item2.MaximumLength ||
                   item1.NumericPrecision != item2.NumericPrecision ||
                   item1.NumericScale != item2.NumericScale ||
                   item1.IsIdentity != item2.IsIdentity;
        }
    }
}