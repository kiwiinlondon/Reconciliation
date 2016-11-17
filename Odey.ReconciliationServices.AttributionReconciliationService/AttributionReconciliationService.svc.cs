using Odey.Framework.Keeley.Entities;
using Odey.ReconciliationServices.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Data.Entity;
using Odey.PortfolioCache.Clients;
using Odey.PortfolioCache.Entities;
using Odey.PortfolioCache.Entities.Enums;
using Odey.Framework.Keeley.Entities.Enums;
using System.IO;
using Odey.Reporting.Clients;
using Odey.Reporting.Entities;
using Odey.Framework.Infrastructure.EmailClient;
using Odey.Framework.Infrastructure.Services;

namespace Odey.ReconciliationServices.AttributionReconciliationService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class AttributionReconciliationService : OdeyServiceBase, IAttributionReconciliation
    {
        public void Reconcile(int fundId, DateTime referenceDate)
        {
            using (KeeleyModel context = new KeeleyModel())
            {
                Fund fund = context.Funds.Include(a => a.LegalEntity).FirstOrDefault(a => a.LegalEntityID == fundId);

                ReturnComparison keeleyToMTDReturnComparison;
                ReturnComparison keeleyToYTDReturnComparison;
                ReturnComparison masterToMTDReturnComparison = null;
                ReturnComparison masterToYTDReturnComparison = null;
                ReturnComparison keeleyToAdminMTDComparison = null;
                ReturnComparison keeleyToAdminYTDComparison = null;
                ReturnComparison masterToAdminMTDComparison = null;
                ReturnComparison masterToAdminYTDComparison = null;
                ReturnComparison keeleyToMasterMTDComparison = null;
                ReturnComparison keeleyToMasterYTDComparison = null;


                decimal mtdReturn;
                decimal ytdReturn;
                GetReturns(fund, referenceDate, out mtdReturn, out ytdReturn);
                if (fund.AdministratorId == (int)AdministratorIds.Quintillion)
                {
                    List<AttributionReconciliationItem> mtdMasterMatchedItems;
                    List<AttributionReconciliationItem> ytdMasterMatchedItems;
                    List<AttributionReconciliationItem> mtdKeeleyMatchedItems;
                    List<AttributionReconciliationItem> ytdKeeleyMatchedItems;
                    List<AttributionReconciliationItem> mtdPositions;
                    List<AttributionReconciliationItem> ytdPositions;
                    GetAdministratorMatchedData(context, fund, referenceDate, out mtdMasterMatchedItems, out ytdMasterMatchedItems, out mtdKeeleyMatchedItems, out ytdKeeleyMatchedItems, out mtdPositions, out ytdPositions);

                    string mtdMasterFilePath;
                    string ytdMasterFilePath;
                    string mtdKeeleyFilePath;
                    string ytdKeeleyFilePath;
                    string mtdPositionFilePath;
                    string ytdPositionFilePath;
                    WriteAdministratorFiles(fund, referenceDate, mtdMasterMatchedItems, ytdMasterMatchedItems, mtdKeeleyMatchedItems, ytdKeeleyMatchedItems, mtdPositions, ytdPositions, 
                        out mtdMasterFilePath, out ytdMasterFilePath, out mtdKeeleyFilePath, out ytdKeeleyFilePath, out mtdPositionFilePath, out ytdPositionFilePath);

                    keeleyToMTDReturnComparison = BuildReturnComparison(ReturnType.ActualVsKeeley, mtdReturn, 0.1m, mtdKeeleyMatchedItems);
                    keeleyToYTDReturnComparison = BuildReturnComparison(ReturnType.ActualVsKeeley, ytdReturn, 0.1m, ytdKeeleyMatchedItems);
                    masterToMTDReturnComparison = BuildReturnComparison(ReturnType.ActualVsMaster, mtdReturn, 0.1m, mtdMasterMatchedItems);
                    masterToYTDReturnComparison = BuildReturnComparison(ReturnType.ActualVsMaster, ytdReturn, 0.1m, ytdMasterMatchedItems);
                    keeleyToAdminMTDComparison = BuildReturnComparison(ReturnType.AdminVsKeeley, 0.1m, 1000, mtdKeeleyMatchedItems, mtdKeeleyFilePath);
                    keeleyToAdminYTDComparison = BuildReturnComparison(ReturnType.AdminVsKeeley, 0.1m, 1000, ytdKeeleyMatchedItems, ytdKeeleyFilePath);
                    masterToAdminMTDComparison = BuildReturnComparison(ReturnType.AdminVsMaster, 0.1m, 1000, mtdMasterMatchedItems, mtdMasterFilePath);
                    masterToAdminYTDComparison = BuildReturnComparison(ReturnType.AdminVsMaster, 0.1m, 1000, ytdMasterMatchedItems, ytdMasterFilePath);
                    keeleyToMasterMTDComparison = BuildReturnComparison(ReturnType.MasterVsKeeley, 0.1m, 1000, mtdPositions, mtdPositionFilePath);
                    keeleyToMasterYTDComparison = BuildReturnComparison(ReturnType.MasterVsKeeley, 0.1m, 1000, ytdPositions, ytdPositionFilePath);
                }
                else
                {
                    decimal mtdTotalContribution;
                    decimal ytdTotalContribution;
                    GetNonQuintillionContribution(fund, referenceDate, out mtdTotalContribution, out ytdTotalContribution);
                    keeleyToMTDReturnComparison = new ReturnComparison(mtdReturn, mtdTotalContribution, 0.1m);
                    keeleyToYTDReturnComparison = new ReturnComparison(ytdReturn, ytdTotalContribution, 0.1m);
                }

                EmailWriter writer = new EmailWriter();
                writer.SendEmail(fund, referenceDate, keeleyToMTDReturnComparison, keeleyToYTDReturnComparison, masterToMTDReturnComparison, masterToYTDReturnComparison, keeleyToAdminMTDComparison, keeleyToAdminYTDComparison, masterToAdminMTDComparison, masterToAdminYTDComparison, keeleyToMasterMTDComparison, keeleyToMasterYTDComparison);
            }
        }

        private enum ReturnType
        {
            ActualVsKeeley,
            ActualVsMaster,
            AdminVsKeeley,
            AdminVsMaster,
            MasterVsKeeley,   //position level         
        }



        private decimal GetReturn(List<AttributionReconciliationItem> matchedItems,bool usePortfolioCache)
        {
            decimal sum;
            if (usePortfolioCache)
            {
                 sum = matchedItems.Where(a=> a.KeeleyValues != null).Sum(a => a.KeeleyValues.ContributionTotal);
            }
            else
            {
                sum = matchedItems.Where(a => a.AdministratorValues != null).Sum(a => a.AdministratorValues.ContributionTotal);
            }
            return sum*100m;
        }

        private decimal GetValue(List<AttributionReconciliationItem> matchedItems, bool usePortfolioCache)
        {
            if (usePortfolioCache)
            {
                return matchedItems.Where(a => a.KeeleyValues != null).Sum(a => a.KeeleyValues.Total);
            }
            else
            {
                return matchedItems.Where(a => a.AdministratorValues != null).Sum(a => a.AdministratorValues.Total);
            }
        }

        private ReturnComparison BuildReturnComparison(ReturnType returnType, decimal actualReturn, decimal returnTolerance, List<AttributionReconciliationItem> matchedItems)
        {                       
            decimal correctReturn;
            decimal returnToCompare;
            switch (returnType)
            {
                case ReturnType.ActualVsKeeley:
                    correctReturn = actualReturn;
                    returnToCompare = GetReturn(matchedItems, true);
                    return new ReturnComparison(correctReturn, returnToCompare, returnTolerance);
                case ReturnType.ActualVsMaster:
                    correctReturn = actualReturn;
                    returnToCompare = GetReturn(matchedItems, false);
                    return new ReturnComparison(correctReturn, returnToCompare, returnTolerance);
                default:
                    throw new ApplicationException("Unknown Return Type");

            }
        }


        private ReturnComparison BuildReturnComparison(ReturnType returnType,  decimal returnTolerance, decimal valueTolerance, List<AttributionReconciliationItem> matchedItems, string fileName)
        {
            decimal correctReturn = GetReturn(matchedItems, false);
            decimal returnToCompare = GetReturn(matchedItems, true);
            decimal correctValue = GetValue(matchedItems, false);
            decimal valueToCompare = GetValue(matchedItems, true);

            List<SimpleComparison> currencyDifferences = GetTopDifferences(matchedItems, valueTolerance, true, 5);
            List<SimpleComparison> instrumentDifferences = GetTopDifferences(matchedItems, valueTolerance, false, 5);

            return new ReturnComparison(correctReturn, returnToCompare, returnTolerance, correctValue, valueToCompare, valueTolerance, fileName, currencyDifferences, instrumentDifferences);
        }

        private List<SimpleComparison> GetTopDifferences(List<AttributionReconciliationItem> matchedItems, decimal valueTolerance, bool isCurrency, int numberOfRecordsToReturn)
        {
            return matchedItems.Where(a => a.IsCurrency == isCurrency)
                .OrderByDescending(a => Math.Abs(a.AdministratorValues.Total - a.KeeleyValues.Total))
                .Take(numberOfRecordsToReturn)
                .Select(a => new SimpleComparison(a.DisplayName, a.AdministratorValues.Total, a.KeeleyValues.Total, valueTolerance))
                .ToList();
       }           

        private void GetReturns(Fund fund, DateTime referenceDate, out decimal mtdReturn, out decimal ytdReturn)
        {
            PerformanceClient client = new PerformanceClient();
            int daysPriorToToday = DateTime.Today.Subtract(referenceDate).Days;
            var performaceResults = client.GetPerformanceTable(fund.LegalEntityID.ToString(), null, null, daysPriorToToday.ToString(), null, "", "false");

            var mtd = performaceResults.FirstOrDefault(a => a.ReturnType == PerformanceReturnTypeIds.MTD);
            if (mtd == null)
            {
                mtd = performaceResults.FirstOrDefault(a => a.ReturnType == PerformanceReturnTypeIds.Inception);
            }
            mtdReturn = mtd.Value;
            var ytd = performaceResults.FirstOrDefault(a => a.ReturnType == PerformanceReturnTypeIds.YTD);
            if (ytd == null)
            {
                ytd = performaceResults.FirstOrDefault(a => a.ReturnType == PerformanceReturnTypeIds.Inception);
            }
            ytdReturn = ytd.Value;
        }



        private void GetNonQuintillionContribution(Fund fund, DateTime referenceDate, out decimal keeleyMTDTotalContribution, out decimal keeleyYTDTotalContribution)
        {
            List<PortfolioDTO> results = GetPortfolioCacheResults(fund, referenceDate, AttributionSourceIds.Keeley);
            List<AttributionDTO> mtdResults = new List<AttributionDTO>();
            List<AttributionDTO> ytdResults = new List<AttributionDTO>();
            foreach (var result in results)
            {
                foreach (var attribution in result.Attributions)
                {
                    if (attribution.AttributionPeriodId == AttributionPeriodIds.MTD)
                    {
                        mtdResults.Add(attribution);
                    }
                    else if (attribution.AttributionPeriodId == AttributionPeriodIds.YTD)
                    {
                        ytdResults.Add(attribution);
                    }
                }
            }
            keeleyMTDTotalContribution = mtdResults.Sum(a => a.TotalPNL ?? 0);
            keeleyYTDTotalContribution = ytdResults.Sum(a => a.TotalPNL ?? 0);
        }

        private void WriteAdministratorFiles(Fund fund, DateTime referenceDate,
            List<AttributionReconciliationItem> mtdMasterMatchedItems,
            List<AttributionReconciliationItem> ytdMasterMatchedItems,
            List<AttributionReconciliationItem> mtdKeeleyMatchedItems,
            List<AttributionReconciliationItem> ytdKeeleyMatchedItems,
            List<AttributionReconciliationItem> mtdPositions,
            List<AttributionReconciliationItem> ytdPositions,
            out string mtdMasterFilePath,
            out string ytdMasterFilePath,
            out string mtdKeeleyFilePath,
            out string ytdKeeleyFilePath,
            out string mtdPositionFilePath,
            out string ytdPositionFilePath)
        {
            string path = $@"\\App02\FileShare\Odey\AttributionRecs\{fund.Name}";
            Directory.CreateDirectory(path);
            FileWriter writer = new FileWriter();
            mtdMasterFilePath = $@"{path}\{fund.Name}{referenceDate:yyyyMMdd}.MTD.Master.xlsx";
            writer.Write(mtdMasterFilePath, mtdMasterMatchedItems, null);

            ytdMasterFilePath = $@"{path}\{fund.Name}{referenceDate:yyyyMMdd}.YTD.Master.xlsx";
            writer.Write(ytdMasterFilePath, ytdMasterMatchedItems, null);

            mtdKeeleyFilePath = $@"{path}\{fund.Name}{referenceDate:yyyyMMdd}.MTD.Keeley.xlsx";
            writer.Write(mtdKeeleyFilePath, mtdKeeleyMatchedItems, null);

            ytdKeeleyFilePath = $@"{path}\{fund.Name}{referenceDate:yyyyMMdd}.YTD.Keeley.xlsx";
            writer.Write(ytdKeeleyFilePath, ytdKeeleyMatchedItems, null);

            mtdPositionFilePath = $@"{path}\{fund.Name}{referenceDate:yyyyMMdd}.MTD.KeeleyVsMaster.xlsx";
            writer.Write(mtdPositionFilePath, mtdPositions, null);

            ytdPositionFilePath = $@"{path}\{fund.Name}{referenceDate:yyyyMMdd}.YTD.KeeleyVsMaster.xlsx";
            writer.Write(ytdPositionFilePath, ytdPositions, null);
        }

        private List<PortfolioDTO> GetPortfolioCacheResults(Fund fund,DateTime referenceDate, AttributionSourceIds attributionSourceId)
        {
            PortfolioCacheClient client = new PortfolioCacheClient();
            return client.GetPortfolioExposures(new PortfolioRequestObject()
            {
                FundIds = new[] { fund.LegalEntityID },
                ReferenceDates = new DateTime[] { referenceDate },
                Scenarios = new ScenarioRequest[] { },
                AttributionSourceIds = attributionSourceId,
                AttributionPeriodIds = AttributionPeriodIds.MTD | AttributionPeriodIds.YTD,
            });
        }

        public void GetAdministratorMatchedData(KeeleyModel context, Fund fund, DateTime referenceDate,
            out List<AttributionReconciliationItem> mtdMaster,

            out List<AttributionReconciliationItem> ytdMaster,

            out List<AttributionReconciliationItem> mtdKeeley,

            out List<AttributionReconciliationItem> ytdKeeley,
            
            out List<AttributionReconciliationItem> mtdPositions,
            
            out List<AttributionReconciliationItem> ytdPositions)
        {

            DateTime ytdStartDate = new DateTime(referenceDate.Year, 01, 01);
            //   DateTime ytdStartDate = new DateTime(2015, 11, 30);
            DateTime mtdStartDate = new DateTime(referenceDate.Year, referenceDate.Month, 1);

            var administratorPortfolio = context.AdministratorPortfolios.Include(a => a.InstrumentMarket.Instrument.Issuer.LegalEntity).Include(a => a.Currency).Where(a => a.FundId == fund.LegalEntityID && ytdStartDate <= a.ReferenceDate && a.ReferenceDate <= referenceDate).ToList();


            var officialNavs = context.OfficialNetAssetValues.Where(a => a.FundId == fund.LegalEntityID && ytdStartDate <= a.ReferenceDate && a.ReferenceDate <= referenceDate && a.ValueIsForReferenceDate).ToDictionary(a => a.ReferenceDate, a => a);

            var attributionFunds = context.AttributionFunds.Where(a => a.FundId == fund.LegalEntityID && ytdStartDate <= a.ReferenceDate && a.ReferenceDate <= referenceDate).ToDictionary(a => a.ReferenceDate, a => a);

            var ytdOpeningAttributionFund = context.AttributionFunds.OrderByDescending(a => a.ReferenceDate).FirstOrDefault(a => a.FundId == fund.LegalEntityID && a.ReferenceDate < ytdStartDate);

            var mtdOpeningAttributionFund = attributionFunds.OrderByDescending(a => a.Key).FirstOrDefault(a => a.Key < mtdStartDate).Value;
            if (mtdOpeningAttributionFund == null)
            {
                mtdOpeningAttributionFund = context.AttributionFunds.OrderByDescending(a => a.ReferenceDate).FirstOrDefault(a => a.FundId == fund.LegalEntityID && a.ReferenceDate < mtdStartDate);
            }

            PortfolioCacheClient client = new PortfolioCacheClient();
            var portfolioCacheResults = GetPortfolioCacheResults(fund, referenceDate, AttributionSourceIds.Master | AttributionSourceIds.Keeley);

            var monthlyData = administratorPortfolio.Where(a => a.ReferenceDate >= mtdStartDate).ToList();
            if (mtdOpeningAttributionFund == null)
            {
                mtdOpeningAttributionFund = ytdOpeningAttributionFund;
            }

            mtdMaster = Build(fund, monthlyData, attributionFunds, mtdOpeningAttributionFund, context, portfolioCacheResults, AttributionPeriodIds.MTD, officialNavs, AttributionSourceIds.Master);

            ytdMaster = Build(fund, administratorPortfolio, attributionFunds, ytdOpeningAttributionFund, context, portfolioCacheResults, AttributionPeriodIds.YTD, officialNavs, AttributionSourceIds.Master);

            mtdKeeley = Build(fund, monthlyData, attributionFunds, mtdOpeningAttributionFund, context, portfolioCacheResults, AttributionPeriodIds.MTD, officialNavs, AttributionSourceIds.Keeley);

            ytdKeeley = Build(fund, administratorPortfolio, attributionFunds, ytdOpeningAttributionFund, context, portfolioCacheResults, AttributionPeriodIds.YTD, officialNavs, AttributionSourceIds.Keeley);


            mtdPositions = BuildPositionMatches(portfolioCacheResults, AttributionPeriodIds.MTD);
            ytdPositions = BuildPositionMatches(portfolioCacheResults, AttributionPeriodIds.YTD);           
        }

        private List<AttributionReconciliationItem> BuildPositionMatches(List<PortfolioDTO> portfolioCacheResults, AttributionPeriodIds periodId)
        {
            Dictionary<int, AttributionPositionReconciliationItem> matchedItems = new Dictionary<int, AttributionPositionReconciliationItem>();
            foreach (var portfolio in portfolioCacheResults)
            {
                AttributionPositionReconciliationItem item;
                if (!matchedItems.TryGetValue(portfolio.PositionId, out item))
                {
                    item = new AttributionPositionReconciliationItem(
                        portfolio.PositionId,
                        portfolio.BookName,
                        portfolio.StrategyName,                        
                        portfolio.InstrumentName,
                        portfolio.InstrumentMarketId,
                        portfolio.PositionCurrency,
                        portfolio.AccrualType,
                        portfolio.AccountId,
                        portfolio.InstrumentClassId == (int)InstrumentClassIds.Currency
                        );
                    matchedItems.Add(portfolio.PositionId,item);
                }

                AttributionDTO masterAttribution = portfolio.Attributions.FirstOrDefault(a => a.AttributionSourceId == AttributionSourceIds.Master && a.AttributionPeriodId == periodId);
                if (masterAttribution!=null)
                {
                    item.AdministratorValues.AddDTO(masterAttribution);
                }
                AttributionDTO keeleyAttribution = portfolio.Attributions.FirstOrDefault(a => a.AttributionSourceId == AttributionSourceIds.Keeley && a.AttributionPeriodId == periodId);
                if (keeleyAttribution != null)
                {
                    item.KeeleyValues.AddDTO(keeleyAttribution);
                }
            }
            return matchedItems.Values.ToList<AttributionReconciliationItem>();
        }

        private List<AttributionReconciliationItem> Build(Fund fund, List<AdministratorPortfolio> administratorPortfolio, Dictionary<DateTime,AttributionFund> attributionFunds, AttributionFund openingAttributionFund, KeeleyModel context,List<PortfolioDTO> portfolioCacheResults,AttributionPeriodIds periodId, Dictionary<DateTime, OfficialNetAssetValue> navs,AttributionSourceIds sourceId)
        {
            Dictionary<int, InstrumentMarket> currencyInstrumentMarketByInstrumentId = context.InstrumentMarkets.Include(a => a.Instrument).Where(a => a.Instrument.InstrumentClassID == (int)InstrumentClassIds.Currency)
                    .ToDictionary(a => a.InstrumentID, a => a);
            Dictionary<Tuple<int, int>, AttributionAdminReconciliationItem> matchedItems = new Dictionary<Tuple<int, int>, AttributionAdminReconciliationItem>();

            foreach (var portfolio in administratorPortfolio)
            {
                //InstrumentMarket instrumentMarket;
                //bool addToOther = false;
                //if (!portfolio.InstrumentMarketId.HasValue)
                //{
                //    instrumentMarket = currencyInstrumentMarketByInstrumentId[portfolio.CurrencyId];
                //    addToOther = true;
                //}
                //else
                //{
                //    instrumentMarket = portfolio.InstrumentMarket;
                //}

                //Tuple<int, int> key = new Tuple<int, int>(instrumentMarket.InstrumentMarketID, portfolio.CurrencyId);
                //AttributionAdminReconciliationItem matchedItem;
                //if (!matchedItems.TryGetValue(key, out matchedItem))
                //{
                //    matchedItem = new AttributionAdminReconciliationItem(instrumentMarket.InstrumentMarketID, portfolio.Currency.IsoCode, instrumentMarket.Instrument.Name, instrumentMarket.InstrumentClassIdAsEnum == InstrumentClassIds.Currency);
                //    matchedItems.Add(key,matchedItem);
                //}

                InstrumentMarket instrumentMarket;
                bool addToOther = false;
                if (!portfolio.InstrumentMarketId.HasValue)
                {
                    instrumentMarket = currencyInstrumentMarketByInstrumentId[portfolio.CurrencyId];
                    addToOther = true;
                }
                else
                {
                    instrumentMarket = portfolio.InstrumentMarket;
                }

                Tuple<int, int> key = new Tuple<int, int>(instrumentMarket.IssuerID, portfolio.CurrencyId);
                AttributionAdminReconciliationItem matchedItem;
                if (!matchedItems.TryGetValue(key, out matchedItem))
                {
                    matchedItem = new AttributionAdminReconciliationItem(instrumentMarket.IssuerID, portfolio.Currency.IsoCode, instrumentMarket.Instrument.Issuer.Name, instrumentMarket.InstrumentClassIdAsEnum == InstrumentClassIds.Currency);
                    matchedItems.Add(key, matchedItem);
                }

                OfficialNetAssetValue nav = navs[portfolio.ReferenceDate];
                decimal fxRate = 1;
                if (nav.FXRateToBase.HasValue)
                {
                    fxRate = nav.FXRateToBase.Value;
                }
                
                matchedItem.AddAdministrator(fund,portfolio,attributionFunds[portfolio.ReferenceDate],openingAttributionFund, addToOther, fxRate);
            }
            foreach (var portfolio in portfolioCacheResults)
            {
                if (portfolio.PositionId == 54468)
                {
                    int i = 0;
                }

                Tuple<int, int> key = new Tuple<int, int>(portfolio.IssuerId, portfolio.PositionCurrencyId);
                AttributionAdminReconciliationItem matchedItem;
                if (!matchedItems.TryGetValue(key, out matchedItem))
                {
                    matchedItem = new AttributionAdminReconciliationItem(portfolio.IssuerId, portfolio.PositionCurrency, portfolio.Issuer, portfolio.InstrumentClassId == (int)InstrumentClassIds.Currency);
                    matchedItems.Add(key, matchedItem);
                }
                matchedItem.AddPortfolioCache(portfolio, sourceId, periodId);
            }
            return matchedItems.Values.ToList<AttributionReconciliationItem>();
        }
    }    
}
