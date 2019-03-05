using System;
using System.Collections.Generic;

namespace Odey.ReconciliationServices.AttributionReconciliationService
{
    public class ReturnComparison
    {
        public bool ValuesExist = false;
        public decimal CorrectReturn { get; private set; }
        public decimal ReturnToCompare { get; private set; }
        public decimal ReturnDifference { get; private set; }
        public bool ReturnWithinTolerance { get; private set; }
        public decimal CorrectValue { get; private set; }
        public decimal ValueToCompare { get; private set; }
        public decimal ValueDifference { get; private set; }
        public bool ValueWithinTolerance { get; private set; }
        public IEnumerable<SimpleComparison> CurrencyDifferences { get; private set; }
        public IEnumerable<SimpleComparison> InstrumentDifferences { get; private set; }
        public string FileName { get; private set; }

        public ReturnComparison(decimal correctReturn, decimal returnToCompare, decimal returnTolerance, decimal correctValue, decimal valueToCompare, decimal valueTolerance, string fileName,
            IEnumerable<SimpleComparison> currencyDifferences, IEnumerable<SimpleComparison> instrumentDifferences)
            : this(correctReturn, returnToCompare, returnTolerance)
        {
            CorrectValue = correctValue;
            ValueToCompare = valueToCompare;
            ValueDifference = GetDifference(CorrectValue, ValueToCompare);
            ValueWithinTolerance = WithinTolerance(ValueDifference, valueTolerance);
            ValuesExist = true;
            FileName = fileName;
            CurrencyDifferences = currencyDifferences;
            InstrumentDifferences = instrumentDifferences;
        }

        public ReturnComparison(decimal correctReturn, decimal returnToCompare, decimal returnTolerance)
        {
            CorrectReturn = correctReturn;

            ReturnToCompare = returnToCompare;
            ReturnDifference = GetDifference(CorrectReturn, ReturnToCompare);
            ReturnWithinTolerance = WithinTolerance(ReturnDifference, returnTolerance);
        }

        private decimal GetDifference(decimal correct, decimal toComare)
        {
            return correct - toComare;
        }

        private bool WithinTolerance(decimal difference, decimal tolerance)
        {
            return Math.Abs(difference) < tolerance;
        }
    }
}