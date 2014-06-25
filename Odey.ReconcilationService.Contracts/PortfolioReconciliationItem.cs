using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Odey.ReconciliationServices.Contracts
{
    [DataContract]
    public class PortfolioReconciliationItem
    {
        [DataMember]
        public int InstrumentMarketId { get; set; }

        [DataMember]
        public int FundId { get; set; }


        [DataMember]
        public bool IsAccrual { get; set; }

        [DataMember]
        public int InstrumentClassId { get; set; }

        [DataMember]
        public DateTime MaturityDate { get; set; }

        [DataMember]
        public decimal Holding { get; set; }

        [DataMember]
        public decimal FXRate { get; set; }

        [DataMember]
        public decimal Price { get; set; }

        [DataMember]
        public decimal MarketValue { get; set; }
    }
}
