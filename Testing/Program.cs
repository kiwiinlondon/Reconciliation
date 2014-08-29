using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Odey.ReconciliationServices;
using Odey.ReconciliationServices.FMKeeleyReconciliationService;
using Odey.ReconciliationServices.Clients;
using Odey.ReconciliationServices.ValuationReconciliationService;
using Odey.ReconciliationServices.Contracts;
using Odey.ReconciliationServices.EzeReconciliationService;
using Odey.ReconciliationServices.ClientPortfolioReconciliationService;

namespace Testing
{
    class Program
    {
        
      
      

        static void Main(string[] args)
        {

            //ClientPortfolioReconciliationService s = new ClientPortfolioReconciliationService();
            //s.Reconcile(@"\\App02\FileShare\CapitaIRE\Giano register Jul 2014.xls",6184,DateTime.Parse("31/07/2014 00:00:00"));
            ValuationReconciliationService s = new ValuationReconciliationService();
            s.MatchPositionsAgainstKeeley(5333, new DateTime(2014, 8, 28),null, new int[0]);


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
