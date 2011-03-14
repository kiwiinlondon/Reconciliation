using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Odey.ReconciliationServices
{
    public class MatchingEngineOutput
    {
        public MatchingEngineOutput(DataRow dataRow1, DataRow dataRow2, MatchOutputType matchOutputType, List<string> misMatchedFieldNames)
        {
            DataRow1 = dataRow1;
            DataRow2 = dataRow2;
            MatchOutputType = matchOutputType;
            MisMatchedFieldNames = misMatchedFieldNames;
        }
        public DataRow DataRow1 { get; private set; }
        public DataRow DataRow2 { get; private set; }
        public MatchOutputType MatchOutputType { get; private set; }
        public List<string> MisMatchedFieldNames { get; private set; }
    }
}
