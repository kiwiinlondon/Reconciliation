using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Odey.ReconciliationServices.Contracts
{
    [DataContract]
    public class ThreeWayNavRecOutput
    {
        [DataMember]
        public string Identifier { get; set; }

        [DataMember]
        public decimal EZE { get; set; }

        [DataMember]
        public decimal FundManager { get; set; }

        [DataMember]
        public decimal Keeley { get; set; }

        public override string ToString()
        {
            return $"EzeID: {Identifier}, EZE: {EZE:n}, FundManager: {FundManager:n}, Keeley: {Keeley:n}";
        }
    }
}
