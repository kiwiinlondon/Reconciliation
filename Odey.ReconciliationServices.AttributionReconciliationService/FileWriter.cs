using DocumentFormat.OpenXml.Extensions;
using DocumentFormat.OpenXml.Office2013.Excel;
using DocumentFormat.OpenXml.Packaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using OX = DocumentFormat.OpenXml.Spreadsheet;

namespace Odey.ReconciliationServices.AttributionReconciliationService
{
    public class FileWriter
    {
        public void Write(string fileName, List<AttributionReconciliationItem> matchedItems,string sheetPrefix)
        {
            using (MemoryStream stream = SpreadsheetReader.Create())
            {
                using (SpreadsheetDocument doc = SpreadsheetDocument.Open(stream, true))
                {
                    SpreadsheetStyleManager styles = new SpreadsheetStyleManager(doc);

                    WriteSheets(doc, styles, matchedItems, sheetPrefix);


                    SpreadsheetWriter.Save(doc);
                    SpreadsheetWriter.StreamToFile(fileName, stream);
                }
            }
        }

        private void WriteSheets(SpreadsheetDocument doc, SpreadsheetStyleManager styles, List<AttributionReconciliationItem> matchedItems, string sheetPrefix)
        {
            List<object> headers = matchedItems.First().Header;
            if (!string.IsNullOrWhiteSpace(sheetPrefix))
            {
                sheetPrefix = $"{sheetPrefix}-";
            }
            OpenXMLWorksheetWrapper pnlWorksheet = new OpenXMLWorksheetWrapper(doc, $"{sheetPrefix}PNL");
            OpenXMLWorksheetWrapper fundAdjustedPNLWorksheet = new OpenXMLWorksheetWrapper(doc, $"{sheetPrefix}FundAdjusted");
            OpenXMLWorksheetWrapper contributionWorksheet = new OpenXMLWorksheetWrapper(doc, $"{sheetPrefix}Contribution");

            WriteToFile(headers,pnlWorksheet, fundAdjustedPNLWorksheet, contributionWorksheet, matchedItems, styles);

            FitColumns(headers,pnlWorksheet);
            FitColumns(headers,fundAdjustedPNLWorksheet);
            FitColumns(headers,contributionWorksheet);
        }

        public void WriteToFile(List<object> headers,OpenXMLWorksheetWrapper pnlWorksheet, OpenXMLWorksheetWrapper fundAdjustedPNLWorksheet, OpenXMLWorksheetWrapper contributionWorksheet, List<AttributionReconciliationItem> matchedItems, SpreadsheetStyleManager styles)
        {
            WriteHeadings(headers,pnlWorksheet, styles);
            WriteHeadings(headers,fundAdjustedPNLWorksheet, styles);
            WriteHeadings(headers,contributionWorksheet, styles);

            int startRow = pnlWorksheet.NextRowNumber;
            foreach (AttributionReconciliationItem item in matchedItems.OrderBy(a => a))
            {
                WriteDetailRow(pnlWorksheet, fundAdjustedPNLWorksheet, contributionWorksheet, item, styles);
            }
            int endRow = pnlWorksheet.LastRowNumber;



            SpreadsheetStyle numberStyle = styles.GetStyle(SpreadsheetStyleManager.STYLE_NUMERIC_0DP);
            SpreadsheetStyle diffStyle = styles.GetStyle(SpreadsheetStyleManager.STYLE_GREY_NUMERIC_0DP);
            SpreadsheetStyle sumStyle = styles.GetStyle(SpreadsheetStyleManager.STYLE_NUMERIC_0DP_SINGLE);
            ApplyStylesAndForumlas(headers.Count,pnlWorksheet, startRow, endRow, numberStyle, diffStyle, sumStyle);
            ApplyStylesAndForumlas(headers.Count,fundAdjustedPNLWorksheet, startRow, endRow, numberStyle, diffStyle, sumStyle);
            ApplyStylesAndForumlas(headers.Count,contributionWorksheet, startRow, endRow, styles.GetStyle(SpreadsheetStyleManager.STYLE_PERCENTAGE_3DP), styles.GetStyle(SpreadsheetStyleManager.STYLE_GREY_PERCENTAGE_3DP), styles.GetStyle(SpreadsheetStyleManager.STYLE_PERCENTAGE_3DP_SINGLE));
        }

