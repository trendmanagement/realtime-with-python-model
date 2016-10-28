using CQG;
using RealtimeSpreadMonitor.Forms;
using RealtimeSpreadMonitor.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RealtimeSpreadMonitor.FormManipulation
{
    internal class GridViewContractSummaryManipulation
    {
        internal GridViewContractSummaryManipulation(
            //OptionRealtimeMonitor optionRealtimeMonitor,
            //DataGridView gridViewContractSummary,
            OptionSpreadManager optionSpreadManager,
            StatusAndConnectedUpdates statusAndConnectedUpdates)
        {
            //this.optionRealtimeMonitor = optionRealtimeMonitor;
            //this.gridViewContractSummary = gridViewContractSummary;
            this.optionSpreadManager = optionSpreadManager;
            this.statusAndConnectedUpdates = statusAndConnectedUpdates;
        }

        //private OptionRealtimeMonitor optionRealtimeMonitor { get; set; }
        //private DataGridView gridViewContractSummary { get; set; }
        private OptionSpreadManager optionSpreadManager;// { get; set; }
        private StatusAndConnectedUpdates statusAndConnectedUpdates;// { get; set; }


        public void setupContractSummaryLiveData(OptionRealtimeMonitor optionRealtimeMonitor)
        {

            DataGridView gridViewContractSummary = optionRealtimeMonitor.getGridViewContractSummary;

            gridViewContractSummary.DataSource = DataCollectionLibrary.contractSummaryDataTable;

            try
            {
                Type contractSummaryColTypes = typeof(CONTRACTSUMMARY_DATA_COLUMNS);
                Array contractSummaryColTypesArray = Enum.GetNames(contractSummaryColTypes);





                //gridViewContractSummary.ColumnCount = contractSummaryColTypesArray.Length;

                //gridViewContractSummary.EnableHeadersVisualStyles = false;

                //DataGridViewCellStyle colTotalPortStyle = gridViewContractSummary.ColumnHeadersDefaultCellStyle;
                //colTotalPortStyle.BackColor = Color.Black;
                //colTotalPortStyle.ForeColor = Color.White;

                //DataGridViewCellStyle rowTotalPortStyle = gridViewContractSummary.RowHeadersDefaultCellStyle;
                //rowTotalPortStyle.BackColor = Color.Black;
                //rowTotalPortStyle.ForeColor = Color.White;


                //gridViewContractSummary.Columns[0].Frozen = true;

                //for (int i = 0; i < gridViewContractSummary.ColumnCount; i++)
                //{
                //    gridViewContractSummary.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                //}

                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < contractSummaryColTypesArray.Length; i++)
                {
                    sb.Clear();

                    sb.Append(contractSummaryColTypesArray.GetValue(i).ToString());

                    //gridViewContractSummary
                    //    .Columns[i]
                    //    .HeaderCell.Value = sb.ToString().Replace('_', ' ');

                    //gridViewContractSummary.Columns[i].Width = 50;

                    DataCollectionLibrary.contractSummaryDataTable.Columns.Add(sb.ToString().Replace('_', ' '));
                }

                //gridViewContractSummary.Columns[(int)CONTRACTSUMMARY_DATA_COLUMNS.CONTRACT].Width = 115;

                //gridViewContractSummary.Columns[(int)CONTRACTSUMMARY_DATA_COLUMNS.LEG].Width = 30;
                //gridViewContractSummary.Columns[(int)CONTRACTSUMMARY_DATA_COLUMNS.LEG].DefaultCellStyle.Font = new Font("Tahoma", 7);

                //gridViewContractSummary.Columns[(int)CONTRACTSUMMARY_DATA_COLUMNS.TIME].Width = 70;
                //gridViewContractSummary.Columns[(int)CONTRACTSUMMARY_DATA_COLUMNS.TIME].DefaultCellStyle.WrapMode = DataGridViewTriState.True;


                //gridViewContractSummary.Columns[(int)CONTRACTSUMMARY_DATA_COLUMNS.SETL_TIME].DefaultCellStyle.Font = new Font("Tahoma", 6);
                //gridViewContractSummary.Columns[(int)CONTRACTSUMMARY_DATA_COLUMNS.SETL_TIME].DefaultCellStyle.WrapMode = DataGridViewTriState.True;

                //gridViewContractSummary.Columns[(int)CONTRACTSUMMARY_DATA_COLUMNS.EXPR].DefaultCellStyle.Font = new Font("Tahoma", 6);
                //gridViewContractSummary.Columns[(int)CONTRACTSUMMARY_DATA_COLUMNS.EXPR].DefaultCellStyle.WrapMode = DataGridViewTriState.True;

                //int rowCount = 0;

                //for (int i = 0; i < DataCollectionLibrary.optionSpreadExpressionList.Count; i++)
                //{
                //    if (DataCollectionLibrary.optionSpreadExpressionList[i].optionExpressionType
                //        != OPTION_EXPRESSION_TYPES.OPTION_EXPRESSION_RISK_FREE_RATE
                //        && DataCollectionLibrary.optionSpreadExpressionList[i].numberOfLotsHeldForContractSummary != 0)
                //    {
                //        rowCount++;
                //    }
                //}

                //gridViewContractSummary.RowCount = rowCount;

                //Color rowColor1 = Color.DarkGray;
                //Color rowColor2 = Color.Black;

                //Color currentRowColor = rowColor1;

                //int heldLotsExpressionCnt = 0;



                //for (int instrumentCnt = 0; instrumentCnt <= DataCollectionLibrary.instrumentList.Count(); instrumentCnt++)
                //foreach(Instrument_mongo im in DataCollectionLibrary.instrumentList)
                //{


                //    for (int i = 0; i < DataCollectionLibrary.optionSpreadExpressionList.Count; i++)
                //    {
                //        if (DataCollectionLibrary.optionSpreadExpressionList[i].optionExpressionType
                //            != OPTION_EXPRESSION_TYPES.OPTION_EXPRESSION_RISK_FREE_RATE
                //            //&&
                //            //DataCollectionLibrary.optionSpreadExpressionList[i].instrument.idinstrument 
                //            //    == DataCollectionLibrary.instrumentList[instrumentCnt].idinstrument
                //                )
                //        {

                //            if (DataCollectionLibrary.optionSpreadExpressionList[i].numberOfLotsHeldForContractSummary != 0)
                //            {

                //                switch (heldLotsExpressionCnt % 2)
                //                {
                //                    case 0:
                //                        currentRowColor = rowColor1;
                //                        break;

                //                    case 1:
                //                        currentRowColor = rowColor2;
                //                        break;
                //                }

                //                optionSpreadManager.contractSummaryExpressionListIdx.Add(i);

                //                //TSErrorCatch.debugWriteOut(optionSpreadManager.optionSpreadExpressionList[i].cqgsymbol + " " + i);

                //                gridViewContractSummary
                //                    .Rows[heldLotsExpressionCnt]
                //                        .HeaderCell.Style.BackColor = currentRowColor;

                //                gridViewContractSummary
                //                    .Rows[heldLotsExpressionCnt]
                //                        .HeaderCell.Value =
                //                            DataCollectionLibrary.optionSpreadExpressionList[i].cqgsymbol;

                //                for (int j = 0; j < gridViewContractSummary.ColumnCount; j++)
                //                {
                //                    gridViewContractSummary.Rows[heldLotsExpressionCnt].Cells[j] = new CustomDataGridViewCell(true);
                //                }

                //                //gridViewContractSummary
                //                //    .Rows[heldLotsExpressionCnt].Cells[(int)CONTRACTSUMMARY_DATA_COLUMNS.TOTAL_QTY].Value =
                //                //    optionSpreadManager.optionSpreadExpressionList[i].numberOfLotsHeldForContractSummary;



                //                gridViewContractSummary
                //                        .Rows[heldLotsExpressionCnt].Cells[(int)CONTRACTSUMMARY_DATA_COLUMNS.SETL_TIME].Value =
                //                        DataCollectionLibrary.optionSpreadExpressionList[i].settlementDateTime;

                //                gridViewContractSummary
                //                        .Rows[heldLotsExpressionCnt].Cells[(int)CONTRACTSUMMARY_DATA_COLUMNS.STRIKE_PRICE].Value =
                //                        DataCollectionLibrary.optionSpreadExpressionList[i].strikePrice;

                //                gridViewContractSummary
                //                        .Rows[heldLotsExpressionCnt].Cells[(int)CONTRACTSUMMARY_DATA_COLUMNS.INSTRUMENT_ID].Value =
                //                        DataCollectionLibrary.optionSpreadExpressionList[i].instrument.idinstrument;

                //                heldLotsExpressionCnt++;
                //            }
                //        }
                //    }
                //}


            }
            catch (Exception ex)
            {
                TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
            }
        }

        public void FillContractSummary()
        {
            if (DataCollectionLibrary.performFullContractRefresh)
            {
                DataCollectionLibrary.performFullContractRefresh = false;
                _FillContractSummary(true);
            }
            else
            {
                _FillContractSummary(false);
            }
        }

        private void _FillContractSummary(bool fullRefresh)
        {


            DataTable dataTable = DataCollectionLibrary.contractSummaryDataTable;

            if (fullRefresh)
            {
                dataTable.Rows.Clear();
            }

            int rowCount = 0;

            foreach (AccountPosition ap in DataCollectionLibrary.accountPositionsList)
            {
                foreach (Position p in ap.positions)
                {

                    if (p.asset.idinstrument == DataCollectionLibrary.instrumentSelectedInTreeGui
                                || DataCollectionLibrary.instrumentSelectedInTreeGui == TradingSystemConstants.ALL_INSTRUMENTS_SELECTED)
                    {
                        AccountAllocation ac =
                            DataCollectionLibrary.portfolioAllocation.accountAllocation_KeyAccountname[ap.name];

                        if (ac.visible)
                        {

                            Instrument_mongo im = DataCollectionLibrary.instrumentHashTable_keyinstrumentid[p.asset.idinstrument];



                            if (fullRefresh)
                            {
                                dataTable.Rows.Add();

                                rowCount = dataTable.Rows.Count - 1;

                                dataTable.Rows[rowCount][(int)CONTRACTSUMMARY_DATA_COLUMNS.CONTRACT] = p.asset.cqgsymbol;

                                dataTable.Rows[rowCount][(int)CONTRACTSUMMARY_DATA_COLUMNS.QTY] = p.qty;

                                dataTable.Rows[rowCount][(int)CONTRACTSUMMARY_DATA_COLUMNS.PREV_QTY] = p.prev_qty;

                                dataTable.Rows[rowCount][(int)CONTRACTSUMMARY_DATA_COLUMNS.INSTRUMENT_ID] = im.idinstrument;

                                dataTable.Rows[rowCount][(int)CONTRACTSUMMARY_DATA_COLUMNS.ACCOUNT] = ap.name;

                                dataTable.Rows[rowCount][(int)CONTRACTSUMMARY_DATA_COLUMNS.FCM_OFFICE] = p;

                                dataTable.Rows[rowCount][(int)CONTRACTSUMMARY_DATA_COLUMNS.FCM_ACCT] = ap.name;

                                
                                dataTable.Rows[rowCount][(int)CONTRACTSUMMARY_DATA_COLUMNS.FCM_OFFICE] = ac.FCM_OFFICE;

                                dataTable.Rows[rowCount][(int)CONTRACTSUMMARY_DATA_COLUMNS.FCM_ACCT] = ac.FCM_ACCT;
                            }

                            dataTable.Rows[dataTable.Rows.Count - 1][(int)CONTRACTSUMMARY_DATA_COLUMNS.TIME]
                                = p.mose.lastTimeUpdated.ToString("MM-dd HH:mm", DateTimeFormatInfo.InvariantInfo);

                            dataTable.Rows[dataTable.Rows.Count - 1][(int)CONTRACTSUMMARY_DATA_COLUMNS.PL_DAY_CHG]
                                = (p.mose.defaultPrice - p.mose.yesterdaySettlement) / im.ticksize * im.tickvalue * p.prev_qty;

                            dataTable.Rows[dataTable.Rows.Count - 1][(int)CONTRACTSUMMARY_DATA_COLUMNS.ORDER_PL_CHG]
                                = (p.mose.defaultPrice - p.mose.transactionPrice) / im.ticksize * im.tickvalue * p.qty;

                            dataTable.Rows[dataTable.Rows.Count - 1][(int)CONTRACTSUMMARY_DATA_COLUMNS.DELTA]
                                = p.mose.delta * p.qty;



                            rowCount++;
                        }
                    }


                }
            }
        }

        public void sendUpdateToContractSummaryLiveDataxxx(OptionRealtimeMonitor optionRealtimeMonitor)
        {

            //DataGridView gridViewContractSummary = optionRealtimeMonitor.getGridViewContractSummary;

            //try
            //{

            //    for (int contractSummaryExpressionCnt = 0;
            //        contractSummaryExpressionCnt < optionSpreadManager.contractSummaryExpressionListIdx.Count(); contractSummaryExpressionCnt++)
            //    {
            //        CQGInstrument cqgInstrument =
            //            DataCollectionLibrary.optionSpreadExpressionList[optionSpreadManager.contractSummaryExpressionListIdx[contractSummaryExpressionCnt]]
            //                .cqgInstrument;

            //        if (cqgInstrument != null)
            //        {

            //            statusAndConnectedUpdates.checkUpdateStatus(gridViewContractSummary, contractSummaryExpressionCnt,
            //                        (int)CONTRACTSUMMARY_DATA_COLUMNS.TIME,
            //                        DataCollectionLibrary.optionSpreadExpressionList[optionSpreadManager.contractSummaryExpressionListIdx[contractSummaryExpressionCnt]]);


            //            //if (optionSpreadManager.realtimeMonitorSettings.eodAnalysis)
            //            if (DataCollectionLibrary.optionSpreadExpressionList
            //                    [optionSpreadManager.contractSummaryExpressionListIdx[contractSummaryExpressionCnt]]
            //                        .instrument.eodAnalysisAtInstrument)
            //            {
            //                //gridViewContractSummary.Columns[(int)CONTRACTSUMMARY_DATA_COLUMNS.TIME].DefaultCellStyle.Font = new Font("Tahoma", 6);

            //                fillContractSummaryLiveData(optionRealtimeMonitor,
            //                    contractSummaryExpressionCnt,
            //                                    (int)CONTRACTSUMMARY_DATA_COLUMNS.TIME,
            //                                    DataCollectionLibrary.optionSpreadExpressionList[optionSpreadManager.contractSummaryExpressionListIdx[contractSummaryExpressionCnt]]
            //                                        .lastTimeUpdated.ToString("yyyy-MM-dd HH:mm", DateTimeFormatInfo.InvariantInfo),
            //                                    false, 0);
            //            }
            //            else
            //            {
            //                //gridViewContractSummary.Columns[(int)CONTRACTSUMMARY_DATA_COLUMNS.TIME].DefaultCellStyle.Font = new Font("Tahoma", 8);

            //                fillContractSummaryLiveData(optionRealtimeMonitor, contractSummaryExpressionCnt,
            //                                    (int)CONTRACTSUMMARY_DATA_COLUMNS.TIME,
            //                                    DataCollectionLibrary.optionSpreadExpressionList[optionSpreadManager.contractSummaryExpressionListIdx[contractSummaryExpressionCnt]]
            //                                        .lastTimeUpdated.ToString("HH:mm", DateTimeFormatInfo.InvariantInfo),
            //                                    false, 0);
            //            }


            //            //                gridViewContractSummary
            //            //                            .Rows[heldLotsExpressionCnt].Cells[(int)CONTRACTSUMMARY_DATA_COLUMNS.TOTAL_QTY].Value =
            //            //                            optionSpreadExpressionList[i].numberOfLotsHeldForContractSummary
            //            //                            * optionSpreadManager.portfolioGroupTotalMultiple;






            //            fillContractSummaryLiveData(optionRealtimeMonitor, contractSummaryExpressionCnt,
            //                        (int)CONTRACTSUMMARY_DATA_COLUMNS.ASK,
            //                        cqgInstrument.ToDisplayPrice(
            //                            DataCollectionLibrary.optionSpreadExpressionList[optionSpreadManager.contractSummaryExpressionListIdx[contractSummaryExpressionCnt]].ask),
            //                            false,
            //                            DataCollectionLibrary.optionSpreadExpressionList[optionSpreadManager.contractSummaryExpressionListIdx[contractSummaryExpressionCnt]].ask);

            //            fillContractSummaryLiveData(optionRealtimeMonitor, contractSummaryExpressionCnt,
            //                    (int)CONTRACTSUMMARY_DATA_COLUMNS.BID,
            //                    cqgInstrument.ToDisplayPrice(
            //                            DataCollectionLibrary.optionSpreadExpressionList[optionSpreadManager.contractSummaryExpressionListIdx[contractSummaryExpressionCnt]].bid),
            //                            false,
            //                            DataCollectionLibrary.optionSpreadExpressionList[optionSpreadManager.contractSummaryExpressionListIdx[contractSummaryExpressionCnt]].bid);

            //            fillContractSummaryLiveData(optionRealtimeMonitor, contractSummaryExpressionCnt,
            //                    (int)CONTRACTSUMMARY_DATA_COLUMNS.LAST,
            //                    cqgInstrument.ToDisplayPrice(
            //                            DataCollectionLibrary.optionSpreadExpressionList[optionSpreadManager.contractSummaryExpressionListIdx[contractSummaryExpressionCnt]].trade),
            //                            false,
            //                            DataCollectionLibrary.optionSpreadExpressionList[optionSpreadManager.contractSummaryExpressionListIdx[contractSummaryExpressionCnt]].trade);

            //            fillContractSummaryLiveData(optionRealtimeMonitor, contractSummaryExpressionCnt,
            //                    (int)CONTRACTSUMMARY_DATA_COLUMNS.DFLT_PRICE,
            //                    cqgInstrument.ToDisplayPrice(
            //                            DataCollectionLibrary.optionSpreadExpressionList[optionSpreadManager.contractSummaryExpressionListIdx[contractSummaryExpressionCnt]].defaultPrice),
            //                            false,
            //                            DataCollectionLibrary.optionSpreadExpressionList[optionSpreadManager.contractSummaryExpressionListIdx[contractSummaryExpressionCnt]].defaultPrice);

            //            fillContractSummaryLiveData(optionRealtimeMonitor, contractSummaryExpressionCnt,
            //                    (int)CONTRACTSUMMARY_DATA_COLUMNS.STTLE,
            //                    cqgInstrument.ToDisplayPrice(
            //                            DataCollectionLibrary.optionSpreadExpressionList[optionSpreadManager.contractSummaryExpressionListIdx[contractSummaryExpressionCnt]].settlement),
            //                            false,
            //                            DataCollectionLibrary.optionSpreadExpressionList[optionSpreadManager.contractSummaryExpressionListIdx[contractSummaryExpressionCnt]].settlement);

            //            if (DataCollectionLibrary.optionSpreadExpressionList[optionSpreadManager.contractSummaryExpressionListIdx[contractSummaryExpressionCnt]]
            //                    .settlementDateTime.Date.CompareTo(DateTime.Now.Date) >= 0)
            //            {
            //                fillContractSummaryLiveData(optionRealtimeMonitor, contractSummaryExpressionCnt,
            //                    (int)CONTRACTSUMMARY_DATA_COLUMNS.SETL_TIME,
            //                            DataCollectionLibrary.optionSpreadExpressionList[optionSpreadManager.contractSummaryExpressionListIdx[contractSummaryExpressionCnt]].settlementDateTime
            //                            .ToString("yyyy-MM-dd HH:mm", DateTimeFormatInfo.InvariantInfo),
            //                            true, 1);
            //            }
            //            else
            //            {
            //                fillContractSummaryLiveData(optionRealtimeMonitor, contractSummaryExpressionCnt,
            //                    (int)CONTRACTSUMMARY_DATA_COLUMNS.SETL_TIME,
            //                            DataCollectionLibrary.optionSpreadExpressionList[optionSpreadManager.contractSummaryExpressionListIdx[contractSummaryExpressionCnt]].settlementDateTime
            //                            .ToString("yyyy-MM-dd HH:mm", DateTimeFormatInfo.InvariantInfo),
            //                            true, -1);
            //            }

            //            fillContractSummaryLiveData(optionRealtimeMonitor, contractSummaryExpressionCnt,
            //                    (int)CONTRACTSUMMARY_DATA_COLUMNS.YEST_STTLE,
            //                    cqgInstrument.ToDisplayPrice(
            //                            DataCollectionLibrary.optionSpreadExpressionList[optionSpreadManager.contractSummaryExpressionListIdx[contractSummaryExpressionCnt]].yesterdaySettlement),
            //                            false,
            //                            DataCollectionLibrary.optionSpreadExpressionList[optionSpreadManager.contractSummaryExpressionListIdx[contractSummaryExpressionCnt]].yesterdaySettlement);



            //            fillContractSummaryLiveData(optionRealtimeMonitor, contractSummaryExpressionCnt,
            //                    (int)CONTRACTSUMMARY_DATA_COLUMNS.IMPL_VOL,
            //                    cqgInstrument.ToDisplayPrice(
            //                            DataCollectionLibrary.optionSpreadExpressionList[optionSpreadManager.contractSummaryExpressionListIdx[contractSummaryExpressionCnt]].impliedVol),
            //                            false,
            //                            DataCollectionLibrary.optionSpreadExpressionList[optionSpreadManager.contractSummaryExpressionListIdx[contractSummaryExpressionCnt]].impliedVol);

            //            fillContractSummaryLiveData(optionRealtimeMonitor, contractSummaryExpressionCnt,
            //                    (int)CONTRACTSUMMARY_DATA_COLUMNS.THEOR_PRICE,
            //                    cqgInstrument.ToDisplayPrice(
            //                            DataCollectionLibrary.optionSpreadExpressionList[optionSpreadManager.contractSummaryExpressionListIdx[contractSummaryExpressionCnt]].theoreticalOptionPrice),
            //                            false,
            //                            DataCollectionLibrary.optionSpreadExpressionList[optionSpreadManager.contractSummaryExpressionListIdx[contractSummaryExpressionCnt]].theoreticalOptionPrice);

            //            fillContractSummaryLiveData(optionRealtimeMonitor, contractSummaryExpressionCnt,
            //                    (int)CONTRACTSUMMARY_DATA_COLUMNS.SPAN_IMPL_VOL,
            //                    Math.Round(
            //                            DataCollectionLibrary.optionSpreadExpressionList[optionSpreadManager.contractSummaryExpressionListIdx[contractSummaryExpressionCnt]].impliedVolFromSpan,
            //                            2).ToString(),
            //                            false,
            //                            DataCollectionLibrary.optionSpreadExpressionList[optionSpreadManager.contractSummaryExpressionListIdx[contractSummaryExpressionCnt]].impliedVolFromSpan);

            //            fillContractSummaryLiveData(optionRealtimeMonitor, contractSummaryExpressionCnt,
            //                    (int)CONTRACTSUMMARY_DATA_COLUMNS.SETL_IMPL_VOL,
            //                    Math.Round(
            //                            DataCollectionLibrary.optionSpreadExpressionList[optionSpreadManager.contractSummaryExpressionListIdx[contractSummaryExpressionCnt]].settlementImpliedVol,
            //                            2).ToString(),
            //                            false,
            //                            DataCollectionLibrary.optionSpreadExpressionList[optionSpreadManager.contractSummaryExpressionListIdx[contractSummaryExpressionCnt]].settlementImpliedVol);


            //            int lots = DataCollectionLibrary.optionSpreadExpressionList[optionSpreadManager.contractSummaryExpressionListIdx[contractSummaryExpressionCnt]]
            //                        .numberOfLotsHeldForContractSummary;

            //            fillContractSummaryLiveData(optionRealtimeMonitor, contractSummaryExpressionCnt,
            //                        (int)CONTRACTSUMMARY_DATA_COLUMNS.QTY,
            //                        lots.ToString(),
            //                            false,
            //                            lots);


            //            double delta = Math.Round(
            //                DataCollectionLibrary.optionSpreadExpressionList[optionSpreadManager.contractSummaryExpressionListIdx[contractSummaryExpressionCnt]].deltaChgForContractSummary,
            //                2);

            //            fillContractSummaryLiveData(optionRealtimeMonitor, contractSummaryExpressionCnt,
            //                    (int)CONTRACTSUMMARY_DATA_COLUMNS.DELTA,
            //                    delta.ToString(),
            //                            true,
            //                            delta);


            //            double plChg = Math.Round(
            //                DataCollectionLibrary.optionSpreadExpressionList[optionSpreadManager.contractSummaryExpressionListIdx[contractSummaryExpressionCnt]].plChgForContractSummary
            //                        , 2);

            //            fillContractSummaryLiveData(optionRealtimeMonitor, contractSummaryExpressionCnt,
            //                    (int)CONTRACTSUMMARY_DATA_COLUMNS.PL_DAY_CHG,
            //                    plChg.ToString(),
            //                            true,
            //                            plChg);


            //            int numberOfOrderContracts =
            //                DataCollectionLibrary.optionSpreadExpressionList[optionSpreadManager.contractSummaryExpressionListIdx[contractSummaryExpressionCnt]].numberOfOrderContracts
            //                        ;

            //            fillContractSummaryLiveData(optionRealtimeMonitor, contractSummaryExpressionCnt,
            //                    (int)CONTRACTSUMMARY_DATA_COLUMNS.PREV_QTY,
            //                    numberOfOrderContracts.ToString(),
            //                            false,
            //                           numberOfOrderContracts);


            //            double plChgOrders = Math.Round(
            //                DataCollectionLibrary.optionSpreadExpressionList[optionSpreadManager.contractSummaryExpressionListIdx[contractSummaryExpressionCnt]].plChgOrders
            //                        , 2);

            //            fillContractSummaryLiveData(optionRealtimeMonitor, contractSummaryExpressionCnt,
            //                    (int)CONTRACTSUMMARY_DATA_COLUMNS.ORDER_PL_CHG,
            //                   plChgOrders.ToString(),
            //                            true,
            //                            plChgOrders);

            //        }

            //    }

            //}
            //catch (Exception ex)
            //{
            //    TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
            //}
        }

        delegate void ThreadSafeFillLiveDataPageDelegate(
            OptionRealtimeMonitor optionRealtimeMonitor,
            int row, int col, String displayValue,
            bool updateColor, double value);

        public void fillContractSummaryLiveData(OptionRealtimeMonitor optionRealtimeMonitor,
            int row, int col, String displayValue,
            bool updateColor, double value)
        {
            try
            {
                if (optionRealtimeMonitor.InvokeRequired)
                {
                    ThreadSafeFillLiveDataPageDelegate d = new ThreadSafeFillLiveDataPageDelegate(threadSafeFillContractSummaryLiveData);

                    optionRealtimeMonitor.Invoke(d, optionRealtimeMonitor, row, col, displayValue, updateColor, value);
                }
                else
                {
                    threadSafeFillContractSummaryLiveData(optionRealtimeMonitor, row, col, displayValue, updateColor, value);
                }
            }
            catch (Exception ex)
            {
                TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
            }
        }

        public void threadSafeFillContractSummaryLiveData(
            OptionRealtimeMonitor optionRealtimeMonitor,
            int row, int col, String displayValue,
            bool updateColor, double value)
        {
            try
            {
                int rowToUpdate = row;

                if (optionRealtimeMonitor.getGridViewContractSummary.Rows[rowToUpdate].Cells[col].Value == null
                    ||
                    optionRealtimeMonitor.getGridViewContractSummary.Rows[rowToUpdate].Cells[col].Value.ToString().CompareTo(displayValue) != 0
                    )
                {
                    optionRealtimeMonitor.getGridViewContractSummary.Rows[rowToUpdate].Cells[col].Value = displayValue;

                    if (updateColor)
                    {
                        optionRealtimeMonitor.getGridViewContractSummary.Rows[rowToUpdate].Cells[col].Style.BackColor
                            = CommonFormManipulation.plUpDownColor(value);
                    }
                }

            }
            catch (Exception ex)
            {
                TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
            }
        }
    }
}
