using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using Odey.ReconciliationServices.Contracts;
using Odey.Framework.Infrastructure.Services;
using System.Data;
using System.Data.SqlClient;
using System.Data.Common;
using Odey.ReconciliationServices;
using Odey.Framework.Keeley.Entities;
using Odey.StaticServices.Clients;
using Odey.ReconcilationServices.FMKeeleyReconciliationService.MatchingEngines;

namespace Odey.ReconciliationServices.FMKeeleyReconciliationService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    public class FMKeeleyReconciliationService : OdeyServiceBase, IFMKeeleyReconciliation
    {
        #region Reconcile CVL Positions 

        public MatchingEngineOutput GetUnmatchedCVLPositions(int fundId, DateTime fromDate, DateTime toDate, bool returnOnlyMismatches)
        {
            Logger.Info(String.Format("Fund {0}", fundId));
            Logger.Info(String.Format("From Date {0} -> {1}", fromDate, toDate));
            DataTable dt1 = GetKeeleyPositions(fundId, fromDate, toDate);
            Logger.Info(String.Format("Keeeley {0}", dt1.Rows.Count));
            FundClient client = new FundClient();
            Fund fund = client.Get(fundId);
            DataTable dt2 = GetFMPositions(fund.FMOrgId, fromDate, toDate);
            Logger.Info(String.Format("CVL {0}", dt2.Rows.Count));
            CVLMatchingEngine engine = new CVLMatchingEngine(Logger);
            MatchingEngineOutput output = engine.Match(dt1, dt2, MatchTypeIds.Full, returnOnlyMismatches,DataSourceIds.KeeleyPortfolio,DataSourceIds.FMContViewLadder);
            Logger.Info(String.Format("Outputs {0}", output.Outputs.Count));
            Logger.Info(String.Format("Outputs Mismatched {0}", output.Outputs.Where(a=>a.MatchOutputType== MatchOutputTypeIds.Mismatched).Count()));
            Logger.Info(String.Format("Outputs Missing from 1 {0}", output.Outputs.Where(a=>a.MatchOutputType== MatchOutputTypeIds.MissingFrom1).Count()));
            Logger.Info(String.Format("Outputs Missing from 2 {0}", output.Outputs.Where(a => a.MatchOutputType == MatchOutputTypeIds.MissingFrom2).Count()));
            
            return output;
        }

        #endregion

        #region Get Matche dNavs

        public MatchingEngineOutput GetMatchedNavs(DateTime referenceDate)
        {
            DataTable dt1 = GetKeeleyNavs(referenceDate);
            DataTable dt2 = GetFMNavs(referenceDate);
            NavMatchingEngine engine = new NavMatchingEngine(Logger);
            MatchingEngineOutput output = engine.Match(dt1, dt2, MatchTypeIds.Full, false, DataSourceIds.KeeleyPortfolio, DataSourceIds.FMContViewLadder);
            return output;
        }

        #endregion

        #region CVL Matching

        private static DataTable GetNewCVLDataTable()
        {

            DataTable dt = new DataTable("Positions");
            DataColumn refDate = dt.Columns.Add("ReferenceDate", typeof(DateTime));
            DataColumn bookId = dt.Columns.Add("FMBookId", typeof(int));
            DataColumn fmSecId = dt.Columns.Add("FMSecId", typeof(int));
            DataColumn ccyIso = dt.Columns.Add("CcyIso", typeof(string));
            DataColumn instClass = dt.Columns.Add("FMInstClass", typeof(string));
            dt.Columns.Add("NetPosition", typeof(decimal));
            dt.Columns.Add("UnitCost", typeof(decimal));
            dt.Columns.Add("Price", typeof(decimal));
            dt.Columns.Add("FXRate", typeof(decimal));
            dt.Columns.Add("MarketValue", typeof(decimal));
            dt.Columns.Add("DeltaMarketValue", typeof(decimal));
            dt.Columns.Add("TotalAccrual", typeof(decimal));
            dt.Columns.Add("CashIncome", typeof(decimal));
            //_dt.Columns.Add("RealisedFXPNL", typeof(decimal));
            //_dt.Columns.Add("RealisedPricePNL", typeof(decimal));
            //_dt.Columns.Add("UnRealisedPNL", typeof(decimal));
            dt.Columns.Add("TotalPNL", typeof(decimal));
            dt.PrimaryKey = new DataColumn[] { refDate, bookId, fmSecId, ccyIso };
            return dt;

        }

        private static Dictionary<string, object> CreateDataSet1Parameters(int fundId, DateTime fromDate, DateTime toDate)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            parameters.Add("@fundId", fundId);
            parameters.Add("@fromDt", fromDate);
            parameters.Add("@toDt", toDate);

            return parameters;
        }

        private static Dictionary<string, object> CreateDataSet2Parameters(int fundId, DateTime fromDate, DateTime toDate)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            parameters.Add("@in_fund_Id", fundId);
            parameters.Add("@in_from_Dt", fromDate);
            parameters.Add("@in_to_Dt", toDate);

            return parameters;
        }

        private static Dictionary<string, string> CreateDataSet1ColumnMappings()
        {
            Dictionary<string, string> columnMappings = new Dictionary<string, string>();
            columnMappings.Add("FMOrgId", "FMBookId");
            columnMappings.Add("Name", "CcyIso");
         
            return columnMappings;
        }

        private static Dictionary<string, string> CreateDataSet2ColumnMappings()
        {
            Dictionary<string, string> columnMappings = new Dictionary<string, string>();
            columnMappings.Add("LADDER_DATE", "ReferenceDate");
            columnMappings.Add("BOOK_ID", "FMBookId");
            columnMappings.Add("RFK_ISEC_ID", "FMSecId");
            columnMappings.Add("PL_CCY", "CcyIso");
            columnMappings.Add("INST_CLASS", "FMInstClass");      
            columnMappings.Add("NET_POSITION","NetPosition");
		    columnMappings.Add("UNIT_COST","UnitCost");
		    columnMappings.Add("MARK_PRICE","Price");
		    columnMappings.Add("XRATE","FXRate");
		    columnMappings.Add("MARK_VALUE","MarketValue");
            columnMappings.Add("DELTA_MARK_VALUE", "DeltaMarketValue");
            columnMappings.Add("TOTAL_ACCRUAL", "TotalAccrual");
		    columnMappings.Add("CASH_INCOME","CashIncome");
		    //columnMappings.Add("FX_RLPL","RealisedFXPNL");
		    //columnMappings.Add("PRICE_RLPL","RealisedPricePNL");
            //columnMappings.Add("TOTAL_UNPL", "UnRealisedPNL");
            columnMappings.Add("TOTAL_PNL", "TotalPNL");
            return columnMappings;
        }

        public static DataTable GetKeeleyPositions(int fundId, DateTime fromDate, DateTime toDate)
        {
            DataTable dt = GetNewCVLDataTable();
            DataSetUtilities.FillKeeleyDataTable(dt, "Portfolio_GetForFMRec", CreateDataSet1Parameters(fundId, fromDate, toDate), CreateDataSet1ColumnMappings());
            return dt;
        }

        public static DataTable GetFMPositions(int bftFundId, DateTime fromDate, DateTime toDate)
        {
            DataTable dt = GetNewCVLDataTable();
            DataSetUtilities.FillFMDataTable(dt, "reconcilation.get_cvl_positions", CreateDataSet2Parameters(bftFundId, fromDate, toDate), CreateDataSet2ColumnMappings());
            return dt;
        }
        #endregion

        #region NAV Matching

        private static DataTable GetNewNavDataTable()
        {

            DataTable dt = new DataTable("Navs");      
            DataColumn fundId = dt.Columns.Add("FMFundId", typeof(int));            
            dt.Columns.Add("MarketValue", typeof(decimal));
            dt.PrimaryKey = new DataColumn[] { fundId };
            return dt;

        }

        private static Dictionary<string, object> CreateNavDataSet1Parameters(DateTime referenceDate)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            parameters.Add("@referenceDate", referenceDate);

            return parameters;
        }

        private static Dictionary<string, object> CreateNavDataSet2Parameters(DateTime referenceDate)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            parameters.Add("in_ladder_date", referenceDate);

            return parameters;
        }

        private static Dictionary<string, string> CreateNavDataSet2ColumnMappings()
        {
            Dictionary<string, string> columnMappings = new Dictionary<string, string>();
            columnMappings.Add("FUND_ID", "FMFundId");
            columnMappings.Add("MARKET_VALUE", "MarketValue");        
            return columnMappings;
        }

        public static DataTable GetKeeleyNavs(DateTime referenceDate)
        {
            DataTable dt = GetNewNavDataTable();
            DataSetUtilities.FillKeeleyDataTable(dt, "Portfolio_GetForFMNavRec", CreateNavDataSet1Parameters(referenceDate),null);
            return dt;
        }

        public static DataTable GetFMNavs(DateTime referenceDate)
        {
            DataTable dt = GetNewNavDataTable();
            DataSetUtilities.FillFMDataTable(dt, "reconcilation.get_navs", CreateNavDataSet2Parameters(referenceDate), CreateNavDataSet2ColumnMappings());
            return dt;
        }
        #endregion
    }
}

