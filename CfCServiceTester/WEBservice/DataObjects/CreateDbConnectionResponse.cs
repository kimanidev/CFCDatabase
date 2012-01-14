using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace CfCServiceTester.WEBservice.DataObjects
{
    [DataContract(Namespace = "CfCServiceNS")]
    public class CreateDbConnectionResponse
    {
        /// <summary>
        /// <para>Connection status:</para>
        /// <list type="bullet">
        ///     <item>
        ///         <code>true</code> - connection is correct (ErrorMessage is empty, Roles contains roles for user in the database)
        ///     </item>
        ///     <item>
        ///         <code>false</code> - incorrect connection (ErrorMessage contains reason, Roles is empty)
        ///     </item>
        /// </list>
        /// </summary>
        [DataMember]
        public bool Connected { get; set; }

        /// <summary>
        /// Error messge, not empty when <code>Connected == false</code>
        /// </summary>
        [DataMember]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// ECurrent server
        /// </summary>
        [DataMember]
        public string CurrentServer { get; set; }

        /// <summary>
        /// Current Database
        /// </summary>
        [DataMember]
        public string CurrentDatabase { get; set; }

        /// <summary>
        /// User's roles in the database, not empty when <code>Connected == true</code>
        /// </summary>
        [DataMember]
        public IList<string> Roles { get; set; }

        /// <summary>
        /// This constructor is visible on server side only
        /// </summary>
        public CreateDbConnectionResponse() : base()
        {
            this.Roles = new List<string>();
        }
    }
}