using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Odey.ReconciliationServices
{
    public enum MatchOutputType
    {
        None = 0,
        Matched = 1,
        MisMatched = 2,
        MissingFrom1 = 3,
        MissingFrom2 = 4
    }
}
