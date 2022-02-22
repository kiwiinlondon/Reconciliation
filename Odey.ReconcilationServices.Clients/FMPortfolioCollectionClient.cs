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
    public class FMPortfolioCollectionClient : OdeyClientBase<IFMPortfolioCollection>, IFMPortfolioCollection
    {
        public void CollectForFMFundId(int orgId, DateTime fromDate, DateTime toDate, bool useNew)
        {
            throw new NotImplementedException();
        }

        public void CollectForFMFundId2(int fmOrgId, DateTime fromDate, DateTime toDate, bool useNew)
        {
            IFMPortfolioCollection proxy = factory.CreateChannel();
            try
            {
                proxy.CollectForFMFundId2(fmOrgId, fromDate, toDate, useNew);
                ((ICommunicationObject)proxy).Close();
            }
            catch
            {
                ((ICommunicationObject)proxy).Abort();
                throw;
            }
        }

        public void CollectForFMFundId(int fmOrgId, DateTime fromDate, DateTime toDate)
        {
            IFMPortfolioCollection proxy = factory.CreateChannel();
            try
            {
                proxy.CollectForFMFundId(fmOrgId, fromDate, toDate);
                ((ICommunicationObject)proxy).Close();
            }
            catch
            {
                ((ICommunicationObject)proxy).Abort();
                throw;
            }
        }

        public void CollectForLatestValuation()
        {
            IFMPortfolioCollection proxy = factory.CreateChannel();
            try
            {
                proxy.CollectForLatestValuation();
                ((ICommunicationObject)proxy).Close();
            }
            catch
            {
                ((ICommunicationObject)proxy).Abort();
                throw;
            }
        }

        public void CollectCustodianAccountPositions(int fmFundId, DateTime referenceDate, bool useNew)
        {
            IFMPortfolioCollection proxy = factory.CreateChannel();
            try
            {
                proxy.CollectCustodianAccountPositions(fmFundId, referenceDate, useNew);
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
