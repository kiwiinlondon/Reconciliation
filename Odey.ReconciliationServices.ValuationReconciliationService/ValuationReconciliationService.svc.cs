using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using Odey.Framework.Infrastructure.Services;
using Odey.ReconciliationServices.Contracts;
using System.Data;

namespace Odey.ReconciliationServices.ValuationReconciliationService
{
    
    public class ValuationReconciliationService : OdeyServiceBase , IValuationReconciliation
    {
        public MatchingEngineOutput MatchPositionsAgainstKeeley(int fundId, DateTime referenceDate, List<PortfolioReconciliationItem> portfolioItems, int[] instrumentClassIdsToExclude)
        {
            DataTable dt1 = GetKeeleyPortfolioAtValuationTime(fundId, referenceDate, instrumentClassIdsToExclude);
            DataTable dt2 = ConvertExternalPortfolioToDataSet(portfolioItems);
            ValuationMatchingEngine engine = new ValuationMatchingEngine(Logger);
            MatchingEngineOutput output = engine.Match(dt1, dt2, MatchTypeIds.Full, false, DataSourceIds.KeeleyPortfolio, DataSourceIds.ExternalPortfolio);
            return output;
        }

        private DataTable ConvertExternalPortfolioToDataSet(List<PortfolioReconciliationItem> portfolioItems)
        {
            DataTable dt = GetNewDataTable();
            foreach (PortfolioReconciliationItem reconciliationItem in portfolioItems)
            {
                DataRow dr = dt.NewRow();
                dr["InstrumentMarketId"] = reconciliationItem.InstrumentMarketId;
                dr["IsAccrual"] = reconciliationItem.IsAccrual;
                dr["InstrumentClassID"] = reconciliationItem.InstrumentClassId;
                dr["MaturityDate"] = reconciliationItem.MaturityDate;
                dr["FundId"] = reconciliationItem.FundId;

                dr["NetPosition"] = reconciliationItem.Holding;
                dr["Price"] = reconciliationItem.Price;
                dr["FXRate"] = reconciliationItem.FXRate;
                dr["MarketValue"] = reconciliationItem.MarketValue;
                dt.Rows.Add(dr);
            }
            return dt;
        }

        private static DataTable GetNewDataTable()
        {

            DataTable dt = new DataTable("Positions");
            DataColumn instrumentMarketId = dt.Columns.Add("InstrumentMarketId", typeof(int));
            DataColumn isAccrual = dt.Columns.Add("IsAccrual", typeof(bool));
            DataColumn instClassId = dt.Columns.Add("InstrumentClassID", typeof(int));
            DataColumn maturityDate = dt.Columns.Add("MaturityDate", typeof(DateTime?));
            DataColumn fundId = dt.Columns.Add("FundId", typeof(int));

            dt.Columns.Add("NetPosition", typeof(decimal));         
            dt.Columns.Add("Price", typeof(decimal));
            dt.Columns.Add("FXRate", typeof(decimal));
            dt.Columns.Add("MarketValue", typeof(decimal));

            dt.PrimaryKey = new DataColumn[] { instrumentMarketId, isAccrual, instClassId, maturityDate, fundId };
            return dt;

        }

        private static Dictionary<string, object> CreateParameters(int fundId, DateTime referenceDate)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            parameters.Add("@fundId", fundId);
            parameters.Add("@referenceDate", referenceDate);

            return parameters;
        }

        private DataTable GetKeeleyPortfolioAtValuationTime(int fundId, DateTime referenceDate, int[] instrumentClassIdsToExclude)
        {
            DataTable dt = GetNewDataTable();
            DataSetUtilities.FillKeeleyDataTable(dt, "Portfolio_GetValuation", CreateParameters(fundId, referenceDate), null);

            //delete rows where inst class id in instrumentClassIdsToExclude
            if (instrumentClassIdsToExclude != null && instrumentClassIdsToExclude.Length>0)
            {
                string toDeleteQuery = string.Format("InstrumentClassID IN ({0})", string.Join(",", instrumentClassIdsToExclude));
                var rows = dt.Select(toDeleteQuery);
                foreach (var row in rows)
                {
                    row.Delete();
                }
                dt.AcceptChanges();
            }

            return dt;
        }

        
    }
}
