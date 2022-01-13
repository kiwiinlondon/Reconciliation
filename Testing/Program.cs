using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Odey.ReconciliationServices;
using Odey.ReconciliationServices.FMKeeleyReconciliationService;
using Odey.ReconciliationServices.Clients;
using Odey.ReconciliationServices.Contracts;
using Odey.ReconciliationServices.EzeReconciliationService;
using Odey.ReconciliationServices.ClientPortfolioReconciliationService;
using Odey.ReconciliationServices.FMPortfolioCollectionService;
using Odey.ReconciliationServices.AttributionReconciliationService;
using System.Data.SqlClient;
using Odey.Framework.Keeley.Entities.Enums;

namespace Testing
{
    class Program
    {

           


        static void Main(string[] args)
        {
            FMPortfolioCollectionClient fMPortfolioCollectionService = new FMPortfolioCollectionClient();
            fMPortfolioCollectionService.CollectForFMFundId2(56777, new DateTime(2021, 9, 30), new DateTime(2021, 9, 30), false);

            //FMPortfolioCollectionService fMPortfolioCollectionService = new FMPortfolioCollectionService();
            
            fMPortfolioCollectionService.CollectForFMFundId2(3755 , new DateTime(2021, 9, 30), new DateTime(2021, 9, 30),true);
            EzeReconciliationService eze = new EzeReconciliationService();
           // eze.GetThreeWayRecOutput(DateTime.Today.AddDays(-3));
//           FMKeeleyReconciliationService servicee = new FMKeeleyReconciliationService();
            FMKeeleyReconcilationClient servicee = new FMKeeleyReconcilationClient();
            servicee.SendFMAdministratorDifferences();
            
            fMPortfolioCollectionService.CollectForLatestValuation();
            
            servicee.GetUnmatchedCVLPositions(10657, new DateTime(2021,07, 13), new DateTime(2021, 07, 13), true);
         //       FMPortfolioCollectionService s = new FMPortfolioCollectionService();
           //     s.CollectForFMFundId(84339, new DateTime(2019, 3, 22), new DateTime(2019, 3, 22));
            //    

            ClientPortfolioReconciliationService s = new ClientPortfolioReconciliationService();
           // ClientPortfolioReconciliationClient s = new ClientPortfolioReconciliationClient();
            var t = s.Reconcile(@"\\App02\FileShare\Quintillion\Client\share_register_by_lot NAT LP 11-06-2020.xls", 10171, new DateTime(2019, 05, 29));

            AttributionReconciliationService service = new AttributionReconciliationService();
            //AttributionReconciliationClient service = new AttributionReconciliationClient();
            //service.Reconcile(5591, new DateTime(2016,01, 5));
            //            service.Reconcile(6184, new DateTime(2016, 10, 28));
            service.Reconcile(3609, new DateTime(2019, 2, 28));
            //service.Reconcile(5591, new DateTime(2016, 3, 31));
            //service.Reconcile(5591, new DateTime(2016, 4, 29));
            //service.Reconcile(5591, new DateTime(2016, 5, 31));
            //service.Reconcile(5591, new DateTime(2016, 6, 30));
            //service.Reconcile(5591, new DateTime(2016, 7, 29));
            //service.Reconcile(5591, new DateTime(2016, 8, 31));
            //service.Reconcile(5591, new DateTime(2016, 9, 30));
            //service.Reconcile(5591, new DateTime(2016, 10, 28));
        }

        static void AddDataRow(DataTable dt, int secId, int fundId, DateTime refDate, string ccy, decimal pos, DateTime matDate, string plCcy, int uSecId)
        {
            DataRow dr1 = dt.NewRow();
            dr1["securityId"] = secId;
            dr1["FundId"] = fundId;
            dr1["ReferenceDate"] = refDate;
            dr1["Ccy"] = ccy;
            dr1["NetPosition"] = pos;
            dr1["MaturityDate"] = matDate;
            dr1["PlCcy"] = plCcy;
            dr1["UnderlyingSecurityId"] = uSecId;
            dt.Rows.Add(dr1);
        }

    }


}
