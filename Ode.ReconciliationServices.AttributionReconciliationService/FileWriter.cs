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
        public void Write(string fileName, Dictionary<Tuple<int, int>, AttributionReconciliationItem> mtdMatchedItems, Dictionary<Tuple<int, int>, AttributionReconciliationItem>  ytdMatchedItems)
        {
            using (MemoryStream stream = SpreadsheetReader.Create())
            {
                using (SpreadsheetDocument doc = SpreadsheetDocument.Open(stream, true))
                {
                    SpreadsheetStyleManager styles = new SpreadsheetStyleManager(doc);

                    WriteSheets(doc, styles, mtdMatchedItems, "MTD");
                    WriteSheets(doc, styles, ytdMatchedItems, "YTD");


                    SpreadsheetWriter.Save(doc);
                    SpreadsheetWriter.StreamToFile(fileName, stream);
                }
            }
        }

        private void WriteSheets(SpreadsheetDocument doc, SpreadsheetStyleManager styles, Dictionary<Tuple<int, int>, AttributionReconciliationItem> matchedItems, string sheetPrefix)
        {
            OpenXMLWorksheetWrapper pnlWorksheet = new OpenXMLWorksheetWrapper(doc, $"{sheetPrefix}-PNL");
            OpenXMLWorksheetWrapper fundAdjustedPNLWorksheet = new OpenXMLWorksheetWrapper(doc, $"{sheetPrefix}-FundAdjusted");
            OpenXMLWorksheetWrapper contributionWorksheet = new OpenXMLWorksheetWrapper(doc, $"{sheetPrefix}-Contribution");

            WriteToFile(pnlWorksheet, fundAdjustedPNLWorksheet, contributionWorksheet, matchedItems, styles);

            FitColumns(pnlWorksheet);
            FitColumns(fundAdjustedPNLWorksheet);
            FitColumns(contributionWorksheet);
        }

        public void WriteToFile(OpenXMLWorksheetWrapper pnlWorksheet, OpenXMLWorksheetWrapper fundAdjustedPNLWorksheet, OpenXMLWorksheetWrapper contributionWorksheet, Dictionary<Tuple<int, int>, AttributionReconciliationItem> matchedItems, SpreadsheetStyleManager styles)
        {

            WriteHeadings(pnlWorksheet, styles);
            WriteHeadings(fundAdjustedPNLWorksheet, styles);
            WriteHeadings(contributionWorksheet, styles);

            int startRow = pnlWorksheet.NextRowNumber;
            foreach (AttributionReconciliationItem item in matchedItems.Values.OrderBy(a => a.Name).ThenBy(a => a.Currency))
            {
                WriteDetailRow(pnlWorksheet, fundAdjustedPNLWorksheet, contributionWorksheet, item, styles);
            }
            int endRow = pnlWorksheet.LastRowNumber;



            SpreadsheetStyle numberStyle = styles.GetStyle(SpreadsheetStyleManager.STYLE_NUMERIC_0DP);
            SpreadsheetStyle diffStyle = styles.GetStyle(SpreadsheetStyleManager.STYLE_GREY_NUMERIC_0DP);
            SpreadsheetStyle sumStyle = styles.GetStyle(SpreadsheetStyleManager.STYLE_NUMERIC_0DP_SINGLE);
            ApplyStylesAndForumlas(pnlWorksheet, startRow, endRow, numberStyle, diffStyle, sumStyle);
            ApplyStylesAndForumlas(fundAdjustedPNLWorksheet, startRow, endRow, numberStyle, diffStyle, sumStyle);
            ApplyStylesAndForumlas(contributionWorksheet, startRow, endRow, styles.GetStyle(SpreadsheetStyleManager.STYLE_PERCENTAGE_3DP), styles.GetStyle(SpreadsheetStyleManager.STYLE_GREY_PERCENTAGE_3DP), styles.GetStyle(SpreadsheetStyleManager.STYLE_PERCENTAGE_3DP_SINGLE));
        }

        private void ApplyStylesAndForumlas(OpenXMLWorksheetWrapper worksheet, int startRow, int endRow, SpreadsheetStyle numberStyle, SpreadsheetStyle diffStyle, SpreadsheetStyle sumStyle)
        {
            
            worksheet.ApplyStyle("D", "U", startRow, endRow, numberStyle);
            worksheet.WriteFormulaToRangeDiffPreviousTwoColumns("F", startRow, endRow, diffStyle);
            worksheet.WriteFormulaToRangeDiffPreviousTwoColumns("I", startRow, endRow, diffStyle);
            worksheet.WriteFormulaToRangeDiffPreviousTwoColumns("L", startRow, endRow, diffStyle);
            worksheet.WriteFormulaToRangeDiffPreviousTwoColumns("O", startRow, endRow, diffStyle);
            worksheet.WriteFormulaToRangeDiffPreviousTwoColumns("R", startRow, endRow, diffStyle);
            worksheet.WriteFormulaToRangeDiffPreviousTwoColumns("U", startRow, endRow, diffStyle);

            int sumRow = endRow + 1;
            worksheet.WriteFormulaSumColumn("D", sumRow, startRow, sumStyle);
            worksheet.WriteFormulaSumColumn("E", sumRow, startRow, sumStyle);
            worksheet.WriteFormulaSumColumn("F", sumRow, startRow, sumStyle);
            worksheet.WriteFormulaSumColumn("G", sumRow, startRow, sumStyle);
            worksheet.WriteFormulaSumColumn("H", sumRow, startRow, sumStyle);
            worksheet.WriteFormulaSumColumn("I", sumRow, startRow, sumStyle);
            worksheet.WriteFormulaSumColumn("J", sumRow, startRow, sumStyle);
            worksheet.WriteFormulaSumColumn("K", sumRow, startRow, sumStyle);
            worksheet.WriteFormulaSumColumn("L", sumRow, startRow, sumStyle);
            worksheet.WriteFormulaSumColumn("M", sumRow, startRow, sumStyle);
            worksheet.WriteFormulaSumColumn("N", sumRow, startRow, sumStyle);
            worksheet.WriteFormulaSumColumn("O", sumRow, startRow, sumStyle);
            worksheet.WriteFormulaSumColumn("P", sumRow, startRow, sumStyle);
            worksheet.WriteFormulaSumColumn("Q", sumRow, startRow, sumStyle);
            worksheet.WriteFormulaSumColumn("R", sumRow, startRow, sumStyle);
            worksheet.WriteFormulaSumColumn("S", sumRow, startRow, sumStyle);
            worksheet.WriteFormulaSumColumn("T", sumRow, startRow, sumStyle);
            worksheet.WriteFormulaSumColumn("U", sumRow, startRow, sumStyle);
            worksheet.FreezePanes(endRow);
        }

        private static void WriteHeadings(OpenXMLWorksheetWrapper worksheet, SpreadsheetStyleManager styles)
        {

            worksheet.WriteRow(new object[] { "Instrument", null, null, "Total", null, null, "Price", null, null, "FX", null, null, "Carry", null, null, "Other", null, null, "Default", null, null }, styles.GetStyle(SpreadsheetStyleManager.STYLE_TABLE_HEADING));
            worksheet.WriteRow(new object[] { "IM Id","Name", "Ccy","Admin", "Cache", "Diff", "Admin", "Cache", "Diff", "Admin", "Cache", "Diff", "Admin", "Cache", "Diff", "Admin", "Cache", "Diff", "Admin", "Cache", "Diff" }, styles.GetStyle(SpreadsheetStyleManager.STYLE_TABLE_HEADING));

            worksheet.MergeCells("A", "C", 1, 1);
            worksheet.MergeCells("D", "F", 1, 1);
            worksheet.MergeCells("G", "I", 1, 1);
            worksheet.MergeCells("J", "L", 1, 1);
            worksheet.MergeCells("M", "O", 1, 1);
            worksheet.MergeCells("P", "R", 1, 1);
            worksheet.MergeCells("S", "U", 1, 1);

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
            if (item.PortfolioCacheValues == null)
            {
                portfolioCacheValues = new AttributionValues();
            }
            else
            {
                portfolioCacheValues = item.PortfolioCacheValues;
            }
            object[] pnlRow = { item.IssuerId, item.Name, item.Currency, administratorValues.Total, portfolioCacheValues.Total, DBNull.Value, administratorValues.PricePNL, portfolioCacheValues.PricePNL, DBNull.Value, administratorValues.FXPNL , portfolioCacheValues.FXPNL, DBNull.Value, administratorValues.CarryPNL, portfolioCacheValues.CarryPNL, DBNull.Value, administratorValues.OtherPNL, portfolioCacheValues.OtherPNL, DBNull.Value, administratorValues.DefaultPNL, portfolioCacheValues.DefaultPNL, DBNull.Value };
            object[] fundAdjustedRow = { item.IssuerId, item.Name, item.Currency, administratorValues.FundAdjustedTotal, portfolioCacheValues.FundAdjustedTotal, DBNull.Value, administratorValues.FundAdjustedPricePNL, portfolioCacheValues.FundAdjustedPricePNL, DBNull.Value, administratorValues.FundAdjustedFXPNL, portfolioCacheValues.FundAdjustedFXPNL, DBNull.Value, administratorValues.FundAdjustedCarryPNL, portfolioCacheValues.FundAdjustedCarryPNL, DBNull.Value, administratorValues.FundAdjustedOtherPNL, portfolioCacheValues.FundAdjustedOtherPNL, DBNull.Value, administratorValues.FundAdjustedDefaultPNL, portfolioCacheValues.FundAdjustedDefaultPNL, DBNull.Value };
            object[] contributionRow = { item.IssuerId, item.Name, item.Currency, administratorValues.ContributionTotal, portfolioCacheValues.ContributionTotal, DBNull.Value, administratorValues.PriceContribution, portfolioCacheValues.PriceContribution, DBNull.Value, administratorValues.FXContribution, portfolioCacheValues.FXContribution, DBNull.Value, administratorValues.CarryContribution, portfolioCacheValues.CarryContribution, DBNull.Value, administratorValues.OtherContribution, portfolioCacheValues.OtherContribution, DBNull.Value, administratorValues.DefaultContribution, portfolioCacheValues.DefaultContribution, DBNull.Value };
            pnlWorksheet.WriteRow(pnlRow, null);
            fundAdjustedPNLWorksheet.WriteRow(fundAdjustedRow, null);
            contributionWorksheet.WriteRow(contributionRow, null);


        }
        private string GetCellAddress(string columnReference, int rowNumber)
        {
            return String.Format("{0}{1}", columnReference, rowNumber);
        }

       




        private static void FitColumns(OpenXMLWorksheetWrapper oWorkSheet)
        {
            const int mainSize = 12;

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
            oWorkSheet.SetColumnWidth("S", mainSize);
            oWorkSheet.SetColumnWidth("T", mainSize);
            oWorkSheet.SetColumnWidth("U", mainSize);

        }
    }
}