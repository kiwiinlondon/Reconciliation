using Odey.Framework.Keeley.Entities;
using Odey.PortfolioCache.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Odey.ReconciliationServices.AttributionReconciliationService
{
    public class AttributionReconciliationItem
    {
        public AttributionReconciliationItem(int issuerId, string currency, string name)
        {
            IssuerId = issuerId;
            Currency = currency;
            Name = name;
        }

        public int IssuerId { get; private set; }

        public string Currency { get; private set; }

        public string Name { get; private set; }

        public List<PortfolioDTO> PortfolioCacheDTOs { get; private set; }

        public AttributionValues PortfolioCacheValues { get; private set; }

        public void AddPortfolioCache(PortfolioDTO dto, AttributionSourceIds sourceId, AttributionPeriodIds periodId)
        {
            if (PortfolioCacheDTOs == null)
            {
                PortfolioCacheDTOs = new List<PortfolioDTO>();
                PortfolioCacheValues = new AttributionValues();
            }
            PortfolioCacheDTOs.Add(dto);
            AttributionDTO attribution = dto.Attributions.FirstOrDefault(a => a.AttributionSourceId == sourceId && a.AttributionPeriodId == periodId);
            PortfolioCacheValues.PricePNL += attribution.PricePNL ?? 0;
            PortfolioCacheValues.CarryPNL += attribution.CarryPNL ?? 0;
            PortfolioCacheValues.FXPNL += attribution.FXPNL ?? 0;
            PortfolioCacheValues.OtherPNL += attribution.OtherPNL ?? 0;
        }

        public List<AdministratorPortfolio> AdministratorPortfolioDTOs { get; set; }

        public AttributionValues AdministratorValues { get; set; }

        public void AddAdministrator(AdministratorPortfolio dto)
        {
            if (AdministratorPortfolioDTOs == null)
            {
                AdministratorPortfolioDTOs = new List<AdministratorPortfolio>();
                AdministratorValues = new AttributionValues();
            }
            AdministratorPortfolioDTOs.Add(dto);
            AdministratorValues.PricePNL += (dto.RealisedPricePNL + dto.UnRealisedPricePNL);
            AdministratorValues.CarryPNL += dto.CarryPNL;
            AdministratorValues.FXPNL += (dto.RealisedFXPNL + dto.UnRealisedFXPNL);
            AdministratorValues.OtherPNL += dto.ManagementPerformanceFee;
        }

    }
}