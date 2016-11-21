using Odey.Framework.Keeley.Entities;
using Odey.PortfolioCache.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Odey.ReconciliationServices.AttributionReconciliationService
{
    public class AttributionAdminReconciliationItem : AttributionReconciliationItem
    {
        public AttributionAdminReconciliationItem(int issuerId, string currency, string name, bool isCurrency) : base(isCurrency)
        {
            IssuerId = issuerId;
            Currency = currency;
            Name = name;
        }

        public int IssuerId { get; private set; }

        public string Currency { get; private set; }

        public string Name { get; private set; }

        public List<PortfolioDTO> PortfolioCacheDTOs { get; private set; }

       

        public void AddPortfolioCache(PortfolioDTO dto, AttributionSourceIds sourceId, AttributionPeriodIds periodId)
        {
            
            AttributionDTO attribution = dto.Attributions.FirstOrDefault(a => a.AttributionSourceId == sourceId && a.AttributionPeriodId == periodId);
            if (attribution != null)
            {
                if (PortfolioCacheDTOs == null)
                {
                    PortfolioCacheDTOs = new List<PortfolioDTO>();
                    KeeleyValues = new AttributionValues();
                }
                PortfolioCacheDTOs.Add(dto);
                KeeleyValues.AddDTO(attribution);
            }
        }




        public List<AdministratorPortfolio> AdministratorPortfolioDTOs { get; set; }

        public override List<object> Header
        {
            get
            {
                return new List<object>() { "IssuerId", "Name", "Currency" };
            }
        }

        public override List<object> Descriptor
        {
            get
            {
                return new List<object>(){ IssuerId.ToString(), Name, Currency };
            }
        }

        public override string DisplayName
        {
            get
            {
                return Name;
            }
        }

        public void AddAdministrator(Fund fund, AdministratorPortfolio dto,AttributionFund attributionFund, AttributionFund openingAttributionFund, bool addToOther, decimal fxRate)
        {
            if (AdministratorPortfolioDTOs == null)
            {
                AdministratorPortfolioDTOs = new List<AdministratorPortfolio>();
                AdministratorValues = new AttributionValues();
            }
            AdministratorPortfolioDTOs.Add(dto);
            decimal percentageOfFund = dto.IsShareClassSpecific ? 1 : attributionFund.PercentageOfFund ;

            decimal pricePNL = (dto.RealisedPricePNL + dto.UnRealisedPricePNL)/fxRate;
            decimal carryPNL = (dto.CarryPNL + (dto.TodayAmortisationBook ??0))/ fxRate;
            decimal fxPNL = (dto.RealisedFXPNL + dto.UnRealisedFXPNL) / fxRate;
            decimal otherPNL = dto.ManagementPerformanceFee / fxRate;

            decimal fundAdjustedPricePNL = pricePNL / attributionFund.AdjustmentFactor * percentageOfFund;
            decimal fundAdjustedCarryPNL = carryPNL / attributionFund.AdjustmentFactor * percentageOfFund;
            decimal fundAdjustedFXPNL = fxPNL / attributionFund.AdjustmentFactor * percentageOfFund;
            decimal fundAdjustedOtherPNL = otherPNL / attributionFund.AdjustmentFactor * percentageOfFund;

            decimal priceContribution = fundAdjustedPricePNL / openingAttributionFund.AdjustedNav;
            decimal carryContribution = fundAdjustedCarryPNL / openingAttributionFund.AdjustedNav;
            decimal fxContribution = fundAdjustedFXPNL / openingAttributionFund.AdjustedNav;
            decimal otherContribution = fundAdjustedOtherPNL / openingAttributionFund.AdjustedNav;

            if (addToOther)
            {
                decimal amountToAdd = pricePNL +carryPNL + otherPNL;
                decimal fundAdjustedAmountToAdd = fundAdjustedPricePNL + fundAdjustedCarryPNL + fundAdjustedOtherPNL;
                decimal contribitonToAdd = priceContribution + carryContribution + otherContribution;

                if (fund.CurrencyID == dto.CurrencyId)
                {
                    amountToAdd += fxPNL;
                    fundAdjustedAmountToAdd += fundAdjustedFXPNL;
                    contribitonToAdd += fxContribution;
                }
                else
                {
                    AdministratorValues.FXPNL += fxPNL;
                    AdministratorValues.FundAdjustedFXPNL += fundAdjustedFXPNL;
                    AdministratorValues.FXContribution += fxContribution;
                }
                AdministratorValues.OtherPNL += amountToAdd;
                AdministratorValues.FundAdjustedOtherPNL += fundAdjustedAmountToAdd;
                AdministratorValues.OtherContribution += contribitonToAdd;
            }
            else
            {
                AdministratorValues.PricePNL += pricePNL;
                AdministratorValues.CarryPNL += carryPNL;
                AdministratorValues.FXPNL += fxPNL;
                AdministratorValues.OtherPNL += otherPNL;

                AdministratorValues.FundAdjustedPricePNL += fundAdjustedPricePNL;
                AdministratorValues.FundAdjustedCarryPNL += fundAdjustedCarryPNL;
                AdministratorValues.FundAdjustedFXPNL += fundAdjustedFXPNL;
                AdministratorValues.FundAdjustedOtherPNL += fundAdjustedOtherPNL;

                AdministratorValues.PriceContribution += priceContribution;
                AdministratorValues.CarryContribution += carryContribution;
                AdministratorValues.FXContribution += fxContribution;
                AdministratorValues.OtherContribution += otherContribution;
            }

            AdministratorValues.DefaultPNL = AdministratorValues.PricePNL + AdministratorValues.CarryPNL + AdministratorValues.OtherPNL;
            AdministratorValues.FundAdjustedDefaultPNL = AdministratorValues.FundAdjustedPricePNL + AdministratorValues.FundAdjustedCarryPNL + AdministratorValues.FundAdjustedOtherPNL;
            AdministratorValues.DefaultContribution = AdministratorValues.PriceContribution + AdministratorValues.CarryContribution + AdministratorValues.OtherContribution;
        }

        public override int CompareTo(object obj)
        {
            AttributionAdminReconciliationItem objToCompare = (AttributionAdminReconciliationItem)obj;
            int compare = Name.CompareTo(objToCompare.Name);
            if ( compare==0)
            {
                return Currency.CompareTo(objToCompare.Currency);
            }
            return compare;
        }
    }
}