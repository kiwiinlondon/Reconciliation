﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Odey.ReconciliationServices;
using Odey.ReconciliationServices.Contracts;
using log4net;
using System.Configuration;

namespace Odey.ReconcilationServices.FMKeeleyReconciliationService.MatchingEngines
{
    public class NavMatchingEngine : MatchingEngine
    {

        public NavMatchingEngine(ILog logger)
            : base(logger)
        {
            
        }

        

        public override bool DecimalsMatch(MatchingEngineOutputItem matchingEngineOutputItem, string fieldName, decimal field1, decimal field2)
        {
            switch (fieldName)
            {
                case "MarketValue":                
                    return !GreaterThanZero(field1 - field2, (decimal)99);
                default:
                    return base.DecimalsMatch(matchingEngineOutputItem, fieldName, field1, field2);
            }

        }
    }
}