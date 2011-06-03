using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Odey.ReconciliationServices;
using System.Data;

namespace Odey.ReconcilationServices.FMKeeleyReconciliationService.MatchingEngines
{
    public class CVLMatchingEngine : MatchingEngine
    {
        protected override bool DecimalsMatch(string fieldName, decimal field1, decimal field2)
        {
            switch (fieldName)
            {
                case "NetPosition":
                    return !GreaterThanZero(field1 - field2,(decimal)1.5);
                case "NotionalMarketValue":
                    return !GreaterThanZero(field1 - field2, (decimal)10);
                default:
                    return base.DecimalsMatch(fieldName, field1, field2);
            }
            
        }

       

        private static bool GreaterThanZero(decimal value,decimal tolerance)
        {
            return (!(-tolerance < value && value < tolerance));
        }
        public static bool IsZero(
            decimal netPosition//,
            //decimal marketValue,
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
            //if (GreaterThanZero(marketValue)) return false;
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
            return IsZero(decimal.Parse(dr["NetPosition"].ToString())//,
                    //decimal.Parse(dr["MarketValue"].ToString()),
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