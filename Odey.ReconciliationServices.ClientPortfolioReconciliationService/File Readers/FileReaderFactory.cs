using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using Odey.Framework.Keeley.Entities.Enums;
using Odey.Framework.Keeley.Entities;


namespace Odey.ReconciliationServices.ClientPortfolioReconciliationService
{
    public static class FileReaderFactory
    {
        public static FileReader Get(string fileName, Fund fund,string[] shareClassIdentifiers)
        {
            switch ((AdministratorIds)fund.AdministratorId.Value)
            {
                case AdministratorIds.Quintillion:
                    return new QuintillionFileReader(fileName, fund.LegalEntityID, shareClassIdentifiers);
                case AdministratorIds.CapitaIRE:
                    return new CapitaIrelandFileReader(fileName, fund.LegalEntityID, shareClassIdentifiers);
                case AdministratorIds.RBCDexia:
                    return new RBCFileReader(fileName, fund.LegalEntityID, shareClassIdentifiers);
                case AdministratorIds.CapitaUK:
                    return new CapitaUKFileReader(fileName, fund.LegalEntityID, shareClassIdentifiers);
                default:
                    throw new ApplicationException(String.Format("Unknown Administrator for fund {0}", fund.Name));
            }
        }
    }
}