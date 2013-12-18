using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using log4net;
using Odey.ReconciliationServices.Contracts;
using System.Configuration;

namespace Odey.ReconciliationServices.EZEReconciliationService.MatchingEngines
{
    public class NavMatchingEngine : MatchingEngine
    {
        public NavMatchingEngine(ILog logger) : base(logger) 
        {
            Tolerance = decimal.Parse(ConfigurationManager.AppSettings["NavTolerance"]);
        }

        private decimal Tolerance { get; set; }

        public override bool DecimalsMatch(MatchingEngineOutputItem matchingEngineOutputItem, string fieldName, decimal field1, decimal field2)
        {
            switch (fieldName)
            {
                case "MarketValue":
                    return !DifferenceGreaterThanPercentage(field1, field2, Tolerance);
                default:
                    return base.DecimalsMatch(matchingEngineOutputItem, fieldName, field1, field2);
            }

        }
    }
}