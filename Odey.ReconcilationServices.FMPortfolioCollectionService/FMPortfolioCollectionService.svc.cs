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

namespace Odey.ReconciliationServices.FMPortfolioCollectionService
{

    public class FMPortfolioCollectionService : OdeyServiceBase, IFMPortfolioCollection
    {
        /// <summary>
        /// FM Strategy name
        /// </summary>
        private static readonly string STRATEGY_NONE = "NONE";

        public void CollectForFMFundId(int fmFundId, DateTime fromDate, DateTime toDate)
        {
            PortfolioClient client = new PortfolioClient();
            
            List<BC.Portfolio> portfolioItems = client.Get(fmFundId, fromDate, toDate);
            using (KeeleyModel context = new KeeleyModel(SecurityCallStackContext.Current))
            {
                List<int> books = context.Books
                    .Where(a => a.Fund.LegalEntity.FMOrgId == fmFundId).Select(a=>a.FMOrgId.Value)
                    .ToList();

                var existingPortfolio =
                    context.FMPortfolios
                    .Where(a => books.Contains(a.BookId) && fromDate <= a.ReferenceDate && a.ReferenceDate <= toDate)
                    .ToDictionary(k => new Tuple<int,int,DateTime, DateTime?,string, string>(k.BookId,k.ISecID,k.ReferenceDate,k.MaturityDate,k.Currency, k.StrategyFMCode),
                                  v=>v);

                foreach(BC.Portfolio portfolio in portfolioItems)
                {
                    int bookId = GetBookId(portfolio);
                    string strategy = GetStrategy(portfolio);
                    var key = new Tuple<int, int, DateTime, DateTime?, string, string>(bookId, portfolio.IsecId, portfolio.LadderDate, portfolio.MaturityDate, portfolio.Currency, strategy);

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
                            StrategyFMCode = strategy
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
                }
                foreach (FMPortfolio existingPortfolioItem in existingPortfolio.Values)
                {
                    context.FMPortfolios.Remove(existingPortfolioItem);
                }
                context.SaveChanges();
            }
        }


        /// <summary>
        /// map OAR portfolio items with a strategy to BK-OUAR-AC for keeley rec
        /// </summary>
        /// <param name="portfolio"></param>
        /// <returns></returns>
        private int GetBookId(BC.Portfolio portfolio)
        {
            if (portfolio.BookId == 56778 && portfolio.Strategy != STRATEGY_NONE && !string.IsNullOrWhiteSpace(portfolio.Strategy))
            {
                //BK-OUAR -> BK-OUAR-AC
                return 79419;
            }
            if (portfolio.BookId == 78663 && portfolio.Strategy != STRATEGY_NONE && !string.IsNullOrWhiteSpace(portfolio.Strategy))
            {
                //BK-ARFF -> BK-ARFF-AC
                return 79420;
            }
            return portfolio.BookId;
        }

        /// <summary>
        /// Sometimes non-OAR and non-ARFF portfolio items from FM have a strategy
        /// Set them to none here instead.
        /// </summary>
        /// <param name="portfolio"></param>
        /// <returns></returns>
        private string GetStrategy(BC.Portfolio portfolio)
        {
            if (portfolio.BookId == 56778 || portfolio.BookId == 78663 )
            {
                //BK-OUAR or BK-ARFF. Retain strategy
                return portfolio.Strategy;
            }

            return STRATEGY_NONE;
        }
    }
}
