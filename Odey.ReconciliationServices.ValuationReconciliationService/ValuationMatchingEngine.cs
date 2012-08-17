using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using log4net;

namespace Odey.ReconciliationServices.ValuationReconciliationService
{
    public class ValuationMatchingEngine : MatchingEngine
    {
        public ValuationMatchingEngine(ILog logger)
            : base(logger) 
        {
           
        }
    }
}