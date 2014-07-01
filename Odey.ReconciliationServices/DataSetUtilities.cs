using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.Common;
using GenericParsing;
using System.Data.OleDb;

namespace Odey.ReconciliationServices
{
    public class DataSetUtilities
    {
        private static string GetConnectionString(string name)
        {
            ConnectionStringSettings connectionStringSettings = ConfigurationManager.ConnectionStrings[name];
            //string connectionString = ConfigurationManager.ConnectionStrings[name].ConnectionString;
            if (connectionStringSettings == null)
            {
                throw new ApplicationException(String.Format("Connection string {0} was not provided", name));
            }
            return connectionStringSettings.ConnectionString;
        }

        #region FM Connection String
        private static string FMConnectionString

        {
            get
            {
                return GetConnectionString("FM");
            }
        }
        #endregion

        #region Keeley Connection String
        private static string KeeleyConnectionString
        {
            get
            {
                return GetConnectionString("Keeley");
            }
        }
        #endregion

        #region Add Parameters
        private static void AddParameters(SqlCommand command, Dictionary<string,object> parameters)
        {
            if (parameters!=null)
            {
                foreach (KeyValuePair<string,object> parameter in parameters)
                {
                    SqlParameter param = new SqlParameter(parameter.Key,parameter.Value);
                    command.Parameters.Add(param);
                }
            }
        }
        #endregion 

        #region Add Column Mappings
        private static void AddColumnMappings(DataTableMapping mapping, Dictionary<string, string> columnMappings)
        {
            if (mapping != null && columnMappings != null)
            {
                foreach (KeyValuePair<string, string> columnMapping in columnMappings)
                {
                    mapping.ColumnMappings.Add(string.Format("[{0}]",columnMapping.Key), columnMapping.Value);
                }
            }
        }
        #endregion

        #region Fill Keeley Data Table
        public static void FillKeeleyDataTable(DataTable dt, string storedProcName, Dictionary<string,object> parameters, Dictionary<string,string> columnMappings)
        {
            if (dt.DataSet == null)
            {
                DataSet ds = new DataSet();
                ds.Tables.Add(dt);
            }            
            using (SqlConnection connection = new SqlConnection(DataSetUtilities.KeeleyConnectionString))
            {
                SqlCommand command = new SqlCommand(storedProcName, connection);
                SqlDataAdapter adapter = new SqlDataAdapter(command);
                command.CommandType = CommandType.StoredProcedure;
                AddParameters(command, parameters);
                DataTableMapping mapping = adapter.TableMappings.Add("Table", dt.TableName);
                AddColumnMappings(mapping, columnMappings);
                connection.Open();
                
                adapter.FillSchema(dt.DataSet, SchemaType.Mapped);
                adapter.Fill(dt.DataSet);

            }
        }
        #endregion

        #region Fill From Excel File
        private static string AddAggregator(string selectText,string columnAlias, List<string> columnsToGroupBy)
        {
            if (columnsToGroupBy!= null && !columnsToGroupBy.Contains(columnAlias))
            {
                return String.Format("Sum([{0}])", selectText);
            }
            return String.Format("[{0}]",selectText);
        }

        private static DataTable GetDataSetSchema(string connectionString)
        {
            using (OleDbConnection conObj = new OleDbConnection(connectionString))
            {
                if (conObj.State != ConnectionState.Open) conObj.Open();
                return conObj.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
            }
        }

        public static void FillFromExcelFile(string fileName, bool firstRowIsHeader, string worksheetName, DataTable dt, Dictionary<string, string> columnMappings, List<string> columnsToGroupBy, Dictionary<string, object[]> inclusionColumnsAndValues, Dictionary<string, object[]> exclusionColumnsAndValues)
        {

            string selectClause = string.Join(",", columnMappings.Select(a => String.Format("{0} as {1}", AddAggregator(a.Key, a.Value, columnsToGroupBy), a.Value)));

            string headerClause = "YES";
            if (!firstRowIsHeader)
            {
                headerClause = "NO";
            }
            string connectionString = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=\"Excel 8.0;IMEX=1;HDR={1};\"", fileName,headerClause);

