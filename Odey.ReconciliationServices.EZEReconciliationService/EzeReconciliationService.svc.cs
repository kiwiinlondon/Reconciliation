using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using Odey.Framework.Infrastructure.Services;
using Odey.ReconciliationServices.Contracts;
using Odey.StaticServices.Clients;
using System.Data;
using Odey.ReconciliationServices.EZEReconciliationService.MatchingEngines;
using GenericParsing;
using System.Configuration;
using BC = Odey.Beauchamp.Contracts;
using Odey.Framework.Keeley.Entities;
using Odey.Beauchamp.Clients;


namespace Odey.ReconciliationServices.EzeReconciliationService
{
    public class EzeReconciliationService : OdeyServiceBase, IEzeReconciliation
    {        
        public MatchingEngineOutput GetMatchedNavs(DateTime referenceDate)
        {
            Logger.Info(String.Format("Reference Date is {0}",referenceDate));
            DataTable dt2 = GetEzeNavs();
            DataTable dt1 = GetKeeleyNavs(referenceDate);
            NavMatchingEngine engine = new NavMatchingEngine(Logger);
            MatchingEngineOutput output = engine.Match(dt1, dt2, MatchTypeIds.Full, false, DataSourceIds.KeeleyPortfolio, DataSourceIds.EZEPortfolio);
            return output;
        }

        private const string EZEIdentifierColumnName = "EZEIdentifier";
        private const string MarketValueColumnName = "MarketValue";
        private const string PercentageDiffColumnName = "PercentageDiff";
        private const string DataTableName = "Navs";
        private static DataTable GetNewNavDataTable()
        {

            DataTable dt = new DataTable(DataTableName);
            DataColumn fundId = dt.Columns.Add(EZEIdentifierColumnName);
            dt.Columns.Add(MarketValueColumnName, typeof(decimal));
            dt.Columns[MarketValueColumnName].DefaultValue = 0;
            dt.Columns.Add(PercentageDiffColumnName, typeof(decimal));
            dt.PrimaryKey = new DataColumn[] { fundId };
            return dt;
        }

        private static Dictionary<string, object> CreateDBParameters(DateTime referenceDate)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            parameters.Add("@referenceDate", referenceDate);

            return parameters;
        }

        private static readonly string KeeleyStoredProcedureName = ConfigurationManager.AppSettings["KeeleyStoredProcedureName"];
        private static readonly string FMStoredProcedureName = ConfigurationManager.AppSettings["FMStoredProcedureName"];
        private static readonly string EZEFileName = ConfigurationManager.AppSettings["EZEFileName"];

        public static DataTable GetKeeleyNavs(DateTime referenceDate)
        {
            DataTable dt = GetNewNavDataTable();
            DataSetUtilities.FillKeeleyDataTable(dt, KeeleyStoredProcedureName, CreateDBParameters(referenceDate), null);
            return dt;
        }

        private static Dictionary<int, string> CreateEZEColumnMappings()
        {
            Dictionary<int, string> columnMappings = new Dictionary<int, string>();
            columnMappings.Add(0, EZEIdentifierColumnName);
            columnMappings.Add(2, MarketValueColumnName);
            return columnMappings;
        }


        public static DataTable GetEzeNavs()
        {
            DataTable dt = GetNewNavDataTable();
            string fileName = EZEFileName;   
            DataSetUtilities.FillFromFile(dt,fileName, CreateEZEColumnMappings());
            return dt;
        }

        public static DataTable GetFMBookNavs(DateTime referenceDate)
        {
            DataTable dt = GetNewNavDataTable();
            DataSetUtilities.FillKeeleyDataTable(dt, FMStoredProcedureName, CreateDBParameters(referenceDate), null);
            return dt;
        }
        

        /// <summary>
        /// load FM Portfolio via the FMPortfolio_GetForEZENavRec stored proc (have already grabbed FM Portfolio to FMPortfolio table)
        /// Then load EZE and Keeley NAVS to compare.
        /// 
        /// Now much shorter list. Only includes funds that we grabbed the FM Portfolio for 
        /// (see Odey.ExtractServices.ExtractRunnerService.AllFundsExtractRunner.Run())
        /// </summary>
        /// <param name="referenceDate"></param>
        /// <returns></returns>
        public List<ThreeWayNavRecOutput> GetThreeWayRecOutput(DateTime referenceDate)
        {
            Logger.Info($"GetThreeWayRecOutput: Reference Date is {referenceDate}");
            Dictionary<string, ThreeWayNavRecOutput> threeWayDict = new Dictionary<string, ThreeWayNavRecOutput>();

            DataTable fmNavs = GetFMBookNavs(referenceDate);
            Logger.Info($"Loaded {fmNavs.Rows.Count} FM funds from Stored Proc '{FMStoredProcedureName}' : {NAVTableToString(fmNavs)}");

            //init dictionary by EZE Id of FM Funds
            foreach (DataRow fmNav in fmNavs.Rows)
            {
                var ezeIdentifier = fmNav[EZEIdentifierColumnName].ToString();
                var fmMarketValue = decimal.Parse(fmNav[MarketValueColumnName].ToString());
                var percentageDiff = decimal.Parse(fmNav[PercentageDiffColumnName].ToString());
                var threeWay = new ThreeWayNavRecOutput
                {
                    Identifier = ezeIdentifier,
                    FundManager = fmMarketValue,
                    DifferencePercentage = Math.Round(percentageDiff*100,2)
                };
                threeWayDict[ezeIdentifier] = threeWay;
            }

            //now fill in EZE and Keeley values. Ignore funds not in FM List above
            DataTable ezeNavs = GetEzeNavs();
            Logger.Info($"Loaded {ezeNavs.Rows.Count} EZE funds from File '{EZEFileName}' : {NAVTableToString(ezeNavs)}");
            AddDataTabletoDict(ezeNavs, threeWayDict, true);

            DataTable keeleyNavs = GetKeeleyNavs(referenceDate);
            Logger.Info($"Loaded {keeleyNavs.Rows.Count} Keeley funds from Stored Proc '{KeeleyStoredProcedureName}' : {NAVTableToString(keeleyNavs)}");
            AddDataTabletoDict(keeleyNavs, threeWayDict, false);

            var ret = threeWayDict.Values.OrderBy(r => r.Identifier).ToList();
            Logger.Info($"GetThreeWayRecOutput: Returning {ret.Count} funds");
            return ret;
        }

        private static string NAVTableToString(DataTable navTable)
        {
            return string.Join(", ", navTable.AsEnumerable().Select(f=> f[EZEIdentifierColumnName]).OrderBy(f=>f).ToList());
        }

        private void AddDataTabletoDict(DataTable dt, Dictionary<string, ThreeWayNavRecOutput> threeWayDict, bool isEze)
        {
            foreach (DataRow dr in dt.Rows)
            {
                string ezeIdentifier = dr[EZEIdentifierColumnName].ToString();
                decimal marketValue = decimal.Parse(dr[MarketValueColumnName].ToString());
                
                ThreeWayNavRecOutput output;
                if (!threeWayDict.TryGetValue(ezeIdentifier, out output))
                {
                    continue; //ignore EZE/Keeley funds not in FM list
                }
                
                if (isEze)
                {
                    output.EZE = marketValue;
                }
                else
                {
                    output.Keeley = marketValue;
                }
            }
        }

        
        
    }
}
