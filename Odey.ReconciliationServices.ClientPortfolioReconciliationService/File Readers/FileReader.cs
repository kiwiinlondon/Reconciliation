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
          //  FirstRowIsHeader = true;
        }

        protected virtual string SheetName { get { return null; } }
        protected abstract string FundColumnName { get; }
        protected abstract string AccountReferenceColumnName { get; }
        protected abstract string QuantityColumnName { get; }
        protected abstract string MarketValueColumnName { get; }
        protected virtual bool FirstRowIsHeader { get { return true; } }
        protected virtual string[] FundsToExclude { get { return null; } }

        protected virtual Dictionary<string, string> CreateColumnMappings()
        {
            Dictionary<string, string> columnMappings = new Dictionary<string, string>();
            columnMappings.Add(AccountReferenceColumnName, ClientPortfolioReconciliationService.AccountReferenceColumnName);
            columnMappings.Add(FundColumnName, ClientPortfolioReconciliationService.FundReferenceColumnName);
            columnMappings.Add(QuantityColumnName, ClientPortfolioReconciliationService.QuantityColumnName);
            columnMappings.Add(MarketValueColumnName, ClientPortfolioReconciliationService.MarketValueColumnName);

            return columnMappings;
        }


        private static List<string> CreateGrouping()
        {
            return new List<string>() { ClientPortfolioReconciliationService.AccountReferenceColumnName, ClientPortfolioReconciliationService.FundReferenceColumnName };
        }


        public DataTable GetData()
        {
            DataTable dt = ClientPortfolioReconciliationService.GetPortfolioDataTable();


            Dictionary<string, object[]> fundsToExclude = new Dictionary<string,object[]>();
            if (FundsToExclude!=null)
            {
                fundsToExclude = new Dictionary<string,object[]>(){{FundColumnName,FundsToExclude}};
            }
           
            DataSetUtilities.FillFromExcelFile(FileName,FirstRowIsHeader, SheetName, dt, CreateColumnMappings(), CreateGrouping(), new Dictionary<string, object[]>() { { FundColumnName, ShareClassIdentifiers } },
                fundsToExclude);
            dt.DataSet.EnforceConstraints = true;
            return dt;
        }

    }
}