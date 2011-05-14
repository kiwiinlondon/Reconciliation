using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Odey.ReconciliationServices.Contracts
{
    [DataContract]
    public class MatchingEngineOutputPropertyValue
    {
        public MatchingEngineOutputPropertyValue() { }

        public MatchingEngineOutputPropertyValue(MatchingEngineOutputProperty matchingEngineOutputProperty)
        {
            MatchingEngineOutputProperty = matchingEngineOutputProperty;
        }

        [DataMember]
        public MatchingEngineOutputProperty MatchingEngineOutputProperty { get; private set; }

        [DataMember]
        public MatchOutputTypeIds MatchOutputTypeId { get; set; }

        [DataMember]
        public object Value1 { get; set; }
        
        [DataMember]
        public object Value2 { get; set; }
    }
}
