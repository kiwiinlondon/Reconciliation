using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Odey.Framework.Infrastructure.Clients;
using Odey.ReconciliationServices.Contracts;
using System.ServiceModel;

namespace Odey.ReconciliationServices.Clients
{
    public class EzeReconciliationClient : OdeyClientBase<IEzeReconciliation>, IEzeReconciliation
    {

        #region IEZEReconciliation Members

        public MatchingEngineOutput GetMatchedNavs(DateTime referenceDate)
        {
            IEzeReconciliation proxy = factory.CreateChannel();
            try
            {
                MatchingEngineOutput e = proxy.GetMatchedNavs(referenceDate);
                ((ICommunicationObject)proxy).Close();
                return e;
            }
            catch
            {
                ((ICommunicationObject)proxy).Abort();
                throw;
            }
        }

        #endregion

        #region IEzeReconciliation Members


        public ThreeWayFundNavRecOutput GetThreeWayRecOutput(DateTime referenceDate)
        {
            IEzeReconciliation proxy = factory.CreateChannel();
            try
            {
                ThreeWayFundNavRecOutput e = proxy.GetThreeWayRecOutput(referenceDate);
                ((ICommunicationObject)proxy).Close();
                return e;
            }
            catch
            {
                ((ICommunicationObject)proxy).Abort();
                throw;
            }
        }

        #endregion
    }
    
}
