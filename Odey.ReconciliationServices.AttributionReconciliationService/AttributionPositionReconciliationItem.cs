using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Odey.ReconciliationServices.AttributionReconciliationService
{
    public class AttributionPositionReconciliationItem : AttributionReconciliationItem
    {
        public AttributionPositionReconciliationItem(int positionId, string book, string strategy, string instrumentMarket, int instrumentMarketId, string currency, string isAccrual, int accountId, bool isCurrency) : base(isCurrency)
        {
            PositionId = positionId;
            Book = book;
            Strategy = strategy;
            InstrumentMarket = instrumentMarket;
            InstrumentMarketId = instrumentMarketId;
            Currency = currency;
            IsAccrual = isAccrual;
            AccountId = accountId;
        }
        public int PositionId { get; private set; }

        public string Book { get; private set; }

        public string Strategy { get; private set; }

        public string InstrumentMarket { get; private set; }

        public int InstrumentMarketId { get; private set; }

        public string Currency { get; private set; }

        public string IsAccrual { get; private set; }

        public int AccountId { get; private set; }

        public override List<object> Descriptor
        {
            get
            {
                return new List<object> { PositionId.ToString(), InstrumentMarket, Currency, Book, Strategy };
            }
        }

        public override List<object> Header
        {
            get
            {
                return new List<object>() { "PositionId", "InstrumentMarket", "Currency", "Book", "Strategy" };
            }
        }

        public override int CompareTo(object obj)
        {
            AttributionPositionReconciliationItem objToCompare = (AttributionPositionReconciliationItem)obj;
            int compare = Book.CompareTo(objToCompare.Book);
            if (compare == 0)
            {
                compare = Strategy.CompareTo(objToCompare.Strategy);
                if (compare == 0)
                {
                    compare = InstrumentMarket.CompareTo(objToCompare.InstrumentMarket);
                    if (compare == 0)
                    {
                        compare = Currency.CompareTo(objToCompare.Currency);
                        if (compare == 0)
                        {
                            return PositionId.CompareTo(objToCompare.PositionId);
                        }
                        else
                        {
                            return compare;
                        }
                    }
                    else
                    {
                        return compare;
                    }
                }
                else
                {
                    return compare;
                }
            }
            else
            { 
                return compare;
            }
        }
    }
}