using RealtimeSpreadMonitor.Model;
using RealtimeSpreadMonitor.Mongo;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace RealtimeSpreadMonitor
{
    public class InstrumentSpecificFIXFields
    {
        public string FCM;
        public string TTGateway;
        public string TTExchange;
        public string TTSymbol;

        public char TAG_47_Rule80A;
        public int TAG_204_CustomerOrFirm;
        public string TAG_18205_TTAccountType;
        public string TAG_440_ClearingAccount;
        public string TAG_16102_FFT2;
    }
    

    public class LiveSpreadTotals
    {
        public double delta;
        
        public double pAndLDay;
        public double pAndLDayOrders;        

        public double pAndLDaySettlementToSettlement;
        public double pAndLDaySettleOrders;


        //USED FOR SAVING FCM DATA TO DATABASE
        public double pAndLLongOrders;
        public double pAndLShortOrders;

        public double pAndLLongSettlementOrders;
        public double pAndLShortSettlementOrders;

        public double longTransAvgPrice;
        public double shortTransAvgPrice;


        public double initialMarginTotals;
        public double maintenanceMarginTotals;

        public double initialFCM_MarginTotals;
        public double maintenanceFCM_MarginTotals;
        
    }

    public class LegData : LiveSpreadTotals
    {
        public DateTime dataDateTimeFromDB;
        public double settlementPriceFromDB;
        public double impliedVolFromDB;
        public double timeToExpInYearsFromDB;

        //public double delta;
        //public double pAndL;

        public MongoDB_OptionSpreadExpression optionSpreadExpression;

        public MongoDB_OptionSpreadExpression optionSpreadSymbolSubstituteExpression;
    }


    //public class OptionInputFieldsFromTblOptionInputSymbols
    //{
    //    public int idOptionInputSymbol;
    //    public String optionInputCQGSymbol;
    //    public int idinstrument;
    //    public int idOptionInputType;
    //    public double multiplier;
    //}


    public class RealtimeMonitorSettings
    {
        public REALTIME_PRICE_FILL_TYPE realtimePriceFillType = REALTIME_PRICE_FILL_TYPE.PRICE_DEFAULT;
        public bool eodAnalysis = false;
        public bool alreadyWritten = false;
    }


    public class OptionChartMargin
    {
        public OPTION_SPREAD_CONTRACT_TYPE contractType;
        
        public int optionYear;
        public int optionMonthInt;
        
        public int contractYear;
        public int contractMonthInt;

        public double strikePrice;
        public Instrument_mongo intrument;
        public int numberOfContracts;

    }


    
}
