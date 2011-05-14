using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Runtime.Serialization;

namespace Odey.ReconciliationServices.Contracts
{
    [DataContract]
    public class MatchingEngineOutput
    {
        public MatchingEngineOutput() {}

        public MatchingEngineOutput(DataTable dt, DataSourceIds dataSource1, DataSourceIds dataSource2)
        {
            PopulateProperties(dt);
            Outputs = new List<MatchingEngineOutputItem>();
            DataSource1 = dataSource1;
            DataSource2 = dataSource2;
        }

        private void PopulateProperties(DataTable dt)
        {
            Key = new List<MatchingEngineOutputProperty>();
            Properties = new Dictionary<string,MatchingEngineOutputProperty>();
            NonKeyProperties = new List<MatchingEngineOutputProperty>();
            foreach (DataColumn dc in dt.Columns)
            {
                MatchingEngineOutputProperty property = new MatchingEngineOutputProperty(dc.ColumnName,dc.DataType);
                if (dt.PrimaryKey.Contains(dc))
                {
                    Key.Add(property);
                    property.IsPartOfKey = true;
                }
                else
                {
                    NonKeyProperties.Add(property);
                }
                Properties.Add(dc.ColumnName,property);
            }
        }
        [DataMember]
        public List<MatchingEngineOutputProperty> Key { get; private set; }

        [DataMember]
        public List<MatchingEngineOutputProperty> NonKeyProperties { get; private set; }

        [DataMember]
        public Dictionary<string,MatchingEngineOutputProperty> Properties { get; private set; }
        
        [DataMember]
        public List<MatchingEngineOutputItem> Outputs { get; private set; }
        
        [DataMember]
        public DataSourceIds DataSource1 { get; private set; }
        
        [DataMember]
        public DataSourceIds DataSource2 { get; private set; }
    }
}
