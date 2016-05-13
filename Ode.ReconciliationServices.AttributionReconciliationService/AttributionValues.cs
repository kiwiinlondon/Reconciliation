using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Odey.ReconciliationServices.AttributionReconciliationService
{
    public class AttributionValues
    {
        public decimal PricePNL {get; set;}
        public decimal FXPNL { get; set; }
        public decimal CarryPNL { get; set; }
        public decimal OtherPNL { get; set; }

        public decimal Total
        {
            get
            {
                return PricePNL + FXPNL + CarryPNL + OtherPNL;
            }
        }

    }
}