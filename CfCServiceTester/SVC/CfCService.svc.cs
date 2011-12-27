using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using System.Data;
using Microsoft.SqlServer.Management.Smo;
using CfCServiceTester.SVC.DataObjects;
using System.Web;
using System.Security.Cryptography;

namespace CfCServiceTester.SVC      //CfCServiceTester
{
    [ServiceContract(Namespace = "CfCServiceNS")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public partial class CfCService
    {
        private static readonly string CertificateKey = "{4908DA1F-CC08-4816-8971-1166CD3B03DB}";

        // To use HTTP GET, add [WebGet] attribute. (Default ResponseFormat is WebMessageFormat.Json)
        // To create an operation that returns XML,
        //     add [WebGet(ResponseFormat=WebMessageFormat.Xml)],
        //     and include the following line in the operation body:
        //         WebOperationContext.Current.OutgoingResponse.ContentType = "text/xml";

        // Ping method for testing the service itself
        [OperationContract]
        public double CostOfSandwiches(int quantity)
        {
            return 1.25 * quantity;
        }
/*
        /// <summary>
        /// The method returns list of SQL servers that are visible for the service. The function will select names with phrase inside the name when
        /// parameter <code>namePattern</code> is not empty.
        /// <remarks>
        ///     Ensure that <code>SQL Server Browser</code> service is started on computers with installed SQL servers.
        ///     SmoApplication.EnumAvailableSqlServers is lying otherwise. 
        ///     SQLEXPRESS instance is not visible on local computer when <code>SQL Server Browser</code> is stopped.
        /// </remarks>
        /// </summary>
        /// <param name="localOnly"><code>true</code> - enumerate local computers only, <code>false</code> - all available SQL servers</param>
        /// <param name="namePattern">Phrase in the server's name.</param>
        /// <returns>List with server's names</returns>
        [OperationContract]
        public IEnumerable<SqlServerDbo> EnumerateSqlServers(bool localOnly, string namePattern)
        {
            DataTable dtSmo = SmoApplication.EnumAvailableSqlServers(localOnly);
            var rzlt = new List<SqlServerDbo>();
            SqlServerDbo dbo; 

            foreach (DataRow dr in dtSmo.Rows)
            {
                dbo = new SqlServerDbo()
                            {
                                Name = dr.Field<string>("Name"),
                                Server = dr.Field<string>("Server"),
                                Instance = dr.Field<string>("Instance"),
                                IsClustered = dr.Field<bool>("IsClustered"),
                                Version = dr.Field<string>("Version"),
                                IsLocal = dr.Field<bool>("IsLocal")
                            };
                if (String.IsNullOrEmpty(namePattern) || dbo.Name.ToUpper().Contains(namePattern.ToUpper()))
                    rzlt.Add(dbo);
            }

            return rzlt;
        }

        /// <summary>
        /// Returns all available databases in selected server
        /// </summary>
        /// <param name="serverName">SQL server's name</param>
        /// <param name="namePattern">Name pattern</param>
        /// <param name="accessibleOnly"><code>true</code> - return accessible databases only</param>
        /// <returns>List of available databases</returns>
        [OperationContract]
        public IEnumerable<DatabaseDbo> EnumerateDatabases(string serverName, string namePattern, bool accessibleOnly)
        {
            try
            {
                Server server = new Server(serverName);
                var rzlt = new List<DatabaseDbo>();
                if (server.Databases == null || server.Databases.Count < 1)
                    return rzlt;
                DatabaseDbo dbo;
                string dbsName;

                foreach (Database dbs in server.Databases)
                {
                    dbsName = dbs.Name;
                    if ((!String.IsNullOrEmpty(dbsName) || String.IsNullOrEmpty(namePattern) || dbsName.ToUpper().Contains(namePattern.ToUpper())) &&
                        (!accessibleOnly || dbs.IsAccessible))
                    {
                        dbo = new DatabaseDbo()
                        {
                            Name = dbsName,
                            ID = dbs.ID,
                            IsAccessible = dbs.IsAccessible,
                            Size = dbs.Size
                        };
                        rzlt.Add(dbo);
                    }
                }
                return rzlt;
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return new List<DatabaseDbo>();;
            }
        }

        /// <summary>
        /// Creates public/private RSA keys, individual pair for every session.
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        public RsaParametersDbo GetPublicKey()
        {
            CspParameters cspParams = new CspParameters();
            cspParams.Flags = CspProviderFlags.UseMachineKeyStore;
            var myRSA = new RSACryptoServiceProvider(cspParams);
            if (HttpContext.Current.Session[CertificateKey] == null)
                HttpContext.Current.Session[CertificateKey] = myRSA.ToXmlString(true);

            myRSA.FromXmlString((string)HttpContext.Current.Session[CertificateKey]);

            RSAParameters param = myRSA.ExportParameters(false);
            var rzlt = new RsaParametersDbo()
            {
                Exponent = CfCService.ToHexString(param.Exponent),
                Modulus = CfCService.ToHexString(param.Modulus)
            };
            return rzlt;
        }
*/
    }
}
