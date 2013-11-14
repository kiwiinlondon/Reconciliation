﻿using System;
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
        private const string DataTableName = "Navs";
        private static DataTable GetNewNavDataTable()
        {

            DataTable dt = new DataTable(DataTableName);
            DataColumn fundId = dt.Columns.Add(EZEIdentifierColumnName);
            dt.Columns.Add(MarketValueColumnName, typeof(decimal));
            dt.PrimaryKey = new DataColumn[] { fundId };
            return dt;
        }

        private static Dictionary<string, object> CreateNavDataSet1Parameters(DateTime referenceDate)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            parameters.Add("@referenceDate", referenceDate);

            return parameters;
        }

        private static readonly string KeeleyStoredProcedureName = ConfigurationManager.AppSettings["KeeleyStoredProcedureName"];
        private static readonly string EZEFileName = ConfigurationManager.AppSettings["EZEFileName"];

        public static DataTable GetKeeleyNavs(DateTime referenceDate)
        {
            DataTable dt = GetNewNavDataTable();
            Logger.Info(String.Format("Keeley ReferenceDate is {0}",referenceDate));
            DataSetUtilities.FillKeeleyDataTable(dt, KeeleyStoredProcedureName, CreateNavDataSet1Parameters(referenceDate), null);
            return dt;
        }

        private static Dictionary<int, string> CreateNavDataSet2ColumnMappings()
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
            DataSetUtilities.FillFromFile(dt,fileName,CreateNavDataSet2ColumnMappings());
            return dt;
        }


        public static DataTable GetFMNavs(DateTime referenceDate, List<Fund> funds)
        {
            DataTable dt = GetNewNavDataTable();
            PortfolioClient client = new PortfolioClient();
            Logger.Info(String.Format("FM ReferenceDate is {0}", referenceDate));
            List<BC.FundNAV> navs = client.GetFundNavs(funds.Select(a => a.FMOrgId).ToArray(), referenceDate);
            foreach (BC.FundNAV fundNav in navs)
            {
                DataRow row = dt.NewRow();
                row["FMFundId"] = fundNav.FundId;
                row["MarketValue"] = fundNav.MarketValue;
                dt.Rows.Add(row);
            }
            return dt;
        }

        #region IEzeReconciliation Members


        public static Dictionary<string,decimal> GetFMBookNavs(DateTime referenceDate, out Dictionary<string,int> ezeIdentifierToOutputMapping)
        {
            
            FundClient fundClient = new FundClient();
            List<Fund> funds = fundClient.GetAll().Where(a => a.PositionsExist == true && a.IsActive).ToList();
            //funds.ForEach(a=> ezeIdentifierToFundTypeMapping.Add(a=>a.
            BookClient bookClient = new BookClient();
            List<Book> books = bookClient.GetAll().Where(a => a.FMOrgId.HasValue).ToList();
            ezeIdentifierToOutputMapping = books.Select(g => new { Order = g.Fund.FundTypeId == 7 ? 1 : 0, EzeIdentifier = string.IsNullOrWhiteSpace(g.EZEIdentifier) ? g.Fund.EZEIdentifier : g.EZEIdentifier }).Distinct().ToDictionary(a => a.EzeIdentifier, a => a.Order);
            PortfolioClient client = new PortfolioClient();
            string bookString = string.Join(",", funds.Select(a => a.FMOrgId));

            List<int> fmBookIds = funds.Select(a => a.FMOrgId).ToList();
            //fmBookIds.Add(
            List<BC.BookNAV> navsByBookId = client.GetBookNavs(fmBookIds.ToArray(), referenceDate);
            var tempQuery = navsByBookId.Join(books,
                NAV => NAV.BookId,
                Book => Book.FMOrgId,
                (NAV, Book) => new { EZEIdentifier = string.IsNullOrWhiteSpace(Book.EZEIdentifier) ? Book.Fund.EZEIdentifier : Book.EZEIdentifier, NAV.MarketValue }).ToList()
                ;
            
            return tempQuery.GroupBy(g => g.EZEIdentifier).ToDictionary(a => a.Key, a => a.Sum(b => b.MarketValue));
        }

       
        public List<ThreeWayNavRecOutput> GetThreeWayRecOutput(DateTime referenceDate)
        {
            Logger.Info(String.Format("Reference Date is {0}", referenceDate));
            Dictionary<string, ThreeWayNavRecOutput> output = new Dictionary<string, ThreeWayNavRecOutput>();
            Dictionary<string,int> ezeIdentifierToOutputMapping;
            Dictionary<string, decimal> fmNavs = GetFMBookNavs(referenceDate, out ezeIdentifierToOutputMapping);
            foreach (KeyValuePair<string, decimal> fmNav in fmNavs.OrderBy(a => ezeIdentifierToOutputMapping[a.Key]).ThenBy(a=>a.Key))
            {
                AddToOutput(fmNav.Key, null, fmNav.Value, null, output);
            }
            DataTable ezeNavs = GetEzeNavs();
            AddDataTableToOutput(ezeNavs, output, true);
            DataTable keeleyNavs = GetKeeleyNavs(referenceDate);
            AddDataTableToOutput(keeleyNavs, output, false);

            return output.Values.ToList();
        }

        private void AddDataTableToOutput(DataTable dt, Dictionary<string, ThreeWayNavRecOutput> output,bool isEze)
        {
            foreach (DataRow dr in dt.Rows)
            {
                string ezeIdentifier = dr[EZEIdentifierColumnName].ToString();
                decimal marketValue = decimal.Parse(dr[MarketValueColumnName].ToString());

                decimal? ezeNav = null;
                decimal? keeleyNav = null;
                if (isEze)
                {
                    ezeNav = marketValue;
                }
                else
                {
                    keeleyNav = marketValue;
                }
                AddToOutput(ezeIdentifier, ezeNav, null, keeleyNav, output);
            }
        }



        public void AddToOutput(string ezeIdentifier, decimal? ezeNav, decimal? fmNav, decimal? keelyNav, Dictionary<string, ThreeWayNavRecOutput> outputs)
        {
            ThreeWayNavRecOutput output;
            if (!outputs.TryGetValue(ezeIdentifier, out output))
            {
                output = new ThreeWayNavRecOutput();
                output.Identifier = ezeIdentifier;
                outputs.Add(ezeIdentifier, output);
            }
            if (ezeNav.HasValue)
            {
                output.EZE = ezeNav.Value;
            }

            if (fmNav.HasValue)
            {
                output.FundManager = fmNav.Value;
            }

            if (keelyNav.HasValue)
            {
                output.Keeley = keelyNav.Value;
            }
        }

        #endregion
    }
}