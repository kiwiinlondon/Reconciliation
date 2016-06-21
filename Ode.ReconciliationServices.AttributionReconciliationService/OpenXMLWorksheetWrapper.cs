
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using DocumentFormat.OpenXml.Spreadsheet;
    using DocumentFormat.OpenXml.Packaging;
    using DocumentFormat.OpenXml.Extensions;
    using System.Data;


namespace Odey.ReconciliationServices.AttributionReconciliationService
{
    public class OpenXMLWorksheetWrapper
    {
        #region Private Properties
        private SheetData SheetData { get; set; }

        private WorksheetPart WorksheetPart { get; set; }

        private SpreadsheetDocument Document { get; set; }

        private WorksheetWriter Writer { get; set; }

        private SheetView SheetView { get; set; }

        private PageSetup _pageSetup = null;
        private PageSetup PageSetup
        {
            get
            {
                if (_pageSetup == null)
                {

                    _pageSetup = WorksheetPart.Worksheet.GetFirstChild<PageSetup>();

                    if (_pageSetup == null)
                    {
                        _pageSetup = new PageSetup();

                        WorksheetPart.Worksheet.AppendChild(_pageSetup);
                    }
                }
                return _pageSetup;
            }
        }

        private SheetProperties _sheetProperties = null;
        private SheetProperties SheetProperties
        {
            get
            {
                if (_sheetProperties == null)
                {
                    _sheetProperties = WorksheetPart.Worksheet.SheetProperties;
                    if (_sheetProperties == null)
                    {
                        _sheetProperties = new SheetProperties();
                        WorksheetPart.Worksheet.SheetProperties = _sheetProperties;
                    }
                }
                return _sheetProperties;
            }
        }

        private PageSetupProperties _pageSetupProperties = null;
        private PageSetupProperties PageSetupProperties
        {
            get
            {
                if (_pageSetupProperties == null)
                {
                    _pageSetupProperties = SheetProperties.PageSetupProperties;
                    if (_pageSetupProperties == null)
                    {
                        _pageSetupProperties = new PageSetupProperties();
                        SheetProperties.PageSetupProperties = _pageSetupProperties;
                    }
                }
                return _pageSetupProperties;
            }
        }
        #endregion

        #region Public Properties
        public string Name { get; set; }
        #endregion




        #region Constructor
        public OpenXMLWorksheetWrapper(SpreadsheetDocument doc, string name)
        {
            Document = doc;
            Name = name;
            WorksheetPart = SpreadsheetReader.GetWorksheetPartByName(doc, name);
            if (WorksheetPart == null)
            {
                WorksheetPart = RenameAndReturnIfFound("Sheet1", name);
                if (WorksheetPart == null)
                {
                    WorksheetPart = RenameAndReturnIfFound("Sheet2", name);
                    if (WorksheetPart == null)
                    {
                        WorksheetPart = RenameAndReturnIfFound("Sheet3", name);
                        if (WorksheetPart == null)
                        {
                            WorksheetPart = SpreadsheetWriter.InsertWorksheet(doc, name);
                        }
                    }

                }
            }
            SheetData = WorksheetPart.Worksheet.GetFirstChild<SheetData>();
            Writer = new WorksheetWriter(doc, WorksheetPart);
            SheetView = WorksheetPart.Worksheet.SheetViews.GetFirstChild<SheetView>();


        }

        private WorksheetPart RenameAndReturnIfFound(string oldName, string newName)
        {
            Sheet sheet = Document.WorkbookPart.Workbook.Descendants<Sheet>().FirstOrDefault(s => s.Name == oldName);
            if (sheet == null)
            {
                return null;
            }
            sheet.Name = newName;
            return SpreadsheetReader.GetWorksheetPartByName(Document, newName);
        }
        #endregion

        #region Writers
        public void WriteRows(List<object[]> rowsOfValues, SpreadsheetStyle style)
        {
            foreach (object[] rowOfValues in rowsOfValues)
            {
                WriteRow(rowOfValues, style);
            }
        }

