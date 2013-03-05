using Odey.Framework.Infrastructure.Services;
using Odey.ReconciliationServices.Contracts;
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
        

        private const string DataTableName = "Portfolio";
        private const string AccountReferenceColumnName = "AccountReference";
        private const string QuantityColumnName = "Quantity";
        private const string MarketValueColumnName = "MarketValue";

        private static DataTable GetPortfolioDataTable()
        {

            DataTable dt = new DataTable(DataTableName);
            DataColumn accountReferenceColumn = dt.Columns.Add(AccountReferenceColumnName, typeof(string));
            dt.Columns.Add(QuantityColumnName, typeof(decimal));
            dt.Columns.Add(MarketValueColumnName, typeof(decimal));
        //    dt.PrimaryKey = new DataColumn[] { accountReferenceColumn };
            return dt;
        }

        public MatchingEngineOutput Reconcile(DataTable administratorValues, int[] fundIds, DateTime referenceDate)
        {
            DataTable keeleyValues = GetKeeleyValues(fundIds, referenceDate);
            ClientPortfolioMatchingEngine engine = new ClientPortfolioMatchingEngine(Logger);
            MatchingEngineOutput output = engine.Match(administratorValues, keeleyValues, MatchTypeIds.Full, true, DataSourceIds.AdministratorClientFile, DataSourceIds.KeeleyClientPortfolio);            

            return output;
        }

        #region Daiwa


        public MatchingEngineOutput ReconcileDaiwa(string fileName, int[] fundId, DateTime referenceDate)
        {
            DataTable values = GetDaiwaValues(fileName);
            values.PrimaryKey = new DataColumn[] { values.Columns[AccountReferenceColumnName] };
            return Reconcile(values, fundId, referenceDate);
        }
        private static Dictionary<string, string> CreateDaiwaColumnMappings()
        {
            Dictionary<string, string> columnMappings = new Dictionary<string, string>();
            columnMappings.Add("fund_id & '~' & holder_id & '~' & acct_id", AccountReferenceColumnName);

            columnMappings.Add("shares", QuantityColumnName);
            columnMappings.Add("market_value", MarketValueColumnName);
            
            return columnMappings;
        }

        private static List<string> CreateDaiwaGrouping()
        {
            return new List<string>() { AccountReferenceColumnName };
        }

        public static DataTable GetDaiwaValues(string fileName)
        {
            DataTable dt = GetPortfolioDataTable();
            DataSetUtilities.FillFromExcelFile(fileName, "share_register_by_lot", dt, CreateDaiwaColumnMappings(), CreateDaiwaGrouping());
            return dt;
        }
        #endregion

        #region Keeley
        private static Dictionary<string, object> CreateKeeleyParameters(int[] fundIds, DateTime referenceDate)
        {
            string fundIdString = string.Join(",", fundIds);
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            parameters.Add("@referenceDate", referenceDate);

            parameters.Add("@fundIds", fundIdString);

            return parameters;
        }

        public static DataTable GetKeeleyValues(int[] fundId, DateTime referenceDate)
        {
            DataTable dt = GetPortfolioDataTable();
            DataSetUtilities.FillKeeleyDataTable(dt, KeeleyStoredProcedureName, CreateKeeleyParameters(fundId, referenceDate), null);
            return dt;
        }

        private static readonly string KeeleyStoredProcedureName = ConfigurationManager.AppSettings["KeeleyClientPortfolioStoredProcedureName"];
        #endregion

        
    }
}
