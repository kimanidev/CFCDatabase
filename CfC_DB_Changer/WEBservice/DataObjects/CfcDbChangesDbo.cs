using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace CfCServiceTester.WEBservice.DataObjects
{
    /// <summary>
    /// <code>[dbo].[CFC_DB_Changes]</code>
    /// </summary>
    [DataContract(Namespace = "CfCServiceNS")]
    public class CfcDbChangesDbo
    {
        /// <summary>
        /// <code>[DB_Change_GUID] [uniqueidentifier] NOT NULL</code>
        /// </summary>
        [DataMember]
        public Guid DB_Change_GUID { get; set; }

        /// <summary>
        /// <code>[CFC_DB_Name] [nvarchar](50) NOT NULL</code>
        /// </summary>
        [DataMember]
        public string CFC_DB_Name { get; set; }

        /// <summary>
        /// <code>[CFC_DB_Major_Version] [smallint] NOT NULL DEFAULT (0)</code>
        /// </summary>
        [DataMember]
        public short CFC_DB_Major_Version { get; set; }
			
        /// <summary>
        /// <code>[CFC_DB_Minor_Version] [smallint] NOT NULL DEFAULT (0)</code>
        /// </summary>
        [DataMember]
		public short CFC_DB_Minor_Version { get; set; }

        /// <summary>
        /// <code>[Seq_No] [int] NOT NULL</code>
        /// </summary>
        [DataMember]
		public int Seq_No { get; set; }

        /// <summary>
        /// <code>[Table_Name] [nvarchar](100) NOT NULL</code>
        /// </summary>
        [DataMember]
		public string Table_Name { get; set; }

        /// <summary>
        /// <code>[Change_Description] [nvarchar](max) NULL</code>
        /// </summary>
        [DataMember]
		public string Change_Description { get; set; }

        /// <summary>
        /// <code>[Created_By] [nvarchar](50) NOT NULL</code>
        /// </summary>
        [DataMember]
		public string Created_By { get; set; }

        /// <summary>
        /// <code>[Created_Date] [datetime] NOT NULL</code>
        /// </summary>
        [DataMember]
        public DateTime Created_Date { get; set; }

        /// <summary>
        /// <code>[Last_Update_By] [nvarchar](50) NOT NULL</code>
        /// </summary>
        [DataMember]
		public string Last_Update_By { get; set; }

        /// <summary>
        /// <code>[Last_Update] [datetime] NOT NULL</code>
        /// </summary>
        [DataMember]
        public DateTime Last_Update { get; set; }
    }
}