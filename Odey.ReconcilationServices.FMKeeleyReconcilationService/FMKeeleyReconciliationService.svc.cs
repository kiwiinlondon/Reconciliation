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
            DataTable dt1 = GetKeeleyPositions(fundId, fromDate, toDate);
            FundClient client = new FundClient();
            Fund fund = client.Get(fundId);
            DataTable dt2 = GetFMPositions(fund.FMOrgId, fromDate, toDate);
            CVLMatchingEngine engine = new CVLMatchingEngine();
            MatchingEngineOutput output = engine.Match(dt1, dt2, MatchTypeIds.Full, returnOnlyMismatches,DataSourceIds.KeeleyPortfolio,DataSourceIds.FMContViewLadder);
            return output;
        }

        #endregion

        private static DataTable _dt = null;
        private static object _dataTableLock = new object();

        private static DataTable GetNewDataTable()
        {
            lock (_dataTableLock)
            {
                if (_dt == null)
                {
                    _dt = new DataTable("Positions");
                    DataColumn refDate = _dt.Columns.Add("ReferenceDate", typeof(DateTime));
                    DataColumn bookId = _dt.Columns.Add("FMBookId", typeof(int));
                    DataColumn fmSecId = _dt.Columns.Add("FMSecId", typeof(int));
                    DataColumn ccyIso = _dt.Columns.Add("CcyIso", typeof(string));
                    _dt.Columns.Add("NetPosition", typeof(decimal));
                    //_dt.Columns.Add("UnitCost", typeof(decimal));
                    _dt.Columns.Add("CurrentPrice", typeof(decimal));
                    _dt.Columns.Add("CurrentFXRate", typeof(decimal));
                    //_dt.Columns.Add("MarketValue", typeof(decimal));
                    _dt.Columns.Add("NotionalMarketValue", typeof(decimal));
                    //_dt.Columns.Add("Accrual", typeof(decimal));
                    //_dt.Columns.Add("CashIncome", typeof(decimal));
                    //_dt.Columns.Add("RealisedFXPNL", typeof(decimal));
                    //_dt.Columns.Add("UnRealisedFXPNL", typeof(decimal));
                    //_dt.Columns.Add("RealisedPricePNL", typeof(decimal));
                    //_dt.Columns.Add("UnRealisedPricePNL", typeof(decimal));
                    _dt.PrimaryKey = new DataColumn[] { refDate, bookId, fmSecId, ccyIso };
                }
                return _dt.Clone();
            }
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
            columnMappings.Add("NET_POSITION","NetPosition");
		    //columnMappings.Add("UNIT_COST","UnitCost");
		    columnMappings.Add("MARK_PRICE","CurrentPrice");
		    columnMappings.Add("XRATE","CurrentFXRate");
		    //columnMappings.Add("MARK_VALUE","MarketValue");
            columnMappings.Add("DELTA_MARK_VALUE", "NotionalMarketValue");
		    //columnMappings.Add("ACCRUAL","Accrual");
		    //columnMappings.Add("CASH_INCOME","CashIncome");
		    //columnMappings.Add("FX_RLPL","RealisedFXPNL");
		    //columnMappings.Add("FX_UNPL","UnRealisedFXPNL");
		    //columnMappings.Add("PRICE_RLPL","RealisedPricePNL");
            //columnMappings.Add("PRICE_UNPL", "UnRealisedPricePNL");

            return columnMappings;
        }

        public static DataTable GetKeeleyPositions(int fundId, DateTime fromDate, DateTime toDate)
        {
            DataTable dt = GetNewDataTable();
            DataSetUtilities.FillKeeleyDataTable(dt, "Portfolio_GetForFMRec", CreateDataSet1Parameters(fundId, fromDate, toDate), CreateDataSet1ColumnMappings());
            return dt;
        }

        public static DataTable GetFMPositions(int bftFundId, DateTime fromDate, DateTime toDate)
        {
            DataTable dt = GetNewDataTable();
            DataSetUtilities.FillFMDataTable(dt, "reconcilation.get_cvl_positions", CreateDataSet2Parameters(bftFundId, fromDate, toDate), CreateDataSet2ColumnMappings());
            return dt;
        }
        
    }
}

