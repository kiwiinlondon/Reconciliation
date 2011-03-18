using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.Common;
using Oracle.DataAccess.Client;

namespace Odey.ReconciliationServices
{
    public class DataSetUtilities
    {

        #region FM Connection String
        private static string FMConnectionString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["FM"].ConnectionString;
            }
        }
        #endregion

        #region Keeley Connection String
        private static string KeeleyConnectionString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["Keeley"].ConnectionString;
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

        private static void AddParameters(OracleCommand command, Dictionary<string, object> parameters)
        {
            if (parameters != null)
            {
                foreach (KeyValuePair<string, object> parameter in parameters)
                {
                    OracleParameter param = new OracleParameter(parameter.Key, parameter.Value);
                    command.Parameters.Add(param);
                }
            }
        }
        #endregion 

        #region Add Column Mappings
        private static void AddColumnMappings(DataTableMapping mapping, Dictionary<string, string> columnMappings)
        {
            if (mapping != null)
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

        #region Fill FM Data Table
        public static void FillFMDataTable(DataTable dt, string storedProcName, Dictionary<string, object> parameters, Dictionary<string, string> columnMappings)
        {
            DataSet ds = new DataSet();
            ds.Tables.Add(dt);
            using (OracleConnection connection = new OracleConnection(DataSetUtilities.FMConnectionString))
            {
                using (OracleCommand command = new OracleCommand(storedProcName, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    using (OracleParameter refCursor = new OracleParameter())
                    {
                        refCursor.OracleDbType = OracleDbType.RefCursor;
                        refCursor.Direction = ParameterDirection.ReturnValue;
                        command.Parameters.Add(refCursor);
                        AddParameters(command, parameters);

                        using (OracleDataAdapter da = new OracleDataAdapter(command))
                        {
                            DataTableMapping mapping = da.TableMappings.Add("Table", dt.TableName);
                            AddColumnMappings(mapping, columnMappings);
                            connection.Open();
                            da.FillSchema(ds, SchemaType.Mapped);
                            da.Fill(ds);
                        }
                    }
                }
            }
        }
        #endregion           
    }        
}
