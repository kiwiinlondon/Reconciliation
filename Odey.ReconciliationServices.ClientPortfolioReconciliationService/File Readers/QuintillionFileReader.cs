using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace Odey.ReconciliationServices.ClientPortfolioReconciliationService
{
    public class QuintillionFileReader : FileReader
    {
        public QuintillionFileReader(string fileName, int fundId, String[] shareClassIdentifiers)
            : base(fileName, fundId, shareClassIdentifiers)
        {
        }

        protected override string FundColumnName { get { return "Classmantraid"; } }
        protected override string AccountReferenceColumnName { get { return "Investormantraid"; } }
        protected override string QuantityColumnName { get { return "Units"; } }
        protected override string MarketValueColumnName { get { return "Totalmv"; } }       

    }
}