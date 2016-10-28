using RealtimeSpreadMonitor.Model;
using RealtimeSpreadMonitor.Mongo;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace RealtimeSpreadMonitor
{

    

    public class PortfolioGroupsStruct
    {
        public int idPortfolioGroup;
        public String portfolioName;
        public bool selected;
    };

    public class FCM_POFFIC_PACCTxxx
    {
        public string FCM_POFFIC;
        public string FCM_PACCT;
    }

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

    

    //public class PortfolioGroupAllocationxx
    //{
    //    //public int idportfoliogroupAllocation { get; set; }
    //    public int idportfoliogroup { get; set; }
    //    public int multiple { get; set; }
    //    public string broker { get; set; }
    //    public string account { get; set; }
    //    public string cfgfile { get; set; }

    //    public string FCM_POFFIC_PACCT { get; set; }
    //    //public List<FCM_POFFIC_PACCT> FCM_POFFIC_PACCT_List = new List<FCM_POFFIC_PACCT>();

    //    public ConcurrentDictionary<string, FCM_POFFIC_PACCT> FCM_POFFIC_PACCT_hashset = 
    //        new ConcurrentDictionary<string, FCM_POFFIC_PACCT>();

    //    public Dictionary<string, string> instrumentAcctHashSet;

    //    public bool useConfigFile = false;
    //}

    

    public class LiveRowInfoIdx
    {
        public int legIdx = -1;
        public int rowIdx;
    }


    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   Option strategy.  </summary>
    ///
    /// <remarks>   Steve Pickering, 7/15/2013. </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    public class OptionStrategy
    {
        public bool supplementContract = false;

        public int idPortfoliogroup;
        public int idStrategy;
        public int idinstrument;
        //public String strategyName;
        //public String strategyFolder;

        /// <summary> Array of index of instrument in instruments </summary>
        public int indexOfInstrumentInInstrumentsArray;
        public Instrument_mongo instrument;

        public DateTime dateTime;

        public OptionStrategyParameter[] optionStrategyParameters;

        public bool holdsCurrentPosition;
        public int idxOfStratsHoldingPositions = -1;

        public double lockedIn_R;

        public LegInfo[] legInfo;

        public int idxOfFutureLeg;

        public LegData[] legData;

        public RollLegInfo[] rollIntoLegInfo;

        public LegData[] rollIntoLegData;

        public bool rollStrikesUpdated;

        public int[] liveGridRowLoc;

        public int[] orderGridRowLoc;


        public List<LiveRowInfoIdx> rollStrikeGridRows;

        public LiveSpreadTotals deltaHedgeTotals = new LiveSpreadTotals();
        public LiveSpreadTotals liveSpreadTotals = new LiveSpreadTotals();

        

        public List<TheoreticalBar> syntheticCloseTheoretical = new List<TheoreticalBar>();

        //modeling variables
        public double syntheticClose;
        public bool syntheticCloseFilled = false;

        //used for the order page to display the type of order
        public SPREAD_POTENTIAL_OPTION_ACTION_TYPES actionType;

        //public RealtimeSpreadMonitor.Forms.OrderChartForm orderChart;  // = new RealtimeSpreadMonitor.Forms.OrderChart();

        public bool wroteIntradaySnapshotToDB;

        public int idSignalType;
        public int idRiskType;
    }

    public class OptionStrategyParameter
    {

        public TBL_DB_PARSE_PARAMETER parseParameter;
        public TBL_STRATEGY_STATE_FIELDS strategyStateFieldType;

        public String stateValueStringNotParsed;
        
        public double stateValue;

        public double[] stateValueParsed;

    }



    public class LegInfo
    {
        public OPTION_SPREAD_CONTRACT_TYPE legContractType;

        //option info
        public long idOption;
        public String optionName;
        public char optionMonth;
        public int optionMonthInt;
        public int optionYear;
        public double optionStrikePrice;
        public char optionCallOrPut;
        public int idUnderlyingContract;

        //future info
        public long idContract;
        public String contractName;
        public char contractMonth;
        public int contractMonthInt;
        public int contractYear;
        
        //common to both futures and options
        public DateTime expirationDate;

        //if this part of the ADM data it is filled by fillInRestOfADMWebInputData
        private String xcqgSymbol;

        public String cqgsymbol
        {
            set { xcqgSymbol = value; }
            get { return xcqgSymbol; }
        }

        public bool useSubstitueSymbolEOD;
        public String instrumentSymbolPreEOD;
        public String instrumentSymbolEODSubstitute;
        
        public String cqgSubstituteSymbol;
        public String cqgSubstituteSymbolWithoutStrike_ForRollover;

        public String cqgSymbolWithoutStrike_ForRollover;

        public DateTime optionExpirationTime;

    }

    public class Instrument_DefaultFutures
    {
        public int idinstrument;

        public Instrument_mongo instrument;

        public LegInfo[] defaultFutures;

        public LegData[] defaultFuturesData;
    }

    public class RollLegInfo : LegInfo
    {
        public int strikeLevelOffsetForRoll;

        public int strikeIndexOfStrikeRange = TradingSystemConstants.STRIKE_PRICE_REFERENCE_COUNT/2;
        //public double[] futurePriceReference = new double[TradingSystemConstants.STRIKE_PRICE_REFERENCE_COUNT];
        public double[] optionStrikePriceReference = new double[TradingSystemConstants.STRIKE_PRICE_REFERENCE_COUNT];

        //future info
        public double futurePriceUsedToCalculateStrikes;
        public double futurePriceFactor;

        public bool reachedBarAfterDecisionBar_ForRoll;

        public double[,] futurePriceRule = new double[2, TradingSystemConstants.STRIKE_PRICE_REFERENCE_COUNT];

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
        
        //public double hedgeTotal;
        //public double yesterdayPL;
        //public double yesterdayHedgePL;
        //public double deltaTotalForOrdersPage;
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



    public class OHLCData
    {
        public DateTime barTime;
        public double open;
        public double high;
        public double low;
        public double close;
        public int volume;
        //public int cumulativeVolume;
        
        public bool errorBar;
    };

    public class TheoreticalBar
    {
        public DateTime barTime;
        public double price;
    }

    public class TradeCalendarData
    {
        public DateTime tcDateTime;
        public int tcTypeId;
    };

    public class TradeCalendarDescription
    {
        public String tcDayTypeDescription;
        public int tcDayTypeIdDescription;
        public String tcTimeDescription;
    };

    public class OptionInputFieldsFromTblOptionInputSymbols
    {
        public int idOptionInputSymbol;
        public String optionInputCQGSymbol;
        public int idinstrument;
        public int idOptionInputType;
        public double multiplier;
    }

    

    


    public class RealtimeMonitorSettings
    {
        public REALTIME_PRICE_FILL_TYPE realtimePriceFillType = REALTIME_PRICE_FILL_TYPE.PRICE_DEFAULT;
        public bool eodAnalysis = false;
        public bool alreadyWritten = false;
    }

    public class RealtimeSystemResults
    {
        public int idPortfoliogroup;
        public int idStrategy;
        public DateTime date;
        public StringBuilder plDayChg       = new StringBuilder();
        public StringBuilder deltaDay       = new StringBuilder();
        public StringBuilder dfltPrice      = new StringBuilder();
        public StringBuilder theorPrice     = new StringBuilder();
        public StringBuilder spanImplVol    = new StringBuilder();
        public StringBuilder settleImplVol  = new StringBuilder();
        public StringBuilder settlement     = new StringBuilder();
        public StringBuilder syntheticClose = new StringBuilder();
        public StringBuilder transPrice     = new StringBuilder();
        public StringBuilder transTime      = new StringBuilder();
        public StringBuilder entryRule      = new StringBuilder();
        public StringBuilder exitRule       = new StringBuilder();

    }

    public class RealtimeSystemPLResults
    {
        public int fcmData;

        public int idPortfoliogroup;
        public string contract;
        public double strikePrice;
        public long idinstrument;       
        public DateTime date;
        
        public double plDayChg;
        public double plLongOrderChg;
        public double plShortOrderChg;


        public double plSettleDayChg;
        public double plLongSettleOrderChg;
        public double plShortSettleOrderChg;


        public int totalQty;
        public int longOrders;
        public int shortOrders;

        public double longTransAvgPrice;
        public double shortTransAvgPrice;

        public double delta;
        public double defaultPrice;
        public double theoreticalPrice;
        public double spanImpliedVol;
        public double settlementImpliedVol;
        public double impliedVol;
        public double bid;
        public double ask;
        public double last;
        public double settle;
        public double yesterdaySettle;
        public long idContract = -1;
        public long idOption = -1;
        public char callOrPutOrFutureChar; 
       
        public string officeAcct;
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
