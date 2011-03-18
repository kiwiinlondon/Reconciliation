using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Odey.ReconciliationServices
{
    public class MatchingEngine
    {
        #region Compare DataTable Structures
        private static void CheckDataTableStructures(DataTable dt1, DataTable dt2, MatchType matchType)
        {
            HashSet<DataColumn> matchedDataColumns = new HashSet<DataColumn>();
            foreach (DataColumn dc1 in dt1.Columns)
            {
                DataColumn dc2 = dt2.Columns[dc1.ColumnName];
                if (dc2==null)
                {
                    throw new ApplicationException(String.Format("Data Table 2 does not contain column {0}", dc1.ColumnName));
                }
                else if (dc1.DataType != dc2.DataType)
                {
                    throw new ApplicationException(String.Format("Data Column {0} has differing types: on table 1 has type {1} and on table 2 has type {2}", dc1.ColumnName, dc1.DataType, dc2.DataType));
                }
                matchedDataColumns.Add(dc2);
            }
            if (matchType == MatchType.Full)
            {
                foreach (DataColumn dc in dt2.Columns)
                {
                    if (!matchedDataColumns.Contains(dc))
                    {
                        throw new ApplicationException(String.Format("Data Table 2 does not contain column {0}", dc.ColumnName));
                    }
                }
            }
        }

        #endregion

        #region Create Primary Key Select String
        private static string CreatePrimaryKeySelectFormat(DataTable dt)
        {
            string format = default(string);
            int count = 0;
            foreach (DataColumn dc in dt.PrimaryKey)
            {
                if (count>0)
                {
                    format = String.Format("{0} AND ",format);
                }
                string part = String.Format("{{{0}}}",count++);

                if (dc.DataType == typeof(string) || dc.DataType == typeof(DateTime))
                {
                    part = String.Format("'{0}'", part);
                }

                format = String.Format("{0}{1} = {2}", format, dc.ColumnName, part);
            }
            return format;
        }
        #endregion

        #region Get Primary Key Values
        private static object[] GetPrimaryKeyValues(DataRow dr)
        {
            object[] values = new Object[dr.Table.PrimaryKey.Length];

            int count = 0;
            foreach (DataColumn dc in dr.Table.PrimaryKey)
            {
                values[count++] = dr[dc];
            }
            return values;
        }
        #endregion 

        

        #region Get Corresponding Row
        private static DataRow GetCorrespondingRow(DataRow dr, DataTable dt,string selectFormat)
        {
            object[] parameters = GetPrimaryKeyValues(dr);
            string selectQuery = String.Format(selectFormat, parameters);
            DataRow[] dataRows = dt.Select(selectQuery);
            switch (dataRows.Length)
            {
                case 0: 
                    return null;
                case 1:
                    return dataRows[0];
                default:
                    throw new ApplicationException("Too Many rows found for key");
            }
        }
        #endregion

        #region Match Field

        protected virtual bool StringsMatch(string fieldName, string field1, string field2) { return (field1.Equals(field2) ? true : false); }
        protected virtual bool IntegersMatch(string fieldName, int field1, int field2) { return (field1.Equals(field2) ? true : false); }
        protected virtual bool DecimalsMatch(string fieldName, decimal field1, decimal field2) 
        {
            if (Math.Abs(field1 - field2) < new decimal(0.00000001))
            {
                return true;
            }
            return false;
        }
        protected virtual bool DatesMatch(string fieldName, DateTime field1, DateTime field2) { return (field1.Equals(field2) ? true : false); }

        private bool FieldsMatch(string fieldName, Type fieldType, object field1, object field2)
        {
            if (field1 == null && field2 == null)
            {
                return true;
            }
            else if ((field1 == null && field2 != null) || (field1 != null && field2 == null))
            {
                return false;
            }
            else if (fieldType == typeof(string))
            {
                return StringsMatch(fieldName,field1.ToString(), field2.ToString());
            }
            else if (fieldType == typeof(int))
            {
                return IntegersMatch(fieldName,int.Parse(field1.ToString()), int.Parse(field2.ToString()));
            }
            else if (fieldType == typeof(decimal))
            {
                return DecimalsMatch(fieldName,decimal.Parse(field1.ToString()), decimal.Parse(field2.ToString()));
            }
            else if (fieldType == typeof(DateTime))
            {
                return DatesMatch(fieldName,DateTime.Parse(field1.ToString()), DateTime.Parse(field2.ToString()));
            }
            else
            {
                return (field1.Equals(field2) ? true : false);
            }
        }
        #endregion 


        #region Match Rows
        private MatchOutputType RowsMatch(DataRow dr1, DataRow dr2, out List<string> misMatchedFieldNames)
        {
            misMatchedFieldNames = new List<string>();
            MatchOutputType matchOutputType = MatchOutputType.Matched;
            foreach (DataColumn dc in dr1.Table.Columns)
            {
                if (!FieldsMatch(dc.ColumnName, dc.DataType, dr1[dc], dr2[dc.ColumnName]))
                {
                    matchOutputType = MatchOutputType.MisMatched;
                    misMatchedFieldNames.Add(dc.ColumnName);
                }               
            }
            return matchOutputType;
        }
        #endregion

        #region Match
        public List<MatchingEngineOutput> Match(DataTable dt1, DataTable dt2, MatchType matchType)
        {
            CheckDataTableStructures(dt1, dt2, matchType);
            List<MatchingEngineOutput> outputs = new List<MatchingEngineOutput>();
            string selectFormat = CreatePrimaryKeySelectFormat(dt1);
            HashSet<DataRow> matchedDataRows = new HashSet<DataRow>(); 
            //for (int i=0; i<dt1.Rows.Count; i++)
            foreach (DataRow dr1 in dt1.Rows)
            {
                //DataRow dr1 = dt1.Rows[i];
                DataRow dr2 = GetCorrespondingRow(dr1, dt2, selectFormat);
                if (dr2 == null && !MissingRowCanBeIgnored(dr1))
                {
                    outputs.Add(new MatchingEngineOutput(dr1, null, MatchOutputType.MissingFrom2, null));
                }
                else
                {
                    List<string> misMatchedFieldNames;
                    MatchOutputType matchOutputType = RowsMatch(dr1, dr2, out misMatchedFieldNames);
                    outputs.Add(new MatchingEngineOutput(dr1, dr2, matchOutputType, misMatchedFieldNames));
                    matchedDataRows.Add(dr2); 
                }       
            }

            if (matchType == MatchType.Full)
            {
                foreach (DataRow dr2 in dt2.Rows)
                {
                    if (!matchedDataRows.Contains(dr2) && !MissingRowCanBeIgnored(dr2))
                    {
                        outputs.Add(new MatchingEngineOutput(null, dr2, MatchOutputType.MissingFrom1, null));
                    }
                }
            }
            return outputs;
        }
        #endregion

        #region Missing Row Can Be Ignored
        protected virtual bool MissingRowCanBeIgnored(DataRow dr)
        {
            return false;
        }

        #endregion
    }
}
