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

namespace Testing
{
    class Program
    {

           


        static void Main(string[] args)
        {


            AttributionReconciliationService service = new AttributionReconciliationService();
            //AttributionReconciliationClient service = new AttributionReconciliationClient();
            service.Reconcile(7504, new DateTime(2016,10, 25));
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
