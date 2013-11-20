using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Odey.ReconciliationServices.ClientPortfolioReconciliationService
{
    public class CapitaIrelandFileReader : FileReader
    {
        public CapitaIrelandFileReader(string fileName, int fundId, String[] shareClassIdentifiers)
            : base(fileName, fundId, shareClassIdentifiers)
        {
        }

        protected override string FundColumnName { get { return "ClassDescription"; } }
        protected override string AccountReferenceColumnName { get { return "InvestorMantraID"; } }
        protected override string QuantityColumnName { get { return "EndUnits"; } }
        protected override string MarketValueColumnName { get { return "EndAmount"; } }
                
    }
}