using System;

namespace Odey.ReconciliationServices.AttributionReconciliationService
{
    public class SimpleComparison
    {
        public SimpleComparison(string name, decimal correct, decimal toCompare, decimal tolerance)
        {
            Name = name;
            Correct = correct;
            ToCompare = toCompare;
            Difference = correct - toCompare;
            DifferenceWithinTolerance = WithinTolerance(Difference, tolerance);
        }

        private bool WithinTolerance(decimal difference, decimal tolerance)
        {
            return Math.Abs(difference) < tolerance;
        }

        public string Name { get; private set; }

        public decimal Correct { get; private set; }

        public decimal ToCompare { get; private set; }

        public decimal Difference { get; private set; }

        public bool DifferenceWithinTolerance { get; private set; }
    }
}