        public void WriteRow(object[] values, SpreadsheetStyle style)
        {
            string columnReference = null;
            int rowNumber = NextRowNumber;

            foreach (object value in values)
            {
                if (columnReference == null)
                {
                    columnReference = "A";
                }
                else
                {
                    columnReference = GetNextColumn(columnReference);
                }
                string cellAddress = GetCellAddress(columnReference, rowNumber);
                if (value != null && value != DBNull.Value)
                {
                    if (value.GetType() == typeof(decimal))
                    {
                        WriteNumber(cellAddress, Math.Round((decimal)value,5), style);
                    }
                    else
                    {
                        WriteString(cellAddress, value.ToString(), style);
                    }
                }
                else
                {
                    WriteEmptyCell(cellAddress, style);
                }
            }
        }

        public void WriteString(string address, string value, SpreadsheetStyle style)
        {
            Writer.PasteText(address, value, style);
        }

        public void WriteStringNextRow(string columnReference, string value, SpreadsheetStyle style)
        {
            string cellAddress = GetNextRowReference(columnReference);

            WriteString(cellAddress, value, style);
        }

        public void WriteNumber(string address, decimal value, SpreadsheetStyle style)
        {
            Writer.PasteNumber(address, value.ToString(), style);
        }

        public void WriteEmptyRow()
        {
            string address = GetCellAddress("A", NextRowNumber);
            WriteEmptyCell(address, null);
        }

        public void WriteEmptyCell(string address, SpreadsheetStyle style)
        {
            if (style == null)
            {
                Writer.FindCell(address);
            }
            else
            {
                ApplyStyle(address, style);
            }
        }
        #endregion

        #region Cell References
        private Row LastRow
        {
            get
            {
                return SheetData.Elements<Row>().LastOrDefault();
            }
        }

        public int LastRowNumber
        {
            get
            {
                Row lastRow = LastRow;
                if (lastRow == null)
                {
                    return 0;
                }
                return (int)lastRow.RowIndex.Value;
            }
        }

        public int NextRowNumber
        {
            get
            {
                return LastRowNumber + 1;
            }
        }

        public string GetLastRowReference(string columnReference)
        {
            return GetCellAddress(columnReference, LastRowNumber);
        }

        public string GetNextRowReference(string columnReference)
        {
            return GetCellAddress(columnReference, LastRowNumber + 1);
        }

        public string FirstColumnNextRowAddress
        {
            get
            {
                return GetCellAddress("A", LastRowNumber + 1);
            }
        }

        private string GetCellAddress(string columnReference, int rowNumber)
        {
            return String.Format("{0}{1}", columnReference, rowNumber);
        }

        public string GetNextColumn(string columnReference)
        {
            int columnIndex = GetColumnIndexFromName(columnReference);
            return GetColumnNameFromIndex(columnIndex + 1);
        }


        public string GetColumnNameFromIndex(int columnIndex)
        {
            int remainder;
            string columnName = "";

            while (columnIndex > 0)
            {
                remainder = (columnIndex - 1) % 26;
                columnName = System.Convert.ToChar(65 + remainder).ToString() + columnName;
                columnIndex = (int)((columnIndex - remainder) / 26);
            }


            return columnName;
        }

        public int GetColumnIndexFromName(String columnName)
        {
            columnName = columnName.ToUpper();
            int value = 0;
            for (int i = 0, k = columnName.Length - 1; i < columnName.Length; i++, k--)
            {
                int alpabetIndex = ((int)columnName[i]) - 64;
                int delta = 0;
                // last column simply add it
                if (k == 0)
                {
                    delta = alpabetIndex - 1;
                }
                else
                { // aggregate
                    if (alpabetIndex == 0)
                        delta = (26 * k);
                    else
                        delta = (alpabetIndex * 26 * k);
                }
                value += delta;
            }
            return value + 1;
        }

        #endregion

        #region Styles

        public void FreezePanes(int endRow)
        {
            
            SheetView sv = SheetView;
            Selection selection = new Selection() { Pane = PaneValues.BottomLeft };

            Pane pane1 = new Pane() { VerticalSplit = 2D, TopLeftCell = $"A{endRow}", ActivePane = PaneValues.BottomLeft, State = PaneStateValues.Frozen };

            sv.Append(pane1);
            sv.Append(selection);
        }

        public void ApplyStyle(string startColumn, string endColumn, int startRow, int endRow, SpreadsheetStyle style)
        {
            string startAddress = GetCellAddress(startColumn, startRow);
            string endAddress = GetCellAddress(endColumn, endRow);
            ApplyStyle(startAddress, endAddress, style);
        }

        public void ApplyStyle(string startAddress, string endAddress, SpreadsheetStyle style)
        {
            if (style != null)
            {
                Writer.SetStyle(style, startAddress, endAddress);
            }
        }

