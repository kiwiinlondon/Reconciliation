using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Odey.ReconciliationServices.Contracts
{
    [DataContract]
    public enum MatchOutputTypeIds
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Matched = 1,
        [EnumMember]
        Mismatched = 2,
        [EnumMember]
        MissingFrom1 = 3,
        [EnumMember]
        MissingFrom2 = 4
    }
}
