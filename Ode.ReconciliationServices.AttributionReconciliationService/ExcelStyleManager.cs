using DocumentFormat.OpenXml.Extensions;
using DocumentFormat.OpenXml.Packaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Drawing;
using OX = DocumentFormat.OpenXml.Spreadsheet;

namespace Odey.ReconciliationServices.AttributionReconciliationService
{

    public class SpreadsheetStyleManager
    {
        public static readonly string STYLE_SHEET_HEADING = "SHEET_HEADING";
        public static readonly string STYLE_BOLD = "BOLD";
        public static readonly string STYLE_TABLE_HEADING = "TABLE_HEADING";
        public static readonly string STYLE_TABLE_DETAIL_HEADING = "TABLE_DETAIL_HEADING";

        public static readonly string STYLE_NUMERIC_0DP = "NUMERIC_0DP";
        public static readonly string STYLE_GREY_NUMERIC_0DP = "GREY_NUMERIC_0DP";
        public static readonly string STYLE_NUMERIC_0DP_SINGLE = "NUMERIC_0DP_SINGLE";
        public static readonly string STYLE_NUMERIC_0DP_DOUBLE = "GREY_NUMERIC_0DP_DOUBLE";

        public static readonly string STYLE_NUMERIC_4DP = "NUMERIC_4DP_DOUBLE";

        public static readonly string STYLE_GREY_PERCENTAGE_2DP = "GREY_PERCENTAGE_2DP";
        public static readonly string STYLE_GREY_PERCENTAGE_2DP_SINGLE = "GREY_PERCENTAGE_2DP_SINGLE";
        public static readonly string STYLE_GREY_PERCENTAGE_2DP_DOUBLE = "GREY_PERCENTAGE_2DP_DOUBLE";

        public static readonly string STYLE_MARKET_VALUE_DIFF_NAV = "STYLE_MARKET_VALUE_DIFF_NAV";
        public static readonly string STYLE_MARKET_VALUE_DIFF = "STYLE_MARKET_VALUE_DIFF";
        public static readonly string STYLE_PRICE_DIFF = "STYLE_PRICE_DIFF";

        public SpreadsheetStyleManager(SpreadsheetDocument doc)
        {
            Document = doc;
            InitiateStyles(doc);
        }
        private void InitiateStyles(SpreadsheetDocument doc)
        {
            //Sheet Heading
            SpreadsheetStyle style = GetBaseSpreadSheetStyle();
            style.IsBold = true;
            SaveStyle(STYLE_SHEET_HEADING, style);
            SaveStyle(STYLE_BOLD, style);

            //Table Heading
            style = GetTableHeadingBaseStyle();
            style.IsBold = true;
            SaveStyle(STYLE_TABLE_HEADING, style);

            //Table Detail Heading
            style = GetTableHeadingBaseStyle();
            SaveStyle(STYLE_TABLE_DETAIL_HEADING, style);

            //Numeric 0DP
            style = GetNumericBaseStyle(SpreadsheetStyleManager.DEFAULT_NUMERIC_FORMAT, null, null);
            SaveStyle(STYLE_NUMERIC_0DP, style);

            //Numeric 4DP 
            style = GetNumericBaseStyle(SpreadsheetStyleManager.DEFAULT_NUMERIC_FORMAT_4DP, null, null);
            SaveStyle(STYLE_NUMERIC_4DP, style);

            //Grey Numeric 0DP
            style = GetNumericBaseStyle(SpreadsheetStyleManager.DEFAULT_NUMERIC_FORMAT,Color.LightGray, null);
            SaveStyle(STYLE_GREY_NUMERIC_0DP, style);

            //Numeric 0DP Single
            style = GetNumericBaseStyle(SpreadsheetStyleManager.DEFAULT_NUMERIC_FORMAT, null, false);
            SaveStyle(STYLE_NUMERIC_0DP_SINGLE, style);

            //Grey Numeric 0DP Double
            style = GetNumericBaseStyle(SpreadsheetStyleManager.DEFAULT_NUMERIC_FORMAT, null, true);
            SaveStyle(STYLE_NUMERIC_0DP_DOUBLE, style);

            //Grey Percentage 2DP
            style = GetNumericBaseStyle(SpreadsheetStyleManager.DEFAULT_PERCENTAGE_FORMAT, Color.LightGray, null);
            SaveStyle(STYLE_GREY_PERCENTAGE_2DP, style);

            //Grey Percentage 2DP Single
            style = GetNumericBaseStyle(SpreadsheetStyleManager.DEFAULT_PERCENTAGE_FORMAT, Color.LightGray, false);
            SaveStyle(STYLE_GREY_PERCENTAGE_2DP_SINGLE, style);

            //Grey Percentage 2DP Double
            style = GetNumericBaseStyle(SpreadsheetStyleManager.DEFAULT_PERCENTAGE_FORMAT, Color.LightGray, false);
            SaveStyle(STYLE_GREY_PERCENTAGE_2DP_DOUBLE, style);

            //MARKET_VALUE_DIFF_NAV
            style = GetNumericBaseStyle(SpreadsheetStyleManager.CreateFormatWithHighlightingPercent(_marketValueDiffNAVHighlightPoint), Color.LightGray, null);
            SaveStyle(STYLE_MARKET_VALUE_DIFF_NAV, style);

            //MARKET_VALUE_DIFF";
            style = GetNumericBaseStyle(SpreadsheetStyleManager.CreateFormatWithHighlightingPercent(_marketValueDiffHighlightPoint), Color.LightGray, null);
            SaveStyle(STYLE_MARKET_VALUE_DIFF, style);

            //PRICE_DIFF
            style = GetNumericBaseStyle(SpreadsheetStyleManager.CreateFormatWithHighlightingPercent(_priceDiffHighlightPoint), Color.LightGray, null);
            SaveStyle(STYLE_PRICE_DIFF, style);

        }
        private static readonly decimal _marketValueDiffNAVHighlightPoint = 0.0001m; //0.1%
        private static readonly decimal _marketValueDiffHighlightPoint = 0.1m; //10%
        private static readonly decimal _priceDiffHighlightPoint = 0.01m; //1%

