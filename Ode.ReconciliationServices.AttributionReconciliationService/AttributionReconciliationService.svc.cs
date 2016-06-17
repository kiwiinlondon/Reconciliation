﻿using Odey.Framework.Keeley.Entities;
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

namespace Odey.ReconciliationServices.AttributionReconciliationService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class AttributionReconciliationService : IAttributionReconcilation
    {

        public void Reconcile(int fundId, DateTime referenceDate)
        {
            using (KeeleyModel context = new KeeleyModel())
            {
                DateTime ytdStartDate = new DateTime(referenceDate.Year, 1, 1);
                DateTime mtdStartDate = new DateTime(referenceDate.Year, referenceDate.Month, 1);

                var administratorPortfolio = context.AdministratorPortfolios.Include(a => a.InstrumentMarket.Instrument.Issuer.LegalEntity).Include(a=>a.Currency).Where(a => a.FundId == fundId && ytdStartDate <= a.ReferenceDate && a.ReferenceDate <= referenceDate).ToList();

                var attributionFunds = context.AttributionFunds.Where(a => a.FundId == fundId && ytdStartDate <= a.ReferenceDate && a.ReferenceDate <= referenceDate).ToDictionary(a => a.ReferenceDate, a => a);

                var ytdOpeningAttributionFund = context.AttributionFunds.OrderByDescending(a=>a.ReferenceDate).FirstOrDefault(a=> a.FundId == fundId && a.ReferenceDate < ytdStartDate);
              
                var mtdOpeningAttributionFund = attributionFunds.OrderByDescending(a => a.Key).FirstOrDefault(a => a.Key < mtdStartDate).Value;

                PortfolioCacheClient client = new PortfolioCacheClient();
                var portfolioCacheResults = client.GetPortfolioExposures(new PortfolioRequestObject()
                {
                    FundIds = new[] { fundId },
                    ReferenceDates = new DateTime[] { referenceDate },
                    Scenarios = new ScenarioRequest[] { },
                    AttributionSourceIds = AttributionSourceIds.Master,
                    AttributionPeriodIds = AttributionPeriodIds.MTD | AttributionPeriodIds.YTD

                });
                Fund fund = context.Funds.Include(a => a.LegalEntity).FirstOrDefault(a => a.LegalEntityID == 741);
                Dictionary<Tuple<int, int>, AttributionReconciliationItem> mtdMatchedItems = Build(fund, administratorPortfolio.Where(a=>a.ReferenceDate >= mtdStartDate).ToList(), attributionFunds, mtdOpeningAttributionFund, context, portfolioCacheResults,AttributionPeriodIds.MTD);
                Dictionary<Tuple<int, int>, AttributionReconciliationItem> ytdMatchedItems = Build(fund, administratorPortfolio, attributionFunds, ytdOpeningAttributionFund, context, portfolioCacheResults, AttributionPeriodIds.YTD);

                FileWriter writer = new FileWriter();
                writer.Write(@"c:\temp\recout.xlsx", mtdMatchedItems, ytdMatchedItems);

            }
        }
                

        private Dictionary<Tuple<int, int>, AttributionReconciliationItem> Build(Fund fund, List<AdministratorPortfolio> administratorPortfolio, Dictionary<DateTime,AttributionFund> attributionFunds, AttributionFund openingAttributionFund, KeeleyModel context,List<PortfolioDTO> portfolioCacheResults,AttributionPeriodIds periodId)
        {
            Dictionary<int, InstrumentMarket> currencyInstrumentMarketByInstrumentId = context.InstrumentMarkets.Include(a => a.Instrument).Where(a => a.Instrument.InstrumentClassID == (int)InstrumentClassIds.Currency)
                    .ToDictionary(a => a.InstrumentID, a => a);
            Dictionary<Tuple<int, int>, AttributionReconciliationItem> matchedItems = new Dictionary<Tuple<int, int>, AttributionReconciliationItem>();

            foreach (var portfolio in administratorPortfolio)
            {
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
                AttributionReconciliationItem matchedItem;
                if (!matchedItems.TryGetValue(key, out matchedItem))
                {
                    matchedItem = new AttributionReconciliationItem(instrumentMarket.IssuerID, portfolio.Currency.IsoCode, instrumentMarket.Instrument.Issuer.Name);
                    matchedItems.Add(key,matchedItem);
                }
                matchedItem.AddAdministrator(fund,portfolio,attributionFunds[portfolio.ReferenceDate],openingAttributionFund, addToOther);
            }
            foreach (var portfolio in portfolioCacheResults)
            {
                Tuple<int, int> key = new Tuple<int, int>(portfolio.IssuerId, portfolio.PositionCurrencyId);
                AttributionReconciliationItem matchedItem;
                if (!matchedItems.TryGetValue(key, out matchedItem))
                {
                    matchedItem = new AttributionReconciliationItem(portfolio.IssuerId, portfolio.PositionCurrency, portfolio.Issuer);
                    matchedItems.Add(key, matchedItem);
                }
                matchedItem.AddPortfolioCache(portfolio, AttributionSourceIds.Master, periodId);
            }
            return matchedItems;
        }
    }    
}
