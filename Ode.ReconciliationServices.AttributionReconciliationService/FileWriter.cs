using DocumentFormat.OpenXml.Extensions;
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
        public void Write(string fileName, Dictionary<Tuple<int, int>, AttributionReconciliationItem> matchedItems)
        {
            using (MemoryStream stream = SpreadsheetReader.Create())
            {
                using (SpreadsheetDocument doc = SpreadsheetDocument.Open(stream, true))
                {
                    SpreadsheetStyleManager styles = new SpreadsheetStyleManager(doc);
                    //OpenXMLWorksheetWrapper exceptionWorksheet = new OpenXMLWorksheetWrapper(doc, "Exceptions");
                    OpenXMLWorksheetWrapper exceptionWorksheet = null;
                    OpenXMLWorksheetWrapper detailWorksheet = new OpenXMLWorksheetWrapper(doc, "Detail");
                    WriteToFile(exceptionWorksheet, detailWorksheet, matchedItems, styles);
                    FitColumns(detailWorksheet);
                    SpreadsheetWriter.Save(doc);
                    SpreadsheetWriter.StreamToFile(fileName, stream);
                }
            }
        }
        public void WriteToFile(OpenXMLWorksheetWrapper exceptionWorksheet, OpenXMLWorksheetWrapper detailWorksheet, Dictionary<Tuple<int, int>, AttributionReconciliationItem> matchedItems, SpreadsheetStyleManager styles)
        {

            WriteHeadings(detailWorksheet,styles);
            int startRow = detailWorksheet.NextRowNumber;
            foreach (AttributionReconciliationItem item in matchedItems.Values.OrderBy(a => a.Name).ThenBy(a => a.Currency))
            {
                WriteRow(detailWorksheet, item, styles);
            }
            int endRow = detailWorksheet.LastRowNumber;
            detailWorksheet.ApplyStyle("D", "R", startRow, endRow, styles.GetStyle(SpreadsheetStyleManager.STYLE_NUMERIC_0DP));
            detailWorksheet.WriteFormulaToRangeDiffPreviousTwoColumns("F", startRow, endRow, styles.GetStyle(SpreadsheetStyleManager.STYLE_GREY_NUMERIC_0DP));
            detailWorksheet.WriteFormulaToRangeDiffPreviousTwoColumns("I", startRow, endRow, styles.GetStyle(SpreadsheetStyleManager.STYLE_GREY_NUMERIC_0DP));
            detailWorksheet.WriteFormulaToRangeDiffPreviousTwoColumns("L", startRow, endRow, styles.GetStyle(SpreadsheetStyleManager.STYLE_GREY_NUMERIC_0DP));
            detailWorksheet.WriteFormulaToRangeDiffPreviousTwoColumns("O", startRow, endRow, styles.GetStyle(SpreadsheetStyleManager.STYLE_GREY_NUMERIC_0DP));
            detailWorksheet.WriteFormulaToRangeDiffPreviousTwoColumns("R", startRow, endRow, styles.GetStyle(SpreadsheetStyleManager.STYLE_GREY_NUMERIC_0DP));

        }

        private static void WriteHeadings(OpenXMLWorksheetWrapper worksheet, SpreadsheetStyleManager styles)
        {

            worksheet.WriteRow(new object[] { "Instrument", null, null, "Total", null, null, "Price", null, null, "FX", null, null, "Carry", null, null, "Other", null, null }, styles.GetStyle(SpreadsheetStyleManager.STYLE_TABLE_HEADING));
            worksheet.WriteRow(new object[] { "IM Id","Name", "Ccy","Admin", "Cache", "Diff", "Admin", "Cache", "Diff", "Admin", "Cache", "Diff", "Admin", "Cache", "Diff", "Admin", "Cache", "Diff" }, styles.GetStyle(SpreadsheetStyleManager.STYLE_TABLE_HEADING));

            worksheet.MergeCells("A", "C", 1, 1);
            worksheet.MergeCells("D", "F", 1, 1);
            worksheet.MergeCells("G", "I", 1, 1);
            worksheet.MergeCells("J", "L", 1, 1);
            worksheet.MergeCells("M", "O", 1, 1);
            worksheet.MergeCells("P", "R", 1, 1);


        }


        public void WriteRow(OpenXMLWorksheetWrapper worksheet, AttributionReconciliationItem item, SpreadsheetStyleManager styles)
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
            if (item.PortfolioCacheValues == null)
            {
                portfolioCacheValues = new AttributionValues();
            }
            else
            {
                portfolioCacheValues = item.PortfolioCacheValues;
            }
            object[] row = { item.IssuerId, item.Name, item.Currency, administratorValues.Total, portfolioCacheValues.Total, DBNull.Value, administratorValues.PricePNL, portfolioCacheValues.PricePNL, DBNull.Value, administratorValues.FXPNL , portfolioCacheValues.FXPNL, DBNull.Value, administratorValues.CarryPNL, portfolioCacheValues.CarryPNL, DBNull.Value, administratorValues.OtherPNL, portfolioCacheValues.OtherPNL, DBNull.Value };
            worksheet.WriteRow(row, null);
            

        }


        private static void FitColumns(OpenXMLWorksheetWrapper oWorkSheet)
        {
            const int mainSize = 10;

            oWorkSheet.SetColumnWidth("A", mainSize);
            oWorkSheet.SetColumnWidth("B", 30);
            oWorkSheet.SetColumnWidth("C", mainSize);
            oWorkSheet.SetColumnWidth("D", mainSize);
            oWorkSheet.SetColumnWidth("E", mainSize);
            oWorkSheet.SetColumnWidth("F", mainSize);
            oWorkSheet.SetColumnWidth("G", mainSize);
            oWorkSheet.SetColumnWidth("H", mainSize);
            oWorkSheet.SetColumnWidth("I", mainSize);
            oWorkSheet.SetColumnWidth("J", mainSize);
            oWorkSheet.SetColumnWidth("K", mainSize);
            oWorkSheet.SetColumnWidth("L", mainSize);
            oWorkSheet.SetColumnWidth("M", mainSize);
            oWorkSheet.SetColumnWidth("O", mainSize);
            oWorkSheet.SetColumnWidth("P", mainSize);
            oWorkSheet.SetColumnWidth("Q", mainSize);
            oWorkSheet.SetColumnWidth("R", mainSize);


        }
    }
}