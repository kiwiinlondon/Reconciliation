using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Odey.ReconciliationServices.Contracts
{
    [DataContract]
    public class MatchingEngineOutputProperty
    {
        public MatchingEngineOutputProperty(){}

        public MatchingEngineOutputProperty(string propertyName, Type propertyType)
        {
            PropertyName = propertyName;
            PropertyType = propertyType;
        }

        [DataMember]
        public string PropertyName { get; private set; }
        
        [DataMember]
        public bool IsPartOfKey { get; set; }

        public Type PropertyType { get; private set; }

    }
}