        private void ApplyStylesAndForumlas(int headerCount, OpenXMLWorksheetWrapper worksheet, int startRow, int endRow, SpreadsheetStyle numberStyle, SpreadsheetStyle diffStyle, SpreadsheetStyle sumStyle)
        {
            
            worksheet.ApplyStyle(alphabet[headerCount], alphabet[headerCount + 17], startRow, endRow, numberStyle); //D U
            worksheet.WriteFormulaToRangeDiffPreviousTwoColumns(alphabet[headerCount + 2], startRow, endRow, diffStyle);//F
            worksheet.WriteFormulaToRangeDiffPreviousTwoColumns(alphabet[headerCount + 5], startRow, endRow, diffStyle);//I
            worksheet.WriteFormulaToRangeDiffPreviousTwoColumns(alphabet[headerCount + 8], startRow, endRow, diffStyle);//L
            worksheet.WriteFormulaToRangeDiffPreviousTwoColumns(alphabet[headerCount + 11], startRow, endRow, diffStyle);//O
            worksheet.WriteFormulaToRangeDiffPreviousTwoColumns(alphabet[headerCount + 14], startRow, endRow, diffStyle);//R
            worksheet.WriteFormulaToRangeDiffPreviousTwoColumns(alphabet[headerCount + 17], startRow, endRow, diffStyle);//U

            int sumRow = endRow + 1;
            worksheet.WriteFormulaSumColumn(alphabet[headerCount+ 0], sumRow, startRow, sumStyle);//D
            worksheet.WriteFormulaSumColumn(alphabet[headerCount+ 1], sumRow, startRow, sumStyle);//E
            worksheet.WriteFormulaSumColumn(alphabet[headerCount + 2], sumRow, startRow, sumStyle);
            worksheet.WriteFormulaSumColumn(alphabet[headerCount + 3], sumRow, startRow, sumStyle);
            worksheet.WriteFormulaSumColumn(alphabet[headerCount + 4], sumRow, startRow, sumStyle);
            worksheet.WriteFormulaSumColumn(alphabet[headerCount + 5], sumRow, startRow, sumStyle);
            worksheet.WriteFormulaSumColumn(alphabet[headerCount + 6], sumRow, startRow, sumStyle);
            worksheet.WriteFormulaSumColumn(alphabet[headerCount + 7], sumRow, startRow, sumStyle);
            worksheet.WriteFormulaSumColumn(alphabet[headerCount + 8], sumRow, startRow, sumStyle);
            worksheet.WriteFormulaSumColumn(alphabet[headerCount + 9], sumRow, startRow, sumStyle);
            worksheet.WriteFormulaSumColumn(alphabet[headerCount + 10], sumRow, startRow, sumStyle);
            worksheet.WriteFormulaSumColumn(alphabet[headerCount + 11], sumRow, startRow, sumStyle);
            worksheet.WriteFormulaSumColumn(alphabet[headerCount + 12], sumRow, startRow, sumStyle);
            worksheet.WriteFormulaSumColumn(alphabet[headerCount + 13], sumRow, startRow, sumStyle);
            worksheet.WriteFormulaSumColumn(alphabet[headerCount + 14], sumRow, startRow, sumStyle);
            worksheet.WriteFormulaSumColumn(alphabet[headerCount + 15], sumRow, startRow, sumStyle);
            worksheet.WriteFormulaSumColumn(alphabet[headerCount + 16], sumRow, startRow, sumStyle);
            worksheet.WriteFormulaSumColumn(alphabet[headerCount + 17], sumRow, startRow, sumStyle);
            worksheet.FreezePanes(endRow);
        }

