using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Odey.ReconciliationServices;
using Odey.ReconciliationServices.FMKeeleyReconciliationService;
using Odey.ReconciliationServices.Clients;

namespace Testing
{
    class Program
    {
        static void AddDataRow(DataTable dt, int secId, int fundId, DateTime refDate, string ccy, decimal pos, DateTime matDate, string plCcy, int uSecId)
        {
            DataRow dr1 = dt.NewRow();            
            dr1["securityId"] = secId;
            dr1["FundId"] = fundId;
            dr1["ReferenceDate"] = refDate;
            dr1["Ccy"] = ccy;
            dr1["NetPosition"] = pos;
            dr1["MaturityDate"] = matDate;
            dr1["PlCcy"] = plCcy;
            dr1["UnderlyingSecurityId"] = uSecId;
            dt.Rows.Add(dr1);
        }
        //static void Main(string[] args)
        //{
        //    DataTable dt1 = new DataTable("Test 1");
        //    DataColumn secId = dt1.Columns.Add("securityId",typeof(int));
        //    DataColumn fundId = dt1.Columns.Add("FundId",typeof(int));
        //    DataColumn refDate = dt1.Columns.Add("ReferenceDate",typeof(DateTime));
        //    DataColumn ccy = dt1.Columns.Add("Ccy",typeof(string));

        //    dt1.PrimaryKey = new DataColumn[] { secId, fundId, refDate, ccy };

        //    DataColumn pos = dt1.Columns.Add("NetPosition", typeof(decimal));
        //    DataColumn maturityDate = dt1.Columns.Add("MaturityDate", typeof(DateTime));
        //    DataColumn plCcy = dt1.Columns.Add("PlCcy", typeof(String));
        //    DataColumn uSecId = dt1.Columns.Add("UnderlyingSecurityId", typeof(int));

        //    AddDataRow(dt1,1, 2, DateTime.Today, "USD", new decimal(5.6), DateTime.Now.AddDays(5), "NZD", 3);
        //    DataTable dt2 = dt1.Clone();
        //    AddDataRow(dt2, 1, 2, DateTime.Today, "USD", new decimal(5.6), DateTime.Now.AddDays(5), "NZD", 3);
        //    MatchingEngine engine = new MatchingEngine();
        //    List<MatchingEngineOutput> outputs = engine.Match(dt1, dt2,MatchType.Full);

            

        //}

        static void Main(string[] args)
        {            
            //DataTable dt1 = FMKeeleyReconciliationService.GetKeeleyPositions(27, new DateTime(2010, 1, 1), new DateTime(2010, 1, 31));
            //DataTable dt2 = FMKeeleyReconciliationService.GetFMPositions(2100, new DateTime(2010, 1, 1), new DateTime(2010, 1, 31));

            FMKeeleyReconciliationService service = new FMKeeleyReconciliationService();
            service.GetUnmatchedCVLPositions(741, new DateTime(2012, 2, 16), new DateTime(2012, 2, 21), true);
           // service.GetMatchedNavs(new DateTime(2011, 7, 28));
            //command.Parameters = parameters;
            //SqlParameter fundParam = new SqlParameter("@fundId", SqlDbType.Int);
            //fundParam.Value = fundId;
            //command.Parameters.Add(fundParam);

            //SqlParameter fromParam = new SqlParameter("@fromDt", SqlDbType.DateTime);
            //fromParam.Value = fromDate;
            //command.Parameters.Add(fromParam);

            //SqlParameter toParam = new SqlParameter("@toDt", SqlDbType.DateTime);
            //toParam.Value = toDate;
            //command.Parameters.Add(toParam);
        }
    }
}
