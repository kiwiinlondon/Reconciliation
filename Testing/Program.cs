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

            var s = new ValuationReconciliationService();

            var i = new PortfolioReconciliationItem()
            {
                FXRate = 0.801402m,
                Holding = -60111153.13m,
                InstrumentClassId = 26,
                InstrumentMarketId = 35,
                IsAccrual = false,
                MarketValue = -75007469.27m,
                MaturityDate = DateTime.Parse("30-Jun-2014"),
                Price = 1m,
            };


            var ret = s.MatchPositionsAgainstKeeley(6184, DateTime.Parse("18-Jun-2014"), new List<PortfolioReconciliationItem>(){ i }, null);

            var m = ret.Outputs.Where(a => a.KeyValues.ContainsValue(35)).ToList();
            
            ret = ret;

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
