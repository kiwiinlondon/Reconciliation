﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Odey.ReconciliationServices;
using System.Data;
using Odey.ReconciliationServices.Contracts;
using Odey.StaticServices.Caches;
using Odey.Framework.Keeley.Entities.Enums;
using Odey.Framework.Keeley.Entities;
using log4net;

namespace Odey.ReconcilationServices.FMKeeleyReconciliationService.MatchingEngines
{
    public class CVLMatchingEngine : MatchingEngine
    {
        public CVLMatchingEngine(ILog logger) : base(logger) { }
        public override bool DecimalsMatch(MatchingEngineOutputItem matchingEngineOutputItem, string fieldName, decimal field1, decimal field2)
        {
            if ((int)matchingEngineOutputItem.KeyValues["FMSecId"] == 2346702 && fieldName.StartsWith("Mark"))
            {
                int i = 0;
            }
            switch (fieldName)
            {
                
                case "NetPosition":
                    return !GreaterThanZero(field1 - field2, (decimal)10);
                case "TotalAccrual":
                case "UnRealisedPNL":
                case "RealisedPricePNL":
                case "MarketValue":                
                case "DeltaMarketValue":
                    return !DifferenceGreaterThanPercentage(Math.Round(field1,0) ,Math.Round(field2,0), .01m,100);
                case "TotalPNL":
                    return GreaterThanZeroIgnoreZeroPositions(matchingEngineOutputItem, fieldName, field1, field2, 9999999999999);
                case "FXRate":
                    return GreaterThanZeroIgnoreZeroPositions(matchingEngineOutputItem, fieldName, field1, field2, 0.1m);
                case "Price":
                    return GreaterThanZeroIgnoreZeroPositions(matchingEngineOutputItem, fieldName, field1, field2, null);
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
            InstrumentMarket instrumentMarket = instrumentMarketFMCache.Get(IdentifierTypeIds.FMSecIdOld, matchingEngineOutputItem.KeyValues["FMSecId"].ToString(), null);            
            if (instrumentMarket.InstrumentClassID == (int)InstrumentClassIds.ForwardFX)
            {
                return !GreaterThanZero(field1 - field2, (decimal)tolerance);
            }
            return false;
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
                return false;
            }
            else
            {
                return false;
            }
        }


        private bool GreaterThanZeroIgnoreZeroPositions(MatchingEngineOutputItem matchingEngineOutputItem, string fieldName, decimal field1, decimal field2, decimal? tolerance)
        {
            
            if (!tolerance.HasValue && base.DecimalsMatch(matchingEngineOutputItem, fieldName, field1, field2))
            {
                return true;
            }
            else if (tolerance.HasValue && !GreaterThanZero(field1 - field2, tolerance.Value))
            {
                return true;
            }

            
            if (!GreaterThanZero((decimal)matchingEngineOutputItem.NonKeyValues["NetPosition"].Value1 , 20))
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
            decimal marketValue,
            decimal accrual
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
            if (GreaterThanZero(marketValue,5)) return false;
            if (GreaterThanZero(accrual, 1)) return false;
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
                    decimal.Parse(dr["MarketValue"].ToString()),
                    decimal.Parse(dr["TotalAccrual"].ToString())
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