using AutoMapper;
using RealtimeSpreadMonitor.FormManipulation;
using RealtimeSpreadMonitor.Forms;
using RealtimeSpreadMonitor.Model;
using RealtimeSpreadMonitor.Mongo;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace RealtimeSpreadMonitor
{
    /*
     *This is the main class that get data ready for display
     */

    public delegate string QueryFutureIdContractDelegate(object queryObject, int idinstrument,
        int contractYear, int contractMonthInt);

    public class OptionSpreadManager
    {
        //private System.Threading.Timer timer3SetupTimeRealtimeStateWriteToDatabase;
        //private TimeSpan timeToWriteStateToDatabase = new TimeSpan(15, 00, 00);

        //private TimeSpan _dataResetTime1 = new TimeSpan(7, 10, 0);
        //public TimeSpan dataResetTime1
        //{
        //    get { return _dataResetTime1; }
        //}

        //private TimeSpan _dataResetTime2 = new TimeSpan(8, 30, 0);
        //public TimeSpan dataResetTime2
        //{
        //    get { return _dataResetTime2; }
        //}

        //internal int threadCount;
        //internal void closeThread(object o, EventArgs e)
        //{
        //    // Decrease number of threads
        //    threadCount--;
        //}

        //internal void openThread(object o, EventArgs e)
        //{
        //    // Decrease number of threads
        //    threadCount++;
        //}

        private OptionArrayTypes optionArrayTypes = new OptionArrayTypes();


        private OptionRealtimeStartup optionRealtimeStartup;
        private OptionStartupProgress optionStartupProgress;



        internal OptionCQGDataManagement optionCQGDataManagement;


        //private Instrument_DefaultFutures[] instrument_DefaultFuturesArray;

        internal Dictionary<int, Instrument_mongo> substituteInstrumentHash = new Dictionary<int, Instrument_mongo>();


        private OptionRealtimeMonitor sOptionRealtimeMonitor;  //the main realtime monitor
        public OptionRealtimeMonitor optionRealtimeMonitor
        {
            get { return sOptionRealtimeMonitor; }
        }

        private List<int> sContractSummaryExpressionListIdx = new List<int>();
        internal List<int> contractSummaryExpressionListIdx
        {
            get { return sContractSummaryExpressionListIdx; }
        }


        //private List<ADMPositionImportWeb> sAdmPositionImportWebForImportDisplay;
        //public List<ADMPositionImportWeb> FCM_positionImportNotConsolidated
        //{
        //    get { return sAdmPositionImportWebForImportDisplay; }
        //}

        //private List<ADMPositionImportWeb> sAdmPositionImportWeb;
        //public List<ADMPositionImportWeb> admPositionImportWeb
        //{
        //    get { return sAdmPositionImportWeb; }
        //}

        private DateTime dateOfADMPositionsFile;

        private ADMDataCommonMethods sADMDataCommonMethods = new ADMDataCommonMethods();
        public ADMDataCommonMethods aDMDataCommonMethods
        {
            get { return sADMDataCommonMethods; }
        }

        private AdmReportSummaryForm sAdmReportWebPositionsForm;
        public AdmReportSummaryForm admReportWebPositionsForm
        {
            get { return sAdmReportWebPositionsForm; }
        }

        //private bool usingEODSettlements;

        //private RealtimeMonitorSettings sRealtimeMonitorSettings;
        //public RealtimeMonitorSettings realtimeMonitorSettings
        //{
        //    get { return sRealtimeMonitorSettings; }
        //}

        //private List<ADMPositionImportWeb> sAdmPositionImportWebListForCompare = new List<ADMPositionImportWeb>();
        //public List<ADMPositionImportWeb> FCM_PostionList_forCompare
        //{
        //    get { return null; } // sAdmPositionImportWebListForCompare; }
        //}

        private HashSet<string> sZeroPriceContractList = new HashSet<string>();
        public HashSet<string> zeroPriceContractList
        {
            get { return sZeroPriceContractList; }
        }

        private HashSet<string> sExceptionContractList = new HashSet<string>();
        public HashSet<string> exceptionContractList
        {
            get { return sExceptionContractList; }
        }


        private StageOrdersToTTWPFLibrary.FixOrderStagingController _stageOrdersLibrary = null;
        public StageOrdersToTTWPFLibrary.FixOrderStagingController stageOrdersLibrary
        {
            get { return _stageOrdersLibrary; }
            set { _stageOrdersLibrary = value; }
        }

        private StageOrdersToTTWPFLibrary.FixConnectionEvent _fxce = null;
        public StageOrdersToTTWPFLibrary.FixConnectionEvent fxce
        {
            get { return _fxce; }
            set { _fxce = value; }
        }

        private bool _fxceConnected = false;
        public bool fxceConnected
        {
            get { return _fxceConnected; }
            set { _fxceConnected = value; }
        }

        //private Dictionary<string, string>[] sInstrumentAcctHashSet;
        //public Dictionary<string, string>[] instrumentAcctHashSet
        //{
        //    get { return sInstrumentAcctHashSet; }
        //}

        private StatusAndConnectedUpdates sStatusAndConnectedUpdates;
        internal StatusAndConnectedUpdates statusAndConnectedUpdates
        {
            get { return sStatusAndConnectedUpdates; }
        }

        private GridViewContractSummaryManipulation sGridViewContractSummaryManipulation;
        internal GridViewContractSummaryManipulation gridViewContractSummaryManipulation
        {
            get { return sGridViewContractSummaryManipulation; }
        }

        private ModelADMCompareCalculationAndDisplay sModelADMCompareCalculationAndDisplay;
        internal ModelADMCompareCalculationAndDisplay modelADMCompareCalculationAndDisplay
        {
            get { return sModelADMCompareCalculationAndDisplay; }
        }

        public OptionSpreadManager(
            OptionRealtimeStartup optionRealtimeStartup,
            OptionStartupProgress optionStartupProgress)
        {
            this.optionRealtimeStartup = optionRealtimeStartup;
            this.optionStartupProgress = optionStartupProgress;

            //sRealtimeMonitorSettings = new RealtimeMonitorSettings();
        }




        internal bool includeExcludeOrdersInModelADMCompare = true;


        /// <summary>
        /// The option data set hash set
        /// used when calling data from database for the options data
        /// </summary>
        private Dictionary<long, DataSet> optionDataSetHashSet = new Dictionary<long, DataSet>();

        private Dictionary<string, long> optionIdFromInfo = new Dictionary<string, long>();

        /// <summary>
        /// The future data set hash set
        /// used when calling data from database for the options data
        /// </summary>
        private Dictionary<long, DataSet> futureDataSetHashSet = new Dictionary<long, DataSet>();

        //private Dictionary<string, long> futureIdFromInfo = new Dictionary<string, long>();


        private bool _supplementContractFilled = false;
        internal bool supplementContractFilled
        {
            get { return _supplementContractFilled; }
        }

        public void initializeOptionSystem(Object obj)
        {
            optionStartupProgress.updateProgressBar(0.75);
            optionStartupProgress.updateProgressBar(1);


            //fillMongoWithPortfolioAllocation();

            fillStaticObjectsFromMongo();

            readADMExludeContractFile();

            fillADMStrategyObjectsFromSavedFiles();

            createCQGconnection();

            optionRealtimeStartup.finishedInitializing(true);

        }

        private void fillMongoWithPortfolioAllocation()
        {
            PortfolioAllocation_Mongo portfolioAllocation_Mongo = new PortfolioAllocation_Mongo();

            portfolioAllocation_Mongo.idportfoliogroup = 1;

            portfolioAllocation_Mongo.accountAllocation = new List<AccountAllocation_Mongo>();

            AccountAllocation_Mongo accountAllocation = new AccountAllocation_Mongo();
            accountAllocation.broker = "WED";
            accountAllocation.account = "new_account1";
            accountAllocation.FCM_OFFICE = "PRI";
            accountAllocation.FCM_ACCT = "00161";

            portfolioAllocation_Mongo.accountAllocation.Add(accountAllocation);

            AccountAllocation_Mongo accountAllocation2 = new AccountAllocation_Mongo();
            accountAllocation2.broker = "ADM";
            accountAllocation2.account = "new_account2";
            accountAllocation2.FCM_OFFICE = "369";
            accountAllocation2.FCM_ACCT = "17003";

            portfolioAllocation_Mongo.accountAllocation.Add(accountAllocation2);

            MongoDBConnectionAndSetup.InsertPortfolioToMongo(portfolioAllocation_Mongo);
        }

        private void fillPortfolioAllocation()
        {
            int idportfoliogroup = 1;

            PortfolioAllocation_Mongo portfolioAllocation_Mongo =
                    MongoDBConnectionAndSetup.GetAccountsPortfolio(idportfoliogroup);

            PortfolioAllocation portfolioAllocation = new PortfolioAllocation();

            //Mapper.Initialize(cfg => cfg.CreateMap<AccountAllocation_Mongo, AccountAllocation>());

            int acctIndex_UsedForTotals = 0;

            foreach (AccountAllocation_Mongo aam in portfolioAllocation_Mongo.accountAllocation)
            {
                Console.Write(aam.account);

                AccountAllocation accountAllocation = new AccountAllocation();

                accountAllocation.broker = aam.broker;

                //if (acctIndex_UsedForTotals == 0)
                //{
                //    accountAllocation.account = "new_account1";
                //}
                //else
                //{
                accountAllocation.account = aam.account;
                //}

                accountAllocation.FCM_OFFICE = aam.FCM_OFFICE;
                accountAllocation.FCM_ACCT = aam.FCM_ACCT;
                //accountAllocation.multiple = aam.multiple;

                StringBuilder acaText = new StringBuilder();
                acaText.Append(accountAllocation.FCM_OFFICE);
                acaText.Append(":");
                acaText.Append(accountAllocation.FCM_ACCT);
                acaText.Append(";");

                accountAllocation.FCM_POFFIC_PACCT = acaText.ToString();
                accountAllocation.visible = true;
                accountAllocation.acctIndex_UsedForTotals_Visibility = acctIndex_UsedForTotals++;

                portfolioAllocation.accountAllocation.Add(accountAllocation);

                var key = Tuple.Create(accountAllocation.FCM_OFFICE, accountAllocation.FCM_ACCT);

                portfolioAllocation.accountAllocation_KeyOfficAcct.Add(
                                key,
                                accountAllocation);

                portfolioAllocation.accountAllocation_KeyAccountname.Add(
                                accountAllocation.account,
                                accountAllocation);
            }


            DataCollectionLibrary.portfolioAllocation = portfolioAllocation;



            //foreach (AccountAllocation aca in DataCollectionLibrary.portfolioAllocation.accountAllocation)
            //{


            //}

        }

        /// <summary>
        /// This is the method that makes the majority of initial calls to the mongodb
        /// </summary>
        private void fillStaticObjectsFromMongo()
        {
            /// <summary>
            /// fill account and position information
            /// </summary>
            ///
            fillPortfolioAllocation();

            foreach (AccountAllocation ac in DataCollectionLibrary.portfolioAllocation.accountAllocation)
            {
                DataCollectionLibrary.accountNameList.Add(ac.account);
            }



            DataCollectionLibrary.accountList = MongoDBConnectionAndSetup.GetAccountInfoFromMongo(DataCollectionLibrary.accountNameList);

            //List<AccountPosition> archive_pos = MongoDBConnectionAndSetup.GetAccountArchivePositionsInfoFromMongo(DataCollectionLibrary.accountNameList);

            //List<AccountPosition> new_pos = MongoDBConnectionAndSetup.GetAccountPositionsInfoFromMongo(DataCollectionLibrary.accountNameList);

            //DataCollectionLibrary.accountPositionsList

            //AdjustQtyBasedOnDate();

            FillAccountPosition(true);



            /// <summary>
            /// fill instrument list
            /// </summary>
            /// 
            List<long> instrumentIdList = GetAllInstrumentIds();



            DataCollectionLibrary.instrumentList = MongoDBConnectionAndSetup.GetInstrumentListFromMongo(instrumentIdList);

            DataCollectionLibrary.instrumentHashTable_keyinstrumentid = DataCollectionLibrary.instrumentList.ToDictionary(x => x.idinstrument, x => x);

            DataCollectionLibrary.instrumentHashTable_keyadmcode = DataCollectionLibrary.instrumentList.ToDictionary(x => x.admcode, x => x);

            SetupInstrumentSummaryList();

            /// <summary>
            /// fill exchange list
            /// </summary>
            /// 
            List<long> exchangeIdList = new List<long>();

            foreach (Instrument_mongo instrument in DataCollectionLibrary.instrumentList)
            {
                if (!exchangeIdList.Contains(instrument.idexchange))
                {
                    exchangeIdList.Add(instrument.idexchange);
                }
            }

            DataCollectionLibrary.exchangeList = MongoDBConnectionAndSetup.GetExchangeListFromMongo(exchangeIdList);

            DataCollectionLibrary.exchangeHashTable_keyidexchange = DataCollectionLibrary.exchangeList.ToDictionary(x => x.idexchange, x => x);

            foreach (Instrument_mongo instrument in DataCollectionLibrary.instrumentList)
            {
                instrument.exchange = DataCollectionLibrary.exchangeHashTable_keyidexchange[instrument.idexchange];
            }

            /// <summary>
            /// filled out initial futures contracts to have minimum for instruments
            /// fills DataCollectionLibrary.optionSpreadExpressionList
            /// </summary>
            ///


            foreach (Instrument_mongo instrument in DataCollectionLibrary.instrumentList)
            {
                List<Contract_mongo> contractQuery =
                    MongoDBConnectionAndSetup.GetContracts(DateTime.Now.Date, instrument.idinstrument);

                Mapper.Initialize(cfg => cfg.CreateMap<Contract_mongo, Asset>());

                foreach (Contract_mongo contract in contractQuery)
                {
                    //Console.WriteLine(contract.idcontract + " " + contract.idinstrument + " " + contract.contractname
                    //    + " " + contract.expirationdate);

                    Asset asset = Mapper.Map<Asset>(contract);

                    asset._type = ASSET_TYPE_MONGO.fut.ToString();

                    asset.yearFraction =
                        calcYearFrac(asset.expirationdate, DateTime.Now.Date);

                    MongoDB_OptionSpreadExpression mose = new MongoDB_OptionSpreadExpression(
                        OPTION_SPREAD_CONTRACT_TYPE.FUTURE,
                            OPTION_EXPRESSION_TYPES.SPREAD_LEG_PRICE);

                    mose.asset = asset;

                    mose.instrument = instrument;

                    DataCollectionLibrary.optionSpreadExpressionList.Add(mose);

                    var key = Tuple.Create(asset.idcontract, asset._type);

                    DataCollectionLibrary.optionSpreadExpressionHashTable_key_Id_Type
                        .TryAdd(key, mose);

                    DataCollectionLibrary.optionSpreadExpressionHashTable_cqgSymbol.TryAdd(mose.asset.cqgsymbol, mose);

                }
            }



            AppendTo_optionSpreadExpressionHashTable();



        }

        internal void FillAccountPosition(bool initializing)
        {
            //List<AccountPosition> archive_pos_list = new List<AccountPosition>();

            if (initializing)
            {
                foreach (string accountName in DataCollectionLibrary.accountNameList)
                {
                    AccountPosition archive = MongoDBConnectionAndSetup.GetAccountArchivePositionsInfoFromMongo(accountName);

                    DataCollectionLibrary.accountPositionsArchiveList.Add(archive);
                }
            }

            Dictionary<string, AccountPosition> archive_pos_dictionary = DataCollectionLibrary.accountPositionsArchiveList.ToDictionary(x => x.name, x => x);

            List<AccountPosition> new_pos = MongoDBConnectionAndSetup.GetAccountPositionsInfoFromMongo(DataCollectionLibrary.accountNameList);

            Dictionary<string, AccountPosition> new_pos_dictionary = new_pos.ToDictionary(x => x.name, x => x);


            foreach (AccountPosition ap in new_pos)
            {
                if (archive_pos_dictionary.ContainsKey(ap.name))
                {
                    AccountPosition prev_acctpos = archive_pos_dictionary[ap.name];

                    Dictionary<string, Position> compare_pos_dictionary = prev_acctpos.positions.ToDictionary(x => x.asset.name, x => x);

                    foreach (Position p in ap.positions)
                    {
                        if (compare_pos_dictionary.ContainsKey(p.asset.name))
                        {
                            Position archive_p = compare_pos_dictionary[p.asset.name];

                            p.prev_qty = archive_p.qty;

                            archive_p.comparedTo_Archive_OnInitialization = true;
                        }
                    }
                }
                else
                {
                    foreach (Position p in ap.positions)
                    {
                        p.prev_qty = 0;
                    }
                }
            }


            foreach (AccountPosition ap_archive in DataCollectionLibrary.accountPositionsArchiveList)
            {
                if (new_pos_dictionary.ContainsKey(ap_archive.name))
                {
                    AccountPosition acctPos = new_pos_dictionary[ap_archive.name];

                    foreach (Position p_archive in ap_archive.positions)
                    {
                        if (!p_archive.comparedTo_Archive_OnInitialization)
                        {
                            p_archive.prev_qty = p_archive.qty;

                            p_archive.qty = 0;

                            acctPos.positions.Add(p_archive);
                        }


                    }
                }


            }



            DataCollectionLibrary.accountPositionsList = new_pos;

            //DataCollectionLibrary.accountPositionsList = new List<AccountPosition>();
        }

        internal void RefreshAccountInfo()
        {
            //DataCollectionLibrary.accountPositionsList = MongoDBConnectionAndSetup.GetAccountPositionsInfoFromMongo(DataCollectionLibrary.accountNameList);

            //AdjustQtyBasedOnDate();

            FillAccountPosition(false);

            AppendTo_optionSpreadExpressionHashTable();
        }

        private void AdjustQtyBasedOnDate()
        {
            foreach (AccountPosition ap in DataCollectionLibrary.accountPositionsList)
            {
                foreach (Position p in ap.positions)
                {
                    if (ap.date_now.CompareTo(DateTime.Now.Date) < 0)
                    {
                        p.prev_qty = p.qty;
                    }
                }
            }
        }

        /// <summary>
        /// Gets all instrument id from the account positions
        /// </summary>
        /// <returns>a list of the instrument ids</returns>
        private List<long> GetAllInstrumentIds()
        {
            //used to test when no contracts in account_positions 
            //List<long> instrumentIdList = new List<long>();
            //instrumentIdList.Add(21);

            List<long> instrumentIdList = new List<long>();

            foreach (AccountPosition ap in DataCollectionLibrary.accountPositionsList)
            {
                foreach (Position p in ap.positions)
                {
                    if (!instrumentIdList.Contains(p.asset.idinstrument))
                    {
                        instrumentIdList.Add(p.asset.idinstrument);
                    }
                }
            }

            return instrumentIdList;
        }

        /// <summary>
        /// sets up the summary p/l memory
        /// all in static heapspace
        /// </summary>
        private void SetupInstrumentSummaryList()
        {

            foreach (Instrument_mongo im in DataCollectionLibrary.instrumentList)
            {
                for (int portfolioGroupCnt = 0; portfolioGroupCnt <= DataCollectionLibrary.portfolioAllocation.accountAllocation.Count; portfolioGroupCnt++)
                {
                    im.instrumentModelCalcTotals_ByAccount.Add(new LiveSpreadTotals());

                    im.instrumentSpreadTotals_ByAccount.Add(new LiveSpreadTotals());



                    im.instrumentADMCalcTotalsByAccount.Add(new LiveSpreadTotals());

                    im.instrumentADMSpreadTotalsByAccount.Add(new LiveSpreadTotals());
                }

            }

            //the total calcs includes an extra increment in array size to include sum of all portfoliogroups
            //DataTotalLibrary.portfolioADMSpreadCalcTotals = new LiveSpreadTotals[DataCollectionLibrary.portfolioAllocation.accountAllocation.Count + 1];
            //DataTotalLibrary.portfolioADMSpreadTotals = new LiveSpreadTotals[DataCollectionLibrary.portfolioAllocation.accountAllocation.Count + 1];



            for (int portfolioGroupCnt = 0; portfolioGroupCnt <= DataCollectionLibrary.portfolioAllocation.accountAllocation.Count; portfolioGroupCnt++)
            {
                DataTotalLibrary.portfolioSpreadCalcTotals.Add(new LiveSpreadTotals());
                DataTotalLibrary.portfolioSpreadTotals.Add(new LiveSpreadTotals());


                DataTotalLibrary.portfolioADMSpreadCalcTotals.Add(new LiveSpreadTotals());
                DataTotalLibrary.portfolioADMSpreadTotals.Add(new LiveSpreadTotals());
            }
        }

        private void AppendTo_optionSpreadExpressionHashTable()
        {
            /// <summary>
            /// filled out positions into OptionSpreadExpression
            /// </summary>
            ///            
            foreach (AccountPosition ap in DataCollectionLibrary.accountPositionsList)
            {
                foreach (Position p in ap.positions)
                {

                    p.asset.yearFraction =
                        calcYearFrac(p.asset.expirationdate, DateTime.Now.Date);

                    MongoDB_OptionSpreadExpression mose;

                    long idkey = 0;

                    bool isoption = false;

                    if (p.asset._type.CompareTo(ASSET_TYPE_MONGO.fut.ToString()) == 0)
                    {
                        idkey = p.asset.idcontract;

                        mose = new MongoDB_OptionSpreadExpression(
                           OPTION_SPREAD_CONTRACT_TYPE.FUTURE,
                               OPTION_EXPRESSION_TYPES.SPREAD_LEG_PRICE);
                    }
                    else
                    {
                        isoption = true;

                        idkey = p.asset.idoption;

                        if (p.asset.callorput.CompareTo(Convert.ToChar(OPTION_CONTRACT_TYPE_MONGO.C.ToString())) == 0)
                        {
                            mose = new MongoDB_OptionSpreadExpression(
                                OPTION_SPREAD_CONTRACT_TYPE.CALL,
                                    OPTION_EXPRESSION_TYPES.SPREAD_LEG_PRICE);
                        }
                        else
                        {
                            mose = new MongoDB_OptionSpreadExpression(
                                OPTION_SPREAD_CONTRACT_TYPE.PUT,
                                    OPTION_EXPRESSION_TYPES.SPREAD_LEG_PRICE);
                        }

                    }



                    var key = Tuple.Create(idkey, p.asset._type);

                    if (!DataCollectionLibrary.optionSpreadExpressionHashTable_key_Id_Type.ContainsKey(key))
                    {
                        mose.asset = p.asset;  //add asset to the mongo option expression

                        //mose.asset.yearFraction = calcYearFrac(p.asset.expirationdate, DateTime.Now.Date);

                        mose.instrument = DataCollectionLibrary.instrumentHashTable_keyinstrumentid[p.asset.idinstrument];


                        DataCollectionLibrary.optionSpreadExpressionList.Add(mose);

                        DataCollectionLibrary.optionSpreadExpressionHashTable_key_Id_Type.TryAdd(key, mose);

                        DataCollectionLibrary.optionSpreadExpressionHashTable_cqgSymbol.TryAdd(mose.asset.cqgsymbol, mose);

                        p.mose = mose;

                        if (isoption)
                        {
                            var futurekey = Tuple.Create(p.asset.idcontract, ASSET_TYPE_MONGO.fut.ToString());

                            MongoDB_OptionSpreadExpression future_mose = DataCollectionLibrary.optionSpreadExpressionHashTable_key_Id_Type[futurekey];

                            future_mose.optionExpressionsThatUseThisFutureAsUnderlying
                                   .Add(p.mose);

                            p.mose.underlyingFutureExpression = future_mose;
                        }

                    }
                    else
                    {
                        p.mose = DataCollectionLibrary.optionSpreadExpressionHashTable_key_Id_Type[key];

                        if (isoption)
                        {

                            TSErrorCatch.debugWriteOut(p.mose.underlyingFutureExpression.asset.cqgsymbol);
                        }
                    }


                }
            }

            FillSettlementsAndImpliedVol();
        }

        /// <summary>
        /// fills settlements and implied vol of all MongoDB_OptionSpreadExpressions
        /// </summary>
        private void FillSettlementsAndImpliedVol()
        {
            TMLAzureModelDBQueries TMLAzureModelDBQueries = new TMLAzureModelDBQueries();

            foreach (MongoDB_OptionSpreadExpression mose in DataCollectionLibrary.optionSpreadExpressionList)
            {
                if (!mose.yesterdaySettlementFilled)
                {
                    if (mose.asset._type.CompareTo(ASSET_TYPE_MONGO.fut.ToString()) == 0)
                    {
                        TMLAzureModelDBQueries.GetContractLatestSettlement(mose);
                    }
                    else if (mose.asset._type.CompareTo(ASSET_TYPE_MONGO.opt.ToString()) == 0)
                    {
                        TMLAzureModelDBQueries.GetOptionLatestSettlementAndImpliedVol(mose);

                        mose.asset.optionExpirationTime = 
                            TMLAzureModelDBQueries.QueryOptionExpirationTimes(mose.instrument.idinstrument, mose.asset.optionmonthint);
                    }
                }
            }
        }

        private void createCQGconnection()
        {
#if DEBUG
            try
#endif
            {
                optionCQGDataManagement = new OptionCQGDataManagement(this,
                    optionRealtimeStartup);
                //optionRealtimeStartup, dataErrorCheck, optionSpreadExpressionList,
                //currentDateContractListMainIdx, optionBuildCommonMethods);
                //optionCQGDataManagement.initializeCQGAndCallbacks();
            }
#if DEBUG
            catch (Exception ex)
            {
                TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
            }
#endif
        }




        /// <summary>
        /// Starts the option spread gu is.
        /// </summary>
        public void startOptionSpreadGUIs()
        {
            sModelADMCompareCalculationAndDisplay
                = new ModelADMCompareCalculationAndDisplay(this);

            //need to run initializing of OptionRealtimeMonitor
            sOptionRealtimeMonitor = new OptionRealtimeMonitor(this, //optionStrategies,                
                optionArrayTypes);
            //, //contractSummaryExpressionListIdx,
            //FCM_PostionList_forCompare);


            sStatusAndConnectedUpdates
                = new StatusAndConnectedUpdates(sOptionRealtimeMonitor, this);

            sGridViewContractSummaryManipulation
                = new GridViewContractSummaryManipulation(this, statusAndConnectedUpdates);



            //set the initially selected instrument to all instruments
            DataCollectionLibrary.instrumentSelectedInTreeGui = TradingSystemConstants.ALL_INSTRUMENTS_SELECTED;


            //should be done after initializing OptionRealtimeMonitor b/c will know rows
            //fillExpressionOptionList();

            //fillADMExpressionOptionList();

            fillAllExpressionsForCQGData();



            sOptionRealtimeMonitor.realtimeMonitorStartupBackgroundUpdateLoop();



            sOptionRealtimeMonitor.setupTreeViewInstruments();

            sOptionRealtimeMonitor.setupTreeViewBrokerAcct();

            gridViewContractSummaryManipulation.setupContractSummaryLiveData(sOptionRealtimeMonitor);
            //sOptionRealtimeMonitor.setupContractSummaryLiveData();

            sModelADMCompareCalculationAndDisplay.fillGridModelADMComparison(sOptionRealtimeMonitor);



            sOptionRealtimeMonitor.Show();

            optionRealtimeStartup.BringToFront();
        }


        private void setupInstrumentSpecificFIXFields()
        {
            SaveOutputFile sof = new SaveOutputFile();

            String fullFile = System.IO.Path.Combine(Directory.GetCurrentDirectory(),
                    TradingSystemConstants.INITIALIZE_CONFIG_DIRECTORY,
                    TradingSystemConstants.REALTIME_CONFIGURATION);

            //Dictionary<string, InstrumentSpecificFIXFields> instrumentSpecificFIXFieldHashSet =
            sof.readInstrumentSpecificFIXField(fullFile, DataCollectionLibrary.instrumentSpecificFIXFieldHashSet);
        }

        private List<OptionStrategy> getSupplementalContracts(OptionArrayTypes optionArrayTypes,
            QueryFutureIdContractDelegate queryFutureIdContractDelegate, object queryObject)
        {
            SaveOutputFile sof = new SaveOutputFile();

            String fullFile = System.IO.Path.Combine(Directory.GetCurrentDirectory(),
                    TradingSystemConstants.INITIALIZE_CONFIG_DIRECTORY,
                    TradingSystemConstants.SUPPLEMENT_CONTRACTS);

            //Dictionary<string, InstrumentSpecificFIXFields> instrumentSpecificFIXFieldHashSet =
            return sof.readSupplementContracts(fullFile, optionArrayTypes, DataCollectionLibrary.initializationParms.idPortfolioGroup,
                queryFutureIdContractDelegate, queryObject);

        }

        internal string getInstrumentSpecificFieldKey(StageOrdersToTTWPFLibrary.Model.OrderModel orderModel)
        {
            StringBuilder key = new StringBuilder();
            key.Append(orderModel.broker_18220);
            key.Append(orderModel.underlyingGateway);
            key.Append(orderModel.underlyingExchange);
            key.Append(orderModel.underlyingExchangeSymbol);

            return key.ToString();
        }





        private string getOptionIdHashSetKeyString(int monthInt,
            int optionYear,
            long idinstrument,
            string callOrPut,
            double strikeInDecimal)
        {
            StringBuilder keyString = new StringBuilder();

            keyString.Append(monthInt);
            keyString.Append(".");
            keyString.Append(optionYear);
            keyString.Append(".");
            keyString.Append(idinstrument);
            keyString.Append(".");
            keyString.Append(callOrPut);
            keyString.Append(".");
            keyString.Append(strikeInDecimal);

            //if (!optionIdFromInfo.ContainsKey(keyString.ToString()))
            //{
            //    optionIdFromInfo.Add(keyString.ToString(), optionId);
            //}

            return keyString.ToString();
        }


        internal string getFutureContractIdHashSetKeyString(int monthInt,
            int futureContractYear,
            long idinstrument)
        {
            StringBuilder keyString = new StringBuilder();

            keyString.Append(monthInt);
            keyString.Append(".");
            keyString.Append(futureContractYear);
            keyString.Append(".");
            keyString.Append(idinstrument);

            return keyString.ToString();
        }


        private void fillAllExpressionsForCQGData()
        {


            //fillExpressionOptionList();

            //fillSubstituteEODExpressionOptionList();



            if (FCM_DataImportLibrary.FCM_Import_Consolidated != null)
            {
                //fillADMExpressionOptionList();

                //fillSubstitueEODADMExpressionOptionList();
            }


            // add risk free rate to expressions

            MongoDB_OptionSpreadExpression expLstRiskFreeRate = new MongoDB_OptionSpreadExpression(
                    OPTION_SPREAD_CONTRACT_TYPE.BLANK,
                    OPTION_EXPRESSION_TYPES.OPTION_EXPRESSION_RISK_FREE_RATE);


            if (DataCollectionLibrary.initializationParms.useCloudDb)
            {
                TMLAzureModelDBQueries btdb = new TMLAzureModelDBQueries();

                expLstRiskFreeRate.optionInputFieldsFromTblOptionInputSymbols =
                    btdb.queryOptionInputSymbols(-1,
                    (int)OPTION_FORMULA_INPUT_TYPES.OPTION_RISK_FREE_RATE);
            }


            expLstRiskFreeRate.asset.cqgsymbol =
                expLstRiskFreeRate.optionInputFieldsFromTblOptionInputSymbols
                .optionInputCQGSymbol;

            expLstRiskFreeRate.asset._type = ASSET_TYPE_MONGO.risk_free_rate.ToString();

            //expLstRiskFreeRate.normalSubscriptionRequest = true;

            DataCollectionLibrary.optionSpreadExpressionList.Add(expLstRiskFreeRate);

            DataCollectionLibrary.riskFreeRateExpression = expLstRiskFreeRate;

            //var key = Tuple.Create()

            //DataCollectionLibrary.optionSpreadExpressionHashTable_key_Id_Type.TryAdd(key, mose_new);

            //DataCollectionLibrary.optionSpreadExpressionHashTable_cqgSymbol.TryAdd(expLstRiskFreeRate.asset.cqgsymbol, expLstRiskFreeRate);



            sOptionRealtimeMonitor.updateSetupExpressionListGridView();
        }




        private void fillADMExpressionOptionList()
        {
#if DEBUG
            try
#endif
            {


                foreach (ADMPositionImportWeb admPositionImportWeb in FCM_DataImportLibrary.FCM_Import_Consolidated)
                {

                    bool foundCqgSymbol = false;


                    MongoDB_OptionSpreadExpression mose = null;

                    if (DataCollectionLibrary.optionSpreadExpressionHashTable_cqgSymbol.ContainsKey(admPositionImportWeb.asset.cqgsymbol))
                    {
                        mose =
                            DataCollectionLibrary.optionSpreadExpressionHashTable_cqgSymbol[admPositionImportWeb.asset.cqgsymbol];

                        foundCqgSymbol = true;
                    }

                    if (!foundCqgSymbol)
                    {
                        MongoDB_OptionSpreadExpression mose_new = new MongoDB_OptionSpreadExpression(
                            admPositionImportWeb.callPutOrFuture,
                            OPTION_EXPRESSION_TYPES.SPREAD_LEG_PRICE);

                        //mose_new.asset.cqgsymbol = admPositionImportWeb.cqgsymbol;

                        mose_new.asset = admPositionImportWeb.asset;


                        mose_new.instrument = admPositionImportWeb.instrument;

                        DateTime currentDate = DateTime.Now.Date;

                        mose_new.impliedVolFromSpan =
                            admPositionImportWeb.contractData.impliedVolFromDB;

                        mose_new.previousDateTimeBoundaryStart
                            = admPositionImportWeb.contractData.dataDateTimeFromDB.Date
                            .AddHours(
                                admPositionImportWeb.instrument.customdayboundarytime.Hour)
                            .AddMinutes(
                               admPositionImportWeb.instrument.customdayboundarytime.Minute)
                            .AddMinutes(1);

                        //mose_new.yesterdaySettlement =
                        //    admPositionImportWeb.contractData.settlementPriceFromDB;
                        //mose_new.yesterdaySettlementFilled = true;

                        char callOrPutOrFutureChar = 'C';

                        switch (mose_new.callPutOrFuture)
                        {
                            case OPTION_SPREAD_CONTRACT_TYPE.CALL:
                                callOrPutOrFutureChar = 'C';
                                break;

                            case OPTION_SPREAD_CONTRACT_TYPE.PUT:
                                callOrPutOrFutureChar = 'P';
                                break;

                            case OPTION_SPREAD_CONTRACT_TYPE.FUTURE:
                                callOrPutOrFutureChar = 'F';
                                break;
                        }

                        mose_new.callPutOrFutureChar = callOrPutOrFutureChar;

                        bool isoption = false;

                        long idkey = 0;

                        if (mose_new.callPutOrFuture != OPTION_SPREAD_CONTRACT_TYPE.FUTURE)
                        {
                            isoption = true;

                            idkey = mose_new.asset.idoption;

                            //mose_new.optionId =
                            //    admPositionImportWeb.contractInfo.idOption;

                            //mose_new.underlyingFutureId =
                            //    admPositionImportWeb.contractInfo.idUnderlyingContract;

                            //mose_new.asset.strikeprice =
                            //    admPositionImportWeb.contractInfo.optionStrikePrice;

                            //mose_new.asset.yearFraction =
                            //    calcYearFrac(
                            //    admPositionImportWeb.contractInfo.expirationDate,
                            //                        DateTime.Now.Date);

                            //mose_new.optionMonthInt = admPositionImportWeb.contractInfo.optionMonthInt;

                            //mose_new.optionYear = admPositionImportWeb.contractInfo.optionYear;
                        }
                        else
                        {
                            idkey = mose_new.asset.idcontract;

                            //mose_new.futureId =
                            //    admPositionImportWeb.contractInfo.idContract;

                            //mose_new.futureContractMonthInt = admPositionImportWeb.contractInfo.contractMonthInt;

                            //mose_new.futureContractYear = admPositionImportWeb.contractInfo.contractYear;
                        }


                        var key = Tuple.Create(idkey, mose_new.asset._type);

                        DataCollectionLibrary.optionSpreadExpressionList.Add(mose_new);

                        DataCollectionLibrary.optionSpreadExpressionHashTable_key_Id_Type.TryAdd(key, mose_new);

                        DataCollectionLibrary.optionSpreadExpressionHashTable_cqgSymbol.TryAdd(mose_new.asset.cqgsymbol, mose_new);

                        if (isoption)
                        {
                            var futurekey = Tuple.Create(mose_new.asset.idcontract, ASSET_TYPE_MONGO.fut.ToString());

                            MongoDB_OptionSpreadExpression future_mose = DataCollectionLibrary.optionSpreadExpressionHashTable_key_Id_Type[futurekey];

                            future_mose.optionExpressionsThatUseThisFutureAsUnderlying
                                   .Add(mose_new);

                            mose_new.underlyingFutureExpression = future_mose;
                        }


                        admPositionImportWeb.contractData.optionSpreadExpression = mose_new;

                        admPositionImportWeb.optionSpreadExpression = mose_new;

                        //mose_new.normalSubscriptionRequest = true;
                    }
                    else
                    {

                        admPositionImportWeb.contractData.optionSpreadExpression = mose;

                        admPositionImportWeb.optionSpreadExpression = mose;

                        //mose.normalSubscriptionRequest = true;
                    }

                }

            }
#if DEBUG
            catch (Exception ex)
            {
                TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
            }
#endif
        }

        private void fillSubstitueEODADMExpressionOptionList()
        {

        }

        public double calcYearFrac(DateTime expirationDate, DateTime currentDateTime)
        {
            double yearFrac = 0;

#if DEBUG
            try
#endif
            {
                TimeSpan spanBetweenCurrentAndExp =
                                   expirationDate - currentDateTime.Date;

                yearFrac = spanBetweenCurrentAndExp.TotalDays / TradingSystemConstants.DAYS_IN_YEAR;

                if (yearFrac < 0)
                {
                    yearFrac = 0;
                }
            }
#if DEBUG
            catch (Exception ex)
            {
                TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
            }
#endif
            return yearFrac;
        }

        public void callOptionRealTimeData(bool sendOnlyUnsubscribed)
        {
#if DEBUG
            try
#endif
            {
                //dataErrorCheck.dataInError = false;

                if (optionCQGDataManagement != null)
                {
                    optionCQGDataManagement.sendSubscribeRequest(sendOnlyUnsubscribed);

                    sOptionRealtimeMonitor.hideMessageToReconnect();
                }
            }
#if DEBUG
            catch (Exception ex)
            {
                TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
            }
#endif
        }

        public void reInitializeCQG()
        {
            optionCQGDataManagement.resetCQG();
        }

        public void fullReConnectCQG()
        {
#if DEBUG
            try
#endif
            {
                //dataErrorCheck.dataInError = false;

                if (optionCQGDataManagement != null)
                {
                    optionCQGDataManagement.shutDownCQGConn();

                    Thread.Sleep(1000);

                    optionCQGDataManagement.connectCQG();

                    sOptionRealtimeMonitor.displayMessageToReConnect();

                    optionRealtimeStartup.BringToFront();
                }
            }
#if DEBUG
            catch (Exception ex)
            {
                TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
            }
#endif
        }



        public void resetDataUpdatesWithLatestExpressions()
        {
            if (optionCQGDataManagement != null)
            {
                optionCQGDataManagement.stopDataManagementAndTotalCalcThreads();
            }

            //DataCollectionLibrary.optionSpreadExpressionList.Clear();

            sOptionRealtimeMonitor.displayMessageToReConnect();

            fillAllExpressionsForCQGData();

            //sOptionRealtimeMonitor.setupExpressionListGridView();

            optionCQGDataManagement.resetThreadStopVariables();

            optionCQGDataManagement.setupCalculateModelValuesAndSummarizeTotals();
        }



        public static double chooseoptionticksize(double currentOptionPrice, double optionticksize,
            double secondaryoptionticksize, double secondaryoptionticksizeRule)
        {

            if (currentOptionPrice <= secondaryoptionticksizeRule)
            {
                optionticksize = secondaryoptionticksize;
            }

            return optionticksize;
        }

        public static double chooseOptionTickDisplay(double currentOptionPrice, double optionTickDisplay,
            double secondaryOptionTickDisplay, double secondaryoptionticksizeRule)
        {

            if (currentOptionPrice <= secondaryoptionticksizeRule)
            {
                optionTickDisplay = secondaryOptionTickDisplay;
            }

            return optionTickDisplay;
        }

        public static double chooseoptiontickvalue(double currentOptionPrice, double optiontickvalue,
            double secondaryoptiontickvalue, double secondaryoptionticksizeRule)
        {
            if (currentOptionPrice <= secondaryoptionticksizeRule)
            {
                optiontickvalue = secondaryoptiontickvalue;
            }

            return optiontickvalue;
        }

        public double getSyntheticClose(OptionStrategy optionStrategy)
        {
            return optionStrategy.syntheticClose;
        }



        public void RunSpreadTotalCalculations()
        {

            for (int accountCnt = 0; accountCnt <= DataCollectionLibrary.portfolioAllocation.accountAllocation.Count; accountCnt++)
            {
                foreach (Instrument_mongo im in DataCollectionLibrary.instrumentList)
                {
                    im.instrumentModelCalcTotals_ByAccount[accountCnt].pAndLDay = 0;

                    im.instrumentModelCalcTotals_ByAccount[accountCnt].delta = 0;
                }

                DataTotalLibrary.portfolioSpreadCalcTotals[accountCnt].pAndLDay = 0;

                DataTotalLibrary.portfolioSpreadCalcTotals[accountCnt].delta = 0;
            }


            int accountCnt2 = 0;
            foreach (AccountPosition ap in DataCollectionLibrary.accountPositionsList)
            {
                foreach (Position p in ap.positions)
                {
                    //double priceChange = p.mose.defaultPrice - p.mose.yesterdaySettlement;
                    Instrument_mongo im = DataCollectionLibrary.instrumentHashTable_keyinstrumentid[p.asset.idinstrument];

                    double ticksize = 1;
                    double tickvalue = 1;

                    if (p.asset._type.CompareTo(ASSET_TYPE_MONGO.fut.ToString()) == 0)
                    {
                        ticksize = im.ticksize;
                        tickvalue = im.tickvalue;
                    }
                    else
                    {
                        ticksize = im.optionticksize;
                        tickvalue = im.optiontickvalue;
                    }

                    p.positionTotals.pAndLDay = (p.mose.defaultPrice - p.mose.yesterdaySettlement) / ticksize * tickvalue * p.prev_qty;
                    p.positionTotals.pAndLDaySettlementToSettlement = (p.mose.settlement - p.mose.yesterdaySettlement) / ticksize * tickvalue * p.prev_qty;
                    p.positionTotals.delta = p.mose.delta * p.qty;

                    if (p.qty != p.prev_qty && p.mose.transactionPriceFilled)
                    {
                        double orderQty = p.qty - p.prev_qty;
                        p.positionTotals.pAndLDay += (p.mose.defaultPrice - p.mose.transactionPrice) / ticksize * tickvalue * orderQty;
                        p.positionTotals.pAndLDaySettlementToSettlement += (p.mose.settlement - p.mose.transactionPrice) / ticksize * tickvalue * orderQty;
                    }

                    im.instrumentModelCalcTotals_ByAccount[accountCnt2].pAndLDay += p.positionTotals.pAndLDay;

                    im.instrumentModelCalcTotals_ByAccount[accountCnt2].delta += p.positionTotals.delta;



                }



                accountCnt2++;
            }

            foreach (Instrument_mongo im in DataCollectionLibrary.instrumentList)
            {
                LiveSpreadTotals lst = new LiveSpreadTotals();

                int accountCnt3 = 0;

                while (accountCnt3 < DataCollectionLibrary.portfolioAllocation.accountAllocation.Count)
                {
                    lst.pAndLDay += im.instrumentModelCalcTotals_ByAccount[accountCnt3].pAndLDay;
                    lst.delta += im.instrumentModelCalcTotals_ByAccount[accountCnt3].delta;

                    im.instrumentSpreadTotals_ByAccount[accountCnt3].pAndLDay = im.instrumentModelCalcTotals_ByAccount[accountCnt3].pAndLDay;

                    im.instrumentSpreadTotals_ByAccount[accountCnt3].delta = im.instrumentModelCalcTotals_ByAccount[accountCnt3].delta;


                    DataTotalLibrary.portfolioSpreadCalcTotals[accountCnt3].pAndLDay += im.instrumentModelCalcTotals_ByAccount[accountCnt3].pAndLDay;

                    DataTotalLibrary.portfolioSpreadCalcTotals[accountCnt3].delta += im.instrumentModelCalcTotals_ByAccount[accountCnt3].delta;


                    accountCnt3++;
                }

                im.instrumentSpreadTotals_ByAccount[accountCnt3].pAndLDay = lst.pAndLDay;

                im.instrumentSpreadTotals_ByAccount[accountCnt3].delta = lst.delta;
            }

            LiveSpreadTotals lst_final = new LiveSpreadTotals();

            int accountCnt4 = 0;
            while (accountCnt4 < DataCollectionLibrary.portfolioAllocation.accountAllocation.Count)
            {
                lst_final.pAndLDay += DataTotalLibrary.portfolioSpreadCalcTotals[accountCnt4].pAndLDay;
                lst_final.delta += DataTotalLibrary.portfolioSpreadCalcTotals[accountCnt4].delta;

                DataTotalLibrary.portfolioSpreadTotals[accountCnt4].pAndLDay = DataTotalLibrary.portfolioSpreadCalcTotals[accountCnt4].pAndLDay;

                DataTotalLibrary.portfolioSpreadTotals[accountCnt4].delta = DataTotalLibrary.portfolioSpreadCalcTotals[accountCnt4].delta;

                accountCnt4++;
            }

            DataTotalLibrary.portfolioSpreadTotals[accountCnt4].pAndLDay = lst_final.pAndLDay;

            DataTotalLibrary.portfolioSpreadTotals[accountCnt4].delta = lst_final.delta;





        }


        static double MyPow(double num, int exp)
        {
            double result = 1.0;
            while (exp > 0)
            {
                if (exp % 2 == 1)
                    result *= num;
                exp >>= 1;
                num *= num;
            }

            return result;
        }

        public void RunADMSpreadTotalCalculations()
        {
            for (int accountCnt = 0; accountCnt <= DataCollectionLibrary.portfolioAllocation.accountAllocation.Count; accountCnt++)
            {
                foreach (Instrument_mongo im in DataCollectionLibrary.instrumentList)
                {
                    im.instrumentADMCalcTotalsByAccount[accountCnt].pAndLDay = 0;

                    im.instrumentADMCalcTotalsByAccount[accountCnt].delta = 0;

                    im.instrumentADMCalcTotalsByAccount[accountCnt].pAndLDaySettlementToSettlement = 0;
                }

                DataTotalLibrary.portfolioADMSpreadCalcTotals[accountCnt].pAndLDay = 0;

                DataTotalLibrary.portfolioADMSpreadCalcTotals[accountCnt].delta = 0;

                DataTotalLibrary.portfolioADMSpreadCalcTotals[accountCnt].pAndLDaySettlementToSettlement = 0;
            }



            foreach (ADMPositionImportWeb fcmPosition in FCM_DataImportLibrary.FCM_Import_Consolidated)
            {
                if (DateTime.Now.Date.CompareTo(
                    fcmPosition.asset.expirationdate.Date)
                        <= 0
                        && fcmPosition.optionSpreadExpression != null)
                {

                    double tickSize;
                    double tickValue;


                    if (fcmPosition.asset._type.CompareTo(ASSET_TYPE_MONGO.fut.ToString()) == 0)
                    {
                        tickSize = fcmPosition.instrument.ticksize;
                        tickValue = fcmPosition.instrument.tickvalue;
                    }
                    else
                    {
                        if (fcmPosition.instrument.secondaryoptionticksize > 0
                            && fcmPosition.instrument.secondaryoptionticksize
                            < fcmPosition.instrument.optionticksize)
                        {
                            tickSize = fcmPosition.instrument.secondaryoptionticksize;
                            tickValue = fcmPosition.instrument.secondaryoptiontickvalue;
                        }
                        else
                        {
                            tickSize = fcmPosition.instrument.optionticksize;
                            tickValue = fcmPosition.instrument.optiontickvalue;
                        }
                    }

                    double numberOfContracts = fcmPosition.netContractsEditable;

                    int numberOfLongTrans = fcmPosition.transNetLong;

                    int numberOfShortTrans = -fcmPosition.transNetShort;


                    double currentPLChange = (fcmPosition
                                .optionSpreadExpression.defaultPrice

                            - fcmPosition.optionSpreadExpression.yesterdaySettlement)
                            / tickSize
                            * tickValue;

                    double currentLongTransPLChange = (fcmPosition.optionSpreadExpression.defaultPrice

                            - fcmPosition.transAvgLongPrice)
                            / tickSize
                            * tickValue;

                    double currentShortTransPLChange = (fcmPosition.optionSpreadExpression.defaultPrice

                            - fcmPosition.transAvgShortPrice)
                            / tickSize
                            * tickValue;

                    double currentPLChangeSettlementToSettlement = (fcmPosition.optionSpreadExpression.settlement

                            - fcmPosition.optionSpreadExpression.yesterdaySettlement)
                            / tickSize
                            * tickValue;

                    double currentLongTransPLChangeToSettle = (fcmPosition.optionSpreadExpression.settlement

                            - fcmPosition.transAvgLongPrice)
                            / tickSize
                            * tickValue;

                    double currentShortTransPLChangeToSettle = (fcmPosition.optionSpreadExpression.settlement

                            - fcmPosition.transAvgShortPrice)
                            / tickSize
                            * tickValue;


                    //if (fcmPosition.instrument.eodAnalysisAtInstrument)
                    {
                        fcmPosition
                            .contractData.pAndLDaySettlementToSettlement =
                                currentPLChangeSettlementToSettlement * numberOfContracts;
                    }

                    fcmPosition
                        .contractData.pAndLDaySettleOrders =
                        currentLongTransPLChangeToSettle * numberOfLongTrans
                        + currentShortTransPLChangeToSettle * numberOfShortTrans;

                    //*************************
                    fcmPosition
                        .contractData.pAndLLongSettlementOrders =
                        currentLongTransPLChangeToSettle * numberOfLongTrans;

                    fcmPosition
                        .contractData.pAndLShortSettlementOrders =
                        currentShortTransPLChangeToSettle * numberOfShortTrans;
                    //*************************


                    fcmPosition
                        .contractData.pAndLDay =
                        currentPLChange * numberOfContracts;

                    fcmPosition
                        .contractData.pAndLDayOrders =
                        currentLongTransPLChange * numberOfLongTrans
                        + currentShortTransPLChange * numberOfShortTrans;


                    //*************************
                    fcmPosition
                        .contractData.pAndLLongOrders =
                        currentLongTransPLChange * numberOfLongTrans;

                    fcmPosition
                        .contractData.pAndLShortOrders =
                        currentShortTransPLChange * numberOfShortTrans;
                    //*************************


                    fcmPosition
                        .contractData.delta =
                        (numberOfContracts + numberOfLongTrans + numberOfShortTrans)
                        * fcmPosition.optionSpreadExpression.delta;



                    //sLiveADMStrategyInfoList[spreadIdx].liveSpreadADMTotals.pAndLDay = spreadTotalPL;

                    //sLiveADMStrategyInfoList[spreadIdx].liveSpreadADMTotals.delta = spreadTotalDelta;

                    //optionStrategies[spreadIdx].syntheticCloseFilled = true;
                }


                Instrument_mongo im = DataCollectionLibrary.instrumentHashTable_keyinstrumentid[fcmPosition.instrument.idinstrument];

                im.instrumentADMCalcTotalsByAccount[fcmPosition.acctGroup.acctIndex_UsedForTotals_Visibility].pAndLDay
                    += fcmPosition.contractData.pAndLDay + fcmPosition.contractData.pAndLDayOrders;

                im.instrumentADMCalcTotalsByAccount[fcmPosition.acctGroup.acctIndex_UsedForTotals_Visibility].delta
                    += fcmPosition.contractData.delta;

                im.instrumentADMCalcTotalsByAccount[fcmPosition.acctGroup.acctIndex_UsedForTotals_Visibility].pAndLDaySettlementToSettlement
                    += fcmPosition.contractData.pAndLDaySettlementToSettlement
                        + fcmPosition.contractData.pAndLDaySettleOrders;

            }


            foreach (Instrument_mongo im in DataCollectionLibrary.instrumentList)
            {
                LiveSpreadTotals lst = new LiveSpreadTotals();

                int accountCnt3 = 0;

                while (accountCnt3 < DataCollectionLibrary.portfolioAllocation.accountAllocation.Count)
                {
                    lst.pAndLDay += im.instrumentADMCalcTotalsByAccount[accountCnt3].pAndLDay;
                    lst.delta += im.instrumentADMCalcTotalsByAccount[accountCnt3].delta;
                    lst.pAndLDaySettlementToSettlement += im.instrumentADMCalcTotalsByAccount[accountCnt3].pAndLDaySettlementToSettlement;

                    im.instrumentADMSpreadTotalsByAccount[accountCnt3].pAndLDay = im.instrumentADMCalcTotalsByAccount[accountCnt3].pAndLDay;

                    im.instrumentADMSpreadTotalsByAccount[accountCnt3].delta = im.instrumentADMCalcTotalsByAccount[accountCnt3].delta;

                    im.instrumentADMSpreadTotalsByAccount[accountCnt3].pAndLDaySettlementToSettlement =
                        im.instrumentADMCalcTotalsByAccount[accountCnt3].pAndLDaySettlementToSettlement;



                    DataTotalLibrary.portfolioADMSpreadCalcTotals[accountCnt3].pAndLDay += im.instrumentADMCalcTotalsByAccount[accountCnt3].pAndLDay;

                    DataTotalLibrary.portfolioADMSpreadCalcTotals[accountCnt3].delta += im.instrumentADMCalcTotalsByAccount[accountCnt3].delta;

                    DataTotalLibrary.portfolioADMSpreadCalcTotals[accountCnt3].pAndLDaySettlementToSettlement +=
                        im.instrumentADMCalcTotalsByAccount[accountCnt3].pAndLDaySettlementToSettlement;


                    accountCnt3++;
                }

                im.instrumentADMSpreadTotalsByAccount[accountCnt3].pAndLDay = lst.pAndLDay;

                im.instrumentADMSpreadTotalsByAccount[accountCnt3].delta = lst.delta;

                im.instrumentADMSpreadTotalsByAccount[accountCnt3].pAndLDaySettlementToSettlement = lst.pAndLDaySettlementToSettlement;
            }


            LiveSpreadTotals lst_final = new LiveSpreadTotals();

            int accountCnt4 = 0;
            while (accountCnt4 < DataCollectionLibrary.portfolioAllocation.accountAllocation.Count)
            {
                lst_final.pAndLDay += DataTotalLibrary.portfolioADMSpreadCalcTotals[accountCnt4].pAndLDay;
                lst_final.delta += DataTotalLibrary.portfolioADMSpreadCalcTotals[accountCnt4].delta;
                lst_final.pAndLDaySettlementToSettlement += DataTotalLibrary.portfolioADMSpreadCalcTotals[accountCnt4].pAndLDaySettlementToSettlement;

                DataTotalLibrary.portfolioADMSpreadTotals[accountCnt4].pAndLDay = DataTotalLibrary.portfolioADMSpreadCalcTotals[accountCnt4].pAndLDay;

                DataTotalLibrary.portfolioADMSpreadTotals[accountCnt4].delta = DataTotalLibrary.portfolioADMSpreadCalcTotals[accountCnt4].delta;

                DataTotalLibrary.portfolioADMSpreadTotals[accountCnt4].pAndLDaySettlementToSettlement
                    = DataTotalLibrary.portfolioADMSpreadCalcTotals[accountCnt4].pAndLDaySettlementToSettlement;

                accountCnt4++;
            }

            DataTotalLibrary.portfolioADMSpreadTotals[accountCnt4].pAndLDay = lst_final.pAndLDay;

            DataTotalLibrary.portfolioADMSpreadTotals[accountCnt4].delta = lst_final.delta;

            DataTotalLibrary.portfolioADMSpreadTotals[accountCnt4].pAndLDaySettlementToSettlement = lst_final.pAndLDaySettlementToSettlement;


            //for (int admPosCnt = 0; admPosCnt < admPositionImportWeb.Count; admPosCnt++)
            //{
            //    Instrument_mongo im = DataCollectionLibrary.instrumentHashTable_keyinstrumentid[admPositionImportWeb[admPosCnt].idinstrument];

            //    im.instrumentADMCalcTotalsByAccount[admPositionImportWeb[admPosCnt].acctGroup.acctIndex_UsedForTotals_Visibility].pAndLDay
            //        += admPositionImportWeb[admPosCnt].contractData.pAndLDay
            //            + admPositionImportWeb[admPosCnt].contractData.pAndLDayOrders;

            //    im.instrumentADMCalcTotalsByAccount[admPositionImportWeb[admPosCnt].acctGroup.acctIndex_UsedForTotals_Visibility].delta
            //        += admPositionImportWeb[admPosCnt].contractData.delta;

            //    im.instrumentADMCalcTotalsByAccount[admPositionImportWeb[admPosCnt].acctGroup.acctIndex_UsedForTotals_Visibility].pAndLDaySettlementToSettlement
            //        += admPositionImportWeb[admPosCnt].contractData.pAndLDaySettlementToSettlement
            //            + admPositionImportWeb[admPosCnt].contractData.pAndLDaySettleOrders;

            //}
        }



        public double roundPriceForCallOrPut(OPTION_SPREAD_CONTRACT_TYPE callOrPut, double futurePriceFactor,
            double optionstrikeincrement)
        {
            if (callOrPut == OPTION_SPREAD_CONTRACT_TYPE.CALL)
            {
                futurePriceFactor = ((int)futurePriceFactor + 1) * optionstrikeincrement;
            }
            else if (callOrPut == OPTION_SPREAD_CONTRACT_TYPE.PUT)
            {
                futurePriceFactor = ((int)futurePriceFactor) * optionstrikeincrement;
            }

            return futurePriceFactor;

        }

        

        

        public void shutDownOptionSpreadRealtime()
        {
#if DEBUG
            try
#endif
            {

                writeInitializationConfigFile();

                //calcFutEquivOfSyntheticCloseThreadShouldStop = true;

                if (_stageOrdersLibrary != null)
                {
                    _stageOrdersLibrary.shutDownAndLogOff();
                }

                if (sOptionRealtimeMonitor != null)
                {
                    sOptionRealtimeMonitor.cancelBackgroundWorker();
                }

                //Thread.Sleep(TradingSystemConstants.OPTIONREALTIMEREFRESH);



                if (optionCQGDataManagement != null)
                {
                    optionCQGDataManagement.stopDataManagementAndTotalCalcThreads();

                    optionCQGDataManagement.shutDownCQGConn();
                }

                int loopCounter = 0;

                while (ThreadTracker.threadCount > 0 && loopCounter < 10)
                {
                    Thread.Sleep(1000);//TradingSystemConstants.MODEL_CALC_TIME_REFRESH);
                    loopCounter++;
                }

                optionRealtimeStartup.Close();
            }
#if DEBUG
            catch (Exception ex)
            {
                TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
            }
#endif
        }

        void writeInitializationConfigFile()
        {
#if DEBUG
            try
#endif
            {
                SaveOutputFile sof = new SaveOutputFile(TradingSystemConstants.INITIALIZE_CONFIG_DIRECTORY);

                sof.createConfigFile(TradingSystemConstants.INITIALIZE_OPTION_REALTIME_FILE_NAME);

                Type configTypes = typeof(INITIALIZATION_CONFIG_VARS);
                Array configNames = Enum.GetNames(configTypes);

                sof.writeConfigLineFile(configNames.GetValue((int)INITIALIZATION_CONFIG_VARS.PORTFOLIOGROUP).ToString(),
                    DataCollectionLibrary.initializationParms.portfolioGroupName);

                sof.writeConfigLineFile(configNames.GetValue((int)INITIALIZATION_CONFIG_VARS.DBSERVERNAME).ToString(),
                    DataCollectionLibrary.initializationParms.dbServerName);

                //sof.writeConfigLineFile(configNames.GetValue((int)INITIALIZATION_CONFIG_VARS.BROKER).ToString(),
                //    initializationParmsSaved.FIX_Broker_18220);

                //sof.writeConfigLineFile(configNames.GetValue((int)INITIALIZATION_CONFIG_VARS.ACCOUNT).ToString(),
                //    initializationParmsSaved.FIX_Acct);

                sof.closeAndSaveFile();
            }
#if DEBUG
            catch (Exception ex)
            {
                TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
            }
#endif
        }

        public void updateEODMonitorDataSettings(bool sendOnlyUnsubscribed)
        {
            Thread updateEODMonitorDataSettingsThread
             = new Thread(new ParameterizedThreadStart(updateEODMonitorDataSettingsRun));
            updateEODMonitorDataSettingsThread.IsBackground = true;
            updateEODMonitorDataSettingsThread.Start(sendOnlyUnsubscribed);
        }

        private void updateEODMonitorDataSettingsRun(Object obj)
        {
            ThreadTracker.openThread(null, null);

            bool sendOnlyUnsubscribed = (bool)obj;

            //fillRollIntoLegExpressions();

            if (DataCollectionLibrary.realtimeMonitorSettings.eodAnalysis)
            {
                foreach (MongoDB_OptionSpreadExpression ope in DataCollectionLibrary.optionSpreadExpressionList)
                {
                    if (ope.optionExpressionType != OPTION_EXPRESSION_TYPES.OPTION_EXPRESSION_RISK_FREE_RATE
                        && ope.instrument.substitutesymbol_eod == 1)
                    {
                        optionCQGDataManagement.fillEODSubstitutePrices(ope);
                    }
                }
            }

            sOptionRealtimeMonitor.updateSetupExpressionListGridView();

            optionCQGDataManagement.sendSubscribeRequest(sendOnlyUnsubscribed);

            updatedRealtimeMonitorSettings(null);

            ThreadTracker.closeThread(null, null);
        }

        public void updatedRealtimeMonitorSettingsThreadRun()
        {
            Thread updatedRealtimeMonitorSettingsThread
             = new Thread(new ParameterizedThreadStart(updatedRealtimeMonitorSettings));
            updatedRealtimeMonitorSettingsThread.IsBackground = true;
            updatedRealtimeMonitorSettingsThread.Start();
        }

        public void updatedRealtimeMonitorSettings(Object obj)
        {
            ThreadTracker.openThread(null, null);

            sOptionRealtimeMonitor.updateStatusStripOptionMonitor();


            for (int i = 0; i < DataCollectionLibrary.optionSpreadExpressionList.Count; i++)
            {
                optionCQGDataManagement.manageExpressionPriceCalcs(DataCollectionLibrary.optionSpreadExpressionList[i]);
            }

            ThreadTracker.closeThread(null, null);
        }

        //public void updateStatusDataFilling()
        //{
        //    int subscribedCount = 0;
        //    for (int i = 0; i < optionSpreadExpressionList.Count; i++)
        //    {
        //        //optionCQGDataManagement.manageExpressionPriceCalcs(optionSpreadExpressionList[i]);
        //        if (optionSpreadExpressionList[i].setSubscriptionLevel)
        //        {
        //            subscribedCount++;
        //        }
        //    }
        //}

        public String rowHeaderLabelCreate(OptionStrategy optionStrategy)
        {
            StringBuilder header = new StringBuilder();
            header.Append(optionStrategy.idStrategy);
            header.Append(" (");
            header.Append(optionStrategy.instrument.cqgsymbol);
            header.Append(") - ");

            header.Append(optionStrategy.instrument
                .customdayboundarytime.ToString("HH:mm", DateTimeFormatInfo.InvariantInfo));

            return header.ToString().Replace('_', ' ');
        }


        internal void fillADMInputWithWebPositions(String[] configFileNames, ImportFileCheck importFileCheck)
        {

            List<ADMPositionImportWeb> FCM_positionImportNotConsolidated_Temp = new List<ADMPositionImportWeb>();

            sADMDataCommonMethods.readADMDetailedPositionImportWeb(configFileNames,
                        FCM_positionImportNotConsolidated_Temp, importFileCheck);

            if (importFileCheck.importfile)
            {
                FCM_DataImportLibrary.FCM_positionImportNotConsolidated = FCM_positionImportNotConsolidated_Temp;

                setupAdmWebDetailedDataForDisplay();

                netOutContractsInDetailedWebView();


                setupAdmWebInputDataFromAzure();



                if (FCM_DataImportLibrary.FCM_Import_Consolidated != null)
                {
                    fillADMExpressionOptionList();

                    FillSettlementsAndImpliedVol();
                }


                dateOfADMPositionsFile
                    = sADMDataCommonMethods.getFileDateTimeOfADMPositionsWeb(configFileNames[0]);

            }

            //return importFileCheck.importfile;
        }

        public void setupAdmWebDetailedDataForDisplay()
        {
            //fills instrument data into ADM data for display


            foreach (ADMPositionImportWeb fcmImport in FCM_DataImportLibrary.FCM_positionImportNotConsolidated)
            {

                var key = Tuple.Create(fcmImport.POFFIC, fcmImport.PACCT);

                if (DataCollectionLibrary.portfolioAllocation.accountAllocation_KeyOfficAcct.ContainsKey(key))
                {
                    fcmImport.acctGroup
                        = DataCollectionLibrary.portfolioAllocation.accountAllocation_KeyOfficAcct[key];
                    fcmImport.MODEL_OFFICE_ACCT
                        = DataCollectionLibrary.portfolioAllocation.accountAllocation_KeyOfficAcct[key].FCM_POFFIC_PACCT;

                }

                if (DataCollectionLibrary.instrumentHashTable_keyadmcode.ContainsKey(fcmImport.PFC))
                {
                    Instrument_mongo instrument = DataCollectionLibrary.instrumentHashTable_keyadmcode[
                        fcmImport.PFC];

                    fcmImport.instrument = instrument;

                    fcmImport.idinstrument = instrument.idinstrument;

                    aDMDataCommonMethods.subContractType((ADM_Input_Data)fcmImport,
                        fcmImport.PSUBTY);

                    if (fcmImport.isFuture)
                    {
                        fcmImport.TradePrice *=
                            instrument.admfuturepricefactor;
                    }
                    else
                    {
                        fcmImport.TradePrice *=
                            instrument.admoptionpricefactor;
                    }
                }


            }

        }

        /// <summary>
        /// Nets the out contracts in detailed web view.
        /// </summary>
        private void netOutContractsInDetailedWebView()
        {
            FCM_DataImportLibrary.FCM_Import_Consolidated = new List<ADMPositionImportWeb>();

            HashSet<int> indexAlreadyIn = new HashSet<int>();

            for (int i = 0; i < FCM_DataImportLibrary.FCM_positionImportNotConsolidated.Count; i++)
            {
                if (!indexAlreadyIn.Contains(i))
                {

                    indexAlreadyIn.Add(i);

                    ADMPositionImportWeb admPositionImportWeb = new ADMPositionImportWeb();

                    FCM_DataImportLibrary.FCM_Import_Consolidated.Add(admPositionImportWeb);

                    sADMDataCommonMethods.copyADMPositionImportWeb(admPositionImportWeb, FCM_DataImportLibrary.FCM_positionImportNotConsolidated[i]);

                    admPositionImportWeb.LongQuantity = 0;
                    admPositionImportWeb.ShortQuantity = 0;

                    netOutTypeContractsOfWebImport(admPositionImportWeb, FCM_DataImportLibrary.FCM_positionImportNotConsolidated[i]);

                    for (int j = 0; j < FCM_DataImportLibrary.FCM_positionImportNotConsolidated.Count; j++)
                    {

                        /*
            E = 10^-12

            A==B  ::  ABS(A-B) < E

            A>B  ::  A-B>= +E

            A<B  ::  A-B<= -E

            A>=B  ::  A-B> -E

            A<=B  ::  A-B< +E

            A!=B  ::  ABS(A-B)>= E
            */

                        if (!indexAlreadyIn.Contains(j))
                        {
                            //if (admPositionImportWeb.PCUSIP.CompareTo(
                            //    admPositionImportWebForImportDisplay[j].PCUSIP) == 0)

                            if (admPositionImportWeb.POFFIC.CompareTo(FCM_DataImportLibrary.FCM_positionImportNotConsolidated[j].POFFIC) == 0
                                && admPositionImportWeb.PACCT.CompareTo(FCM_DataImportLibrary.FCM_positionImportNotConsolidated[j].PACCT) == 0

                                && admPositionImportWeb.PEXCH == FCM_DataImportLibrary.FCM_positionImportNotConsolidated[j].PEXCH
                                && admPositionImportWeb.PFC.CompareTo(FCM_DataImportLibrary.FCM_positionImportNotConsolidated[j].PFC) == 0
                                && admPositionImportWeb.PCTYM.CompareTo(FCM_DataImportLibrary.FCM_positionImportNotConsolidated[j].PCTYM) == 0
                                && admPositionImportWeb.PSUBTY.CompareTo(FCM_DataImportLibrary.FCM_positionImportNotConsolidated[j].PSUBTY) == 0
                                &&
                                Math.Abs(admPositionImportWeb.strikeInDecimal - FCM_DataImportLibrary.FCM_positionImportNotConsolidated[j].strikeInDecimal) < 0.0000000001)
                            {
                                indexAlreadyIn.Add(j);

                                netOutTypeContractsOfWebImport(admPositionImportWeb, FCM_DataImportLibrary.FCM_positionImportNotConsolidated[j]);
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < FCM_DataImportLibrary.FCM_Import_Consolidated.Count; i++)
            {
                if (FCM_DataImportLibrary.FCM_Import_Consolidated[i].transNetLong != 0)
                {
                    FCM_DataImportLibrary.FCM_Import_Consolidated[i].transAvgLongPrice /= FCM_DataImportLibrary.FCM_Import_Consolidated[i].transNetLong;
                }

                if (FCM_DataImportLibrary.FCM_Import_Consolidated[i].transNetShort != 0)
                {
                    FCM_DataImportLibrary.FCM_Import_Consolidated[i].transAvgShortPrice /= FCM_DataImportLibrary.FCM_Import_Consolidated[i].transNetShort;
                }

            }

            foreach (ADMPositionImportWeb fcm_DataImportLib in FCM_DataImportLibrary.FCM_Import_Consolidated)
            {
                if (fcm_DataImportLib.transNetLong != 0)
                {
                    fcm_DataImportLib.transAvgLongPrice /= fcm_DataImportLib.transNetLong;
                }

                if (fcm_DataImportLib.transNetShort != 0)
                {
                    fcm_DataImportLib.transAvgShortPrice /= fcm_DataImportLib.transNetShort;
                }

            }
        }

        private void netOutTypeContractsOfWebImport(ADMPositionImportWeb admPositionImportWeb,
            ADMPositionImportWeb consolidateFromPositionImportWeb)
        {
            if (consolidateFromPositionImportWeb.RecordType.Trim().CompareTo("Position") == 0)
            {
                admPositionImportWeb.LongQuantity += consolidateFromPositionImportWeb.LongQuantity;

                admPositionImportWeb.ShortQuantity += consolidateFromPositionImportWeb.ShortQuantity;

                admPositionImportWeb.Net = admPositionImportWeb.LongQuantity -
                    admPositionImportWeb.ShortQuantity;

                admPositionImportWeb.netContractsEditable = admPositionImportWeb.Net;
            }
            else if (consolidateFromPositionImportWeb.RecordType.Trim().CompareTo("Transaction") == 0)
            {

                if (consolidateFromPositionImportWeb.LongQuantity != 0)
                {
                    admPositionImportWeb.transAvgLongPrice += consolidateFromPositionImportWeb.LongQuantity
                        * consolidateFromPositionImportWeb.TradePrice;

                    admPositionImportWeb.transNetLong += (int)consolidateFromPositionImportWeb.LongQuantity;
                }

                if (consolidateFromPositionImportWeb.ShortQuantity != 0)
                {
                    admPositionImportWeb.transAvgShortPrice += consolidateFromPositionImportWeb.ShortQuantity
                        * consolidateFromPositionImportWeb.TradePrice;

                    admPositionImportWeb.transNetShort += (int)consolidateFromPositionImportWeb.ShortQuantity;
                }
            }
        }



        public void displayADMInputWithWebPositions()
        {
            if (sAdmReportWebPositionsForm == null)
            {
                sAdmReportWebPositionsForm = new AdmReportSummaryForm();
            }

            sAdmReportWebPositionsForm.fillFileDateTimeLabel(dateOfADMPositionsFile);

            //sAdmReportWebPositionsForm.displayADMInputWithWebPositions(this, optionRealtimeMonitor.contractSummaryInstrumentSelectedIdx);

            sAdmReportWebPositionsForm.Show();

            sAdmReportWebPositionsForm.BringToFront();

            sAdmReportWebPositionsForm.setupAdmInputSummaryGrid(FCM_DataImportLibrary.FCM_positionImportNotConsolidated);


        }

        public void highlightAdmReportWebPositionsInADMStrategyInfoList(int stratCounter, int legCounter)
        {
            //if (!liveADMStrategyInfoList[stratCounter].admLegInfo[legCounter].notInADMPositionsWebData)
            //{
            //    int rowInWebPositionsData = findOfADMWebPositionsData(
            //        liveADMStrategyInfoList[stratCounter].admLegInfo[legCounter].aDMPositionImportWeb.cqgSymbol);

            //    if (sAdmReportWebPositionsForm != null)
            //        sAdmReportWebPositionsForm.highlightRow(rowInWebPositionsData, false);
            //}
        }





        internal void setupAdmWebInputDataFromAzure()
        {
            try
            {
                //TMLAzureModelDBQueries btdb = new TMLAzureModelDBQueries();


                foreach (ADMPositionImportWeb admPositionImportWeb in FCM_DataImportLibrary.FCM_Import_Consolidated)
                {
                    //fillInRestOfADMWebInputDataFromAzure(admPositionImportWeb, btdb);

                    fillInRestOfADMWebInputDataFrom_Mongo_AndAzure(admPositionImportWeb);

                    //TSErrorCatch.debugWriteOut(admPositionImportWeb[admRowCounter].cqgSymbol);
                }
            }
            catch (Exception e)
            {
                TSErrorCatch.debugWriteOut(e.ToString());
            }
            //btdb.closeDB();
        }


        /*fillInRestOfADMWebInputData used to fill the variables of the ADM data
         * ADMPositionImportWeb admPositionImportWeb_Local: web import data
         * TMLModelDBQueries btdb: database connection
         * 
         * this fills the CQGSymbol that can be used to compare model contracts and ADM contracts
         */
        internal void fillInRestOfADMWebInputDataFromAzure(
            ADMPositionImportWeb FCM_DatatImportedRow_Local,
            TMLAzureModelDBQueries btdb)
        {

            //{
            //    FCM_DatatImportedRow_Local.dateTime = DateTime.Now.Date;



            //    String dateFormat = "yyyyMM";

            //    FCM_DatatImportedRow_Local.PCTYM_dateTime = DateTime.ParseExact(
            //        FCM_DatatImportedRow_Local.PCTYM, dateFormat, CultureInfo.InvariantCulture);


            //    FCM_DatatImportedRow_Local.contractInfo.legContractType = FCM_DatatImportedRow_Local.callPutOrFuture;

            //    Asset asset = new Asset();
            //    FCM_DatatImportedRow_Local.asset = asset;

            //    if (FCM_DatatImportedRow_Local.callPutOrFuture != OPTION_SPREAD_CONTRACT_TYPE.FUTURE)
            //    {
            //        FCM_DatatImportedRow_Local.contractInfo.optionYear =
            //            FCM_DatatImportedRow_Local.PCTYM_dateTime.Year;

            //        FCM_DatatImportedRow_Local.contractInfo.optionMonthInt =
            //            FCM_DatatImportedRow_Local.PCTYM_dateTime.Month;


            //        String keyString = getOptionIdHashSetKeyString(FCM_DatatImportedRow_Local.contractInfo.optionMonthInt,
            //                        FCM_DatatImportedRow_Local.contractInfo.optionYear,
            //                        FCM_DatatImportedRow_Local.instrument.idinstrument,
            //                        FCM_DatatImportedRow_Local.PSUBTY,
            //                        FCM_DatatImportedRow_Local.strikeInDecimal);

            //        long idOption = 0;

            //        if (optionIdFromInfo.ContainsKey(keyString))
            //        {
            //            idOption = optionIdFromInfo[keyString];
            //        }
            //        else
            //        {
            //            idOption = btdb.queryOptionIDFromInfo(FCM_DatatImportedRow_Local);

            //            optionIdFromInfo.Add(keyString.ToString(), idOption);
            //        }



            //        FCM_DatatImportedRow_Local.contractInfo.idOption = idOption;

            //        btdb.queryOptionInfoAndDataFromCloud(
            //                       FCM_DatatImportedRow_Local.contractInfo.idOption,
            //                       FCM_DatatImportedRow_Local.instrument.idinstrument,
            //                       FCM_DatatImportedRow_Local.contractInfo,
            //                       FCM_DatatImportedRow_Local.contractData,
            //                       DataCollectionLibrary.initializationParms.modelDateTime,
            //                       optionArrayTypes,
            //                       optionDataSetHashSet);

            //        btdb.queryFutureInfoAndDataFromCloud(
            //            FCM_DatatImportedRow_Local.contractInfo.idUnderlyingContract,
            //            FCM_DatatImportedRow_Local.contractInfo,
            //            FCM_DatatImportedRow_Local.contractData,
            //            DataCollectionLibrary.initializationParms.modelDateTime,
            //            optionArrayTypes,
            //            futureDataSetHashSet, true, this, futureIdFromInfo);

            //        FCM_DatatImportedRow_Local.cqgsymbol = FCM_DatatImportedRow_Local.contractInfo.cqgsymbol;

            //        //fill asset object 
            //        FCM_DatatImportedRow_Local.asset.idoption = idOption;
            //        FCM_DatatImportedRow_Local.asset.optionname =
            //            FCM_DatatImportedRow_Local.contractInfo.optionName;
            //        FCM_DatatImportedRow_Local.asset.optionmonth =
            //            FCM_DatatImportedRow_Local.contractInfo.optionMonth;
            //        FCM_DatatImportedRow_Local.asset.optionmonthint =
            //            FCM_DatatImportedRow_Local.contractInfo.optionMonthInt;
            //        FCM_DatatImportedRow_Local.asset.optionyear =
            //            FCM_DatatImportedRow_Local.contractInfo.optionYear;
            //        FCM_DatatImportedRow_Local.asset.strikeprice =
            //            FCM_DatatImportedRow_Local.contractInfo.optionStrikePrice;

            //        FCM_DatatImportedRow_Local.asset.callorput =
            //            FCM_DatatImportedRow_Local.contractInfo.optionCallOrPut;

            //        FCM_DatatImportedRow_Local.asset.contractname =
            //            FCM_DatatImportedRow_Local.contractInfo.contractName;

            //        FCM_DatatImportedRow_Local.asset.month =
            //            FCM_DatatImportedRow_Local.contractInfo.contractMonth;

            //        FCM_DatatImportedRow_Local.asset.monthint =
            //            FCM_DatatImportedRow_Local.contractInfo.contractMonthInt;

            //        FCM_DatatImportedRow_Local.asset.year =
            //            FCM_DatatImportedRow_Local.contractInfo.contractYear;

            //        FCM_DatatImportedRow_Local.asset.idcontract =
            //            FCM_DatatImportedRow_Local.contractInfo.idContract;

            //        FCM_DatatImportedRow_Local.asset.name =
            //            FCM_DatatImportedRow_Local.contractInfo.optionName;

            //        FCM_DatatImportedRow_Local.asset.cqgsymbol =
            //            FCM_DatatImportedRow_Local.contractInfo.cqgsymbol;

            //        FCM_DatatImportedRow_Local.asset._type =
            //            ASSET_TYPE_MONGO.opt.ToString();

            //        FCM_DatatImportedRow_Local.asset.expirationdate =
            //            FCM_DatatImportedRow_Local.contractInfo.expirationDate;

            //        FCM_DatatImportedRow_Local.asset.idinstrument =
            //            FCM_DatatImportedRow_Local.instrument.idinstrument;

            //        FCM_DatatImportedRow_Local.asset.yearFraction =
            //            calcYearFrac(FCM_DatatImportedRow_Local.contractInfo.expirationDate,
            //                DateTime.Now.Date);


            //    }
            //    else // is a future contract
            //    {
            //        FCM_DatatImportedRow_Local.contractInfo.contractYear =
            //            FCM_DatatImportedRow_Local.PCTYM_dateTime.Year;

            //        FCM_DatatImportedRow_Local.contractInfo.contractMonthInt =
            //            FCM_DatatImportedRow_Local.PCTYM_dateTime.Month;




            //        String keyString = getFutureContractIdHashSetKeyString(FCM_DatatImportedRow_Local.contractInfo.contractMonthInt,
            //                        FCM_DatatImportedRow_Local.contractInfo.contractYear,
            //                        FCM_DatatImportedRow_Local.instrument.idinstrument);


            //        long idContract = 0;

            //        if (futureIdFromInfo.ContainsKey(keyString))
            //        {
            //            idContract = futureIdFromInfo[keyString];
            //        }
            //        else
            //        {
            //            idContract = btdb.queryFutureContractId(FCM_DatatImportedRow_Local);
            //        }


            //        //int idContract = btdb.queryFutureContractId(admPositionImportWeb_Local);

            //        FCM_DatatImportedRow_Local.contractInfo.idContract = idContract;

            //        btdb.queryFutureInfoAndDataFromCloud(
            //                        FCM_DatatImportedRow_Local.contractInfo.idContract,
            //                        FCM_DatatImportedRow_Local.contractInfo,
            //                        FCM_DatatImportedRow_Local.contractData,
            //                        FCM_DatatImportedRow_Local.dateTime,
            //                        optionArrayTypes,
            //                        futureDataSetHashSet, false, this, futureIdFromInfo);

            //        FCM_DatatImportedRow_Local.cqgsymbol = FCM_DatatImportedRow_Local.contractInfo.cqgsymbol;


            //        //fill asset object                     

            //        FCM_DatatImportedRow_Local.asset.contractname =
            //            FCM_DatatImportedRow_Local.contractInfo.contractName;

            //        FCM_DatatImportedRow_Local.asset.month =
            //            FCM_DatatImportedRow_Local.contractInfo.contractMonth;

            //        FCM_DatatImportedRow_Local.asset.monthint =
            //            FCM_DatatImportedRow_Local.contractInfo.contractMonthInt;

            //        FCM_DatatImportedRow_Local.asset.year =
            //            FCM_DatatImportedRow_Local.contractInfo.contractYear;

            //        FCM_DatatImportedRow_Local.asset.idcontract =
            //            FCM_DatatImportedRow_Local.contractInfo.idContract;

            //        FCM_DatatImportedRow_Local.asset.name =
            //            FCM_DatatImportedRow_Local.contractInfo.optionName;

            //        FCM_DatatImportedRow_Local.asset.cqgsymbol =
            //            FCM_DatatImportedRow_Local.contractInfo.cqgsymbol;

            //        FCM_DatatImportedRow_Local.asset._type =
            //            ASSET_TYPE_MONGO.fut.ToString();

            //        FCM_DatatImportedRow_Local.asset.expirationdate =
            //            FCM_DatatImportedRow_Local.contractInfo.expirationDate;

            //        FCM_DatatImportedRow_Local.asset.idinstrument =
            //            FCM_DatatImportedRow_Local.instrument.idinstrument;

            //        FCM_DatatImportedRow_Local.asset.yearFraction =
            //            calcYearFrac(FCM_DatatImportedRow_Local.contractInfo.expirationDate,
            //                DateTime.Now.Date);



            //    }

            //    if (FCM_DatatImportedRow_Local.instrument.substitutesymbol_eod == 1)
            //    {
            //        //sets up data for realtime EOD SPAN symbol

            //        FCM_DatatImportedRow_Local.contractInfo.useSubstitueSymbolEOD = true;

            //        FCM_DatatImportedRow_Local.contractInfo.instrumentSymbolPreEOD =
            //            FCM_DatatImportedRow_Local.instrument.instrumentsymbol_pre_eod;

            //        FCM_DatatImportedRow_Local.contractInfo.instrumentSymbolEODSubstitute =
            //            FCM_DatatImportedRow_Local.instrument.instrumentsymboleod_eod;

            //        if (FCM_DatatImportedRow_Local.cqgsymbol != null)
            //        {
            //            //admPositionImportWeb_Local.contractInfo.cqgSubstituteSymbol =
            //            //    admPositionImportWeb_Local.cqgSymbol.Replace(
            //            //        admPositionImportWeb_Local.contractInfo.instrumentSymbolPreEOD,
            //            //        admPositionImportWeb_Local.contractInfo.instrumentSymbolEODSubstitute);

            //            generateCQGSymbolForEODSubstitution(
            //                        FCM_DatatImportedRow_Local.contractInfo, FCM_DatatImportedRow_Local.instrument);

            //            FCM_DatatImportedRow_Local.cqgSubstituteSymbol = FCM_DatatImportedRow_Local.contractInfo.cqgSubstituteSymbol;
            //        }
            //    }

            //}
        }


        internal void fillInRestOfADMWebInputDataFrom_Mongo_AndAzure(
            ADMPositionImportWeb FCM_DatatImportedRow_Local)
        {

            {
                FCM_DatatImportedRow_Local.dateTime = DateTime.Now.Date;



                String dateFormat = "yyyyMM";

                FCM_DatatImportedRow_Local.PCTYM_dateTime = DateTime.ParseExact(
                    FCM_DatatImportedRow_Local.PCTYM, dateFormat, CultureInfo.InvariantCulture);


                //FCM_DatatImportedRow_Local.contractInfo.legContractType = FCM_DatatImportedRow_Local.callPutOrFuture;

                Asset asset = new Asset();
                FCM_DatatImportedRow_Local.asset = asset;

                if (FCM_DatatImportedRow_Local.callPutOrFuture != OPTION_SPREAD_CONTRACT_TYPE.FUTURE)
                {
                    //FCM_DatatImportedRow_Local.contractInfo.optionYear =
                    //    FCM_DatatImportedRow_Local.PCTYM_dateTime.Year;

                    //FCM_DatatImportedRow_Local.contractInfo.optionMonthInt =
                    //    FCM_DatatImportedRow_Local.PCTYM_dateTime.Month;


                    FCM_DatatImportedRow_Local.asset.optionyear =
                        FCM_DatatImportedRow_Local.PCTYM_dateTime.Year;

                    FCM_DatatImportedRow_Local.asset.optionmonthint =
                        FCM_DatatImportedRow_Local.PCTYM_dateTime.Month;



                    Option_mongo option_mongo = MongoDBConnectionAndSetup.GetOption(FCM_DatatImportedRow_Local.asset.optionmonthint,
                        FCM_DatatImportedRow_Local.asset.optionyear,
                        FCM_DatatImportedRow_Local.instrument.idinstrument,
                        FCM_DatatImportedRow_Local.PSUBTY, FCM_DatatImportedRow_Local.strikeInDecimal);


                    FCM_DatatImportedRow_Local.asset.cqgsymbol = option_mongo.cqgsymbol;
                    FCM_DatatImportedRow_Local.asset.optionyear = option_mongo.optionyear;
                    FCM_DatatImportedRow_Local.asset.idinstrument = option_mongo.idinstrument;
                    FCM_DatatImportedRow_Local.asset.idcontract = option_mongo.idcontract;
                    FCM_DatatImportedRow_Local.asset.strikeprice = option_mongo.strikeprice;
                    FCM_DatatImportedRow_Local.asset.callorput = option_mongo.callorput;
                    FCM_DatatImportedRow_Local.asset.optionmonth = option_mongo.optionmonth;
                    FCM_DatatImportedRow_Local.asset.idoption = option_mongo.idoption;
                    FCM_DatatImportedRow_Local.asset.expirationdate = option_mongo.expirationdate;
                    FCM_DatatImportedRow_Local.asset.optionname = option_mongo.optionname;
                    FCM_DatatImportedRow_Local.asset.optionmonthint = option_mongo.optionmonthint;


                    FCM_DatatImportedRow_Local.asset.name = option_mongo.optionname;

                    FCM_DatatImportedRow_Local.asset._type = ASSET_TYPE_MONGO.opt.ToString();

                    FCM_DatatImportedRow_Local.asset.yearFraction =
                        calcYearFrac(FCM_DatatImportedRow_Local.asset.expirationdate,
                            DateTime.Now.Date);


                    FCM_DatatImportedRow_Local.cqgsymbol = FCM_DatatImportedRow_Local.asset.cqgsymbol;

                }
                else // is a future contract
                {
                    //FCM_DatatImportedRow_Local.contractInfo.contractYear =
                    //    FCM_DatatImportedRow_Local.PCTYM_dateTime.Year;

                    //FCM_DatatImportedRow_Local.contractInfo.contractMonthInt =
                    //    FCM_DatatImportedRow_Local.PCTYM_dateTime.Month;

                    FCM_DatatImportedRow_Local.asset.year =
                        FCM_DatatImportedRow_Local.PCTYM_dateTime.Year;

                    FCM_DatatImportedRow_Local.asset.monthint =
                        FCM_DatatImportedRow_Local.PCTYM_dateTime.Month;


                    //String keyString = getFutureContractIdHashSetKeyString(FCM_DatatImportedRow_Local.contractInfo.contractMonthInt,
                    //                FCM_DatatImportedRow_Local.contractInfo.contractYear,
                    //                FCM_DatatImportedRow_Local.instrument.idinstrument);


                    //long idContract = 0;

                    //if (futureIdFromInfo.ContainsKey(keyString))
                    //{
                    //    idContract = futureIdFromInfo[keyString];
                    //}
                    //else
                    //{
                    //    idContract = btdb.queryFutureContractId(FCM_DatatImportedRow_Local);
                    //}

                    Contract_mongo contract_mongo = MongoDBConnectionAndSetup.GetContract(
                        FCM_DatatImportedRow_Local.instrument.idinstrument,
                        FCM_DatatImportedRow_Local.asset.year,
                        FCM_DatatImportedRow_Local.asset.monthint);


                    FCM_DatatImportedRow_Local.asset.cqgsymbol = contract_mongo.cqgsymbol;

                    FCM_DatatImportedRow_Local.asset.contractname = contract_mongo.contractname;

                    FCM_DatatImportedRow_Local.asset.idcontract = contract_mongo.idcontract;

                    FCM_DatatImportedRow_Local.asset.month = contract_mongo.month;

                    FCM_DatatImportedRow_Local.asset.expirationdate = contract_mongo.expirationdate;

                    FCM_DatatImportedRow_Local.asset.year = contract_mongo.year;

                    FCM_DatatImportedRow_Local.asset.idinstrument = contract_mongo.idinstrument;

                    FCM_DatatImportedRow_Local.asset._type =
                        ASSET_TYPE_MONGO.fut.ToString();

                    FCM_DatatImportedRow_Local.asset.yearFraction =
                        calcYearFrac(contract_mongo.expirationdate, DateTime.Now.Date);

                    FCM_DatatImportedRow_Local.cqgsymbol = FCM_DatatImportedRow_Local.asset.cqgsymbol;

                }

                if (FCM_DatatImportedRow_Local.instrument.substitutesymbol_eod == 1)
                {
                    //sets up data for realtime EOD SPAN symbol

                    //FCM_DatatImportedRow_Local.contractInfo.useSubstitueSymbolEOD = true;

                    //FCM_DatatImportedRow_Local.contractInfo.instrumentSymbolPreEOD =
                    //    FCM_DatatImportedRow_Local.instrument.instrumentsymbol_pre_eod;

                    //FCM_DatatImportedRow_Local.contractInfo.instrumentSymbolEODSubstitute =
                    //    FCM_DatatImportedRow_Local.instrument.instrumentsymboleod_eod;

                    //if (FCM_DatatImportedRow_Local.asset.cqgsymbol != null)
                    //{
                    //    //admPositionImportWeb_Local.contractInfo.cqgSubstituteSymbol =
                    //    //    admPositionImportWeb_Local.cqgSymbol.Replace(
                    //    //        admPositionImportWeb_Local.contractInfo.instrumentSymbolPreEOD,
                    //    //        admPositionImportWeb_Local.contractInfo.instrumentSymbolEODSubstitute);

                    //    generateCQGSymbolForEODSubstitution(
                    //                FCM_DatatImportedRow_Local.contractInfo, FCM_DatatImportedRow_Local.instrument);

                    //    FCM_DatatImportedRow_Local.cqgSubstituteSymbol = FCM_DatatImportedRow_Local.contractInfo.cqgSubstituteSymbol;
                    //}
                }

            }
        }


        /// <summary>
        /// Fills the adm strategy objects from saved files.
        /// </summary>
        public void fillADMStrategyObjectsFromSavedFiles()
        {
            String[] nameOfADMFile = new String[1];
            nameOfADMFile[0] = sADMDataCommonMethods.getNameOfADMPositionImportWebStored();

            ImportFileCheck importFileCheck = new ImportFileCheck();

            importFileCheck.importingBackedUpSavedFile = true;

            fillADMInputWithWebPositions(nameOfADMFile, importFileCheck);

        }



        /// <summary>
        /// Reads the adm exlude contract file.
        /// </summary>
        public void readADMExludeContractFile()
        {
            //String nameOfADMFile = sADMDataCommonMethods.getNameOfADMPositionImportWebStored();

            //admPositionImportWebForImportDisplay
            //            = sADMDataCommonMethods.readADMDetailedPositionImportWeb(nameOfADMFile);

            sADMDataCommonMethods.readADMExcludeContractInfo(this);
        }



        //public int findOfADMWebPositionsData(String cqgSymbol)
        //{
        //    int rowCounter = 0;

        //    while (rowCounter < sAdmPositionImportWeb.Count)
        //    {
        //        if (sAdmPositionImportWeb[rowCounter].cqgsymbol.CompareTo(cqgSymbol) == 0)
        //        {
        //            return rowCounter;
        //        }

        //        rowCounter++;
        //    }

        //    return -1;
        //}



        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Adm web positions and model comparison. </summary>
        ///
        /// <remarks>   Steve Pickering, 8/12/2013. </remarks>
        ///
        /// <param name="strategyCount">    LiveADMStrategy strategy count. </param>
        /// <param name="legIdx">           Zero-based index of the leg. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public void admWebPositionsAndModelComparison(int strategyCount,
            int legIdx)
        {
            //search through strategies and find futures leg



            //if (sLiveADMStrategyInfoList[strategyCount].optionStrategy != null)
            //{
            //    if (sLiveADMStrategyInfoList[strategyCount].admLegInfo[legIdx].aDMPositionImportWeb.callPutOrFuture
            //        == OPTION_SPREAD_CONTRACT_TYPE.FUTURE)
            //    {

            //        //TSErrorCatch.debugWriteOut("test");

            //        OptionStrategy optionStrategy = sLiveADMStrategyInfoList[strategyCount].optionStrategy;

            //        int modelLots = 0;

            //        int stratLegCount = 0;

            //        while (stratLegCount < optionStrategy.legInfo.Length)
            //        {
            //            if (optionStrategy.legInfo[stratLegCount].legContractType
            //                == OPTION_SPREAD_CONTRACT_TYPE.FUTURE)
            //            {
            //                modelLots += (int)optionStrategy.optionStrategyParameters[(int)TBL_STRATEGY_STATE_FIELDS.currentPosition]
            //                    .stateValueParsed[stratLegCount];


            //            }

            //            stratLegCount++;
            //        }

            //        sLiveADMStrategyInfoList[strategyCount].admLegInfo[legIdx].numberOfModelContracts = modelLots;
            //    }
            //    //else
            //    //{

            //    //}
            //}
        }

        public void updateADMSummaryForm(long contractSummaryInstrumentSelectedIdx)
        {
            if (sAdmReportWebPositionsForm != null)
            {
                sAdmReportWebPositionsForm.updateSelectedInstrumentFromTreeADM(contractSummaryInstrumentSelectedIdx);
            }
        }

        internal string selectAcct(string tradingTechnologiesExchangeSymbol,
            AccountAllocation portfolioGroupAllocation, bool fillingPayoffGrid)
        {
            string acct = "";

            acct = portfolioGroupAllocation.account;

            return acct;
        }

        private String generateCQGSymbolForEODSubstitution(LegInfo legInfo, Instrument_mongo instrument)
        {
            string cqgSubstitutionSymbol = "";

            if (legInfo.legContractType == OPTION_SPREAD_CONTRACT_TYPE.FUTURE)
            {
                legInfo.cqgSubstituteSymbol =
                                       legInfo.cqgsymbol.Replace(
                                           legInfo.instrumentSymbolPreEOD,
                                           legInfo.instrumentSymbolEODSubstitute);
            }
            else
            {
                //internal Instrument[] instruments { get; set; }
                //private Instrument_DefaultFutures[] instrument_DefaultFuturesArray;

                //internal Dictionary<int, Instrument> substituteInstrumentHash = new Dictionary<int, Instrument>();

                //int idxCnt = 0;
                //while(idxCnt < instruments.Length)
                //{
                //    if(instruments[idxCnt].idinstrument == legInfo.idUnderlyingContract)
                //    {
                //        break;
                //    }

                //    idxCnt++;
                //}

                Instrument_mongo instrumentSubstitute =
                    substituteInstrumentHash[instrument.instrumentid_eod];

                legInfo.cqgSubstituteSymbol =
                    generateOptionCQGSymbolForEODSubstitution(
                        legInfo.optionCallOrPut,
                        legInfo.instrumentSymbolEODSubstitute,
                        legInfo.optionMonth.ToString(),
                        legInfo.optionYear,
                        legInfo.optionStrikePrice,
                        instrumentSubstitute.optionstrikeincrement,
                        instrumentSubstitute.optionstrikedisplay,
                        instrumentSubstitute.idinstrument);
            }

            //generateOptionCQGSymbolForEODSubstitution(
            //    optionStrategies[i].rollIntoLegInfo[legCounter].optionCallOrPut,
            //    optionStrategies[i].rollIntoLegInfo[legCounter].instrumentSymbolEODSubstitute,
            //    optionStrategies[i].rollIntoLegInfo[legCounter].contractMonth,
            //    optionStrategies[i].rollIntoLegInfo[legCounter].optionYear,
            //    optionStrategies[i].rollIntoLegInfo[legCounter].optionStrikePrice,
            //    optionStrategies[i].rollIntoLegInfo[legCounter].instrumentSymbolEODSubstitute);


            return cqgSubstitutionSymbol;
        }

        private String generateOptionCQGSymbolForEODSubstitution(
            char contractTypeOptionOrPut,
            //int contractType, 
            String underlyingSymbol, String month, int year,
            double optionStrikePrice, double optionstrikeincrement, double optionStrikeDisplay, long instrumentId)
        {
            StringBuilder cqgSymbol = new StringBuilder();

#if DEBUG
            try
#endif
            {
                //cqgSymbol.Append(contractTypeCharSymbolArray.GetValue(contractType).ToString());

                cqgSymbol.Append(contractTypeOptionOrPut);

                //cqgSymbol.Append(".US.");

                cqgSymbol.Append(".");

                cqgSymbol.Append(underlyingSymbol);

                cqgSymbol.Append(month);

                cqgSymbol.Append((year % 100));

                cqgSymbol.Append(ConversionAndFormatting.convertToStrikeForCQGSymbol(
                                        optionStrikePrice,
                                        optionstrikeincrement,
                                        optionStrikeDisplay, instrumentId));
            }
#if DEBUG
            catch (Exception ex)
            {
                TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
            }
#endif

            return cqgSymbol.ToString();
        }
    }
}
