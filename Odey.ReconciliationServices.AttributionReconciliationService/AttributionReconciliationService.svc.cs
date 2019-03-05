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
using Odey.Framework.Infrastructure.Services;
using Odey.Framework.Keeley.Entities.Enums;
using Odey.Query.Client;
using Odey.Query.Contracts;
using Odey.Query.Contracts.Portfolio;
using Odey.Reporting.Clients;
using Odey.Reporting.Entities;
using Fund = Odey.Framework.Keeley.Entities.Fund;

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
                Fund fund = context.Funds.Include(a => a.LegalEntity).Include(a => a.DealingDateDefinition).FirstOrDefault(a => a.LegalEntityID == fundId);
                decimal mtdReturn;
                decimal ytdReturn;
                decimal dayReturn;
                GetReturns(fund, referenceDate, out mtdReturn, out ytdReturn, out dayReturn);

                decimal mtdTotalContribution;
                decimal ytdTotalContribution;
                decimal dayTotalContribution;
                GetContribution(fund, referenceDate, out mtdTotalContribution, out ytdTotalContribution, out dayTotalContribution);

                var keeleyToMTDReturnComparison = new ReturnComparison(mtdReturn, mtdTotalContribution, 0.1m);
                var keeleyToYTDReturnComparison = new ReturnComparison(ytdReturn, ytdTotalContribution, 0.1m);
                var keeleyToDayReturnComparison = new ReturnComparison(dayReturn, dayTotalContribution, 0.1m);

                EmailWriter writer = new EmailWriter();
                writer.SendEmail(fund, referenceDate, keeleyToMTDReturnComparison, keeleyToYTDReturnComparison, keeleyToDayReturnComparison);
            }
        }

        private void GetReturns(Fund fund, DateTime referenceDate, out decimal mtdReturn, out decimal ytdReturn, out decimal valuationReturn)
        {
            PerformanceClient client = new PerformanceClient();
            int daysPriorToToday = DateTime.Today.Subtract(referenceDate).Days;
            var performaceResults = client.GetPerformanceTable(fund.LegalEntityID.ToString(), "-1", null, daysPriorToToday.ToString(), null, "", "false");

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

            var valuation = performaceResults.FirstOrDefault(a => a.ReturnType == PerformanceReturnTypeIds.SinceLastValuation);
            if (valuation == null)
            {
                valuation = performaceResults.FirstOrDefault(a => a.ReturnType == PerformanceReturnTypeIds.Inception);
            }
            valuationReturn = valuation.Value;
        }

        private void GetContribution(Fund fund, DateTime referenceDate, out decimal keeleyMTDTotalContribution, out decimal keeleyYTDTotalContribution, out decimal keeleyValuationTotalContribution)
        {
            var attributionSource = GetAttributionSource(fund);

            // QueryClient is more effecient than running a report.
            var queryDefinition = new PortfolioQueryDefinition();
            queryDefinition.ReferenceDate = referenceDate;
            queryDefinition.CreateCurrencyRows = false;
            queryDefinition.FundIds = new int[] { fund.LegalEntityID };

            queryDefinition.Dimensions.Add(new DimensionField(DimensionFields.Fund));

            // YTD
            queryDefinition.Measures.Add(new MeasureField(AggregationFields.TotalAttributionReturn)
            {
                AttributionSource = attributionSource,
                Period = AttributionPeriodIds.YTD,
            });

            // MTD
            queryDefinition.Measures.Add(new MeasureField(AggregationFields.TotalAttributionReturn)
            {
                AttributionSource = attributionSource,
                Period = AttributionPeriodIds.MTD,
            });

            // Since Last Valuation
            queryDefinition.Measures.Add(new MeasureField(AggregationFields.TotalAttributionReturn)
            {
                AttributionSource = attributionSource,
                Period = AttributionPeriodIds.SinceLastValuation,
            });

            var client = new QueryClient();
            var results = client.ExecutePortfolioQueries(new PortfolioQueryDefinition[] { queryDefinition });

            keeleyYTDTotalContribution = Convert.ToDecimal(results[0].MeasureValues[0][0])*100;
            keeleyMTDTotalContribution = Convert.ToDecimal(results[0].MeasureValues[0][1])*100;
            keeleyValuationTotalContribution = Convert.ToDecimal(results[0].MeasureValues[0][2])*100;
        }
        
        private static AttributionSourceIds GetAttributionSource(Fund fund)
        {
            // OAR has a cutoff so we need to use valuation
            AttributionSourceIds attributionSource;
            if (fund.DealingDateDefinition.ValuationCutOff != null)
            {
                attributionSource = AttributionSourceIds.Valuation;
            }
            else
            {
                attributionSource = AttributionSourceIds.Master;
            }

            return attributionSource;
        }
    }    
}
