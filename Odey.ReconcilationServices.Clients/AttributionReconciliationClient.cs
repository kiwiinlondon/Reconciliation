using Odey.Framework.Infrastructure.Clients;
using Odey.ReconciliationServices.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Odey.ReconciliationServices.Clients
{
    public class AttributionReconciliationClient : OdeyClientBase<IAttributionReconciliation>, IAttributionReconciliation
    {
        public void Reconcile(int fundId, DateTime referenceDate)
        {
            IAttributionReconciliation proxy = factory.CreateChannel();
            try
            {
                 proxy.Reconcile(fundId, referenceDate);
                ((ICommunicationObject)proxy).Close();
            }
            catch
            {
                ((ICommunicationObject)proxy).Abort();
                throw;
            }
        }

        


    }
}
