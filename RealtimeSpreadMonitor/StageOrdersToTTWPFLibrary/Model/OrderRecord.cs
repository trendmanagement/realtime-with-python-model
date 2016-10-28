﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageOrdersToTTWPFLibrary.Model
{
    class OrderRecord : NotifyPropertyChangedBase
    {
        public QuickFix.FIX42.NewOrderSingle OriginalNOS { get; private set; }

        public OrderRecord(QuickFix.FIX42.NewOrderSingle nos)
        {
            OriginalNOS = nos;

            decimal price = -1;
            if (nos.OrdType.Obj == QuickFix.Fields.OrdType.LIMIT && nos.IsSetPrice())
                price = nos.Price.Obj;

            ClOrdID = nos.ClOrdID.Obj;
            Symbol = nos.Symbol.Obj;
            Side = FixEnumTranslator.Translate(nos.Side);
            OrdType = FixEnumTranslator.Translate(nos.OrdType);
            Price = price;
            Status = "New";

            if (nos.IsSetField(167))
            {
                SecurityType = FixEnumTranslator.Translate(nos.SecurityType);
            }

            if (nos.IsSetField(201))
            {
                CallOrPut = FixEnumTranslator.Translate(nos.PutOrCall);
            }

            if (nos.IsSetField(202))
            {
                Strike = nos.StrikePrice.Obj;
            }
        }

        private string _clOrdID = "";
        public string ClOrdID
        {
            get { return _clOrdID; }
            set { _clOrdID = value; OnPropertyChanged("ClOrdID"); }
        }

        private string _symbol = "";
        public string Symbol
        {
            get { return _symbol; }
            set { _symbol = value; OnPropertyChanged("Symbol"); }
        }

        private string _side = "";
        public string Side
        {
            get { return _side; }
            set { _side = value; OnPropertyChanged("Side"); }
        }

        private string _ordType = "";
        public string OrdType
        {
            get { return _ordType; }
            set { _ordType = value; OnPropertyChanged("OrdType"); }
        }

        private decimal _price = 0m;
        public decimal Price
        {
            get { return _price; }
            set { _price = value; OnPropertyChanged("Price"); }
        }

        private string _status { get; set; }
        public string Status
        {
            get { return _status; }
            set { _status = value; OnPropertyChanged("Status"); }
        }

        private string _orderID = "(unset)";
        public string OrderID
        {
            get { return _orderID; }
            set { _orderID = value; OnPropertyChanged("OrderID"); }
        }

        private string _securityType { get; set; }
        public string SecurityType
        {
            get { return _securityType; }
            set { _securityType = value; OnPropertyChanged("SecurityType"); }
        }

        private string _callOrPut { get; set; }
        public string CallOrPut
        {
            get { return _callOrPut; }
            set { _callOrPut = value; OnPropertyChanged("CallOrPut"); }
        }

        private decimal _strike = 0m;
        public decimal Strike
        {
            get { return _strike; }
            set { _strike = value; OnPropertyChanged("Strike"); }
        }
    }
}
