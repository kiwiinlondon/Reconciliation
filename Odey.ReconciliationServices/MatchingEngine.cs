using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Odey.ReconciliationServices.Contracts;
using log4net;

namespace Odey.ReconciliationServices
{
    public class MatchingEngine
    {
        public MatchingEngine(ILog logger)
        {
            Logger = logger;
        }
        protected ILog Logger { get; set; }

        #region Compare DataTable Structures
        private static void CheckDataTableStructures(DataTable dt1, DataTable dt2, MatchTypeIds matchType)
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
            if (matchType == MatchTypeIds.Full)
            {
                foreach (DataColumn dc in dt2.Columns)
                {
                    if (!matchedDataColumns.Contains(dc))
                    {
                        throw new ApplicationException(String.Format("Data Table 1 does not contain column {0}", dc.ColumnName));
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

                if (dc.DataType == typeof(string) || dc.DataType == typeof(DateTime) && !dc.AllowDBNull)
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

        protected virtual bool StringsMatch(MatchingEngineOutputItem matchingEngineOutputItem, string fieldName, string field1, string field2) { return (field1.Equals(field2) ? true : false); }
        protected virtual bool IntegersMatch(MatchingEngineOutputItem matchingEngineOutputItem,string fieldName, int field1, int field2) { return (field1.Equals(field2) ? true : false); }
        public virtual bool DecimalsMatch(MatchingEngineOutputItem matchingEngineOutputItem, string fieldName, decimal field1, decimal field2)
        {
            return DecimalsMatch(matchingEngineOutputItem, fieldName, field1, field2, new decimal(0.00001));
        }
        public virtual bool DecimalsMatch(MatchingEngineOutputItem matchingEngineOutputItem, string fieldName, decimal field1, decimal field2, decimal tolerance) 
        {
            if (Math.Abs(field1 - field2) < tolerance)
            {
                return true;
            }
            return false;
        }
        protected virtual bool DatesMatch(MatchingEngineOutputItem matchingEngineOutputItem,string fieldName, DateTime field1, DateTime field2) { return (field1.Equals(field2) ? true : false); }

        private bool FieldsMatch(MatchingEngineOutputItem matchingEngineOutputItem, string fieldName, Type fieldType, object field1, object field2)
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
                return StringsMatch(matchingEngineOutputItem,fieldName, field1.ToString(), field2.ToString());
            }
            else if (fieldType == typeof(int))
            {
                return IntegersMatch(matchingEngineOutputItem,fieldName, int.Parse(field1.ToString()), int.Parse(field2.ToString()));
            }
            else if (fieldType == typeof(decimal))
            {
                return DecimalsMatch(matchingEngineOutputItem, fieldName, decimal.Parse(field1.ToString()), decimal.Parse(field2.ToString()));
            }
            else if (fieldType == typeof(DateTime))
            {
                return DatesMatch(matchingEngineOutputItem, fieldName, DateTime.Parse(field1.ToString()), DateTime.Parse(field2.ToString()));
            }
            else
            {
                return (field1.Equals(field2) ? true : false);
            }
        }
        #endregion 

        #region Get Matched State Missing Row
        private MatchOutputTypeIds GetMatchedStateMissingRow(DataRow dr, bool row1IsMissing)
        {
            if (RowCanBeIgnored(dr))
            {
                    return MatchOutputTypeIds.Matched; 
            }
            else
            {               
                if (row1IsMissing)
                {
                    return MatchOutputTypeIds.MissingFrom1; 
                }
                else
                {
                    return MatchOutputTypeIds.MissingFrom2; 
                }
                
            }
        }
        #endregion

        #region Get Value From Data Row
        private object GetValueFromDataRow(DataRow dr, string columnName)
        {
            if (dr == null)
            {
                return null;
            }
            else
            {
                if (dr[columnName] == DBNull.Value)
                {
                    return null;
                }
                else
                {
                    return dr[columnName];
                }
            }
        }
        #endregion

        #region Match Rows
        private MatchingEngineOutputItem MatchRows(DataRow dr1, DataRow dr2, List<MatchingEngineOutputProperty> keyProperties, List<MatchingEngineOutputProperty> nonKeyProperties)
        {
            MatchingEngineOutputItem matchingEngineOutputItem= new MatchingEngineOutputItem ();
            matchingEngineOutputItem.KeyValues = new Dictionary<string, object>();
            matchingEngineOutputItem.NonKeyValues = new Dictionary<string, MatchingEngineOutputPropertyValue>();
            matchingEngineOutputItem.MismatchedProperties = new List<MatchingEngineOutputPropertyValue>();
            matchingEngineOutputItem.MatchOutputType = MatchOutputTypeIds.None;
           

            DataRow dr = null;
            if (dr1 == null)
            {
                dr = dr2;
                matchingEngineOutputItem.MatchOutputType = GetMatchedStateMissingRow(dr2,true);
            }
            else
            {
                dr = dr1;
                if (dr2 == null)
                {
                    matchingEngineOutputItem.MatchOutputType = GetMatchedStateMissingRow(dr1, false);
                }
            }
            foreach (MatchingEngineOutputProperty property in keyProperties)
            {
                object value = GetValueFromDataRow(dr, property.PropertyName);
                if (value.GetType()== typeof(DateTime) && (DateTime)value == new DateTime(1976,5,20)) //Geoff's birthday is null
                {
                    value = null;
                }
                    
                matchingEngineOutputItem.KeyValues.Add(property.PropertyName, value);
            }
         
            foreach(MatchingEngineOutputProperty property in nonKeyProperties)
            {             
                MatchingEngineOutputPropertyValue value = new MatchingEngineOutputPropertyValue(property);
                value.Value1 = GetValueFromDataRow(dr1, property.PropertyName);
                value.Value2 = GetValueFromDataRow(dr2, property.PropertyName);

                if (matchingEngineOutputItem.MatchOutputType == MatchOutputTypeIds.None || matchingEngineOutputItem.MatchOutputType == MatchOutputTypeIds.Mismatched)
                {
                    bool fieldsMatch = FieldsMatch(matchingEngineOutputItem,property.PropertyName, property.PropertyType, value.Value1, value.Value2);
                    if (!fieldsMatch)
                    {
                        matchingEngineOutputItem.MatchOutputType = MatchOutputTypeIds.Mismatched;
                        
                        value.MatchOutputTypeId = MatchOutputTypeIds.Mismatched;
                    }
                    else
                    {
                        value.MatchOutputTypeId = MatchOutputTypeIds.Matched;
                    }
                }                
                else
                {
                     value.MatchOutputTypeId = matchingEngineOutputItem.MatchOutputType;
                }
                if (value.MatchOutputTypeId != MatchOutputTypeIds.Matched)
                {
                    matchingEngineOutputItem.MismatchedProperties.Add(value);
                }
                matchingEngineOutputItem.NonKeyValues.Add(property.PropertyName, value);   
            }
            if (matchingEngineOutputItem.MatchOutputType == MatchOutputTypeIds.None)
            {
                matchingEngineOutputItem.MatchOutputType = MatchOutputTypeIds.Matched;
            }
            return matchingEngineOutputItem; 
        }
        #endregion

        #region Add To Values
        private void AddToOutput(MatchingEngineOutput matchingEngineOutput, MatchingEngineOutputItem item, bool returnOnlyMismatches)
        {
            if (!(item.MatchOutputType == MatchOutputTypeIds.Matched && returnOnlyMismatches))
            {
                matchingEngineOutput.Outputs.Add(item);
            }
        }
        #endregion

        #region Greater Than Zero
        protected static bool GreaterThanZero(decimal value, decimal tolerance)
        {
            return (!(-tolerance < value && value < tolerance));
        }
        
        public static bool DifferenceGreaterThanPercentage(decimal value1, decimal value2, decimal tolerance)
        {
            decimal difference = value2 - value1;
            if (difference == 0)
            {
                return false;
            }
            decimal smallestValue = value1;
            if (smallestValue > value2)
            {
                difference = value1 - value2;
                smallestValue = value2;
            }
            if (smallestValue == 0)
            {
                return true;
            }
            return (difference / smallestValue > tolerance);
        }
        #endregion


        #region Match
        public MatchingEngineOutput Match(DataTable dt1, DataTable dt2, MatchTypeIds matchType, bool returnOnlyMismatches, DataSourceIds dataSource1, DataSourceIds dataSource2)
        {
            
            CheckDataTableStructures(dt1, dt2, matchType);
            MatchingEngineOutput matchingEngineOutput = new MatchingEngineOutput(dt1,dataSource1,dataSource2);            
            string selectFormat = CreatePrimaryKeySelectFormat(dt1);
            HashSet<DataRow> d2FoundRows = new HashSet<DataRow>(); 
            foreach (DataRow dr1 in dt1.Rows)
            {
                DataRow dr2 = GetCorrespondingRow(dr1, dt2, selectFormat);
                MatchingEngineOutputItem item = MatchRows(dr1, dr2, matchingEngineOutput.Key, matchingEngineOutput.NonKeyProperties);
                AddToOutput(matchingEngineOutput,item,returnOnlyMismatches);
                if (dr2 != null)
                {
                    d2FoundRows.Add(dr2);
                }
            }

            if (matchType == MatchTypeIds.Full)
            {
                foreach (DataRow dr2 in dt2.Rows)
                {
                    if (!d2FoundRows.Contains(dr2))
                    {
                        MatchingEngineOutputItem item = MatchRows(null, dr2, matchingEngineOutput.Key, matchingEngineOutput.NonKeyProperties);
                        AddToOutput(matchingEngineOutput, item, returnOnlyMismatches);
                    }
                }
            }                
            return matchingEngineOutput;
        }
        #endregion

        #region Missing Row Can Be Ignored
        protected virtual bool RowCanBeIgnored(DataRow item)
        {
            return false;
        }

        #endregion
    }
}
