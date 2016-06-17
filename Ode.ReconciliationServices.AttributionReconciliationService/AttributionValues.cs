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
        public decimal DefaultPNL { get; set; }

        public decimal FundAdjustedPricePNL { get; set; }
        public decimal FundAdjustedFXPNL { get; set; }
        public decimal FundAdjustedCarryPNL { get; set; }
        public decimal FundAdjustedOtherPNL { get; set; }
        public decimal FundAdjustedDefaultPNL { get; set; }

        public decimal PriceContribution { get; set; }
        public decimal FXContribution { get; set; }
        public decimal CarryContribution { get; set; }
        public decimal OtherContribution { get; set; }

        public decimal DefaultContribution { get; set; }

        public decimal Total
        {
            get
            {
                return PricePNL + FXPNL + CarryPNL + OtherPNL;
            }
        }

        public decimal FundAdjustedTotal
        {
            get
            {
                return FundAdjustedPricePNL + FundAdjustedFXPNL + FundAdjustedCarryPNL + FundAdjustedOtherPNL;
            }
        }

        public decimal ContributionTotal
        {
            get
            {
                return PriceContribution + FXContribution + CarryContribution + OtherContribution;
            }
        }

    }
}