        private SpreadsheetStyle GetNumericBaseStyle(string numberFormat, Color? backgroundColour, bool? doubleBorder)
        {
            SpreadsheetStyle style = GetBaseSpreadSheetStyle();
            if (backgroundColour.HasValue)
            {
                style.SetBackgroundColor(SpreadsheetStyleManager.GetRBGColor(backgroundColour.Value));
            }
            if (doubleBorder.HasValue)
            {
                style.SetBorderTop(SpreadsheetStyleManager.GetRBGColor(Color.Black), OX.BorderStyleValues.Thin);
                OX.BorderStyleValues bottomBorder = OX.BorderStyleValues.Thin;
                if (doubleBorder.Value)
                {
                    bottomBorder = OX.BorderStyleValues.Double;
                }
                style.SetBorderBottom(SpreadsheetStyleManager.GetRBGColor(Color.Black), bottomBorder);
            }
            style.SetFormat(numberFormat);
            return style;
        }

        private SpreadsheetStyle GetTableHeadingBaseStyle()
        {
            SpreadsheetStyle style = GetBaseSpreadSheetStyle();
            style.SetBackgroundColor(SpreadsheetStyleManager.GetRBGColor(Color.SteelBlue));
            style.SetColor(SpreadsheetStyleManager.GetRBGColor(Color.White));
            style.SetBorder(SpreadsheetStyleManager.GetRBGColor(Color.LightGray), OX.BorderStyleValues.Thin);
            style.SetHorizontalAlignment(OX.HorizontalAlignmentValues.Center);
            return style;
        }


        private SpreadsheetDocument Document { get; set; }

        private Dictionary<string, SpreadsheetStyle> StylesByName = new Dictionary<string, SpreadsheetStyle>();


        public void SaveStyle(string name, SpreadsheetStyle style)
        {
            if (StylesByName.Keys.Contains(name))
            {
                throw new ApplicationException(String.Format("Style {0} already exists", name));
            }
            StylesByName.Add(name, style);
        }

        public SpreadsheetStyle GetStyle(string name)
        {
            SpreadsheetStyle style;
            if (!StylesByName.TryGetValue(name, out style))
            {
                throw new ApplicationException(String.Format("Style {0} does not exist", name));
            }
            return style;
        }

        public static string GetRBGColor(Color colour)
        {


            return String.Format("{0:X2}{1:X2}{2:X2}", colour.R, colour.G, colour.B);

        }

        public static readonly string DEFAULT_NUMERIC_FORMAT = @"#,###;-#,###;-";
        public static readonly string DEFAULT_NUMERIC_FORMAT_2DP = @"#,###.00;-#,###.00;-";
        public static readonly string DEFAULT_NUMERIC_FORMAT_4DP = @"#,##0.0000;-#,###.0000;-";
        public static readonly string DEFAULT_PERCENTAGE_FORMAT = @"0.00%;-0.00%;-";


        public static string CreateFormatWithHighlightingPercent(decimal highlightDifferencePoint)
        {
            return String.Format("[Red][>{0}]0.00%;[=0]-;0.00%", highlightDifferencePoint);
        }


        public SpreadsheetStyle GetBaseSpreadSheetStyle()
        {

            return SpreadsheetReader.GetDefaultStyle(Document);

        }
    }




}