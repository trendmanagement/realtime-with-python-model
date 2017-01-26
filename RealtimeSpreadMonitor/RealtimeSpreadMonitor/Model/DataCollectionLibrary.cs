using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RealtimeSpreadMonitor.Mongo;
using RealtimeSpreadMonitor.Forms;
using RealtimeSpreadMonitor.FormManipulation;

namespace RealtimeSpreadMonitor.Model
{
    static class DataCollectionLibrary
    {
        

        /// <summary>
        /// the following are all used to keep track of the instruments called out to CQG
        /// </summary>
        internal static List<MongoDB_OptionSpreadExpression> optionSpreadExpressionList
            = new List<MongoDB_OptionSpreadExpression>();

        internal static ConcurrentDictionary<Tuple<long, string>, MongoDB_OptionSpreadExpression> optionSpreadExpressionHashTable_key_Id_Type
            = new ConcurrentDictionary<Tuple<long, string>, MongoDB_OptionSpreadExpression>();

        internal static ConcurrentDictionary<string, MongoDB_OptionSpreadExpression> optionSpreadExpressionHashTable_cqgSymbol
            = new ConcurrentDictionary<string, MongoDB_OptionSpreadExpression>();

        internal static ConcurrentDictionary<string, MongoDB_OptionSpreadExpression> optionSpreadExpressionHashTable_keySymbol
            = new ConcurrentDictionary<string, MongoDB_OptionSpreadExpression>();

        internal static ConcurrentDictionary<string, MongoDB_OptionSpreadExpression> optionSpreadExpressionHashTable_keyCQGInId
            = new ConcurrentDictionary<string, MongoDB_OptionSpreadExpression>();

        internal static ConcurrentDictionary<string, MongoDB_OptionSpreadExpression> optionSpreadExpressionHashTable_keyFullName
            = new ConcurrentDictionary<string, MongoDB_OptionSpreadExpression>();
        //^^^^^^^^^^^^^


        

        internal static MongoDB_OptionSpreadExpression riskFreeRateExpression = null;


        internal static PortfolioAllocation portfolioAllocation
                    = new PortfolioAllocation();

        internal static List<string> accountNameList = new List<string>();


        internal static List<Account> accountList
            = new List<Account>();

        internal static List<AccountPosition> accountPositionsList
            = new List<AccountPosition>();

        internal static List<AccountPosition> accountPositionsArchiveList
            = new List<AccountPosition>();



        internal static List<Instrument_mongo> instrumentList
            = new List<Instrument_mongo>();

        internal static Dictionary<long, Instrument_mongo> instrumentHashTable_keyinstrumentid
            = new Dictionary<long, Instrument_mongo>();

        internal static Dictionary<string, Instrument_mongo> instrumentHashTable_keyadmcode
            = new Dictionary<string, Instrument_mongo>();


        internal static List<Instrument_Info> instrumentInfoList
            = new List<Instrument_Info>();

        internal static Dictionary<long, Instrument_Info> instrumentInfoTable_keyinstrumentid
            = new Dictionary<long, Instrument_Info>();



        internal static List<Exchange_mongo> exchangeList
            = new List<Exchange_mongo>();

        internal static Dictionary<long, Exchange_mongo> exchangeHashTable_keyidexchange
            = new Dictionary<long, Exchange_mongo>();




        internal static InitializationParms initializationParms = new InitializationParms();

        internal static RealtimeMonitorSettings realtimeMonitorSettings = new RealtimeMonitorSettings();




        /// <summary>
        /// instrumentSpecificFIXFieldHashSet
        /// the key is a string of broker_18220, underlyingGateway, underlyingExchange and underlyingExchangeSymbol
        /// </summary>
        internal static ConcurrentDictionary<string, InstrumentSpecificFIXFields> instrumentSpecificFIXFieldHashSet
            = new ConcurrentDictionary<string, InstrumentSpecificFIXFields>();


        /// <summary>
        /// this is the instrument that is selected in the instrument tree in the gui
        /// </summary>
        internal static int instrumentSelectedInTreeGui { get; set; }


        //TODO this should be deleted
        internal static List< string> orderSummaryList = new List<string>();

        /// <summary>
        /// used for the updating of data in the order summary
        /// </summary>
        internal static DataTable orderSummaryDataTable = new DataTable();

        /// <summary>
        /// used for the updating of data in the contract summary
        /// </summary>
        internal static DataTable contractSummaryDataTable = new DataTable();
        internal static bool performFullContractRefresh = true;

        /// <summary>
        /// used for the updating of data in the FCM summary
        /// </summary>
        internal static DataTable FCM_SummaryDataTable = new DataTable();
        internal static bool performFull_FCMSummary_Refresh = true;

        /// <summary>
        /// marks whether supplement contracts have been filled, these are contracts that need to be pushed into the realtime when specified
        /// in configuration startup to supply data for contracts
        /// </summary>
        internal static bool supplementContractFilled = false;

        /// <summary>
        /// used to mark whether FIX TT connected
        /// </summary>
        internal static bool _fxceConnected = false;

    }

    static class DataTotalLibrary
    {
        internal static List<LiveSpreadTotals> portfolioSpreadCalcTotals = new List<LiveSpreadTotals>();

        internal static List<LiveSpreadTotals> portfolioADMSpreadCalcTotals = new List<LiveSpreadTotals>();
    }

    static class FCM_DataImportLibrary
    {
        internal static List<ADMPositionImportWeb> FCM_positionImportNotConsolidated;

        internal static List<ADMPositionImportWeb> FCM_Import_Consolidated;

        internal static List<ADMPositionImportWeb> FCM_PostionList_forCompare = new List<ADMPositionImportWeb>();


        internal static DataTable FCM_fullImportDataTableForDisplay = new DataTable();


        /// <summary>
        /// this is the form to see all of the data imported from the GMI files
        /// </summary>
        internal static FCM_ReportSummaryForm FCM_ReportWebPositionsForm;
    }

    static class ContractsModel_Library
    {
        internal static GridViewContractSummaryManipulation gridViewContractSummaryManipulation;

        internal static GridViewFCMPostionManipulation gridViewFCMPostionManipulation;
    }

    static class ThreadTracker
    {
        internal static int threadCount = 0;

        internal static void closeThread(object o, EventArgs e)
        {
            // Decrease number of threads
            threadCount--;
        }

        internal static void openThread(object o, EventArgs e)
        {
            // Decrease number of threads
            threadCount++;
        }
    }

    static class TimerThreadInfo
    {
        internal static TimeSpan _dataResetTime1 = new TimeSpan(7, 10, 0);

        internal static TimeSpan _dataResetTime2 = new TimeSpan(8, 30, 0);

        internal static TimeSpan refreshMongoOrders = new TimeSpan(0, 0, 0);
    }

    
}
