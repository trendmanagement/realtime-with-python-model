using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using RealtimeSpreadMonitor.Mongo;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace RealtimeSpreadMonitor.Model
{
    public class AccountAllocation_Mongo
    {
        public string broker { get; set; }
        public string account { get; set; }
        public string FCM_OFFICE { get; set; }
        public string FCM_ACCT { get; set; }

        //public int multiple { get; set; } = 1;
    }

    public class AccountAllocation : AccountAllocation_Mongo
    {

        public string FCM_POFFIC_PACCT { get; set; }

        public bool visible { get; set; } = true;

        public int acctIndex_UsedForTotals_Visibility = 0;
    }

    public class PortfolioAllocation_Mongo
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId _id { get; set; }

        public int idportfoliogroup { get; set; }

        public List<AccountAllocation_Mongo> accountAllocation { get; set; }
    }

    public class PortfolioAllocation
    {
        public int idportfoliogroup { get; set; }

        public List<AccountAllocation> accountAllocation = new List<AccountAllocation>();

        public Dictionary<Tuple<string,string>, AccountAllocation> accountAllocation_KeyOfficAcct =
            new Dictionary<Tuple<string, string>, AccountAllocation>();

        public Dictionary<string, AccountAllocation> accountAllocation_KeyAccountname =
            new Dictionary<string, AccountAllocation>();

        public int brokerAccountChosen { get; set; }

    }

    public class AccountInfo
    {
        public double size_factor { get; set; }
    }

    public class Account
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId _id { get; set; }

        public string campaign_name { get; set; }

        public string client_name { get; set; }

        public string mmclass_name { get; set; }

        public string name { get; set; }

        public AccountInfo info { get; set; }
    }

    public class Asset
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId _id { get; set; }

        /// <summary>
        /// option specific fields
        /// </summary>
        public long idoption { get; set; }

        public string optionname { get; set; }

        public char optionmonth { get; set; }

        public int optionmonthint { get; set; }

        public int optionyear { get; set; }

        public double strikeprice { get; set; }

        public char callorput { get; set; }

        /// <summary>
        /// future specific fields
        /// </summary>
        public string contractname { get; set; }

        public char month { get; set; }

        public int monthint { get; set; }

        public int year { get; set; }


        /// <summary>
        /// used for both futures and options
        /// </summary>
        public long idcontract { get; set; }



        public string name { get; set; }

        public string cqgsymbol { get; set; }


        /// <summary>
        /// used to specify future = fut or option = opt
        /// </summary>
        public string _type { get; set; }

        public DateTime expirationdate { get; set; }

        public long idinstrument { get; set; }

        public double yearFraction { get; set; }


        public DateTime optionExpirationTime { get; set; }
    }

    public class Position
    {
        //[BsonId]
        //public string name { get; set; }

        public int qty { get; set; }

        public int prev_qty { get; set; }

        public Asset asset { get; set; }

        public MongoDB_OptionSpreadExpression mose;

        public bool comparedTo_Archive_OnInitialization = false;

        //public double intradayPl;
        //public double settlementToSettlementPl;
        //public double delta;

        public LiveSpreadTotals positionTotals = new LiveSpreadTotals();
    }

    public class AccountPosition
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId _id { get; set; }

        public DateTime date_now { get; set; }

        public string campaign_name { get; set; }

        public string client_name { get; set; }

        public string mmclass_name { get; set; }

        public string name { get; set; }

        public AccountInfo info { get; set; }

        public List<Position> positions { get; set; }
    }

    public class OrderSummary_AccountPosition
    {
        public AccountPosition accountPosition;
        public Position position;
        public bool tested;
    }

    public class Contract_mongo
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId _id { get; set; }

        public string cqgsymbol { get; set; }

        public string contractname { get; set; }

        public long idcontract { get; set; }

        public char month { get; set; }

        public DateTime expirationdate { get; set; }

        public int monthint { get; set; }

        public int year { get; set; }

        public long idinstrument { get; set; }
    }

    public class Option_mongo
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId _id { get; set; }

        public string cqgsymbol { get; set; }

        public int optionyear { get; set; }

        public long idinstrument { get; set; }

        public long idcontract { get; set; }

        public double strikeprice { get; set; }

        public char callorput { get; set; }

        public char optionmonth { get; set; }

        public long idoption { get; set; }

        public DateTime expirationdate { get; set; }

        public string optionname { get; set; }

        public int optionmonthint { get; set; }
    }

    public class Instrument_mongo
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId _id { get; set; }

        public long idinstrument { get; set; }

        public string symbol { get; set; }

        public string description { get; set; }

        public string cqgsymbol { get; set; }

        public string exchangesymbol { get; set; }

        public string optionexchangesymbol { get; set; }

        public string exchangesymbolTT { get; set; }

        public string optionexchangesymbolTT { get; set; }

        public long idinstrumentgroup { get; set; }

        public long idexchange { get; set; }

        public long margin { get; set; }

        public double commissionpercontract { get; set; }

        public byte modeled { get; set; }

        public byte enabled { get; set; }

        public short listedspread { get; set; }

        public DateTime datastart { get; set; }

        public int timeshifthours { get; set; }

        public double ticksize { get; set; }

        public double tickdisplay { get; set; }

        public double tickvalue { get; set; }

        public double optionstrikeincrement { get; set; }

        public double optionstrikedisplay { get; set; }

        public double optionstrikedisplayTT { get; set; }

        public double optionticksize { get; set; }

        public double optiontickdisplay { get; set; }

        public double optiontickvalue { get; set; }

        public double secondaryoptionticksize { get; set; }

        public double secondaryoptiontickvalue { get; set; }

        public double secondaryoptiontickdisplay { get; set; }

        public double secondaryoptionticksizerule { get; set; }

        public double spanticksize { get; set; }

        public double spantickdisplay { get; set; }

        public double spanstrikedisplay { get; set; }

        public double spanoptionticksize { get; set; }

        public double spanoptiontickdisplay { get; set; }

        public double optionadmstrikedisplay { get; set; }

        public double admoptionftpfilestrikedisplay { get; set; }

        public DateTime optionstart { get; set; }

        public DateTime spanoptionstart { get; set; }

        public byte stoptype { get; set; }

        public long pricebandinticks { get; set; }

        public long limittickoffset { get; set; }

        public DateTime customdayboundarytime { get; set; }

        public short usedailycustomdata { get; set; }

        public short decisionoffsetminutes { get; set; }

        public byte optionenabled { get; set; }

        public byte productionenabled { get; set; }

        public string admcode { get; set; }

        public string admexchangecode { get; set; }

        public double admfuturepricefactor { get; set; }

        public double admoptionpricefactor { get; set; }

        public string spanfuturecode { get; set; }

        public string spanoptioncode { get; set; }

        public byte optiondatamonthscollected { get; set; }

        public string notes { get; set; }

        public short idAssetClass { get; set; }

        public short substitutesymbol_eod { get; set; }

        public string instrumentsymbol_pre_eod { get; set; }

        public string instrumentsymboleod_eod { get; set; }

        public int instrumentid_eod { get; set; }

        public DateTime settlementtime { get; set; }


        /// <summary>
        /// everything below is not filled in from the database
        /// </summary>

        public Exchange_mongo exchange;


        public DateTime settlementTime;

        public bool eodAnalysisAtInstrument;

        public DateTime settlementDateTimeMarker;

        //public string tradingTechnologiesExchange;

        //public string tradingTechnologiesGateway;

        public string coreAPImarginId;

        public string coreAPI_FCM_marginId;

        public double coreAPIinitialMargin;

        public double coreAPImaintenanceMargin;

        public double coreAPI_FCM_initialMargin;

        public double coreAPI_FCM_maintenanceMargin;

        /// <summary>
        /// used to caculate and store the instrument totals
        /// </summary>
        public List<LiveSpreadTotals> instrumentModelCalcTotals_ByAccount = new List<LiveSpreadTotals>();
        public List<LiveSpreadTotals> instrumentSpreadTotals_ByAccount = new List<LiveSpreadTotals>();

        public List<LiveSpreadTotals> instrumentADMCalcTotalsByAccount = new List<LiveSpreadTotals>();
        public List<LiveSpreadTotals> instrumentADMSpreadTotalsByAccount = new List<LiveSpreadTotals>();


        /// <summary>
        /// used for margin summary values
        /// </summary>
        public Instrument_Summary_Values instrument_summary_values = new Instrument_Summary_Values();
    }

    public class Instrument_Summary_Values
    {
        public double margin_summary = 0;

        public double span_init_margin = 0;

        public double span_maint_margin = 0;

        public double span_init_fcm_margin = 0;

        public double span_maint_fcm_margin = 0;
    }

    public class Exchange_mongo
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId _id { get; set; }

        public long idexchange { get; set; }

        public string exchange { get; set; }

        public string spanexchangesymbol { get; set; }

        public string spanexchwebapisymbol { get; set; }

        public string tradingtechnologies_exchange { get; set; }

        public string tradingtechnologies_gateway { get; set; }
    }

    public class InitializationParms
    {
        public int tmlSystemRunType;
        public bool runLiveSystem;
        public bool connectToXtrader;
        public bool initializePersistence;
        public bool runFromDb;
        public String dbServerName;

        public int idPortfolioGroup;

        public String portfolioGroupName;

        public DateTime modelDateTime;

        public bool useHalfday;
        public DateTime halfDayTransactionTime;
        public int halfDayDecisionOffsetMinutes;

        public String[] initializationConfigs;

        public int FIX_OrderPlacementType;

        public bool useCloudDb = false;
    };
}
