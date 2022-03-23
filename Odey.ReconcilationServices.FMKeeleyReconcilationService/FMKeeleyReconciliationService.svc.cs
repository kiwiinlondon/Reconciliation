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
using Odey.Framework.Infrastructure.ErrorReporting;
using Odey.ReconciliationServices;
using Odey.Framework.Keeley.Entities;
using Odey.StaticServices.Clients;
using Odey.ReconcilationServices.FMKeeleyReconciliationService.MatchingEngines;
using Odey.ReconciliationServices.Clients;
using Odey.ExtractServices.Clients;
using Odey.ExtractServices.Contracts;
using System.Configuration;

namespace Odey.ReconciliationServices.FMKeeleyReconciliationService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    public class FMKeeleyReconciliationService : OdeyServiceBase, IFMKeeleyReconciliation
    {

        private static bool? _useNew;

        private bool UseNew
        {
            get
            {
                if (_useNew == null)
                {
                    var useNewInstance = ConfigurationManager.AppSettings["UseNewFMInstance"];
                    if (useNewInstance == null)
                    {
                        _useNew = false;
                    }
                    else
                    {
                        _useNew = bool.Parse(useNewInstance);
                    }
                }
                return _useNew.Value;
            }
        }

        #region Reconcile CVL Positions 

        public MatchingEngineOutput GetUnmatchedCVLPositions(int fundId, DateTime fromDate, DateTime toDate, bool returnOnlyMismatches)
        {
            
            FundClient client = new FundClient();
            Fund fund = client.Get(fundId);

            var fmOrgId = fund.FMOrgId.Value;
            if (UseNew)
            {
                fmOrgId = fund.LegalEntity.NewFMOrgId.Value;
            }

            DataTable dt2 = GetFMPositions(fmOrgId, fromDate, toDate);
            Logger.Info(String.Format("CVL {0}", dt2.Rows.Count));

            Logger.Info(String.Format("Fund {0}", fundId));
            Logger.Info(String.Format("From Date {0} -> {1}", fromDate, toDate));
            DataTable dt1 = GetKeeleyPositions(fundId, fromDate, toDate);
            Logger.Info(String.Format("Keeeley {0}", dt1.Rows.Count));

            CVLMatchingEngine engine = new CVLMatchingEngine(Logger);
            MatchingEngineOutput output = engine.Match(dt1, dt2, MatchTypeIds.Full, returnOnlyMismatches,DataSourceIds.KeeleyPortfolio,DataSourceIds.FMContViewLadder);
            Logger.Info(String.Format("Outputs {0}", output.Outputs.Count));
            Logger.Info(String.Format("Outputs Mismatched {0}", output.Outputs.Where(a=>a.MatchOutputType== MatchOutputTypeIds.Mismatched).Count()));
            Logger.Info(String.Format("Outputs Missing from 1 {0}", output.Outputs.Where(a=>a.MatchOutputType== MatchOutputTypeIds.MissingFrom1).Count()));
            Logger.Info(String.Format("Outputs Missing from 2 {0}", output.Outputs.Where(a => a.MatchOutputType == MatchOutputTypeIds.MissingFrom2).Count()));
            
            return output;
        }

        #endregion

        #region Get Matched Navs

        //public MatchingEngineOutput GetMatchedNavs(DateTime referenceDate)
        //{
        //    FundClient client = new FundClient();
        //    List<Fund> funds = client.GetAll().Where(a=>a.PositionsExist == true && a.FMOrgId.HasValue).ToList();
        //    DataTable dt2 = GetFMNavs(referenceDate, funds);
        //    DataTable dt1 = GetKeeleyNavs(referenceDate);            
        //    NavMatchingEngine engine = new NavMatchingEngine(Logger);
        //    MatchingEngineOutput output = engine.Match(dt1, dt2, MatchTypeIds.Full, false, DataSourceIds.KeeleyPortfolio, DataSourceIds.FMContViewLadder);
        //    return output;
        //}

        #endregion

        #region CVL Matching

        private static DataTable GetNewCVLDataTable()
        {

            DataTable dt = new DataTable("Positions");
            DataColumn refDate = dt.Columns.Add("ReferenceDate", typeof(DateTime));
            DataColumn bookId = dt.Columns.Add("FMBookId", typeof(int));
            DataColumn fmSecId = dt.Columns.Add("FMSecId", typeof(int));
            DataColumn ccyIso = dt.Columns.Add("CcyIso", typeof(string));
            DataColumn maturityDate = dt.Columns.Add("MaturityDate", typeof(DateTime));
            DataColumn strategyFMCode = dt.Columns.Add("StrategyFMCode", typeof(string));

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
            dt.PrimaryKey = new DataColumn[] { refDate, maturityDate, bookId, fmSecId, ccyIso, strategyFMCode };
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

            parameters.Add("@BFTFundId", fundId);
            parameters.Add("@FromDate", fromDate);
            parameters.Add("@ToDate", toDate);

            return parameters;
        }
        //private static Dictionary<string, object> CreateDataSet2Parameters(int fundId, DateTime fromDate, DateTime toDate)
        //{
        //    Dictionary<string, object> parameters = new Dictionary<string, object>();

        //    parameters.Add("@in_fund_Id", fundId);
        //    parameters.Add("@in_from_Dt", fromDate);
        //    parameters.Add("@in_to_Dt", toDate);

        //    return parameters;
        //}

        private static Dictionary<string, string> CreateDataSet1ColumnMappings()
        {
            Dictionary<string, string> columnMappings = new Dictionary<string, string>();
            columnMappings.Add("FMOrgId", "FMBookId");
         
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
            DataSetUtilities.FillKeeleyDataTable(dt, "Portfolio_GetForFMRec", CreateDataSet1Parameters(fundId, fromDate, toDate), null);
            return dt;
        }

        public static DataTable GetCVLPositions(int bftFundId, DateTime fromDate, DateTime toDate)
        {
            DataTable dt = GetNewCVLDataTable();
            DataSetUtilities.FillKeeleyDataTable(dt, "FMPortfolio_Get", CreateDataSet2Parameters(bftFundId, fromDate, toDate), null);
            return dt;
        }

        public static DataTable GetFMPositions(int bftFundId, DateTime fromDate, DateTime toDate)
        {
            //try a few times before failing
            int errorCount = 0;
            const int maxErrorCount = 3;
            var tryAgain = true;

            while (tryAgain)
            {
                try
                {
                    FMPortfolioCollectionClient client = new FMPortfolioCollectionClient();
                    client.CollectForFMFundId(bftFundId, fromDate, toDate);
                    return GetCVLPositions(bftFundId, fromDate, toDate);
                }
                catch (Exception e)
                {
                    errorCount++;
                    if (errorCount >= maxErrorCount)
                    {
                        tryAgain = false;
                        string errMsg = $"Error Count {errorCount} : Error in FMKeeleyReconciliationService.GetFMPositions calling FMPortfolioCollectionClient.CollectForFMFundId for bftFundId {bftFundId}, fromDate {fromDate}, toDate {toDate}. TOO MANY ERRORS. Not Trying again. : {e.Message}";
                        Logger.ErrorFormat(errMsg, e);
                        new ErrorReportingClient().ReportException("FMKeeleyReconciliationService", errMsg);
                    }
                    else
                    {
                        string errMsg = $"Error Count {errorCount} : Error in FMKeeleyReconciliationService.GetFMPositions calling FMPortfolioCollectionClient.CollectForFMFundId for bftFundId {bftFundId}, fromDate {fromDate}, toDate {toDate}. Trying again. : {e.Message}";
                        Logger.ErrorFormat(errMsg, e);
                        new ErrorReportingClient().ReportException("FMKeeleyReconciliationService", errMsg);
                    }
                }
            }
            //too many errors. 
            throw new Exception($"Too Many Exceptions in FMKeeleyReconciliationService.GetFMPositions calling FMPortfolioCollectionClient.CollectForFMFundId for bftFundId { bftFundId}, fromDate { fromDate}, toDate { toDate}. Throwing Exception.");

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

        public void SendFMAdministratorDifferences()
        {
            Logger.Info("Collecting Positions from FM");
            FMPortfolioCollectionClient client = new FMPortfolioCollectionClient();
            client.CollectForLatestValuation();
            Logger.Info("Finished Collecting Positions from FM");
            SSRSReportRunnerClient reportRunnerClient = new SSRSReportRunnerClient();
            Logger.Info("Sending Differences between FM and Admin");
            reportRunnerClient.Email(new SSRSReport[] { new SSRSReport() { Folder = "Portfolio", Report = "FMNavCheck", OutputTypeId = SSRSOutputTypeIds.MHTML } }, "FMAdminRec@odey.com", "FM vs Admin Report");
            Logger.Info("Finished Sending Differences between FM and Admin");
        }

        //public static DataTable GetFMNavs(DateTime referenceDate, List<Fund> funds)
        //{
        //    DataTable dt = GetNewNavDataTable();
        //    PortfolioClient client = new PortfolioClient();
        //    List<BC.FundNAV> navs = client.GetFundNavs(funds.Select(a=>a.FMOrgId.Value).ToArray(), referenceDate);
        //    foreach (BC.FundNAV fundNav in navs)
        //    {
        //        DataRow row = dt.NewRow();
        //        row["FMFundId"] = fundNav.FundId;
        //        row["MarketValue"] = fundNav.MarketValue;                
        //        dt.Rows.Add(row);
        //    }
        //    return dt;
        //}
        #endregion
    }
}

