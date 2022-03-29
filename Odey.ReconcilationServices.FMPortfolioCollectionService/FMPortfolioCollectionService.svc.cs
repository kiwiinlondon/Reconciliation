using Odey.Beauchamp.Clients;
using BC=Odey.Beauchamp.Contracts;
using Odey.Framework.Infrastructure.Services;
using Odey.Framework.Keeley.Entities;
using Odey.ReconciliationServices.Contracts;
using ServiceModelEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Data.Entity;
using Odey.Framework.Keeley.Entities.Enums;
using System.Configuration;

namespace Odey.ReconciliationServices.FMPortfolioCollectionService
{

    public class FMPortfolioCollectionService : OdeyServiceBase, IFMPortfolioCollection
    {
        /// <summary>
        /// FM Strategy name
        /// </summary>
        private static readonly string STRATEGY_NONE = "NONE";

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


        public void CollectForLatestValuation()
        {
            using (KeeleyModel context = new KeeleyModel(SecurityCallStackContext.Current))
            {                
                List<(int FMFundId, DateTime ReferenceDate)> funds;
                if (UseNew)
                {
                    funds = context.OfficialNetAssetValues.Include(a => a.Fund.LegalEntity)
                        .Where(a => a.Fund.IsActive && a.Fund.AdministratorId == (int)AdministratorIds.Quintillion && a.Fund.DealingDateDefinition.PeriodicityId == (int)PeriodicityIds.Daily && a.Fund.LegalEntity.NewFMOrgId.HasValue && a.ValueIsForReferenceDate && !a.Fund.ParentFundId.HasValue && a.FundId != (int)FundIds.DYSS).ToList()
                        .GroupBy(a => a.Fund.LegalEntity.NewFMOrgId.Value)
                        .Select(a => (a.Key, a.Max(m => m.ReferenceDate))).ToList();
                }
                else
                {
                    funds = context.OfficialNetAssetValues.Include(a => a.Fund.LegalEntity)
                        .Where(a => a.Fund.IsActive && a.Fund.AdministratorId == (int)AdministratorIds.Quintillion && a.Fund.DealingDateDefinition.PeriodicityId == (int)PeriodicityIds.Daily && a.Fund.LegalEntity.FMOrgId.HasValue && a.ValueIsForReferenceDate && !a.Fund.ParentFundId.HasValue && a.FundId != (int)FundIds.DYSS).ToList()
                        .GroupBy(a => a.Fund.LegalEntity.FMOrgId.Value)
                        .Select(a => ( a.Key, a.Max(m => m.ReferenceDate) )).ToList();
                }
                foreach (var fund in funds)
                {
                    CollectForFMFundId(fund.FMFundId, fund.ReferenceDate, fund.ReferenceDate);
                }
            }
        
        }
        public void CollectForFMFundId(int fmFundId, DateTime fromDate, DateTime toDate)
        {
            CollectForFMFundId2(fmFundId, fromDate, toDate, UseNew);
        }


