using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Odey.ReconciliationServices
{
    [DataContract]
    public enum MatchTypeIds
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Part = 1,
        [EnumMember]
        Full = 2
    }
}
