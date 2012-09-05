using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Odey.ReconciliationServices.Contracts
{
    [DataContract]
    public class ThreeWayFundNavRecOutput
    {
        [DataMember]
        public int FundId { get; set; }

        [DataMember]
        public object EZE { get; set; }

        [DataMember]
        public object FundManager { get; set; }

        [DataMember]
        public object Keeley { get; set; }
    }
}
