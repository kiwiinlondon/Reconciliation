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
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class FMPortfolioCollectionService : OdeyServiceBase, IFMPortfolioCollection 
    {

        public void CollectForFMFundId(int fmFundId, DateTime fromDate, DateTime toDate)
        {
            PortfolioClient client = new PortfolioClient();
            
            List<BC.Portfolio> portfolioItems = client.Get(fmFundId, fromDate, toDate);
            using (KeeleyModel context = new KeeleyModel(SecurityCallStackContext.Current))
            {
                List<int> books = context.Books.Where(a => a.Fund.LegalEntity.FMOrgId == fmFundId).Select(a=>a.FMOrgId.Value).ToList();
                Dictionary<Tuple<int,int,DateTime, DateTime?,string>, FMPortfolio> existingPortfolio =
                    context.FMPortfolios.Where(a => books.Contains(a.BookId) && fromDate <= a.ReferenceDate && a.ReferenceDate <= toDate).ToDictionary(k =>
                        new Tuple<int,int,DateTime, DateTime?,string>(k.BookId,k.ISecID,k.ReferenceDate,k.MaturityDate,k.Currency),v=>v);
                foreach(BC.Portfolio portfolio in portfolioItems)
                {
                    Tuple<int, int, DateTime, DateTime?, string> key = new Tuple<int, int, DateTime, DateTime?, string>(portfolio.BookId, portfolio.IsecId, portfolio.LadderDate, portfolio.MaturityDate, portfolio.Currency);
                    FMPortfolio existingPortfolioItem;
                    if (!existingPortfolio.TryGetValue(key, out existingPortfolioItem))
                    {
                        existingPortfolioItem = new FMPortfolio();
                        existingPortfolioItem.BookId = portfolio.BookId;
                        existingPortfolioItem.ISecID = portfolio.IsecId;
                        existingPortfolioItem.ReferenceDate = portfolio.LadderDate;
                        existingPortfolioItem.MaturityDate = portfolio.MaturityDate;
                        existingPortfolioItem.Currency = portfolio.Currency;
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
    }
}
