using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace Odey.ReconciliationServices.ClientPortfolioReconciliationService
{
    public abstract class FileReader
    {
        protected string FileName;
        protected int FundId;
        protected String[] ShareClassIdentifiers;
        public FileReader(string fileName, int fundId, String[] shareClassIdentifiers)
        {
            FundId = fundId;
            FileName = fileName;
            ShareClassIdentifiers = shareClassIdentifiers;
        }

        protected abstract  Dictionary<string, string> CreateColumnMappings();
        protected abstract string SheetName { get; }
        protected abstract string FundColumnName { get; }

        private static List<string> CreateGrouping()
        {
            return new List<string>() { ClientPortfolioReconciliationService.AccountReferenceColumnName, ClientPortfolioReconciliationService.FundReferenceColumnName };
        }


        public DataTable GetData()
        {
            DataTable dt = ClientPortfolioReconciliationService.GetPortfolioDataTable();
            DataSetUtilities.FillFromExcelFile(FileName, SheetName, dt, CreateColumnMappings(), CreateGrouping(), new Dictionary<string, object[]>() { { FundColumnName, ShareClassIdentifiers } });
            dt.DataSet.EnforceConstraints = true;
            return dt;
        }

    }
}