                                                            //3+  //-3   -2   -1   0    1    2    3    4    5    6    7    8    9    10   11   12   13   14   15   16   17   18   19   20
                                                                  //0    1    2    3    4    5    6    7    8    9    10   11   12   13   14   15   16   17   18   19   20
        private static readonly string[] alphabet = new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "AA", "AB", "AC" };

        private static void WriteHeadings(List<object> headerFields, OpenXMLWorksheetWrapper worksheet, SpreadsheetStyleManager styles)
        {
            var topRow = new object[headerFields.Count].ToList();
            topRow[0] = "Descriptor";
            topRow.AddRange(new List<object>() { "Total", null, null, "Price", null, null, "FX", null, null, "Carry", null, null, "Other", null, null, "Default", null, null });
            worksheet.WriteRow(topRow.ToArray(), styles.GetStyle(SpreadsheetStyleManager.STYLE_TABLE_HEADING));

            var headerRow = headerFields.ToList();
            headerRow.AddRange(new object[] { "Admin", "Cache", "Diff", "Admin", "Cache", "Diff", "Admin", "Cache", "Diff", "Admin", "Cache", "Diff", "Admin", "Cache", "Diff", "Admin", "Cache", "Diff" });
            worksheet.WriteRow(headerRow.ToArray(), styles.GetStyle(SpreadsheetStyleManager.STYLE_TABLE_HEADING));

            //worksheet.MergeCells("A", "C", 1, 1);
            worksheet.MergeCells("A", alphabet[headerFields.Count-1], 1, 1);
            //worksheet.MergeCells("D", "F", 1, 1);
            worksheet.MergeCells(alphabet[headerFields.Count], alphabet[headerFields.Count +2], 1, 1);
            //worksheet.MergeCells("G", "I", 1, 1);
            worksheet.MergeCells(alphabet[headerFields.Count+3], alphabet[headerFields.Count + 5], 1, 1);
            //worksheet.MergeCells("J", "L", 1, 1);
            worksheet.MergeCells(alphabet[headerFields.Count + 6], alphabet[headerFields.Count + 8], 1, 1);
            //worksheet.MergeCells("M", "O", 1, 1);
            worksheet.MergeCells(alphabet[headerFields.Count + 9], alphabet[headerFields.Count + 11], 1, 1);
            //worksheet.MergeCells("P", "R", 1, 1);
            worksheet.MergeCells(alphabet[headerFields.Count + 12], alphabet[headerFields.Count + 14], 1, 1);
            //worksheet.MergeCells("S", "U", 1, 1);
            worksheet.MergeCells(alphabet[headerFields.Count + 15], alphabet[headerFields.Count + 17], 1, 1);

        }


