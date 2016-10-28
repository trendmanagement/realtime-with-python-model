using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RealtimeSpreadMonitor.Mongo;

namespace RealtimeSpreadMonitor.Model
{
    static class DataCollectionLibrary
    {
        


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



        internal static List<Exchange_mongo> exchangeList
            = new List<Exchange_mongo>();

        internal static Dictionary<long, Exchange_mongo> exchangeHashTable_keyidexchange
            = new Dictionary<long, Exchange_mongo>();




        //internal static internal Dictionary<string, Instrument_mongo> instrumentHashTable_keyadmcode
        //    = new Dictionary<string, Instrument_mongo>();



        //internal static internal Dictionary<long, List<Contract>> contractHashTableByInstId
        //    = new Dictionary<long, List<Contract>>();


        



        //internal static DataTable contractSummaryGridListDataTable = new DataTable();


        internal static InitializationParms initializationParms = new InitializationParms();

        internal static RealtimeMonitorSettings realtimeMonitorSettings = new RealtimeMonitorSettings();






        

        //internal static int brokerAccountChosen;





        //internal static int brokerAccountChosen;

        

        /// <summary>
        /// The portfolio group allocation chosen
        /// this is a list of the portfoliogroupallocation chosen from the tree
        /// </summary>
        //internal static List<int> portfolioGroupAllocationChosen = new List<int>();

        //internal static ConcurrentDictionary<string, FCM_POFFIC_PACCT> portfolioGroupFcmOfficeAcctChosenHashSet
        //    = new ConcurrentDictionary<string, FCM_POFFIC_PACCT>();

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
        //internal static bool refreshedContractsFromMongo = true;
    }

    static class DataTotalLibrary
    {
        //internal static LiveSpreadTotals[] instrumentModelCalcTotals;
        //internal static LiveSpreadTotals[] instrumentSpreadTotals;
        internal static List<LiveSpreadTotals> portfolioSpreadCalcTotals = new List<LiveSpreadTotals>();
        internal static List<LiveSpreadTotals> portfolioSpreadTotals = new List<LiveSpreadTotals>();

        //internal static LiveSpreadTotals[,] instrumentADMCalcTotalsByAccount;
        //internal static LiveSpreadTotals[,] instrumentADMSpreadTotalsByAccount;
        internal static List<LiveSpreadTotals> portfolioADMSpreadCalcTotals = new List<LiveSpreadTotals>();
        internal static List<LiveSpreadTotals> portfolioADMSpreadTotals = new List<LiveSpreadTotals>();
    }

    static class FCM_DataImportLibrary
    {
        internal static List<ADMPositionImportWeb> FCM_positionImportNotConsolidated;

        internal static List<ADMPositionImportWeb> FCM_Import_Consolidated;

        internal static List<ADMPositionImportWeb> FCM_PostionList_forCompare = new List<ADMPositionImportWeb>();
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
