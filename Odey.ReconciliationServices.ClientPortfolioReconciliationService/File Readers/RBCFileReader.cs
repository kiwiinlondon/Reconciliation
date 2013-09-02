using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Odey.ReconciliationServices.ClientPortfolioReconciliationService
{
    public class RBCFileReader : FileReader
    {
        public RBCFileReader(string fileName, int fundId, String[] shareClassIdentifiers)
            : base(fileName, fundId, shareClassIdentifiers)
        {
        }

        protected override string FundColumnName { get { return "F9"; } }
        protected override string AccountReferenceColumnName { get { return "F1"; } }
        protected override string QuantityColumnName { get { return "F14"; } }
        protected override string MarketValueColumnName { get { return "F18"; } }
        protected override bool FirstRowIsHeader  { get { return false; } }
        protected override string[] FundsToExclude { get { return new string[] {"fund-no"}; } }
       

    }
}