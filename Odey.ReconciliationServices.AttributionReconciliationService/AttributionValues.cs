
using Odey.PortfolioCache.Entities;
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


        public void AddDTO(AttributionDTO attribution)
        {
            PricePNL += attribution.PricePNL ?? 0;
            CarryPNL += attribution.CarryPNL ?? 0;
            FXPNL += attribution.FXPNL ?? 0;
            OtherPNL += attribution.OtherPNL ?? 0;
            DefaultPNL += attribution.PNL;

            FundAdjustedPricePNL += attribution.PriceFundAdjustedPNL ?? 0;
            FundAdjustedCarryPNL += attribution.CarryFundAdjustedPNL ?? 0;
            FundAdjustedFXPNL += attribution.FXFundAdjustedPNL ?? 0;
            FundAdjustedOtherPNL += attribution.OtherFundAdjustedPNL ?? 0;
            FundAdjustedDefaultPNL += attribution.PNLFundAdjusted;

            PriceContribution += attribution.PriceContribution ?? 0;
            CarryContribution += attribution.CarryContribution ?? 0;
            FXContribution += attribution.FXContribution ?? 0;
            OtherContribution += attribution.OtherContribution ?? 0;
            DefaultContribution += attribution.Contribution;
        }

    }
}