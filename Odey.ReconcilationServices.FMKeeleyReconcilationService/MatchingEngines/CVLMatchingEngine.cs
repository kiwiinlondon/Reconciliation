using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Odey.ReconciliationServices;
using System.Data;
using Odey.ReconciliationServices.Contracts;
using Odey.StaticServices.Caches;
using Odey.Framework.Keeley.Entities.Enums;
using Odey.Framework.Keeley.Entities;

namespace Odey.ReconcilationServices.FMKeeleyReconciliationService.MatchingEngines
{
    public class CVLMatchingEngine : MatchingEngine
    {
        protected override bool DecimalsMatch(MatchingEngineOutputItem matchingEngineOutputItem, string fieldName, decimal field1, decimal field2)
        {
            switch (fieldName)
            {
                
                case "NetPosition":                    
                case "TotalAccrual":
                case "UnRealisedPNL":
                case "RealisedPricePNL":
                case "MarketValue":
                case "TotalPNL":                
                    return !GreaterThanZero(field1 - field2, (decimal)20);
                case "DeltaMarketValue":
                    return GreaterThanZeroIgnoreSipp(matchingEngineOutputItem, fieldName, field1, field2, 20);
                case "FXRate":
                    return GreaterThanZeroIgnoreSipp(matchingEngineOutputItem, fieldName, field1, field2, null);
                default:
                    return base.DecimalsMatch(matchingEngineOutputItem, fieldName, field1, field2);
            }
            
        }



        //If the correct forward rate does not exist in FM FM will use the best fit.
        //However if the rate changes the new rate will not be applied to the position
        private bool ForwardFXCheck(MatchingEngineOutputItem matchingEngineOutputItem, string fieldName, decimal field1, decimal field2, decimal tolerance, decimal? normalFXTolerance)
        {
            if (!normalFXTolerance.HasValue && base.DecimalsMatch(matchingEngineOutputItem, fieldName, field1, field2))
            {
                return true;
            }
            else if (normalFXTolerance.HasValue && !GreaterThanZero(field1 - field2, normalFXTolerance.Value))
            {
                return true;
            }
            InstrumentMarketNoSetupCache instrumentMarketFMCache = new InstrumentMarketNoSetupCache();
            int instrumentMarketId = instrumentMarketFMCache.Get(IdentifierTypeIds.FMSecId, matchingEngineOutputItem.KeyValues["FMSecId"].ToString()).Value;
            InstrumentMarketByIdCache instrumentMarketCache = new InstrumentMarketByIdCache();
            InstrumentMarket instrumentMarket = instrumentMarketCache.Get(instrumentMarketId);
            if (instrumentMarket.InstrumentClassID == (int)InstrumentClassIds.ForwardFx)
            {
                return !GreaterThanZero(field1 - field2, (decimal)tolerance);
            }
            return false;
        }

        private static bool GreaterThanZero(decimal value,decimal tolerance)
        {
            return (!(-tolerance < value && value < tolerance));
        }

        private bool GreaterThanZeroIgnoreSipp(MatchingEngineOutputItem matchingEngineOutputItem, string fieldName, decimal field1, decimal field2, decimal? tolerance)
        {
            if (!tolerance.HasValue && base.DecimalsMatch(matchingEngineOutputItem, fieldName, field1, field2))
            {
                return true;
            }
            else if (tolerance.HasValue && !GreaterThanZero(field1 - field2, tolerance.Value))
            {
                return true;
            }
            if (matchingEngineOutputItem.KeyValues["FMBookId"].ToString() == "11111")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool IsZero(
            decimal netPosition,
            decimal marketValue//,
            //decimal deltaEquityPosition,
            //decimal realisedFXPNL,
            //decimal unRealisedFXPNL,
            //decimal realisedPricePNL,
            //decimal unRealisedPricePNL,
            //decimal accrual,
            //decimal cashIncome
                )
        {
            if (GreaterThanZero(netPosition,1))
            {
                return false;
            }
            if (GreaterThanZero(marketValue,1)) return false;
            //if (GreaterThanZero(deltaEquityPosition)) return false;
            //if (GreaterThanZero(realisedFXPNL)) return false;
            //if (GreaterThanZero(unRealisedFXPNL)) return false;
            //if (GreaterThanZero(realisedPricePNL)) return false;
            //if (GreaterThanZero(unRealisedPricePNL)) return false;
            //if (GreaterThanZero(accrual)) return false;
            //if (GreaterThanZero(cashIncome)) return false;
            return true;
        }

        protected override bool RowCanBeIgnored(DataRow dr)
        {
            return IsZero(decimal.Parse(dr["NetPosition"].ToString()),
                    decimal.Parse(dr["MarketValue"].ToString())//,
                    //decimal.Parse(dr["DeltaEquityPosition"].ToString()),
                    //decimal.Parse(dr["Accrual"].ToString()),
                    //decimal.Parse(dr["CashIncome"].ToString()),
                    //decimal.Parse(dr["RealisedFXPNL"].ToString()),
                    //decimal.Parse(dr["UnRealisedFXPNL"].ToString()),
                    //decimal.Parse(dr["RealisedPricePNL"].ToString()),
                    //decimal.Parse(dr["UnRealisedPricePNL"].ToString())
                    );           
        }
    }
}