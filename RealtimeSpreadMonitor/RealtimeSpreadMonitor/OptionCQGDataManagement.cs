using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CQG;
using RealtimeSpreadMonitor.Forms;
using System.Drawing;
using System.Threading;
using System.Collections.Concurrent;
using RealtimeSpreadMonitor.Model;
using RealtimeSpreadMonitor.Mongo;

namespace RealtimeSpreadMonitor
{
    internal class OptionCQGDataManagement
    {
        OptionSpreadManager optionSpreadManager;

        //ConcurrentDictionary<string, int> optionSpreadExpressionFutureTimedBarsListIdx = new ConcurrentDictionary<string, int>();

        //ConcurrentDictionary<string, int> optionSpreadExpressionCheckSubscribedListIdx = new ConcurrentDictionary<string, int>();
        //ConcurrentDictionary<string, int> optionSpreadExpressionListHashTableIdx = new ConcurrentDictionary<string, int>();


        CQG.CQGCEL m_CEL;

        private bool resetOrderPageOnceAllDataIn = false;


        private Thread subscriptionThread;
        private bool subscriptionThreadShouldStop = false;
        private const int SUBSCRIPTION_TIMEDELAY_CONSTANT = 125;

        private Thread calculateModelValuesAndSummarizeTotalsThread;
        private bool calculateModelValuesThreadShouldStop = false;


        public OptionCQGDataManagement(
            OptionSpreadManager optionSpreadManager,
            OptionRealtimeStartup optionRealtimeStartup)
        {
            this.optionSpreadManager = optionSpreadManager;

            //setupCalculateModelValuesAndSummarizeTotals();
        }

        public void stopDataManagementAndTotalCalcThreads()
        {
            if (subscriptionThread != null && subscriptionThread.IsAlive)
                subscriptionThreadShouldStop = true;

            if (calculateModelValuesAndSummarizeTotalsThread != null && calculateModelValuesAndSummarizeTotalsThread.IsAlive)
                calculateModelValuesThreadShouldStop = true;

        }

        public void resetThreadStopVariables()
        {
            subscriptionThreadShouldStop = false;

            calculateModelValuesThreadShouldStop = false;
        }

        public void shutDownCQGConn()
        {
            AsyncTaskListener.StatusUpdateAsync("CQG DATA DOWN", STATUS_FORMAT.ALARM, STATUS_TYPE.DATA_STATUS);

            if (m_CEL != null)
            {
                if (m_CEL.IsStarted)
                    m_CEL.RemoveAllInstruments();

                //m_CEL.Shutdown();
            }


        }

        public void connectCQG()
        {
            if (m_CEL != null)
            {
                m_CEL.Startup();
            }

            AsyncTaskListener.StatusUpdateAsync("CQG UP", STATUS_FORMAT.DEFAULT, STATUS_TYPE.DATA_STATUS);
        }

        public void resetCQG()
        {
            if (!m_CEL.IsStarted)
            {
                initializeCQGAndCallbacks();
            }
        }

        public void initializeCQGAndCallbacks()
        {
            try
            {

                m_CEL = new CQG.CQGCEL();

                //m_CEL_CELDataConnectionChg(CQG.eConnectionStatus.csConnectionDown);
                //(callsFromCQG,&CallsFromCQG.m_CEL_CELDataConnectionChg);
                m_CEL.DataConnectionStatusChanged += new CQG._ICQGCELEvents_DataConnectionStatusChangedEventHandler(m_CEL_CELDataConnectionChg);

                //m_CEL.LineTimeChanged += new CQG._ICQGCELEvents_LineTimeChangedEventHandler(m_CEL_LineTimeChanged);

                // 		//m_CEL.DataError += new _ICQGCELEvents_DataErrorEventHandler(CEL_DataError);
                //m_CEL.InstrumentsGroupResolved += new CQG._ICQGCELEvents_InstrumentsGroupResolvedEventHandler(m_CEL_InstrumentsGroupResolved);
                // 		m_CEL.InstrumentsGroupChanged += new _ICQGCELEvents_InstrumentsGroupChangedEventHandler(m_CEL_InstrumentsGroupChanged);

                m_CEL.TimedBarsResolved += new CQG._ICQGCELEvents_TimedBarsResolvedEventHandler(m_CEL_TimedBarResolved);
                m_CEL.TimedBarsAdded += new CQG._ICQGCELEvents_TimedBarsAddedEventHandler(m_CEL_TimedBarsAdded);
                //m_CEL.TimedBarsInserted += new CQG._ICQGCELEvents_TimedBarsInsertedEventHandler(m_CEL_TimedBarsInserted);
                m_CEL.TimedBarsUpdated += new CQG._ICQGCELEvents_TimedBarsUpdatedEventHandler(m_CEL_TimedBarsUpdated);

                //m_CEL.IncorrectSymbol += new _ICQGCELEvents_IncorrectSymbolEventHandler(CEL_IncorrectSymbol);
                m_CEL.InstrumentSubscribed += new _ICQGCELEvents_InstrumentSubscribedEventHandler(m_CEL_InstrumentSubscribed);
                m_CEL.InstrumentChanged += new _ICQGCELEvents_InstrumentChangedEventHandler(m_CEL_InstrumentChanged);

                m_CEL.DataError += new _ICQGCELEvents_DataErrorEventHandler(m_CEL_DataError);

                //m_CEL.APIConfiguration.NewInstrumentMode = true;

                m_CEL.APIConfiguration.ReadyStatusCheck = CQG.eReadyStatusCheck.rscOff;

                m_CEL.APIConfiguration.CollectionsThrowException = false;

                m_CEL.APIConfiguration.TimeZoneCode = CQG.eTimeZone.tzPacific;
                //m_CEL.Startup();

                connectCQG();
            }
            catch (Exception ex)
            {
                TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
            }
        }

        private void m_CEL_CELDataConnectionChg(CQG.eConnectionStatus new_status)
        {
            StringBuilder connStatusShortString = new StringBuilder();

            STATUS_FORMAT statusFormat = STATUS_FORMAT.DEFAULT;

            //if (m_CEL.IsStarted)
            {
                connStatusShortString.Append("CQG:");

                if (new_status != CQG.eConnectionStatus.csConnectionUp)
                {
                    if (new_status == CQG.eConnectionStatus.csConnectionDelayed)
                    {
                        statusFormat = STATUS_FORMAT.CAUTION;

                        connStatusShortString.Append("DELAYED");
                    }
                    else
                    {
                        statusFormat = STATUS_FORMAT.ALARM;

                        connStatusShortString.Append("DOWN");
                    }
                }
                else
                {
                    connStatusShortString.Append("UP");
                }
            }


            AsyncTaskListener.StatusUpdateAsync(connStatusShortString.ToString(), statusFormat, STATUS_TYPE.CQG_CONNECTION_STATUS);

        }

        private void m_CEL_DataError(System.Object cqg_error, System.String error_description)
        {
            AsyncTaskListener.StatusUpdateAsync("CQG ERROR", STATUS_FORMAT.ALARM, STATUS_TYPE.DATA_STATUS);
        }

