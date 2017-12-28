using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using RealtimeSpreadMonitor.Model;

namespace RealtimeSpreadMonitor.Mongo
{
    class MongoObjectsForRealtime
    {
    }

    public class MongoDB_OptionSpreadExpression
    {
        //public CQG.CQGInstrument cqgInstrument;
        //public CQG.CQGTimedBars futureTimedBars;

        public int lastTimedBarInIndex = 0;

        public Asset asset;

        //public bool continueUpdate = true;

        public Instrument_mongo instrument;



        //public OptionInputFieldsFromTblOptionInputSymbols optionInputFieldsFromTblOptionInputSymbols;



        //public bool normalSubscriptionRequest = false;
        public bool substituteSubscriptionRequest = false;

        public bool useSubstituteSymbolAtEOD = false;

        //this is used for margin calculation from option payoff chart
        //******************************************
        public int optionMonthInt;
        public int optionYear;

        public int futureContractMonthInt;
        public int futureContractYear;
        //******************************************

        public bool setSubscriptionLevel = false;
        public bool requestedMinuteBars = false;

        public OPTION_EXPRESSION_TYPES optionExpressionType;

        public OPTION_SPREAD_CONTRACT_TYPE callPutOrFuture;

        public char callPutOrFutureChar;


        //public long optionId; //only filled if contract is an option
        //public long underlyingFutureId;

        public long substituteOptionId; //only filled if contract is an option
        public long substituteUnderlyingFutureId;


        //public long futureId; //only filled if contract is a future
        public long substituteFutureId;


        //public double strikePrice;

        public double riskFreeRate = 0.01;
        public bool riskFreeRateFilled = false;

        //public double yearFraction;

        public DateTime lastTimeUpdated;

        public double minutesSinceLastUpdate = 0;

        public DateTime lastTimeFuturePriceUpdated; //is separate b/c can get time stamp off of historical bars







        public double ask;
        public bool askFilled;

        public double bid;
        public bool bidFilled;

        public double trade;
        public bool tradeFilled;

        public double settlement;
        public bool settlementFilled;
        public bool manuallyFilled;
        public DateTime settlementDateTime;
        public bool settlementIsCurrentDay;


        /// <summary>
        /// the following variables are filled from the SQL db
        /// </summary>
        public double yesterdaySettlement;
        public DateTime yesterdaySettlementDateTime;
        public bool yesterdaySettlementFilled;


        public double impliedVolFromSpan;




        public double defaultBidPriceBeforeTheor;


        public double defaultAskPriceBeforeTheor;
      

        public double defaultMidPriceBeforeTheor;

        public double defaultPrice;
        public bool defaultPriceFilled;


        public bool lastBarAnalyzed = false;

        public DateTime todayDecisionTime;
        public double decisionPrice;
        public DateTime decisionPriceTime;
        public bool decisionPriceTimePassed = false;
        public bool decisionPriceFilled = false;


        public DateTime todayTransactionTimeBoundary;
        public double transactionPrice;
        public DateTime transactionPriceTime;
        public bool transactionPriceTimePassed = false;
        public bool transactionPriceFilled = false;

        //public bool filledAfterTransactionTimeBoundary = false;





        public double theoreticalOptionPrice;

        public double settlementImpliedVol;

        public double impliedVol;

        public bool impliedVolFilled = false; //used for calculating option transaction price

        public double delta;
        //public double gamma;
        //public double vega;
        //public double theta;



        //public List<int> spreadIdx = new List<int>();
        //public List<int> legIdx = new List<int>();
        //public List<int> rowIdx = new List<int>();


        //public List<int> substituteSymbolSpreadIdx = new List<int>();
        //public List<int> substituteSymbolLegIdx = new List<int>();
        //public List<int> substituteSymbolRowIdx = new List<int>();

        public MongoDB_OptionSpreadExpression mainExpressionSubstitutionUsedFor;


        //public int transactionBarIdx;
        //public CQG.CQGTimedBar todayTransactionBar;
        
        //public bool reachedTransactionTimeBoundary = false;
        

        //public int decisionBarIdx;
        //public CQG.CQGTimedBar decisionBar;
        //public DateTime todayDecisionTime;
        //public bool reachedDecisionBar = false;
        //public bool reachedBarAfterDecisionBar = false;
        //public bool reached1MinAfterDecisionBarUsedForSnapshot = false;

        //public DateTime settlementDateTimeMarker;
        //public bool reachedSettlementDateTimeMarker;


        //public CQG_REFRESH_STATE guiRefresh = CQG_REFRESH_STATE.NOTHING;
        public CQG_REFRESH_STATE totalCalcsRefresh = CQG_REFRESH_STATE.NOTHING;

        public MongoDB_OptionSpreadExpression underlyingFutureExpression;

        public List<MongoDB_OptionSpreadExpression> optionExpressionsThatUseThisFutureAsUnderlying = new List<MongoDB_OptionSpreadExpression>();




        //Expression List grid
        public int dataGridExpressionListRow;

        //************************

        public MongoDB_OptionSpreadExpression(OPTION_SPREAD_CONTRACT_TYPE callPutOrFuture,
            OPTION_EXPRESSION_TYPES optionExpressionType)
        {
            this.callPutOrFuture = callPutOrFuture;
            this.optionExpressionType = optionExpressionType;

            this.asset = new Asset();

        }


    }
}
