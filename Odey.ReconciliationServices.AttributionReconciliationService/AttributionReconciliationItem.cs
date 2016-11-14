using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Odey.ReconciliationServices.AttributionReconciliationService
{
    public abstract class AttributionReconciliationItem : IComparable
    {

        public AttributionReconciliationItem(bool isCurrency)
        {
            IsCurrency = isCurrency;
            AdministratorValues = new AttributionValues();
            KeeleyValues = new AttributionValues();
        }
        public AttributionValues AdministratorValues { get; set; }

        public AttributionValues KeeleyValues { get; set; }

        public abstract List<object> Descriptor { get; }

        public abstract List<object> Header { get; }

        public bool IsCurrency { get; private set; }

        public abstract int CompareTo(object obj);
    }
}