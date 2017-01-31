using RealtimeSpreadMonitor.Model;
using RealtimeSpreadMonitor.Mongo;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace RealtimeSpreadMonitor.Forms
{
    public partial class OptionPLChartUserForm : UserControl
    {
        delegate void ThreadSafeUpdateMargin(double marginVal);

        private bool highlightingIntheMoney = false;

        private Instrument_mongo instrument;
        //private Exchange_mongo exchange;

        //private Array optionSpreadContractTypesArray;

        private double rRisk;

        private PAYOFF_CHART_TYPE chartType;

        private int countOfTest = 1000;

        private double constantImpliedVol = 20;

        //private List<int> contractSummaryExpressionListIdx;

        //private List<MongoDB_OptionSpreadExpression> optionSpreadExpressionList;

        private OptionArrayTypes optionArrayTypes;

        private OptionSpreadManager optionSpreadManager;

        //private int brokerAccountChosen;
        private HashSet<Tuple<string, string>> brokerAccountChosen = new HashSet<Tuple<string, string>>();

        private Dictionary<string, int> brokerAcctDictionary = new Dictionary<string, int>();

        public OptionPLChartUserForm()
        {
            InitializeComponent();
        }

        public void startupChart(OptionArrayTypes optionArrayTypes, OptionSpreadManager optionSpreadManager)
        {
            this.optionSpreadManager = optionSpreadManager;

            this.optionArrayTypes = optionArrayTypes;

            setupGrid();

            setupTreeViewBrokerAcct();
        }

        public enum PAYOFF_CHART_TYPE
        {
            CONTRACT_SUMMARY_PAYOFF,
            ORDER_SUMMARY_PAYOFF,
            CONTRACT_AND_ORDER_SUMMARY_PAYOFF
        }

        public enum OPTION_PL_COLUMNS
        {
            CNTRT_TYPE,

            PRDT_CODE,

            STRIKE,

            AVG_PRC,

            NET,

            IMPL_VOL,

            DAYS_TO_EXP,

            YEAR,

            MTH_AS_INT,

            BRKR,

            ACCT,

            DEL_ROW,

            SEL_ROW
        };

        private void addGridDeleteImg(int row, int cell)
        {
            //DataGridViewImageCell imageCell = new DataGridViewImageCell();

            //imageCell.Value 

            gridViewSpreadGrid.Rows[row].Cells[cell].Value = "X";


        }

        private void setupGrid()
        {
            Type optionPLColTypes = typeof(OPTION_PL_COLUMNS);
            Array optionPLColTypesArray = Enum.GetNames(optionPLColTypes);


            gridViewSpreadGrid.ColumnCount = optionPLColTypesArray.Length - 4;

            DataGridViewComboBoxColumn myCboBox = new DataGridViewComboBoxColumn();
            myCboBox.DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing;

            for (int contractTypeCount = 0; contractTypeCount < optionArrayTypes.optionSpreadContractTypesArray.Length; contractTypeCount++)
            {
                myCboBox.Items.Add(optionArrayTypes.optionSpreadContractTypesArray.GetValue(contractTypeCount).ToString());
            }

            gridViewSpreadGrid.Columns.Insert(0, myCboBox);


            //*************
            DataGridViewComboBoxColumn brokerCboBox = new DataGridViewComboBoxColumn();
            brokerCboBox.DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing;
            DataGridViewComboBoxColumn acctCboBox = new DataGridViewComboBoxColumn();
            acctCboBox.DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing;

            HashSet<string> brokerAdded = new HashSet<string>();
            HashSet<string> acctAdded = new HashSet<string>();



            for (int brokerCnt = 0; brokerCnt < DataCollectionLibrary.portfolioAllocation.accountAllocation.Count; brokerCnt++)
            {
                StringBuilder brokerAcct = new StringBuilder();
                brokerAcct.Append(DataCollectionLibrary.portfolioAllocation.accountAllocation[brokerCnt].broker);
                brokerAcct.Append(DataCollectionLibrary.portfolioAllocation.accountAllocation[brokerCnt].account);

                brokerAcctDictionary.Add(brokerAcct.ToString(), brokerCnt);

                if (!brokerAdded.Contains(DataCollectionLibrary.portfolioAllocation.accountAllocation[brokerCnt].broker))
                {
                    brokerAdded.Add(DataCollectionLibrary.portfolioAllocation.accountAllocation[brokerCnt].broker);
                    brokerCboBox.Items.Add(DataCollectionLibrary.portfolioAllocation.accountAllocation[brokerCnt].broker);
                }

                string acct = optionSpreadManager.selectAcct(
                                "",
                                    DataCollectionLibrary.portfolioAllocation.accountAllocation[brokerCnt], true);

                //if (DataCollectionLibrary.portfolioAllocation.accountAllocation[brokerCnt].useConfigFile)
                //{
                //    acctAdded.Add(DataCollectionLibrary.portfolioAllocation.accountAllocation[brokerCnt].cfgfile);
                //    acctCboBox.Items.Add(DataCollectionLibrary.portfolioAllocation.accountAllocation[brokerCnt].cfgfile);
                //}
                //else 
                if (!acctAdded.Contains(acct))
                {
                    acctAdded.Add(acct);
                    acctCboBox.Items.Add(acct);
                }
            }

            gridViewSpreadGrid.Columns.Insert((int)OPTION_PL_COLUMNS.BRKR, brokerCboBox);
            gridViewSpreadGrid.Columns.Insert((int)OPTION_PL_COLUMNS.ACCT, acctCboBox);
            //*************


            DataGridViewCheckBoxColumn myChkBox = new DataGridViewCheckBoxColumn();
            gridViewSpreadGrid.Columns.Add(myChkBox);


            gridViewSpreadGrid.EnableHeadersVisualStyles = false;

            DataGridViewCellStyle colTotalPortStyle = gridViewSpreadGrid.ColumnHeadersDefaultCellStyle;
            colTotalPortStyle.BackColor = Color.Black;
            colTotalPortStyle.ForeColor = Color.White;

            DataGridViewCellStyle rowTotalPortStyle = gridViewSpreadGrid.RowHeadersDefaultCellStyle;
            rowTotalPortStyle.BackColor = Color.Black;
            rowTotalPortStyle.ForeColor = Color.White;

            gridViewSpreadGrid.Columns[(int)OPTION_PL_COLUMNS.CNTRT_TYPE].Width = 55;
            gridViewSpreadGrid.Columns[(int)OPTION_PL_COLUMNS.PRDT_CODE].Width = 35;
            gridViewSpreadGrid.Columns[(int)OPTION_PL_COLUMNS.STRIKE].Width = 50;
            gridViewSpreadGrid.Columns[(int)OPTION_PL_COLUMNS.AVG_PRC].Width = 50;
            gridViewSpreadGrid.Columns[(int)OPTION_PL_COLUMNS.NET].Width = 30;
            gridViewSpreadGrid.Columns[(int)OPTION_PL_COLUMNS.IMPL_VOL].Width = 50;
            gridViewSpreadGrid.Columns[(int)OPTION_PL_COLUMNS.DAYS_TO_EXP].Width = 35;
            gridViewSpreadGrid.Columns[(int)OPTION_PL_COLUMNS.YEAR].Width = 40;
            gridViewSpreadGrid.Columns[(int)OPTION_PL_COLUMNS.MTH_AS_INT].Width = 30;

            gridViewSpreadGrid.Columns[(int)OPTION_PL_COLUMNS.BRKR].Width = 40;
            gridViewSpreadGrid.Columns[(int)OPTION_PL_COLUMNS.ACCT].Width = 80;

            gridViewSpreadGrid.Columns[(int)OPTION_PL_COLUMNS.DEL_ROW].Width = 30;
            //gridViewSpreadGrid.Columns[(int)OPTION_PL_COLUMNS.DEL_ROW].DefaultCellStyle.BackColor = Color.Black;
            gridViewSpreadGrid.Columns[(int)OPTION_PL_COLUMNS.DEL_ROW].DefaultCellStyle.ForeColor = Color.Red;
            gridViewSpreadGrid.Columns[(int)OPTION_PL_COLUMNS.DEL_ROW].DefaultCellStyle.Font =
                new Font("Tahoma", 8, FontStyle.Bold);
            gridViewSpreadGrid.Columns[(int)OPTION_PL_COLUMNS.DEL_ROW].DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleCenter;

            gridViewSpreadGrid.Columns[(int)OPTION_PL_COLUMNS.SEL_ROW].Width = 30;

            //gridViewSpreadGrid.Columns[(int)OPTION_PL_COLUMNS.DELETE].DefaultCellStyle = Color.Red;
            //Font = new Font("Tahoma", 7);

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < optionPLColTypesArray.Length; i++)
            {
                sb.Clear();

                sb.Append(optionPLColTypesArray.GetValue(i).ToString());

                gridViewSpreadGrid
                    .Columns[i]
                    .HeaderCell.Value = sb.ToString().Replace('_', ' ');
            }
        }

        public void setupTreeViewBrokerAcct()
        {
            //brokerAccountChosen = DataCollectionLibrary.portfolioAllocation.accountAllocation.Count;
            

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

                    treeVal.Append(DataCollectionLibrary.accountPositionsList[groupAllocCnt].client_name);
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

            treeViewBrokerAcct.Nodes[treeViewBrokerAcct.Nodes.Count - 1].Checked = true;

        }


        private void treeViewBrokerAcct_AfterSelect(object sender, TreeViewEventArgs e)
        {
            //TreeNode x = treeViewBrokerAcct.SelectedNode;

            //if (x != null)
            //{

            //    brokerAccountChosen = x.Index;

            //    int legCount = 0;
            //    while (legCount < gridViewSpreadGrid.RowCount)
            //    {
            //        gridViewSpreadGrid.Rows[legCount].DefaultCellStyle.BackColor = Color.White;

            //        if (brokerAccountChosen < DataCollectionLibrary.portfolioAllocation.accountAllocation.Count)
            //        {
            //            if ((gridViewSpreadGrid.Rows[legCount]
            //                        .Cells[(int)OPTION_PL_COLUMNS.BRKR].Value != null
            //                &&
            //                gridViewSpreadGrid.Rows[legCount]
            //                        .Cells[(int)OPTION_PL_COLUMNS.ACCT].Value != null)
            //                &&
            //                (
            //                gridViewSpreadGrid.Rows[legCount]
            //                        .Cells[(int)OPTION_PL_COLUMNS.BRKR].Value.ToString().CompareTo(
            //                DataCollectionLibrary.portfolioAllocation.accountAllocation[brokerAccountChosen].broker) != 0
            //                ||
            //                gridViewSpreadGrid.Rows[legCount]
            //                        .Cells[(int)OPTION_PL_COLUMNS.ACCT].Value.ToString().CompareTo(
            //                DataCollectionLibrary.portfolioAllocation.accountAllocation[brokerAccountChosen].account) != 0
            //                ))
            //            {
            //                gridViewSpreadGrid.Rows[legCount].DefaultCellStyle.BackColor = Color.DarkGray;
            //                //continueThisLeg = false;
            //            }
            //        }

            //        legCount++;
            //    }

            //    fillChart();
            //}
        }


        

        private void treeViewBrokerAcct_AfterCheck(object sender, TreeViewEventArgs e)
        {
            treeViewBrokerAcct.BeginUpdate();

            if(e.Node.Index == treeViewBrokerAcct.Nodes.Count - 1)
            {
                bool allselected = e.Node.Checked;

                //foreach (TreeNode tn in treeViewBrokerAcct.Nodes)
                for(int i = 0; i < treeViewBrokerAcct.Nodes.Count - 1; i++)
                {
                    treeViewBrokerAcct.Nodes[i].Checked = allselected;
                }
            }
            else
            {
                Tuple<string,string> tuple = Tuple.Create(DataCollectionLibrary.portfolioAllocation.accountAllocation[e.Node.Index].broker,
                             DataCollectionLibrary.portfolioAllocation.accountAllocation[e.Node.Index].account);

                if(e.Node.Checked)
                {
                    if (!brokerAccountChosen.Contains(tuple))
                    {
                        brokerAccountChosen.Add(tuple);
                    }
                }
                else
                {
                    if (brokerAccountChosen.Contains(tuple))
                    {
                        brokerAccountChosen.Remove(tuple);
                    }
                }

                int legCount = 0;
                while (legCount < gridViewSpreadGrid.RowCount)
                {
                    gridViewSpreadGrid.Rows[legCount].DefaultCellStyle.BackColor = Color.White;
                    //********
                    if (gridViewSpreadGrid.Rows[legCount]
                                    .Cells[(int)OPTION_PL_COLUMNS.BRKR].Value != null
                            &&
                            gridViewSpreadGrid.Rows[legCount]
                                    .Cells[(int)OPTION_PL_COLUMNS.ACCT].Value != null)
                    {


                        if (!brokerAccountChosen.Contains(Tuple.Create(gridViewSpreadGrid.Rows[legCount]
                                    .Cells[(int)OPTION_PL_COLUMNS.BRKR].Value.ToString(),
                                    gridViewSpreadGrid.Rows[legCount]
                                    .Cells[(int)OPTION_PL_COLUMNS.ACCT].Value.ToString())))
                        {
                            gridViewSpreadGrid.Rows[legCount].DefaultCellStyle.BackColor = Color.DarkGray;
                        }
                        //continueThisLeg = false;
                    }


                    legCount++;
                }

                clearChart();
            }

            

            treeViewBrokerAcct.EndUpdate();
        }

        public void fillGridFromContractSummary(
            OptionSpreadManager optionSpreadManager,
            List<int> contractSummaryExpressionListIdx,
            List<MongoDB_OptionSpreadExpression> optionSpreadExpressionList,
            Instrument_mongo instrument,
            double rRisk, PAYOFF_CHART_TYPE chartType)
        {
            fillGrid(
                //optionSpreadManager,
                //contractSummaryExpressionListIdx,
                //optionSpreadExpressionList,
                instrument,
                //rRisk, 
                chartType//,
                         //null
                );
        }

        /// <summary>
        /// Fills the grid.
        /// </summary>
        /// <param name="optionSpreadManager">The option spread manager.</param>
        /// <param name="contractSummaryExpressionListIdx">Index of the contract summary expression list.</param>
        /// <param name="optionSpreadExpressionList">The option spread expression list.</param>
        /// <param name="instrument">The instrument.</param>
        /// <param name="rRisk">The r risk.</param>
        /// <param name="chartType">Type of the chart.</param>
        /// <param name="contractListOfRollovers">The contract list of rollovers.</param>
        public void fillGrid(Instrument_mongo instrument,
            PAYOFF_CHART_TYPE chartType)
        {

            //this.optionSpreadManager = optionSpreadManager;

            //this.contractSummaryExpressionListIdx = contractSummaryExpressionListIdx;
            //this.optionSpreadExpressionList = optionSpreadExpressionList;

            this.instrument = instrument;

            //this.rRisk = rRisk;

            this.chartType = chartType;

            StringBuilder title = new StringBuilder();
            title.Append(instrument.cqgsymbol);

            title.Append(" ");
            title.Append(DataCollectionLibrary.initializationParms.portfolioGroupName);
            title.Append(" ");

            if (chartType == PAYOFF_CHART_TYPE.ORDER_SUMMARY_PAYOFF)
            {
                title.Append(" Order ");
            }
            else if (chartType == PAYOFF_CHART_TYPE.CONTRACT_SUMMARY_PAYOFF)
            {
                title.Append(" Contract ");
            }
            else
            {
                title.Append(" Contract And Order ");
            }


            lblPayoffChartName.Text = title.ToString();


            int numberOfRows = 0;

            foreach (AccountPosition ap in DataCollectionLibrary.accountPositionsList)
            {
                numberOfRows += ap.positions.Count();
            }

            rRiskTextBox.Text = Math.Round(rRisk).ToString();


            countTextBox.Text = countOfTest.ToString();

            if (numberOfRows > 0)
            {

                int rowCount = 0;

                foreach (AccountPosition ap in DataCollectionLibrary.accountPositionsList)
                {
                    foreach (Position p in ap.positions)
                    {

                        if (p.asset.idinstrument == DataCollectionLibrary.instrumentSelectedInTreeGui
                                    || DataCollectionLibrary.instrumentSelectedInTreeGui == TradingSystemConstants.ALL_INSTRUMENTS_SELECTED)
                        {

                            int numOfContracts = 0;
                            if (chartType == PAYOFF_CHART_TYPE.ORDER_SUMMARY_PAYOFF)
                            {
                                numOfContracts = p.qty - p.prev_qty;
                            }
                            else if (chartType == PAYOFF_CHART_TYPE.CONTRACT_SUMMARY_PAYOFF)
                            {
                                numOfContracts = p.prev_qty;
                            }
                            else
                            {
                                numOfContracts = p.qty;
                            }

                            AccountAllocation ac =
                                DataCollectionLibrary.portfolioAllocation.accountAllocation_KeyAccountname[ap.name];

                            long idkey = 0;

                            if (p.asset._type.CompareTo(ASSET_TYPE_MONGO.fut.ToString()) == 0)
                            {
                                idkey = p.asset.idcontract;
                            }
                            else
                            {
                                idkey = p.asset.idoption;
                            }

                            var key = Tuple.Create(idkey, p.asset._type);

                            MongoDB_OptionSpreadExpression mose =
                                    DataCollectionLibrary.optionSpreadExpressionHashTable_key_Id_Type[key];

                            string acct = optionSpreadManager.selectAcct(
                                    //optionSpreadExpressionList[contractSummaryExpressionListIdx[contractCount]].instrument.exchangeSymbol,
                                    instrument.exchangesymbol,
                                    ac, true);

                            if (fillInGridDataRow(rowCount,
                                ac.broker,
                                acct,
                                mose.callPutOrFuture,
                                p.asset.strikeprice,
                                mose.defaultPrice,
                                mose.impliedVol * 100,
                                p.asset.year,
                                p.asset.monthint,
                                p.asset.optionyear,
                                p.asset.optionmonthint,
                                p.asset.productcode,
                                numOfContracts,
                                p.asset.yearFraction * 365
                                ))
                            {
                                rowCount++;
                            }
                        }
                    }
                }

                if (DataCollectionLibrary.riskFreeRateExpression != null)
                {
                    double rfr = DataCollectionLibrary.riskFreeRateExpression.riskFreeRate * 100;

                    riskFreeTextBox.Text = rfr.ToString();
                }

                //#####################


                int futureIndexLeg = findFutureLeg();

                if (futureIndexLeg < 0)
                {
                    addFutureRow(null, null, 0);
                }

            }

        }

        private double calcYearFrac(DateTime expirationDate, DateTime currentDateTime)
        {
            double yearFrac = 0;



            TimeSpan spanBetweenCurrentAndExp =
                                expirationDate - currentDateTime.Date;

            yearFrac = spanBetweenCurrentAndExp.TotalDays / TradingSystemConstants.DAYS_IN_YEAR;

            if (yearFrac < 0)
            {
                yearFrac = 0;
            }


            return yearFrac;
        }

        public void fillGridWithFCMData(
            OptionSpreadManager optionSpreadManager,
            //List<int> contractSummaryExpressionListIdx,
            //List<ContractList> contractListOfRollovers,
            List<ADMPositionImportWeb> admPositionImportWebList,
            //List<MongoDB_OptionSpreadExpression> optionSpreadExpressionList,
            Instrument_mongo instrument, double rRisk, bool includeOrders)
        {
#if DEBUG
            try
#endif
            {
                //this.contractSummaryExpressionListIdx = contractSummaryExpressionListIdx;
                //this.optionSpreadExpressionList = optionSpreadExpressionList;

                this.instrument = instrument;

                this.rRisk = rRisk;

                //this.chartType = chartType;

                StringBuilder title = new StringBuilder();
                title.Append(instrument.cqgsymbol);

                title.Append(" ");
                title.Append(DataCollectionLibrary.initializationParms.portfolioGroupName);
                title.Append(" ");

                title.Append(" @ FCM");



                //this.Text = title.ToString();
                lblPayoffChartName.Text = title.ToString();


                int numberOfRows = 0;

                for (int count = 0; count < admPositionImportWebList.Count; count++)
                {
                    if (admPositionImportWebList[count].instrument.idinstrument
                        == instrument.idinstrument)
                    {
                        numberOfRows++;
                    }
                }

                foreach (AccountPosition ap in DataCollectionLibrary.accountPositionsList)
                {
                    numberOfRows += ap.positions.Count();
                }




                rRiskTextBox.Text = Math.Round(rRisk).ToString();


                countTextBox.Text = countOfTest.ToString();

                if (admPositionImportWebList.Count > 0 && numberOfRows > 0)
                {

                    //gridViewSpreadGrid.RowCount = numberOfRows;

                    int rowCount = 0;


                    for (int contractCount = 0; contractCount < admPositionImportWebList.Count; contractCount++)
                    {
                        if (admPositionImportWebList[contractCount].instrument.idinstrument
                        == instrument.idinstrument)
                        {

                            var key = Tuple.Create(admPositionImportWebList[contractCount].POFFIC,
                                admPositionImportWebList[contractCount].PACCT);

                            if (DataCollectionLibrary.portfolioAllocation.accountAllocation_KeyOfficAcct.ContainsKey(key))
                            {
                                AccountAllocation ac
                                    = DataCollectionLibrary.portfolioAllocation.accountAllocation_KeyOfficAcct[key];

                                string acct = optionSpreadManager.selectAcct(
                                    instrument.exchangesymbol,
                                    ac, true);

                                if (fillInGridDataRow(rowCount,
                                    ac.broker,
                                    acct,
                                    admPositionImportWebList[contractCount].callPutOrFuture,
                                    admPositionImportWebList[contractCount].asset.strikeprice,
                                    admPositionImportWebList[contractCount].optionSpreadExpression.defaultPrice,
                                    admPositionImportWebList[contractCount].optionSpreadExpression.impliedVol * 100,
                                    admPositionImportWebList[contractCount].asset.year,
                                    admPositionImportWebList[contractCount].asset.monthint,
                                    admPositionImportWebList[contractCount].asset.optionyear,
                                    admPositionImportWebList[contractCount].asset.optionmonthint,
                                    admPositionImportWebList[contractCount].asset.productcode,
                                    Convert.ToInt32(admPositionImportWebList[contractCount].netContractsEditable
                                        + admPositionImportWebList[contractCount].transNetLong
                                        - admPositionImportWebList[contractCount].transNetShort),
                                    admPositionImportWebList[contractCount].optionSpreadExpression.asset.yearFraction
                                    * 365
                                    ))
                                {
                                    rowCount++;
                                }
                            }
                        }
                    }
                    //}


                    if (includeOrders)
                    {
                        foreach (AccountPosition ap in DataCollectionLibrary.accountPositionsList)
                        {
                            foreach (Position p in ap.positions)
                            {
                                int numOfContracts = p.qty - p.prev_qty;

                                if ((p.asset.idinstrument == DataCollectionLibrary.instrumentSelectedInTreeGui
                                    || DataCollectionLibrary.instrumentSelectedInTreeGui == TradingSystemConstants.ALL_INSTRUMENTS_SELECTED)
                                    && numOfContracts > 0)
                                {

                                    AccountAllocation ac =
                                        DataCollectionLibrary.portfolioAllocation.accountAllocation_KeyAccountname[ap.name];

                                    long idkey = 0;

                                    if (p.asset._type.CompareTo(ASSET_TYPE_MONGO.fut.ToString()) == 0)
                                    {
                                        idkey = p.asset.idcontract;
                                    }
                                    else
                                    {
                                        idkey = p.asset.idoption;
                                    }

                                    var key = Tuple.Create(idkey, p.asset._type);

                                    MongoDB_OptionSpreadExpression mose =
                                            DataCollectionLibrary.optionSpreadExpressionHashTable_key_Id_Type[key];

                                    string acct = optionSpreadManager.selectAcct(
                                            instrument.exchangesymbol,
                                            ac, true);

                                    if (fillInGridDataRow(rowCount,
                                        ac.broker,
                                        acct,
                                        mose.callPutOrFuture,
                                        p.asset.strikeprice,
                                        mose.defaultPrice,
                                        mose.impliedVol * 100,
                                        p.asset.year,
                                        p.asset.monthint,
                                        p.asset.optionyear,
                                        p.asset.optionmonthint,
                                        p.asset.productcode,
                                        numOfContracts,
                                        p.asset.yearFraction * 365
                                        ))
                                    {
                                        rowCount++;
                                    }
                                }
                            }
                        }
                    }

                    if (DataCollectionLibrary.riskFreeRateExpression != null)
                    {
                        double rfr = DataCollectionLibrary.riskFreeRateExpression.riskFreeRate * 100;

                        riskFreeTextBox.Text = rfr.ToString();
                    }

                    int futureIndexLeg = findFutureLeg();

                    if (futureIndexLeg < 0)
                    {
                        addFutureRow(null, null, 0);
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

        public void fillGridFromFCMModelDiff(
            OptionSpreadManager optionSpreadManager,
            //List<ADMPositionImportWeb> admPositionImportWebListForCompare,
            //List<MongoDB_OptionSpreadExpression> optionSpreadExpressionList,
            Instrument_mongo instrument, bool includeOrders)
        {

            this.instrument = instrument;

            //this.chartType = chartType;

            StringBuilder title = new StringBuilder();
            title.Append(instrument.cqgsymbol);

            title.Append(" ");
            title.Append(DataCollectionLibrary.initializationParms.portfolioGroupName);
            title.Append(" ");

            title.Append(" @ FCM");



            //this.Text = title.ToString();
            lblPayoffChartName.Text = title.ToString();


            int numberOfRows = 0;

            for (int count = 0; count < FCM_DataImportLibrary.FCM_PostionList_forCompare.Count; count++)
            {
                if (FCM_DataImportLibrary.FCM_PostionList_forCompare[count].idinstrument
                    == instrument.idinstrument)
                {
                    numberOfRows++;
                }
            }


            //rRiskTextBox.Text = Math.Round(rRisk).ToString();


            countTextBox.Text = countOfTest.ToString();

            if (FCM_DataImportLibrary.FCM_PostionList_forCompare.Count > 0 && numberOfRows > 0)
            {

                //gridViewSpreadGrid.RowCount = numberOfRows;

                int rowCount = 0;

                for (int contractCount = 0; contractCount < FCM_DataImportLibrary.FCM_PostionList_forCompare.Count; contractCount++)
                {
                    if (FCM_DataImportLibrary.FCM_PostionList_forCompare[contractCount].idinstrument
                    == instrument.idinstrument)
                    {
                        int netLots = 0;
                        if (includeOrders)
                        {
                            netLots = Convert.ToInt32(FCM_DataImportLibrary.FCM_PostionList_forCompare[contractCount].rebalanceLotsForPayoffWithOrders);
                        }
                        else
                        {
                            netLots = Convert.ToInt32(FCM_DataImportLibrary.FCM_PostionList_forCompare[contractCount].rebalanceLotsForPayoffNoOrders);
                        }

                        OPTION_SPREAD_CONTRACT_TYPE callPutOrFuture;
                        double defaultPrice = 0;
                        double impliedVol = 0;
                        double yearFraction = 0;
                        double strikePrice = 0;
                        int contractYear;
                        int contractMonth;
                        int optionYear;
                        int optionMonth;
                        string product_code = "";



                        callPutOrFuture = FCM_DataImportLibrary.FCM_PostionList_forCompare[contractCount].callPutOrFuture;

                        //strikePrice = FCM_DataImportLibrary.FCM_PostionList_forCompare[contractCount].strikeInDecimal;

                        strikePrice = FCM_DataImportLibrary.FCM_PostionList_forCompare[contractCount].asset.strikeprice;

                        yearFraction = FCM_DataImportLibrary.FCM_PostionList_forCompare[contractCount].asset.yearFraction * 365;

                        /*impliedVol = constantImpliedVol;

                        double riskFreeRate = Convert.ToDouble(riskFreeTextBox.Text) / 100;

                        if (callPutOrFuture == OPTION_SPREAD_CONTRACT_TYPE.FUTURE)
                        {
                            defaultPrice = FCM_DataImportLibrary.FCM_PostionList_forCompare[contractCount].futurePriceUsedToCalculateStrikes;
                        }
                        else
                        {
                            char typeSymbol = 'P';

                            if (callPutOrFuture
                                == OPTION_SPREAD_CONTRACT_TYPE.CALL)
                            {
                                typeSymbol = 'C';
                            }


                            defaultPrice = OptionCalcs.blackScholes(typeSymbol,
                                FCM_DataImportLibrary.FCM_PostionList_forCompare[contractCount].futurePriceUsedToCalculateStrikes,
                                        strikePrice,
                                        yearFraction / 365, riskFreeRate,
                                        impliedVol / 100);
                        }*/





                        contractYear =
                            FCM_DataImportLibrary.FCM_PostionList_forCompare[contractCount].asset.year;
                        contractMonth =
                            FCM_DataImportLibrary.FCM_PostionList_forCompare[contractCount].asset.monthint;
                        optionYear =
                            FCM_DataImportLibrary.FCM_PostionList_forCompare[contractCount].asset.optionyear;
                        optionMonth =
                            FCM_DataImportLibrary.FCM_PostionList_forCompare[contractCount].asset.optionmonthint;

                        product_code = FCM_DataImportLibrary.FCM_PostionList_forCompare[contractCount].asset.productcode;





                        if (FCM_DataImportLibrary.FCM_PostionList_forCompare[contractCount].optionSpreadExpression != null)
                        {
                            //callPutOrFuture = FCM_DataImportLibrary.FCM_PostionList_forCompare[contractCount].optionSpreadExpression.callPutOrFuture;

                            defaultPrice = FCM_DataImportLibrary.FCM_PostionList_forCompare[contractCount].optionSpreadExpression.defaultPrice;

                            impliedVol = FCM_DataImportLibrary.FCM_PostionList_forCompare[contractCount]
                                .optionSpreadExpression.impliedVol * 100;

                            //yearFraction = FCM_DataImportLibrary.FCM_PostionList_forCompare[contractCount].optionSpreadExpression.asset.yearFraction
                            //    * 365;

                            

                            /*contractYear =
                                FCM_DataImportLibrary.FCM_PostionList_forCompare[contractCount].optionSpreadExpression.asset.year;  //futureContractYear;
                            contractMonth =
                                FCM_DataImportLibrary.FCM_PostionList_forCompare[contractCount].optionSpreadExpression.asset.monthint; //futureContractMonthInt;
                            optionYear =
                                FCM_DataImportLibrary.FCM_PostionList_forCompare[contractCount].optionSpreadExpression.asset.optionyear; //optionYear;
                            optionMonth =
                                FCM_DataImportLibrary.FCM_PostionList_forCompare[contractCount].optionSpreadExpression.asset.optionmonthint; //optionMonthInt;

                            product_code = FCM_DataImportLibrary.FCM_PostionList_forCompare[contractCount].asset.productcode;
                            */

                        }
                        //else if (FCM_DataImportLibrary.FCM_PostionList_forCompare[contractCount] != null &&
                        //    FCM_DataImportLibrary.FCM_PostionList_forCompare[contractCount].optionSpreadExpression != null)
                        //{
                        //    callPutOrFuture = FCM_DataImportLibrary.FCM_PostionList_forCompare[contractCount]
                        //        .optionSpreadExpression.callPutOrFuture;

                        //    defaultPrice = FCM_DataImportLibrary.FCM_PostionList_forCompare[contractCount]
                        //        .optionSpreadExpression.defaultPrice;

                        //    impliedVol = FCM_DataImportLibrary.FCM_PostionList_forCompare[contractCount]
                        //        .optionSpreadExpression.impliedVol * 100;

                        //    yearFraction = FCM_DataImportLibrary.FCM_PostionList_forCompare[contractCount]
                        //        .optionSpreadExpression.asset.yearFraction
                        //        * 365;

                        //    strikePrice = FCM_DataImportLibrary.FCM_PostionList_forCompare[contractCount]
                        //        .optionSpreadExpression.asset.strikeprice;

                        //    contractYear =
                        //        FCM_DataImportLibrary.FCM_PostionList_forCompare[contractCount]
                        //        .optionSpreadExpression.futureContractYear;
                        //    contractMonth =
                        //        FCM_DataImportLibrary.FCM_PostionList_forCompare[contractCount]
                        //        .optionSpreadExpression.futureContractMonthInt;
                        //    optionYear =
                        //        FCM_DataImportLibrary.FCM_PostionList_forCompare[contractCount]
                        //        .optionSpreadExpression.optionYear;
                        //    optionMonth =
                        //        FCM_DataImportLibrary.FCM_PostionList_forCompare[contractCount]
                        //        .optionSpreadExpression.optionMonthInt;

                        //    product_code = FCM_DataImportLibrary.FCM_PostionList_forCompare[contractCount].asset.productcode;
                        //}
                        else
                        {
                            //callPutOrFuture = FCM_DataImportLibrary.FCM_PostionList_forCompare[contractCount].callPutOrFuture;

                            //strikePrice = FCM_DataImportLibrary.FCM_PostionList_forCompare[contractCount].strikeInDecimal;

                            //yearFraction = FCM_DataImportLibrary.FCM_PostionList_forCompare[contractCount].asset.yearFraction * 365;

                            impliedVol = constantImpliedVol;

                            double riskFreeRate = Convert.ToDouble(riskFreeTextBox.Text) / 100;

                            if (callPutOrFuture == OPTION_SPREAD_CONTRACT_TYPE.FUTURE)
                            {
                                defaultPrice = FCM_DataImportLibrary.FCM_PostionList_forCompare[contractCount].futurePriceUsedToCalculateStrikes;
                            }
                            else
                            {
                                char typeSymbol = 'P';

                                if (callPutOrFuture
                                    == OPTION_SPREAD_CONTRACT_TYPE.CALL)
                                {
                                    typeSymbol = 'C';
                                }


                                defaultPrice = OptionCalcs.blackScholes(typeSymbol,
                                    FCM_DataImportLibrary.FCM_PostionList_forCompare[contractCount].futurePriceUsedToCalculateStrikes,
                                            strikePrice,
                                            yearFraction / 365, riskFreeRate,
                                            impliedVol / 100);
                            }





                            //contractYear =
                            //    FCM_DataImportLibrary.FCM_PostionList_forCompare[contractCount].asset.year;
                            //contractMonth =
                            //    FCM_DataImportLibrary.FCM_PostionList_forCompare[contractCount].asset.monthint;
                            //optionYear =
                            //    FCM_DataImportLibrary.FCM_PostionList_forCompare[contractCount].asset.optionyear;
                            //optionMonth =
                            //    FCM_DataImportLibrary.FCM_PostionList_forCompare[contractCount].asset.optionmonthint;

                            //product_code = FCM_DataImportLibrary.FCM_PostionList_forCompare[contractCount].asset.productcode;

                        }


                        string acct = optionSpreadManager.selectAcct(
                                        instrument.exchangesymbol,
                                        FCM_DataImportLibrary.FCM_PostionList_forCompare[contractCount].acctGroup, true);

                        if (fillInGridDataRow(rowCount,
                            FCM_DataImportLibrary.FCM_PostionList_forCompare[contractCount].acctGroup.broker,
                            acct,
                            callPutOrFuture,
                            strikePrice,
                            defaultPrice,
                            impliedVol,
                            contractYear,
                            contractMonth,
                            optionYear,
                            optionMonth,
                            product_code,
                            netLots,
                            yearFraction
                            ))
                        {
                            rowCount++;
                        }
                    }
                }





                if (DataCollectionLibrary.riskFreeRateExpression != null)
                {
                    double rfr = DataCollectionLibrary.riskFreeRateExpression.riskFreeRate * 100;

                    riskFreeTextBox.Text = rfr.ToString();
                }

                //int expCount = 0;

                //while (expCount < DataCollectionLibrary.optionSpreadExpressionList.Count())
                //{
                //    if (DataCollectionLibrary.optionSpreadExpressionList[expCount].optionExpressionType ==
                //            OPTION_EXPRESSION_TYPES.OPTION_EXPRESSION_RISK_FREE_RATE)
                //    {
                //        double rfr = DataCollectionLibrary.optionSpreadExpressionList[expCount].riskFreeRate
                //            * 100;

                //        riskFreeTextBox.Text = rfr.ToString();

                //        break;
                //    }

                //    expCount++;
                //}

                int futureIndexLeg = findFutureLeg();

                if (futureIndexLeg < 0)
                {
                    addFutureRow(null, null, 0);
                }
            }

        }


        public void fillGridFromFCMModelNetDifference(
            OptionSpreadManager optionSpreadManager,
            List<ADMPositionImportWeb> admPositionImportWebListForCompare,
            List<MongoDB_OptionSpreadExpression> optionSpreadExpressionList,
            Instrument_mongo instrument, bool includeOrders, string titlePassed)
        {
            //this.contractSummaryExpressionListIdx = contractSummaryExpressionListIdx;
            //this.optionSpreadExpressionList = optionSpreadExpressionList;

            this.instrument = instrument;

            //this.chartType = chartType;

            StringBuilder title = new StringBuilder();
            title.Append(instrument.cqgsymbol);

            title.Append(" ");
            title.Append(DataCollectionLibrary.initializationParms.portfolioGroupName);
            title.Append(" ");

            title.Append(titlePassed);



            //this.Text = title.ToString();
            lblPayoffChartName.Text = title.ToString();


            int numberOfRows = 0;

            for (int count = 0; count < admPositionImportWebListForCompare.Count; count++)
            {
                if (admPositionImportWebListForCompare[count].idinstrument
                    == instrument.idinstrument)
                {
                    numberOfRows++;
                }
            }



            countTextBox.Text = countOfTest.ToString();

            if (admPositionImportWebListForCompare.Count > 0 && numberOfRows > 0)
            {

                int rowCount = 0;

                for (int contractCount = 0; contractCount < admPositionImportWebListForCompare.Count; contractCount++)
                {
                    if (admPositionImportWebListForCompare[contractCount].idinstrument
                    == instrument.idinstrument)
                    {
                        int netLots = 0;
                        if (includeOrders)
                        {
                            netLots = -Convert.ToInt32(admPositionImportWebListForCompare[contractCount].rebalanceLotsForPayoffWithOrders);
                        }
                        else
                        {
                            netLots = -Convert.ToInt32(admPositionImportWebListForCompare[contractCount].rebalanceLotsForPayoffNoOrders);
                        }

                        OPTION_SPREAD_CONTRACT_TYPE callPutOrFuture;
                        double defaultPrice = 0;
                        double impliedVol = 0;
                        double yearFraction = 0;
                        double strikePrice = 0;
                        int contractYear;
                        int contractMonth;
                        int optionYear;
                        int optionMonth;
                        string product_code;

                        callPutOrFuture = admPositionImportWebListForCompare[contractCount].callPutOrFuture;

                        strikePrice = admPositionImportWebListForCompare[contractCount].strikeInDecimal;

                        yearFraction = admPositionImportWebListForCompare[contractCount].asset.yearFraction * 365;

                        contractYear =
                                admPositionImportWebListForCompare[contractCount].asset.year;
                            contractMonth =
                                admPositionImportWebListForCompare[contractCount].asset.month;
                            optionYear =
                                admPositionImportWebListForCompare[contractCount].asset.optionyear;
                            optionMonth =
                                admPositionImportWebListForCompare[contractCount].asset.optionmonth;

                            product_code = admPositionImportWebListForCompare[contractCount].asset.productcode;

                        if (admPositionImportWebListForCompare[contractCount].optionSpreadExpression != null)
                        {
                            /*callPutOrFuture = admPositionImportWebListForCompare[contractCount].optionSpreadExpression.callPutOrFuture;

                            defaultPrice = admPositionImportWebListForCompare[contractCount].optionSpreadExpression.defaultPrice;*/

                            impliedVol = admPositionImportWebListForCompare[contractCount]
                                .optionSpreadExpression.impliedVol * 100;

                            /*yearFraction = admPositionImportWebListForCompare[contractCount].optionSpreadExpression.asset.yearFraction
                                * 365;

                            strikePrice = admPositionImportWebListForCompare[contractCount].optionSpreadExpression.asset.strikeprice;

                            contractYear =
                                admPositionImportWebListForCompare[contractCount].optionSpreadExpression.futureContractYear;
                            contractMonth =
                                admPositionImportWebListForCompare[contractCount].optionSpreadExpression.futureContractMonthInt;
                            optionYear =
                                admPositionImportWebListForCompare[contractCount].optionSpreadExpression.optionYear;
                            optionMonth =
                                admPositionImportWebListForCompare[contractCount].optionSpreadExpression.optionMonthInt;

                            product_code = admPositionImportWebListForCompare[contractCount].asset.productcode;*/
                        }
                        /*else if (admPositionImportWebListForCompare[contractCount].positionTotals != null &&
                            admPositionImportWebListForCompare[contractCount].optionSpreadExpression != null)
                        {
                            callPutOrFuture = admPositionImportWebListForCompare[contractCount]
                                .optionSpreadExpression.callPutOrFuture;

                            defaultPrice = admPositionImportWebListForCompare[contractCount]
                                .optionSpreadExpression.defaultPrice;

                            impliedVol = admPositionImportWebListForCompare[contractCount]
                                .optionSpreadExpression.impliedVol * 100;

                            yearFraction = admPositionImportWebListForCompare[contractCount]
                                .optionSpreadExpression.asset.yearFraction
                                * 365;

                            strikePrice = admPositionImportWebListForCompare[contractCount]
                                .optionSpreadExpression.asset.strikeprice;

                            contractYear =
                                admPositionImportWebListForCompare[contractCount]
                                .optionSpreadExpression.futureContractYear;
                            contractMonth =
                                admPositionImportWebListForCompare[contractCount]
                                .optionSpreadExpression.futureContractMonthInt;
                            optionYear =
                                admPositionImportWebListForCompare[contractCount]
                                .optionSpreadExpression.optionYear;
                            optionMonth =
                                admPositionImportWebListForCompare[contractCount]
                                .optionSpreadExpression.optionMonthInt;

                            product_code = admPositionImportWebListForCompare[contractCount].asset.productcode;
                        }*/
                        else
                        {




                            //callPutOrFuture = admPositionImportWebListForCompare[contractCount].callPutOrFuture;

                            //strikePrice = admPositionImportWebListForCompare[contractCount].strikeInDecimal;

                            //yearFraction = admPositionImportWebListForCompare[contractCount].asset.yearFraction * 365;

                            impliedVol = constantImpliedVol;

                            double riskFreeRate = Convert.ToDouble(riskFreeTextBox.Text) / 100;

                            if (callPutOrFuture == OPTION_SPREAD_CONTRACT_TYPE.FUTURE)
                            {
                                defaultPrice = admPositionImportWebListForCompare[contractCount].futurePriceUsedToCalculateStrikes;
                            }
                            else
                            {
                                char typeSymbol = 'P';

                                if (callPutOrFuture
                                    == OPTION_SPREAD_CONTRACT_TYPE.CALL)
                                {
                                    typeSymbol = 'C';
                                }


                                defaultPrice = OptionCalcs.blackScholes(typeSymbol,
                                    admPositionImportWebListForCompare[contractCount].futurePriceUsedToCalculateStrikes,
                                            strikePrice,
                                            yearFraction / 365, riskFreeRate,
                                            impliedVol / 100);
                            }
                        }





                            

                        


                        string acct = optionSpreadManager.selectAcct(
                                        instrument.exchangesymbol,
                                        admPositionImportWebListForCompare[contractCount].acctGroup, true);

                        if (fillInGridDataRow(rowCount,
                            admPositionImportWebListForCompare[contractCount].acctGroup.broker,
                            acct,
                            callPutOrFuture,
                            strikePrice,
                            defaultPrice,
                            impliedVol,
                            contractYear,
                            contractMonth,
                            optionYear,
                            optionMonth,
                            product_code,
                            netLots,
                            yearFraction
                            ))
                        {
                            rowCount++;
                        }
                    }
                }



                if (DataCollectionLibrary.riskFreeRateExpression != null)
                {
                    double rfr = DataCollectionLibrary.riskFreeRateExpression.riskFreeRate * 100;

                    riskFreeTextBox.Text = rfr.ToString();
                }

                int futureIndexLeg = findFutureLeg();

                if (futureIndexLeg < 0)
                {
                    addFutureRow(null, null, 0);
                }
            }

        }


        private int getFutureExpressionIdx()
        {
            int futureExpCnt = 0;

            while (futureExpCnt < DataCollectionLibrary.optionSpreadExpressionList.Count())
            {
                if (DataCollectionLibrary.optionSpreadExpressionList[futureExpCnt].instrument != null
                    && instrument.idinstrument ==
                        DataCollectionLibrary.optionSpreadExpressionList[futureExpCnt].instrument.idinstrument
                    &&
                    DataCollectionLibrary.optionSpreadExpressionList[futureExpCnt].callPutOrFuture ==
                        OPTION_SPREAD_CONTRACT_TYPE.FUTURE)
                {
                    break;
                }

                futureExpCnt++;
            }

            if (futureExpCnt >= DataCollectionLibrary.optionSpreadExpressionList.Count())
            {
                futureExpCnt--;
            }

            return futureExpCnt;
        }

        public void addFutureRow(string broker, string acct, int numberOfLots)
        {
            //int futureIndexLeg = findFutureLeg();

            //if (futureIndexLeg < 0)
            {


                int futureExpCnt = getFutureExpressionIdx();

                

                int row = gridViewSpreadGrid.Rows.Add();

                addGridDeleteImg(row, (int)OPTION_PL_COLUMNS.DEL_ROW);

                gridViewSpreadGrid.Rows[row].Cells[(int)OPTION_PL_COLUMNS.CNTRT_TYPE].Value =
                    optionArrayTypes.optionSpreadContractTypesArray.GetValue(
                            (int)OPTION_SPREAD_CONTRACT_TYPE.FUTURE)
                                .ToString();


                gridViewSpreadGrid.Rows[row].Cells[(int)OPTION_PL_COLUMNS.PRDT_CODE].Value =
                    instrument.exchangesymbolTT;


                gridViewSpreadGrid.Rows[row].Cells[(int)OPTION_PL_COLUMNS.STRIKE].Value = 0;



                gridViewSpreadGrid.Rows[row].Cells[(int)OPTION_PL_COLUMNS.AVG_PRC].Value =
                    ConversionAndFormatting.convertToTickMovesString(
                        DataCollectionLibrary.optionSpreadExpressionList[futureExpCnt].defaultPrice,
                        instrument.ticksize,
                        instrument.tickdisplay);

                gridViewSpreadGrid.Rows[row].Cells[(int)OPTION_PL_COLUMNS.IMPL_VOL].Value = 0;

                gridViewSpreadGrid.Rows[row].Cells[(int)OPTION_PL_COLUMNS.DAYS_TO_EXP].Value = 0;

                gridViewSpreadGrid.Rows[row].Cells[(int)OPTION_PL_COLUMNS.YEAR].Value =
                        DataCollectionLibrary.optionSpreadExpressionList[futureExpCnt].futureContractYear;
                //optionSpreadExpressionList[contractSummaryExpressionListIdx[contractCount]].futureContractYear;

                gridViewSpreadGrid.Rows[row].Cells[(int)OPTION_PL_COLUMNS.MTH_AS_INT].Value =
                        DataCollectionLibrary.optionSpreadExpressionList[futureExpCnt].futureContractMonthInt;

                gridViewSpreadGrid.Rows[row].Cells[(int)OPTION_PL_COLUMNS.NET].Value = numberOfLots;


                if (broker != null)
                {
                    gridViewSpreadGrid.Rows[row].Cells[(int)OPTION_PL_COLUMNS.BRKR].Value =
                            broker;
                }

                if (acct != null)
                {
                    gridViewSpreadGrid.Rows[row].Cells[(int)OPTION_PL_COLUMNS.ACCT].Value =
                            acct;
                }

            }
        }

        public int findFutureLeg()
        {
            int legCount = 0;
            int futureIdx = -1;

            while (legCount < gridViewSpreadGrid.RowCount)
            {
                if (gridViewSpreadGrid.Rows[legCount]
                            .Cells[(int)OPTION_PL_COLUMNS.CNTRT_TYPE].Value != null)
                {
                    String contractType = gridViewSpreadGrid.Rows[legCount]
                                .Cells[(int)OPTION_PL_COLUMNS.CNTRT_TYPE].Value.ToString();

                    if (contractType.CompareTo(optionArrayTypes.optionSpreadContractTypesArray.GetValue(
                            (int)OPTION_SPREAD_CONTRACT_TYPE.FUTURE)) == 0)
                    {
                        futureIdx = legCount;
                        break;
                    }
                }

                legCount++;
            }

            return futureIdx;
        }

        public void fillChart()
        {
            Series serPrice = new Series();

            serPrice.ChartType = SeriesChartType.Line;

            serPrice.Name = "Spread PL";

            Series serPriceAtExp = new Series();

            serPriceAtExp.ChartType = SeriesChartType.Line;

            serPriceAtExp.Name = "Spread PL At Expiration";


            Series futurePriceSeries = new Series();

            futurePriceSeries.ChartType = SeriesChartType.Line;

            futurePriceSeries.Name = "Future Price";

            //Series rRiskSeries = null;

            Series deltaSeries = new Series();

            deltaSeries.ChartType = SeriesChartType.Line;

            deltaSeries.Name = "Delta";

            Series deltaSeriesAtExp = new Series();

            deltaSeriesAtExp.ChartType = SeriesChartType.Line;

            deltaSeriesAtExp.Name = "Delta At Exp";


            //if (chartType )
            {

                //rRiskSeries = new Series();

                //rRiskSeries.ChartType = SeriesChartType.Line;

                //rRiskSeries.Name = "R-Risk";

                rRiskTextBox.Text = Math.Round(rRisk).ToString();

                rRisk = Convert.ToDouble(rRiskTextBox.Text);
            }

            double maxTotal = double.NegativeInfinity;
            double minTotal = double.PositiveInfinity;

            countOfTest = Convert.ToInt16(countTextBox.Text);

            int rowIdx = 0;

            int futureIdx = findFutureLeg();

            //while (legCount < gridViewSpreadGrid.RowCount)
            //{
            //    if (gridViewSpreadGrid.Rows[legCount]
            //                .Cells[(int)OPTION_PL_COLUMNS.CONTRACT_TYPE].Value != null)
            //    {
            //        String contractType = gridViewSpreadGrid.Rows[legCount]
            //                    .Cells[(int)OPTION_PL_COLUMNS.CONTRACT_TYPE].Value.ToString();

            //        if (contractType.CompareTo(optionArrayTypes.optionSpreadContractTypesArray.GetValue(
            //                (int)OPTION_SPREAD_CONTRACT_TYPE.FUTURE)) == 0)
            //        {
            //            futureIdx = legCount;
            //            break;
            //        }
            //    }

            //    legCount++;
            //}



            double futureAvgPrice = returnFuturesAveragePrice(futureIdx);





            lblFuturePrice.Text = "Future: " +

            ConversionAndFormatting.convertToTickMovesString(
                                futureAvgPrice,
                                instrument.ticksize,
                                instrument.tickdisplay);


            double[] futurePriceArray = new double[countOfTest * 2 + 1];



            double startOfTest =
                futureAvgPrice
                - countOfTest *
                instrument.ticksize;


            if (startOfTest < 0)
            {
                startOfTest = 0;
            }

            for (int count = 0; count < futurePriceArray.Length; count++)
            {
                futurePriceArray[count] = startOfTest +
                    count *
                    instrument.ticksize;

            }


            double riskFreeRate = Convert.ToDouble(riskFreeTextBox.Text) / 100;

            for (int futurePointCount = 0; futurePointCount < futurePriceArray.Length; futurePointCount++)
            {
                double plTotal = 0;
                double plAtExpTotal = 0;

                double deltaTotal = 0;
                double deltaTotalAtExp = 0;


                rowIdx = 0;

                while (rowIdx < gridViewSpreadGrid.RowCount)
                {
                    bool continueThisLeg = true;

                    if (gridViewSpreadGrid.Rows[rowIdx]
                                    .Cells[(int)OPTION_PL_COLUMNS.BRKR].Value != null
                            &&
                            gridViewSpreadGrid.Rows[rowIdx]
                                    .Cells[(int)OPTION_PL_COLUMNS.ACCT].Value != null)
                    {

                        if (!brokerAccountChosen.Contains(Tuple.Create(gridViewSpreadGrid.Rows[rowIdx]
                                    .Cells[(int)OPTION_PL_COLUMNS.BRKR].Value.ToString(),
                                    gridViewSpreadGrid.Rows[rowIdx]
                                    .Cells[(int)OPTION_PL_COLUMNS.ACCT].Value.ToString())))
                        {
                            continueThisLeg = false;
                        }
                    }

                    //if (brokerAccountChosen < DataCollectionLibrary.portfolioAllocation.accountAllocation.Count)
                    //{
                    //    if ((gridViewSpreadGrid.Rows[rowIdx]
                    //                .Cells[(int)OPTION_PL_COLUMNS.BRKR].Value != null
                    //        &&
                    //        gridViewSpreadGrid.Rows[rowIdx]
                    //                .Cells[(int)OPTION_PL_COLUMNS.ACCT].Value != null)
                    //        &&
                    //        (
                    //        gridViewSpreadGrid.Rows[rowIdx]
                    //                .Cells[(int)OPTION_PL_COLUMNS.BRKR].Value.ToString().CompareTo(
                    //        DataCollectionLibrary.portfolioAllocation.accountAllocation[brokerAccountChosen].broker) != 0
                    //        ||
                    //        gridViewSpreadGrid.Rows[rowIdx]
                    //                .Cells[(int)OPTION_PL_COLUMNS.ACCT].Value.ToString().CompareTo(
                    //        DataCollectionLibrary.portfolioAllocation.accountAllocation[brokerAccountChosen].account) != 0
                    //        ))
                    //    {
                    //        continueThisLeg = false;
                    //    }
                    //}



                    int cellCount = 0;

                    while (continueThisLeg && cellCount <= (int)OPTION_PL_COLUMNS.DEL_ROW)  //gridViewSpreadGrid.Rows[legCount].Cells.Count)
                    {
                        if (gridViewSpreadGrid.Rows[rowIdx].Cells[cellCount].Value == null)
                        {
                            continueThisLeg = false;
                            break;
                        }

                        cellCount++;
                    }

                    if (continueThisLeg)
                    {
                        String contractType = gridViewSpreadGrid.Rows[rowIdx]
                            .Cells[(int)OPTION_PL_COLUMNS.CNTRT_TYPE].Value.ToString();

                        //double strike = Convert.ToDouble(gridViewSpreadGrid.Rows[legCount]
                        //                        .Cells[(int)OPTION_PL_COLUMNS.STRIKE].Value);

                        double strike =
                        //ConversionAndFormatting.convertToTickMovesDouble(
                        Convert.ToDouble(gridViewSpreadGrid.Rows[rowIdx].Cells[(int)OPTION_PL_COLUMNS.STRIKE].Value);
                        //instrument.optionstrikeincrement,
                        //instrument.optionStrikeDisplay
                        //instrument.ticksize, instrument.tickDisplay
                        //);

                        double daysToExp = Convert.ToDouble(gridViewSpreadGrid.Rows[rowIdx]
                                                .Cells[(int)OPTION_PL_COLUMNS.DAYS_TO_EXP].Value);
                        daysToExp /= 365;

                        double implVol = Convert.ToDouble(gridViewSpreadGrid.Rows[rowIdx]
                                                .Cells[(int)OPTION_PL_COLUMNS.IMPL_VOL].Value);
                        //implVol = 15;
                        implVol /= 100;

                        double numOfContracts = Convert.ToDouble(gridViewSpreadGrid.Rows[rowIdx]
                                                .Cells[(int)OPTION_PL_COLUMNS.NET].Value);

                        double avgPrice = 0;  // Convert.ToDouble(gridViewSpreadGrid.Rows[legCount]
                        //   .Cells[(int)OPTION_PL_COLUMNS.AVG_PRICE].Value);

                        if (contractType.CompareTo(optionArrayTypes.optionSpreadContractTypesArray.GetValue(
                            (int)OPTION_SPREAD_CONTRACT_TYPE.FUTURE)) == 0)
                        {
                            avgPrice = ConversionAndFormatting.convertToTickMovesDouble(
                                gridViewSpreadGrid.Rows[rowIdx]
                                    .Cells[(int)OPTION_PL_COLUMNS.AVG_PRC].Value.ToString(),
                                instrument.ticksize, instrument.tickdisplay);
                        }
                        else
                        {
                            double tempAvgPrice;

                            if (instrument.secondaryoptionticksizerule > 0)
                            {
                                tempAvgPrice = ConversionAndFormatting.convertToTickMovesDouble(
                                    gridViewSpreadGrid.Rows[rowIdx]
                                        .Cells[(int)OPTION_PL_COLUMNS.AVG_PRC].Value.ToString(),
                                    instrument.secondaryoptionticksize, instrument.secondaryoptiontickdisplay);
                            }
                            else
                            {
                                tempAvgPrice = ConversionAndFormatting.convertToTickMovesDouble(
                                    gridViewSpreadGrid.Rows[rowIdx]
                                        .Cells[(int)OPTION_PL_COLUMNS.AVG_PRC].Value.ToString(),
                                    instrument.optionticksize, instrument.optiontickdisplay);
                            }

                            double optionticksize = OptionSpreadManager.chooseoptionticksize(
                                tempAvgPrice,
                                instrument.optionticksize,
                                instrument.secondaryoptionticksize,
                                instrument.secondaryoptionticksizerule);

                            double tickDisplay = OptionSpreadManager.chooseoptionticksize(
                                tempAvgPrice,
                                instrument.optiontickdisplay,
                                instrument.secondaryoptiontickdisplay,
                                instrument.secondaryoptionticksizerule);


                            ConversionAndFormatting.convertToTickMovesDouble(
                                    gridViewSpreadGrid.Rows[rowIdx]
                                        .Cells[(int)OPTION_PL_COLUMNS.AVG_PRC].Value.ToString(),
                                    optionticksize, tickDisplay);


                            //if (instrument.secondaryoptionticksizerule > 0)
                            //{
                            //    avgPrice = ConversionAndFormatting.convertToTickMovesDouble(
                            //        gridViewSpreadGrid.Rows[legCount]
                            //            .Cells[(int)OPTION_PL_COLUMNS.AVG_PRC].Value.ToString(),
                            //        instrument.secondaryoptionticksize, instrument.optiontickdisplay);
                            //}
                            //else
                            //{
                            //    avgPrice = ConversionAndFormatting.convertToTickMovesDouble(
                            //        gridViewSpreadGrid.Rows[legCount]
                            //            .Cells[(int)OPTION_PL_COLUMNS.AVG_PRC].Value.ToString(),
                            //        instrument.optionticksize, instrument.optiontickdisplay);
                            //}
                        }

                        char typeSymbol;
                        bool run = true;

                        double price;
                        double priceAtExp;

                        double tickSize;
                        double tickValue;

                        double delta;
                        double deltaAtExp;


                        if (contractType.CompareTo(optionArrayTypes.optionSpreadContractTypesArray.GetValue(
                                (int)OPTION_SPREAD_CONTRACT_TYPE.CALL)) == 0)
                        {
                            typeSymbol = 'C';

                            price = OptionCalcs.blackScholes(typeSymbol,
                                futurePriceArray[futurePointCount],
                                        strike,
                                         daysToExp, riskFreeRate,
                                        implVol);

                            priceAtExp = OptionCalcs.blackScholes(typeSymbol,
                                futurePriceArray[futurePointCount],
                                        strike,
                                         0, riskFreeRate,
                                        implVol);

                            tickSize = instrument.optionticksize;
                            tickValue = instrument.optiontickvalue;

                            delta = OptionCalcs.gDelta(typeSymbol,
                                futurePriceArray[futurePointCount],
                                        strike,
                                         daysToExp, riskFreeRate, 0,
                                        implVol);

                            deltaAtExp = OptionCalcs.gDelta(typeSymbol,
                                futurePriceArray[futurePointCount],
                                        strike,
                                         0, riskFreeRate, 0,
                                        implVol);
                        }
                        else if (contractType.CompareTo(optionArrayTypes.optionSpreadContractTypesArray.GetValue(
                                (int)OPTION_SPREAD_CONTRACT_TYPE.PUT)) == 0)
                        {
                            typeSymbol = 'P';

                            price = OptionCalcs.blackScholes(typeSymbol,
                                futurePriceArray[futurePointCount],
                                        strike,
                                         daysToExp, riskFreeRate,
                                        implVol);

                            priceAtExp = OptionCalcs.blackScholes(typeSymbol,
                                futurePriceArray[futurePointCount],
                                        strike,
                                         0, riskFreeRate,
                                        implVol);

                            tickSize = instrument.optionticksize;
                            tickValue = instrument.optiontickvalue;

                            delta = OptionCalcs.gDelta(typeSymbol,
                                futurePriceArray[futurePointCount],
                                        strike,
                                         daysToExp, riskFreeRate, 0,
                                        implVol);

                            deltaAtExp = OptionCalcs.gDelta(typeSymbol,
                                futurePriceArray[futurePointCount],
                                        strike,
                                         0, riskFreeRate, 0,
                                        implVol);
                        }
                        else
                        {
                            typeSymbol = 'F';

                            price = futurePriceArray[futurePointCount];
                            priceAtExp = futurePriceArray[futurePointCount];

                            tickSize = instrument.ticksize;
                            tickValue = instrument.tickvalue;

                            delta = 1;
                            deltaAtExp = 1;
                        }

                        if (run)
                        {
                            double pl =
                                    numOfContracts *
                                    (price - avgPrice)
                                        / tickSize
                                        * tickValue;

                            plTotal += pl;

                            pl =
                                    numOfContracts *
                                    (priceAtExp - avgPrice)
                                        / tickSize
                                        * tickValue;

                            plAtExpTotal += pl;

                            deltaTotal += numOfContracts * delta * 100;

                            deltaTotalAtExp += numOfContracts * deltaAtExp * 100;

                        }
                    }
                    rowIdx++;
                }

                if (plTotal > maxTotal)
                {
                    maxTotal = plTotal;
                }

                if (plTotal < minTotal)
                {
                    minTotal = plTotal;
                }

                //spreadPl[futurePointCount] = plTotal;

                //spreadPlAtExp[futurePointCount] = plAtExpTotal;



                DataPoint dp = new DataPoint();

                dp.SetValueXY(
                    //ConversionAndFormatting.convertToTickMovesString(
                    //        futurePriceArray[futurePointCount],
                    //        instrument.ticksize, instrument.tickDisplay),
                    futurePriceArray[futurePointCount],
                    plTotal);

                serPrice.Points.Add(dp);

                DataPoint dpAtExp = new DataPoint();

                dpAtExp.SetValueXY(
                    futurePriceArray[futurePointCount],
                    plAtExpTotal);

                serPriceAtExp.Points.Add(dpAtExp);


                //deltaTotalSeries[futurePointCount] = plTotal;

                DataPoint deltaP = new DataPoint();

                deltaP.SetValueXY(
                    //ConversionAndFormatting.convertToTickMovesString(
                    //        futurePriceArray[futurePointCount],
                    //        instrument.ticksize, instrument.tickDisplay),
                    futurePriceArray[futurePointCount],
                    deltaTotal);

                deltaSeries.Points.Add(deltaP);


                DataPoint deltaPAtExp = new DataPoint();

                deltaPAtExp.SetValueXY(
                    //ConversionAndFormatting.convertToTickMovesString(
                    //        futurePriceArray[futurePointCount],
                    //        instrument.ticksize, instrument.tickDisplay),
                    futurePriceArray[futurePointCount],
                    deltaTotalAtExp);

                deltaSeriesAtExp.Points.Add(deltaPAtExp);

            }

            //if (!chartType)
            //{
            //    DataPoint rRiskPt = new DataPoint();
            //    double rRiskOffset = spreadPl[countOfTest + 1] - rRisk;
            //    rRiskPt.SetValueXY(futurePriceArray[0], rRiskOffset);
            //    rRiskSeries.Points.Add(rRiskPt);

            //    rRiskPt = new DataPoint();
            //    rRiskPt.SetValueXY(futurePriceArray[futurePriceArray.Length - 1], rRiskOffset);
            //    rRiskSeries.Points.Add(rRiskPt);
            //}

            DataPoint futPt = new DataPoint();
            futPt.SetValueXY(
                    //ConversionAndFormatting.convertToTickMovesString(
                    //        futureAvgPrice,
                    //        instrument.ticksize, instrument.tickDisplay),
                    futureAvgPrice,
                            minTotal);
            futurePriceSeries.Points.Add(futPt);

            futPt = new DataPoint();
            futPt.SetValueXY(
                    //ConversionAndFormatting.convertToTickMovesString(
                    //        futureAvgPrice,
                    //        instrument.ticksize, instrument.tickDisplay),
                    futureAvgPrice,
                            maxTotal);
            futurePriceSeries.Points.Add(futPt);

            chart1.Series.Clear();
            chart1.ResetAutoValues();

            chart1.Series.Add(serPrice);
            chart1.Series.Add(serPriceAtExp);

            chart1.Series.Add(futurePriceSeries);


            //if (chartType == PAYOFF_CHART_TYPE.CONTRACT_SUMMARY_PAYOFF)
            //{
            //    chart1.Series.Add(rRiskSeries);
            //}

            chart2.Series.Clear();
            chart2.ResetAutoValues();
            chart2.Series.Add(deltaSeries);
            chart2.Series.Add(deltaSeriesAtExp);
        }

        private void clearChart()
        {
            chart1.Series.Clear();
            chart1.ResetAutoValues();

            chart2.Series.Clear();
            chart2.ResetAutoValues();
        }

        private void btnChart_Click(object sender, EventArgs e)
        {
            fillChart();
        }

        private void btnVolFill_Click(object sender, EventArgs e)
        {
#if DEBUG
            try
#endif
            {
                int legCount = 0;

                double vol = 0;

                if (volTextBox.Text != null)
                {
                    vol = Convert.ToDouble(volTextBox.Text);

                    while (legCount < gridViewSpreadGrid.RowCount)
                    {
                        if (gridViewSpreadGrid.Rows[legCount]
                            .Cells[(int)OPTION_PL_COLUMNS.CNTRT_TYPE].Value != null)
                        {
                            gridViewSpreadGrid.Rows[legCount]
                                .Cells[(int)OPTION_PL_COLUMNS.IMPL_VOL].Value
                                = vol;
                        }

                        legCount++;
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

        private void btnDaysFill_Click(object sender, EventArgs e)
        {
#if DEBUG
            try
#endif
            {
                int legCount = 0;

                double days = 0;

                if (daysTextBox.Text != null)
                {
                    days = Convert.ToDouble(daysTextBox.Text);

                    while (legCount < gridViewSpreadGrid.RowCount)
                    {
                        if (gridViewSpreadGrid.Rows[legCount]
                            .Cells[(int)OPTION_PL_COLUMNS.CNTRT_TYPE].Value != null)
                        {
                            gridViewSpreadGrid.Rows[legCount]
                                .Cells[(int)OPTION_PL_COLUMNS.DAYS_TO_EXP].Value
                                = days;
                        }

                        legCount++;
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

        private void gridViewSpreadGrid_MouseClick(object sender, MouseEventArgs e)
        {
#if DEBUG
            try
#endif
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Right)
                {
                    optionPLChartDataMenuStrip.Show(gridViewSpreadGrid, new Point(e.X, e.Y));
                    //optionConfigMenuStrip.Show(optionConfigSetupGrid, new Point(e.X, e.Y));

                }
            }
#if DEBUG
            catch (Exception ex)
            {
                TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
            }
#endif
        }

        private void addRowMenuItem_Click(object sender, EventArgs e)
        {
#if DEBUG
            try
#endif
            {
                int row = gridViewSpreadGrid.Rows.Add();

                addGridDeleteImg(row, (int)OPTION_PL_COLUMNS.DEL_ROW);
            }
#if DEBUG
            catch (Exception ex)
            {
                TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
            }
#endif
        }

        private void deleteRowMenuItem_Click(object sender, EventArgs e)
        {
#if DEBUG
            try
#endif
            {
                if (gridViewSpreadGrid.SelectedCells.Count > 0)
                {
                    int selectedRow = gridViewSpreadGrid.SelectedCells[0].RowIndex;
                    gridViewSpreadGrid.Rows.RemoveAt(selectedRow);
                }

            }
#if DEBUG
            catch (Exception ex)
            {
                TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
            }
#endif

            //hideNet0Rows(true);
        }

        private void gridViewSpreadGrid_CellMouseMove(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex < 0 || e.RowIndex < 0)
            {

                return;

            }

            if (e.ColumnIndex == (int)OPTION_PL_COLUMNS.DEL_ROW)
            {
                gridViewSpreadGrid.Cursor = Cursors.Hand;
            }
            else
            {
                gridViewSpreadGrid.Cursor = Cursors.Default;

            }
        }

        private void gridViewSpreadGrid_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex < 0 || e.RowIndex < 0)
            {

                return;

            }

            if (e.ColumnIndex == (int)OPTION_PL_COLUMNS.DEL_ROW)
            {

                gridViewSpreadGrid.Rows.RemoveAt(e.RowIndex);

            }
        }

        public void updateInitialMargin(double value)
        {
            ThreadSafeUpdateMargin d = new ThreadSafeUpdateMargin(updateInitialMarginThreadSafe);

            this.Invoke(d, value);
        }

        private void updateInitialMarginThreadSafe(double value)
        {
            lblInitialMargin.Text = value.ToString();
        }

        public void updateMaintenanceMargin(double value)
        {
            ThreadSafeUpdateMargin d = new ThreadSafeUpdateMargin(updateMaintenanceMarginThreadSafe);

            this.Invoke(d, value);
        }

        private void updateMaintenanceMarginThreadSafe(double value)
        {
            lblMaintenanceMargin.Text = value.ToString();
        }

        private void btnMargin_Click(object sender, EventArgs e)
        {
            //Type optionPLColTypes = typeof(OPTION_PL_COLUMNS);
            //Array optionPLColTypesArray = Enum.GetNames(optionPLColTypes);

            //for (int i = 0; i < optionPLColTypesArray.Length; i++)
            //{
            //    sb.Clear();

            //    sb.Append(optionPLColTypesArray.GetValue(i).ToString());

            //    gridViewSpreadGrid
            //        .Columns[i]
            //        .HeaderCell.Value = sb.ToString().Replace('_', ' ');
            //}

            int futureIdx = -1;
            int legCount = 0;

            while (legCount < gridViewSpreadGrid.RowCount)
            {
                if (gridViewSpreadGrid.Rows[legCount]
                            .Cells[(int)OPTION_PL_COLUMNS.CNTRT_TYPE].Value != null)
                {
                    String contractType = gridViewSpreadGrid.Rows[legCount]
                                .Cells[(int)OPTION_PL_COLUMNS.CNTRT_TYPE].Value.ToString();

                    if (contractType.CompareTo(optionArrayTypes.optionSpreadContractTypesArray.GetValue(
                            (int)OPTION_SPREAD_CONTRACT_TYPE.FUTURE)) == 0)
                    {
                        futureIdx = legCount;
                        break;
                    }
                }

                legCount++;
            }

            if (futureIdx > -1)
            {

                List<OptionChartMargin> contractList = new List<OptionChartMargin>();

                for (int i = 0; i < gridViewSpreadGrid.Rows.Count; i++)
                {
                    bool continueToCallMargin = true;

                    int cellCnt = 0;

                    while (cellCnt < (int)OPTION_PL_COLUMNS.SEL_ROW)
                    {
                        if (gridViewSpreadGrid.Rows[i]
                                .Cells[(int)OPTION_PL_COLUMNS.CNTRT_TYPE].Value == null)
                        {
                            continueToCallMargin = false;
                            break;
                        }

                        cellCnt++;
                    }

                    if (continueToCallMargin)
                    {

                        OptionChartMargin ocm = new OptionChartMargin();



                        ocm.strikePrice =
                            Convert.ToDouble(gridViewSpreadGrid.Rows[i].Cells[(int)OPTION_PL_COLUMNS.STRIKE].Value);

                        ocm.intrument = instrument;

                        ocm.numberOfContracts =
                            Convert.ToInt16(gridViewSpreadGrid.Rows[i].Cells[(int)OPTION_PL_COLUMNS.NET].Value);


                        //        double daysToExp = Convert.ToDouble(gridViewSpreadGrid.Rows[legCount]
                        //                                .Cells[(int)OPTION_PL_COLUMNS.DAYS_TO_EXP].Value);
                        //        daysToExp /= 365;

                        //        double implVol = Convert.ToDouble(gridViewSpreadGrid.Rows[legCount]
                        //                                .Cells[(int)OPTION_PL_COLUMNS.IMPLIED_VOL].Value);
                        //        //implVol = 15;
                        //        implVol /= 100;

                        //        double numOfContracts = Convert.ToDouble(gridViewSpreadGrid.Rows[legCount]
                        //                                .Cells[(int)OPTION_PL_COLUMNS.NET].Value);


                        String contractType = gridViewSpreadGrid.Rows[i]
                                    .Cells[(int)OPTION_PL_COLUMNS.CNTRT_TYPE].Value.ToString();

                        if (contractType.CompareTo(optionArrayTypes.optionSpreadContractTypesArray.GetValue(
                            (int)OPTION_SPREAD_CONTRACT_TYPE.FUTURE)) == 0)
                        {
                            ocm.contractType = OPTION_SPREAD_CONTRACT_TYPE.FUTURE;

                            ocm.contractYear =
                                Convert.ToInt16(gridViewSpreadGrid.Rows[i].Cells[(int)OPTION_PL_COLUMNS.YEAR].Value);

                            ocm.contractMonthInt =
                                Convert.ToInt16(gridViewSpreadGrid.Rows[i].Cells[(int)OPTION_PL_COLUMNS.MTH_AS_INT].Value);
                        }
                        else
                        {
                            ocm.contractYear =
                                Convert.ToInt16(gridViewSpreadGrid.Rows[futureIdx].Cells[(int)OPTION_PL_COLUMNS.YEAR].Value);

                            ocm.contractMonthInt =
                                Convert.ToInt16(gridViewSpreadGrid.Rows[futureIdx].Cells[(int)OPTION_PL_COLUMNS.MTH_AS_INT].Value);

                            ocm.optionYear =
                                Convert.ToInt16(gridViewSpreadGrid.Rows[i].Cells[(int)OPTION_PL_COLUMNS.YEAR].Value);

                            ocm.optionMonthInt =
                                Convert.ToInt16(gridViewSpreadGrid.Rows[i].Cells[(int)OPTION_PL_COLUMNS.MTH_AS_INT].Value);

                            if (contractType.CompareTo(optionArrayTypes.optionSpreadContractTypesArray.GetValue(
                               (int)OPTION_SPREAD_CONTRACT_TYPE.CALL)) == 0)
                            {
                                ocm.contractType = OPTION_SPREAD_CONTRACT_TYPE.CALL;
                            }
                            else if (contractType.CompareTo(optionArrayTypes.optionSpreadContractTypesArray.GetValue(
                                (int)OPTION_SPREAD_CONTRACT_TYPE.PUT)) == 0)
                            {
                                ocm.contractType = OPTION_SPREAD_CONTRACT_TYPE.PUT;
                            }
                        }


                        //ocm.contractType gridViewSpreadGrid.Rows[i].Cells[(int)OPTION_PL_COLUMNS.CONTRACT_TYPE].Value;


                        contractList.Add(ocm);
                    }
                }

                CMEMarginCallSingleInstrument cmeMarginCall = new CMEMarginCallSingleInstrument(contractList,
                   instrument, instrument.exchange, null, //this,
                   false);

                cmeMarginCall.generateMarginRequest();
            }


        }

        private void btnStageEntry_Click(object sender, EventArgs e)
        {
            stageOrders(false, false);
        }

        private void stageOrders(bool reverseLots, bool sendSelectedRows)
        {
            if (optionSpreadManager.stageOrdersLibrary != null)
            {
                List<StageOrdersToTTWPFLibrary.Model.OrderModel> contractListToStage = new List<StageOrdersToTTWPFLibrary.Model.OrderModel>();

                int futureIdx = -1;
                int legCount = 0;

                while (legCount < gridViewSpreadGrid.RowCount)
                {
                    if (gridViewSpreadGrid.Rows[legCount]
                                .Cells[(int)OPTION_PL_COLUMNS.CNTRT_TYPE].Value != null)
                    {
                        String contractType = gridViewSpreadGrid.Rows[legCount]
                                    .Cells[(int)OPTION_PL_COLUMNS.CNTRT_TYPE].Value.ToString();

                        if (contractType.CompareTo(optionArrayTypes.optionSpreadContractTypesArray.GetValue(
                                (int)OPTION_SPREAD_CONTRACT_TYPE.FUTURE)) == 0)
                        {
                            futureIdx = legCount;
                            break;
                        }
                    }

                    legCount++;
                }

                if (futureIdx > -1)
                {
                    for (int rowIdx = 0; rowIdx < gridViewSpreadGrid.Rows.Count; rowIdx++)
                    {
                        if (!sendSelectedRows
                            ||
                            (sendSelectedRows &&
                            Convert.ToBoolean(gridViewSpreadGrid.Rows[rowIdx].Cells[(int)OPTION_PL_COLUMNS.SEL_ROW].Value)))
                        {

                            int stageOrderLots = Convert.ToInt16(gridViewSpreadGrid.Rows[rowIdx].Cells[(int)OPTION_PL_COLUMNS.NET].Value);

                            if (reverseLots)
                            {
                                stageOrderLots = -1 * stageOrderLots;
                            }

                            if (stageOrderLots != 0
                                && gridViewSpreadGrid.Rows[rowIdx].Cells[(int)OPTION_PL_COLUMNS.BRKR].Value != null
                                && gridViewSpreadGrid.Rows[rowIdx].Cells[(int)OPTION_PL_COLUMNS.ACCT].Value != null)
                            {
                                bool continueToSubmitThisLeg = true;

                                if (!brokerAccountChosen.Contains(Tuple.Create(gridViewSpreadGrid.Rows[rowIdx]
                                    .Cells[(int)OPTION_PL_COLUMNS.BRKR].Value.ToString(),
                                    gridViewSpreadGrid.Rows[rowIdx]
                                    .Cells[(int)OPTION_PL_COLUMNS.ACCT].Value.ToString())))
                                {
                                    continueToSubmitThisLeg = false;
                                }

                                //if (brokerAccountChosen < DataCollectionLibrary.portfolioAllocation.accountAllocation.Count)
                                //{
                                //    if (gridViewSpreadGrid.Rows[rowIdx]
                                //                .Cells[(int)OPTION_PL_COLUMNS.BRKR].Value.ToString().CompareTo(
                                //        DataCollectionLibrary.portfolioAllocation.accountAllocation[brokerAccountChosen].broker) != 0
                                //        ||
                                //        gridViewSpreadGrid.Rows[rowIdx]
                                //                .Cells[(int)OPTION_PL_COLUMNS.ACCT].Value.ToString().CompareTo(
                                //        DataCollectionLibrary.portfolioAllocation.accountAllocation[brokerAccountChosen].account) != 0
                                //        )
                                //    {
                                //        continueToSubmitThisLeg = false;
                                //    }
                                //}

                                if (continueToSubmitThisLeg)
                                {
                                    contractListToStage.Add(getOrder(futureIdx, rowIdx,
                                        stageOrderLots));
                                }

                            }
                        }
                    }
                }

                if (contractListToStage.Count > 0)
                {
                    optionSpreadManager.stageOrdersLibrary.stageOrders(contractListToStage);
                }

            }
        }


        private StageOrdersToTTWPFLibrary.Model.OrderModel getOrder(int futureIdx, int rowIdx,
            int stageOrderLots)
        {

            //for (int i = 0; i < gridViewSpreadGrid.Rows.Count; i++)
            {
                //int stageOrderLots = Convert.ToInt16(gridViewSpreadGrid.Rows[i].Cells[(int)OPTION_PL_COLUMNS.NET].Value);

                //if (stageOrderLots != 0)
                {

                    StageOrdersToTTWPFLibrary.Model.OrderModel orderModel =
                                new StageOrdersToTTWPFLibrary.Model.OrderModel();

                    orderModel.cqgsymbol = "";


                    orderModel.underlyingExchange = instrument.exchange.tradingtechnologies_exchange;

                    orderModel.underlyingGateway = instrument.exchange.tradingtechnologies_gateway;



                    orderModel.orderQty = Math.Abs(stageOrderLots);

                    //orderModel.underlyingExchangeSymbol = instrument.exchangeSymbolTT;

                    String contractType = gridViewSpreadGrid.Rows[rowIdx]
                                .Cells[(int)OPTION_PL_COLUMNS.CNTRT_TYPE].Value.ToString();

                    if (contractType.CompareTo(optionArrayTypes.optionSpreadContractTypesArray.GetValue(
                        (int)OPTION_SPREAD_CONTRACT_TYPE.FUTURE)) == 0)
                    {
                        orderModel.securityType = StageOrdersToTTWPFLibrary.Enums.SECURITY_TYPE.FUTURE;

                        orderModel.contractMonthint =
                            Convert.ToInt16(gridViewSpreadGrid.Rows[rowIdx].Cells[(int)OPTION_PL_COLUMNS.MTH_AS_INT].Value);

                        orderModel.contractYear =
                            Convert.ToInt16(gridViewSpreadGrid.Rows[rowIdx].Cells[(int)OPTION_PL_COLUMNS.YEAR].Value);

                        orderModel.maturityMonthYear =
                                    new DateTime(orderModel.contractYear, orderModel.contractMonthint, 1)
                                        .ToString("yyyyMM", DateTimeFormatInfo.InvariantInfo);

                        orderModel.underlyingExchangeSymbol =                             
                            gridViewSpreadGrid.Rows[rowIdx].Cells[(int)OPTION_PL_COLUMNS.PRDT_CODE].Value.ToString();
                            //instrument.exchangesymbolTT;

                        //TSErrorCatch.debugWriteOut("future " + instrument.exchangesymbolTT + " " +
                        //    gridViewSpreadGrid.Rows[rowIdx].Cells[(int)OPTION_PL_COLUMNS.PRDT_CODE].Value.ToString());

                    }
                    else
                    {
                        orderModel.securityType = StageOrdersToTTWPFLibrary.Enums.SECURITY_TYPE.OPTION;

                        orderModel.underlyingExchangeSymbol =
                            gridViewSpreadGrid.Rows[rowIdx].Cells[(int)OPTION_PL_COLUMNS.PRDT_CODE].Value.ToString();
                            //instrument.optionexchangesymbolTT;

                        //TSErrorCatch.debugWriteOut("option " + instrument.optionexchangesymbolTT + " " +
                        //    gridViewSpreadGrid.Rows[rowIdx].Cells[(int)OPTION_PL_COLUMNS.PRDT_CODE].Value.ToString());

                        //instrument.span_cqg_codes_dictionary[]

                        orderModel.contractYear =
                            Convert.ToInt16(gridViewSpreadGrid.Rows[futureIdx].Cells[(int)OPTION_PL_COLUMNS.YEAR].Value);

                        orderModel.contractMonthint =
                            Convert.ToInt16(gridViewSpreadGrid.Rows[futureIdx].Cells[(int)OPTION_PL_COLUMNS.MTH_AS_INT].Value);

                        orderModel.optionYear =
                            Convert.ToInt16(gridViewSpreadGrid.Rows[rowIdx].Cells[(int)OPTION_PL_COLUMNS.YEAR].Value);

                        orderModel.optionMonthInt =
                            Convert.ToInt16(gridViewSpreadGrid.Rows[rowIdx].Cells[(int)OPTION_PL_COLUMNS.MTH_AS_INT].Value);

                        orderModel.maturityMonthYear =
                            new DateTime(orderModel.optionYear, orderModel.optionMonthInt, 1)
                                .ToString("yyyyMM", DateTimeFormatInfo.InvariantInfo);

                        if (contractType.CompareTo(optionArrayTypes.optionSpreadContractTypesArray.GetValue(
                           (int)OPTION_SPREAD_CONTRACT_TYPE.CALL)) == 0)
                        {
                            orderModel.optionType = StageOrdersToTTWPFLibrary.Enums.OPTION_TYPE.CALL;
                        }
                        else if (contractType.CompareTo(optionArrayTypes.optionSpreadContractTypesArray.GetValue(
                            (int)OPTION_SPREAD_CONTRACT_TYPE.PUT)) == 0)
                        {
                            orderModel.optionType = StageOrdersToTTWPFLibrary.Enums.OPTION_TYPE.PUT;
                        }

                        double sd = instrument.optionstrikedisplayTT;

                        orderModel.optionStrikePrice =
                                (decimal)ConversionAndFormatting.convertToStrikeForTT(
                                Convert.ToDouble(gridViewSpreadGrid.Rows[rowIdx].Cells[(int)OPTION_PL_COLUMNS.STRIKE].Value),
                                instrument.optionstrikeincrement,
                                sd,
                                instrument.idinstrument);
                    }

                    if (stageOrderLots > 0)
                    {
                        orderModel.side = StageOrdersToTTWPFLibrary.Enums.Side.Buy;
                    }
                    else
                    {
                        orderModel.side = StageOrdersToTTWPFLibrary.Enums.Side.Sell;
                    }

                    orderModel.stagedOrderMessage = "SIGNAL TRIGGER ORDER FROM PAYOFF";



                    orderModel.orderPrice = findLimitPrice(
                                    orderModel.side,
                                    orderModel.securityType,
                                    gridViewSpreadGrid.Rows[rowIdx].Cells[(int)OPTION_PL_COLUMNS.AVG_PRC].Value.ToString(),
                                    instrument);

                    orderModel.orderPlacementType = DataCollectionLibrary.initializationParms.FIX_OrderPlacementType;



                    string brkrFromGrid = gridViewSpreadGrid.Rows[rowIdx]
                                                    .Cells[(int)OPTION_PL_COLUMNS.BRKR].Value.ToString();

                    string acctFromGrid = gridViewSpreadGrid.Rows[rowIdx]
                                    .Cells[(int)OPTION_PL_COLUMNS.ACCT].Value.ToString();

                    string brkAndAcct = brkrFromGrid.Trim() + acctFromGrid.Trim();

                    int brokerAcctIdx = 0;

                    if (brokerAcctDictionary.ContainsKey(brkAndAcct))
                    {
                        brokerAcctIdx = brokerAcctDictionary[brkAndAcct];
                    }

                    string acct =
                        optionSpreadManager.selectAcct(instrument.exchangesymbol,
                        DataCollectionLibrary.portfolioAllocation.accountAllocation[brokerAcctIdx], false);


                    orderModel.broker_18220 = brkrFromGrid;

                    orderModel.acct = acct;


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


                    return orderModel;

                    //contractListToStage.Add(orderModel);

                }

            }



        }

        private decimal findLimitPrice(StageOrdersToTTWPFLibrary.Enums.Side side,
            StageOrdersToTTWPFLibrary.Enums.SECURITY_TYPE securityType,
            String avgPriceAsString, Instrument_mongo instrument)
        {

            decimal priceToReturn = 0;
            double avgPrice = 0;
            double offsetPrice = 0;
            int tickOffset = Convert.ToInt16(tickOffsetBox.Text);

            double tickSize = 0;
            double tickDisplay = 0;

            //double tickDisplayTT_multiplier = 1;

            bool isOptionContract = true;

            if (securityType == StageOrdersToTTWPFLibrary.Enums.SECURITY_TYPE.FUTURE)
            {
                tickSize = instrument.ticksize;
                tickDisplay = instrument.tickdisplay;

                isOptionContract = false;

                //tickDisplayTT_multiplier = instrument.tickDisplayTT;
            }
            else
            {
                double tempAvgPrice;

                if (instrument.secondaryoptionticksizerule > 0)
                {
                    tempAvgPrice = ConversionAndFormatting.convertToTickMovesDouble(
                        avgPriceAsString,
                        instrument.secondaryoptionticksize, instrument.secondaryoptiontickdisplay);
                }
                else
                {
                    tempAvgPrice = ConversionAndFormatting.convertToTickMovesDouble(
                        avgPriceAsString,
                        instrument.optionticksize, instrument.optiontickdisplay);
                }

                //double optionticksize = OptionSpreadManager.chooseoptionticksize(
                //    tempAvgPrice,
                //    instrument.optionticksize,
                //    instrument.secondaryoptionticksize,
                //    instrument.secondaryoptionticksizerule);




                tickSize = OptionSpreadManager.chooseoptionticksize(
                    tempAvgPrice,
                    instrument.optionticksize,
                    instrument.secondaryoptionticksize,
                    instrument.secondaryoptionticksizerule);

                tickDisplay = OptionSpreadManager.chooseoptionticksize(
                    tempAvgPrice,
                    instrument.optiontickdisplay,
                    instrument.secondaryoptiontickdisplay,
                    instrument.secondaryoptionticksizerule);


                //tickDisplayTT_multiplier = instrument.optiontickdisplayTT;
            }

            avgPrice = ConversionAndFormatting.convertToTickMovesDouble(
                                avgPriceAsString,
                                tickSize, tickDisplay);

            if (side == StageOrdersToTTWPFLibrary.Enums.Side.Buy)
            {
                offsetPrice = avgPrice - tickOffset * tickSize;

            }
            else
            {
                offsetPrice = avgPrice + tickOffset * tickSize;
            }

            priceToReturn =
                    Convert.ToDecimal(ConversionAndFormatting.convertToOrderPriceForTT(
                        offsetPrice,
                        tickSize,
                        tickDisplay,
                        instrument.idinstrument, isOptionContract));

            if (priceToReturn < 0)
            {
                priceToReturn = 0;
            }

            return priceToReturn;

        }

        private void btnStageEntrySelected_Click(object sender, EventArgs e)
        {
            stageOrders(false, true);
        }

        private void btnStageLiquidate_Click(object sender, EventArgs e)
        {
            stageOrders(true, false);
        }

        private void btnStageLiquidateSelected_Click(object sender, EventArgs e)
        {
            stageOrders(true, true);
        }

        private void addFutureRowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            addFutureRow(null, null, 0);
        }

        private int clearGridDataRowContractsAndReturnLots(
            string broker, string acct,
            string contractType,
            double strikePrice,
            int futureContractYear, int futureContractMonth,
            int optionYear, int optionMonth)
        {
            int returnNumberOfLots = 0;

            int checkRow = 0;
            //bool alreadyIn = false;
            while (checkRow < gridViewSpreadGrid.Rows.Count)
            {
                int colCheck = 0;
                bool continueToCheckRow = true;
                while (colCheck <= (int)OPTION_PL_COLUMNS.DEL_ROW)
                {
                    if (gridViewSpreadGrid.Rows[checkRow].Cells[colCheck].Value == null)
                    {
                        continueToCheckRow = false;
                        break;
                    }

                    colCheck++;
                }


                if (continueToCheckRow
                        && gridViewSpreadGrid.Rows[checkRow].Cells[(int)OPTION_PL_COLUMNS.CNTRT_TYPE].Value
                        .ToString().CompareTo(contractType) == 0)
                {

                    bool brokerAcctMatch = false;

                    if (gridViewSpreadGrid.Rows[checkRow].Cells[(int)OPTION_PL_COLUMNS.BRKR].Value.ToString()
                        .CompareTo(broker) == 0
                        &&
                        gridViewSpreadGrid.Rows[checkRow].Cells[(int)OPTION_PL_COLUMNS.ACCT].Value.ToString()
                        .CompareTo(acct) == 0)
                    {
                        brokerAcctMatch = true;
                    }

                    if (contractType.CompareTo(
                        optionArrayTypes.optionSpreadContractTypesArray.GetValue((int)OPTION_SPREAD_CONTRACT_TYPE.FUTURE)
                            .ToString()) == 0)
                    {
                        if (brokerAcctMatch
                            &&
                            Convert.ToInt32(gridViewSpreadGrid.Rows[checkRow].Cells[(int)OPTION_PL_COLUMNS.YEAR].Value) ==
                            futureContractYear

                        && Convert.ToInt32(gridViewSpreadGrid.Rows[checkRow].Cells[(int)OPTION_PL_COLUMNS.MTH_AS_INT].Value) ==
                            futureContractMonth
                            )
                        {
                            returnNumberOfLots = Convert.ToInt32(gridViewSpreadGrid.Rows[checkRow].Cells[(int)OPTION_PL_COLUMNS.NET].Value);

                            gridViewSpreadGrid.Rows[checkRow].Cells[(int)OPTION_PL_COLUMNS.NET].Value = 0;
                            //Convert.ToInt32(gridViewSpreadGrid.Rows[checkRow].Cells[(int)OPTION_PL_COLUMNS.NET].Value)
                            //    + numberOfContracts;

                            //alreadyIn = true;
                            break;
                        }
                    }
                    else
                    {
                        if (brokerAcctMatch
                            &&
                            Convert.ToDouble(gridViewSpreadGrid.Rows[checkRow].Cells[(int)OPTION_PL_COLUMNS.STRIKE].Value) ==
                            strikePrice

                        && Convert.ToInt32(gridViewSpreadGrid.Rows[checkRow].Cells[(int)OPTION_PL_COLUMNS.YEAR].Value) ==
                            optionYear

                        && Convert.ToInt32(gridViewSpreadGrid.Rows[checkRow].Cells[(int)OPTION_PL_COLUMNS.MTH_AS_INT].Value) ==
                            optionMonth
                            )
                        {
                            returnNumberOfLots = Convert.ToInt32(gridViewSpreadGrid.Rows[checkRow].Cells[(int)OPTION_PL_COLUMNS.NET].Value);

                            gridViewSpreadGrid.Rows[checkRow].Cells[(int)OPTION_PL_COLUMNS.NET].Value = 0;

                            //gridViewSpreadGrid.Rows[checkRow].Cells[(int)OPTION_PL_COLUMNS.NET].Value =
                            //    Convert.ToInt32(gridViewSpreadGrid.Rows[checkRow].Cells[(int)OPTION_PL_COLUMNS.NET].Value)
                            //        + numberOfContracts;

                            //alreadyIn = true;
                            break;
                        }
                    }

                }

                checkRow++;
            }

            return returnNumberOfLots;
        }

        private bool fillInGridDataRow(int row,
            string broker, string acct,
            OPTION_SPREAD_CONTRACT_TYPE osct,
            double strikePrice, double defaultPrice,
            double impliedVol, int futureContractYear, int futureContractMonth,
            int optionYear, int optionMonth, string product_code,
            int numberOfContracts, double daysToExp)
        {


            int checkRow = 0;
            bool alreadyIn = false;
            while (checkRow < gridViewSpreadGrid.Rows.Count)
            {
                int colCheck = 0;
                bool continueToCheckRow = true;
                while (colCheck <= (int)OPTION_PL_COLUMNS.DEL_ROW)
                {
                    if (gridViewSpreadGrid.Rows[checkRow].Cells[colCheck].Value == null)
                    {
                        continueToCheckRow = false;
                        break;
                    }

                    colCheck++;
                }


                if (continueToCheckRow
                        && gridViewSpreadGrid.Rows[checkRow].Cells[(int)OPTION_PL_COLUMNS.CNTRT_TYPE].Value
                        .ToString().CompareTo(optionArrayTypes.optionSpreadContractTypesArray.GetValue((int)osct)
                            .ToString()) == 0)
                {

                    bool brokerAcctMatch = false;

                    if (gridViewSpreadGrid.Rows[checkRow].Cells[(int)OPTION_PL_COLUMNS.BRKR].Value.ToString()
                        .CompareTo(broker) == 0
                        &&
                        gridViewSpreadGrid.Rows[checkRow].Cells[(int)OPTION_PL_COLUMNS.ACCT].Value.ToString()
                        .CompareTo(acct) == 0)
                    {
                        brokerAcctMatch = true;
                    }

                    if (osct == OPTION_SPREAD_CONTRACT_TYPE.FUTURE)
                    {
                        if (brokerAcctMatch
                            &&
                            Convert.ToInt32(gridViewSpreadGrid.Rows[checkRow].Cells[(int)OPTION_PL_COLUMNS.YEAR].Value) ==
                            futureContractYear

                        && Convert.ToInt32(gridViewSpreadGrid.Rows[checkRow].Cells[(int)OPTION_PL_COLUMNS.MTH_AS_INT].Value) ==
                            futureContractMonth
                            )
                        {
                            gridViewSpreadGrid.Rows[checkRow].Cells[(int)OPTION_PL_COLUMNS.NET].Value =
                                Convert.ToInt32(gridViewSpreadGrid.Rows[checkRow].Cells[(int)OPTION_PL_COLUMNS.NET].Value)
                                    + numberOfContracts;

                            alreadyIn = true;
                            break;
                        }
                    }
                    else
                    {
                        if (brokerAcctMatch
                            &&
                            Convert.ToDouble(gridViewSpreadGrid.Rows[checkRow].Cells[(int)OPTION_PL_COLUMNS.STRIKE].Value) ==
                            strikePrice

                        && Convert.ToInt32(gridViewSpreadGrid.Rows[checkRow].Cells[(int)OPTION_PL_COLUMNS.YEAR].Value) ==
                            optionYear

                        && Convert.ToInt32(gridViewSpreadGrid.Rows[checkRow].Cells[(int)OPTION_PL_COLUMNS.MTH_AS_INT].Value) ==
                            optionMonth

                        && gridViewSpreadGrid.Rows[checkRow].Cells[(int)OPTION_PL_COLUMNS.PRDT_CODE].Value.ToString().Trim().CompareTo(
                            product_code.Trim()) == 0
                            )
                        {
                            gridViewSpreadGrid.Rows[checkRow].Cells[(int)OPTION_PL_COLUMNS.NET].Value =
                                Convert.ToInt32(gridViewSpreadGrid.Rows[checkRow].Cells[(int)OPTION_PL_COLUMNS.NET].Value)
                                    + numberOfContracts;

                            alreadyIn = true;
                            break;
                        }
                    }

                }

                checkRow++;
            }

            if (!alreadyIn)
            {
                gridViewSpreadGrid.Rows.Add();

                addGridDeleteImg(row, (int)OPTION_PL_COLUMNS.DEL_ROW);

                gridViewSpreadGrid.Rows[row].Cells[(int)OPTION_PL_COLUMNS.BRKR].Value = broker;

                gridViewSpreadGrid.Rows[row].Cells[(int)OPTION_PL_COLUMNS.ACCT].Value = acct;

                gridViewSpreadGrid.Rows[row].Cells[(int)OPTION_PL_COLUMNS.CNTRT_TYPE].Value =
                    optionArrayTypes.optionSpreadContractTypesArray.GetValue((int)osct)
                            .ToString();

                gridViewSpreadGrid.Rows[row].Cells[(int)OPTION_PL_COLUMNS.PRDT_CODE].Value =
                    product_code;

                gridViewSpreadGrid.Rows[row].Cells[(int)OPTION_PL_COLUMNS.STRIKE].Value =
                    strikePrice;

                if (osct == OPTION_SPREAD_CONTRACT_TYPE.FUTURE)
                {
                    if (instrument != null)
                    {

                        //avgPrice = contractListOfRollovers[contractCount].expression.defaultPrice;

                        gridViewSpreadGrid.Rows[row].Cells[(int)OPTION_PL_COLUMNS.AVG_PRC].Value =
                            ConversionAndFormatting.convertToTickMovesString(
                            defaultPrice,
                            instrument.ticksize,
                            instrument.tickdisplay);



                    }
                    else
                    {
                        gridViewSpreadGrid.Rows[row].Cells[(int)OPTION_PL_COLUMNS.AVG_PRC].Value = 0;

                    }

                    gridViewSpreadGrid.Rows[row].Cells[(int)OPTION_PL_COLUMNS.IMPL_VOL].Value = 0;

                    //ConversionAndFormatting.convertToTickMovesString(
                    //        avgPrice,
                    //        optionSpreadExpressionList[contractSummaryExpressionListIdx[contractCount]].instrument.ticksize,
                    //        optionSpreadExpressionList[contractSummaryExpressionListIdx[contractCount]].instrument.tickDisplay);

                    gridViewSpreadGrid.Rows[row].Cells[(int)OPTION_PL_COLUMNS.YEAR].Value =
                        futureContractYear;

                    gridViewSpreadGrid.Rows[row].Cells[(int)OPTION_PL_COLUMNS.MTH_AS_INT].Value =
                        futureContractMonth;
                }
                else
                {

                    if (instrument != null)
                    {

                        double optionticksize = OptionSpreadManager.chooseoptionticksize(
                            defaultPrice,
                            instrument.optionticksize,
                            instrument.secondaryoptionticksize,
                            instrument.secondaryoptionticksizerule);

                        double tickDisplay = OptionSpreadManager.chooseoptionticksize(
                            defaultPrice,
                            instrument.optiontickdisplay,
                            instrument.secondaryoptiontickdisplay,
                            instrument.secondaryoptionticksizerule);

                        gridViewSpreadGrid.Rows[row].Cells[(int)OPTION_PL_COLUMNS.AVG_PRC].Value =
                                ConversionAndFormatting.convertToTickMovesString(
                                defaultPrice,
                                optionticksize,
                                tickDisplay);

                        gridViewSpreadGrid.Rows[row].Cells[(int)OPTION_PL_COLUMNS.IMPL_VOL].Value =
                            impliedVol;
                    }
                    else
                    {
                        gridViewSpreadGrid.Rows[row].Cells[(int)OPTION_PL_COLUMNS.AVG_PRC].Value = 0;

                        gridViewSpreadGrid.Rows[row].Cells[(int)OPTION_PL_COLUMNS.IMPL_VOL].Value = 0;
                    }


                    gridViewSpreadGrid.Rows[row].Cells[(int)OPTION_PL_COLUMNS.YEAR].Value =
                        optionYear;

                    gridViewSpreadGrid.Rows[row].Cells[(int)OPTION_PL_COLUMNS.MTH_AS_INT].Value =
                        optionMonth;
                }

                gridViewSpreadGrid.Rows[row].Cells[(int)OPTION_PL_COLUMNS.NET].Value =
                    numberOfContracts;



                gridViewSpreadGrid.Rows[row].Cells[(int)OPTION_PL_COLUMNS.DAYS_TO_EXP].Value =
                    daysToExp;
            }

            //hideNet0Rows(true);

            return !alreadyIn;

        }



        private void addOptionWing(OPTION_SPREAD_CONTRACT_TYPE contractTypeWing)
        {
            //#if DEBUG
            try
            //#endif
            {

                int futureIdx = -1;
                int legCount = 0;

                double optionImplVol = 0;
                double optionCount = 0;

                double daysToExpOfOption = 0;
                bool daysToExpFilled = false;

                bool product_code_Filled = false;

                int optionYear = 0;
                int optionMonthInt = 0;

                string product_code = "";

                DataGridViewSelectedRowCollection selectedRows = gridViewSpreadGrid.SelectedRows;

                bool foundImplVol = false;

                if (selectedRows.Count > 0)
                {

                    String contractType = selectedRows[0]
                               .Cells[(int)OPTION_PL_COLUMNS.CNTRT_TYPE].Value.ToString();

                    if (contractType.CompareTo(optionArrayTypes.optionSpreadContractTypesArray.GetValue(
                            (int)OPTION_SPREAD_CONTRACT_TYPE.CALL)) == 0
                        || contractType.CompareTo(optionArrayTypes.optionSpreadContractTypesArray.GetValue(
                            (int)OPTION_SPREAD_CONTRACT_TYPE.PUT)) == 0
                        )
                    {
                        if (selectedRows[0].Cells[(int)OPTION_PL_COLUMNS.IMPL_VOL].Value != null)
                        {
                            optionImplVol = Convert.ToDouble(selectedRows[0]
                                    .Cells[(int)OPTION_PL_COLUMNS.IMPL_VOL].Value);

                            foundImplVol = true;
                        }

                        if (selectedRows[0].Cells[(int)OPTION_PL_COLUMNS.DAYS_TO_EXP].Value != null)
                        {
                            daysToExpOfOption = Convert.ToDouble(selectedRows[0]
                                    .Cells[(int)OPTION_PL_COLUMNS.DAYS_TO_EXP].Value) / 365;

                            optionYear = Convert.ToInt16(selectedRows[0]
                                    .Cells[(int)OPTION_PL_COLUMNS.YEAR].Value);

                            optionMonthInt = Convert.ToInt16(selectedRows[0]
                                    .Cells[(int)OPTION_PL_COLUMNS.MTH_AS_INT].Value);

                            daysToExpFilled = true;
                        }

                        if (selectedRows[0].Cells[(int)OPTION_PL_COLUMNS.PRDT_CODE].Value != null)
                        {
                            product_code = selectedRows[0]
                                    .Cells[(int)OPTION_PL_COLUMNS.PRDT_CODE].Value.ToString();

                            product_code_Filled = true;
                        }

                    }
                }


                while (legCount < gridViewSpreadGrid.RowCount)
                {
                    if (gridViewSpreadGrid.Rows[legCount]
                                .Cells[(int)OPTION_PL_COLUMNS.CNTRT_TYPE].Value != null)
                    {
                        String contractType = gridViewSpreadGrid.Rows[legCount]
                                    .Cells[(int)OPTION_PL_COLUMNS.CNTRT_TYPE].Value.ToString();

                        if (contractType.CompareTo(optionArrayTypes.optionSpreadContractTypesArray.GetValue(
                                (int)OPTION_SPREAD_CONTRACT_TYPE.FUTURE)) == 0)
                        {
                            futureIdx = legCount;
                            //break;
                        }
                        else
                        {
                            if (!foundImplVol)
                            {
                                if (gridViewSpreadGrid.Rows[legCount]
                                    .Cells[(int)OPTION_PL_COLUMNS.IMPL_VOL].Value != null)
                                {
                                    optionImplVol += Convert.ToDouble(gridViewSpreadGrid.Rows[legCount]
                                            .Cells[(int)OPTION_PL_COLUMNS.IMPL_VOL].Value);

                                    optionCount++;
                                }
                            }

                            if (!daysToExpFilled
                                && gridViewSpreadGrid.Rows[legCount]
                                        .Cells[(int)OPTION_PL_COLUMNS.DAYS_TO_EXP].Value != null)
                            {
                                daysToExpOfOption = Convert.ToDouble(gridViewSpreadGrid.Rows[legCount]
                                        .Cells[(int)OPTION_PL_COLUMNS.DAYS_TO_EXP].Value) / 365;

                                optionYear = Convert.ToInt16(gridViewSpreadGrid.Rows[legCount]
                                        .Cells[(int)OPTION_PL_COLUMNS.YEAR].Value);

                                optionMonthInt = Convert.ToInt16(gridViewSpreadGrid.Rows[legCount]
                                        .Cells[(int)OPTION_PL_COLUMNS.MTH_AS_INT].Value);

                                daysToExpFilled = true;
                            }

                            if (!product_code_Filled
                                && gridViewSpreadGrid.Rows[legCount]
                                        .Cells[(int)OPTION_PL_COLUMNS.PRDT_CODE].Value != null)
                            {
                                product_code = gridViewSpreadGrid.Rows[legCount]
                                        .Cells[(int)OPTION_PL_COLUMNS.PRDT_CODE].Value.ToString();

                                product_code_Filled = true;
                            }
                        }
                    }

                    legCount++;
                }

                if (!foundImplVol)
                {
                    if (optionCount > 0)
                    {
                        optionImplVol /= optionCount;
                    }
                }

                optionImplVol /= 100;


                if (futureIdx > -1)
                {
                    if (gridViewSpreadGrid.Rows[futureIdx]
                                .Cells[(int)OPTION_PL_COLUMNS.AVG_PRC].Value != null)
                    {

                        double futureClose = ConversionAndFormatting.convertToTickMovesDouble(
                                    gridViewSpreadGrid.Rows[futureIdx]
                                        .Cells[(int)OPTION_PL_COLUMNS.AVG_PRC].Value.ToString(),
                                    instrument.ticksize, instrument.tickdisplay);


                        double futurePriceFactorForAllLegs = futureClose / instrument.optionstrikeincrement;

                        double futurePriceFactor = optionSpreadManager.roundPriceForCallOrPut(
                                            OPTION_SPREAD_CONTRACT_TYPE.PUT,
                                            futurePriceFactorForAllLegs, instrument.optionstrikeincrement);

                        int refStartOfFuturePriceCount = -TradingSystemConstants.STRIKE_PRICE_REFERENCE_COUNT / 2;

                        int strikeCount = 0;

                        int strikeCountLimit = 20;

                        if (priceLimitTickTextBox.Text != null)
                        {
                            strikeCountLimit = Convert.ToInt16(strikeIncrLimitTextBox.Text);
                        }

                        while (strikeCount <= strikeCountLimit)
                        {

                            char contractCallOrPutChar = 'C';
                            double strikeDirection = 1;

                            if (contractTypeWing == OPTION_SPREAD_CONTRACT_TYPE.PUT)
                            {
                                contractCallOrPutChar = 'P';
                                strikeDirection = -1;
                            }



                            double tempFuturePriceFactor = futurePriceFactor
                                        + (refStartOfFuturePriceCount * instrument.optionstrikeincrement);

                            double strikeTest =
                                strikeDirection * strikeCount * instrument.optionstrikeincrement
                                    + tempFuturePriceFactor;


                            double rfr = Convert.ToDouble(riskFreeTextBox.Text) / 100;



                            double price = OptionCalcs.blackScholes(contractCallOrPutChar,
                                futureClose,
                                        strikeTest,
                                        daysToExpOfOption, rfr,
                                        optionImplVol);

                            //TSErrorCatch.debugWriteOut(price + " priceinstrument.optionticksize " + price / instrument.optionticksize
                            //    + " ticksize " + instrument.optionticksize);

                            int priceTickLimit = 5;

                            if (priceLimitTickTextBox.Text != null)
                            {
                                priceTickLimit = Convert.ToInt16(priceLimitTickTextBox.Text);
                            }

                            if (price / instrument.optionticksize <= priceTickLimit
                                ||
                                strikeCount == strikeCountLimit)
                            {
                                int rowCount = gridViewSpreadGrid.Rows.Count;

                                for (int groupAllocCnt = 0; groupAllocCnt < DataCollectionLibrary.portfolioAllocation.accountAllocation.Count; groupAllocCnt++)
                                {
                                    if (brokerAccountChosen.Contains(Tuple.Create(DataCollectionLibrary.portfolioAllocation.accountAllocation[groupAllocCnt].broker,
                                            DataCollectionLibrary.portfolioAllocation.accountAllocation[groupAllocCnt].account)))
                                    {

                                        string acct = optionSpreadManager.selectAcct(
                                        instrument.exchangesymbol,
                                        DataCollectionLibrary.portfolioAllocation.accountAllocation[groupAllocCnt], true);

                                        if (
                                            fillInGridDataRow(rowCount,
                                        DataCollectionLibrary.portfolioAllocation.accountAllocation[groupAllocCnt].broker,
                                        acct,
                                        contractTypeWing,
                                        strikeTest,
                                        price,
                                        optionImplVol * 100,
                                        0,
                                        0,
                                        optionYear,
                                        optionMonthInt,
                                        product_code,
                                        Convert.ToInt16(DataCollectionLibrary.accountList[groupAllocCnt].info.size_factor),
                                        daysToExpOfOption * 365
                                        ))
                                        {
                                            rowCount++;
                                        }
                                    }
                                }

                                break; //breaks while strike loop

                            }

                            strikeCount++;
                        }
                    }
                }

            }

            //#if DEBUG
            catch (Exception ex)
            {
                TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
            }
            //#endif

            //hideNet0Rows(true);
        }

        private void addPutWingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            addOptionWing(OPTION_SPREAD_CONTRACT_TYPE.PUT);
        }

        private void addCallWingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            addOptionWing(OPTION_SPREAD_CONTRACT_TYPE.CALL);
        }

        private void syntheticOption(
            OPTION_SPREAD_CONTRACT_TYPE contractBeingReplaced,
            OPTION_SPREAD_CONTRACT_TYPE syntheticReplacementContract,
            int futureMultiplier, int optionMultiplier)
        {
#if DEBUG
            try
#endif
            {
                DataGridViewSelectedRowCollection selectedRows = gridViewSpreadGrid.SelectedRows;



                if (selectedRows.Count > 0)
                {
                    string contractTypeInSelectedRow = selectedRows[0].Cells[(int)OPTION_PL_COLUMNS.CNTRT_TYPE].Value.ToString();

                    if (contractTypeInSelectedRow.CompareTo(optionArrayTypes.optionSpreadContractTypesArray.GetValue(
                                    (int)contractBeingReplaced)) == 0)
                    {



                        int futureIdx = -1;
                        int legCount = 0;

                        double optionImplVol = 0;
                        double optionCount = 0;

                        //double daysToExpOfOption = 0;
                        //bool daysToExpFilled = false;

                        //int optionYear = 0;
                        //int optionMonthInt = 0;


                        double daysToExpOfOption = Convert.ToDouble(selectedRows[0]
                                    .Cells[(int)OPTION_PL_COLUMNS.DAYS_TO_EXP].Value) / 365;

                        int optionYear = Convert.ToInt16(selectedRows[0]
                                    .Cells[(int)OPTION_PL_COLUMNS.YEAR].Value);

                        int optionMonthInt = Convert.ToInt16(selectedRows[0]
                                    .Cells[(int)OPTION_PL_COLUMNS.MTH_AS_INT].Value);

                        double strike = Convert.ToDouble(selectedRows[0]
                                    .Cells[(int)OPTION_PL_COLUMNS.STRIKE].Value);

                        string product_code = selectedRows[0]
                                    .Cells[(int)OPTION_PL_COLUMNS.PRDT_CODE].Value.ToString();


                        while (legCount < gridViewSpreadGrid.RowCount)
                        {
                            if (gridViewSpreadGrid.Rows[legCount]
                                        .Cells[(int)OPTION_PL_COLUMNS.CNTRT_TYPE].Value != null)
                            {
                                String contractType = gridViewSpreadGrid.Rows[legCount]
                                            .Cells[(int)OPTION_PL_COLUMNS.CNTRT_TYPE].Value.ToString();

                                if (contractType.CompareTo(
                                    optionArrayTypes.optionSpreadContractTypesArray.GetValue(
                                        (int)OPTION_SPREAD_CONTRACT_TYPE.FUTURE)) == 0)
                                {
                                    futureIdx = legCount;
                                    //break;
                                }
                                else
                                {
                                    if (gridViewSpreadGrid.Rows[legCount]
                                        .Cells[(int)OPTION_PL_COLUMNS.IMPL_VOL].Value != null)
                                    {
                                        optionImplVol += Convert.ToDouble(gridViewSpreadGrid.Rows[legCount]
                                                .Cells[(int)OPTION_PL_COLUMNS.IMPL_VOL].Value);

                                        optionCount++;
                                    }


                                }
                            }

                            legCount++;
                        }

                        if (optionCount > 0)
                        {
                            optionImplVol /= optionCount;

                            optionImplVol /= 100;
                        }

                        if (futureIdx > -1)
                        {
                            if (gridViewSpreadGrid.Rows[futureIdx]
                                        .Cells[(int)OPTION_PL_COLUMNS.AVG_PRC].Value != null)
                            {

                                double futureClose = ConversionAndFormatting.convertToTickMovesDouble(
                                            gridViewSpreadGrid.Rows[futureIdx]
                                                .Cells[(int)OPTION_PL_COLUMNS.AVG_PRC].Value.ToString(),
                                            instrument.ticksize, instrument.tickdisplay);


                                double futurePriceFactorForAllLegs = futureClose / instrument.optionstrikeincrement;

                                double futurePriceFactor = optionSpreadManager.roundPriceForCallOrPut(
                                                    OPTION_SPREAD_CONTRACT_TYPE.PUT,
                                                    futurePriceFactorForAllLegs, instrument.optionstrikeincrement);

                                int refStartOfFuturePriceCount = -TradingSystemConstants.STRIKE_PRICE_REFERENCE_COUNT / 2;

                                int futureExpressionIdx = getFutureExpressionIdx();

                                //int strikeCount = 0;

                                //int strikeCountLimit = 20;



                                //while (strikeCount <= strikeCountLimit)
                                {

                                    char contractCallOrPutChar = 'C';
                                    //int futureLongOrShort = -1;

                                    if (syntheticReplacementContract == OPTION_SPREAD_CONTRACT_TYPE.PUT)
                                    {
                                        contractCallOrPutChar = 'P';
                                        //futureLongOrShort = 1;
                                    }



                                    double tempFuturePriceFactor = futurePriceFactor
                                                + (refStartOfFuturePriceCount * instrument.optionstrikeincrement);

                                    //double strikeTest =
                                    //    strikeDirection * strikeCount * instrument.optionstrikeincrement
                                    //        + tempFuturePriceFactor;


                                    double rfr = Convert.ToDouble(riskFreeTextBox.Text) / 100;



                                    double price = OptionCalcs.blackScholes(contractCallOrPutChar,
                                        futureClose,
                                                strike,
                                                 daysToExpOfOption, rfr,
                                                optionImplVol);



                                    int rowCount = gridViewSpreadGrid.Rows.Count;

                                    for (int groupAllocCnt = 0; groupAllocCnt < DataCollectionLibrary.portfolioAllocation.accountAllocation.Count; groupAllocCnt++)
                                    {
                                        if (brokerAccountChosen.Contains(Tuple.Create(DataCollectionLibrary.portfolioAllocation.accountAllocation[groupAllocCnt].broker,
                                            DataCollectionLibrary.portfolioAllocation.accountAllocation[groupAllocCnt].account)))
                                        {

                                            string acct = optionSpreadManager.selectAcct(
                                            instrument.exchangesymbol,
                                            DataCollectionLibrary.portfolioAllocation.accountAllocation[groupAllocCnt], true);

                                            if (
                                                fillInGridDataRow(rowCount,
                                            DataCollectionLibrary.portfolioAllocation.accountAllocation[groupAllocCnt].broker,
                                            acct,
                                            syntheticReplacementContract,
                                            strike,
                                            price,
                                            optionImplVol * 100,
                                            0,
                                            0,
                                            optionYear,
                                            optionMonthInt,
                                            product_code,
                                            Convert.ToInt16(DataCollectionLibrary.accountList[groupAllocCnt].info.size_factor)
                                                * optionMultiplier,
                                            daysToExpOfOption * 365
                                            ))
                                            {
                                                rowCount++;
                                            }



                                            if (
                                                fillInGridDataRow(rowCount,
                                            DataCollectionLibrary.portfolioAllocation.accountAllocation[groupAllocCnt].broker,
                                            acct,
                                            OPTION_SPREAD_CONTRACT_TYPE.FUTURE,
                                            0,
                                            futureClose,
                                            0,
                                            DataCollectionLibrary.optionSpreadExpressionList[futureExpressionIdx].futureContractYear,
                                            DataCollectionLibrary.optionSpreadExpressionList[futureExpressionIdx].futureContractMonthInt,
                                            0,
                                            0,
                                            instrument.exchangesymbolTT,
                                            Convert.ToInt16(DataCollectionLibrary.accountList[groupAllocCnt].info.size_factor)
                                                * futureMultiplier,
                                            0
                                            ))
                                            {
                                                rowCount++;
                                            }

                                        }


                                    }

                                }
                            }
                        }
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


        private void replaceWithSyntheticOption(
            OPTION_SPREAD_CONTRACT_TYPE contractBeingReplaced,
            OPTION_SPREAD_CONTRACT_TYPE syntheticReplacementContract)
        {
#if DEBUG
            try
#endif
            {
                DataGridViewSelectedRowCollection selectedRows = gridViewSpreadGrid.SelectedRows;



                if (selectedRows.Count > 0)
                {
                    string contractTypeInSelectedRow = selectedRows[0].Cells[(int)OPTION_PL_COLUMNS.CNTRT_TYPE].Value.ToString();

                    if (contractTypeInSelectedRow.CompareTo(optionArrayTypes.optionSpreadContractTypesArray.GetValue(
                                    (int)contractBeingReplaced)) == 0)
                    {



                        int futureIdx = -1;
                        int legCount = 0;

                        double optionImplVol = 0;
                        double optionCount = 0;


                        double daysToExpOfOption = Convert.ToDouble(selectedRows[0]
                                    .Cells[(int)OPTION_PL_COLUMNS.DAYS_TO_EXP].Value) / 365;

                        int optionYear = Convert.ToInt16(selectedRows[0]
                                    .Cells[(int)OPTION_PL_COLUMNS.YEAR].Value);

                        int optionMonthInt = Convert.ToInt16(selectedRows[0]
                                    .Cells[(int)OPTION_PL_COLUMNS.MTH_AS_INT].Value);

                        double strike = Convert.ToDouble(selectedRows[0]
                                    .Cells[(int)OPTION_PL_COLUMNS.STRIKE].Value);

                        string product_code = selectedRows[0]
                                    .Cells[(int)OPTION_PL_COLUMNS.PRDT_CODE].Value.ToString();


                        while (legCount < gridViewSpreadGrid.RowCount)
                        {
                            if (gridViewSpreadGrid.Rows[legCount]
                                        .Cells[(int)OPTION_PL_COLUMNS.CNTRT_TYPE].Value != null)
                            {
                                String contractType = gridViewSpreadGrid.Rows[legCount]
                                            .Cells[(int)OPTION_PL_COLUMNS.CNTRT_TYPE].Value.ToString();

                                if (contractType.CompareTo(
                                    optionArrayTypes.optionSpreadContractTypesArray.GetValue(
                                        (int)OPTION_SPREAD_CONTRACT_TYPE.FUTURE)) == 0)
                                {
                                    futureIdx = legCount;
                                    //break;
                                }
                                else
                                {
                                    if (gridViewSpreadGrid.Rows[legCount]
                                        .Cells[(int)OPTION_PL_COLUMNS.IMPL_VOL].Value != null)
                                    {
                                        optionImplVol += Convert.ToDouble(gridViewSpreadGrid.Rows[legCount]
                                                .Cells[(int)OPTION_PL_COLUMNS.IMPL_VOL].Value);

                                        optionCount++;
                                    }


                                }
                            }

                            legCount++;
                        }

                        if (optionCount > 0)
                        {
                            optionImplVol /= optionCount;

                            optionImplVol /= 100;
                        }

                        if (futureIdx > -1)
                        {
                            if (gridViewSpreadGrid.Rows[futureIdx]
                                        .Cells[(int)OPTION_PL_COLUMNS.AVG_PRC].Value != null)
                            {

                                double futureClose = ConversionAndFormatting.convertToTickMovesDouble(
                                            gridViewSpreadGrid.Rows[futureIdx]
                                                .Cells[(int)OPTION_PL_COLUMNS.AVG_PRC].Value.ToString(),
                                            instrument.ticksize, instrument.tickdisplay);


                                double futurePriceFactorForAllLegs = futureClose / instrument.optionstrikeincrement;

                                double futurePriceFactor = optionSpreadManager.roundPriceForCallOrPut(
                                                    OPTION_SPREAD_CONTRACT_TYPE.PUT,
                                                    futurePriceFactorForAllLegs, instrument.optionstrikeincrement);

                                int refStartOfFuturePriceCount = -TradingSystemConstants.STRIKE_PRICE_REFERENCE_COUNT / 2;

                                int futureExpressionIdx = getFutureExpressionIdx();


                                //while (strikeCount <= strikeCountLimit)
                                {

                                    char contractCallOrPutChar = 'C';
                                    //int futureLongOrShort = -1;

                                    if (syntheticReplacementContract == OPTION_SPREAD_CONTRACT_TYPE.PUT)
                                    {
                                        contractCallOrPutChar = 'P';
                                        //futureLongOrShort = 1;
                                    }



                                    double tempFuturePriceFactor = futurePriceFactor
                                                + (refStartOfFuturePriceCount * instrument.optionstrikeincrement);


                                    double rfr = Convert.ToDouble(riskFreeTextBox.Text) / 100;



                                    double price = OptionCalcs.blackScholes(contractCallOrPutChar,
                                        futureClose,
                                                strike,
                                                 daysToExpOfOption, rfr,
                                                optionImplVol);



                                    int rowCount = gridViewSpreadGrid.Rows.Count;

                                    for (int groupAllocCnt = 0; groupAllocCnt < DataCollectionLibrary.portfolioAllocation.accountAllocation.Count; groupAllocCnt++)
                                    {
                                        if (brokerAccountChosen.Contains(Tuple.Create(DataCollectionLibrary.portfolioAllocation.accountAllocation[groupAllocCnt].broker,
                                            DataCollectionLibrary.portfolioAllocation.accountAllocation[groupAllocCnt].account)))
                                        {

                                            string acct = optionSpreadManager.selectAcct(
                                            instrument.exchangesymbol,
                                            DataCollectionLibrary.portfolioAllocation.accountAllocation[groupAllocCnt], true);

                                            int net = clearGridDataRowContractsAndReturnLots(
                                                DataCollectionLibrary.portfolioAllocation.accountAllocation[groupAllocCnt].broker,
                                                acct,
                                                optionArrayTypes.optionSpreadContractTypesArray.GetValue((int)contractBeingReplaced).ToString(),
                                                strike,
                                                optionYear,
                                                optionMonthInt,
                                                optionYear,
                                                optionMonthInt);

                                            int[] numberOfContractsToReplace = getSyntheticNumberOfContracts(net,
                                                optionArrayTypes.optionSpreadContractTypesArray.GetValue((int)contractBeingReplaced).ToString());


                                            if (
                                                fillInGridDataRow(rowCount,
                                            DataCollectionLibrary.portfolioAllocation.accountAllocation[groupAllocCnt].broker,
                                            acct,
                                            syntheticReplacementContract,
                                            strike,
                                            price,
                                            optionImplVol * 100,
                                            0,
                                            0,
                                            optionYear,
                                            optionMonthInt,
                                            product_code,
                                            numberOfContractsToReplace[1],
                                            daysToExpOfOption * 365
                                            ))
                                            {
                                                rowCount++;
                                            }



                                            if (
                                                fillInGridDataRow(rowCount,
                                            DataCollectionLibrary.portfolioAllocation.accountAllocation[groupAllocCnt].broker,
                                            acct,
                                            OPTION_SPREAD_CONTRACT_TYPE.FUTURE,
                                            0,
                                            futureClose,
                                            0,
                                            DataCollectionLibrary.optionSpreadExpressionList[futureExpressionIdx].futureContractYear,
                                            DataCollectionLibrary.optionSpreadExpressionList[futureExpressionIdx].futureContractMonthInt,
                                            0,
                                            0,
                                            instrument.exchangesymbolTT,
                                            numberOfContractsToReplace[0],
                                            0
                                            ))
                                            {
                                                rowCount++;
                                            }

                                        }

                                    }

                                }
                            }
                        }
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

        private void addSyntheticPutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            syntheticOption(
                OPTION_SPREAD_CONTRACT_TYPE.PUT,
                OPTION_SPREAD_CONTRACT_TYPE.CALL, -1, 1);
        }

        private void addSyntheticCallToolStripMenuItem_Click(object sender, EventArgs e)
        {
            syntheticOption(
                OPTION_SPREAD_CONTRACT_TYPE.CALL,
                OPTION_SPREAD_CONTRACT_TYPE.PUT, 1, 1);
        }

        private void addSyntheticPutShortToolStripMenuItem_Click(object sender, EventArgs e)
        {
            syntheticOption(
                OPTION_SPREAD_CONTRACT_TYPE.PUT,
                OPTION_SPREAD_CONTRACT_TYPE.CALL, 1, -1);
        }

        private void addSyntheticCallShortToolStripMenuItem_Click(object sender, EventArgs e)
        {
            syntheticOption(
                OPTION_SPREAD_CONTRACT_TYPE.CALL,
                OPTION_SPREAD_CONTRACT_TYPE.PUT, -1, -1);
        }

        private void replaceWithSyntheticToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataGridViewSelectedRowCollection selectedRows = gridViewSpreadGrid.SelectedRows;

            if (selectedRows.Count > 0)
            {
                //string contractTypeInSelectedRow = selectedRows[0].Cells[(int)OPTION_PL_COLUMNS.NET].Value.ToString();

                int net = Convert.ToInt16(selectedRows[0]
                                        .Cells[(int)OPTION_PL_COLUMNS.NET].Value);

                int absnet = Math.Abs(net);

                String contractType = selectedRows[0]
                               .Cells[(int)OPTION_PL_COLUMNS.CNTRT_TYPE].Value.ToString();

                if (contractType.CompareTo(optionArrayTypes.optionSpreadContractTypesArray.GetValue(
                        (int)OPTION_SPREAD_CONTRACT_TYPE.CALL)) == 0)
                {
                    if (net > 0)
                    {
                        replaceWithSyntheticOption(
                            OPTION_SPREAD_CONTRACT_TYPE.CALL,
                            OPTION_SPREAD_CONTRACT_TYPE.PUT);
                    }
                    else if (net < 0)
                    {
                        replaceWithSyntheticOption(
                            OPTION_SPREAD_CONTRACT_TYPE.CALL,
                            OPTION_SPREAD_CONTRACT_TYPE.PUT);
                    }
                }
                else if (contractType.CompareTo(optionArrayTypes.optionSpreadContractTypesArray.GetValue(
                        (int)OPTION_SPREAD_CONTRACT_TYPE.PUT)) == 0)
                {
                    if (net > 0)
                    {
                        replaceWithSyntheticOption(
                            OPTION_SPREAD_CONTRACT_TYPE.PUT,
                            OPTION_SPREAD_CONTRACT_TYPE.CALL);
                    }
                    else if (net < 0)
                    {
                        replaceWithSyntheticOption(
                            OPTION_SPREAD_CONTRACT_TYPE.PUT,
                            OPTION_SPREAD_CONTRACT_TYPE.CALL);
                    }
                }



            }
        }

        private int[] getSyntheticNumberOfContracts(int net, String contractType)
        {
            int[] numberOfLotsToReturn = new int[2];


            int absnet = Math.Abs(net);

            if (contractType.CompareTo(optionArrayTypes.optionSpreadContractTypesArray.GetValue(
                    (int)OPTION_SPREAD_CONTRACT_TYPE.CALL)) == 0)
            {
                if (net > 0)
                {
                    numberOfLotsToReturn[0] = absnet;
                    numberOfLotsToReturn[1] = absnet;

                    //syntheticOption(
                    //    OPTION_SPREAD_CONTRACT_TYPE.CALL,
                    //    OPTION_SPREAD_CONTRACT_TYPE.PUT, absnet, absnet);
                }
                else if (net < 0)
                {
                    numberOfLotsToReturn[0] = -absnet;
                    numberOfLotsToReturn[1] = -absnet;

                    //syntheticOption(
                    //    OPTION_SPREAD_CONTRACT_TYPE.CALL,
                    //    OPTION_SPREAD_CONTRACT_TYPE.PUT, -absnet, -absnet);
                }
            }
            else if (contractType.CompareTo(optionArrayTypes.optionSpreadContractTypesArray.GetValue(
                    (int)OPTION_SPREAD_CONTRACT_TYPE.PUT)) == 0)
            {
                if (net > 0)
                {
                    numberOfLotsToReturn[0] = -absnet;
                    numberOfLotsToReturn[1] = absnet;

                    //syntheticOption(
                    //    OPTION_SPREAD_CONTRACT_TYPE.PUT,
                    //    OPTION_SPREAD_CONTRACT_TYPE.CALL, -absnet, absnet);
                }
                else if (net < 0)
                {
                    numberOfLotsToReturn[0] = absnet;
                    numberOfLotsToReturn[1] = -absnet;

                    //syntheticOption(
                    //    OPTION_SPREAD_CONTRACT_TYPE.PUT,
                    //    OPTION_SPREAD_CONTRACT_TYPE.CALL, absnet, -absnet);
                }
            }

            return numberOfLotsToReturn;

        }

        private void hideNet0Rows(bool hide)
        {
            int hiddenRows = 0;

            for (int row = 0; row < gridViewSpreadGrid.Rows.Count; row++)
            {
                if (hide)
                {
                    if (gridViewSpreadGrid.Rows[row].Cells[(int)OPTION_PL_COLUMNS.CNTRT_TYPE].Value != null
                        &&
                        gridViewSpreadGrid.Rows[row].Cells[(int)OPTION_PL_COLUMNS.CNTRT_TYPE].Value
                        .ToString().CompareTo(optionArrayTypes.optionSpreadContractTypesArray.GetValue(
                                (int)OPTION_SPREAD_CONTRACT_TYPE.FUTURE)) != 0)
                    {
                        if (gridViewSpreadGrid.Rows[row].Cells[(int)OPTION_PL_COLUMNS.NET].Value != null
                            &&
                            Convert.ToInt16(
                            gridViewSpreadGrid.Rows[row].Cells[(int)OPTION_PL_COLUMNS.NET].Value) == 0)
                        {
                            gridViewSpreadGrid.Rows[row].Visible = false;
                            hiddenRows++;
                        }
                        else
                        {
                            gridViewSpreadGrid.Rows[row].Visible = true;
                        }
                    }
                }
                else
                {
                    gridViewSpreadGrid.Rows[row].Visible = true;
                }
            }

            lblTotalRows.Text = "Total Rows: " + gridViewSpreadGrid.Rows.Count;

            lblHiddenRows.Text = " Hidden Rows: " + hiddenRows;
        }

        private void hideNet0RowsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            hideNet0Rows(true);
        }

        private void unhideNet0OptionRowsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            hideNet0Rows(false);
        }


        private double returnFuturesAveragePrice(int futureIdx)
        {
            double futureAvgPrice = 0;

            if (futureIdx >= 0)
            {
                if (gridViewSpreadGrid.Rows[futureIdx]
                                    .Cells[(int)OPTION_PL_COLUMNS.AVG_PRC].Value != null)
                {
                    futureAvgPrice = ConversionAndFormatting.convertToTickMovesDouble(
                                gridViewSpreadGrid.Rows[futureIdx]
                                    .Cells[(int)OPTION_PL_COLUMNS.AVG_PRC].Value.ToString(),
                                instrument.ticksize, instrument.tickdisplay);
                }
                else
                {
                    String caption = "FILL IN A PRICE FOR THE FUTURE CONTRACT";
                    String message = "FILL IN A PRICE FOR THE FUTURE CONTRACT";
                    MessageBoxButtons buttons = MessageBoxButtons.OK;
                    System.Windows.Forms.DialogResult result;

                    // Displays the MessageBox.
                    result = MessageBox.Show(message, caption, buttons, MessageBoxIcon.Error);
                }
            }
            else
            {


                int expCount = 0;

                while (expCount < DataCollectionLibrary.optionSpreadExpressionList.Count())
                {
                    if (DataCollectionLibrary.optionSpreadExpressionList[expCount].instrument != null
                        && instrument.idinstrument ==
                            DataCollectionLibrary.optionSpreadExpressionList[expCount].instrument.idinstrument
                        &&
                        DataCollectionLibrary.optionSpreadExpressionList[expCount].callPutOrFuture ==
                            OPTION_SPREAD_CONTRACT_TYPE.FUTURE)
                    {
                        futureAvgPrice = DataCollectionLibrary.optionSpreadExpressionList[expCount].defaultPrice;

                        break;
                    }

                    expCount++;
                }

            }

            return futureAvgPrice;
        }

        private void testMoneynessOfOptions(bool runHighlight)
        {
            int futureIndexLeg = findFutureLeg();

            double futureAveragePrice = returnFuturesAveragePrice(futureIndexLeg);

            for (int row = 0; row < gridViewSpreadGrid.Rows.Count; row++)
            {

                bool highlightedRow = false;
                if (runHighlight)
                {
                    if (gridViewSpreadGrid.Rows[row].Cells[(int)OPTION_PL_COLUMNS.CNTRT_TYPE].Value != null
                        &&
                        gridViewSpreadGrid.Rows[row].Cells[(int)OPTION_PL_COLUMNS.CNTRT_TYPE].Value
                        .ToString().CompareTo(optionArrayTypes.optionSpreadContractTypesArray.GetValue(
                                (int)OPTION_SPREAD_CONTRACT_TYPE.CALL)) == 0)
                    {
                        if (gridViewSpreadGrid.Rows[row].Cells[(int)OPTION_PL_COLUMNS.STRIKE].Value != null)
                        {
                            double strikePrice = Convert.ToDouble(
                                gridViewSpreadGrid.Rows[row].Cells[(int)OPTION_PL_COLUMNS.STRIKE].Value);

                            if ((futureAveragePrice - strikePrice) / instrument.optionstrikeincrement >= Convert.ToInt16(comboBox1.Text))
                            {
                                gridViewSpreadGrid.Rows[row].DefaultCellStyle.BackColor = Color.Yellow;

                                highlightedRow = true;
                            }
                        }
                    }
                    else if (gridViewSpreadGrid.Rows[row].Cells[(int)OPTION_PL_COLUMNS.CNTRT_TYPE].Value != null
                        &&
                        gridViewSpreadGrid.Rows[row].Cells[(int)OPTION_PL_COLUMNS.CNTRT_TYPE].Value
                        .ToString().CompareTo(optionArrayTypes.optionSpreadContractTypesArray.GetValue(
                                (int)OPTION_SPREAD_CONTRACT_TYPE.PUT)) == 0)
                    {
                        if (gridViewSpreadGrid.Rows[row].Cells[(int)OPTION_PL_COLUMNS.STRIKE].Value != null)
                        {
                            double strikePrice = Convert.ToDouble(
                                gridViewSpreadGrid.Rows[row].Cells[(int)OPTION_PL_COLUMNS.STRIKE].Value);

                            if ((strikePrice - futureAveragePrice) / instrument.optionstrikeincrement >= Convert.ToInt16(comboBox1.Text))
                            {
                                gridViewSpreadGrid.Rows[row].DefaultCellStyle.BackColor = Color.Yellow;

                                highlightedRow = true;
                            }
                        }
                    }
                }

                if (!highlightedRow)
                {
                    gridViewSpreadGrid.Rows[row].DefaultCellStyle.BackColor = Color.White;
                }

            }
        }

        private void highlightUnhighlightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            highlightingIntheMoney = !highlightingIntheMoney;

            testMoneynessOfOptions(highlightingIntheMoney);

            if (highlightingIntheMoney)
            {
                highlightUnhighlightToolStripMenuItem.BackColor = Color.Yellow;
            }
            else
            {
                highlightUnhighlightToolStripMenuItem.BackColor = menuStrip1.BackColor;
            }
        }


        internal void highlightModelFCMDifferences()
        {

            for (int row = 0; row < gridViewSpreadGrid.Rows.Count; row++)
            {
                //gridViewSpreadGrid.Rows[row].Selected = false;
                //gridViewSpreadGrid.SelectAll();

                if (gridViewSpreadGrid.Rows[row].Cells[(int)OPTION_PL_COLUMNS.NET].Value != null
                        &&
                        Convert.ToInt16(gridViewSpreadGrid.Rows[row].Cells[(int)OPTION_PL_COLUMNS.NET].Value) != 0)
                {

                    gridViewSpreadGrid.Rows[row].Cells[(int)OPTION_PL_COLUMNS.NET].Style.BackColor = Color.LimeGreen;

                }

            }
        }

        private void gridViewSpreadGrid_CellMouseClick_1(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex < 0 || e.RowIndex < 0)
            {

                return;

            }

            if (e.ColumnIndex == (int)OPTION_PL_COLUMNS.DEL_ROW)
            {

                gridViewSpreadGrid.Rows.RemoveAt(e.RowIndex);

            }
        }

        private void gridViewSpreadGrid_CellMouseMove_1(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex < 0 || e.RowIndex < 0)
            {

                return;

            }

            if (e.ColumnIndex == (int)OPTION_PL_COLUMNS.DEL_ROW)
            {
                gridViewSpreadGrid.Cursor = Cursors.Hand;
            }
            else
            {
                gridViewSpreadGrid.Cursor = Cursors.Default;

            }
        }

        private void btnRefreshData_Click(object sender, EventArgs e)
        {
            for (int row = 0; row < gridViewSpreadGrid.Rows.Count; row++)
            {
                if (gridViewSpreadGrid.Rows[row].Cells[(int)OPTION_PL_COLUMNS.CNTRT_TYPE].Value != null
                    && gridViewSpreadGrid.Rows[row].Cells[(int)OPTION_PL_COLUMNS.STRIKE].Value != null
                    && gridViewSpreadGrid.Rows[row].Cells[(int)OPTION_PL_COLUMNS.YEAR].Value != null
                    && gridViewSpreadGrid.Rows[row].Cells[(int)OPTION_PL_COLUMNS.MTH_AS_INT].Value != null)
                {

                    //        CNTRT_TYPE,

                    //STRIKE,

                    //AVG_PRC,

                    //NET,

                    //IMPL_VOL,

                    //DAYS_TO_EXP,

                    //YEAR,

                    //MTH_AS_INT,

                    //BRKR,

                    //ACCT,

                    //DEL_ROW,

                    //SEL_ROW

                    OPTION_SPREAD_CONTRACT_TYPE currentContractType = OPTION_SPREAD_CONTRACT_TYPE.FUTURE;

                    if (gridViewSpreadGrid.Rows[row].Cells[(int)OPTION_PL_COLUMNS.CNTRT_TYPE].Value
                    .ToString().CompareTo(optionArrayTypes.optionSpreadContractTypesArray.GetValue(
                            (int)OPTION_SPREAD_CONTRACT_TYPE.CALL)) == 0)
                    {
                        currentContractType = OPTION_SPREAD_CONTRACT_TYPE.CALL;
                    }
                    else if (gridViewSpreadGrid.Rows[row].Cells[(int)OPTION_PL_COLUMNS.CNTRT_TYPE].Value
                    .ToString().CompareTo(optionArrayTypes.optionSpreadContractTypesArray.GetValue(
                            (int)OPTION_SPREAD_CONTRACT_TYPE.PUT)) == 0)
                    {
                        currentContractType = OPTION_SPREAD_CONTRACT_TYPE.PUT;
                    }

                    double strike = Convert.ToDouble(gridViewSpreadGrid.Rows[row].Cells[(int)OPTION_PL_COLUMNS.STRIKE].Value);

                    int year = Convert.ToInt16(gridViewSpreadGrid.Rows[row].Cells[(int)OPTION_PL_COLUMNS.YEAR].Value);

                    int monthInt = Convert.ToInt16(gridViewSpreadGrid.Rows[row].Cells[(int)OPTION_PL_COLUMNS.MTH_AS_INT].Value);


                    int expressionCnt = 0;

                    while (expressionCnt < DataCollectionLibrary.optionSpreadExpressionList.Count)
                    {
                        if (DataCollectionLibrary.optionSpreadExpressionList[expressionCnt].instrument != null
                            && instrument.idinstrument == DataCollectionLibrary.optionSpreadExpressionList[expressionCnt].instrument.idinstrument)
                        {
                            if (currentContractType == DataCollectionLibrary.optionSpreadExpressionList[expressionCnt].callPutOrFuture)
                            {
                                if (currentContractType == OPTION_SPREAD_CONTRACT_TYPE.FUTURE)
                                {
                                    if (year == DataCollectionLibrary.optionSpreadExpressionList[expressionCnt].asset.year //.futureContractYear
                                        && monthInt == DataCollectionLibrary.optionSpreadExpressionList[expressionCnt].asset.monthint) //.futureContractMonthInt)
                                    {


                                        gridViewSpreadGrid.Rows[row].Cells[(int)OPTION_PL_COLUMNS.AVG_PRC].Value =
                                            ConversionAndFormatting.convertToTickMovesString(
                                           DataCollectionLibrary.optionSpreadExpressionList[expressionCnt].defaultPrice,
                                            instrument.ticksize,
                                            instrument.tickdisplay);

                                        break;
                                    }
                                }
                                else
                                {
                                    if (year == DataCollectionLibrary.optionSpreadExpressionList[expressionCnt].asset.optionyear //.optionYear
                                        && monthInt == DataCollectionLibrary.optionSpreadExpressionList[expressionCnt].asset.optionmonthint //.optionMonthInt
                                        && strike == DataCollectionLibrary.optionSpreadExpressionList[expressionCnt].asset.strikeprice)
                                    {
                                        double optionticksize = OptionSpreadManager.chooseoptionticksize(
                                            DataCollectionLibrary.optionSpreadExpressionList[expressionCnt].defaultPrice,
                                            instrument.optionticksize,
                                            instrument.secondaryoptionticksize,
                                            instrument.secondaryoptionticksizerule);

                                        double tickDisplay = OptionSpreadManager.chooseoptionticksize(
                                            DataCollectionLibrary.optionSpreadExpressionList[expressionCnt].defaultPrice,
                                            instrument.optiontickdisplay,
                                            instrument.secondaryoptiontickdisplay,
                                            instrument.secondaryoptionticksizerule);

                                        gridViewSpreadGrid.Rows[row].Cells[(int)OPTION_PL_COLUMNS.AVG_PRC].Value =
                                                ConversionAndFormatting.convertToTickMovesString(
                                                DataCollectionLibrary.optionSpreadExpressionList[expressionCnt].defaultPrice,
                                                optionticksize,
                                                tickDisplay);

                                        break;
                                    }
                                }
                            }
                        }

                        expressionCnt++;
                    }
                }



            }
        }


    }

}