        public void ApplyStyle(string address, SpreadsheetStyle style)
        {
            if (style != null)
            {
                Writer.SetStyle(style, address);
            }
        }
        #endregion

        #region Merge
        public void MergeCells(string startColumn, string endColumn, int startRow, int endRow)
        {
            string startAddress = GetCellAddress(startColumn, startRow);
            string endAddress = GetCellAddress(endColumn, endRow);
            MergeCells(startAddress, endAddress);
        }

        public void MergeCells(string startAddress, string endAddress)
        {
            Writer.MergeCells(startAddress, endAddress);
        }
        #endregion

        #region Formulas

        #region Subtract 2 Cells
        public void WriteFormulaToRangeDiffPreviousTwoColumns(string columnReference, int startRowNumber, int endRowNumber, SpreadsheetStyle style)
        {
            WriteFormulaToRangeDiffTwoColumns(columnReference, startRowNumber, endRowNumber, -2, -1, style);
        }

        public void WriteFormulaToRangeDiffTwoColumns(string columnReference, int startRowNumber, int endRowNumber, int column1RelativePosition, int column2RelativePosition, SpreadsheetStyle style)
        {
            int columnIndex = GetColumnIndexFromName(columnReference);
            string firstColumnReference = GetColumnNameFromIndex(columnIndex + column1RelativePosition);
            string secondColumnReference = GetColumnNameFromIndex(columnIndex + column2RelativePosition);

            string formulaTemplate = String.Format("{0}{2}-{1}{2}", firstColumnReference, secondColumnReference, "{0}");

            WriteFormulaToRange(columnReference, startRowNumber, endRowNumber, formulaTemplate, style);
        }
        #endregion

        private string GetAddressTemplate(string columnReference)
        {
            return String.Format("{0}{1}", columnReference, "{0}");
        }

        #region Percent Difference Between Two Cells
        public void WriteFormulaToRangePercentDiffPreviousTwoColumns(string columnReference, int startRowNumber, int endRowNumber, SpreadsheetStyle style)
        {
            WriteFormulaToRangePercentDiffTwoColumns(columnReference, startRowNumber, endRowNumber, -2, -1, style);
        }

        public void WriteFormulaPercentDiff(string address, string address1, string address2, SpreadsheetStyle style)
        {
            string formula = GetPercentageDiffTwoColumnFormula(address1, address2);
            WriteFormula(address, formula, style);
        }

        public void WriteFormulaPercentDiff(string column, int row, string column1, string column2, SpreadsheetStyle style)
        {
            string address = GetCellAddress(column, row);
            string address1 = GetCellAddress(column1, row);
            string address2 = GetCellAddress(column2, row);
            WriteFormulaPercentDiff(address, address1, address2, style);
        }

        public void WriteFormulaToRangePercentDiffTwoColumns(string columnReference, int startRowNumber, int endRowNumber,
            int column1RelativePosition, int column2RelativePosition, SpreadsheetStyle style)
        {
            int columnIndex = GetColumnIndexFromName(columnReference);
            string firstColumnReference = GetColumnNameFromIndex(columnIndex + column1RelativePosition);
            string secondColumnReference = GetColumnNameFromIndex(columnIndex + column2RelativePosition);
            string formulaTemplate = GetPercentageDiffTwoColumnFormula(GetAddressTemplate(firstColumnReference), GetAddressTemplate(secondColumnReference));
            WriteFormulaToRange(columnReference, startRowNumber, endRowNumber, formulaTemplate, style);
        }

        private string GetPercentageDiffTwoColumnFormula(string address1, string address2)
        {
            return String.Format("IF({0}={1},0,ABS({0}-{1})/ABS(IF({1}=0,{0},{1})))", address2, address1);
        }
        #endregion

        #region Difference between two cells as percent of third number
        public void WriteFormulaDiffTwoCellsAsPercentOfCell(string address, string address1, string address2, string referenceCellAddress, SpreadsheetStyle style)
        {
            string formula = GetDiffTwoColumnAsPercentOfCell(address1, address2, referenceCellAddress);
            WriteFormula(address, formula, style);
        }

