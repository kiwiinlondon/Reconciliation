using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.Common;
using GenericParsing;

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
                    mapping.ColumnMappings.Add(columnMapping.Key, columnMapping.Value);
                }
            }
        }
        #endregion

        #region Fill Keeley Data Table
        public static void FillKeeleyDataTable(DataTable dt, string storedProcName, Dictionary<string,object> parameters, Dictionary<string,string> columnMappings)
        {
            DataSet ds = new DataSet();
            ds.Tables.Add(dt);
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



        #region Fill From File
        public static void FillFromFile(DataTable dt, string fileName, Dictionary<int, string> columnMappings)
        {
            using (GenericParserAdapter parser = new GenericParserAdapter(fileName))
            {
                //DataTable rawDataTable = parser.GetDataTable();
                parser.FirstRowHasHeader = false;
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
