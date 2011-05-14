using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Runtime.Serialization;

namespace Odey.ReconciliationServices.Contracts
{
    [DataContract]
    public class MatchingEngineOutputItem
    {               
        [DataMember]
        public MatchOutputTypeIds MatchOutputType { get; set; }

        [DataMember]
        public Dictionary<string, object> KeyValues { get; set; }

        [DataMember]
        public Dictionary<string, MatchingEngineOutputPropertyValue> NonKeyValues { get; set; }

        [DataMember]
        public List<MatchingEngineOutputPropertyValue> MismatchedProperties { get; set; }
    }
}