        private void m_CEL_InstrumentSubscribed(String symbol, CQGInstrument cqgInstrument)
        {
            try
            {
                AsyncTaskListener.StatusUpdateAsync("CQG GOOD", STATUS_FORMAT.DEFAULT, STATUS_TYPE.DATA_STATUS);


                if (DataCollectionLibrary.optionSpreadExpressionHashTable_keySymbol.ContainsKey(symbol))
                {
                    MongoDB_OptionSpreadExpression optionSpreadExpression =
                        DataCollectionLibrary.optionSpreadExpressionHashTable_keySymbol[symbol];

                    //optionSpreadExpressionCheckSubscribedListIdx

                    //expressionCounter = optionSpreadExpressionCheckSubscribedListIdx[symbol];

                    //while (expressionCounter < optionSpreadExpressionList.Count)
                    //{
                    if (optionSpreadExpression.continueUpdate
                        && !optionSpreadExpression.setSubscriptionLevel)
                    {
                        optionSpreadExpression.setSubscriptionLevel = true;

                        optionSpreadExpression.cqgInstrument = cqgInstrument;

                        //optionSpreadExpressionListHashTableIdx.AddOrUpdate(
                        //        cqgInstrument.FullName, idx,
                        //        (oldKey, oldValue) => idx);

                        DataCollectionLibrary.optionSpreadExpressionHashTable_keyFullName.AddOrUpdate(
                                cqgInstrument.FullName, optionSpreadExpression,
                                (oldKey, oldValue) => optionSpreadExpression);

                        //if (cqgInstrument.FullName.CompareTo("P.US.EU6J1511100") == 0)
                        //{
                        //    Console.WriteLine(cqgInstrument.FullName);
                        //}

                        fillPricesFromQuote(optionSpreadExpression,
                            optionSpreadExpression.cqgInstrument.Quotes);

                        //if is an option (not a future)
                        if (optionSpreadExpression.callPutOrFuture ==
                                OPTION_SPREAD_CONTRACT_TYPE.CALL
                                || optionSpreadExpression.callPutOrFuture ==
                                OPTION_SPREAD_CONTRACT_TYPE.PUT)
                        {
                            fillDefaultMidPrice_Option(optionSpreadExpression);


                        }
                        else if (optionSpreadExpression.optionExpressionType == OPTION_EXPRESSION_TYPES.OPTION_EXPRESSION_RISK_FREE_RATE)
                        {
                            fillDefaultPrice_RiskFreeRate(optionSpreadExpression);


                        }


                        if (optionSpreadExpression.optionExpressionType
                        == OPTION_EXPRESSION_TYPES.SPREAD_LEG_PRICE)
                        {
                            if (optionSpreadExpression.callPutOrFuture ==
                                    OPTION_SPREAD_CONTRACT_TYPE.FUTURE)
                            {
                                optionSpreadExpression.cqgInstrument.DataSubscriptionLevel
                                    = eDataSubscriptionLevel.dsQuotes;
                            }
                            else
                            {
                                optionSpreadExpression.cqgInstrument.DataSubscriptionLevel
                                    = eDataSubscriptionLevel.dsQuotesAndBBA;

                                optionSpreadExpression.cqgInstrument.BBAType
                                     = eDOMandBBAType.dbtCombined;
                            }

                        }
                        else
                        {
                            //set updates for interest rate to stop
                            optionSpreadExpression.cqgInstrument.DataSubscriptionLevel
                                = eDataSubscriptionLevel.dsNone;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
            }
        }

        private void m_CEL_InstrumentChanged(CQGInstrument cqgInstrument,
                                 CQGQuotes quotes,
                                 CQGInstrumentProperties props)
        {
            try
            {
                if (DataCollectionLibrary.optionSpreadExpressionHashTable_keyFullName.ContainsKey(cqgInstrument.FullName))
                {

                    MongoDB_OptionSpreadExpression optionSpreadExpression
                        = DataCollectionLibrary.optionSpreadExpressionHashTable_keyFullName[cqgInstrument.FullName];


                    if (optionSpreadExpression != null
                        && optionSpreadExpression.continueUpdate
                        && optionSpreadExpression.cqgInstrument != null

                        && cqgInstrument.CEL != null)
                    {

                        CQGQuote quoteAsk = quotes[eQuoteType.qtAsk];
                        CQGQuote quoteBid = quotes[eQuoteType.qtBid];
                        CQGQuote quoteTrade = quotes[eQuoteType.qtTrade];
                        CQGQuote quoteSettlement = quotes[eQuoteType.qtSettlement];
                        CQGQuote quoteYestSettlement = quotes[eQuoteType.qtYesterdaySettlement];

                        if ((quoteAsk != null)
                            || (quoteBid != null)
                            || (quoteTrade != null)
                            || (quoteSettlement != null)
                            || (quoteYestSettlement != null))
                        {

                            fillPricesFromQuote(optionSpreadExpression,
                                optionSpreadExpression.cqgInstrument.Quotes);

                            if (optionSpreadExpression.callPutOrFuture ==
                                OPTION_SPREAD_CONTRACT_TYPE.CALL
                                || optionSpreadExpression.callPutOrFuture ==
                                OPTION_SPREAD_CONTRACT_TYPE.PUT)
                            {
                                fillDefaultMidPrice_Option(optionSpreadExpression);


                            }
                            else if (optionSpreadExpression.optionExpressionType == OPTION_EXPRESSION_TYPES.OPTION_EXPRESSION_RISK_FREE_RATE)
                            {
                                fillDefaultPrice_RiskFreeRate(optionSpreadExpression);


                            }

                        }

                    }
                }
            }
            catch (Exception ex)
            {
                TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
            }
        }

        /// <summary>
        /// M_s the ce l_ timed bar resolved.
        /// </summary>
        /// <param name="cqg_TimedBarsIn">The CQG_ timed bars in.</param>
        /// <param name="cqg_error">The cqg_error.</param>
        private void m_CEL_TimedBarResolved(CQG.CQGTimedBars cqg_TimedBarsIn, CQGError cqg_error)
        {
            //Debug.WriteLine("m_CEL_ExpressionResolved" + cqg_expression.Count);
            try
            {
                //TSErrorCatch.debugWriteOut(cqg_TimedBarsIn.Id);

                if (cqg_error == null)
                {
                    //int expressionCounter = 0;

                    if (DataCollectionLibrary.optionSpreadExpressionHashTable_keyCQGInId.ContainsKey(cqg_TimedBarsIn.Id))
                    {

                        MongoDB_OptionSpreadExpression ose = DataCollectionLibrary.optionSpreadExpressionHashTable_keyCQGInId[cqg_TimedBarsIn.Id];

                        //while (expressionCounter < optionSpreadExpressionList.Count)
                        //{
                        if (ose.continueUpdate
                            && ose.futureTimedBars != null)
                        {
                            ose.requestedMinuteBars = true;

                            ose.lastTimedBarInIndex = cqg_TimedBarsIn.Count - 1;

                            if (cqg_TimedBarsIn.Count > 0)
                            {

                                int timeBarsIn_CurrentDay_TransactionIdx = cqg_TimedBarsIn.Count - 1;

                                //changed decision and transaction date to the modelDateTime rather than
                                //the date of the latest bar. Because it was failing on instruments like cattle
                                //and hogs that don't have data
                                ose.todayTransactionTimeBoundary
                                    = DataCollectionLibrary.initializationParms.modelDateTime.Date
                                    .AddHours(
                                        ose.instrument.customdayboundarytime.Hour)
                                    .AddMinutes(
                                        ose.instrument.customdayboundarytime.Minute);

                                ose.todayDecisionTime
                                    = DataCollectionLibrary.initializationParms.modelDateTime.Date
                                    .AddHours(
                                        ose.instrument.customdayboundarytime.Hour)
                                    .AddMinutes(
                                        ose.instrument.customdayboundarytime.Minute
                                        - ose.instrument.decisionoffsetminutes);


                                int timedBarsInCounter = 0;

                                while (timedBarsInCounter < cqg_TimedBarsIn.Count)
                                {

                                    if (!ose.reachedTransactionTimeBoundary
                                        && cqg_TimedBarsIn[timedBarsInCounter].Timestamp
                                        .CompareTo(ose.todayTransactionTimeBoundary) <= 0)
                                    {
                                        ose.todayTransactionBar = cqg_TimedBarsIn[timedBarsInCounter];

                                    }

                                    if (!ose.reachedTransactionTimeBoundary
                                        && cqg_TimedBarsIn[timedBarsInCounter].Timestamp
                                        .CompareTo(ose.todayTransactionTimeBoundary) >= 0)
                                    {
                                        ose.reachedTransactionTimeBoundary = true;
                                    }

                                    if (!ose.reachedDecisionBar
                                        && cqg_TimedBarsIn[timedBarsInCounter].Timestamp
                                        .CompareTo(ose.todayDecisionTime) <= 0)
                                    {
                                        ose.decisionBar = cqg_TimedBarsIn[timedBarsInCounter];
                                    }

                                    if (!ose.reachedDecisionBar
                                        && cqg_TimedBarsIn[timedBarsInCounter].Timestamp
                                        .CompareTo(ose.todayDecisionTime) >= 0)
                                    {
                                        ose.reachedDecisionBar = true;
                                    }

                                    if (!ose.reachedBarAfterDecisionBar
                                        && cqg_TimedBarsIn[timedBarsInCounter].Timestamp
                                        .CompareTo(ose.todayDecisionTime) > 0)
                                    {
                                        ose.reachedBarAfterDecisionBar = true;
                                    }

                                    //if (!ose.reached1MinAfterDecisionBarUsedForSnapshot
                                    //    && cqg_TimedBarsIn[timedBarsInCounter].Timestamp
                                    //    .CompareTo(ose.todayDecisionTime.AddMinutes(1)) > 0)
                                    //{
                                    //    ose.reached1MinAfterDecisionBarUsedForSnapshot = true;
                                    //}


                                    timedBarsInCounter++;

                                }

                                int backTimeCounter = cqg_TimedBarsIn.Count - 1;

                                while (backTimeCounter >= 0)
                                {
                                    if (cqg_TimedBarsIn[backTimeCounter].Close
                                    != -TradingSystemConstants.CQG_DATA_ERROR_CODE)
                                    {
                                        ose.lastTimeFuturePriceUpdated =
                                            cqg_TimedBarsIn[backTimeCounter].Timestamp;

                                        ose.trade =
                                            cqg_TimedBarsIn[backTimeCounter].Close;

                                        ose.tradeFilled = true;

                                        break;
                                    }

                                    backTimeCounter--;
                                }



                                fillDefaultMidPrice_Future(ose);



                            }
                        }
                    }
                }
                else
                {
                    AsyncTaskListener.StatusUpdateAsync("CQG ERROR", STATUS_FORMAT.ALARM, STATUS_TYPE.DATA_STATUS);
                }
            }
            catch (Exception ex)
            {
                TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
            }
        }


        private void m_CEL_TimedBarsAdded(CQG.CQGTimedBars cqg_TimedBarsIn)
        {
            try
            {

                if (DataCollectionLibrary.optionSpreadExpressionHashTable_keyCQGInId.ContainsKey(cqg_TimedBarsIn.Id))
                {

                    MongoDB_OptionSpreadExpression ose = DataCollectionLibrary.optionSpreadExpressionHashTable_keyCQGInId[cqg_TimedBarsIn.Id];

                    if (ose.continueUpdate
                        && ose.futureTimedBars != null)
                    {
                        int lastTimedBarInIndex = ose.lastTimedBarInIndex;

                        ose.lastTimedBarInIndex = cqg_TimedBarsIn.Count - 1;


                        while (lastTimedBarInIndex < cqg_TimedBarsIn.Count)
                        {

                            if (!ose.reachedTransactionTimeBoundary
                                && cqg_TimedBarsIn[lastTimedBarInIndex].Timestamp
                                .CompareTo(ose.todayTransactionTimeBoundary) <= 0)
                            {
                                ose.todayTransactionBar = cqg_TimedBarsIn[lastTimedBarInIndex];

                            }

                            if (!ose.reachedTransactionTimeBoundary
                                && cqg_TimedBarsIn[lastTimedBarInIndex].Timestamp
                                .CompareTo(ose.todayTransactionTimeBoundary) >= 0)
                            {
                                ose.reachedTransactionTimeBoundary = true;
                            }

                            if (!ose.reachedDecisionBar
                                && cqg_TimedBarsIn[lastTimedBarInIndex].Timestamp
                                .CompareTo(ose.todayDecisionTime) <= 0)
                            {
                                ose.decisionBar = cqg_TimedBarsIn[lastTimedBarInIndex];
                            }

                            if (!ose.reachedDecisionBar
                                && cqg_TimedBarsIn[lastTimedBarInIndex].Timestamp
                                .CompareTo(ose.todayDecisionTime) >= 0)
                            {
                                ose.reachedDecisionBar = true;
                            }

                            if (!ose.reachedBarAfterDecisionBar
                                && cqg_TimedBarsIn[lastTimedBarInIndex].Timestamp
                                .CompareTo(ose.todayDecisionTime) > 0)
                            {
                                ose.reachedBarAfterDecisionBar = true;
                            }

                            //if (!ose.reached1MinAfterDecisionBarUsedForSnapshot
                            //    && cqg_TimedBarsIn[lastTimedBarInIndex].Timestamp
                            //    .CompareTo(ose.todayDecisionTime.AddMinutes(1)) > 0)
                            //{
                            //    ose.reached1MinAfterDecisionBarUsedForSnapshot = true;
                            //}


                            lastTimedBarInIndex++;

                        }

                        int lastbaridx = cqg_TimedBarsIn.Count - 1;

                        if (cqg_TimedBarsIn[lastbaridx].Close
                                    != -TradingSystemConstants.CQG_DATA_ERROR_CODE)
                        {
                            ose.lastTimeFuturePriceUpdated =
                                        cqg_TimedBarsIn[lastbaridx].Timestamp;

                            ose.trade =
                                    cqg_TimedBarsIn[lastbaridx].Close;

                            ose.tradeFilled = true;

                            fillDefaultMidPrice_Future(ose);


                        }

                    }
                }
            }
            catch (Exception ex)
            {
                TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
            }
        }

        private void m_CEL_TimedBarsUpdated(CQG.CQGTimedBars cqg_TimedBarsIn, int index)
        {
            //Debug.WriteLine("m_CEL_ExpressionResolved" + cqg_expression.Count);
            try
            {


                if (DataCollectionLibrary.optionSpreadExpressionHashTable_keyCQGInId.ContainsKey(cqg_TimedBarsIn.Id))
                {

                    MongoDB_OptionSpreadExpression ose = DataCollectionLibrary.optionSpreadExpressionHashTable_keyCQGInId[cqg_TimedBarsIn.Id];

                    if (ose.continueUpdate
                        && ose.futureTimedBars != null
                        //&& ose.futureBarData != null
                        //&& ose.futureBarData.Count > 0
                        )
                    {

                        if (index == cqg_TimedBarsIn.Count - 1
                            && cqg_TimedBarsIn[index].Close
                            != -TradingSystemConstants.CQG_DATA_ERROR_CODE)
                        {
                            ose.lastTimeFuturePriceUpdated =
                                cqg_TimedBarsIn[index].Timestamp;

                            ose.trade =
                                cqg_TimedBarsIn[index].Close;

                            ose.tradeFilled = true;

                            fillDefaultMidPrice_Future(ose);



                        }


                    }
                }


            }
            catch (Exception ex)
            {
                TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
            }
        }



        public void sendSubscribeRequest(bool sendOnlyUnsubscribed)
        {

#if DEBUG
            try
#endif
            {
                //if (this.InvokeRequired)
                {
                    //ThreadSafeSendSubscribeRequestRunDelegate d = new ThreadSafeSendSubscribeRequestRunDelegate(sendSubscribeRequestRun);

                    //optionSpreadExpressionFutureTimedBarsListIdx.Clear();

                    subscriptionThread = new Thread(new ParameterizedThreadStart(sendSubscribeRequestRun));
                    subscriptionThread.IsBackground = true;
                    subscriptionThread.Start(sendOnlyUnsubscribed);

                    //ThreadPool.QueueUserWorkItem(new WaitCallback(sendSubscribeRequestRun));

                    //this.Invoke(d, sendOnlyUnsubscribed);
                }
                //                 else
                //                 {
                //                     sendSubscribeRequestRun(sendOnlyUnsubscribed);
                //                 }
            }
#if DEBUG
            catch (Exception ex)
            {
                TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
            }
#endif

        }

        public void sendSubscribeRequestRun(Object obj)
        {
            ThreadTracker.openThread(null, null);

            try
            {
                //m_CEL.RemoveAllTimedBars();
                //Thread.Sleep(3000);

                if (m_CEL.IsStarted)
                {
                    bool sendOnlyUnsubscribed = (bool)obj;

                    int i = 0;

                    //m_CEL.NewInstrument("C.US.EPN1312600");

                    while (!subscriptionThreadShouldStop && i < DataCollectionLibrary.optionSpreadExpressionList.Count)
                    {

                        //TSErrorCatch.debugWriteOut("SUBSCRIBE " + optionSpreadExpressionList[i].cqgsymbol);

                        if (sendOnlyUnsubscribed)
                        {


                            if (!DataCollectionLibrary.optionSpreadExpressionList[i].setSubscriptionLevel)
                            {
                                Thread.Sleep(SUBSCRIPTION_TIMEDELAY_CONSTANT);


                                string msg =
                                    "SUBSCRIBE " + DataCollectionLibrary.optionSpreadExpressionList[i].asset.cqgsymbol
                                    + " : " + (i + 1) + " OF " +
                                    DataCollectionLibrary.optionSpreadExpressionList.Count;

                                AsyncTaskListener.StatusUpdateAsync(msg, STATUS_FORMAT.ALARM, STATUS_TYPE.DATA_SUBSCRIPTION_STATUS);


                                m_CEL.NewInstrument(DataCollectionLibrary.optionSpreadExpressionList[i].asset.cqgsymbol);

                                int idx = i;

                                DataCollectionLibrary.optionSpreadExpressionHashTable_keySymbol.AddOrUpdate(
                                    DataCollectionLibrary.optionSpreadExpressionList[i].asset.cqgsymbol,
                                    DataCollectionLibrary.optionSpreadExpressionList[i],
                                    (oldKey, oldValue) => DataCollectionLibrary.optionSpreadExpressionList[i]);

                            }

                            if (DataCollectionLibrary.optionSpreadExpressionList[i].callPutOrFuture == OPTION_SPREAD_CONTRACT_TYPE.FUTURE)
                            {
                                if (!DataCollectionLibrary.optionSpreadExpressionList[i].requestedMinuteBars)
                                //&& DataCollectionLibrary.optionSpreadExpressionList[i].normalSubscriptionRequest)
                                {
                                    requestFutureContractTimeBars(DataCollectionLibrary.optionSpreadExpressionList[i],
                                        i);
                                }
                            }

                        }
                        else
                        {
                            DataCollectionLibrary.optionSpreadExpressionList[i].setSubscriptionLevel = false;

                            Thread.Sleep(SUBSCRIPTION_TIMEDELAY_CONSTANT);

                            string msg =
                                    "SUBSCRIBE " + DataCollectionLibrary.optionSpreadExpressionList[i].asset.cqgsymbol
                                    + " : " + (i + 1) + " OF " +
                                    DataCollectionLibrary.optionSpreadExpressionList.Count;

                            AsyncTaskListener.StatusUpdateAsync(msg, STATUS_FORMAT.ALARM, STATUS_TYPE.DATA_SUBSCRIPTION_STATUS);


                            m_CEL.NewInstrument(DataCollectionLibrary.optionSpreadExpressionList[i].asset.cqgsymbol);

                            int idx = i;

                            DataCollectionLibrary.optionSpreadExpressionHashTable_keySymbol.AddOrUpdate(
                                DataCollectionLibrary.optionSpreadExpressionList[i].asset.cqgsymbol,
                                DataCollectionLibrary.optionSpreadExpressionList[i],
                                (oldKey, oldValue) => DataCollectionLibrary.optionSpreadExpressionList[i]);



                            if (DataCollectionLibrary.optionSpreadExpressionList[i].callPutOrFuture == OPTION_SPREAD_CONTRACT_TYPE.FUTURE)
                            {
                                DataCollectionLibrary.optionSpreadExpressionList[i].requestedMinuteBars = false;

                                requestFutureContractTimeBars(DataCollectionLibrary.optionSpreadExpressionList[i],
                                    i);

                            }
                        }



                        i++;
                    }

                    Thread.Sleep(SUBSCRIPTION_TIMEDELAY_CONSTANT);

                    AsyncTaskListener.StatusUpdateAsync("", STATUS_FORMAT.ALARM, STATUS_TYPE.DATA_SUBSCRIPTION_STATUS);

                }
            }
            catch (Exception ex)
            {
                TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
            }

            ThreadTracker.closeThread(null, null);
        }



        public void requestFutureContractTimeBars(MongoDB_OptionSpreadExpression optionSpreadExpression,
            int optionSpreadExpressionIdx)
        {
            try
            {
                //m_CEL.RemoveAllInstruments();

                //Thread.Sleep(5000);

                CQGTimedBarsRequest timedBarsRequest = m_CEL.CreateTimedBarsRequest();
                //timedBarsRequest.Symbol = underlyingFutureContractProps.underlyingInstrumentName;

                timedBarsRequest.Symbol = optionSpreadExpression.asset.cqgsymbol;

                //timedBarsRequest.Symbol = "F.US.USAH1";
                timedBarsRequest.SessionsFilter = 31;
                //timedBarsRequest.SessionFlags = CQG.eSessionFlag.sfDailyFromIntraday;

                //if (underlyingFutureMinOrDaily == TradingSystemConstants.DATA_COLLECTION_MINUTE_BARS)
                //{
                timedBarsRequest.IntradayPeriod = 1;
                //}
                //else
                //{
                //    timedBarsRequest.HistoricalPeriod = CQG.eHistoricalPeriod.hpDaily;
                //}

                //if (runningForContinuousContract)
                //{
                //    timedBarsRequest.Continuation = eTimeSeriesContinuationType.tsctActive;
                //    timedBarsRequest.EqualizeCloses = false;
                //}
                //else
                //{
                timedBarsRequest.Continuation = CQG.eTimeSeriesContinuationType.tsctNoContinuation;
                //}

                //timedBarsRequest.Continuation = CQG.eTimeSeriesContinuationType.tsctNoContinuation;
                //timedBarsRequest.EqualizeCloses = false;
                //DateTime rangeStart = m_CEL.Environment.LineTime.AddDays(-1);

                DateTime rangeStart = m_CEL.Environment.LineTime.Date;
                //optionSpreadExpression.previousDateTimeBoundaryStart;

                DateTime rangeEnd = m_CEL.Environment.LineTime;

                timedBarsRequest.RangeStart = rangeStart;
                timedBarsRequest.RangeEnd = rangeEnd;

                timedBarsRequest.IncludeEnd = true;

                timedBarsRequest.UpdatesEnabled = true;

                //timedBarsRequest.

                optionSpreadExpression.futureTimedBars = m_CEL.RequestTimedBars(timedBarsRequest);


                DataCollectionLibrary.optionSpreadExpressionHashTable_keyCQGInId.AddOrUpdate(
                    optionSpreadExpression.futureTimedBars.Id,
                    optionSpreadExpression, (oldKey, oldValue) => optionSpreadExpression);
            }
            catch (Exception ex)
            {
                TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
            }
        }

        public void manageExpressionPriceCalcs(MongoDB_OptionSpreadExpression optionSpreadExpression)
        {
            //             if (!optionSpreadExpression.riskFreeRateFilled)
            //             {
            //                 optionSpreadExpression.riskFreeRate = 0.01;
            //             }

            //if (!optionSpreadManager.realtimeMonitorSettings.eodAnalysis)
            //optionSpreadExpression.lastTimeUpdated = DateTime.Now;

            //             fillPricesFromQuote(optionSpreadExpression, quotes);
            // 
            //             fillDefaultPrice(optionSpreadExpression);

            if (optionSpreadExpression.optionExpressionType == OPTION_EXPRESSION_TYPES.OPTION_EXPRESSION_RISK_FREE_RATE)
            {
                // fill all instruments with the latest interest rate

                for (int expressionCounter = 0;
                        expressionCounter < DataCollectionLibrary.optionSpreadExpressionList.Count; expressionCounter++)
                {
                    DataCollectionLibrary.optionSpreadExpressionList[expressionCounter].riskFreeRateFilled = true;

                    DataCollectionLibrary.optionSpreadExpressionList[expressionCounter].riskFreeRate = optionSpreadExpression.riskFreeRate;

                    //only update if subscribed
                    if (DataCollectionLibrary.optionSpreadExpressionList[expressionCounter].setSubscriptionLevel)
                    {
                        DataCollectionLibrary.optionSpreadExpressionList[expressionCounter].totalCalcsRefresh = CQG_REFRESH_STATE.DATA_UPDATED;
                    }
                }
            }
            else
            {
                if (optionSpreadExpression.callPutOrFuture == OPTION_SPREAD_CONTRACT_TYPE.FUTURE)
                {

                    optionSpreadExpression.impliedVol = 0;
                    optionSpreadExpression.delta = 1 * TradingSystemConstants.OPTION_DELTA_MULTIPLIER;

                    ///Below fills decision bar for future and options of future
                    if (optionSpreadExpression.decisionBar != null)
                    {
                        optionSpreadExpression.decisionPrice = optionSpreadExpression.decisionBar.Close;

                        optionSpreadExpression.decisionPriceTime = optionSpreadExpression.decisionBar.Timestamp;

                        if (optionSpreadExpression.reachedDecisionBar)
                        {
                            optionSpreadExpression.decisionPriceFilled = true;
                        }
                    }


                    if (optionSpreadExpression.todayTransactionBar != null)
                    {
                        optionSpreadExpression.transactionPrice =
                            optionSpreadExpression.todayTransactionBar.Close;

                        optionSpreadExpression.transactionPriceTime =
                            optionSpreadExpression.todayTransactionBar.Timestamp;

                        if (optionSpreadExpression.reachedTransactionTimeBoundary)
                        {
                            optionSpreadExpression.transactionPriceFilled = true;
                        }

                    }

                    TSErrorCatch.debugWriteOut(optionSpreadExpression.asset.cqgsymbol + " " + optionSpreadExpression.optionExpressionsThatUseThisFutureAsUnderlying.Count());

                    foreach (MongoDB_OptionSpreadExpression optionSpreadThatUsesFuture in optionSpreadExpression.optionExpressionsThatUseThisFutureAsUnderlying)
                    {
                        generatingGreeksAndImpliedVol(optionSpreadThatUsesFuture);

                        fillEodAnalysisPrices(optionSpreadThatUsesFuture);

                        fillTheoreticalOptionPrice(optionSpreadThatUsesFuture);



                        if (optionSpreadThatUsesFuture.setSubscriptionLevel)
                        {
                            optionSpreadThatUsesFuture.totalCalcsRefresh = CQG_REFRESH_STATE.DATA_UPDATED;
                        }

                        //tempListOfExpressionsFilled.Add(optionSpreadExpression.optionExpressionIdxUsedInFuture[expressionCounter]);
                    }


                }
                else
                {
                    generatingGreeksAndImpliedVol(optionSpreadExpression);

                    fillEodAnalysisPrices(optionSpreadExpression);

                    //fillTheoreticalOptionPrice(optionSpreadExpression);
                }



                optionSpreadExpression.totalCalcsRefresh = CQG_REFRESH_STATE.DATA_UPDATED;
            }
        }

        private void fillPricesFromQuote(MongoDB_OptionSpreadExpression optionSpreadExpression, CQGQuotes quotes)
        {
            //double defaultPrice = 0;

            //CQGQuote quoteAsk = optionSpreadExpression.cqgInstrument.Quotes[eQuoteType.qtAsk];
            //CQGQuote quoteBid = optionSpreadExpression.cqgInstrument.Quotes[eQuoteType.qtBid];
            //CQGQuote quoteTrade = optionSpreadExpression.cqgInstrument.Quotes[eQuoteType.qtTrade];
            //CQGQuote quoteSettlement = optionSpreadExpression.cqgInstrument.Quotes[eQuoteType.qtSettlement];
            //CQGQuote quoteYestSettlement = optionSpreadExpression.cqgInstrument.Quotes[eQuoteType.qtYesterdaySettlement];

            CQGQuote quoteAsk = quotes[eQuoteType.qtAsk];
            CQGQuote quoteBid = quotes[eQuoteType.qtBid];
            CQGQuote quoteTrade = quotes[eQuoteType.qtTrade];
            CQGQuote quoteSettlement = quotes[eQuoteType.qtSettlement];
            CQGQuote quoteYestSettlement = quotes[eQuoteType.qtYesterdaySettlement];

            if (optionSpreadExpression.callPutOrFuture != OPTION_SPREAD_CONTRACT_TYPE.FUTURE)
            {
                if (quoteAsk != null)
                {
                    if (quoteAsk.IsValid)
                    {
                        optionSpreadExpression.ask = quoteAsk.Price;

                        optionSpreadExpression.askFilled = true;
                    }
                    else
                    {
                        optionSpreadExpression.ask = 0;

                        optionSpreadExpression.askFilled = false;
                    }
                }

                if (quoteBid != null)
                {
                    if (quoteBid.IsValid)
                    {
                        optionSpreadExpression.bid = quoteBid.Price;

                        optionSpreadExpression.bidFilled = true;
                    }
                    else
                    {
                        optionSpreadExpression.bid = 0;

                        optionSpreadExpression.bidFilled = false;
                    }
                }

                if (quoteTrade != null)
                {
                    if (quoteTrade.IsValid)
                    {
                        optionSpreadExpression.trade = quoteTrade.Price;

                        optionSpreadExpression.tradeFilled = true;
                    }
                    else if (optionSpreadExpression.callPutOrFuture != OPTION_SPREAD_CONTRACT_TYPE.FUTURE)
                    {
                        optionSpreadExpression.trade = 0;

                        optionSpreadExpression.tradeFilled = false;
                    }
                }
            }

            if (quoteSettlement != null)
            {
                if (quoteSettlement.IsValid)
                {
                    if (DataCollectionLibrary.realtimeMonitorSettings.eodAnalysis
                        || (optionSpreadExpression.instrument != null
                            && optionSpreadExpression.instrument.eodAnalysisAtInstrument))
                    {
                        if (optionSpreadExpression.substituteSubscriptionRequest
                            || !optionSpreadExpression.useSubstituteSymbolAtEOD)
                        {
                            if (!optionSpreadExpression.manuallyFilled)
                            {
                                optionSpreadExpression.settlement = quoteSettlement.Price;

                                optionSpreadExpression.settlementDateTime = quoteSettlement.ServerTimestamp;
                            }

                        }
                    }
                    else
                    {
                        if (!optionSpreadExpression.manuallyFilled)
                        {
                            optionSpreadExpression.settlement = quoteSettlement.Price;

                            optionSpreadExpression.settlementDateTime = quoteSettlement.ServerTimestamp;
                        }
                    }

                    if (optionSpreadExpression.settlementDateTime.Date.CompareTo(DateTime.Now.Date) == 0)
                    {
                        optionSpreadExpression.settlementIsCurrentDay = true;
                    }


                    optionSpreadExpression.settlementFilled = true;

                    fillEODSubstitutePrices(optionSpreadExpression);

                }
                else
                {
                    if (!optionSpreadExpression.manuallyFilled)
                    {
                        optionSpreadExpression.settlement = 0;

                        optionSpreadExpression.settlementFilled = false;
                    }
                }


            }

        }

        public void fillEODSubstitutePrices(MongoDB_OptionSpreadExpression optionSpreadExpression)
        {
            //if (optionSpreadManager.realtimeMonitorSettings.eodAnalysis
            if (optionSpreadExpression.instrument != null
                && optionSpreadExpression.instrument.eodAnalysisAtInstrument
                && optionSpreadExpression.substituteSubscriptionRequest)
            {
                //foreach (MongoDB_OptionSpreadExpression optionSpreadExpressionSingle in
                //    optionSpreadExpression.mainExpressionSubstitutionUsedFor)
                {
                    //if (!optionSpreadExpression.manuallyFilled)

                    if (!optionSpreadExpression.mainExpressionSubstitutionUsedFor.manuallyFilled)
                    {
                        optionSpreadExpression.mainExpressionSubstitutionUsedFor.settlement = optionSpreadExpression.settlement;

                        optionSpreadExpression.mainExpressionSubstitutionUsedFor.settlementDateTime = optionSpreadExpression.settlementDateTime;

                        optionSpreadExpression.mainExpressionSubstitutionUsedFor.settlementIsCurrentDay = optionSpreadExpression.settlementIsCurrentDay;

                        optionSpreadExpression.mainExpressionSubstitutionUsedFor.settlementFilled = true;
                    }
                }
            }
        }

        private void fillDefaultMidPrice_Future(MongoDB_OptionSpreadExpression optionSpreadExpression)
        {
            double defaultPrice = 0;

            optionSpreadExpression.lastTimeUpdated = optionSpreadExpression.lastTimeFuturePriceUpdated;

            TimeSpan span = DateTime.Now - optionSpreadExpression.lastTimeUpdated;

            optionSpreadExpression.minutesSinceLastUpdate = span.TotalMinutes;

            if (optionSpreadExpression.tradeFilled)
            {
                defaultPrice = optionSpreadExpression.trade;
            }
            else if (optionSpreadExpression.settlementFilled)
            {
                defaultPrice = optionSpreadExpression.settlement;
            }
            else if (optionSpreadExpression.yesterdaySettlementFilled)
            {
                defaultPrice = optionSpreadExpression.yesterdaySettlement;
            }

            if (defaultPrice == 0)
            {
                defaultPrice = TradingSystemConstants.OPTION_ZERO_PRICE;
            }

            //can set default price for futures here b/c no further price possibilities for future;
            optionSpreadExpression.defaultPrice = defaultPrice;

            optionSpreadExpression.defaultPriceFilled = true;


            manageExpressionPriceCalcs(optionSpreadExpression);


        }

        private void fillDefaultMidPrice_Option(MongoDB_OptionSpreadExpression optionSpreadExpression)
        {
            double defaultPrice = 0;



            optionSpreadExpression.lastTimeUpdated = DateTime.Now;

            optionSpreadExpression.minutesSinceLastUpdate = 0;

            if (optionSpreadExpression.bidFilled
            && optionSpreadExpression.askFilled)
            {


                defaultPrice = (optionSpreadExpression.bid + optionSpreadExpression.ask) / 2;


                //rounding to nearest tick;
                if (optionSpreadExpression.instrument != null)
                {

                    double optionticksize = OptionSpreadManager.chooseoptionticksize(defaultPrice,
                        optionSpreadExpression.instrument.optionticksize,
                        optionSpreadExpression.instrument.secondaryoptionticksize,
                        optionSpreadExpression.instrument.secondaryoptionticksizerule);

                    defaultPrice = ((int)((defaultPrice + optionticksize / 2) /
                        optionticksize)) * optionticksize;

                }

            }
            else if (optionSpreadExpression.askFilled)
            {
                defaultPrice = optionSpreadExpression.ask;
            }
            else if (optionSpreadExpression.bidFilled)
            {
                defaultPrice = optionSpreadExpression.bid;
            }

            if (defaultPrice == 0)
            {
                defaultPrice = optionSpreadExpression.instrument.optionticksize;  //OptionConstants.OPTION_ZERO_PRICE;
            }

            optionSpreadExpression.defaultMidPriceBeforeTheor = defaultPrice;

            if (optionSpreadExpression.askFilled)
            {
                optionSpreadExpression.defaultAskPriceBeforeTheor = optionSpreadExpression.ask;
            }
            else
            {
                optionSpreadExpression.defaultAskPriceBeforeTheor = defaultPrice;
            }

            if (optionSpreadExpression.bidFilled)
            {
                optionSpreadExpression.defaultBidPriceBeforeTheor = optionSpreadExpression.bid;
            }
            else
            {
                optionSpreadExpression.defaultBidPriceBeforeTheor = defaultPrice;
            }

            manageExpressionPriceCalcs(optionSpreadExpression);
        }



        private void fillDefaultPrice_RiskFreeRate(MongoDB_OptionSpreadExpression optionSpreadExpression)
        {
            double defaultPrice = 0;

            //if (optionSpreadExpression.optionExpressionType == OPTION_EXPRESSION_TYPES.OPTION_EXPRESSION_RISK_FREE_RATE)

            if (optionSpreadExpression.tradeFilled)
            {
                defaultPrice = optionSpreadExpression.trade;
            }
            else if (optionSpreadExpression.settlementFilled)
            {
                defaultPrice = optionSpreadExpression.settlement;
            }
            else if (optionSpreadExpression.yesterdaySettlementFilled)
            {
                defaultPrice = optionSpreadExpression.yesterdaySettlement;
            }

            if (defaultPrice == 0)
            {
                defaultPrice = 0.01;
            }

            defaultPrice = defaultPrice == 0 ? 0 :
                ((int)((100 - defaultPrice + TradingSystemConstants.EPSILON) * 1000) / 100000.0);

            optionSpreadExpression.riskFreeRate = defaultPrice;



            manageExpressionPriceCalcs(optionSpreadExpression);

        }



        public void generatingGreeksAndImpliedVol(MongoDB_OptionSpreadExpression optionSpreadExpression)
        {




            if (optionSpreadExpression.callPutOrFuture == OPTION_SPREAD_CONTRACT_TYPE.FUTURE)
            {
                optionSpreadExpression.impliedVol = 0;
                optionSpreadExpression.delta = 1 * TradingSystemConstants.OPTION_DELTA_MULTIPLIER;
            }

            else
            {

                MongoDB_OptionSpreadExpression futureExpression = optionSpreadExpression.underlyingFutureExpression;

                if (optionSpreadExpression.defaultPriceFilled
                    && futureExpression.defaultPriceFilled)
                {

                    double optionticksize = optionSpreadExpression.instrument.optionticksize;

                    if (optionSpreadExpression.instrument.secondaryoptionticksize > 0)
                    {
                        optionticksize = optionSpreadExpression.instrument.secondaryoptionticksize;
                    }

                    optionSpreadExpression.impliedVol =
                        OptionCalcs.calculateOptionVolatilityNR(
                           optionSpreadExpression.asset.callorput,
                           futureExpression.defaultPrice,
                           optionSpreadExpression.asset.strikeprice,
                           optionSpreadExpression.asset.yearFraction, optionSpreadExpression.riskFreeRate,
                           optionSpreadExpression.defaultPrice, optionticksize); // *100;


                    if (optionSpreadExpression.impliedVol > 1000
                        || optionSpreadExpression.impliedVol < -1000
                        )
                    {
                        optionSpreadExpression.defaultPrice = optionSpreadExpression.theoreticalOptionPrice;

                        optionSpreadExpression.impliedVol = optionSpreadExpression.impliedVolFromSpan;


                        optionSpreadExpression.defaultPriceFilled = true;

                        optionSpreadExpression.totalCalcsRefresh = CQG_REFRESH_STATE.DATA_UPDATED;
                    }

                    optionSpreadExpression.impliedVolFilled = true;

                    optionSpreadExpression.delta =
                        OptionCalcs.gDelta(
                           optionSpreadExpression.asset.callorput,
                           futureExpression.defaultPrice,
                           optionSpreadExpression.asset.strikeprice,
                           optionSpreadExpression.asset.yearFraction, optionSpreadExpression.riskFreeRate,
                           0,
                           optionSpreadExpression.impliedVol) * TradingSystemConstants.OPTION_DELTA_MULTIPLIER;

                    //optionSpreadExpression.gamma =
                    //    OptionCalcs.gGamma(
                    //    //optionSpreadExpression.callPutOrFutureChar,
                    //       futureExpression.defaultPrice,
                    //       optionSpreadExpression.strikePrice,
                    //       optionSpreadExpression.yearFraction, optionSpreadExpression.riskFreeRate,
                    //       0,
                    //       optionSpreadExpression.impliedVol);

                    //optionSpreadExpression.vega =
                    //    OptionCalcs.gVega(
                    //    //optionSpreadExpression.callPutOrFutureChar,
                    //       futureExpression.defaultPrice,
                    //       optionSpreadExpression.strikePrice,
                    //       optionSpreadExpression.yearFraction, optionSpreadExpression.riskFreeRate,
                    //       0,
                    //       optionSpreadExpression.impliedVol);

                    //optionSpreadExpression.theta =
                    //    OptionCalcs.gTheta(
                    //        optionSpreadExpression.callPutOrFutureChar,
                    //       futureExpression.defaultPrice,
                    //       optionSpreadExpression.strikePrice,
                    //       optionSpreadExpression.yearFraction, optionSpreadExpression.riskFreeRate,
                    //       0,
                    //       optionSpreadExpression.impliedVol);
                }

            }

        }

        public void generatingSettlementGreeks(MongoDB_OptionSpreadExpression optionSpreadExpression)
        //MongoDB_OptionSpreadExpression futureExpression)
        {



            //if (futureExpression.settlementFilled
            if (optionSpreadExpression.settlementFilled)
            {

                MongoDB_OptionSpreadExpression futureExpression = optionSpreadExpression.underlyingFutureExpression;

                if (optionSpreadExpression.settlementFilled)
                {

                    double optionticksize = optionSpreadExpression.instrument.optionticksize;

                    if (optionSpreadExpression.instrument.secondaryoptionticksize > 0)
                    {
                        optionticksize = optionSpreadExpression.instrument.secondaryoptionticksize;
                    }

                    optionSpreadExpression.settlementImpliedVol =
                        OptionCalcs.calculateOptionVolatilityNR(
                           optionSpreadExpression.asset.callorput,
                           futureExpression.settlement,
                           optionSpreadExpression.asset.strikeprice,
                           optionSpreadExpression.asset.yearFraction, optionSpreadExpression.riskFreeRate,
                           optionSpreadExpression.settlement, optionticksize);// *100;
                }

                //if (optionBuildCommonMethods.usingEODSettlements)
                //{
                //optionSpreadThatUsesFuture.impliedVol = optionSpreadExpression.settlementImpliedVol;
                //}


            }

        }



        public void fillEodAnalysisPrices(MongoDB_OptionSpreadExpression optionSpreadExpression)
        {
            //if (optionSpreadManager.realtimeMonitorSettings.eodAnalysis)

            generatingSettlementGreeks(optionSpreadExpression);

            if (optionSpreadExpression.callPutOrFuture != OPTION_SPREAD_CONTRACT_TYPE.FUTURE)
            {

                MongoDB_OptionSpreadExpression futureExpression = optionSpreadExpression.underlyingFutureExpression;

                if (futureExpression.decisionPriceFilled)
                {

                    optionSpreadExpression.decisionPrice =
                        OptionCalcs.blackScholes(
                            optionSpreadExpression.asset.callorput,
                               futureExpression.decisionPrice,
                               optionSpreadExpression.asset.strikeprice,
                               optionSpreadExpression.asset.yearFraction, optionSpreadExpression.riskFreeRate,
                               optionSpreadExpression.impliedVolFromSpan);

                    optionSpreadExpression.decisionPriceFilled = true;

                    double optionticksize = OptionSpreadManager.chooseoptionticksize(
                                optionSpreadExpression.decisionPrice,
                                optionSpreadExpression.instrument.optionticksize,
                                optionSpreadExpression.instrument.secondaryoptionticksize,
                                optionSpreadExpression.instrument.secondaryoptionticksizerule);

                    optionSpreadExpression.decisionPrice =
                        ((int)((optionSpreadExpression.decisionPrice + optionticksize / 2) /
                            optionticksize)) * optionticksize;

                    //StringBuilder testingOut = new StringBuilder();
                    //testingOut.Append("DECISION,");
                    //testingOut.Append(optionSpreadExpression.instrument.name);
                    //testingOut.Append(",CALL OR PUT, ");
                    //testingOut.Append(optionSpreadExpression.callPutOrFutureChar);
                    //testingOut.Append(",FUT PRICE, ");
                    //testingOut.Append(futureExpression.decisionPrice);
                    //testingOut.Append(",STRIKE PRICE, ");
                    //testingOut.Append(optionSpreadExpression.strikePrice);
                    //testingOut.Append(",YEAR FRACTION, ");
                    //testingOut.Append(optionSpreadExpression.yearFraction);
                    //testingOut.Append(",RFR, ");
                    //testingOut.Append(optionSpreadExpression.riskFreeRate);
                    //testingOut.Append(",SETTLE, ");
                    //testingOut.Append(optionSpreadExpression.settlementImpliedVol);
                    //testingOut.Append(",OPTION PRICE, ");
                    //testingOut.Append(optionSpreadExpression.decisionPrice);

                    //TSErrorCatch.debugWriteOut(testingOut.ToString());

                    optionSpreadExpression.decisionPriceTime = futureExpression.decisionPriceTime;

                    optionSpreadExpression.decisionPriceFilled = true;

                    //optionSpreadExpression.reachedDecisionBar = futureExpression.reachedDecisionBar;

                    //this is used to fill the order with the mark of when decision bar filled
                    //optionSpreadExpression.reachedBarAfterDecisionBar = futureExpression.reachedBarAfterDecisionBar;
                }

                //optionSpreadExpression.reached1MinAfterDecisionBarUsedForSnapshot = futureExpression.reached1MinAfterDecisionBarUsedForSnapshot;

                //optionSpreadExpression.instrument.eodAnalysisAtInstrument = futureExpression.instrument.eodAnalysisAtInstrument;

                if (futureExpression.transactionPriceFilled)
                {
                    if (futureExpression.instrument.eodAnalysisAtInstrument)
                    {
                        optionSpreadExpression.transactionPrice =
                            OptionCalcs.blackScholes(
                                optionSpreadExpression.asset.callorput,
                                   futureExpression.transactionPrice,
                                   optionSpreadExpression.asset.strikeprice,
                                   optionSpreadExpression.asset.yearFraction, optionSpreadExpression.riskFreeRate,
                                   optionSpreadExpression.settlementImpliedVol);

                        optionSpreadExpression.transactionPriceFilled = true;
                    }
                    else
                    {

                        if (!optionSpreadExpression.filledAfterTransactionTimeBoundary)
                        {
                            if (optionSpreadExpression.impliedVolFilled)
                            {
                                optionSpreadExpression.transactionPrice =
                                    OptionCalcs.blackScholes(
                                        optionSpreadExpression.asset.callorput,
                                           futureExpression.transactionPrice,
                                           optionSpreadExpression.asset.strikeprice,
                                           optionSpreadExpression.asset.yearFraction, optionSpreadExpression.riskFreeRate,
                                           optionSpreadExpression.impliedVol);

                                optionSpreadExpression.transactionPriceFilled = true;

                                optionSpreadExpression.filledAfterTransactionTimeBoundary = true;

                            }
                        }
                    }

                    double optionticksize = OptionSpreadManager.chooseoptionticksize(
                                optionSpreadExpression.transactionPrice,
                                optionSpreadExpression.instrument.optionticksize,
                                optionSpreadExpression.instrument.secondaryoptionticksize,
                                optionSpreadExpression.instrument.secondaryoptionticksizerule);

                    optionSpreadExpression.transactionPrice =
                        ((int)((optionSpreadExpression.transactionPrice + optionticksize / 2) /
                            optionticksize)) * optionticksize;

                    //testingOut.Clear();
                    //testingOut.Append("TRANS,");
                    //testingOut.Append(optionSpreadExpression.instrument.name);
                    //testingOut.Append(",CALL OR PUT, ");
                    //testingOut.Append(optionSpreadExpression.callPutOrFutureChar);
                    //testingOut.Append(",FUT PRICE, ");
                    //testingOut.Append(futureExpression.transactionPrice);
                    //testingOut.Append(",STRIKE PRICE, ");
                    //testingOut.Append(optionSpreadExpression.strikePrice);
                    //testingOut.Append(",YEAR FRACTION, ");
                    //testingOut.Append(optionSpreadExpression.yearFraction);
                    //testingOut.Append(",RFR, ");
                    //testingOut.Append(optionSpreadExpression.riskFreeRate);
                    //testingOut.Append(",SETTLE, ");
                    //testingOut.Append(optionSpreadExpression.settlementImpliedVol);
                    //testingOut.Append(",OPTION PRICE, ");
                    //testingOut.Append(optionSpreadExpression.transactionPrice);

                    //TSErrorCatch.debugWriteOut(testingOut.ToString());

                    optionSpreadExpression.transactionPriceTime = futureExpression.transactionPriceTime;

                    //optionSpreadExpression.decisionPriceFilled = true;

                    //optionSpreadExpression.reachedTransactionTimeBoundary = futureExpression.reachedTransactionTimeBoundary;
                }

                //optionSpreadExpression.defaultPrice = optionSpreadExpression.theoreticalOptionPrice;

                //CHANGED DEC 30 2015 
                //if (optionSpreadExpression.instrument.eodAnalysisAtInstrument)
                //{
                //    optionSpreadExpression.defaultPrice =
                //        optionSpreadExpression.transactionPrice;

                //    optionSpreadExpression.minutesSinceLastUpdate = 0;

                //    optionSpreadExpression.lastTimeUpdated =
                //        optionSpreadExpression.transactionPriceTime;

                //    optionSpreadExpression.defaultPriceFilled = true;
                //}
            }
        }

        public void fillTheoreticalOptionPrice(MongoDB_OptionSpreadExpression optionSpreadExpression)
        {

            if (optionSpreadExpression.callPutOrFuture != OPTION_SPREAD_CONTRACT_TYPE.FUTURE)
            {
                MongoDB_OptionSpreadExpression futureExpression = optionSpreadExpression.underlyingFutureExpression;



                if (futureExpression.defaultPriceFilled)
                {


                    optionSpreadExpression.theoreticalOptionPrice =
                        OptionCalcs.blackScholes(
                        optionSpreadExpression.asset.callorput,
                           futureExpression.defaultPrice,
                           optionSpreadExpression.asset.strikeprice,
                           optionSpreadExpression.asset.yearFraction, optionSpreadExpression.riskFreeRate,
                           optionSpreadExpression.impliedVolFromSpan);  // / 100);

                    double optionticksize = OptionSpreadManager.chooseoptionticksize(
                        optionSpreadExpression.theoreticalOptionPrice,
                        optionSpreadExpression.instrument.optionticksize,
                        optionSpreadExpression.instrument.secondaryoptionticksize,
                        optionSpreadExpression.instrument.secondaryoptionticksizerule);

                    optionSpreadExpression.theoreticalOptionPrice =
                        ((int)((optionSpreadExpression.theoreticalOptionPrice + optionticksize / 2) /
                            optionticksize)) * optionticksize;

                    if (optionSpreadExpression.theoreticalOptionPrice == 0)
                    {
                        optionSpreadExpression.theoreticalOptionPrice = optionSpreadExpression.instrument.optionticksize; // OptionConstants.OPTION_ZERO_PRICE;
                    }

                    if (DataCollectionLibrary.realtimeMonitorSettings.realtimePriceFillType
                            == REALTIME_PRICE_FILL_TYPE.PRICE_MID_BID_ASK)
                    {
                        optionSpreadExpression.defaultPrice = optionSpreadExpression.defaultMidPriceBeforeTheor;
                    }
                    else if (DataCollectionLibrary.realtimeMonitorSettings.realtimePriceFillType
                            == REALTIME_PRICE_FILL_TYPE.PRICE_ASK)
                    {
                        optionSpreadExpression.defaultPrice = optionSpreadExpression.defaultAskPriceBeforeTheor;
                    }
                    else if (DataCollectionLibrary.realtimeMonitorSettings.realtimePriceFillType
                            == REALTIME_PRICE_FILL_TYPE.PRICE_BID)
                    {
                        optionSpreadExpression.defaultPrice = optionSpreadExpression.defaultBidPriceBeforeTheor;
                    }
                    else if (DataCollectionLibrary.realtimeMonitorSettings.realtimePriceFillType
                            == REALTIME_PRICE_FILL_TYPE.PRICE_DEFAULT)
                    {
                        bool midPriceAcceptable = false;

                        if (optionSpreadExpression.bidFilled && optionSpreadExpression.askFilled
                            &&
                            Math.Abs((optionSpreadExpression.bid - optionSpreadExpression.ask)
                        / OptionSpreadManager.chooseoptionticksize(optionSpreadExpression.ask,
                            optionSpreadExpression.instrument.optionticksize,
                            optionSpreadExpression.instrument.secondaryoptionticksize,
                            optionSpreadExpression.instrument.secondaryoptionticksizerule))
                        < TradingSystemConstants.OPTION_ACCEPTABLE_BID_ASK_SPREAD)
                        {
                            midPriceAcceptable = true;
                        }

                        if (midPriceAcceptable
                            ||
                            Math.Abs((optionSpreadExpression.defaultMidPriceBeforeTheor
                            - optionSpreadExpression.theoreticalOptionPrice)
                            / optionSpreadExpression.theoreticalOptionPrice) <= TradingSystemConstants.OPTION_DEFAULT_THEORETICAL_PRICE_RANGE)
                        {
                            //                         TSErrorCatch.debugWriteOut(optionSpreadExpression.cqgsymbol + "  NOT theoretical Price "
                            //                             + optionSpreadExpression.optionDefaultPriceWithoutTheoretical);


                            bool filledDefaultPrice = false;

                            if (optionSpreadExpression.callPutOrFuture == OPTION_SPREAD_CONTRACT_TYPE.CALL)
                            {
                                if (futureExpression.defaultPrice >
                                    optionSpreadExpression.asset.strikeprice + (TradingSystemConstants.STRIKE_COUNT_FOR_DEFAULT_TO_THEORETICAL
                                        * optionSpreadExpression.instrument.optionstrikeincrement))
                                {
                                    optionSpreadExpression.defaultPrice = optionSpreadExpression.theoreticalOptionPrice;
                                    filledDefaultPrice = true;
                                }
                            }
                            else if (optionSpreadExpression.callPutOrFuture == OPTION_SPREAD_CONTRACT_TYPE.PUT)
                            {
                                if (futureExpression.defaultPrice <
                                    optionSpreadExpression.asset.strikeprice - (TradingSystemConstants.STRIKE_COUNT_FOR_DEFAULT_TO_THEORETICAL
                                        * optionSpreadExpression.instrument.optionstrikeincrement))
                                {
                                    optionSpreadExpression.defaultPrice = optionSpreadExpression.theoreticalOptionPrice;
                                    filledDefaultPrice = true;
                                }
                            }

                            if (!filledDefaultPrice)
                            {
                                optionSpreadExpression.defaultPrice = optionSpreadExpression.defaultMidPriceBeforeTheor;
                            }
                        }
                        else
                        {
                            /*TSErrorCatch.debugWriteOut(optionSpreadExpression.cqgsymbol + "  IS theoretical Price");*/

                            optionSpreadExpression.defaultPrice = optionSpreadExpression.theoreticalOptionPrice;
                        }
                    }
                    else
                    {
                        optionSpreadExpression.defaultPrice = optionSpreadExpression.theoreticalOptionPrice;
                    }



                    optionSpreadExpression.defaultPriceFilled = true;

                }
            }
        }

        //        public void setupCalculateModelValuesAndSummarizeTotals()
        //        {

        //#if DEBUG
        //            try
        //#endif
        //            {
        //                calculateModelValuesAndSummarizeTotalsThread = new Thread(new ParameterizedThreadStart(calculateModelValuesAndSummarizeTotalsThreadRun));
        //                calculateModelValuesAndSummarizeTotalsThread.IsBackground = true;
        //                calculateModelValuesAndSummarizeTotalsThread.Start();
        //            }
        //#if DEBUG
        //            catch (Exception ex)
        //            {
        //                TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
        //            }
        //#endif

        //        }

        //private void calculateModelValuesAndSummarizeTotalsThreadRun(Object obj)
        //{
        //    ThreadTracker.openThread(null, null);

        //    try
        //    {
        //        while (!calculateModelValuesThreadShouldStop)
        //        {
        //            calculateModelValuesAndSummarizeTotals();

        //            Thread.Sleep(TradingSystemConstants.MODEL_CALC_TIME_REFRESH);

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
        //    }

        //    ThreadTracker.closeThread(null, null);
        //}

        //private void calculateModelValuesAndSummarizeTotals()
        //{

        //    optionSpreadManager.RunSpreadTotalCalculations();

        //    optionSpreadManager.RunADMSpreadTotalCalculations();

        //}

    }
}