        public void WriteDetailRow(OpenXMLWorksheetWrapper pnlWorksheet, OpenXMLWorksheetWrapper fundAdjustedPNLWorksheet, OpenXMLWorksheetWrapper contributionWorksheet, AttributionReconciliationItem item, SpreadsheetStyleManager styles)
        {
            AttributionValues administratorValues;
            if (item.AdministratorValues == null)
            {
                administratorValues = new AttributionValues();
            }
            else
            {
                administratorValues = item.AdministratorValues;
            }
            AttributionValues portfolioCacheValues;
            if (item.KeeleyValues == null)
            {
                portfolioCacheValues = new AttributionValues();
            }
            else
            {
                portfolioCacheValues = item.KeeleyValues;
            }

            WriteRow(pnlWorksheet,item.Descriptor, new List<object>{ administratorValues.Total, portfolioCacheValues.Total, DBNull.Value, administratorValues.PricePNL, portfolioCacheValues.PricePNL, DBNull.Value, administratorValues.FXPNL, portfolioCacheValues.FXPNL, DBNull.Value, administratorValues.CarryPNL, portfolioCacheValues.CarryPNL, DBNull.Value, administratorValues.OtherPNL, portfolioCacheValues.OtherPNL, DBNull.Value, administratorValues.DefaultPNL, portfolioCacheValues.DefaultPNL, DBNull.Value });
            WriteRow(fundAdjustedPNLWorksheet, item.Descriptor, new List<object> { administratorValues.FundAdjustedTotal, portfolioCacheValues.FundAdjustedTotal, DBNull.Value, administratorValues.FundAdjustedPricePNL, portfolioCacheValues.FundAdjustedPricePNL, DBNull.Value, administratorValues.FundAdjustedFXPNL, portfolioCacheValues.FundAdjustedFXPNL, DBNull.Value, administratorValues.FundAdjustedCarryPNL, portfolioCacheValues.FundAdjustedCarryPNL, DBNull.Value, administratorValues.FundAdjustedOtherPNL, portfolioCacheValues.FundAdjustedOtherPNL, DBNull.Value, administratorValues.FundAdjustedDefaultPNL, portfolioCacheValues.FundAdjustedDefaultPNL, DBNull.Value });
            WriteRow(contributionWorksheet, item.Descriptor, new List<object> { administratorValues.ContributionTotal, portfolioCacheValues.ContributionTotal, DBNull.Value, administratorValues.PriceContribution, portfolioCacheValues.PriceContribution, DBNull.Value, administratorValues.FXContribution, portfolioCacheValues.FXContribution, DBNull.Value, administratorValues.CarryContribution, portfolioCacheValues.CarryContribution, DBNull.Value, administratorValues.OtherContribution, portfolioCacheValues.OtherContribution, DBNull.Value, administratorValues.DefaultContribution, portfolioCacheValues.DefaultContribution, DBNull.Value });
        }

        private void WriteRow(OpenXMLWorksheetWrapper worksheet,List<object> nameDescriptor, List<object> values)
        {
            List<object> row = nameDescriptor.ToList();
            row.AddRange(values);
            worksheet.WriteRow(row.ToArray(), null);
        }

        private string GetCellAddress(string columnReference, int rowNumber)
        {
            return String.Format("{0}{1}", columnReference, rowNumber);
        }

       




        private static void FitColumns(List<object> headers, OpenXMLWorksheetWrapper oWorkSheet)
        {

            const int mainSize = 12;

            int headerCount = headers.Count;


            for (int i = 0; i < headers.Count; i++)
            {
                string header = headers[i].ToString();
                int size = mainSize;
                if (header == "Name")
                {
                    size = 30;
                }
                oWorkSheet.SetColumnWidth(alphabet[i], size);                
            }
            oWorkSheet.SetColumnWidth(alphabet[headerCount], mainSize);
            oWorkSheet.SetColumnWidth(alphabet[headerCount + 1], mainSize);
            oWorkSheet.SetColumnWidth(alphabet[headerCount + 2], mainSize);
            oWorkSheet.SetColumnWidth(alphabet[headerCount + 3], mainSize);
            oWorkSheet.SetColumnWidth(alphabet[headerCount + 4], mainSize);
            oWorkSheet.SetColumnWidth(alphabet[headerCount + 5], mainSize);
            oWorkSheet.SetColumnWidth(alphabet[headerCount + 6], mainSize);
            oWorkSheet.SetColumnWidth(alphabet[headerCount + 7], mainSize);
            oWorkSheet.SetColumnWidth(alphabet[headerCount + 8], mainSize);
            oWorkSheet.SetColumnWidth(alphabet[headerCount + 9], mainSize);
            oWorkSheet.SetColumnWidth(alphabet[headerCount + 10], mainSize);
            oWorkSheet.SetColumnWidth(alphabet[headerCount + 11], mainSize);
            oWorkSheet.SetColumnWidth(alphabet[headerCount + 12], mainSize);
            oWorkSheet.SetColumnWidth(alphabet[headerCount + 13], mainSize);
            oWorkSheet.SetColumnWidth(alphabet[headerCount + 14], mainSize);
            oWorkSheet.SetColumnWidth(alphabet[headerCount + 15], mainSize);
            oWorkSheet.SetColumnWidth(alphabet[headerCount + 16], mainSize);
            oWorkSheet.SetColumnWidth(alphabet[headerCount+ 17], mainSize);

        }
    }
}