        public void CollectCustodianAccountPositions(int fmFundId, DateTime referenceDate, bool useNew)
        {
            BC.IPortfolio client = new PortfolioClient(useNew);

            var portfolioItems = client.GetCustodianPortfolio(fmFundId, referenceDate);
            using (KeeleyModel context = new KeeleyModel(SecurityCallStackContext.Current))
            {
                List<int> books;
                if (useNew)
                {
                    books = context.Books
                    .Where(a => a.Fund.LegalEntity.NewFMOrgId == fmFundId && a.NewFMOrgId.HasValue).Select(a => a.NewFMOrgId.Value)
                    .ToList();
                    if (fmFundId == 3638)
                    {
                        books.Add(4344);
                    }
                    if (fmFundId == 3586)
                    {
                        books.Add(4364);
                    }
                    if (fmFundId == 3582)
                    {
                        books.Add(4365);
                    }
                    if (fmFundId == 3379)
                    {
                        books.Add(4352);
                    }
                    if (fmFundId == 3629)
                    {
                        books.Add(4313);
                    }
                }
                else
                {
                    books = context.Books
                    .Where(a => a.Fund.LegalEntity.FMOrgId == fmFundId && a.FMOrgId.HasValue).Select(a => a.FMOrgId.Value)
                    .ToList();
                }

                var existingPortfolio =
                    context.FMCustodianPortfolios
                    .Where(a => books.Contains(a.BookId) && a.ReferenceDate == referenceDate)
                    .ToDictionary(k => (k.BookId, k.IsecId, k.ReferenceDate, k.Currency, k.Strategy,k.AccountId,k.CustodianId),
                                  v => v);

                foreach (var portfolio in portfolioItems)
                {
                    var key = (portfolio.BookId, portfolio.IsecId, portfolio.LadderDate, portfolio.Currency, portfolio.Strategy, portfolio.AccountId, portfolio.CustodianId);


                    if (!existingPortfolio.TryGetValue(key, out var existingPortfolioItem))
                    {
                        existingPortfolioItem = new FMCustodianPortfolio
                        {
                            BookId = portfolio.BookId,
                            IsecId = portfolio.IsecId,
                            ReferenceDate = portfolio.LadderDate,
                            Currency = portfolio.Currency,
                            Strategy = portfolio.Strategy,
                            AccountId = portfolio.AccountId,
                            CustodianId = portfolio.CustodianId
                        };
                        context.FMCustodianPortfolios.Add(existingPortfolioItem);
                    }
                    else
                    {
                        existingPortfolio.Remove(key);
                    }
                    existingPortfolioItem.NetPosition = Math.Round(portfolio.NetPosition, 8);
                }
                foreach (var existingPortfolioItem in existingPortfolio.Values)
                {
                    context.FMCustodianPortfolios.Remove(existingPortfolioItem);
                }
                context.SaveChanges();
            }
        }

        public void CollectForFMFundId2(int fmFundId, DateTime fromDate, DateTime toDate,bool useNew)
        {
            BC.IPortfolio client = new PortfolioClient(useNew);
            
            List<BC.Portfolio> portfolioItems = client.Get(fmFundId, fromDate, toDate);
            using (KeeleyModel context = new KeeleyModel(SecurityCallStackContext.Current))
            {
                List<int> books;
                if (useNew)
                {
                    books = context.Books
                    .Where(a => a.Fund.LegalEntity.NewFMOrgId == fmFundId && a.NewFMOrgId.HasValue).Select(a => a.NewFMOrgId.Value)
                    .ToList();
                    if (fmFundId == 3638)
                    {
                        books.Add(4344);
                    }
                    if (fmFundId == 3586)
                    {
                        books.Add(4364);
                    }
                    if (fmFundId == 3582)
                    {
                        books.Add(4365);
                    }
                    if (fmFundId == 3379)
                    {
                        books.Add(4352);
                    }
                    if (fmFundId == 3629)
                    {
                        books.Add(4313);
                    }
                }
                else
                {
                    books = context.Books
                    .Where(a => a.Fund.LegalEntity.FMOrgId == fmFundId && a.FMOrgId.HasValue).Select(a => a.FMOrgId.Value)
                    .ToList();
                }
          
                var existingPortfolio =
                    context.FMPortfolios
                    .Where(a => books.Contains(a.BookId) && fromDate <= a.ReferenceDate && a.ReferenceDate <= toDate)
                    .ToDictionary(k => new Tuple<int,int,DateTime, DateTime?,string, string>(k.BookId,k.ISecID,k.ReferenceDate,k.MaturityDate,k.Currency, k.StrategyFMCode),
                                  v=>v);
                
                foreach (BC.Portfolio portfolio in portfolioItems)
                {
                    if (portfolio.Strategy!="NONE")
                    {
                        int a = 1;
                    }
                    int bookId = GetBookId(portfolio);
                    var key = new Tuple<int, int, DateTime, DateTime?, string, string>(bookId, portfolio.IsecId, portfolio.LadderDate, portfolio.MaturityDate, portfolio.Currency, portfolio.Strategy);

                    FMPortfolio existingPortfolioItem;
                    if (!existingPortfolio.TryGetValue(key, out existingPortfolioItem))
                    {
                        existingPortfolioItem = new FMPortfolio
                        {
                            BookId = bookId,
                            ISecID = portfolio.IsecId,
                            ReferenceDate = portfolio.LadderDate,
                            MaturityDate = portfolio.MaturityDate,
                            Currency = portfolio.Currency,
                            StrategyFMCode = portfolio.Strategy
                        };
                        context.FMPortfolios.Add(existingPortfolioItem);
                    }
                    else
                    {
                        existingPortfolio.Remove(key);
                    }
                    existingPortfolioItem.DeltaMarketValue = Math.Round(portfolio.DeltaMarkValue,8);
                    existingPortfolioItem.FXRate = Math.Round(portfolio.FXRate,8);
                    existingPortfolioItem.MarketValue = Math.Round(portfolio.MarkValue,8);
                    existingPortfolioItem.NetPosition = Math.Round(portfolio.NetPosition,8);
                    existingPortfolioItem.Price = Math.Round(portfolio.Price,8);
                    existingPortfolioItem.TotalAccrual = Math.Round(portfolio.TotalAccrual, 8);
                    existingPortfolioItem.Isin = CleanIdentifier(portfolio.Isin);
                    existingPortfolioItem.Sedol = CleanIdentifier(portfolio.Sedol);
                    existingPortfolioItem.BloombergTicker = portfolio.BloombergTicker;
                }
                foreach (FMPortfolio existingPortfolioItem in existingPortfolio.Values)
                {
                    context.FMPortfolios.Remove(existingPortfolioItem);
                }
                context.SaveChanges();
            }
        }

