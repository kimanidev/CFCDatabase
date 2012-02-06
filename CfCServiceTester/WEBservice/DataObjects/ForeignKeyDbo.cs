using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using Microsoft.SqlServer.Management.Smo;

namespace CfCServiceTester.WEBservice.DataObjects
{
    /// <summary>
    /// Foreign key descriptor, 
    /// <see cref="http://msdn.microsoft.com/en-US/library/microsoft.sqlserver.management.smo.foreignkey%28v=sql.90%29.aspx"/>
    /// </summary>
    [DataContract(Namespace = "CfCServiceNS")]
    public class ForeignKeyDbo
    {
        /// <summary>
        /// Gets or sets the name of the foreign key.
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Represents a collection of Column objects. Each Column object represents a column included in the foreign key.
        /// </summary>
        [DataMember]
        public IList<ForeignKeyColumnDbo> Columns { get; set; }

        /// <summary>
        /// Gets or sets the action taken when the row that is referenced by the foreign key is deleted. 
        /// </summary>
        [DataMember]
        public ForeignKeyAction DeleteAction { get; set; }

        /// <summary>
        /// Gets or sets the Boolean property value that specifies whether the foreign key supports cascading updates.
        /// </summary>
        [DataMember]
        public ForeignKeyAction UpdateAction { get; set; }

        /// <summary>
        /// Gets or sets the Boolean property value that specifies whether the foreign key constraint was enabled 
        /// without checking existing rows. 
        /// </summary>
        [DataMember]
        public bool IsChecked { get; set; }

        /// <summary>
        /// Gets or sets the table that contains the primary key referenced by the foreign key.  
        /// </summary>
        [DataMember]
        public string ReferencedTable { get; set; }

        /// <summary>
        /// Columns from primary key and unique constraints in the ReferencedTable table.  
        /// </summary>
        [DataMember]
        public IList<string> AvailableTargetColumns { get; set; }

        public ForeignKeyDbo()
        {
            this.Columns = new List<ForeignKeyColumnDbo>();
        }
    }

    /// <summary>
    /// Foreign key column. 
    /// <see cref="http://msdn.microsoft.com/en-us/library/microsoft.sqlserver.management.smo.foreignkeycolumn.aspx"/>
    /// </summary>
    [DataContract(Namespace = "CfCServiceNS")]
    public class ForeignKeyColumnDbo
    {
        /// <summary>
        /// Gets or sets the name of the foreign key column.
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the referenced column.
        /// </summary>
        [DataMember]
        public string ReferencedColumn { get; set; }
    }
}