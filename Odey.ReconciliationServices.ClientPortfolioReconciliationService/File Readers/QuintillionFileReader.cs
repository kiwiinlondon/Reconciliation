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

        protected override Dictionary<string, string> CreateColumnMappings()
        {
            Dictionary<string, string> columnMappings = new Dictionary<string, string>();
            columnMappings.Add("Investormantraid", ClientPortfolioReconciliationService.AccountReferenceColumnName);
            columnMappings.Add(FundColumnName, ClientPortfolioReconciliationService.FundReferenceColumnName);
            columnMappings.Add("Units", ClientPortfolioReconciliationService.QuantityColumnName);
            columnMappings.Add("Totalmv", ClientPortfolioReconciliationService.MarketValueColumnName);

            return columnMappings;
        }

    }
}