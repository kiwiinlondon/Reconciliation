using log4net;
using Odey.ReconciliationServices.Contracts;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace Odey.ReconciliationServices.ClientPortfolioReconciliationService
{

    public class ClientPortfolioMatchingEngine : MatchingEngine
    {
        public ClientPortfolioMatchingEngine(ILog logger)
            : base(logger)
        {

        }

        protected override bool DecimalsMatch(MatchingEngineOutputItem matchingEngineOutputItem, string fieldName, decimal field1, decimal field2)
        {
            if (fieldName == "MarketValue")
            {
                if (base.DecimalsMatch(matchingEngineOutputItem, fieldName, Math.Round(field1, 1), Math.Round(field2, 1)))
                {
                    return true;
                }
                else if (field1!=0 && Math.Abs(field1-field2)/field1<0.000030m)
                {
                    return true;
                }
                return false;
            }
            else
            {
                return base.DecimalsMatch(matchingEngineOutputItem, fieldName, Math.Round(field1, 1), Math.Round(field2, 1));
            }
        }
         

        protected override bool RowCanBeIgnored(DataRow dr)
        {
            return Math.Abs(decimal.Parse(dr["Quantity"].ToString())) <= 0.001m || Math.Abs(decimal.Parse(dr["MarketValue"].ToString())) <= 50;
        }

        
    }
}