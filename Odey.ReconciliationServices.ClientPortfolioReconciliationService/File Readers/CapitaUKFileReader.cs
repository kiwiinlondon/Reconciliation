using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Odey.ReconciliationServices.ClientPortfolioReconciliationService
{
    public class CapitaUKFileReader : FileReader
    {
        public CapitaUKFileReader(string fileName, int fundId, String[] shareClassIdentifiers)
            : base(fileName, fundId, shareClassIdentifiers)
        {
        }

        protected override string FundColumnName { get { return "Sedol Code"; } }
        protected override string AccountReferenceColumnName { get { return "Investor Code"; } }
        protected override string QuantityColumnName { get { return "Holding"; } }
        protected override string MarketValueColumnName { get { return "Value"; } }

    }
}