        public void WriteFormulaDiffTwoCellsAsPercentOfCell(string column, int row, string column1, string column2, string referenceCellAddress, SpreadsheetStyle style)
        {
            string address = GetCellAddress(column, row);
            string address1 = GetCellAddress(column1, row);
            string address2 = GetCellAddress(column2, row);
            WriteFormulaDiffTwoCellsAsPercentOfCell(address, address1, address2, referenceCellAddress, style);
        }

        public void WriteFormulaToRangeDiffTwoColumnAsPercentOfCell(string columnReference, int startRowNumber, int endRowNumber,
            int column1RelativePosition, int column2RelativePosition, string referenceCellLabel, SpreadsheetStyle style)
        {
            int columnIndex = GetColumnIndexFromName(columnReference);
            string firstColumnReference = GetColumnNameFromIndex(columnIndex + column1RelativePosition);
            string secondColumnReference = GetColumnNameFromIndex(columnIndex + column2RelativePosition);

            string formulaTemplate = GetDiffTwoColumnAsPercentOfCell(GetAddressTemplate(firstColumnReference), GetAddressTemplate(secondColumnReference), referenceCellLabel);
            WriteFormulaToRange(columnReference, startRowNumber, endRowNumber, formulaTemplate, style);

        }

        private string GetDiffTwoColumnAsPercentOfCell(string address1, string address2, string referenceCellLabel)
        {
            return String.Format("=ABS({0}-{1})/{2}", address1, address2, referenceCellLabel);
        }

        #endregion

        #region Sum
        public void WriteFormulaSumColumn(string columnReference, int row, int startRow, SpreadsheetStyle style)
        {
            string cellAddress = GetCellAddress(columnReference, row);
            string firstCellLabel = GetCellAddress(columnReference, startRow);
            string secondCellLabel = GetCellAddress(columnReference, row - 1);
            string formula = String.Format("=Sum({0}:{1})", firstCellLabel, secondCellLabel);
            WriteFormula(cellAddress, formula, style);
        }
        #endregion

        #region Total Cells
        public void WriteFormulaAddTotalsTogether(string column, int row, List<int> rowNumbersThatHaveTotals, SpreadsheetStyle style)
        {
            string formula = null;
            string cellAddress = null;
            foreach (int rowWithTotal in rowNumbersThatHaveTotals)
            {
                cellAddress = GetCellAddress(column, rowWithTotal);
                if (formula == null)
                {
                    formula = String.Format("={0}", cellAddress);
                }
                else
                {
                    formula = String.Format("{0}+{1}", formula, cellAddress);
                }
            }
            cellAddress = GetCellAddress(column, row);
            WriteFormula(cellAddress, formula, style);
        }
        #endregion
        public void WriteFormulaToRange(string columnReference, int startRowNumber, int endRowNumber, string formulaTemplate, SpreadsheetStyle style)
        {
            for (int i = startRowNumber; i <= endRowNumber; i++)
            {
                string cellAddress = GetCellAddress(columnReference, i);
                string formula = String.Format(formulaTemplate, i);
                WriteFormula(cellAddress, formula);
            }
            ApplyStyle(columnReference, columnReference, startRowNumber, endRowNumber, style);
        }

        public void WriteFormula(string address, string formula)
        {
            WriteFormula(address, formula, null);
        }

        public void WriteFormula(string address, string formula, SpreadsheetStyle style)
        {
            var a = new CellFormula(formula);
            Cell cell = Writer.FindCell(address);
            cell.CellFormula = a;
            //a.CalculateCell = true;
            if (style != null)
            {
                ApplyStyle(address, address, style);
            }
        }
        #endregion

        #region Column Width
        public void SetColumnWidth(string column, decimal width)
        {
            //SheetData.
            Writer.SetColumnWidth(column, (double)width);
            //    writer.FindColumn(2)
        }
        #endregion

        #region Document Setup
        public void SetZoomScale(decimal value)
        {
            SheetView.ZoomScale = 90;
        }

        public void SetPrintArea(string address1, string address2Column, int address2Row)
        {
            string address2 = GetCellAddress(address2Column, address2Row);
            Writer.SetPrintArea(Name, address1, address2);
        }

        public OrientationValues PageOrientation
        {
            get
            {
                return PageSetup.Orientation;
            }
            set
            {
                PageSetup.Orientation = value;
            }
        }

        public bool FitToPage
        {
            get
            {
                return PageSetupProperties.FitToPage;
            }
            set
            {
                PageSetupProperties.FitToPage = value;
            }
        }
        #endregion
    }
}