        private string CleanIdentifier(string identifier)
        {
            if (!string.IsNullOrWhiteSpace(identifier))
            {
                var space = identifier.IndexOf(" ");
                if (space>0)
                {
                    identifier = identifier.Substring(0, space);
                }   
            }
            return identifier;
        }

        /// <summary>
        /// map OAR portfolio items with a strategy to BK-OUAR-AC for keeley rec
        /// </summary>
        /// <param name="portfolio"></param>
        /// <returns></returns>
        private int GetBookId(BC.Portfolio portfolio)
        {

            //if (portfolio.BookId == 56778 && portfolio.Strategy != STRATEGY_NONE && !string.IsNullOrWhiteSpace(portfolio.Strategy))
            //{
            //    //BK-OUAR -> BK-OUAR-AC
            //    return 79419;
            //}
            //if (portfolio.BookId == 78663 && portfolio.Strategy != STRATEGY_NONE && !string.IsNullOrWhiteSpace(portfolio.Strategy))
            //{
            //    //BK-ARFF -> BK-ARFF-AC
            //    return 79420;
            //}

            //if (portfolio.BookId == 80460 && portfolio.Strategy != STRATEGY_NONE && !string.IsNullOrWhiteSpace(portfolio.Strategy))
            //{
            //    //BK-ARFF -> BK-ARFF-AC
            //    return -9999;
            //}

            //if (portfolio.BookId == 70380 && portfolio.Strategy != STRATEGY_NONE && !string.IsNullOrWhiteSpace(portfolio.Strategy))
            //{
            //    //BK-DEVM -> BK-DEVM-AC
            //    return -8888;
            //}
            //if (portfolio.BookId == 78506 && portfolio.Strategy != STRATEGY_NONE && !string.IsNullOrWhiteSpace(portfolio.Strategy))
            //{
            //    //BK-FDXH -> BK-FDXH-AC
            //    return -7777;
            //}
            //if (portfolio.BookId == 84340 && portfolio.Strategy != STRATEGY_NONE && !string.IsNullOrWhiteSpace(portfolio.Strategy))
            //{
            //    //BK-IAR -> BK-IAR-AC
            //    return -6666;
            //}

            return portfolio.BookId;
        }
        
    }
}