            if (string.IsNullOrWhiteSpace(worksheetName))
            {
                DataTable schema = GetDataSetSchema(connectionString);
                DataRow row = schema.Rows[0];
                worksheetName = row["TABLE_NAME"].ToString();
                if (worksheetName.EndsWith("$"))
                {
                    worksheetName = worksheetName.Substring(0,worksheetName.Length-1);
                }
                else
                {
                    worksheetName = worksheetName.Substring(1, worksheetName.Length - 3);
                }
            }
            string query = String.Format("SELECT {0} FROM [{1}$]", selectClause, worksheetName);

            if (columnsToGroupBy != null)
            {
                string whereClause = " Where";
                AddWhereClause(ref query, ref whereClause, inclusionColumnsAndValues, true);
                AddWhereClause(ref query, ref whereClause, exclusionColumnsAndValues, false);
                string groupByClause = string.Join(",", columnMappings.Where(a => columnsToGroupBy.Contains(a.Value)).Select(a => String.Format("[{0}]", a.Key)));
                query = string.Format("{0} group by {1}", query, groupByClause);
            }

            //Sum([balance at date]) as Quantity,Sum([eval]) as MarketValue

           // query = "SELECT [accout-no] as AccountReference,[fund-no] as Fund,Sum([balance at date]) as Quantity,Sum([eval]) as MarketValue FROM [Sheet1$] Where [fund-no] in ('OD1')  group by [accout-no],[fund-no]";
           // query = "select * from [Sheet1$]";
           // string query = "SELECT fund_id FROM [share_register_by_lot$] ";
            var adapter = new OleDbDataAdapter(query, connectionString);

            //"select * frfunds_fundid & '~' & trans_holderid & '~' & trans_acctid"
            DataTableMapping mapping = adapter.TableMappings.Add("Table", dt.TableName);
            AddColumnMappings(mapping, columnMappings);
            adapter.FillSchema(dt.DataSet, SchemaType.Mapped);
            adapter.Fill(dt.DataSet);            
        }

        #endregion

        private static void AddWhereClause(ref string query, ref string whereClause, Dictionary<string, object[]> columnsAndValues, bool include)
        {
            if (columnsAndValues != null && columnsAndValues.Count > 0)
            {
                string whereString = null;
                string whereInclusive = "In";
                if (!include)
                {
                    whereInclusive = "Not In";
                }

                foreach (KeyValuePair<string, object[]> columnAndValues in columnsAndValues)
                {
                    object[] values = columnAndValues.Value;
                    if (values != null && values.Length > 0)
                    {
                        string whereValues = string.Join(",", values.Select(a => string.Format("'{0}'", a)));
                        whereString = string.Format("{0} [{1}] {2} ({3})", whereClause, columnAndValues.Key, whereInclusive, whereValues);
                        whereClause = " And";
                    }
                }
                query = string.Format("{0}{1}", query, whereString);
            }
        }

        #region Fill From File

        public static void FillFromFile(DataTable dt, string fileName, Dictionary<int, string> columnMappings)
        {
            FillFromFile(dt,fileName,columnMappings,false);
        }

        public static void FillFromFile(DataTable dt, string fileName, Dictionary<int, string> columnMappings, bool firstRowHasHeader)
        {
            using (GenericParserAdapter parser = new GenericParserAdapter(fileName))
            {
                //DataTable rawDataTable = parser.GetDataTable();
                parser.FirstRowHasHeader = firstRowHasHeader;
                while (parser.Read())
                {
                    DataRow dr = dt.NewRow();
                    foreach(KeyValuePair<int,string> columnMapping in columnMappings)
                    {                        
                        dr[columnMapping.Value] = parser[columnMapping.Key];
                    }
                    dt.Rows.Add(dr);
                }      
            }
        }
        #endregion



    }
}
