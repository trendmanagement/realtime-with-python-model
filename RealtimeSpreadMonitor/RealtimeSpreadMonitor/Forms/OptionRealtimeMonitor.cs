using CQG;
using RealtimeSpreadMonitor.FormManipulation;
using RealtimeSpreadMonitor.Model;
using RealtimeSpreadMonitor.Mongo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace RealtimeSpreadMonitor.Forms
{
    public partial class OptionRealtimeMonitor : Form
    {


        private bool continueUpdating = true;

        private DateTime previousTime = DateTime.Now;

        private BackgroundWorker backgroundWorkerOptionRealtimeMonitor;

        delegate void ThreadSafeMoveSplitterDistance(int splitterDistance);

        delegate void ThreadSafeFillLiveDataGridViewPageDelegate(DataGridView gridView, int row, int col, String displayValue,
            bool updateColor, double value);

        delegate void ThreadSafeFillLiveDataPageDelegate(int row, int col, String displayValue,
            bool updateColor, double value);



        //delegate void ThreadSafeMarkAsConnectedDelegate(DataGridView gridView, int row, int col, bool connected, Color backgroundColor);

        delegate void ThreadSafeFillGridViewValueAsStringAndColorDelegate(int rowToUpdate, int col, string displayValue, Color color);

        //delegate void ThreadSafeUpdateStatusStripOptionMonitorDelegate();

        //delegate void ThreadSafeUpdateStatusSubscribeData(String subcriptionMessage);



        delegate void ThreadSafeUpdateInstrumentSummaryDelegate(int row, int col, double val);

        //delegate void ThreadSafeUpdateListViewDelegate(ListView listView, int row, int col, String val);

        //delegate void ThreadSafeUpdateBackColorListViewDelegate(ListView listView, int row, Color backColor);

        //delegate void ThreadSafeAddItemToListView(ListView listView, ListViewItem listViewItem);

        //delegate void ThreadSafeRemoveItemFromListView(ListView listView, int itemRow);

        delegate void ThreadSafeUpdateButtonText(ToolStripButton toolStripBtn, string buttonText);

        //delegate void ThreadSafeFillGridModelADMComparison();

        //delegate void ThreadSafeBeginEndUpdateList(bool begin);

        delegate void ThreadSafeGenericDelegateWithoutParams();

        //variables passed from OptionSpreadManager
        private OptionSpreadManager optionSpreadManager;

        //private OptionStrategy[] optionStrategies;
        private List<Instrument_mongo> instruments;

        private DataTable portfolioSummaryDataTable = new DataTable();
        private DataTable portfolioSummarySettlementDataTable = new DataTable();

        //private DataTable contractSummaryLiveDataTable = new DataTable();

        //private LiveSpreadTotals[] instrumentSpreadTotals;
        //private LiveSpreadTotals portfolioSpreadTotals;

        //private LiveSpreadTotals[,] instrumentADMSpreadTotals;
        //private LiveSpreadTotals[] portfolioADMSpreadTotals;

        private OptionArrayTypes optionArrayTypes;

        private List<MongoDB_OptionSpreadExpression> optionSpreadExpressionList = DataCollectionLibrary.optionSpreadExpressionList;

        public Settings settings;

        //private ChartForm orderChartForm = null;
        //private bool orderChartInstantiated = false;

        private FileLoadList fileLoadListForm = null;



        //private List<int> contractSummaryExpressionListIdx;  // = new List<int>();



        private bool hideUnhideInstrumentsInSummaryPL = true;

        internal DataGridView getGridViewContractSummary
        {
            get { return gridViewContractSummary; }
        }

        //private GridViewFCMPostionManipulation gridViewFCMPostionManipulation;

        //private ModelADMCompareCalculationAndDisplay modelADMCompareCalculationAndDisplay;

        internal DataGridView getGridLiveFCMData
        {
            get { return gridLiveFCMData; }
        }

        internal DataGridView getGridViewModelADMCompare
        {
            get { return gridViewModelADMCompare; }
        }

        public OptionRealtimeMonitor(
            OptionSpreadManager optionSpreadManager,
            //OptionStrategy[] optionStrategies,            
            OptionArrayTypes optionArrayTypes //List<int> contractSummaryExpressionListIdx,
                                              //List<ADMPositionImportWeb> admPositionImportWebListForCompare
            )
        {
            AsyncTaskListener.UpdatedStatus += AsyncTaskListener_UpdatedStatus;





            this.optionSpreadManager = optionSpreadManager;

            this.instruments = DataCollectionLibrary.instrumentList;

            this.optionArrayTypes = optionArrayTypes;

            InitializeComponent();


            this.Text =
                //"NEW VERSION PORTFOLIO: "                 
                DataCollectionLibrary.initializationParms.portfolioGroupName + "  Version:"
                + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();


            modelDateStatus.Text = DataCollectionLibrary.initializationParms.modelDateTime
                .ToString("yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo);



            initializeRealtimeMonitor();



            Type orderPlacementTypes = typeof(StageOrdersToTTWPFLibrary.Enums.ORDER_PLACEMENT_TYPE);
            Array orderPlacementTypesArray = Enum.GetNames(orderPlacementTypes);

            for (int i = 0; i < orderPlacementTypesArray.Length; i++)
            {
                cmbxOrderPlacementType.Items.Add(orderPlacementTypesArray.GetValue(i).ToString());
            }

            cmbxOrderPlacementType.SelectedIndex = 0;


            if (DataCollectionLibrary.supplementContractFilled)
            {
                toolStripStatusLabelUsingSupplementContract.Text = "SUP CON";
                toolStripStatusLabelUsingSupplementContract.ToolTipText = "SUPPLEMENT CONTRACTS FILE FILLED";
                toolStripStatusLabelUsingSupplementContract.BackColor = Color.Aqua;
            }
            else
            {
                toolStripStatusLabelUsingSupplementContract.Text = "";
            }


        }




        public void initializeRealtimeMonitor()
        {


            setupInstrumentSummary();

            setupPortfolioSummary();

            setupPortfolioSummarySettlements();

            setupOrderSummaryList();


            //gridViewFCMPostionManipulation.SetupFCMSummaryData(this);

            optionSpreadManager.modelADMCompareCalculationAndDisplay.setupGridModelADMComparison(this);

            setupExpressionListGridView();

            setupPreviousPLAnalysis(dataGridPreviousModelPriceCompare, dataGridPreviousModelPL, true);

            setupPreviousPLAnalysis(dataGridPreviousFCMPriceCompare, dataGridPreviousFCMPL, false);


        }




        public void updateStatusStripOptionMonitor()
        {

            if (DataCollectionLibrary.realtimeMonitorSettings.eodAnalysis)
            {
                AsyncTaskListener.StatusUpdateAsync("OPT THEO SETL",
                        STATUS_FORMAT.CAUTION, STATUS_TYPE.PRICE_TYPE);

                AsyncTaskListener.StatusUpdateAsync("EOD",
                        STATUS_FORMAT.CAUTION, STATUS_TYPE.EOD_SETTLEMENT);
            }
            else
            {
                string priceType = "";

                switch (DataCollectionLibrary.realtimeMonitorSettings.realtimePriceFillType)
                {
                    case REALTIME_PRICE_FILL_TYPE.PRICE_DEFAULT:
                        priceType = "PRC DEF";
                        break;

                    case REALTIME_PRICE_FILL_TYPE.PRICE_ASK:
                        priceType = "PRC ASK";
                        break;

                    case REALTIME_PRICE_FILL_TYPE.PRICE_MID_BID_ASK:
                        priceType = "PRC MID";
                        break;

                    case REALTIME_PRICE_FILL_TYPE.PRICE_BID:
                        priceType = "PRC BID";
                        break;

                    case REALTIME_PRICE_FILL_TYPE.PRICE_THEORETICAL:
                        priceType = "PRC THEOR";
                        break;
                }

                AsyncTaskListener.StatusUpdateAsync(priceType,
                        STATUS_FORMAT.DEFAULT, STATUS_TYPE.PRICE_TYPE);

                AsyncTaskListener.StatusUpdateAsync("",
                        STATUS_FORMAT.DEFAULT, STATUS_TYPE.EOD_SETTLEMENT);
            }
        }

        public void updateStatusDataFilling()
        {
            int subscribedCount = 0;

            int subCnt = 0;

            while (subCnt < optionSpreadExpressionList.Count)
            {
                if (optionSpreadExpressionList[subCnt].setSubscriptionLevel)
                {
                    subscribedCount++;
                }

                subCnt++;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(subscribedCount);
            sb.Append(" OF ");
            sb.Append(optionSpreadExpressionList.Count);

            if (subscribedCount < optionSpreadExpressionList.Count)
            {
                AsyncTaskListener.StatusUpdateAsync(sb.ToString(),
                    STATUS_FORMAT.CAUTION, STATUS_TYPE.DATA_FILLING_COUNT);
            }
            else
            {
                AsyncTaskListener.StatusUpdateAsync(sb.ToString(),
                    STATUS_FORMAT.DEFAULT, STATUS_TYPE.DATA_FILLING_COUNT);
            }


        }




        //used for running the reconnect to CQG timers
        private System.Threading.Timer timer1SetupTimeForCQGData;
        private System.Threading.Timer timer2SetupTimeForCQGData;

        public void realtimeMonitorStartupBackgroundUpdateLoop()
        {


            setUpTimerForCQGDataReset(TimerThreadInfo._dataResetTime1, 0);


            setUpTimerForCQGDataReset(TimerThreadInfo._dataResetTime2, 1);


            backgroundWorkerOptionRealtimeMonitor = new BackgroundWorker();
            backgroundWorkerOptionRealtimeMonitor.WorkerReportsProgress = true;
            backgroundWorkerOptionRealtimeMonitor.WorkerSupportsCancellation = true;

            backgroundWorkerOptionRealtimeMonitor.DoWork +=
                new DoWorkEventHandler(setupBackgroundWorkerOptionSummaryRealtime);

            backgroundWorkerOptionRealtimeMonitor.ProgressChanged +=
                new ProgressChangedEventHandler(
                    backgroundWorkerOptionSummaryRealtime_ProgressChanged);

            backgroundWorkerOptionRealtimeMonitor.RunWorkerAsync();

        }

        private void setUpTimerForCQGDataReset(TimeSpan alertTime, int timerToUse)
        {
            DateTime current = DateTime.Now;
            TimeSpan timeToGo = alertTime - current.TimeOfDay;
            if (timeToGo < TimeSpan.Zero)
            {
                return;//time already passed
            }


            System.Threading.TimerCallback timerDelegate = resetConnectionToCQGAtTimeInvoke;
            //new System.Threading.TimerCallback(resetConnectionToCQGAtTimeInvoke);

            System.Threading.Timer timer = new System.Threading.Timer(
                timerDelegate, null, timeToGo, Timeout.InfiniteTimeSpan);

            switch (timerToUse)
            {
                case 0:
                    timer1SetupTimeForCQGData = timer;
                    break;
                case 1:
                    timer2SetupTimeForCQGData = timer;
                    break;
            }


        }

        private void resetConnectionToCQGAtTimeInvoke(object StateObj)
        {
            //TSErrorCatch.debugWriteOut("test************************");

            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(resetConnectionToCQGAtTime));
            }
            else
            {
                resetConnectionToCQGAtTime();
            }
        }

        private void resetConnectionToCQGAtTime()
        {
            optionSpreadManager.fullReConnectCQG();
            Thread.Sleep(2000);
            optionSpreadManager.callOptionRealTimeData(false);
        }



        private bool updateOptionRealtimeMonitorGUI = true;

        public void setupBackgroundWorkerOptionSummaryRealtime(object sender,
            DoWorkEventArgs e)
        {

            ThreadTracker.openThread(null, null);

            while (continueUpdating)
            {
                System.Threading.Thread.Sleep(TradingSystemConstants.OPTIONREALTIMEREFRESH);

                if (updateOptionRealtimeMonitorGUI)
                {
                    updateOptionRealtimeMonitorGUI = false;

                    backgroundWorkerOptionRealtimeMonitor.ReportProgress(0);
                }





            }


            ThreadTracker.closeThread(null, null);

        }





        public void backgroundWorkerOptionSummaryRealtime_ProgressChanged(object sender,
            ProgressChangedEventArgs e)
        {

            if (continueUpdating)
            {
                DateTime current = DateTime.Now;
                TimeSpan timeToGo = TimerThreadInfo.refreshMongoOrders - DateTime.Now.TimeOfDay;
                if (timeToGo < TimeSpan.Zero)
                {
                    optionSpreadManager.RefreshAccountInfo();

                    TimerThreadInfo.refreshMongoOrders = DateTime.Now.AddSeconds(30).TimeOfDay;
                }

                if (DataCollectionLibrary._fxceConnected)
                {
                    AsyncTaskListener.StatusUpdateAsync("TTFIX UP",
                        STATUS_FORMAT.DEFAULT, STATUS_TYPE.TT_FIX_CONNECTION);
                }
                else
                {
                    AsyncTaskListener.StatusUpdateAsync("TTFIX DN",
                        STATUS_FORMAT.ALARM, STATUS_TYPE.TT_FIX_CONNECTION);
                }

                optionSpreadManager.RunSpreadTotalCalculations();

                optionSpreadManager.RunADMSpreadTotalCalculations();




                //*******************
                //update without new thread datatable
                sendUpdateToPortfolioTotalGrid();


                sendUpdateToPortfolioTotalSettlementGrid();


                fillOrderSummaryList();
                //*******************

                ContractsModel_Library.gridViewContractSummaryManipulation.FillContractSummary();

                ContractsModel_Library.gridViewFCMPostionManipulation.Fill_FCM_ContractSummary();


                updateLiveDataPage(null);
                //Thread callUpdateLivePage = new Thread(new ParameterizedThreadStart(updateLiveDataPage));
                //callUpdateLivePage.IsBackground = true;
                //callUpdateLivePage.Start();

                //updateLiveDataPage(null);

                //DateTime ct = DateTime.Now;
                //TSErrorCatch.debugWriteOut(ct.ToString("HH:mm:ss", DateTimeFormatInfo.InvariantInfo));

                updateOptionRealtimeMonitorGUI = true;


                //Thread.Sleep(7000);
                //updateLiveDataPage(null);

            }

        }

        public void updateLiveDataPage(Object obj)
        {
            this.Invoke(new EventHandler(ThreadTracker.openThread));

            //if (setupLiveGrid)
            {








                optionSpreadManager.modelADMCompareCalculationAndDisplay.fillGridModelADMComparison(this);

                updateStatusDataFilling();

                sendUpdateToExpressionListGrid();
            }


            this.Invoke(new EventHandler(ThreadTracker.closeThread));
        }

        public void cancelBackgroundWorker()
        {
            backgroundWorkerOptionRealtimeMonitor.CancelAsync();

            continueUpdating = false;

        }


        public void setupTreeViewInstruments()
        {
            for (int instrumentCnt = 0; instrumentCnt <= instruments.Count(); instrumentCnt++)
            {
                //instrumentRollIntoSummary[i] = new InstrumentRollIntoSummary();

                if (instrumentCnt == instruments.Count())
                {


                    treeViewInstruments.Nodes.Add(instrumentCnt.ToString(), "ALL INSTRUMENTS");

                    treeViewInstruments.SelectedNode = treeViewInstruments.Nodes[instrumentCnt];

                    //treeViewInstruments.SelectedNode.BackColor = Color.Yellow;
                }
                else
                {
                    treeViewInstruments.Nodes.Add(instrumentCnt.ToString(), instruments[instrumentCnt].cqgsymbol);
                }
            }
        }

        public void setupTreeViewBrokerAcct()
        {
            DataCollectionLibrary.portfolioAllocation.brokerAccountChosen = DataCollectionLibrary.portfolioAllocation.accountAllocation.Count;

            for (int groupAllocCnt = 0; groupAllocCnt <= DataCollectionLibrary.portfolioAllocation.accountAllocation.Count; groupAllocCnt++)
            {

                if (groupAllocCnt == DataCollectionLibrary.portfolioAllocation.accountAllocation.Count)
                {


                    treeViewBrokerAcct.Nodes.Add(groupAllocCnt.ToString(), "ALL ACCTS");

                    treeViewBrokerAcct.SelectedNode = treeViewBrokerAcct.Nodes[groupAllocCnt];
                }
                else
                {
                    StringBuilder treeVal = new StringBuilder();


                    treeVal.Append(DataCollectionLibrary.portfolioAllocation.accountAllocation[groupAllocCnt].accountFromMongo.client_name);
                    treeVal.Append("|");
                    treeVal.Append(DataCollectionLibrary.portfolioAllocation.accountAllocation[groupAllocCnt].broker);
                    treeVal.Append("|");
                    treeVal.Append(DataCollectionLibrary.portfolioAllocation.accountAllocation[groupAllocCnt].account);
                    treeVal.Append("|");
                    treeVal.Append(DataCollectionLibrary.portfolioAllocation.accountAllocation[groupAllocCnt].accountFromMongo.campaign_name);
                    treeVal.Append("|");
                    treeVal.Append(DataCollectionLibrary.portfolioAllocation.accountAllocation[groupAllocCnt].FCM_OFFICE);
                    treeVal.Append(DataCollectionLibrary.portfolioAllocation.accountAllocation[groupAllocCnt].FCM_ACCT);

                    treeViewBrokerAcct.Nodes.Add(groupAllocCnt.ToString(), treeVal.ToString());
                }
            }
        }




        public void setupExpressionListGridView()
        {
            try
            {

                Type liveColTypes = typeof(EXPRESSION_LIST_VIEW);
                Array liveColTypesArray = Enum.GetNames(liveColTypes);

                dataGridViewExpressionList.ColumnCount = liveColTypesArray.Length;

                dataGridViewExpressionList.EnableHeadersVisualStyles = false;

                DataGridViewCellStyle colTotalPortStyle = dataGridViewExpressionList.ColumnHeadersDefaultCellStyle;
                colTotalPortStyle.BackColor = Color.Black;
                colTotalPortStyle.ForeColor = Color.White;

                DataGridViewCellStyle rowTotalPortStyle = dataGridViewExpressionList.RowHeadersDefaultCellStyle;
                rowTotalPortStyle.BackColor = Color.Black;
                rowTotalPortStyle.ForeColor = Color.White;

                dataGridViewExpressionList.Columns[0].Frozen = true;

                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < dataGridViewExpressionList.ColumnCount; i++)
                {
                    dataGridViewExpressionList.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;

                    if (i == (int)EXPRESSION_LIST_VIEW.MANUAL_STTLE)
                    {
                        DataGridViewCheckBoxColumn manualSettleCol = new DataGridViewCheckBoxColumn();
                        {
                            manualSettleCol.HeaderText = liveColTypesArray.GetValue(
                                (int)ADM_MODEL_POSITION_COMPARE_FIELDS_DISPLAYED.ZERO_PRICE).ToString().Replace('_', ' ');
                            //column.Name = ColumnName.OutOfOffice.ToString();
                            manualSettleCol.AutoSizeMode =
                                DataGridViewAutoSizeColumnMode.DisplayedCells;
                            manualSettleCol.FlatStyle = FlatStyle.Standard;
                            //column.ThreeState = true;
                            manualSettleCol.CellTemplate = new DataGridViewCheckBoxCell();
                            manualSettleCol.CellTemplate.Style.BackColor = Color.LightBlue;
                            manualSettleCol.ReadOnly = false;
                            //column.e = false;
                        }

                        dataGridViewExpressionList.Columns.RemoveAt(i);
                        dataGridViewExpressionList.Columns.Insert(i, manualSettleCol);

                        //dataGridViewExpressionList.Columns.Insert(i, myChkBox);

                        //dataGridViewExpressionList.Columns[i].ReadOnly = false;
                    }
                    else
                    {
                        dataGridViewExpressionList.Columns[i].ReadOnly = true;
                    }



                    sb.Clear();

                    sb.Append(liveColTypesArray.GetValue(i).ToString());

                    dataGridViewExpressionList
                        .Columns[i]
                        .HeaderCell.Value = sb.ToString().Replace('_', ' ');

                    dataGridViewExpressionList.Columns[i].Width = 50;
                }



                //for (int i = 0; i < liveColTypesArray.Length; i++)
                //{

                //}

                //************
                //dataGridViewExpressionList.Columns[(int)OPTION_LIVE_ADM_DATA_COLUMNS.CONTRACT].Width = 115;

                //dataGridViewExpressionList.Columns[(int)OPTION_LIVE_ADM_DATA_COLUMNS.LEG].Width = 30;
                //dataGridViewExpressionList.Columns[(int)OPTION_LIVE_ADM_DATA_COLUMNS.LEG].DefaultCellStyle.Font = new Font("Tahoma", 7);

                dataGridViewExpressionList.Columns[(int)EXPRESSION_LIST_VIEW.TIME].Width = 70;
                dataGridViewExpressionList.Columns[(int)EXPRESSION_LIST_VIEW.TIME].DefaultCellStyle.Font = new Font("Tahoma", 6);
                dataGridViewExpressionList.Columns[(int)EXPRESSION_LIST_VIEW.TIME].DefaultCellStyle.WrapMode = DataGridViewTriState.True;


                dataGridViewExpressionList.Columns[(int)EXPRESSION_LIST_VIEW.SETL_TIME].DefaultCellStyle.Font = new Font("Tahoma", 6);
                dataGridViewExpressionList.Columns[(int)EXPRESSION_LIST_VIEW.SETL_TIME].DefaultCellStyle.WrapMode = DataGridViewTriState.True;

                //dataGridViewExpressionList.Columns[(int)EXPRESSION_LIST_VIEW.EXPR].DefaultCellStyle.Font = new Font("Tahoma", 6);
                //dataGridViewExpressionList.Columns[(int)EXPRESSION_LIST_VIEW.EXPR].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                //************

                //List<LiveADMStrategyInfo> liveADMStrategyInfoList = optionSpreadManager.liveADMStrategyInfoList;

                //updateSetupExpressionListGridView();

            }
            catch (Exception ex)
            {
                TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
            }
        }

        public void updateSetupExpressionListGridView()
        {
            try
            {

                //dataGridViewExpressionList.RowCount = optionSpreadExpressionList.Count - 1;
                updateExpressionGridRowCount();

                int rowIdx = 0;


                Color rowColor1 = Color.DarkGray;
                Color rowColor2 = Color.DarkBlue;

                Color currentRowColor = rowColor1;

                int optionSpreadExpressionIdx = 0;

                //for (int instrumentCnt = 0; instrumentCnt <= instruments.Count(); instrumentCnt++)
                //if(false)
                {
                    //int instrumentCnt = 0;
                    optionSpreadExpressionIdx = 0;

                    foreach (MongoDB_OptionSpreadExpression ose in optionSpreadExpressionList)
                    {
                        if (ose.optionExpressionType != OPTION_EXPRESSION_TYPES.OPTION_EXPRESSION_RISK_FREE_RATE)
                        //&& ose.instrument.idinstrument == instruments[instrumentCnt].idinstrument)
                        {
                            ose.dataGridExpressionListRow = rowIdx;

                            switch (rowIdx % 2)
                            {
                                case 0:
                                    currentRowColor = rowColor1;
                                    break;

                                case 1:
                                    currentRowColor = rowColor2;
                                    break;

                            }

                            //FIX THIS THREAD SAFE
                            //dataGridViewExpressionList
                            //        .Rows[rowIdx]
                            //            .HeaderCell.Style.BackColor = currentRowColor;

                            //dataGridViewExpressionList
                            //        .Rows[rowIdx]
                            //        .HeaderCell.Value =
                            //            ose.cqgsymbol;

                            fillExpressionListGridHeaders(rowIdx, -1, ose.asset.cqgsymbol, currentRowColor);

                            fillDataGridViewExpressionListPage(rowIdx,
                                    (int)EXPRESSION_LIST_VIEW.INSTRUMENT_ID,
                                   ose.instrument.idinstrument.ToString(), true, ose.instrument.idinstrument);

                            fillDataGridViewExpressionListPage(rowIdx,
                                    (int)EXPRESSION_LIST_VIEW.EXPRESSION_IDX,
                                   optionSpreadExpressionIdx.ToString(), true, optionSpreadExpressionIdx);

                            //dataGridViewExpressionList.Rows[rowIdx].Cells[(int)EXPRESSION_LIST_VIEW.INSTRUMENT_ID].Value = instrumentCnt;


                            rowIdx++;

                        }

                        optionSpreadExpressionIdx++;
                    }
                }

                //updateColorOfADMStrategyGrid();

            }
            catch (Exception ex)
            {
                TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
            }

            //return liveLegRowIdexes;
        }

        private void updateExpressionGridRowCount()
        {
            if (this.InvokeRequired)
            {
                ThreadSafeGenericDelegateWithoutParams d =
                    new ThreadSafeGenericDelegateWithoutParams(threadSafeUpdateExpressionGridRowCount);

                this.Invoke(d);
            }
            else
            {
                threadSafeUpdateExpressionGridRowCount();
            }
        }

        private void threadSafeUpdateExpressionGridRowCount()
        {
            dataGridViewExpressionList.RowCount = optionSpreadExpressionList.Count - 1;
        }

        public void fillExpressionListGridHeaders(int rowToUpdate, int col, string displayValue, Color color)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    //ThreadSafeFillGridViewValueAsStringAndColorDelegate(int rowToUpdate, int col, string displayValue, Color color);

                    ThreadSafeFillGridViewValueAsStringAndColorDelegate d =
                        new ThreadSafeFillGridViewValueAsStringAndColorDelegate(threadSafeFillExpressionListGridHeaders);

                    this.Invoke(d, rowToUpdate, col, displayValue, color);
                }
                else
                {
                    threadSafeFillExpressionListGridHeaders(rowToUpdate, col, displayValue, color);
                }
            }
            catch (Exception ex)
            {
                TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
            }
        }

        public void threadSafeFillExpressionListGridHeaders(int rowToUpdate, int col, string displayValue, Color color)
        {
            try
            {
                //int rowToUpdate = row;

                //if (
                //    (
                //    dataGridViewExpressionList.Rows[rowToUpdate].Cells[col].Value == null
                //    ||
                //    dataGridViewExpressionList.Rows[rowToUpdate].Cells[col].Value.ToString().CompareTo(displayValue) != 0
                //    ))
                {
                    dataGridViewExpressionList
                                    .Rows[rowToUpdate]
                                        .HeaderCell.Style.BackColor = color;

                    dataGridViewExpressionList
                            .Rows[rowToUpdate]
                            .HeaderCell.Value = displayValue;
                }

            }
            catch (Exception ex)
            {
                TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
            }
        }




        /// <summary>
        /// This sets up the sums of margin, r-risk etc for each account
        /// </summary>
        public void setupInstrumentSummary()
        {
#if DEBUG
            try
#endif
            {

                Type instrumentSummaryGridRowTypes = typeof(INSTRUMENT_SUMMARY_GRID_ROWS);
                Array instrumentSummaryGridRowTypesArray = Enum.GetNames(instrumentSummaryGridRowTypes);

                //ADD EXTRA COLUMN FOR TOTAL
                instrumentSummaryGrid.ColumnCount = instruments.Count() + 1;
                instrumentSummaryGrid.RowCount = instrumentSummaryGridRowTypesArray.Length;



                for (int i = 0; i < instrumentSummaryGrid.ColumnCount; i++)
                {
                    instrumentSummaryGrid.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;

                    instrumentSummaryGrid.Columns[i].Width = 70;
                }

                instrumentSummaryGrid.Columns[instruments.Count()].HeaderText = "TOTAL";


                for (int i = 0; i < instrumentSummaryGridRowTypesArray.Length; i++)
                {
                    instrumentSummaryGrid.Rows[i].HeaderCell.Value = instrumentSummaryGridRowTypesArray.GetValue(i).ToString();
                }

                double marginSum = 0;
                //double rRiskSum = 0;
                //double oneR = 0;
                //double rStatus = 0;
                //double lockedInR = 0;

                foreach (AccountPosition ap in DataCollectionLibrary.accountPositionsList)
                {
                    foreach (Position p in ap.positions)
                    {
                        Instrument_mongo im = DataCollectionLibrary.instrumentHashTable_keyinstrumentid[p.asset.idinstrument];

                        im.instrument_summary_values.margin_summary += p.qty * im.margin;
                    }
                }

                for (int i = 0; i <= instruments.Count(); i++)
                {
                    if (i < instruments.Count())
                    {
                        instrumentSummaryGrid.Columns[i].HeaderText = instruments[i].cqgsymbol
                            + " - " + instruments[i].exchangesymbol;

                        marginSum += DataCollectionLibrary.instrumentList[i].instrument_summary_values.margin_summary;


                        instrumentSummaryGrid.Rows[(int)INSTRUMENT_SUMMARY_GRID_ROWS.MARGIN_SUMMARY].Cells[i].Value =
                            DataCollectionLibrary.instrumentList[i].instrument_summary_values.margin_summary;

                    }
                    else
                    {
                        instrumentSummaryGrid.Rows[(int)INSTRUMENT_SUMMARY_GRID_ROWS.MARGIN_SUMMARY].Cells[i].Value = marginSum;
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

        public void setupPortfolioSummary()
        {
#if DEBUG
            try
#endif
            {

                Type portfolioSummaryGridRowTypes = typeof(PORTFOLIO_SUMMARY_GRID_ROWS);
                Array portfolioSummaryGridRowTypesArray = Enum.GetNames(portfolioSummaryGridRowTypes);

                for (int i = 0; i < instruments.Count(); i++)
                {
                    portfolioSummaryDataTable.Columns.Add(instruments[i].cqgsymbol
                        + " - " + instruments[i].exchangesymbol);
                }

                //ADD EXTRA COLUMN FOR TOTAL
                portfolioSummaryDataTable.Columns.Add("TOTAL");

                for (int i = 0; i < portfolioSummaryGridRowTypesArray.Length; i++)
                {
                    portfolioSummaryDataTable.Rows.Add();
                }

                //this must be at the end after all the columns and rows are added
                portfolioSummaryGrid.DataSource = portfolioSummaryDataTable;

            }
#if DEBUG
            catch (Exception ex)
            {
                TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
            }
#endif
        }


        private void portfolioSummaryGrid_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            //http://stackoverflow.com/questions/13231149/datagridview-not-updating-refreshing

            for (int i = 0; i < portfolioSummaryGrid.ColumnCount; i++)
            {
                portfolioSummaryGrid.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;

                portfolioSummaryGrid.Columns[i].Width = 70;
            }

            Type portfolioSummaryGridRowTypes = typeof(PORTFOLIO_SUMMARY_GRID_ROWS);
            Array portfolioSummaryGridRowTypesArray = Enum.GetNames(portfolioSummaryGridRowTypes);

            for (int i = 0; i < portfolioSummaryGridRowTypesArray.Length; i++)
            {
                portfolioSummaryGrid.Rows[i].HeaderCell.Value = portfolioSummaryGridRowTypesArray.GetValue(i).ToString();
            }
        }



        public void setupPortfolioSummarySettlements()
        {
#if DEBUG
            try
#endif
            {
                Type portfolioSummaryGridRowTypes = typeof(PORTFOLIO_SETTLEMENT_SUMMARY_GRID_ROWS);
                Array portfolioSummaryGridRowTypesArray = Enum.GetNames(portfolioSummaryGridRowTypes);

                for (int i = 0; i < instruments.Count(); i++)
                {
                    portfolioSummarySettlementDataTable.Columns.Add(instruments[i].cqgsymbol
                        + " - " + instruments[i].exchangesymbol);

                    //portfolioSummaryGridSettlements.Columns[i].HeaderText = instruments[i].cqgsymbol
                    //    + " - " + instruments[i].exchangeSymbol;

                }

                portfolioSummarySettlementDataTable.Columns.Add("TOTAL");




                for (int i = 0; i < portfolioSummaryGridRowTypesArray.Length; i++)
                {
                    portfolioSummarySettlementDataTable.Rows.Add();
                }

                //this must be at the end after all the columns and rows are added
                portfolioSummaryGridSettlements.DataSource = portfolioSummarySettlementDataTable;

                //for (int i = 0; i < portfolioSummaryGridSettlements.ColumnCount; i++)
                //{
                //    portfolioSummaryGridSettlements.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;

                //    portfolioSummaryGridSettlements.Columns[i].Width = 70;
                //}

                //portfolioSummaryGridSettlements.Columns[instruments.Count()].HeaderText = "TOTAL";


                //for (int i = 0; i < portfolioSummaryGridRowTypesArray.Length; i++)
                //{
                //    portfolioSummaryGridSettlements.Rows[i].HeaderCell.Value = portfolioSummaryGridRowTypesArray.GetValue(i).ToString();
                //}

            }
#if DEBUG
            catch (Exception ex)
            {
                TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
            }
#endif
        }

        private void portfolioSummaryGridSettlements_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            for (int i = 0; i < portfolioSummaryGridSettlements.ColumnCount; i++)
            {
                portfolioSummaryGridSettlements.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;

                portfolioSummaryGridSettlements.Columns[i].Width = 70;
            }

            Type portfolioSummaryGridRowTypes = typeof(PORTFOLIO_SETTLEMENT_SUMMARY_GRID_ROWS);
            Array portfolioSummaryGridRowTypesArray = Enum.GetNames(portfolioSummaryGridRowTypes);

            for (int i = 0; i < portfolioSummaryGridRowTypesArray.Length; i++)
            {
                portfolioSummaryGridSettlements.Rows[i].HeaderCell.Value = portfolioSummaryGridRowTypesArray.GetValue(i).ToString().Replace('_', ' ');
                //portfolioSummaryGridSettlements..HeaderCell..Width = 100;
            }
        }

        public void setupPreviousPLAnalysis(DataGridView dataGridViewPriceCompare, DataGridView dataGridViewPL,
             bool model)
        {
#if DEBUG
            try
#endif
            {
                Type previousPriceGridRowTypes = typeof(PREVIOUS_PRICE_COMPARE_ANALYSIS);
                Array previousPriceGridRowTypesArray = Enum.GetNames(previousPriceGridRowTypes);

                dataGridViewPriceCompare.ColumnCount = previousPriceGridRowTypesArray.Length;

                dataGridViewPriceCompare.EnableHeadersVisualStyles = false;

                DataGridViewCellStyle colTotalPortStyle = dataGridViewPriceCompare.ColumnHeadersDefaultCellStyle;
                colTotalPortStyle.BackColor = Color.Black;
                colTotalPortStyle.ForeColor = Color.White;

                DataGridViewCellStyle rowTotalPortStyle = dataGridViewPriceCompare.RowHeadersDefaultCellStyle;
                rowTotalPortStyle.BackColor = Color.Black;
                rowTotalPortStyle.ForeColor = Color.White;


                dataGridViewPriceCompare.Columns[0].Frozen = true;

                for (int i = 0; i < dataGridViewPriceCompare.ColumnCount; i++)
                {
                    dataGridViewPriceCompare.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                }

                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < previousPriceGridRowTypesArray.Length; i++)
                {
                    sb.Clear();

                    sb.Append(previousPriceGridRowTypesArray.GetValue(i).ToString());

                    dataGridViewPriceCompare
                        .Columns[i]
                        .HeaderCell.Value = sb.ToString().Replace('_', ' ');

                    dataGridViewPriceCompare.Columns[i].Width = 60;
                }

                if (model)
                {
                    dataGridViewPriceCompare.TopLeftHeaderCell.Value = "MODEL";
                }
                else
                {
                    dataGridViewPriceCompare.TopLeftHeaderCell.Value = "FCM";
                }

                dataGridViewPriceCompare.RowCount = 0;






                Type previousPLGridRowTypes = typeof(PREVIOUS_PL_COMPARE_ANALYSIS);
                Array previousPLGridRowTypesArray = Enum.GetNames(previousPLGridRowTypes);

                dataGridViewPL.ColumnCount = previousPLGridRowTypesArray.Length;

                dataGridViewPL.EnableHeadersVisualStyles = false;

                colTotalPortStyle = dataGridViewPL.ColumnHeadersDefaultCellStyle;
                colTotalPortStyle.BackColor = Color.Black;
                colTotalPortStyle.ForeColor = Color.White;

                rowTotalPortStyle = dataGridViewPL.RowHeadersDefaultCellStyle;
                rowTotalPortStyle.BackColor = Color.Black;
                rowTotalPortStyle.ForeColor = Color.White;


                dataGridViewPL.Columns[0].Frozen = true;

                for (int i = 0; i < dataGridViewPL.ColumnCount; i++)
                {
                    dataGridViewPL.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                }

                //StringBuilder sb = new StringBuilder();

                for (int i = 0; i < previousPLGridRowTypesArray.Length; i++)
                {
                    sb.Clear();

                    sb.Append(previousPLGridRowTypesArray.GetValue(i).ToString());

                    dataGridViewPL
                        .Columns[i]
                        .HeaderCell.Value = sb.ToString().Replace('_', ' ');

                    dataGridViewPL.Columns[i].Width = 80;
                }

                if (model)
                {
                    dataGridViewPL.TopLeftHeaderCell.Value = "MODEL";
                }
                else
                {
                    dataGridViewPL.TopLeftHeaderCell.Value = "FCM";
                }

                dataGridViewPL.RowCount = instruments.Count() + 1;

            }
#if DEBUG
            catch (Exception ex)
            {
                TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
            }
#endif
        }




        public void setupOrderSummaryList()
        {
            orderSummaryGrid.DataSource = DataCollectionLibrary.orderSummaryDataTable;

            DataCollectionLibrary.orderSummaryDataTable.Columns.Add(ORDER_SUMMARY_COLUMNS.RFRSH_TIME.ToString());
            DataCollectionLibrary.orderSummaryDataTable.Columns.Add(ORDER_SUMMARY_COLUMNS.INST.ToString());
            DataCollectionLibrary.orderSummaryDataTable.Columns.Add(ORDER_SUMMARY_COLUMNS.CONTRACT.ToString());
            DataCollectionLibrary.orderSummaryDataTable.Columns.Add(ORDER_SUMMARY_COLUMNS.QTY.ToString());
            DataCollectionLibrary.orderSummaryDataTable.Columns.Add(ORDER_SUMMARY_COLUMNS.DECS_T.ToString());
            DataCollectionLibrary.orderSummaryDataTable.Columns.Add(ORDER_SUMMARY_COLUMNS.TRANS_T.ToString());
            DataCollectionLibrary.orderSummaryDataTable.Columns.Add(ORDER_SUMMARY_COLUMNS.DECS_P.ToString());
            DataCollectionLibrary.orderSummaryDataTable.Columns.Add(ORDER_SUMMARY_COLUMNS.TRANS_P.ToString());
            DataCollectionLibrary.orderSummaryDataTable.Columns.Add(ORDER_SUMMARY_COLUMNS.DECS_FILL.ToString());
            DataCollectionLibrary.orderSummaryDataTable.Columns.Add(ORDER_SUMMARY_COLUMNS.INSID.ToString());
            DataCollectionLibrary.orderSummaryDataTable.Columns.Add(ORDER_SUMMARY_COLUMNS.ACCT.ToString());
            DataCollectionLibrary.orderSummaryDataTable.Columns.Add(ORDER_SUMMARY_COLUMNS.FCM_OFFICE.ToString());
            DataCollectionLibrary.orderSummaryDataTable.Columns.Add(ORDER_SUMMARY_COLUMNS.FCM_ACCT.ToString());



            //DataCollectionLibrary.orderSummaryDataTable.Columns.Add(ORDER_SUMMARY_COLUMNS.INST.ToString());
            //DataCollectionLibrary.orderSummaryDataTable.Columns.Add("Contract");
            //DataCollectionLibrary.orderSummaryDataTable.Columns.Add("#");
            //DataCollectionLibrary.orderSummaryDataTable.Columns.Add("Decs T");
            //DataCollectionLibrary.orderSummaryDataTable.Columns.Add("Tran T");
            //DataCollectionLibrary.orderSummaryDataTable.Columns.Add("Decs P");
            //DataCollectionLibrary.orderSummaryDataTable.Columns.Add("Tran P");
            //DataCollectionLibrary.orderSummaryDataTable.Columns.Add("Decs Filled");
            //DataCollectionLibrary.orderSummaryDataTable.Columns.Add("InsId");
            //DataCollectionLibrary.orderSummaryDataTable.Columns.Add("Acct");
            //DataCollectionLibrary.orderSummaryDataTable.Columns.Add("OFFICE");
            //DataCollectionLibrary.orderSummaryDataTable.Columns.Add("FCM_ACCOUNT");


        }



        public void fillOrderSummaryList()
        {

            //foreach(OrderSummary_AccountPosition os_ap in DataCollectionLibrary.orderSummaryList)
            //{
            //    os_ap.tested = false;
            //}

            DataCollectionLibrary.orderSummaryDataTable.Clear(); // .Rows.Clear();

            //if (test)
            {

                foreach (AccountPosition ap in DataCollectionLibrary.accountPositionsList)
                {
                    foreach (Position p in ap.positions)
                    {
                        //TODO ENABLE THIS FOR PROPER ORDER QUANTITY
                        if (p.qty != p.prev_qty)
                        {
                            if (p.asset.idinstrument == DataCollectionLibrary.instrumentSelectedInTreeGui
                                || DataCollectionLibrary.instrumentSelectedInTreeGui == TradingSystemConstants.ALL_INSTRUMENTS_SELECTED)
                            {
                                AccountAllocation ac =
                                    DataCollectionLibrary.portfolioAllocation.accountAllocation_KeyAccountname[ap.name];

                                if (ac.visible)
                                {
                                    enterExpressionsIntoOrderSummary(
                                        ap, p);
                                }
                            }
                        }
                    }
                }
            }

        }



        private void enterExpressionsIntoOrderSummary(AccountPosition ap, Position p)
        {

            DataTable dataTable = DataCollectionLibrary.orderSummaryDataTable;



            Instrument_mongo im = DataCollectionLibrary.instrumentHashTable_keyinstrumentid[p.asset.idinstrument];

            DateTime currentTime = DateTime.Now;
            DateTime decisionTime = new DateTime(currentTime.Year,
                currentTime.Month, currentTime.Day, 
                im.customdayboundarytime.AddMinutes(-im.decisionoffsetminutes).Hour,
                im.customdayboundarytime.AddMinutes(-im.decisionoffsetminutes).Minute,0);

            dataTable.Rows.Add();
            dataTable.Rows[dataTable.Rows.Count - 1][(int)ORDER_SUMMARY_COLUMNS.RFRSH_TIME]
                = ap.date_now.ToString("MM-dd HH:mm", DateTimeFormatInfo.InvariantInfo); ;
            dataTable.Rows[dataTable.Rows.Count - 1][(int)ORDER_SUMMARY_COLUMNS.INST] = im.cqgsymbol;
            dataTable.Rows[dataTable.Rows.Count - 1][(int)ORDER_SUMMARY_COLUMNS.CONTRACT] = p.asset.cqgsymbol;
            dataTable.Rows[dataTable.Rows.Count - 1][(int)ORDER_SUMMARY_COLUMNS.QTY] = p.qty - p.prev_qty;

            dataTable.Rows[dataTable.Rows.Count - 1][(int)ORDER_SUMMARY_COLUMNS.DECS_T] =
                decisionTime.ToString("HH:mm", DateTimeFormatInfo.InvariantInfo);
            dataTable.Rows[dataTable.Rows.Count - 1][(int)ORDER_SUMMARY_COLUMNS.TRANS_T] =
                im.customdayboundarytime.ToString("HH:mm", DateTimeFormatInfo.InvariantInfo);

            if (p.mose.decisionPriceFilled)
            {
                if (p.mose.cqgInstrument != null)
                {
                    dataTable.Rows[dataTable.Rows.Count - 1][(int)ORDER_SUMMARY_COLUMNS.DECS_P] =
                        p.mose.cqgInstrument.ToDisplayPrice(p.mose.decisionPrice);
                }
            }
            else
            {
                dataTable.Rows[dataTable.Rows.Count - 1][(int)ORDER_SUMMARY_COLUMNS.DECS_P] = " ";
            }

            if (p.mose.transactionPriceFilled)
            {
                if (p.mose.cqgInstrument != null)
                {
                    dataTable.Rows[dataTable.Rows.Count - 1][(int)ORDER_SUMMARY_COLUMNS.TRANS_P] =
                    p.mose.cqgInstrument.ToDisplayPrice(p.mose.transactionPrice);
                }
            }
            else
            {
                dataTable.Rows[dataTable.Rows.Count - 1][(int)ORDER_SUMMARY_COLUMNS.TRANS_P] = " ";
            }

            dataTable.Rows[dataTable.Rows.Count - 1][(int)ORDER_SUMMARY_COLUMNS.DECS_FILL] =
                ap.date_now.CompareTo(decisionTime) >= 0;
            //p.mose.reachedBarAfterDecisionBar;

            dataTable.Rows[dataTable.Rows.Count - 1][(int)ORDER_SUMMARY_COLUMNS.INSID] = im.idinstrument;

            dataTable.Rows[dataTable.Rows.Count - 1][(int)ORDER_SUMMARY_COLUMNS.ACCT] = ap.name;

            AccountAllocation ac =
                        DataCollectionLibrary.portfolioAllocation.accountAllocation_KeyAccountname[ap.name];

            dataTable.Rows[dataTable.Rows.Count - 1][(int)ORDER_SUMMARY_COLUMNS.FCM_OFFICE] = ac.FCM_OFFICE;

            dataTable.Rows[dataTable.Rows.Count - 1][(int)ORDER_SUMMARY_COLUMNS.FCM_ACCT] = ac.FCM_ACCT;


        }


        public void sendUpdateToPortfolioTotalGrid()
        {

            int instrumentCnt = 0;

            foreach (Instrument_mongo im in DataCollectionLibrary.instrumentList)
            {
                UpdateCell((int)PORTFOLIO_SUMMARY_GRID_ROWS.PL_CHG, instrumentCnt,
                                portfolioSummaryDataTable, Math.Round(im.instrumentModelCalcTotals_ByAccount[
                            DataCollectionLibrary.portfolioAllocation.brokerAccountChosen].pAndLDay, 2).ToString());
                //portfolioSummaryDataTable.Rows[(int)PORTFOLIO_SUMMARY_GRID_ROWS.PL_CHG][instrumentCnt]
                //    = Math.Round(im.instrumentModelCalcTotals_ByAccount[
                //            DataCollectionLibrary.portfolioAllocation.brokerAccountChosen].pAndLDay, 2);

                UpdateCell((int)PORTFOLIO_SUMMARY_GRID_ROWS.TOTAL_DELTA, instrumentCnt,
                                portfolioSummaryDataTable, Math.Round(im.instrumentModelCalcTotals_ByAccount[
                            DataCollectionLibrary.portfolioAllocation.brokerAccountChosen].delta, 2).ToString());
                //portfolioSummaryDataTable.Rows[(int)PORTFOLIO_SUMMARY_GRID_ROWS.TOTAL_DELTA][instrumentCnt]
                //    = Math.Round(im.instrumentModelCalcTotals_ByAccount[
                //            DataCollectionLibrary.portfolioAllocation.brokerAccountChosen].delta, 2);

                UpdateCell((int)PORTFOLIO_SUMMARY_GRID_ROWS.FCM_PL_CHG, instrumentCnt,
                                portfolioSummaryDataTable, Math.Round(im.instrumentADMCalcTotalsByAccount[
                            DataCollectionLibrary.portfolioAllocation.brokerAccountChosen].pAndLDay, 2).ToString());
                //portfolioSummaryDataTable.Rows[(int)PORTFOLIO_SUMMARY_GRID_ROWS.FCM_PL_CHG][instrumentCnt]
                //    = Math.Round(im.instrumentADMCalcTotalsByAccount[
                //            DataCollectionLibrary.portfolioAllocation.brokerAccountChosen].pAndLDay, 2);

                UpdateCell((int)PORTFOLIO_SUMMARY_GRID_ROWS.FCM_TOTAL_DELTA, instrumentCnt,
                                portfolioSummaryDataTable, Math.Round(im.instrumentADMCalcTotalsByAccount[
                            DataCollectionLibrary.portfolioAllocation.brokerAccountChosen].delta, 2).ToString());
                //portfolioSummaryDataTable.Rows[(int)PORTFOLIO_SUMMARY_GRID_ROWS.FCM_TOTAL_DELTA][instrumentCnt]
                //    = Math.Round(im.instrumentADMCalcTotalsByAccount[
                //        DataCollectionLibrary.portfolioAllocation.brokerAccountChosen].delta, 2);

                instrumentCnt++;


            }

            UpdateCell((int)PORTFOLIO_SUMMARY_GRID_ROWS.PL_CHG, DataCollectionLibrary.instrumentList.Count,
                    portfolioSummaryDataTable, Math.Round(DataTotalLibrary.portfolioSpreadCalcTotals[
                    DataCollectionLibrary.portfolioAllocation.brokerAccountChosen].pAndLDay, 2).ToString());
            //portfolioSummaryDataTable.Rows[(int)PORTFOLIO_SUMMARY_GRID_ROWS.PL_CHG][DataCollectionLibrary.instrumentList.Count]
            //        = Math.Round(DataTotalLibrary.portfolioSpreadCalcTotals[
            //        DataCollectionLibrary.portfolioAllocation.brokerAccountChosen].pAndLDay, 2);

            UpdateCell((int)PORTFOLIO_SUMMARY_GRID_ROWS.TOTAL_DELTA, DataCollectionLibrary.instrumentList.Count,
                    portfolioSummaryDataTable, Math.Round(DataTotalLibrary.portfolioSpreadCalcTotals[
                    DataCollectionLibrary.portfolioAllocation.brokerAccountChosen].delta, 2).ToString());
            //portfolioSummaryDataTable.Rows[(int)PORTFOLIO_SUMMARY_GRID_ROWS.TOTAL_DELTA][DataCollectionLibrary.instrumentList.Count]
            //    = Math.Round(DataTotalLibrary.portfolioSpreadCalcTotals[
            //        DataCollectionLibrary.portfolioAllocation.brokerAccountChosen].delta, 2);

            UpdateCell((int)PORTFOLIO_SUMMARY_GRID_ROWS.FCM_PL_CHG, DataCollectionLibrary.instrumentList.Count,
                    portfolioSummaryDataTable, Math.Round(DataTotalLibrary.portfolioADMSpreadCalcTotals[
                    DataCollectionLibrary.portfolioAllocation.brokerAccountChosen].pAndLDay, 2).ToString());
            //portfolioSummaryDataTable.Rows[(int)PORTFOLIO_SUMMARY_GRID_ROWS.FCM_PL_CHG][DataCollectionLibrary.instrumentList.Count]
            //    = Math.Round(DataTotalLibrary.portfolioADMSpreadCalcTotals[
            //        DataCollectionLibrary.portfolioAllocation.brokerAccountChosen].pAndLDay, 2);

            UpdateCell((int)PORTFOLIO_SUMMARY_GRID_ROWS.FCM_TOTAL_DELTA, DataCollectionLibrary.instrumentList.Count,
                    portfolioSummaryDataTable, Math.Round(DataTotalLibrary.portfolioADMSpreadCalcTotals[
                    DataCollectionLibrary.portfolioAllocation.brokerAccountChosen].delta, 2).ToString());
            //portfolioSummaryDataTable.Rows[(int)PORTFOLIO_SUMMARY_GRID_ROWS.FCM_TOTAL_DELTA][DataCollectionLibrary.instrumentList.Count]
            //    = Math.Round(DataTotalLibrary.portfolioADMSpreadCalcTotals[
            //        DataCollectionLibrary.portfolioAllocation.brokerAccountChosen].delta, 2);


        }

        private void UpdateCell(int row, int col, DataTable dataTable, string displayValue)
        {
            if (dataTable.Rows[row][col] == null
                || dataTable.Rows[row][col].ToString().CompareTo(displayValue) != 0)
            {
                dataTable.Rows[row][col] = displayValue;
            }
        }


        public void sendUpdateToPortfolioTotalSettlementGrid()
        {
            //for (int instrumentCnt = 0; instrumentCnt < DataTotalLibrary.instrumentSpreadTotals.Length; instrumentCnt++)

            int instrumentCnt = 0;
            foreach (Instrument_mongo im in DataCollectionLibrary.instrumentList)
            {
                UpdateCell((int)PORTFOLIO_SETTLEMENT_SUMMARY_GRID_ROWS.TOTAL_MODEL_SETTLEMENT_PL_CHG, instrumentCnt,
                    portfolioSummarySettlementDataTable, Math.Round(im.instrumentModelCalcTotals_ByAccount[
                        DataCollectionLibrary.portfolioAllocation.brokerAccountChosen].pAndLDaySettlementToSettlement, 2).ToString());
                //portfolioSummarySettlementDataTable.Rows[(int)PORTFOLIO_SETTLEMENT_SUMMARY_GRID_ROWS.TOTAL_MODEL_SETTLEMENT_PL_CHG][instrumentCnt] =
                //    Math.Round(im.instrumentModelCalcTotals_ByAccount[
                //        DataCollectionLibrary.portfolioAllocation.brokerAccountChosen].pAndLDaySettlementToSettlement, 2);

                UpdateCell((int)PORTFOLIO_SETTLEMENT_SUMMARY_GRID_ROWS.TOTAL_ADM_SETTLEMENT_PL_CHG, instrumentCnt,
                    portfolioSummarySettlementDataTable, Math.Round(im.instrumentADMCalcTotalsByAccount[
                        DataCollectionLibrary.portfolioAllocation.brokerAccountChosen].pAndLDaySettlementToSettlement, 2).ToString());
                //portfolioSummarySettlementDataTable.Rows[(int)PORTFOLIO_SETTLEMENT_SUMMARY_GRID_ROWS.TOTAL_ADM_SETTLEMENT_PL_CHG][instrumentCnt] =
                //    Math.Round(im.instrumentADMCalcTotalsByAccount[
                //        DataCollectionLibrary.portfolioAllocation.brokerAccountChosen].pAndLDaySettlementToSettlement, 2);

            }

            UpdateCell((int)PORTFOLIO_SETTLEMENT_SUMMARY_GRID_ROWS.TOTAL_MODEL_SETTLEMENT_PL_CHG, DataCollectionLibrary.instrumentList.Count,
                    portfolioSummarySettlementDataTable, Math.Round(DataTotalLibrary.portfolioSpreadCalcTotals[
                        DataCollectionLibrary.portfolioAllocation.brokerAccountChosen].pAndLDaySettlementToSettlement, 2).ToString());
            //portfolioSummarySettlementDataTable.Rows[(int)PORTFOLIO_SETTLEMENT_SUMMARY_GRID_ROWS.TOTAL_MODEL_SETTLEMENT_PL_CHG][DataCollectionLibrary.instrumentList.Count] =
            //        Math.Round(DataTotalLibrary.portfolioSpreadCalcTotals[
            //        DataCollectionLibrary.portfolioAllocation.brokerAccountChosen].pAndLDaySettlementToSettlement, 2);

            UpdateCell((int)PORTFOLIO_SETTLEMENT_SUMMARY_GRID_ROWS.TOTAL_ADM_SETTLEMENT_PL_CHG, DataCollectionLibrary.instrumentList.Count,
                    portfolioSummarySettlementDataTable, Math.Round(DataTotalLibrary.portfolioADMSpreadCalcTotals[
                        DataCollectionLibrary.portfolioAllocation.brokerAccountChosen].pAndLDaySettlementToSettlement, 2).ToString());
            //portfolioSummarySettlementDataTable.Rows[(int)PORTFOLIO_SETTLEMENT_SUMMARY_GRID_ROWS.TOTAL_ADM_SETTLEMENT_PL_CHG][DataCollectionLibrary.instrumentList.Count] =
            //    Math.Round(DataTotalLibrary.portfolioADMSpreadCalcTotals[
            //        DataCollectionLibrary.portfolioAllocation.brokerAccountChosen].pAndLDaySettlementToSettlement, 2);


        }

        public void sendUpdateToExpressionListGrid()  //*eQuoteType cqgQuoteType,*/ int spreadExpressionIdx /*int colIdx*/)
        {
            //CQGQuote quote = optionSpreadExpressionList[spreadExpressionIdx].cqgInstrument.Quotes[cqgQuoteType];

            try
            {

                int optionSpreadCounter = 0;

                foreach (MongoDB_OptionSpreadExpression ose in optionSpreadExpressionList)
                {
                    if (ose.optionExpressionType != OPTION_EXPRESSION_TYPES.OPTION_EXPRESSION_RISK_FREE_RATE
                        &&
                        ose.dataGridExpressionListRow <= dataGridViewExpressionList.Rows.Count)
                    {
                        //ose.dataGridExpressionListRow = rowIdx;
                        CQGInstrument cqgInstrument = ose.cqgInstrument;

                        if (cqgInstrument != null)  // && CQG. cqgInstrument)
                        {
                            //MongoDB_OptionSpreadExpression optionSpreadExpressionList = optionStrategies[optionSpreadCounter].legData[legCounter].optionSpreadExpression;

                            //optionSpreadManager.statusAndConnectedUpdates.checkUpdateStatus(dataGridViewExpressionList, ose.dataGridExpressionListRow,
                            //    (int)EXPRESSION_LIST_VIEW.TIME, ose);

                            //if (optionSpreadManager.realtimeMonitorSettings.eodAnalysis)
                            if (ose.instrument.eodAnalysisAtInstrument)
                            {
                                //dataGridViewExpressionList.Columns[(int)EXPRESSION_LIST_VIEW.TIME].DefaultCellStyle.Font = new Font("Tahoma", 6);

                                fillDataGridViewExpressionListPage(ose.dataGridExpressionListRow,
                                        (int)EXPRESSION_LIST_VIEW.TIME,
                                        ose.lastTimeUpdated.ToString("yyyy-MM-dd HH:mm", DateTimeFormatInfo.InvariantInfo),
                                        false, 0);
                            }
                            else
                            {
                                //dataGridViewExpressionList.Columns[(int)EXPRESSION_LIST_VIEW.TIME].DefaultCellStyle.Font = new Font("Tahoma", 8);

                                fillDataGridViewExpressionListPage(ose.dataGridExpressionListRow,
                                        (int)EXPRESSION_LIST_VIEW.TIME,
                                        ose.lastTimeUpdated.ToString("HH:mm", DateTimeFormatInfo.InvariantInfo),
                                        false, 0);
                            }

                            fillDataGridViewExpressionListPage(ose.dataGridExpressionListRow,
                                    (int)EXPRESSION_LIST_VIEW.DELTA,
                                    Math.Round(ose.delta, 2).ToString(), true, ose.delta);

                            //************************************************
                            fillDataGridViewExpressionListPage(ose.dataGridExpressionListRow,
                                    (int)EXPRESSION_LIST_VIEW.TRANS_PRICE,
                                    ose.transactionPrice.ToString(), false, ose.transactionPrice);

                            //if (optionSpreadManager.realtimeMonitorSettings.eodAnalysis)
                            //if (ose.instrument.eodAnalysisAtInstrument)
                            //{
                            //    fillDataGridViewExpressionListPage(ose.dataGridExpressionListRow,
                            //            (int)EXPRESSION_LIST_VIEW.ORDER_PL,
                            //            Math.Round(ose.plChgOrdersToSettlement, 2).ToString(), true, ose.plChgOrdersToSettlement);
                            //}
                            //else
                            //{
                            //    fillDataGridViewExpressionListPage(ose.dataGridExpressionListRow,
                            //            (int)EXPRESSION_LIST_VIEW.ORDER_PL,
                            //            Math.Round(ose.plChgOrders, 2).ToString(), true, ose.plChgOrders);
                            //}
                            //************************************************


                            fillDataGridViewExpressionListPage(ose.dataGridExpressionListRow,
                                    (int)EXPRESSION_LIST_VIEW.DFLT_PRICE,
                                    ose.defaultPrice.ToString(), false, ose.defaultPrice);

                            //if (ose.decisionPriceFilled != null)
                            //{
                            //    fillDataGridViewExpressionListPage(ose.dataGridExpressionListRow,
                            //            (int)EXPRESSION_LIST_VIEW.CUM_VOL,
                            //            "0", false,
                            //            0);
                            //}

                            fillDataGridViewExpressionListPage(ose.dataGridExpressionListRow,
                                    (int)EXPRESSION_LIST_VIEW.THEOR_PRICE,
                                    ose.theoreticalOptionPrice.ToString(), false, ose.theoreticalOptionPrice);

                            fillDataGridViewExpressionListPage(ose.dataGridExpressionListRow,
                                    (int)EXPRESSION_LIST_VIEW.SPAN_IMPL_VOL,
                                    Math.Round(ose.impliedVolFromSpan, 2).ToString(), false, ose.impliedVolFromSpan);

                            fillDataGridViewExpressionListPage(ose.dataGridExpressionListRow,
                                    (int)EXPRESSION_LIST_VIEW.SETL_IMPL_VOL,
                                    Math.Round(ose.settlementImpliedVol, 2).ToString(), false, ose.settlementImpliedVol);

                            fillDataGridViewExpressionListPage(ose.dataGridExpressionListRow,
                                    (int)EXPRESSION_LIST_VIEW.IMPL_VOL,
                                    Math.Round(ose.impliedVol, 2).ToString(), false, ose.impliedVol);



                            fillDataGridViewExpressionListPage(ose.dataGridExpressionListRow,
                                    (int)EXPRESSION_LIST_VIEW.BID,
                                    ose.bid.ToString(), false, ose.bid);

                            fillDataGridViewExpressionListPage(ose.dataGridExpressionListRow,
                                    (int)EXPRESSION_LIST_VIEW.ASK,
                                    ose.ask.ToString(), false, ose.ask);

                            fillDataGridViewExpressionListPage(ose.dataGridExpressionListRow,
                                    (int)EXPRESSION_LIST_VIEW.LAST,
                                    ose.trade.ToString(), false, ose.trade);

                            fillDataGridViewExpressionListPage(ose.dataGridExpressionListRow,
                                    (int)EXPRESSION_LIST_VIEW.STTLE,
                                    ose.settlement.ToString(), false, ose.settlement);

                            if (ose.settlementDateTime.Date.CompareTo(DateTime.Now.Date) >= 0)
                            {
                                fillDataGridViewExpressionListPage(ose.dataGridExpressionListRow,
                                        (int)EXPRESSION_LIST_VIEW.SETL_TIME,
                                        ose.settlementDateTime.ToString("yyyy-MM-dd HH:mm", DateTimeFormatInfo.InvariantInfo),
                                            true, 1);
                            }
                            else
                            {
                                fillDataGridViewExpressionListPage(ose.dataGridExpressionListRow,
                                        (int)EXPRESSION_LIST_VIEW.SETL_TIME,
                                        ose.settlementDateTime.ToString("yyyy-MM-dd HH:mm", DateTimeFormatInfo.InvariantInfo),
                                            true, -1);
                            }

                            fillDataGridViewExpressionListPage(ose.dataGridExpressionListRow,
                                    (int)EXPRESSION_LIST_VIEW.YEST_STTLE,
                                    ose.yesterdaySettlement.ToString(), false, ose.yesterdaySettlement);


                        }
                    }

                    optionSpreadCounter++;
                }



            }
            catch (Exception ex)
            {
                TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
            }

        }


        public void fillDataGridViewExpressionListPage(int row, int col, String displayValue,
            bool updateColor, double value)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    ThreadSafeFillLiveDataPageDelegate d = new ThreadSafeFillLiveDataPageDelegate(threadSafeFillDataGridViewExpressionListPage);

                    this.Invoke(d, row, col, displayValue, updateColor, value);
                }
                else
                {
                    threadSafeFillDataGridViewExpressionListPage(row, col, displayValue, updateColor, value);
                }
            }
            catch (Exception ex)
            {
                TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
            }
        }

        public void threadSafeFillDataGridViewExpressionListPage(int row, int col, String displayValue,
            bool updateColor, double value)
        {
            try
            {
                int rowToUpdate = row;

                if (
                    (
                    dataGridViewExpressionList.Rows[rowToUpdate].Cells[col].Value == null
                    ||
                    dataGridViewExpressionList.Rows[rowToUpdate].Cells[col].Value.ToString().CompareTo(displayValue) != 0
                    ))
                {
                    dataGridViewExpressionList.Rows[rowToUpdate].Cells[col].Value = displayValue;

                    if (updateColor)
                    {
                        dataGridViewExpressionList.Rows[rowToUpdate].Cells[col].Style.BackColor = plUpDownColor(value);
                    }
                }

            }
            catch (Exception ex)
            {
                TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
            }
        }


        //*********

        delegate void ThreadSafeHideUnhideContractSummaryLiveDataDelegate(DataGridView gridView, int row, bool visible);

        public void HideUnhideSummaryData(DataGridView gridView, int row, bool visible)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    ThreadSafeHideUnhideContractSummaryLiveDataDelegate d =
                        new ThreadSafeHideUnhideContractSummaryLiveDataDelegate(threadSafeHideUnhideSummaryData);

                    this.Invoke(d, gridView, row, visible);
                }
                else
                {
                    threadSafeHideUnhideSummaryData(gridView, row, visible);
                }
            }
            catch (Exception ex)
            {
                TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
            }
        }

        public void threadSafeHideUnhideSummaryData(DataGridView gridView, int row, bool visible)
        {
            try
            {
                //if (gridView.DataSource != null)
                //{
                //    CurrencyManager currencyManager1 = (CurrencyManager)BindingContext[gridView.DataSource];
                //    currencyManager1.SuspendBinding();
                //    gridView.Rows[row].Visible = visible;
                //    currencyManager1.ResumeBinding();
                //}
                //else
                {
                    gridView.Rows[row].Visible = visible;
                }



            }
            catch (Exception ex)
            {
                TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
            }
        }


        //public void fillLiveADMDataPage(int row, int col, String displayValue,
        //    bool updateColor, double value)
        //{
        //    try
        //    {
        //        if (this.InvokeRequired)
        //        {
        //            ThreadSafeFillLiveDataPageDelegate d = new ThreadSafeFillLiveDataPageDelegate(threadSafeFillLiveADMDataPage);

        //            this.Invoke(d, row, col, displayValue, updateColor, value);
        //        }
        //        else
        //        {
        //            threadSafeFillLiveADMDataPage(row, col, displayValue, updateColor, value);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
        //    }
        //}

        //public void threadSafeFillLiveADMDataPage(int row, int col, String displayValue,
        //    bool updateColor, double value)
        //{
        //    try
        //    {
        //        int rowToUpdate = row;

        //        if (gridLiveFCMData.Rows[rowToUpdate].Cells[col].Value == null
        //            ||
        //            gridLiveFCMData.Rows[rowToUpdate].Cells[col].Value.ToString().CompareTo(displayValue) != 0
        //            )
        //        {
        //            gridLiveFCMData.Rows[rowToUpdate].Cells[col].Value = displayValue;

        //            if (updateColor)
        //            {
        //                gridLiveFCMData.Rows[rowToUpdate].Cells[col].Style.BackColor = plUpDownColor(value);
        //            }
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
        //    }
        //}



        //public void threadSafeFillPortfolioSummary(DataGridView gridView, int row, int col, String displayValue,
        //    bool updateColor, double value)
        //{
        //    try
        //    {
        //        if (gridView.Rows[row].Cells[col].Value == null
        //            ||
        //            gridView.Rows[row].Cells[col].Value.ToString().CompareTo(displayValue) != 0
        //            )
        //        {
        //            gridView.Rows[row].Cells[col].Value = displayValue;

        //            if (updateColor)
        //            {
        //                gridView.Rows[row].Cells[col].Style.BackColor = plUpDownColor(value);
        //                //portfolioSummaryGrid.Rows[row].Cells[col].Style.ForeColor = Color.Black;
        //            }
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
        //    }
        //}


        private Color plUpDownColor(double value)
        {
            if (value >= 0)
            {
                return RealtimeColors.positiveBackColor;
            }
            else
            {
                return RealtimeColors.negativeBackColor;
            }
        }

        //private Color orderEngagedColor(double value)
        //{
        //    if (value >= 0)
        //    {
        //        return Color.LawnGreen;
        //    }
        //    else
        //    {
        //        return Color.WhiteSmoke;
        //    }
        //}

        internal void draggingFileCheck(object sender, DragEventArgs e)
        {


            Array files = (Array)(e.Data.GetData(DataFormats.FileDrop));

            if (fileLoadListForm == null)
            {
                fileLoadListForm = new FileLoadList(this);
            }

            fileLoadListForm.Show();
            fileLoadListForm.BringToFront();

            for (int i = 0; i < files.Length; i++)
            {
                fileLoadListForm.loadFiles((String)files.GetValue(i));
            }
        }

        public void closeFileLoadList()
        {
            fileLoadListForm.Close();

            fileLoadListForm = null;
        }

        public void loadFiles(String[] files)
        {


            if (files != null && files.Length > 0)
            {


                //String configFileName = files;  // (String)files.GetValue(0);

                //fileLoadListForm.loadFiles(configFileName);

                {
                    bool updatedSettings = false;

                    if (DataCollectionLibrary.realtimeMonitorSettings.eodAnalysis)
                    {
                        DataCollectionLibrary.realtimeMonitorSettings.eodAnalysis = false;

                        updatedSettings = true;
                    }

                    if (settings == null)
                    {
                        settings = new Settings(optionSpreadManager);
                    }
                    else
                    {
                        settings.updateSettings();
                    }

                    if (updatedSettings)
                    {
                        optionSpreadManager.updateEODMonitorDataSettings(false);
                    }

                    Thread loadingADMFileThread = new Thread(new ParameterizedThreadStart(loadingADMFile));
                    loadingADMFileThread.IsBackground = true;
                    loadingADMFileThread.Start(files);

                    //loadingADMFile(files);

                    //loadingADMFile(configFileName);
                    //optionSpreadManager.displayADMInputWithWebPositions();
                    //optionSpreadManager.displayADMInputWithWebPositions();
                }
            }
        }

        private void OptionRealtimeMonitor_DragDrop(object sender, DragEventArgs e)
        {

            draggingFileCheck(sender, e);

        }

        private void loadingADMFile(Object configFileNameObj)
        {
            this.Invoke(new EventHandler(ThreadTracker.openThread));

            String[] configFileName = (String[])configFileNameObj;

            ImportFileCheck importFileCheck = new ImportFileCheck();

            importFileCheck.importingBackedUpSavedFile = false;

            //bool importFile = 

            optionSpreadManager.fillADMInputWithWebPositions(configFileName,
                importFileCheck);

            if (importFileCheck.importfile)
            {
                //optionSpreadManager.aDMDataCommonMethods.copyADMStoredDataFile(configFileName);


                //June 15 2015
                optionSpreadManager.aDMDataCommonMethods.copyADMDataToFile(
                    FCM_DataImportLibrary.FCM_positionImportNotConsolidated,
                    importFileCheck);
                //

                optionSpreadManager.modelADMCompareCalculationAndDisplay.fillGridModelADMComparison(this);

                //gridViewFCMPostionManipulation.fillGridLiveADMData(this);

                //gridViewFCMPostionManipulation.FillContractSummary();

                optionSpreadManager.resetDataUpdatesWithLatestExpressions();
            }

            ThreadSafeGenericDelegateWithoutParams displayFCMDataDelegateRun =
                new ThreadSafeGenericDelegateWithoutParams(
                    optionSpreadManager.displayADMInputWithWebPositions);

            this.Invoke(displayFCMDataDelegateRun);

            //optionSpreadManager.displayADMInputWithWebPositions();

            this.Invoke(new EventHandler(ThreadTracker.closeThread));
        }


        private void admPositionsCheckAndFillGrid(ADMPositionImportWeb aDMPositionImportWeb,
            int rowCounter, bool isInADMList)
        {
            //DateTime currentDate = DateTime.Now;


            //gridLiveADMData.Rows[rowCounter].Cells[(int)OPTION_LIVE_ADM_DATA_COLUMNS.CONTRACT].Value =
            //                    liveADMStrategyInfo.admLegInfo[legCounter].aDMPositionImportWeb.cqgsymbol;

            //gridLiveADMData.Rows[rowCounter].Cells[(int)OPTION_LIVE_ADM_DATA_COLUMNS.LEG].Value = legCounter + 1;

            //gridLiveADMData.Rows[rowCounter].Cells[(int)OPTION_LIVE_ADM_DATA_COLUMNS.NET].Value =
            //    liveADMStrategyInfo.admLegInfo[legCounter].numberOfContracts;



            //gridLiveADMData.Rows[rowCounter].Cells[(int)OPTION_LIVE_ADM_DATA_COLUMNS.MODEL].Value =
            //                liveADMStrategyInfo.admLegInfo[legCounter].numberOfModelContracts;


            //TimeSpan span = liveADMStrategyInfo.admLegInfo[legCounter].aDMPositionImportWeb.legInfo.expirationDate.Date - currentDate.Date;

            //gridLiveADMData.Rows[rowCounter].Cells[(int)OPTION_LIVE_ADM_DATA_COLUMNS.CNTDN].Value =
            //                    span.Days;

            //gridLiveADMData.Rows[rowCounter].Cells[(int)OPTION_LIVE_ADM_DATA_COLUMNS.EXPR].Value =
            //    new DateTime(
            //                        liveADMStrategyInfo.admLegInfo[legCounter].aDMPositionImportWeb.legInfo.expirationDate.Year,
            //                        liveADMStrategyInfo.admLegInfo[legCounter].aDMPositionImportWeb.legInfo.expirationDate.Month,
            //                        liveADMStrategyInfo.admLegInfo[legCounter].aDMPositionImportWeb.legInfo.expirationDate.Day,
            //                        liveADMStrategyInfo.admLegInfo[legCounter].aDMPositionImportWeb.legInfo.optionExpirationTime.Hour,
            //                        liveADMStrategyInfo.admLegInfo[legCounter].aDMPositionImportWeb.legInfo.optionExpirationTime.Minute,
            //                        0
            //                    )
            //                    .ToString("yyyy-MM-dd HH:mm", DateTimeFormatInfo.InvariantInfo);

            //gridLiveADMData.Rows[rowCounter].Cells[(int)OPTION_LIVE_ADM_DATA_COLUMNS.AVERAGE_PRC].Value =
            //    liveADMStrategyInfo.admLegInfo[legCounter].aDMPositionImportWeb.AveragePrice;

            //gridLiveADMData.Rows[rowCounter].Cells[(int)OPTION_LIVE_ADM_DATA_COLUMNS.STRIKE].Value =
            //    liveADMStrategyInfo.admLegInfo[legCounter].aDMPositionImportWeb.strike;

            //gridLiveADMData.Rows[rowCounter].Cells[(int)OPTION_LIVE_ADM_DATA_COLUMNS.DESCRIP].Value =
            //    liveADMStrategyInfo.admLegInfo[legCounter].aDMPositionImportWeb.Description;

            //gridLiveADMData.Rows[rowCounter].Cells[(int)OPTION_LIVE_ADM_DATA_COLUMNS.CUSIP].Value =
            //    liveADMStrategyInfo.admLegInfo[legCounter].aDMPositionImportWeb.PCUSIP;

            //gridLiveADMData.Rows[rowCounter].Cells[(int)OPTION_LIVE_ADM_DATA_COLUMNS.SPREAD_ID].Value = strategyCount;





        }

        private void checkFuturesModelLotsAgainstADMNetAndHighLight(int legIdx, int rowIdx)
        {
            //if (liveADMStrategyInfo.admLegInfo[legIdx].numberOfModelContracts !=
            //        liveADMStrategyInfo.admLegInfo[legIdx].numberOfContracts)
            //{
            //    gridLiveADMData.Rows[rowIdx].Cells[(int)OPTION_LIVE_ADM_DATA_COLUMNS.MODEL].Style.BackColor = Color.Aquamarine;
            //}
            //else
            //{
            //    gridLiveADMData.Rows[rowIdx].Cells[(int)OPTION_LIVE_ADM_DATA_COLUMNS.MODEL].Style.BackColor = Color.White;
            //}
        }

        private void OptionRealtimeMonitor_DragEnter(object sender, DragEventArgs e)
        {
#if DEBUG
            try
#endif
            {
                e.Effect = DragDropEffects.All;
            }
#if DEBUG
            catch (Exception ex)
            {
                TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
            }
#endif
        }

        public void hideMessageToReconnect()
        {
            if (this.InvokeRequired)
            {
                ThreadSafeMoveSplitterDistance d =
                    new ThreadSafeMoveSplitterDistance(threadSafeMoveSplitter);

                this.Invoke(d, 0);
            }
            else
            {
                threadSafeMoveSplitter(0);
            }
        }

        public void displayMessageToReConnect()
        {
            //splitContainer4.SplitterDistance = 50;

            if (this.InvokeRequired)
            {
                ThreadSafeMoveSplitterDistance d =
                    new ThreadSafeMoveSplitterDistance(threadSafeMoveSplitter);

                this.Invoke(d, 50);
            }
            else
            {
                threadSafeMoveSplitter(50);
            }
        }

        public void threadSafeMoveSplitter(int splitterDistance)
        {
            try
            {
                splitContainer4.SplitterDistance = splitterDistance;
            }
            catch (Exception ex)
            {
                TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
            }
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (settings == null)
            {
                settings = new Settings(optionSpreadManager);
            }

            settings.Show();

            settings.BringToFront();
        }





        private void bringUpADMWebpageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //             ProcessStartInfo psi = new ProcessStartInfo(@"<Path for Internet explorer - iexplorer.exe file>");
            // 
            //             psi.Arguments = @"<path for your web application>";
            // 
            //             Process.Start(psi);

            System.Diagnostics.Process.Start("IEXPLORE.EXE", "https://members.admis.com/AccountLogin.aspx?ReturnUrl=%2fdefault.aspx");
        }

        private void showADMWebImportedDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            optionSpreadManager.displayADMInputWithWebPositions();

            //updateColorOfADMStrategyGrid();
        }

        private void gridLiveADMData_DragDrop(object sender, DragEventArgs e)
        {

        }

        private void gridLiveADMData_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;

            if (e.Data.GetDataPresent(typeof(ADMDragOverData)))
            {


                Point clientPoint = gridLiveFCMData.PointToClient(new Point(e.X, e.Y));

                int rowIndexOfItemUnderMouseToDrop = gridLiveFCMData.HitTest(clientPoint.X, clientPoint.Y).RowIndex;

                if (rowIndexOfItemUnderMouseToDrop != -1)
                {
                    gridLiveFCMData.Rows[rowIndexOfItemUnderMouseToDrop].Selected = true;
                }
            }
        }

        private void gridLiveADMData_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (e.ColumnIndex != (int)OPTION_LIVE_ADM_DATA_COLUMNS.NET_EDITABLE)
            {
                e.Cancel = true;
            }

        }

        private void gridLiveADMData_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == (int)OPTION_LIVE_ADM_DATA_COLUMNS.NET_EDITABLE)
            {

                //int admPosWebIdx = Convert.ToInt16(gridLiveFCMData.Rows[e.RowIndex].Cells[
                //    (int)OPTION_LIVE_ADM_DATA_COLUMNS.ADMPOSWEB_IDX].Value);

                //int contracts = Convert.ToInt16(gridLiveFCMData.Rows[e.RowIndex].Cells[e.ColumnIndex].Value);

                //if (contracts != Convert.ToInt16(gridLiveFCMData.Rows[e.RowIndex].Cells[
                //    (int)OPTION_LIVE_ADM_DATA_COLUMNS.NET_AT_ADM].Value))
                //{
                //    gridLiveFCMData.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.BackColor = RealtimeColors.negativeBackColor;
                //}
                //else
                //{
                //    gridLiveFCMData.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.BackColor = Color.White;
                //}

                //FCM_DataImportLibrary.FCM_Import_Consolidated[admPosWebIdx].netContractsEditable = contracts;




            }

        }



        private void OptionRealtimeMonitor_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;

            Cursor.Current = Cursors.WaitCursor;

            //portfolioSummaryGrid.Rows[1].Cells[1].Value = "9999";

            //Thread threadTest = new Thread(new ParameterizedThreadStart(runSystem));
            //threadTest.IsBackground = true;
            //threadTest.Start();

            //Thread.Sleep(7000);

            optionSpreadManager.shutDownOptionSpreadRealtime();
        }



        private void copyListToClipboard(ListView listView1)
        {

            // Bail out early if no selected items 
            if (listView1.SelectedItems.Count == 0)
                return;

            StringBuilder buffer = new StringBuilder();
            // Loop over all the selected items 
            foreach (ListViewItem currentItem in listView1.SelectedItems)
            {
                // Don't need to look at currentItem, because it is in subitem[0] 
                // So just loop over all the subitems of this selected item 
                foreach (ListViewItem.ListViewSubItem sub in currentItem.SubItems)
                {
                    // Append the text and tab 
                    buffer.Append(sub.Text);
                    buffer.Append("\t");
                }
                // Annoyance: there is a trailing tab in the buffer, get rid of it 
                buffer.Remove(buffer.Length - 1, 1);
                // If you only use \n, not all programs (notepad!!!) will recognize the newline 
                buffer.Append("\r\n");
            }
            // Set output to clipboard. 
            Clipboard.SetDataObject(buffer.ToString(), true);

        }

        private void orderSummaryList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.C && e.Modifiers == Keys.Control)
            {
                copyListToClipboard((ListView)sender);
            }
        }




        private void makeCMEMarginRequestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //TODO THIS NEEDS FIXING
            //CMEMarginCall cmeMarginCall = new CMEMarginCall(optionSpreadExpressionList,
            //    this, DataTotalLibrary.portfolioSpreadTotals, FCM_DataImportLibrary.FCM_Import_Consolidated, false);

            //cmeMarginCall.generateMarginRequest();

            //Thread calcMarginCallThread = new Thread(new ParameterizedThreadStart(cmeMarginCall.generateMarginRequest));

            //calcMarginCallThread.Start();

            //cmeMarginCall.generateMarginRequest(optionSpreadExpressionList, optionStrategies,
            //    instruments, this, portfolioSpreadTotals);

            //for (int i = 0; i < instruments.Count(); i++)
            //{
            //    instrumentSummaryGrid.Rows[(int)INSTRUMENT_SUMMARY_GRID_ROWS.SPAN_INIT_MARGIN].Cells[i].Value =
            //        instruments[i].coreAPIinitialMargin;

            //    instrumentSummaryGrid.Rows[(int)INSTRUMENT_SUMMARY_GRID_ROWS.SPAN_MAINT_MARGIN].Cells[i].Value =
            //        instruments[i].coreAPImaintenanceMargin;
            //}
        }


        private void treeViewInstrumentsContractSummary_AfterSelect_1(object sender, TreeViewEventArgs e)
        {
            TreeNode x = treeViewInstruments.SelectedNode;

            if (x != null)
            {
                if (x.Index >= DataCollectionLibrary.instrumentList.Count)
                {
                    DataCollectionLibrary.instrumentSelectedInTreeGui
                        = TradingSystemConstants.ALL_INSTRUMENTS_SELECTED;
                }
                else
                {
                    DataCollectionLibrary.instrumentSelectedInTreeGui
                        = Convert.ToInt32(DataCollectionLibrary.instrumentList[x.Index].idinstrument);
                }

                updateSelectedInstrumentFromTree();
            }
        }

        private void updateSelectedInstrumentFromTree()
        {
            fillOrderSummaryList();

            DataCollectionLibrary.performFullContractRefresh = true;

            ContractsModel_Library.gridViewContractSummaryManipulation.FillContractSummary();

            DataCollectionLibrary.performFull_FCMSummary_Refresh = true;

            ContractsModel_Library.gridViewFCMPostionManipulation.Fill_FCM_ContractSummary();

            if (FCM_DataImportLibrary.FCM_ReportWebPositionsForm != null)
            {
                FCM_DataImportLibrary.FCM_ReportWebPositionsForm.fillFCMData();
            }



            //if (x != null)
            {
                //contractSummaryInstrumentSelectedIdx = x.Index;

                if (DataCollectionLibrary.instrumentSelectedInTreeGui == TradingSystemConstants.ALL_INSTRUMENTS_SELECTED)
                {

                    try
                    {
                        for (int liveADMDataCntRow = 0; liveADMDataCntRow < gridLiveFCMData.RowCount; liveADMDataCntRow++)
                        {
                            var key = Tuple.Create(
                                gridLiveFCMData.Rows[liveADMDataCntRow].Cells[(int)OPTION_LIVE_ADM_DATA_COLUMNS.POFFIC].Value.ToString(),
                                gridLiveFCMData.Rows[liveADMDataCntRow].Cells[(int)OPTION_LIVE_ADM_DATA_COLUMNS.PACCT].Value.ToString());

                            if (DataCollectionLibrary.portfolioAllocation.accountAllocation_KeyOfficAcct.ContainsKey(key))
                            {
                                AccountAllocation ac =
                                    DataCollectionLibrary.portfolioAllocation.accountAllocation_KeyOfficAcct[key];

                                if (ac.visible)
                                {
                                    HideUnhideSummaryData(gridLiveFCMData, liveADMDataCntRow, true);
                                }
                                else
                                {
                                    HideUnhideSummaryData(gridLiveFCMData, liveADMDataCntRow, false);
                                }
                            }
                            else
                            {
                                HideUnhideSummaryData(gridLiveFCMData, liveADMDataCntRow, true);
                            }
                        }
                    }
                    catch
                    { }

                    for (int dataGridViewExpressionListCount = 0; dataGridViewExpressionListCount < dataGridViewExpressionList.RowCount;
                        dataGridViewExpressionListCount++)
                    {
                        HideUnhideSummaryData(dataGridViewExpressionList, dataGridViewExpressionListCount, true);
                    }
                }
                else
                {
                    ///
                    ///THIS IS WHERE NOT ALL INSTRUMENTS ARE SELECTED
                    ///

                    for (int aDMCompareCntRow = 0;
                        aDMCompareCntRow < gridLiveFCMData.RowCount; aDMCompareCntRow++)
                    {
                        int instrumentId
                            = Convert.ToInt32(gridLiveFCMData.Rows[aDMCompareCntRow].Cells[(int)OPTION_LIVE_ADM_DATA_COLUMNS.INSTRUMENT_ID].Value);

                        var key = Tuple.Create(
                            gridLiveFCMData.Rows[aDMCompareCntRow].Cells[(int)OPTION_LIVE_ADM_DATA_COLUMNS.POFFIC].Value.ToString(),
                            gridLiveFCMData.Rows[aDMCompareCntRow].Cells[(int)OPTION_LIVE_ADM_DATA_COLUMNS.PACCT].Value.ToString());

                        bool acctVisible = true;
                        if (DataCollectionLibrary.portfolioAllocation.accountAllocation_KeyOfficAcct.ContainsKey(key))
                        {
                            AccountAllocation ac =
                                DataCollectionLibrary.portfolioAllocation.accountAllocation_KeyOfficAcct[key];

                            acctVisible = ac.visible;
                        }

                        if (Convert.ToInt32(gridLiveFCMData.Rows[aDMCompareCntRow].Cells[(int)OPTION_LIVE_ADM_DATA_COLUMNS.INSTRUMENT_ID].Value)
                            == DataCollectionLibrary.instrumentSelectedInTreeGui
                            && acctVisible)
                        {
                            HideUnhideSummaryData(gridLiveFCMData, aDMCompareCntRow, true);
                        }
                        else
                        {
                            HideUnhideSummaryData(gridLiveFCMData, aDMCompareCntRow, false);
                        }
                    }

                    for (int dataGridViewExpressionListCount = 0;
                        dataGridViewExpressionListCount < dataGridViewExpressionList.RowCount; dataGridViewExpressionListCount++)
                    {
                        int instrumentId = Convert.ToInt16(dataGridViewExpressionList.Rows[dataGridViewExpressionListCount].Cells[(int)EXPRESSION_LIST_VIEW.INSTRUMENT_ID].Value);

                        if (DataCollectionLibrary.instrumentSelectedInTreeGui == instrumentId)
                        {
                            HideUnhideSummaryData(dataGridViewExpressionList, dataGridViewExpressionListCount, true);
                        }
                        else
                        {
                            HideUnhideSummaryData(dataGridViewExpressionList, dataGridViewExpressionListCount, false);
                        }
                    }
                }


                updateModelADMCompareViewable();

            }




        }

        internal void updateModelADMCompareViewable()
        {



            //if (DataCollectionLibrary.instrumentSelectedInTreeGui == TradingSystemConstants.ALL_INSTRUMENTS_SELECTED)
            //{

            //    for (int modelADMCompareCntRow = 0;
            //                    modelADMCompareCntRow < gridViewModelADMCompare.RowCount; modelADMCompareCntRow++)
            //    {
            //        bool accountGroupDisplay = true;

            //        if (!(DataCollectionLibrary.portfolioAllocation.brokerAccountChosen == DataCollectionLibrary.portfolioAllocation.accountAllocation.Count))
            //        {
            //            int acctGrpIdx = Convert.ToInt16(gridViewModelADMCompare.Rows[modelADMCompareCntRow]
            //                .Cells[(int)ADM_MODEL_POSITION_COMPARE_FIELDS_DISPLAYED.ACCT_GROUP_IDX].Value);

            //            if (DataCollectionLibrary.brokerAccountChosen == acctGrpIdx)
            //            {
            //                accountGroupDisplay = true;
            //            }
            //            else
            //            {
            //                accountGroupDisplay = false;
            //            }
            //        }

            //        if (accountGroupDisplay)
            //        {
            //            HideUnhideSummaryData(gridViewModelADMCompare, modelADMCompareCntRow, true);
            //        }
            //        else
            //        {
            //            HideUnhideSummaryData(gridViewModelADMCompare, modelADMCompareCntRow, false);
            //        }
            //    }
            //}
            //else
            //{
            //    for (int modelADMCompareCntRow = 0;
            //            modelADMCompareCntRow < gridViewModelADMCompare.RowCount; modelADMCompareCntRow++)
            //    {
            //        int instrumentId = Convert.ToInt16(gridViewModelADMCompare.Rows[modelADMCompareCntRow].Cells[(int)ADM_MODEL_POSITION_COMPARE_FIELDS_DISPLAYED.INSTRUMENT_ID].Value);

            //        bool accountGroupDisplay = true;

            //        if (!(DataCollectionLibrary.brokerAccountChosen == DataCollectionLibrary.portfolioAllocation.accountAllocation.Count))
            //        {
            //            int acctGrpIdx = Convert.ToInt16(gridViewModelADMCompare.Rows[modelADMCompareCntRow]
            //                .Cells[(int)ADM_MODEL_POSITION_COMPARE_FIELDS_DISPLAYED.ACCT_GROUP_IDX].Value);

            //            if (DataCollectionLibrary.brokerAccountChosen == acctGrpIdx)
            //            {
            //                accountGroupDisplay = true;
            //            }
            //            else
            //            {
            //                accountGroupDisplay = false;
            //            }
            //        }

            //        if (DataCollectionLibrary.instrumentSelectedInTreeGui == instrumentId
            //            && accountGroupDisplay)
            //        {
            //            HideUnhideSummaryData(gridViewModelADMCompare, modelADMCompareCntRow, true);
            //        }
            //        else
            //        {
            //            HideUnhideSummaryData(gridViewModelADMCompare, modelADMCompareCntRow, false);
            //        }

            //    }
            //}
        }


        private void tsbtnPortfolioAndOrderPayoff_Click(object sender, EventArgs e)
        {
            if (DataCollectionLibrary.instrumentSelectedInTreeGui != TradingSystemConstants.ALL_INSTRUMENTS_SELECTED)
            {

                //OptionPLChart scPrice = new OptionPLChart(optionArrayTypes, optionSpreadManager);
                OptionPayoffChart scPrice = new OptionPayoffChart();
                scPrice.optionPLChartUserForm1.startupChart(optionArrayTypes, optionSpreadManager);

                //List<ContractList> contractListOfRollovers = new List<ContractList>();


                //foreach (var cl in
                //    optionSpreadManager.instrumentRollIntoSummary[DataCollectionLibrary.contractSummaryInstrumentSelectedIdx]
                //        .contractHashTable)
                //{
                //    // cl.Value.cqgsymbol\
                //    //TSErrorCatch.debugWriteOut(cl.Value.cqgsymbol);
                //    if (cl.Value.currentlyRollingContract
                //        && cl.Value.numberOfContracts != 0)
                //    {
                //        contractListOfRollovers.Add(cl.Value);
                //    }
                //}

                //contractSummaryExpressionListIdx,
                //    optionSpreadExpressionList, instruments[contractSummaryInstrumentSelectedIdx],

                //List<int> contractsInOrders = new List<int>();

                //for (int contractSummaryCnt = 0; contractSummaryCnt < contractSummaryExpressionListIdx.Count; contractSummaryCnt++)
                //{
                //    contractsInOrders.Add(contractSummaryExpressionListIdx[contractSummaryCnt]);
                //}

                //for (int expressionCount = 0; expressionCount < optionSpreadExpressionList.Count; expressionCount++)
                //{
                //    if (DataCollectionLibrary.orderSummaryList.Contains(optionSpreadExpressionList[expressionCount].cqgsymbol)
                //        && !contractsInOrders.Contains(expressionCount))
                //    {
                //        contractsInOrders.Add(expressionCount);
                //    }
                //}

                Instrument_mongo im =
                    DataCollectionLibrary
                        .instrumentHashTable_keyinstrumentid[DataCollectionLibrary.instrumentSelectedInTreeGui];

                scPrice.optionPLChartUserForm1.fillGrid(
                    //optionSpreadManager, contractsInOrders,
                    //optionSpreadExpressionList, 
                    im,
                    //0, 
                    OptionPLChartUserForm.PAYOFF_CHART_TYPE.CONTRACT_AND_ORDER_SUMMARY_PAYOFF
                    //null
                    );

                scPrice.optionPLChartUserForm1.fillChart();

                scPrice.Show();
            }
        }

        private void tsbtnOrderPayoffChart_Click(object sender, EventArgs e)
        {
            if (DataCollectionLibrary.instrumentSelectedInTreeGui != TradingSystemConstants.ALL_INSTRUMENTS_SELECTED)
            {


                //List<ContractList> contractListOfRollovers = new List<ContractList>();


                //foreach (var cl in
                //    optionSpreadManager.instrumentRollIntoSummary[DataCollectionLibrary.contractSummaryInstrumentSelectedIdx]
                //        .contractHashTable)
                //{
                //    // cl.Value.cqgsymbol\
                //    //TSErrorCatch.debugWriteOut(cl.Value.cqgsymbol);
                //    if (cl.Value.currentlyRollingContract
                //        && cl.Value.numberOfContracts != 0)
                //    {
                //        contractListOfRollovers.Add(cl.Value);
                //    }
                //}


                //OptionPLChart scPrice = new OptionPLChart(optionArrayTypes, optionSpreadManager);
                OptionPayoffChart scPrice = new OptionPayoffChart();
                scPrice.optionPLChartUserForm1.startupChart(optionArrayTypes, optionSpreadManager);

                List<int> contractsInOrders = new List<int>();

                //for (int expressionCount = 0; expressionCount < optionSpreadExpressionList.Count; expressionCount++)
                //{
                //    if (DataCollectionLibrary.orderSummaryList.Contains(optionSpreadExpressionList[expressionCount].cqgsymbol))
                //    {
                //        contractsInOrders.Add(expressionCount);
                //    }
                //}

                Instrument_mongo im =
                    DataCollectionLibrary
                        .instrumentHashTable_keyinstrumentid[DataCollectionLibrary.instrumentSelectedInTreeGui];

                scPrice.optionPLChartUserForm1.fillGrid(
                    //optionSpreadManager, contractsInOrders,
                    //optionSpreadExpressionList, 
                    im,
                    //0, 
                    OptionPLChartUserForm.PAYOFF_CHART_TYPE.ORDER_SUMMARY_PAYOFF
                    //, null
                    );

                scPrice.optionPLChartUserForm1.fillChart();

                scPrice.Show();
            }
        }

        private void tsbtnContractSummaryPayoffChart_Click(object sender, EventArgs e)
        {
            if (DataCollectionLibrary.instrumentSelectedInTreeGui != TradingSystemConstants.ALL_INSTRUMENTS_SELECTED)
            {
                double rRisk = 0;

                //OptionPLChart scPrice = new OptionPLChart(optionArrayTypes, optionSpreadManager);

                OptionPayoffChart scPrice = new OptionPayoffChart();
                scPrice.optionPLChartUserForm1.startupChart(optionArrayTypes, optionSpreadManager);

                scPrice.optionPLChartUserForm1.fillGridFromContractSummary(
                    optionSpreadManager, null,
                    //contractSummaryExpressionListIdx,
                    optionSpreadExpressionList,
                    DataCollectionLibrary.instrumentHashTable_keyinstrumentid[DataCollectionLibrary.instrumentSelectedInTreeGui],
                    rRisk,
                    OptionPLChartUserForm.PAYOFF_CHART_TYPE.CONTRACT_SUMMARY_PAYOFF);

                scPrice.optionPLChartUserForm1.fillChart();

                scPrice.Show();
            }
        }

        public void fillInstrumentSummary(int row, int col, double value)
        {
            try
            {
                if (this.InvokeRequired)
                {

                    ThreadSafeUpdateInstrumentSummaryDelegate d = new ThreadSafeUpdateInstrumentSummaryDelegate(threadSafeUpdateInstrumentSummary);

                    this.Invoke(d, row, col, value);
                }
                else
                {
                    threadSafeUpdateInstrumentSummary(row, col, value);
                }
            }
            catch (Exception ex)
            {
                TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
            }
        }

        private void threadSafeUpdateInstrumentSummary(int row, int col, double val)
        {
            try
            {

                instrumentSummaryGrid.Rows[row].Cells[col].Value = val;

            }
            catch (Exception ex)
            {
                TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
            }
        }

        private void makePortfolioCSVFilesForCOREToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //TODO THIS NEEDS FIXING
            //CMEMarginCall cmeMarginCall = new CMEMarginCall(optionSpreadExpressionList,
            //    this, DataTotalLibrary.portfolioSpreadTotals, FCM_DataImportLibrary.FCM_Import_Consolidated, true);

            //cmeMarginCall.generateMarginRequest();
        }

        public void updateButtonText(ToolStripButton toolStripBtn, string buttonText)
        {

            if (this.InvokeRequired)
            {

                ThreadSafeUpdateButtonText d = new ThreadSafeUpdateButtonText(threadSafeUpdateHideUnhideButtonText);

                this.Invoke(d, buttonText);
            }
            else
            {
                threadSafeUpdateHideUnhideButtonText(toolStripBtn, buttonText);
            }

        }


        private void threadSafeUpdateHideUnhideButtonText(ToolStripButton toolStripBtn, string buttonText)
        {

            toolStripBtn.Text = buttonText;
        }

        private void toolStripBtnADMPayoff_Click(object sender, EventArgs e)
        {
            if (DataCollectionLibrary.instrumentSelectedInTreeGui != TradingSystemConstants.ALL_INSTRUMENTS_SELECTED)
            {
                double rRisk = 0;

                //OptionPLChart scPrice = new OptionPLChart(optionArrayTypes, optionSpreadManager);
                OptionPayoffChart scPrice = new OptionPayoffChart();
                scPrice.optionPLChartUserForm1.startupChart(optionArrayTypes, optionSpreadManager);


                scPrice.optionPLChartUserForm1.fillGridWithFCMData(
                    optionSpreadManager, //null, null, 
                    FCM_DataImportLibrary.FCM_Import_Consolidated,
                    //optionSpreadExpressionList, 
                    DataCollectionLibrary.instrumentHashTable_keyinstrumentid[DataCollectionLibrary.instrumentSelectedInTreeGui],
                    rRisk, false);
                //OptionPLChart.PAYOFF_CHART_TYPE.CONTRACT_SUMMARY_PAYOFF);

                scPrice.optionPLChartUserForm1.fillChart();

                scPrice.Show();
            }
        }



        private void btnIncludeExcludeOrders_Click(object sender, EventArgs e)
        {
            //exclude include orders in model adm compare

            optionSpreadManager.includeExcludeOrdersInModelADMCompare = !optionSpreadManager.includeExcludeOrdersInModelADMCompare;

            if (optionSpreadManager.includeExcludeOrdersInModelADMCompare)
            {
                updateButtonText(btnIncludeExcludeOrders, "EXCLUDE ORDERS");
            }
            else
            {
                updateButtonText(btnIncludeExcludeOrders, "INCLUDE ORDERS");
            }





            optionSpreadManager.modelADMCompareCalculationAndDisplay.fillGridModelADMComparison(this);
        }



        private void gridViewModelADMCompare_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == (int)ADM_MODEL_POSITION_COMPARE_FIELDS_DISPLAYED.ZERO_PRICE)
            {

                bool zeroPrice = Convert.ToBoolean(gridViewModelADMCompare.Rows[e.RowIndex].Cells[e.ColumnIndex].Value);

                //optionSpreadManager.admPositionImportWebListForCompare[e.RowIndex].exclude = exclude;

                if (zeroPrice)
                {
                    optionSpreadManager.zeroPriceContractList.Add(FCM_DataImportLibrary.FCM_PostionList_forCompare[e.RowIndex].asset.cqgsymbol);
                    optionSpreadManager.exceptionContractList.Remove(FCM_DataImportLibrary.FCM_PostionList_forCompare[e.RowIndex].asset.cqgsymbol);
                }
                else
                {
                    optionSpreadManager.zeroPriceContractList.Remove(FCM_DataImportLibrary.FCM_PostionList_forCompare[e.RowIndex].asset.cqgsymbol);
                }

                optionSpreadManager.modelADMCompareCalculationAndDisplay.setBackgroundZeroPrice_ModelADMCompare(this, e.RowIndex, e.ColumnIndex);

                //optionSpreadManager.aDMDataCommonMethods.saveADMStrategyInfo(optionSpreadManager);
            }
            else if (e.ColumnIndex == (int)ADM_MODEL_POSITION_COMPARE_FIELDS_DISPLAYED.EXCEPTIONS)
            {

                bool exception = Convert.ToBoolean(gridViewModelADMCompare.Rows[e.RowIndex].Cells[e.ColumnIndex].Value);

                //optionSpreadManager.admPositionImportWebListForCompare[e.RowIndex].exclude = exclude;

                if (exception)
                {
                    optionSpreadManager.exceptionContractList.Add(FCM_DataImportLibrary.FCM_PostionList_forCompare[e.RowIndex].asset.cqgsymbol);
                    optionSpreadManager.zeroPriceContractList.Remove(FCM_DataImportLibrary.FCM_PostionList_forCompare[e.RowIndex].asset.cqgsymbol);
                }
                else
                {
                    optionSpreadManager.exceptionContractList.Remove(FCM_DataImportLibrary.FCM_PostionList_forCompare[e.RowIndex].asset.cqgsymbol);
                }

                optionSpreadManager.modelADMCompareCalculationAndDisplay.setBackgroundZeroPrice_ModelADMCompare(this, e.RowIndex, e.ColumnIndex);


            }

            optionSpreadManager.aDMDataCommonMethods.saveADMStrategyInfo(optionSpreadManager);

            //TSErrorCatch.debugWriteOut(e.ColumnIndex + "  " + e.RowIndex + " " + sender.ToString());


        }

        private void gridViewModelADMCompare_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            gridViewModelADMCompare.EndEdit();
        }

        private void dateTimePreviousPL_ValueChanged(object sender, EventArgs e)
        {

        }


        private void hideUnhideInstrumentsWithoutPositionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            hideUnhideInstrumentsInSummaryPL = !hideUnhideInstrumentsInSummaryPL;

            if (hideUnhideInstrumentsInSummaryPL)
            {
                for (int i = 0; i < instruments.Count(); i++)
                {
                    portfolioSummaryGrid.Columns[i].Visible = true;
                }
            }
            else
            {
                for (int instrumentCnt = 0; instrumentCnt < instruments.Count(); instrumentCnt++)
                {

                    //bool showInstrument = false;

                    //int expressionCnt = 0;

                    //while (expressionCnt < optionSpreadExpressionList.Count)
                    //{
                    //    if (optionSpreadExpressionList[expressionCnt].optionExpressionType
                    //        != OPTION_EXPRESSION_TYPES.OPTION_EXPRESSION_RISK_FREE_RATE
                    //        &&
                    //        optionSpreadExpressionList[expressionCnt].instrument.idinstrument == instruments[instrumentCnt].idinstrument)
                    //    {

                    //        if (optionSpreadExpressionList[expressionCnt].numberOfLotsHeldForContractSummary != 0
                    //            || optionSpreadExpressionList[expressionCnt].numberOfOrderContracts != 0)
                    //        {
                    //            showInstrument = true;
                    //            break;
                    //        }
                    //    }

                    //    expressionCnt++;
                    //}

                    //if (!showInstrument)
                    //{
                    //    int admWebPositionCounter = 0;

                    //    while (admWebPositionCounter < FCM_DataImportLibrary.FCM_Import_Consolidated.Count)
                    //    {
                    //        if (FCM_DataImportLibrary.FCM_Import_Consolidated[admWebPositionCounter]
                    //            .instrument.idinstrument == instruments[instrumentCnt].idinstrument)
                    //        {
                    //            showInstrument = true;
                    //            break;
                    //        }

                    //        admWebPositionCounter++;
                    //    }
                    //}

                    //if (showInstrument)
                    //{
                    //    portfolioSummaryGrid.Columns[instrumentCnt].Visible = true;
                    //}
                    //else
                    //{
                    //    portfolioSummaryGrid.Columns[instrumentCnt].Visible = false;
                    //}
                }

            }

        }

        private void dataGridViewExpressionList_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == (int)EXPRESSION_LIST_VIEW.MANUAL_STTLE)
            {

                //bool zeroPrice = Convert.ToBoolean(gridViewModelADMCompare.Rows[e.RowIndex].Cells[e.ColumnIndex].Value);

                bool x = Convert.ToBoolean(dataGridViewExpressionList
                                .Rows[e.RowIndex]
                                .Cells[e.ColumnIndex].Value);

                if (x)
                {
                    dataGridViewExpressionList.Rows[e.RowIndex].Cells[(int)EXPRESSION_LIST_VIEW.STTLE].ReadOnly = false;

                    if (dataGridViewExpressionList.Rows[e.RowIndex].Cells[(int)EXPRESSION_LIST_VIEW.STTLE].Value == null)
                    {
                        dataGridViewExpressionList.Rows[e.RowIndex].Cells[(int)EXPRESSION_LIST_VIEW.STTLE].Value = 0;
                    }

                }
                else
                {
                    dataGridViewExpressionList.Rows[e.RowIndex].Cells[(int)EXPRESSION_LIST_VIEW.STTLE].ReadOnly = true;

                    int idx = Convert.ToInt16(
                            dataGridViewExpressionList
                                .Rows[e.RowIndex].Cells[(int)EXPRESSION_LIST_VIEW.EXPRESSION_IDX].Value);

                    optionSpreadExpressionList[idx].manuallyFilled = false;
                    optionSpreadExpressionList[idx].settlementFilled = false;
                    optionSpreadExpressionList[idx].settlementIsCurrentDay = false;

                    optionSpreadExpressionList[idx].setSubscriptionLevel = false;

                    optionSpreadManager.updateEODMonitorDataSettings(true);

                    //dataGridViewExpressionList.Rows[e.RowIndex].Cells[(int)EXPRESSION_LIST_VIEW.STTLE].Value = "###";
                }
            }
            else if (e.ColumnIndex == (int)EXPRESSION_LIST_VIEW.STTLE)
            {
                if (dataGridViewExpressionList.Rows[e.RowIndex].Cells[(int)EXPRESSION_LIST_VIEW.STTLE].Value != null)
                {
                    int idx = Convert.ToInt16(
                            dataGridViewExpressionList
                                .Rows[e.RowIndex].Cells[(int)EXPRESSION_LIST_VIEW.EXPRESSION_IDX].Value);

                    optionSpreadExpressionList[idx].settlement =
                        Convert.ToDouble(
                            dataGridViewExpressionList
                                .Rows[e.RowIndex].Cells[(int)EXPRESSION_LIST_VIEW.STTLE].Value);

                    optionSpreadExpressionList[idx].manuallyFilled = true;
                    optionSpreadExpressionList[idx].settlementFilled = true;
                    optionSpreadExpressionList[idx].settlementIsCurrentDay = true;

                    optionSpreadExpressionList[idx].setSubscriptionLevel = false;

                    optionSpreadManager.updateEODMonitorDataSettings(true);
                }
            }
        }

        private void dataGridViewExpressionList_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == (int)EXPRESSION_LIST_VIEW.MANUAL_STTLE)
            {
                dataGridViewExpressionList.EndEdit();
            }
        }



        private void testToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (optionSpreadManager.stageOrdersLibrary == null)
            {
                optionSpreadManager.stageOrdersLibrary = new StageOrdersToTTWPFLibrary.FixOrderStagingController();

                optionSpreadManager.fxce = optionSpreadManager.stageOrdersLibrary.initializeFixSetup(
                    DataCollectionLibrary.initializationParms.portfolioGroupName);

                optionSpreadManager.fxce.CallAlert += new StageOrdersToTTWPFLibrary.FixConnectionEvent.AlertEventHandler(trigger_CallAlert);

                //optionSpreadManager.stageOrdersLibrary.


            }
        }

        void trigger_CallAlert(object sender, StageOrdersToTTWPFLibrary.AlertEventArgs e)
        {
            DataCollectionLibrary._fxceConnected = e.fixConn;
        }



        private void tsbtnStageOrders_Click(object sender, EventArgs e)
        {

            //if (optionSpreadManager.contractSummaryInstrumentSelectedIdx != instruments.Count()
            //    && optionSpreadManager.stageOrdersLibrary != null)
            if (optionSpreadManager.stageOrdersLibrary != null)
            {


                List<StageOrdersToTTWPFLibrary.Model.OrderModel> contractListToStage = new List<StageOrdersToTTWPFLibrary.Model.OrderModel>();


                //foreach (var cl in
                //    optionSpreadManager.instrumentRollIntoSummary[DataCollectionLibrary.contractSummaryInstrumentSelectedIdx]
                //        .contractHashTable)
                //{
                //    // cl.Value.cqgsymbol\
                //    //TSErrorCatch.debugWriteOut(cl.Value.cqgsymbol);
                //    if (cl.Value.currentlyRollingContract
                //        && cl.Value.numberOfContracts != 0)
                //    {

                //        for (int acctCnt = 0; acctCnt < DataCollectionLibrary.portfolioGroupAllocationChosen.Count; acctCnt++)
                //        {

                //            StageOrdersToTTWPFLibrary.Model.OrderModel orderModel = null;

                //            int cltsCount = 0;
                //            bool contractAlreadyInList = false;

                //            while (cltsCount < contractListToStage.Count)
                //            {
                //                if (contractListToStage[cltsCount].cqgsymbol.CompareTo(cl.Value.cqgsymbol) == 0
                //                    && contractListToStage[cltsCount].broker_18220.CompareTo(
                //                        DataCollectionLibrary.portfolioAllocation.accountAllocation[
                //                        DataCollectionLibrary.portfolioGroupAllocationChosen[acctCnt]].broker) == 0
                //                    && contractListToStage[cltsCount].acct.CompareTo(
                //                        optionSpreadManager.selectAcct(orderModel.underlyingExchangeSymbol,
                //                        DataCollectionLibrary.portfolioAllocation.accountAllocation[
                //                        DataCollectionLibrary.portfolioGroupAllocationChosen[acctCnt]], false)) == 0
                //                    )
                //                {
                //                    orderModel = contractListToStage[cltsCount];

                //                    contractAlreadyInList = true;

                //                    break;
                //                }

                //                cltsCount++;
                //            }

                //            if (!contractAlreadyInList)
                //            {
                //                orderModel =
                //                    new StageOrdersToTTWPFLibrary.Model.OrderModel();


                //                orderModel.cqgsymbol = cl.Value.cqgsymbol;
                //                orderModel.optionMonthInt = cl.Value.optionMonthInt;
                //                orderModel.optionYear = cl.Value.optionYear;
                //                orderModel.optionStrikePrice =
                //                    (decimal)ConversionAndFormatting.convertToStrikeForTT(cl.Value.strikePrice,
                //                    instruments[cl.Value.indexOfInstrumentInInstrumentsArray].optionstrikeincrement,
                //                    instruments[cl.Value.indexOfInstrumentInInstrumentsArray].optionstrikedisplayTT,
                //                    instruments[cl.Value.indexOfInstrumentInInstrumentsArray].idinstrument);

                //                orderModel.contractMonthint = cl.Value.contractMonthInt;
                //                orderModel.contractYear = cl.Value.contractYear;

                //                //orderModel.expirationDate = cl.Value.expirationDate;



                //                orderModel.underlyingExchange =
                //                    instruments[cl.Value.indexOfInstrumentInInstrumentsArray].tradingTechnologiesExchange;

                //                orderModel.underlyingGateway =
                //                    instruments[cl.Value.indexOfInstrumentInInstrumentsArray].tradingTechnologiesGateway;

                //                orderModel.lotsTotal += cl.Value.numberOfContracts;

                //                //orderModel.orderQty = Math.Abs(cl.Value.numberOfContracts);


                //                //this is a rolled contract so don't send price with this

                //                if (cl.Value.contractType == OPTION_SPREAD_CONTRACT_TYPE.FUTURE)
                //                {
                //                    orderModel.securityType = StageOrdersToTTWPFLibrary.Enums.SECURITY_TYPE.FUTURE;

                //                    orderModel.underlyingExchangeSymbol =
                //                        instruments[cl.Value.indexOfInstrumentInInstrumentsArray].exchangesymbolTT;

                //                    orderModel.maturityMonthYear =
                //                            new DateTime(orderModel.contractYear, orderModel.contractMonthint, 1)
                //                            .ToString("yyyyMM", DateTimeFormatInfo.InvariantInfo);
                //                }
                //                else
                //                {
                //                    orderModel.securityType = StageOrdersToTTWPFLibrary.Enums.SECURITY_TYPE.OPTION;

                //                    orderModel.underlyingExchangeSymbol =
                //                        instruments[cl.Value.indexOfInstrumentInInstrumentsArray].optionexchangesymbolTT;

                //                    orderModel.maturityMonthYear =
                //                            new DateTime(orderModel.optionYear, orderModel.optionMonthInt, 1)
                //                                .ToString("yyyyMM", DateTimeFormatInfo.InvariantInfo);

                //                    if (cl.Value.contractType == OPTION_SPREAD_CONTRACT_TYPE.CALL)
                //                    {
                //                        orderModel.optionType = StageOrdersToTTWPFLibrary.Enums.OPTION_TYPE.CALL;
                //                    }
                //                    else
                //                    {
                //                        orderModel.optionType = StageOrdersToTTWPFLibrary.Enums.OPTION_TYPE.PUT;
                //                    }


                //                }

                //                orderModel.broker_18220 =
                //                    DataCollectionLibrary.portfolioAllocation.accountAllocation[
                //                        DataCollectionLibrary.portfolioGroupAllocationChosen[acctCnt]].broker;

                //                orderModel.acct =
                //                    optionSpreadManager.selectAcct(orderModel.underlyingExchangeSymbol,
                //                        DataCollectionLibrary.portfolioAllocation.accountAllocation[
                //                        DataCollectionLibrary.portfolioGroupAllocationChosen[acctCnt]], false);
                //            }
                //            else //contractAlreadyInList
                //            {
                //                orderModel.lotsTotal += cl.Value.numberOfContracts;
                //            }

                //            orderModel.orderQty = Math.Abs(orderModel.lotsTotal)
                //                * DataCollectionLibrary.portfolioAllocation.accountAllocation[
                //                        DataCollectionLibrary.portfolioGroupAllocationChosen[acctCnt]].multiple;

                //            if (orderModel.lotsTotal > 0)
                //            {
                //                orderModel.side = StageOrdersToTTWPFLibrary.Enums.Side.Buy;
                //            }
                //            else
                //            {
                //                orderModel.side = StageOrdersToTTWPFLibrary.Enums.Side.Sell;
                //            }

                //            orderModel.stagedOrderMessage = "ROLL TRIGGER ORDER";


                //            string instrumentSpecificFieldKey = optionSpreadManager.getInstrumentSpecificFieldKey(orderModel);


                //            if (DataCollectionLibrary.instrumentSpecificFIXFieldHashSet.ContainsKey(instrumentSpecificFieldKey))
                //            {
                //                InstrumentSpecificFIXFields instrumentSpecificFIXFields
                //                    = DataCollectionLibrary.instrumentSpecificFIXFieldHashSet[instrumentSpecificFieldKey];

                //                orderModel.TAG47_Rule80A.useTag = true;
                //                orderModel.TAG47_Rule80A.tagCharValue = instrumentSpecificFIXFields.TAG_47_Rule80A;

                //                orderModel.TAG204_CustomerOrFirm.useTag = true;
                //                orderModel.TAG204_CustomerOrFirm.tagIntValue = instrumentSpecificFIXFields.TAG_204_CustomerOrFirm;

                //                orderModel.TAG18205_TTAccountType.useTag = true;
                //                orderModel.TAG18205_TTAccountType.tagStringValue = instrumentSpecificFIXFields.TAG_18205_TTAccountType;

                //                orderModel.TAG440_ClearingAccount.useTag = true;
                //                orderModel.TAG440_ClearingAccount.tagStringValue = instrumentSpecificFIXFields.TAG_440_ClearingAccount;

                //                orderModel.TAG16102_FFT2.useTag = true;
                //                orderModel.TAG16102_FFT2.tagStringValue = orderModel.acct;
                //            }

                //            //*****************************
                //            orderModel.orderPrice = 0;


                //            orderModel.orderPlacementType = cmbxOrderPlacementType.SelectedIndex;
                //            //*****************************

                //            contractListToStage.Add(orderModel);
                //        }

                //    }
                //}

                for (int expressionCount = 0; expressionCount < optionSpreadExpressionList.Count; expressionCount++)
                {
                    //TSErrorCatch.debugWriteOut(
                    //    optionSpreadExpressionList[expressionCount].cqgsymbol + " "
                    //    +
                    //     orderSummarySelectedItems[0].SubItems[1].ToString());

                    if (DataCollectionLibrary.orderSummaryList.Contains(optionSpreadExpressionList[expressionCount].asset.cqgsymbol))
                    {

                        //for (int acctCnt = 0; acctCnt < DataCollectionLibrary.portfolioGroupAllocationChosen.Count; acctCnt++)
                        foreach (AccountAllocation ac in DataCollectionLibrary.portfolioAllocation.accountAllocation)
                        {

                            StageOrdersToTTWPFLibrary.Model.OrderModel orderModel =
                                new StageOrdersToTTWPFLibrary.Model.OrderModel();

                            int cltsCount = 0;
                            bool contractAlreadyInList = false;

                            while (cltsCount < contractListToStage.Count)
                            {
                                if (contractListToStage[cltsCount].cqgsymbol.CompareTo(
                                        optionSpreadExpressionList[expressionCount].asset.cqgsymbol) == 0
                                    && contractListToStage[cltsCount].broker_18220.CompareTo(
                                        ac.broker) == 0
                                    && contractListToStage[cltsCount].acct.CompareTo(
                                        optionSpreadManager.selectAcct(orderModel.underlyingExchangeSymbol,
                                        ac, false)) == 0
                                    )
                                {
                                    orderModel = contractListToStage[cltsCount];

                                    contractAlreadyInList = true;

                                    break;
                                }

                                cltsCount++;
                            }

                            if (!contractAlreadyInList)
                            {

                                orderModel.cqgsymbol = optionSpreadExpressionList[expressionCount].asset.cqgsymbol;
                                orderModel.optionMonthInt = optionSpreadExpressionList[expressionCount].optionMonthInt;

                                orderModel.optionYear = optionSpreadExpressionList[expressionCount].optionYear;
                                orderModel.optionStrikePrice =
                                    (decimal)ConversionAndFormatting.convertToStrikeForTT(
                                    optionSpreadExpressionList[expressionCount].asset.strikeprice,
                                    optionSpreadExpressionList[expressionCount].instrument.optionstrikeincrement,
                                    optionSpreadExpressionList[expressionCount].instrument.optionstrikedisplayTT,
                                    optionSpreadExpressionList[expressionCount].instrument.idinstrument);

                                orderModel.contractMonthint = optionSpreadExpressionList[expressionCount].futureContractMonthInt;
                                orderModel.contractYear = optionSpreadExpressionList[expressionCount].futureContractYear;

                                //orderModel.expirationDate = optionSpreadExpressionList[expressionCount].con;



                                orderModel.underlyingExchange =
                                    optionSpreadExpressionList[expressionCount].instrument.exchange.tradingtechnologies_exchange;

                                orderModel.underlyingGateway =
                                    optionSpreadExpressionList[expressionCount].instrument.exchange.tradingtechnologies_gateway;

                                //TODO FIX THIS NUMBER OF ORDERS
                                orderModel.lotsTotal += 0; // optionSpreadExpressionList[expressionCount].numberOfOrderContracts;

                                //orderModel.orderQty = Math.Abs(optionSpreadExpressionList[expressionCount].numberOfOrderContracts);

                                //orderModel.orderPrice = cl.Value.;

                                if (optionSpreadExpressionList[expressionCount].callPutOrFuture == OPTION_SPREAD_CONTRACT_TYPE.FUTURE)
                                {
                                    orderModel.securityType = StageOrdersToTTWPFLibrary.Enums.SECURITY_TYPE.FUTURE;

                                    orderModel.underlyingExchangeSymbol =
                                        optionSpreadExpressionList[expressionCount].asset.productcode;
                                        //optionSpreadExpressionList[expressionCount].instrument.exchangesymbolTT;

                                    orderModel.maturityMonthYear =
                                        new DateTime(orderModel.contractYear, orderModel.contractMonthint, 1)
                                            .ToString("yyyyMM", DateTimeFormatInfo.InvariantInfo);

                                }
                                else
                                {
                                    orderModel.securityType = StageOrdersToTTWPFLibrary.Enums.SECURITY_TYPE.OPTION;

                                    orderModel.underlyingExchangeSymbol =
                                        optionSpreadExpressionList[expressionCount].asset.productcode;
                                        //optionSpreadExpressionList[expressionCount].instrument.optionexchangesymbolTT;

                                    orderModel.maturityMonthYear =
                                        new DateTime(orderModel.optionYear, orderModel.optionMonthInt, 1)
                                            .ToString("yyyyMM", DateTimeFormatInfo.InvariantInfo);

                                    if (optionSpreadExpressionList[expressionCount].callPutOrFuture == OPTION_SPREAD_CONTRACT_TYPE.CALL)
                                    {
                                        orderModel.optionType = StageOrdersToTTWPFLibrary.Enums.OPTION_TYPE.CALL;
                                    }
                                    else
                                    {
                                        orderModel.optionType = StageOrdersToTTWPFLibrary.Enums.OPTION_TYPE.PUT;
                                    }

                                }

                                orderModel.broker_18220 =
                                    ac.broker;

                                orderModel.acct =
                                    optionSpreadManager.selectAcct(orderModel.underlyingExchangeSymbol,
                                        ac, false);

                            }
                            else
                            {

                                //TODO FIX THIS NUMBER OF ORDERS
                                orderModel.lotsTotal += 0;// optionSpreadExpressionList[expressionCount].numberOfOrderContracts;
                            }

                            orderModel.orderQty = Math.Abs(orderModel.lotsTotal);
                            //* ac.multiple;


                            if (orderModel.lotsTotal > 0)
                            {
                                orderModel.side = StageOrdersToTTWPFLibrary.Enums.Side.Buy;
                            }
                            else
                            {
                                orderModel.side = StageOrdersToTTWPFLibrary.Enums.Side.Sell;
                            }

                            orderModel.orderPrice = findLimitPrice(
                                        orderModel.side,
                                        optionSpreadExpressionList[expressionCount]);

                            orderModel.stagedOrderMessage = "SIGNAL TRIGGER ORDER";



                            string instrumentSpecificFieldKey = optionSpreadManager.getInstrumentSpecificFieldKey(orderModel);


                            if (DataCollectionLibrary.instrumentSpecificFIXFieldHashSet.ContainsKey(instrumentSpecificFieldKey))
                            {
                                InstrumentSpecificFIXFields instrumentSpecificFIXFields
                                    = DataCollectionLibrary.instrumentSpecificFIXFieldHashSet[instrumentSpecificFieldKey];

                                orderModel.TAG47_Rule80A.useTag = true;
                                orderModel.TAG47_Rule80A.tagCharValue = instrumentSpecificFIXFields.TAG_47_Rule80A;

                                orderModel.TAG204_CustomerOrFirm.useTag = true;
                                orderModel.TAG204_CustomerOrFirm.tagIntValue = instrumentSpecificFIXFields.TAG_204_CustomerOrFirm;

                                orderModel.TAG18205_TTAccountType.useTag = true;
                                orderModel.TAG18205_TTAccountType.tagStringValue = instrumentSpecificFIXFields.TAG_18205_TTAccountType;

                                orderModel.TAG440_ClearingAccount.useTag = true;
                                orderModel.TAG440_ClearingAccount.tagStringValue = instrumentSpecificFIXFields.TAG_440_ClearingAccount;

                                orderModel.TAG16102_FFT2.useTag = true;
                                orderModel.TAG16102_FFT2.tagStringValue = orderModel.acct;
                            }


                            orderModel.orderPlacementType = cmbxOrderPlacementType.SelectedIndex;

                            contractListToStage.Add(orderModel);
                        }
                    }
                }


                if (contractListToStage.Count > 0)
                {
                    optionSpreadManager.stageOrdersLibrary.stageOrders(contractListToStage);
                }

            }
        }

        private decimal findLimitPrice(StageOrdersToTTWPFLibrary.Enums.Side side,
            MongoDB_OptionSpreadExpression optionSpreadExpression)
        //double price, double tickSize, double tickDisplay)
        {
            decimal priceToReturn = 0;
            double offsetPrice = 0;
            int tickOffset = Convert.ToInt16(tsbtnOrderTickOffset.Text);

            double tickSize = 0;
            double tickDisplay = 0;

            bool isOptionContract = true;
            //double tickDisplayTT_multiplier = 1;

            if (optionSpreadExpression.callPutOrFuture == OPTION_SPREAD_CONTRACT_TYPE.FUTURE)
            {
                tickSize = optionSpreadExpression.instrument.ticksize;
                tickDisplay = optionSpreadExpression.instrument.tickdisplay;

                isOptionContract = false;
                //tickDisplayTT_multiplier = optionSpreadExpression.instrument.tickDisplayTT;
            }
            //else
            //{
            //    tickSize = optionSpreadExpression.instrument.optionticksize;
            //    tickDisplay = optionSpreadExpression.instrument.optiontickdisplay;

            //    //tickDisplayTT_multiplier = optionSpreadExpression.instrument.optiontickdisplayTT;
            //}

            if (side == StageOrdersToTTWPFLibrary.Enums.Side.Buy)
            {
                if (optionSpreadExpression.bidFilled)
                {
                    offsetPrice = optionSpreadExpression.bid;
                }
                else
                {
                    offsetPrice = optionSpreadExpression.defaultPrice;
                }

                if (isOptionContract)
                {
                    tickSize = OptionSpreadManager.chooseoptionticksize(
                                    offsetPrice,
                                    optionSpreadExpression.instrument.optionticksize,
                                    optionSpreadExpression.instrument.secondaryoptionticksize,
                                    optionSpreadExpression.instrument.secondaryoptionticksizerule);

                    tickDisplay = OptionSpreadManager.chooseoptionticksize(
                                offsetPrice,
                                optionSpreadExpression.instrument.optiontickdisplay,
                                optionSpreadExpression.instrument.secondaryoptiontickdisplay,
                                optionSpreadExpression.instrument.secondaryoptionticksizerule);
                }

                offsetPrice = offsetPrice - tickOffset * tickSize;
            }
            else
            {
                if (optionSpreadExpression.askFilled)
                {
                    offsetPrice = optionSpreadExpression.ask;
                }
                else
                {
                    offsetPrice = optionSpreadExpression.defaultPrice;
                }

                if (isOptionContract)
                {
                    tickSize = OptionSpreadManager.chooseoptionticksize(
                                    offsetPrice,
                                    optionSpreadExpression.instrument.optionticksize,
                                    optionSpreadExpression.instrument.secondaryoptionticksize,
                                    optionSpreadExpression.instrument.secondaryoptionticksizerule);

                    tickDisplay = OptionSpreadManager.chooseoptionticksize(
                                offsetPrice,
                                optionSpreadExpression.instrument.optiontickdisplay,
                                optionSpreadExpression.instrument.secondaryoptiontickdisplay,
                                optionSpreadExpression.instrument.secondaryoptionticksizerule);
                }

                offsetPrice = offsetPrice + tickOffset * tickSize;
            }

            priceToReturn =
                    Convert.ToDecimal(ConversionAndFormatting.convertToOrderPriceForTT(
                        offsetPrice,
                        tickSize,
                        tickDisplay, optionSpreadExpression.instrument.idinstrument, isOptionContract));

            if (priceToReturn < 0)
            {
                priceToReturn = 0;
            }

            return priceToReturn;

        }

        private void stageOrdersFromExpressionList_Click(object sender, EventArgs e)
        {
            sendOrderOrSecDefRequest(true);
        }

        private void sendOrderOrSecDefRequest(bool sendOrder)
        {

            if (optionSpreadManager.stageOrdersLibrary != null)
            {
                List<StageOrdersToTTWPFLibrary.Model.OrderModel> contractListToStage = new List<StageOrdersToTTWPFLibrary.Model.OrderModel>();


                for (int i = 0; i < dataGridViewExpressionList.RowCount; i++)
                {
                    if (dataGridViewExpressionList.Rows[i].Selected)
                    {
                        int expressionCount = Convert.ToInt16(dataGridViewExpressionList.Rows[i].Cells[(int)EXPRESSION_LIST_VIEW.EXPRESSION_IDX].Value);
                        //optionSpreadExpressionList[i]

                        //if (orderSummaryHashTable.Contains(optionSpreadExpressionList[expressionCount].cqgsymbol))

                        //for (int acctCnt = 0; acctCnt < DataCollectionLibrary.portfolioGroupAllocationChosen.Count; acctCnt++)
                        foreach (AccountAllocation ac in DataCollectionLibrary.portfolioAllocation.accountAllocation)
                        {
                            if (ac.visible)
                            {

                                StageOrdersToTTWPFLibrary.Model.OrderModel orderModel =
                                    new StageOrdersToTTWPFLibrary.Model.OrderModel();

                                orderModel.cqgsymbol = optionSpreadExpressionList[expressionCount].asset.cqgsymbol;
                                orderModel.optionMonthInt = optionSpreadExpressionList[expressionCount].optionMonthInt;

                                double sd = Convert.ToDouble(toolStripTextBoxStrikeDisplay.Text);
                                if (sd == 0)
                                {
                                    sd = optionSpreadExpressionList[expressionCount].instrument.optionstrikedisplayTT;
                                }

                                orderModel.optionYear = optionSpreadExpressionList[expressionCount].optionYear;
                                orderModel.optionStrikePrice =
                                    (decimal)ConversionAndFormatting.convertToStrikeForTT(
                                    optionSpreadExpressionList[expressionCount].asset.strikeprice,
                                    optionSpreadExpressionList[expressionCount].instrument.optionstrikeincrement,
                                    sd,
                                    optionSpreadExpressionList[expressionCount].instrument.idinstrument);

                                orderModel.contractMonthint = optionSpreadExpressionList[expressionCount].futureContractMonthInt;
                                orderModel.contractYear = optionSpreadExpressionList[expressionCount].futureContractYear;

                                //orderModel.expirationDate = optionSpreadExpressionList[expressionCount].con;

                                //orderModel.orderPrice = 
                                //    Convert.ToDecimal(ConversionAndFormatting.convertToTickMovesDouble(
                                //    optionSpreadExpressionList[expressionCount].defaultPrice,
                                //    optionSpreadExpressionList[expressionCount].instrument.ticksize,
                                //    optionSpreadExpressionList[expressionCount].instrument.tickDisplay));

                                orderModel.underlyingExchange =
                                    optionSpreadExpressionList[expressionCount].instrument.exchange.tradingtechnologies_exchange;

                                orderModel.underlyingGateway =
                                    optionSpreadExpressionList[expressionCount].instrument.exchange.tradingtechnologies_gateway;

                                int stageOrderLots = Convert.ToInt16(toolStripStageOrderContracts.Text);

                                orderModel.orderQty = Math.Abs(stageOrderLots);
                                //* ac.multiple;

                                //orderModel.orderPrice = cl.Value.;

                                if (optionSpreadExpressionList[expressionCount].callPutOrFuture == OPTION_SPREAD_CONTRACT_TYPE.FUTURE)
                                {
                                    orderModel.securityType = StageOrdersToTTWPFLibrary.Enums.SECURITY_TYPE.FUTURE;

                                    orderModel.underlyingExchangeSymbol =
                                        optionSpreadExpressionList[expressionCount].asset.productcode;
                                        //optionSpreadExpressionList[expressionCount].instrument.exchangesymbolTT;

                                    orderModel.maturityMonthYear =
                                        new DateTime(orderModel.contractYear, orderModel.contractMonthint, 1)
                                            .ToString("yyyyMM", DateTimeFormatInfo.InvariantInfo);

                                    //orderModel.orderPrice = 0;
                                    //Convert.ToDecimal(ConversionAndFormatting.convertToTickMovesDouble(
                                    //optionSpreadExpressionList[expressionCount].defaultPrice,
                                    //optionSpreadExpressionList[expressionCount].instrument.ticksize,
                                    //optionSpreadExpressionList[expressionCount].instrument.tickDisplay));
                                }
                                else
                                {
                                    orderModel.securityType = StageOrdersToTTWPFLibrary.Enums.SECURITY_TYPE.OPTION;

                                    orderModel.underlyingExchangeSymbol =
                                        optionSpreadExpressionList[expressionCount].asset.productcode;
                                        //optionSpreadExpressionList[expressionCount].instrument.optionexchangesymbolTT;

                                    orderModel.maturityMonthYear =
                                        new DateTime(orderModel.optionYear, orderModel.optionMonthInt, 1)
                                            .ToString("yyyyMM", DateTimeFormatInfo.InvariantInfo);

                                    if (optionSpreadExpressionList[expressionCount].callPutOrFuture == OPTION_SPREAD_CONTRACT_TYPE.CALL)
                                    {
                                        orderModel.optionType = StageOrdersToTTWPFLibrary.Enums.OPTION_TYPE.CALL;
                                    }
                                    else
                                    {
                                        orderModel.optionType = StageOrdersToTTWPFLibrary.Enums.OPTION_TYPE.PUT;
                                    }

                                    //orderModel.orderPrice = 0;
                                    //Convert.ToDecimal(ConversionAndFormatting.convertToTickMovesDouble(
                                    //optionSpreadExpressionList[expressionCount].defaultPrice,
                                    //optionSpreadExpressionList[expressionCount].instrument.optionticksize,
                                    //optionSpreadExpressionList[expressionCount].instrument.optiontickdisplay));

                                }

                                if (stageOrderLots > 0)
                                {
                                    orderModel.side = StageOrdersToTTWPFLibrary.Enums.Side.Buy;
                                }
                                else
                                {
                                    orderModel.side = StageOrdersToTTWPFLibrary.Enums.Side.Sell;
                                }

                                //orderModel.orderPrice = Convert.ToDecimal(toolStripTextBoxTickDisplay.Text);

                                orderModel.orderPrice = findLimitPrice(
                                        orderModel.side,
                                        optionSpreadExpressionList[expressionCount]);

                                //        Convert.ToDecimal(toolStripTextBoxTickDisplay.Text);

                                orderModel.stagedOrderMessage = "SIGNAL TRIGGER ORDER";


                                orderModel.broker_18220 = ac.broker;

                                orderModel.acct =
                                    //DataCollectionLibrary.portfolioAllocation.accountAllocation[
                                    //    optionSpreadManager.portfolioGroupAllocationChosen[acctCnt]].account;
                                    optionSpreadManager.selectAcct(orderModel.underlyingExchangeSymbol,
                                        ac, false);
                                //(String)(cmbxAcct.SelectedItem);

                                orderModel.orderPlacementType = cmbxOrderPlacementType.SelectedIndex;


                                string instrumentSpecificFieldKey = optionSpreadManager.getInstrumentSpecificFieldKey(orderModel);


                                if (DataCollectionLibrary.instrumentSpecificFIXFieldHashSet.ContainsKey(instrumentSpecificFieldKey))
                                {
                                    InstrumentSpecificFIXFields instrumentSpecificFIXFields
                                        = DataCollectionLibrary.instrumentSpecificFIXFieldHashSet[instrumentSpecificFieldKey];

                                    orderModel.TAG47_Rule80A.useTag = true;
                                    orderModel.TAG47_Rule80A.tagCharValue = instrumentSpecificFIXFields.TAG_47_Rule80A;

                                    orderModel.TAG204_CustomerOrFirm.useTag = true;
                                    orderModel.TAG204_CustomerOrFirm.tagIntValue = instrumentSpecificFIXFields.TAG_204_CustomerOrFirm;

                                    orderModel.TAG18205_TTAccountType.useTag = true;
                                    orderModel.TAG18205_TTAccountType.tagStringValue = instrumentSpecificFIXFields.TAG_18205_TTAccountType;

                                    orderModel.TAG440_ClearingAccount.useTag = true;
                                    orderModel.TAG440_ClearingAccount.tagStringValue = instrumentSpecificFIXFields.TAG_440_ClearingAccount;

                                    orderModel.TAG16102_FFT2.useTag = true;
                                    orderModel.TAG16102_FFT2.tagStringValue = orderModel.acct;
                                }


                                contractListToStage.Add(orderModel);
                            }
                        }
                    }
                }


                if (contractListToStage.Count > 0)
                {
                    if (sendOrder)
                    {
                        optionSpreadManager.stageOrdersLibrary.stageOrders(contractListToStage);
                    }
                    else
                    {
                        optionSpreadManager.stageOrdersLibrary.securityDefRequest(contractListToStage);
                    }
                }

                //DataGridViewSelectedRowCollection selectedRows = dataGridViewExpressionList.SelectedRows;


                //foreach (DataGridViewRow dataGridViewRow in selectedRows) 
                //{
                //    TSErrorCatch.debugWriteOut(dataGridViewRow.Cells[-1].Value.ToString());
                //}

                //TSErrorCatch.debugWriteOut("test");
            }
        }


        private void tsbtnFcmAndOrderPayoff_Click(object sender, EventArgs e)
        {
            if (DataCollectionLibrary.instrumentSelectedInTreeGui != TradingSystemConstants.ALL_INSTRUMENTS_SELECTED)
            {
                double rRisk = 0;

                //OptionPLChart scPrice = new OptionPLChart(optionArrayTypes, optionSpreadManager);
                OptionPayoffChart scPrice = new OptionPayoffChart();
                scPrice.optionPLChartUserForm1.startupChart(optionArrayTypes, optionSpreadManager);


                scPrice.optionPLChartUserForm1.fillGridWithFCMData(
                    optionSpreadManager, //null, null, 
                    FCM_DataImportLibrary.FCM_Import_Consolidated,
                    //optionSpreadExpressionList, 
                    DataCollectionLibrary.instrumentHashTable_keyinstrumentid[DataCollectionLibrary.instrumentSelectedInTreeGui],
                    rRisk, true);
                //OptionPLChart.PAYOFF_CHART_TYPE.CONTRACT_SUMMARY_PAYOFF);

                scPrice.optionPLChartUserForm1.fillChart();

                scPrice.Show();
            }


        }

        private void tsbtnModelFCMDiff_Click(object sender, EventArgs e)
        {
            if (DataCollectionLibrary.instrumentSelectedInTreeGui != TradingSystemConstants.ALL_INSTRUMENTS_SELECTED)
            {
                //OptionPLChart scPrice = new OptionPLChart(optionArrayTypes, optionSpreadManager);
                OptionPayoffChart scPrice = new OptionPayoffChart();
                scPrice.optionPLChartUserForm1.startupChart(optionArrayTypes, optionSpreadManager);


                scPrice.optionPLChartUserForm1.fillGridFromFCMModelDiff(optionSpreadManager,
                    DataCollectionLibrary.instrumentHashTable_keyinstrumentid[DataCollectionLibrary.instrumentSelectedInTreeGui],
                    false);
                //OptionPLChart.PAYOFF_CHART_TYPE.CONTRACT_SUMMARY_PAYOFF);

                scPrice.optionPLChartUserForm1.fillChart();

                scPrice.Show();
            }



        }

        private void tsbtnModelFCMOrder_Click(object sender, EventArgs e)
        {
            if (DataCollectionLibrary.instrumentSelectedInTreeGui != TradingSystemConstants.ALL_INSTRUMENTS_SELECTED)
            {
                OptionPayoffChart scPrice = new OptionPayoffChart();
                scPrice.optionPLChartUserForm1.startupChart(optionArrayTypes, optionSpreadManager);

                scPrice.optionPLChartUserForm1.fillGridFromFCMModelDiff(
                    optionSpreadManager,
                    DataCollectionLibrary.instrumentHashTable_keyinstrumentid[DataCollectionLibrary.instrumentSelectedInTreeGui],
                    true);

                scPrice.optionPLChartUserForm1.fillChart();

                scPrice.Show();
            }
        }

        private void btnCallAllInstruments_Click(object sender, EventArgs e)
        {
            optionSpreadManager.callOptionRealTimeData(false);
        }

        private void btnCallUnsubscribed_Click(object sender, EventArgs e)
        {
            optionSpreadManager.callOptionRealTimeData(true);
        }



        private void btnCQGRecon_Click(object sender, EventArgs e)
        {
            optionSpreadManager.reInitializeCQG();
        }

        delegate void ThreadSafeUpdateCQGReconnectBtn(bool enabled);





        public void threadSafeUpdateCQGConnectionStatus(String connectStatus, Color connColor,
            String connectShortStringStatus)
        {
            this.ConnectionStatus.Text = connectShortStringStatus;
            this.ConnectionStatus.ToolTipText = connectStatus;
            this.ConnectionStatus.ForeColor = connColor;
        }



        private void cmbxOrderPlacementType_SelectedIndexChanged(object sender, EventArgs e)
        {
            DataCollectionLibrary.initializationParms.FIX_OrderPlacementType =
                cmbxOrderPlacementType.SelectedIndex;

        }

        private void treeViewBrokerAcct_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode x = treeViewBrokerAcct.SelectedNode;

            if (x != null)
            {

                DataCollectionLibrary.portfolioAllocation.brokerAccountChosen = x.Index;

                foreach (AccountAllocation ac in DataCollectionLibrary.portfolioAllocation.accountAllocation)
                {
                    if (x.Index == DataCollectionLibrary.portfolioAllocation.accountAllocation.Count)
                    {
                        ac.visible = true;
                    }
                    else
                    {
                        if (x.Index == ac.acctIndex_UsedForTotals_Visibility)
                        {
                            ac.visible = true;
                        }
                        else
                        {
                            ac.visible = false;
                        }
                    }
                }


                //resetPortfolioGroupFcmOfficeAcctChosenHashSet();

                sendUpdateToPortfolioTotalGrid();

                //if (optionSpreadManager.realtimeMonitorSettings.eodAnalysis)

                sendUpdateToPortfolioTotalSettlementGrid();


                //optionSpreadManager.gridViewContractSummaryManipulation.fillContractSummary();

                //fillOrderSummaryList();

                updateSelectedInstrumentFromTree();
            }
        }

        /// <summary>
        /// this makes a hashset with a key FCM_POFFIC and FCM_PACCT to be referenced by FCM data
        /// </summary>
        //private void resetPortfolioGroupFcmOfficeAcctChosenHashSet()
        //{
        //    //DataCollectionLibrary.portfolioGroupFcmOfficeAcctChosenHashSet.Clear();

        //    //for (int portfolioGroupAllocationChosenCount = 0;
        //    //        portfolioGroupAllocationChosenCount < DataCollectionLibrary.portfolioGroupAllocationChosen.Count;
        //    //        portfolioGroupAllocationChosenCount++)
        //    //{

        //    //    int idxOfPortfolioGroup = DataCollectionLibrary.portfolioGroupAllocationChosen[portfolioGroupAllocationChosenCount];

        //    //    foreach (var fcmOfficAcctKeyValPair in DataCollectionLibrary.portfolioAllocation.FCM_POFFIC_PACCT_hashset)
        //    //    {
        //    //        FCM_POFFIC_PACCT fcmOfficAcct = fcmOfficAcctKeyValPair.Value;

        //    //        StringBuilder key = new StringBuilder();
        //    //        key.Append(fcmOfficAcct.FCM_POFFIC);
        //    //        key.Append(fcmOfficAcct.FCM_PACCT);

        //    //        DataCollectionLibrary.portfolioGroupFcmOfficeAcctChosenHashSet.TryAdd(key.ToString(),
        //    //            fcmOfficAcct);
        //    //    }
        //    //}
        //}



        private void orderSummaryGrid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {

            orderSummaryGrid_CellFormattingDelegate.Invoke(e,
                orderSummaryGrid, DataCollectionLibrary.orderSummaryDataTable, Color.LightBlue);


        }



        private delegate void OrderSummaryGridFormattingDelegate(DataGridViewCellFormattingEventArgs e, DataGridView dataGrid,
            DataTable dataTable, Color backColor);

        OrderSummaryGridFormattingDelegate orderSummaryGrid_CellFormattingDelegate
            = new OrderSummaryGridFormattingDelegate(orderSummaryGridFormatting);



        private static void orderSummaryGridFormatting(DataGridViewCellFormattingEventArgs e, DataGridView dataGrid,
            DataTable dataTable, Color backColor)
        {
            e.CellStyle.BackColor = backColor;

            if (dataGrid.Columns[e.ColumnIndex].Name == ORDER_SUMMARY_COLUMNS.CONTRACT.ToString())
            {
                dataGrid.Columns[e.ColumnIndex].MinimumWidth = 110;
            }

            if (dataGrid.Columns[e.ColumnIndex].Name == ORDER_SUMMARY_COLUMNS.QTY.ToString())
            {
                int lots = Convert.ToInt16(dataTable.Rows[e.RowIndex][e.ColumnIndex]);

                if (lots >= 0)
                {
                    e.CellStyle.BackColor = RealtimeColors.positiveBackColor;
                }
                else
                {
                    e.CellStyle.BackColor = RealtimeColors.negativeBackColor;
                }
            }


            if (dataGrid.Columns[e.ColumnIndex].Name == ORDER_SUMMARY_COLUMNS.DECS_FILL.ToString())
            {
                if (dataTable.Rows[e.RowIndex][e.ColumnIndex].ToString().Length > 0)
                {
                    bool descisionFilled = Convert.ToBoolean(dataTable.Rows[e.RowIndex][e.ColumnIndex]);

                    if (descisionFilled)
                    {
                        e.CellStyle.BackColor = RealtimeColors.positiveBackColor;
                    }
                    else
                    {
                        e.CellStyle.BackColor = RealtimeColors.negativeBackColor;
                    }
                }
            }

        }

        private void fTPGMIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fileLoadListForm == null)
            {
                fileLoadListForm = new FileLoadList(this);
            }

            fileLoadListForm.Show();
            fileLoadListForm.BringToFront();

            Thread callFtpGmiThread = new Thread(new ParameterizedThreadStart(callFtpGmi));
            callFtpGmiThread.IsBackground = true;
            callFtpGmiThread.Start();
        }

        private void callFtpGmi(object objectin)
        {
            this.Invoke(new EventHandler(ThreadTracker.openThread));

            FTPGetGMI ftpGetGMI = new FTPGetGMI();


            fileLoadListForm.updateLoadingFTPfilesProgressBar(40);


            List<string> downloadedFiles = ftpGetGMI.getFTPFiles();

            if (fileLoadListForm != null)
            {

                fileLoadListForm.updateLoadingFTPfilesProgressBar(50);

                for (int i = 0; i < downloadedFiles.Count; i++)
                {
                    fileLoadListForm.loadFiles(downloadedFiles[i]);
                }

                fileLoadListForm.updateLoadingFTPfilesProgressBar(100);
            }

            this.Invoke(new EventHandler(ThreadTracker.closeThread));
        }

        private void sendSecurityDefFromExpressionList_Click(object sender, EventArgs e)
        {
            sendOrderOrSecDefRequest(false);
        }


        private void btnSOD_Click(object sender, EventArgs e)
        {
            btnSOD.Enabled = false;

            if (DataCollectionLibrary.instrumentSelectedInTreeGui != TradingSystemConstants.ALL_INSTRUMENTS_SELECTED)
            {
                SystemSounds.Asterisk.Play();
            }
            else
            //if (optionSpreadManager.contractSummaryInstrumentSelectedIdx == instruments.Count())
            {
                OptionPayoffComparison sodPayoffs = new OptionPayoffComparison();

                sodPayoffs.Text = "Start Of Day Comparison";

                for (int instrumentCnt = 0; instrumentCnt < instruments.Count(); instrumentCnt++)
                {
                    OptionPayoffComparisonUserControl optionPayoffComparisonUserControl =
                        sodPayoffs.addTabForInstrument(instruments[instrumentCnt].cqgsymbol + "-"
                        + instruments[instrumentCnt].exchangesymbol);

                    double rRisk = 0;


                    //M + O
                    //if (optionSpreadManager.contractSummaryInstrumentSelectedIdx != instruments.Count())
                    {

                        OptionPLChartUserForm scPrice = optionPayoffComparisonUserControl.optionPLChartUserFormModel;
                        scPrice.startupChart(optionArrayTypes, optionSpreadManager);
                        //scPrice.adjustChartSplitter();

                        List<ContractList> contractListOfRollovers = new List<ContractList>();




                        //OptionPLChart scPrice = new OptionPLChart(optionArrayTypes, optionSpreadManager);

                        //OptionPayoffChart scPrice = new OptionPayoffChart();
                        //scPrice.optionPLChartUserForm1.startupChart(optionArrayTypes, optionSpreadManager);

                        scPrice.fillGridFromContractSummary(
                            optionSpreadManager, null,
                            //contractSummaryExpressionListIdx,
                            optionSpreadExpressionList, instruments[instrumentCnt],
                            rRisk, OptionPLChartUserForm.PAYOFF_CHART_TYPE.CONTRACT_SUMMARY_PAYOFF);

                        scPrice.fillChart();

                        scPrice.Show();
                    }

                    //FCM
                    //if (optionSpreadManager.contractSummaryInstrumentSelectedIdx != instruments.Count())
                    {

                        //OptionPLChart scPrice = new OptionPLChart(optionArrayTypes, optionSpreadManager);

                        OptionPLChartUserForm scPrice = optionPayoffComparisonUserControl.optionPLChartUserFormFCM;
                        scPrice.startupChart(optionArrayTypes, optionSpreadManager);

                        //scPrice.adjustChartSplitter();

                        //TODO fix un comment
                        //scPrice.fillGridWithFCMData(
                        //    optionSpreadManager, null, null, optionSpreadManager.admPositionImportWeb,
                        //    optionSpreadExpressionList, instruments[instrumentCnt],
                        //    rRisk);


                        scPrice.fillChart();


                    }

                    //Model + Orders vs. FCM Diff
                    //if (optionSpreadManager.contractSummaryInstrumentSelectedIdx != instruments.Count())
                    {
                        //OptionPLChart scPrice = new OptionPLChart(optionArrayTypes, optionSpreadManager);
                        //scPrice.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
                        //scPrice.adjustChartSplitter();

                        OptionPLChartUserForm scPrice = optionPayoffComparisonUserControl.optionPLChartUserFormDifference;
                        scPrice.startupChart(optionArrayTypes, optionSpreadManager);

                        //scPrice.adjustChartSplitter();

                        scPrice.fillGridFromFCMModelNetDifference(
                            optionSpreadManager,
                            FCM_DataImportLibrary.FCM_PostionList_forCompare,
                            optionSpreadExpressionList, instruments[instrumentCnt],
                            false, "Difference");
                        //OptionPLChart.PAYOFF_CHART_TYPE.CONTRACT_SUMMARY_PAYOFF);


                        scPrice.fillChart();

                        scPrice.highlightModelFCMDifferences();


                    }

                }

                //LayoutMdi(MdiLayout.TileHorizontal);



                sodPayoffs.Show();

                //eodPayoffs.LayoutMdi(MdiLayout.TileHorizontal);

                //Thread.Sleep(2000);


                //LayoutMdi(MdiLayout.TileHorizontal);
            }

            btnSOD.Enabled = true;
        }


        private void btnEOD_Click(object sender, EventArgs e)
        {
            //if (optionSpreadManager.contractSummaryInstrumentSelectedIdx != instruments.Count())

            btnEOD.Enabled = false;

            if (DataCollectionLibrary.instrumentSelectedInTreeGui != TradingSystemConstants.ALL_INSTRUMENTS_SELECTED)
            {
                SystemSounds.Asterisk.Play();
            }
            else
            //if (optionSpreadManager.contractSummaryInstrumentSelectedIdx == instruments.Count())
            {
                OptionPayoffComparison eodPayoffs = new OptionPayoffComparison();

                eodPayoffs.Text = "End Of Day Comparison";


                for (int instrumentCnt = 0; instrumentCnt < instruments.Count(); instrumentCnt++)
                {
                    OptionPayoffComparisonUserControl optionPayoffComparisonUserControl =
                        eodPayoffs.addTabForInstrument(instruments[instrumentCnt].cqgsymbol + "-"
                        + instruments[instrumentCnt].exchangesymbol);


                    //M + O
                    //if (optionSpreadManager.contractSummaryInstrumentSelectedIdx != instruments.Count())
                    {

                        OptionPLChartUserForm scPrice = optionPayoffComparisonUserControl.optionPLChartUserFormModel;
                        scPrice.startupChart(optionArrayTypes, optionSpreadManager);
                        //scPrice.adjustChartSplitter();

                        //List<ContractList> contractListOfRollovers = new List<ContractList>();


                        //foreach (var cl in
                        //    optionSpreadManager.instrumentRollIntoSummary[instrumentCnt]
                        //        .contractHashTable)
                        //{
                        //    // cl.Value.cqgsymbol\
                        //    //TSErrorCatch.debugWriteOut(cl.Value.cqgsymbol);
                        //    if (cl.Value.currentlyRollingContract
                        //        && cl.Value.numberOfContracts != 0)
                        //    {
                        //        contractListOfRollovers.Add(cl.Value);
                        //    }
                        //}

                        //contractSummaryExpressionListIdx,
                        //    optionSpreadExpressionList, instruments[contractSummaryInstrumentSelectedIdx],

                        //List<int> contractsInOrders = new List<int>();

                        //for (int contractSummaryCnt = 0; contractSummaryCnt < contractSummaryExpressionListIdx.Count; contractSummaryCnt++)
                        //{
                        //    contractsInOrders.Add(contractSummaryExpressionListIdx[contractSummaryCnt]);
                        //}

                        //for (int expressionCount = 0; expressionCount < optionSpreadExpressionList.Count; expressionCount++)
                        //{
                        //    if (DataCollectionLibrary.orderSummaryList.Contains(optionSpreadExpressionList[expressionCount].asset.cqgsymbol)
                        //        && !contractsInOrders.Contains(expressionCount))
                        //    {
                        //        contractsInOrders.Add(expressionCount);
                        //    }
                        //}



                        scPrice.fillGrid(
                            //optionSpreadManager, contractsInOrders,
                            //optionSpreadExpressionList, 
                            instruments[instrumentCnt],
                            //0, 
                            OptionPLChartUserForm.PAYOFF_CHART_TYPE.CONTRACT_AND_ORDER_SUMMARY_PAYOFF
                            //null
                            );

                        scPrice.fillChart();
                    }

                    //FCM
                    //if (optionSpreadManager.contractSummaryInstrumentSelectedIdx != instruments.Count())
                    {
                        double rRisk = 0;

                        //OptionPLChart scPrice = new OptionPLChart(optionArrayTypes, optionSpreadManager);

                        OptionPLChartUserForm scPrice = optionPayoffComparisonUserControl.optionPLChartUserFormFCM;
                        scPrice.startupChart(optionArrayTypes, optionSpreadManager);

                        //scPrice.adjustChartSplitter();

                        //TODO fix un comment
                        //scPrice.fillGridWithFCMData(
                        //    optionSpreadManager, null, null, optionSpreadManager.admPositionImportWeb,
                        //    optionSpreadExpressionList, instruments[instrumentCnt],
                        //    rRisk);


                        scPrice.fillChart();
                    }

                    //Model + Orders vs. FCM Diff
                    //if (optionSpreadManager.contractSummaryInstrumentSelectedIdx != instruments.Count())
                    {
                        //OptionPLChart scPrice = new OptionPLChart(optionArrayTypes, optionSpreadManager);
                        //scPrice.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
                        //scPrice.adjustChartSplitter();

                        OptionPLChartUserForm scPrice = optionPayoffComparisonUserControl.optionPLChartUserFormDifference;
                        scPrice.startupChart(optionArrayTypes, optionSpreadManager);

                        //scPrice.adjustChartSplitter();

                        scPrice.fillGridFromFCMModelNetDifference(
                            optionSpreadManager,
                            FCM_DataImportLibrary.FCM_PostionList_forCompare,
                            optionSpreadExpressionList, instruments[instrumentCnt],
                            true, "Difference");
                        //OptionPLChart.PAYOFF_CHART_TYPE.CONTRACT_SUMMARY_PAYOFF);


                        scPrice.fillChart();

                        scPrice.highlightModelFCMDifferences();

                    }

                }

                eodPayoffs.Show();

            }

            btnEOD.Enabled = true;
        }


        //private void btnEOD_Clickxxx(object sender, EventArgs e)
        //{
        //    if (optionSpreadManager.contractSummaryInstrumentSelectedIdx != instruments.Count())
        //    {
        //        EODPayoffs eodPayoffs = new EODPayoffs();

        //        //M + O
        //        if (optionSpreadManager.contractSummaryInstrumentSelectedIdx != instruments.Count())
        //        {

        //            //OptionPLChart scPrice = new OptionPLChart(optionArrayTypes, optionSpreadManager);
        //            OptionPayoffChart scPrice = new OptionPayoffChart();
        //            scPrice.optionPLChartUserForm1.startupChart(optionArrayTypes, optionSpreadManager);

        //            scPrice.optionPLChartUserForm1.adjustChartSplitter();

        //            List<ContractList> contractListOfRollovers = new List<ContractList>();


        //            foreach (var cl in
        //                optionSpreadManager.instrumentRollIntoSummary[optionSpreadManager.contractSummaryInstrumentSelectedIdx]
        //                    .contractHashTable)
        //            {
        //                // cl.Value.cqgsymbol\
        //                //TSErrorCatch.debugWriteOut(cl.Value.cqgsymbol);
        //                if (cl.Value.currentlyRollingContract
        //                    && cl.Value.numberOfContracts != 0)
        //                {
        //                    contractListOfRollovers.Add(cl.Value);
        //                }
        //            }

        //            //contractSummaryExpressionListIdx,
        //            //    optionSpreadExpressionList, instruments[contractSummaryInstrumentSelectedIdx],

        //            List<int> contractsInOrders = new List<int>();

        //            for (int contractSummaryCnt = 0; contractSummaryCnt < contractSummaryExpressionListIdx.Count; contractSummaryCnt++)
        //            {
        //                contractsInOrders.Add(contractSummaryExpressionListIdx[contractSummaryCnt]);
        //            }

        //            for (int expressionCount = 0; expressionCount < optionSpreadExpressionList.Count; expressionCount++)
        //            {
        //                if (optionSpreadManager.orderSummaryHashTable.Contains(optionSpreadExpressionList[expressionCount].cqgsymbol)
        //                    && !contractsInOrders.Contains(expressionCount))
        //                {
        //                    contractsInOrders.Add(expressionCount);
        //                }
        //            }



        //            scPrice.fillGrid(optionSpreadManager, contractsInOrders,
        //                optionSpreadExpressionList, instruments[optionSpreadManager.contractSummaryInstrumentSelectedIdx],
        //                0, OptionPLChart.PAYOFF_CHART_TYPE.CONTRACT_AND_ORDER_SUMMARY_PAYOFF,
        //                contractListOfRollovers);

        //            scPrice.fillChart();

        //            scPrice.AutoScroll = true;

        //            scPrice.HorizontalScroll.Value = scPrice.HorizontalScroll.Maximum;

        //            scPrice.MdiParent = eodPayoffs;

        //            scPrice.Size = new System.Drawing.Size(eodPayoffs.Width, eodPayoffs.Height / 3);

        //            scPrice.Show();

        //            scPrice.Location = new Point(20, 10);
        //        }

        //        //FCM
        //        if (optionSpreadManager.contractSummaryInstrumentSelectedIdx != instruments.Count())
        //        {
        //            double rRisk = 0;

        //            for (int i = 0; i < optionStrategies.Length; i++)
        //            {
        //                if (optionSpreadManager.contractSummaryInstrumentSelectedIdx == optionStrategies[i].indexOfInstrumentInInstrumentsArray)
        //                {
        //                    rRisk += optionStrategies[i].optionStrategyParameters[(int)TBL_STRATEGY_STATE_FIELDS.rRisk].stateValue;
        //                }
        //            }

        //            OptionPLChart scPrice = new OptionPLChart(optionArrayTypes, optionSpreadManager);

        //            scPrice.adjustChartSplitter();

        //            scPrice.fillGridWithFCMData(
        //                optionSpreadManager, null, null, optionSpreadManager.admPositionImportWeb,
        //                optionSpreadExpressionList, instruments[optionSpreadManager.contractSummaryInstrumentSelectedIdx],
        //                rRisk);
        //            //OptionPLChart.PAYOFF_CHART_TYPE.CONTRACT_SUMMARY_PAYOFF);

        //            scPrice.fillChart();

        //            scPrice.AutoScroll = true;

        //            scPrice.HorizontalScroll.Value = scPrice.HorizontalScroll.Maximum;

        //            scPrice.MdiParent = eodPayoffs;

        //            scPrice.Size = new System.Drawing.Size(eodPayoffs.Width, eodPayoffs.Height / 3);

        //            scPrice.Show();

        //            scPrice.Location = new Point(20, 10);
        //        }

        //        //Model + Orders vs. FCM Diff
        //        if (optionSpreadManager.contractSummaryInstrumentSelectedIdx != instruments.Count())
        //        {
        //            OptionPLChart scPrice = new OptionPLChart(optionArrayTypes, optionSpreadManager);
        //            //scPrice.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
        //            scPrice.adjustChartSplitter();

        //            scPrice.fillGridFromFCMModelNetDifference(
        //                optionSpreadManager,
        //                optionSpreadManager.admPositionImportWebListForCompare,
        //                optionSpreadExpressionList, instruments[optionSpreadManager.contractSummaryInstrumentSelectedIdx],
        //                true, "Difference");
        //            //OptionPLChart.PAYOFF_CHART_TYPE.CONTRACT_SUMMARY_PAYOFF);

        //            scPrice.fillChart();
        //            //scPrice.AutoScaleMode = AutoScaleMode.Font;
        //            //scPrice.AutoSize = false;
        //            scPrice.AutoScroll = true;

        //            scPrice.HorizontalScroll.Value = scPrice.HorizontalScroll.Maximum;

        //            scPrice.MdiParent = eodPayoffs;

        //            //float scaleX = 100;// ((float)Screen.PrimaryScreen.WorkingArea.Width / 1024);
        //            //float scaleY = 100; // ((float)Screen.PrimaryScreen.WorkingArea.Height / 768);
        //            //SizeF aSf = new SizeF(scaleX, scaleY);

        //            //scPrice.Scale(aSf);
        //            scPrice.Size = new System.Drawing.Size(eodPayoffs.Width, eodPayoffs.Height / 3);



        //            scPrice.Show();

        //            //scPrice.Location = new Point(20, 10);
        //        }

        //        //LayoutMdi(MdiLayout.TileHorizontal);



        //        eodPayoffs.Show();

        //        eodPayoffs.LayoutMdi(MdiLayout.TileHorizontal);

        //        //Thread.Sleep(2000);


        //        //LayoutMdi(MdiLayout.TileHorizontal);
        //    }
        //}


        private void portfolioSummaryGrid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            portfolioSummaryGrid_CellFormattingDelegate.Invoke(e, portfolioSummaryDataTable);
        }

        private void portfolioSummaryGridSettlements_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            portfolioSummaryGridSettlements_CellFormattingDelegate.Invoke(e, portfolioSummarySettlementDataTable);
        }

        private delegate void PlTotalGridFormattingDelegate(DataGridViewCellFormattingEventArgs e,
            DataTable dataTable);

        PlTotalGridFormattingDelegate portfolioSummaryGrid_CellFormattingDelegate
            = new PlTotalGridFormattingDelegate(plTotalGridFormatting);

        PlTotalGridFormattingDelegate portfolioSummaryGridSettlements_CellFormattingDelegate
            = new PlTotalGridFormattingDelegate(plTotalGridSettlementFormatting);

        private static void plTotalGridFormatting(DataGridViewCellFormattingEventArgs e,
            DataTable dataTable)
        {

            if (dataTable.Rows.Count > 0
                && dataTable.Rows[e.RowIndex][e.ColumnIndex] != null)
            {


                string objString = (dataTable.Rows[e.RowIndex][e.ColumnIndex] as string);

                double cellValue = Convert.ToDouble(string.IsNullOrEmpty(objString) ? "0.0" : objString);

                if (cellValue >= 0)
                {
                    e.CellStyle.BackColor = RealtimeColors.positiveBackColor;
                }
                else
                {
                    e.CellStyle.BackColor = RealtimeColors.negativeBackColor;
                }
            }

        }


        private static void plTotalGridSettlementFormatting(DataGridViewCellFormattingEventArgs e,
            DataTable dataTable)
        {

            if (dataTable.Rows.Count > 0
                && dataTable.Rows[e.RowIndex][e.ColumnIndex] != null
                && (e.RowIndex == 0 || e.RowIndex == 2))
            {


                string objString = (dataTable.Rows[e.RowIndex][e.ColumnIndex] as string);

                double cellValue = Convert.ToDouble(string.IsNullOrEmpty(objString) ? "0.0" : objString);

                if (cellValue >= 0)
                {
                    e.CellStyle.BackColor = RealtimeColors.positiveBackColor;
                }
                else
                {
                    e.CellStyle.BackColor = RealtimeColors.negativeBackColor;
                }
            }

        }

        private void AsyncTaskListener_UpdatedStatus(
            string msg = null,
            STATUS_FORMAT statusFormat = STATUS_FORMAT.DEFAULT,
            STATUS_TYPE connStatus = STATUS_TYPE.NO_STATUS)
        {
            //*******************
            Action action = new Action(
                () =>
                {

                    Color foreColor = Color.Black;
                    Color backColor = Color.LightGreen;

                    switch (statusFormat)
                    {
                        case STATUS_FORMAT.CAUTION:
                            foreColor = Color.Black;
                            backColor = Color.Yellow;
                            break;

                        case STATUS_FORMAT.ALARM:
                            foreColor = Color.Black;
                            backColor = Color.Red;
                            break;

                    }

                    switch (connStatus)
                    {
                        case STATUS_TYPE.DATA_FILLING_COUNT:
                            statusOfUpdatedInstruments.Text = msg;
                            statusOfUpdatedInstruments.ForeColor = ForeColor;
                            statusOfUpdatedInstruments.BackColor = backColor;
                            break;

                        case STATUS_TYPE.CQG_CONNECTION_STATUS:
                            ConnectionStatus.Text = msg;
                            ConnectionStatus.ForeColor = ForeColor;
                            ConnectionStatus.BackColor = backColor;
                            break;

                        case STATUS_TYPE.DATA_STATUS:
                            DataStatus.Text = msg;
                            DataStatus.ForeColor = ForeColor;
                            DataStatus.BackColor = backColor;
                            break;

                        case STATUS_TYPE.DATA_SUBSCRIPTION_STATUS:
                            StatusSubscribeData.Text = msg;
                            StatusSubscribeData.ForeColor = ForeColor;
                            StatusSubscribeData.BackColor = backColor;
                            break;

                        case STATUS_TYPE.TT_FIX_CONNECTION:
                            toolStripFixConnectionStatus.Text = msg;
                            toolStripFixConnectionStatus.ForeColor = ForeColor;
                            toolStripFixConnectionStatus.BackColor = backColor;
                            break;

                        case STATUS_TYPE.PRICE_TYPE:
                            statusPriceType.Text = msg;
                            statusPriceType.ForeColor = ForeColor;
                            statusPriceType.BackColor = backColor;
                            break;

                        case STATUS_TYPE.EOD_SETTLEMENT:
                            statusEODSettlement.Text = msg;
                            statusEODSettlement.ForeColor = ForeColor;
                            statusEODSettlement.BackColor = backColor;
                            break;
                    }

                });

            try
            {
                Invoke(action);
            }
            catch (Exception ex)
            //catch (ObjectDisposedException)
            {
                // User closed the form
                //Console.Write("test");
                AsyncTaskListener.LogMessageAsync(ex.ToString());
            }

            //*******************
        }

        private void OptionRealtimeMonitor_Shown(object sender, EventArgs e)
        {
            optionSpreadManager.optionCQGDataManagement.initializeCQGAndCallbacks();

            updateStatusStripOptionMonitor();


        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            optionSpreadManager.RefreshAccountInfo();
        }



        PlTotalGridFormattingDelegate ContractSummaryFormat_DelegateCall
            = new PlTotalGridFormattingDelegate(ContractSummaryFormatting);

        private void gridViewContractSummary_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            ContractSummaryFormat_DelegateCall.Invoke(e, DataCollectionLibrary.contractSummaryDataTable);
        }

        private static void ContractSummaryFormatting(DataGridViewCellFormattingEventArgs e,
            DataTable dataTable)
        {

            if (dataTable.Rows.Count > 0
                && dataTable.Rows[e.RowIndex][e.ColumnIndex] != null)
            {

                if (
                e.ColumnIndex == (int)CONTRACTSUMMARY_DATA_COLUMNS.QTY
                ||
                e.ColumnIndex == (int)CONTRACTSUMMARY_DATA_COLUMNS.PREV_QTY
                ||
                e.ColumnIndex == (int)CONTRACTSUMMARY_DATA_COLUMNS.ORDER_QTY
                ||
                e.ColumnIndex == (int)CONTRACTSUMMARY_DATA_COLUMNS.PL_DAY_CHG
                ||
                e.ColumnIndex == (int)CONTRACTSUMMARY_DATA_COLUMNS.ORDER_PL_CHG
                ||
                e.ColumnIndex == (int)CONTRACTSUMMARY_DATA_COLUMNS.DELTA
                )
                {


                    string objString = (dataTable.Rows[e.RowIndex][e.ColumnIndex] as string);

                    double cellValue = Convert.ToDouble(string.IsNullOrEmpty(objString) ? "0.0" : objString);

                    if (cellValue >= 0)
                    {
                        e.CellStyle.BackColor = RealtimeColors.positiveBackColor;
                    }
                    else
                    {
                        e.CellStyle.BackColor = RealtimeColors.negativeBackColor;
                    }
                }
                else if (e.ColumnIndex == (int)CONTRACTSUMMARY_DATA_COLUMNS.CONTRACT
                    || e.ColumnIndex == (int)CONTRACTSUMMARY_DATA_COLUMNS.RFRSH_TIME
                    || e.ColumnIndex == (int)CONTRACTSUMMARY_DATA_COLUMNS.TIME)
                {
                    if (e.RowIndex % 2 == 0)
                    {
                        e.CellStyle.BackColor = RealtimeColors.offsetRowBackColor;
                    }
                }
            }

        }

        PlTotalGridFormattingDelegate FCMContractSummaryFormat_DelegateCall
            = new PlTotalGridFormattingDelegate(FCM_ContractSummaryFormatting);

        private void gridLiveFCMData_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            FCMContractSummaryFormat_DelegateCall.Invoke(e, DataCollectionLibrary.FCM_SummaryDataTable);
        }

        private static void FCM_ContractSummaryFormatting(DataGridViewCellFormattingEventArgs e,
            DataTable dataTable)
        {

            if (dataTable.Rows.Count > 0
                && dataTable.Rows[e.RowIndex][e.ColumnIndex] != null)
            {
                if (
                e.ColumnIndex == (int)OPTION_LIVE_ADM_DATA_COLUMNS.NET_AT_ADM
                ||
                e.ColumnIndex == (int)OPTION_LIVE_ADM_DATA_COLUMNS.NET_EDITABLE
                ||
                e.ColumnIndex == (int)OPTION_LIVE_ADM_DATA_COLUMNS.PL_DAY_CHG
                ||
                e.ColumnIndex == (int)OPTION_LIVE_ADM_DATA_COLUMNS.PL_TRANS
                )
                {


                    string objString = (dataTable.Rows[e.RowIndex][e.ColumnIndex] as string);

                    double cellValue = Convert.ToDouble(string.IsNullOrEmpty(objString) ? "0.0" : objString);

                    if (cellValue >= 0)
                    {
                        e.CellStyle.BackColor = RealtimeColors.positiveBackColor;
                    }
                    else
                    {
                        e.CellStyle.BackColor = RealtimeColors.negativeBackColor;
                    }
                }
                else if (e.ColumnIndex == (int)OPTION_LIVE_ADM_DATA_COLUMNS.CONTRACT)
                {
                    if (e.RowIndex % 2 == 0)
                    {
                        e.CellStyle.BackColor = RealtimeColors.offsetRowBackColor;
                    }
                }
            }

        }
    }
}
