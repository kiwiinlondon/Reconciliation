using Odey.Framework.Infrastructure.Services;
using Odey.Framework.Keeley.Entities;
using Odey.ReconciliationServices.Contracts;
using Odey.StaticServices.Clients;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace Odey.ReconciliationServices.ClientPortfolioReconciliationService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class ClientPortfolioReconciliationService : OdeyServiceBase, IClientPortfolioReconciliation
    {


        public const string DataTableName = "Portfolio";
        public const string AccountReferenceColumnName = "AccountReference";
        public const string FundReferenceColumnName = "Fund";
        public const string QuantityColumnName = "Quantity";
        public const string MarketValueColumnName = "MarketValue";

        public static DataTable GetPortfolioDataTable()
        {
            DataSet ds = new DataSet();
            DataTable dt = ds.Tables.Add(DataTableName);
            DataColumn accountReferenceColumn = dt.Columns.Add(AccountReferenceColumnName, typeof(string));
            DataColumn fundReferenceColumn = dt.Columns.Add(FundReferenceColumnName, typeof(string));
            dt.Columns.Add(QuantityColumnName, typeof(decimal));
            dt.Columns.Add(MarketValueColumnName, typeof(decimal));
            dt.PrimaryKey = new DataColumn[] { accountReferenceColumn, fundReferenceColumn };
            dt.DataSet.EnforceConstraints = false;
            return dt;
        }

        public MatchingEngineOutput Reconcile(DataTable administratorValues, int fundId, DateTime referenceDate)
        {
            DataTable keeleyValues = GetKeeleyValues(fundId, referenceDate);
            
            ClientPortfolioMatchingEngine engine = new ClientPortfolioMatchingEngine(Logger);
            MatchingEngineOutput output = engine.Match(administratorValues, keeleyValues, MatchTypeIds.Full, true, DataSourceIds.AdministratorClientFile, DataSourceIds.KeeleyClientPortfolio);            

            return output;
        }

        
        private static Dictionary<int,Fund> Funds = new FundClient().GetAll().ToDictionary(a => a.LegalEntityID,a=>a);

        private static readonly Dictionary<int, string[]> AdministratorShareClassIdsByFund = Funds.Values.Where(a => a.AdministratorIdentifier != null).GroupBy(g => g.ParentFundId.HasValue ? g.ParentFundId.Value : g.LegalEntityID).ToDictionary(a => a.Key, a => a.Select(s => s.AdministratorIdentifier).ToArray());

        public MatchingEngineOutput Reconcile(string fileName, int fundId, DateTime referenceDate)
        {
            if (!AdministratorShareClassIdsByFund.TryGetValue(fundId,out var identifiers))
            {
                identifiers = Funds.Values.Where(a => a.LegalEntityID == fundId).Select(a => a.AdministratorIdentifier).ToArray();
            }
            FileReader fileReader = FileReaderFactory.Get(fileName, Funds[fundId], identifiers);
            DataTable values = fileReader.GetData();
            var t = values.Rows.Cast<DataRow>().Where(a => (string)a[1] == "O07");
            return Reconcile(values, fundId, referenceDate);
        }
        

    

        #region Keeley
        private static Dictionary<string, object> CreateKeeleyParameters(int fundId, DateTime referenceDate)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            parameters.Add("@referenceDate", referenceDate);

            parameters.Add("@fundIds", fundId.ToString());

            return parameters;
        }

        public static DataTable GetKeeleyValues(int fundId, DateTime referenceDate)
        {
            DataTable dt = GetPortfolioDataTable();
            DataSetUtilities.FillKeeleyDataTable(dt, KeeleyStoredProcedureName, CreateKeeleyParameters(fundId, referenceDate), null);
            dt.DataSet.EnforceConstraints = true;
            return dt;
        }

        private static readonly string KeeleyStoredProcedureName = ConfigurationManager.AppSettings["KeeleyClientPortfolioStoredProcedureName"];
        #endregion

        
    }
}
