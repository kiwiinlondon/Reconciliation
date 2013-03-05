using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Odey.ReconciliationServices.Contracts
{
    [DataContract]
    public enum DataSourceIds
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        FMContViewLadder = 1,
        [EnumMember]
        KeeleyPortfolio = 2,
        [EnumMember]
        EZEPortfolio = 3 ,
        [EnumMember]
        ExternalPortfolio = 4,   
        [EnumMember]
        AdministratorClientFile = 5,  
        [EnumMember]
        KeeleyClientPortfolio = 6  
    }
}
