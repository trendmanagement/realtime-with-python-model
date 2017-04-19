using CQG;
using RealtimeSpreadMonitor.Forms;
using RealtimeSpreadMonitor.Model;
using RealtimeSpreadMonitor.Mongo;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;

namespace RealtimeSpreadMonitor.FormManipulation
{
    internal class GridViewFCMPostionManipulation
    {

        internal GridViewFCMPostionManipulation()
        //StatusAndConnectedUpdates statusAndConnectedUpdates)
        {
            //this.optionSpreadManager = optionSpreadManager;
            //this.statusAndConnectedUpdates = statusAndConnectedUpdates;
        }

        internal void SetupFCMSummaryData(OptionRealtimeMonitor optionRealtimeMonitor)
        {

            DataGridView gridLiveFCMData = optionRealtimeMonitor.getGridLiveFCMData;

            gridLiveFCMData.DataSource = DataCollectionLibrary.FCM_SummaryDataTable;

            try
            {

                Type liveColTypes = typeof(OPTION_LIVE_ADM_DATA_COLUMNS);
                Array liveColTypesArray = Enum.GetNames(liveColTypes);

                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < liveColTypesArray.Length; i++)
                {
                    sb.Clear();

                    sb.Append(liveColTypesArray.GetValue(i).ToString());

                    DataCollectionLibrary.FCM_SummaryDataTable.Columns.Add(sb.ToString().Replace('_', ' '));
                }

                //fillGridLiveADMData(optionRealtimeMonitor);
                //Fill_FCM_ContractSummary();
            }
            catch (Exception ex)
            {
                TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
            }
        }

        public void Fill_FCM_ContractSummary()
        {
            if (DataCollectionLibrary.performFull_FCMSummary_Refresh)
            {
                DataCollectionLibrary.performFull_FCMSummary_Refresh = false;
                _FillFCMSummary(true);
            }
            else
            {
                _FillFCMSummary(false);
            }
        }

        internal void _FillFCMSummary(bool fullRefresh)
        {
            try
            {
                DataTable dataTable = DataCollectionLibrary.FCM_SummaryDataTable;

                if (fullRefresh)
                {
                    dataTable.Rows.Clear();
                }

                int rowIdx = 0;


                List<ADMPositionImportWeb> admPositionImportWeb = FCM_DataImportLibrary.FCM_Import_Consolidated;



                {

                    foreach (ADMPositionImportWeb admpiw in admPositionImportWeb)
                    {
                        //if (admPositionImportWeb[admWebPositionCounter].instrument.idinstrument 
                        //        == DataCollectionLibrary.instrumentList[instrumentCnt].idinstrument)
                        if ((admpiw.instrument.idinstrument == DataCollectionLibrary.instrumentSelectedInTreeGui
                                || DataCollectionLibrary.instrumentSelectedInTreeGui == TradingSystemConstants.ALL_INSTRUMENTS_SELECTED)
                                &&
                                admpiw.acctGroup.visible)
                        {
                            if (fullRefresh)
                            {
                                dataTable.Rows.Add();

                                rowIdx = dataTable.Rows.Count - 1;


                                admpiw.liveADMRowIdx = rowIdx;


                                dataTable.Rows[rowIdx][(int)OPTION_LIVE_ADM_DATA_COLUMNS.CONTRACT] =
                                            admpiw.asset.cqgsymbol;


                                DateTime currentDate = DateTime.Now;


                                dataTable.Rows[rowIdx][(int)OPTION_LIVE_ADM_DATA_COLUMNS.POFFIC] =
                                    admpiw.POFFIC;

                                dataTable.Rows[rowIdx][(int)OPTION_LIVE_ADM_DATA_COLUMNS.PACCT] =
                                    admpiw.PACCT;


                                dataTable.Rows[rowIdx][(int)OPTION_LIVE_ADM_DATA_COLUMNS.NET_EDITABLE] =
                                    admpiw.netContractsEditable;

                                dataTable.Rows[rowIdx][(int)OPTION_LIVE_ADM_DATA_COLUMNS.NET_AT_ADM] =
                                    admpiw.Net;


                                dataTable.Rows[rowIdx][(int)OPTION_LIVE_ADM_DATA_COLUMNS.LONG_TRANS] =
                                    admpiw.transNetLong;

                                dataTable.Rows[rowIdx][(int)OPTION_LIVE_ADM_DATA_COLUMNS.SHORT_TRANS] =
                                    admpiw.transNetShort;


                                TimeSpan span = admpiw.asset.expirationdate.Date - currentDate.Date;

                                dataTable.Rows[rowIdx][(int)OPTION_LIVE_ADM_DATA_COLUMNS.CNTDN] =
                                                    span.Days;

                                dataTable.Rows[rowIdx][(int)OPTION_LIVE_ADM_DATA_COLUMNS.EXPR] =
                                    new DateTime(
                                                        admpiw.asset.expirationdate.Year,
                                                        admpiw.asset.expirationdate.Month,
                                                        admpiw.asset.expirationdate.Day,
                                                        0,//admpiw.asset.optionExpirationTime.Hour,
                                                        0,//admpiw.asset.optionExpirationTime.Minute,
                                                        0
                                                    )
                                                    .ToString("yyyy-MM-dd HH:mm", DateTimeFormatInfo.InvariantInfo);

                                dataTable.Rows[rowIdx][(int)OPTION_LIVE_ADM_DATA_COLUMNS.AVG_LONG_TRANS_PRC] =
                                    admpiw.transAvgLongPrice;

                                dataTable.Rows[rowIdx][(int)OPTION_LIVE_ADM_DATA_COLUMNS.AVG_SHORT_TRANS_PRC] =
                                    admpiw.transAvgShortPrice;

                                dataTable.Rows[rowIdx][(int)OPTION_LIVE_ADM_DATA_COLUMNS.STRIKE] =
                                    admpiw.strike;

                                dataTable.Rows[rowIdx][(int)OPTION_LIVE_ADM_DATA_COLUMNS.DESCRIP] =
                                    admpiw.Description;

                                dataTable.Rows[rowIdx][(int)OPTION_LIVE_ADM_DATA_COLUMNS.CUSIP] =
                                    admpiw.PCUSIP;

                                //dataTable.Rows[rowIdx][(int)OPTION_LIVE_ADM_DATA_COLUMNS.ADMPOSWEB_IDX] =
                                //    admWebPositionCounter;

                                dataTable.Rows[rowIdx][(int)OPTION_LIVE_ADM_DATA_COLUMNS.INSTRUMENT_ID] =
                                    admpiw.instrument.idinstrument;

                            }

                            if (dataTable != null && dataTable.Rows.Count > 0 && admpiw.optionSpreadExpression != null)
                            {
                                UpdateCell(rowIdx, (int)OPTION_LIVE_ADM_DATA_COLUMNS.TIME,
                                    dataTable, admpiw.optionSpreadExpression.lastTimeUpdated.ToString("yyyy-MM-dd HH:mm", DateTimeFormatInfo.InvariantInfo));
                                //dataTable.Rows[rowIdx][(int)OPTION_LIVE_ADM_DATA_COLUMNS.TIME] =
                                //        admpiw.optionSpreadExpression.lastTimeUpdated.ToString("yyyy-MM-dd HH:mm", DateTimeFormatInfo.InvariantInfo);

                                //if (admpiw.optionSpreadExpression.cqgInstrument != null)

                                UpdateCell(rowIdx, (int)OPTION_LIVE_ADM_DATA_COLUMNS.ASK,
                                    dataTable, admpiw.optionSpreadExpression.ask.ToString());
                                //dataTable.Rows[rowIdx][(int)OPTION_LIVE_ADM_DATA_COLUMNS.ASK] =
                                //    (admpiw.optionSpreadExpression.ask);

                                UpdateCell(rowIdx, (int)OPTION_LIVE_ADM_DATA_COLUMNS.BID,
                                    dataTable, admpiw.optionSpreadExpression.bid.ToString());
                                //dataTable.Rows[rowIdx][(int)OPTION_LIVE_ADM_DATA_COLUMNS.BID] =
                                //    (admpiw.optionSpreadExpression.bid);

                                UpdateCell(rowIdx, (int)OPTION_LIVE_ADM_DATA_COLUMNS.LAST,
                                    dataTable, admpiw.optionSpreadExpression.trade.ToString());
                                //dataTable.Rows[rowIdx][(int)OPTION_LIVE_ADM_DATA_COLUMNS.LAST] =
                                //    (admpiw.optionSpreadExpression.trade);

                                UpdateCell(rowIdx, (int)OPTION_LIVE_ADM_DATA_COLUMNS.DFLT_PRICE,
                                    dataTable, admpiw.optionSpreadExpression.defaultPrice.ToString());
                                //dataTable.Rows[rowIdx][(int)OPTION_LIVE_ADM_DATA_COLUMNS.DFLT_PRICE] =
                                //    (admpiw.optionSpreadExpression.defaultPrice);

                                UpdateCell(rowIdx, (int)OPTION_LIVE_ADM_DATA_COLUMNS.STTLE,
                                    dataTable, admpiw.optionSpreadExpression.settlement.ToString());
                                //dataTable.Rows[rowIdx][(int)OPTION_LIVE_ADM_DATA_COLUMNS.STTLE] =
                                //    (admpiw.optionSpreadExpression.settlement);

                                UpdateCell(rowIdx, (int)OPTION_LIVE_ADM_DATA_COLUMNS.THEOR_PRICE,
                                    dataTable, admpiw.optionSpreadExpression.theoreticalOptionPrice.ToString());
                                //dataTable.Rows[rowIdx][(int)OPTION_LIVE_ADM_DATA_COLUMNS.THEOR_PRICE] =
                                //    (admpiw.optionSpreadExpression.theoreticalOptionPrice);

                                UpdateCell(rowIdx, (int)OPTION_LIVE_ADM_DATA_COLUMNS.SETL_TIME,
                                    dataTable, admpiw.optionSpreadExpression.settlementDateTime.ToString("yyyy-MM-dd HH:mm", DateTimeFormatInfo.InvariantInfo));
                                //dataTable.Rows[rowIdx][(int)OPTION_LIVE_ADM_DATA_COLUMNS.SETL_TIME] =
                                //        admpiw.optionSpreadExpression.settlementDateTime.ToString("yyyy-MM-dd HH:mm", DateTimeFormatInfo.InvariantInfo);

                                UpdateCell(rowIdx, (int)OPTION_LIVE_ADM_DATA_COLUMNS.YEST_STTLE,
                                    dataTable, admpiw.optionSpreadExpression.yesterdaySettlementDateTime.ToString("yyyy-MM-dd HH:mm", DateTimeFormatInfo.InvariantInfo));
                                //dataTable.Rows[rowIdx][(int)OPTION_LIVE_ADM_DATA_COLUMNS.YEST_STTLE] =
                                //        admpiw.optionSpreadExpression.yesterdaySettlementDateTime.ToString("yyyy-MM-dd HH:mm", DateTimeFormatInfo.InvariantInfo);

                                UpdateCell(rowIdx, (int)OPTION_LIVE_ADM_DATA_COLUMNS.IMPL_VOL,
                                    dataTable, Math.Round(admpiw.optionSpreadExpression.impliedVol, 2).ToString());
                                //dataTable.Rows[rowIdx][(int)OPTION_LIVE_ADM_DATA_COLUMNS.IMPL_VOL] =
                                //        Math.Round(admpiw.optionSpreadExpression.impliedVol, 2);

                                UpdateCell(rowIdx, (int)OPTION_LIVE_ADM_DATA_COLUMNS.SPAN_IMPL_VOL,
                                    dataTable, Math.Round(admpiw.optionSpreadExpression.impliedVolFromSpan, 2).ToString());
                                //dataTable.Rows[rowIdx][(int)OPTION_LIVE_ADM_DATA_COLUMNS.SPAN_IMPL_VOL] =
                                //        Math.Round(admpiw.optionSpreadExpression.impliedVolFromSpan, 2);

                                UpdateCell(rowIdx, (int)OPTION_LIVE_ADM_DATA_COLUMNS.SETL_IMPL_VOL,
                                    dataTable, Math.Round(admpiw.optionSpreadExpression.settlementImpliedVol, 2).ToString());
                                //dataTable.Rows[rowIdx][(int)OPTION_LIVE_ADM_DATA_COLUMNS.SETL_IMPL_VOL] =
                                //        Math.Round(admpiw.optionSpreadExpression.settlementImpliedVol, 2);

                                UpdateCell(rowIdx, (int)OPTION_LIVE_ADM_DATA_COLUMNS.RFR,
                                    dataTable, admpiw.optionSpreadExpression.riskFreeRate.ToString());
                                //dataTable.Rows[rowIdx][(int)OPTION_LIVE_ADM_DATA_COLUMNS.RFR] =
                                //        admpiw.optionSpreadExpression.riskFreeRate;

                                UpdateCell(rowIdx, (int)OPTION_LIVE_ADM_DATA_COLUMNS.PL_DAY_CHG,
                                    dataTable, Math.Round(admpiw.positionTotals.pAndLDay, 2).ToString());
                                //dataTable.Rows[rowIdx][(int)OPTION_LIVE_ADM_DATA_COLUMNS.PL_DAY_CHG] =
                                //        Math.Round(admpiw.positionTotals.pAndLDay, 2);

                                UpdateCell(rowIdx, (int)OPTION_LIVE_ADM_DATA_COLUMNS.PL_TRANS,
                                    dataTable, Math.Round(admpiw.positionTotals.pAndLDayOrders, 2).ToString());
                                //dataTable.Rows[rowIdx][(int)OPTION_LIVE_ADM_DATA_COLUMNS.PL_TRANS] =
                                //        Math.Round(admpiw.positionTotals.pAndLDayOrders, 2);

                                UpdateCell(rowIdx, (int)OPTION_LIVE_ADM_DATA_COLUMNS.DELTA,
                                    dataTable, Math.Round(admpiw.positionTotals.delta, 2).ToString());
                                //dataTable.Rows[rowIdx][(int)OPTION_LIVE_ADM_DATA_COLUMNS.DELTA] =
                                //        Math.Round(admpiw.positionTotals.delta, 2);


                            }

                            //*********************************


                            rowIdx++;

                            //rowIdx += liveADMStrategyInfoList[stratCounter].admLegInfo.Count + 1;



                        }
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

        private void UpdateCell(int row, int col, DataTable dataTable, string displayValue)
        {
            try
            {
                if (dataTable.Rows[row][col] == null
                    || dataTable.Rows[row][col].ToString().CompareTo(displayValue) != 0)
                {
                    dataTable.Rows[row][col] = displayValue;
                }
            }
            catch (Exception ex)
            {
                TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
            }
        }

        //internal void setupGridLiveADMDataXXX(OptionRealtimeMonitor optionRealtimeMonitor)
        //{

        //    DataGridView gridLiveFCMData = optionRealtimeMonitor.getGridLiveFCMData;

        //    try
        //    {

        //        Type liveColTypes = typeof(OPTION_LIVE_ADM_DATA_COLUMNS);
        //        Array liveColTypesArray = Enum.GetNames(liveColTypes);

        //        gridLiveFCMData.ColumnCount = liveColTypesArray.Length;

        //        gridLiveFCMData.EnableHeadersVisualStyles = false;

        //        DataGridViewCellStyle colTotalPortStyle = gridLiveFCMData.ColumnHeadersDefaultCellStyle;
        //        colTotalPortStyle.BackColor = Color.Black;
        //        colTotalPortStyle.ForeColor = Color.White;

        //        DataGridViewCellStyle rowTotalPortStyle = gridLiveFCMData.RowHeadersDefaultCellStyle;
        //        rowTotalPortStyle.BackColor = Color.Black;
        //        rowTotalPortStyle.ForeColor = Color.White;

        //        gridLiveFCMData.Columns[0].Frozen = true;

        //        StringBuilder sb = new StringBuilder();

        //        for (int i = 0; i < gridLiveFCMData.ColumnCount; i++)
        //        {
        //            gridLiveFCMData.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;

        //            if (i != (int)OPTION_LIVE_ADM_DATA_COLUMNS.NET_EDITABLE)
        //            {
        //                gridLiveFCMData.Columns[i].ReadOnly = true;
        //            }

        //            sb.Clear();

        //            sb.Append(liveColTypesArray.GetValue(i).ToString());

        //            gridLiveFCMData
        //                .Columns[i]
        //                .HeaderCell.Value = sb.ToString().Replace('_', ' ');

        //            gridLiveFCMData.Columns[i].Width = 50;
        //        }



        //        //for (int i = 0; i < liveColTypesArray.Length; i++)
        //        //{

        //        //}

        //        //************
        //        //gridLiveADMData.Columns[(int)OPTION_LIVE_ADM_DATA_COLUMNS.CONTRACT].Width = 115;

        //        //gridLiveADMData.Columns[(int)OPTION_LIVE_ADM_DATA_COLUMNS.LEG].Width = 30;
        //        //gridLiveADMData.Columns[(int)OPTION_LIVE_ADM_DATA_COLUMNS.LEG].DefaultCellStyle.Font = new Font("Tahoma", 7);

        //        gridLiveFCMData.Columns[(int)OPTION_LIVE_ADM_DATA_COLUMNS.TIME].Width = 70;
        //        gridLiveFCMData.Columns[(int)OPTION_LIVE_ADM_DATA_COLUMNS.TIME].DefaultCellStyle.WrapMode = DataGridViewTriState.True;


        //        gridLiveFCMData.Columns[(int)OPTION_LIVE_ADM_DATA_COLUMNS.SETL_TIME].DefaultCellStyle.Font = new Font("Tahoma", 6);
        //        gridLiveFCMData.Columns[(int)OPTION_LIVE_ADM_DATA_COLUMNS.SETL_TIME].DefaultCellStyle.WrapMode = DataGridViewTriState.True;

        //        gridLiveFCMData.Columns[(int)OPTION_LIVE_ADM_DATA_COLUMNS.EXPR].DefaultCellStyle.Font = new Font("Tahoma", 6);
        //        gridLiveFCMData.Columns[(int)OPTION_LIVE_ADM_DATA_COLUMNS.EXPR].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
        //        //************

        //        //List<LiveADMStrategyInfo> liveADMStrategyInfoList = optionSpreadManager.liveADMStrategyInfoList;

        //        fillGridLiveADMData(optionRealtimeMonitor);

        //    }
        //    catch (Exception ex)
        //    {
        //        TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
        //    }
        //}

        //delegate void ThreadSafeFillGridModelADMComparison(OptionRealtimeMonitor optionRealtimeMonitor);

        //internal void fillGridLiveADMData(OptionRealtimeMonitor optionRealtimeMonitor)
        //{
        //    DataGridView gridLiveFCMData = optionRealtimeMonitor.getGridLiveFCMData;

        //    if (optionRealtimeMonitor.InvokeRequired)
        //    {

        //        ThreadSafeFillGridModelADMComparison d = new ThreadSafeFillGridModelADMComparison(threadsafefillGridLiveADMData);

        //        optionRealtimeMonitor.Invoke(d, optionRealtimeMonitor);
        //    }
        //    else
        //    {
        //        threadsafefillGridLiveADMData(optionRealtimeMonitor);
        //    }

        //}

        //internal void threadsafefillGridLiveADMData(OptionRealtimeMonitor optionRealtimeMonitor)
        //{
        //    try
        //    {
        //        DataGridView gridLiveFCMData = optionRealtimeMonitor.getGridLiveFCMData;



        //        List<ADMPositionImportWeb> admPositionImportWeb = FCM_DataImportLibrary.FCM_Import_Consolidated;



        //        //gridLiveFCMData.RowCount = admPositionImportWeb.Count;


        //        int rowIdx = 0;


        //        Color rowColor1 = Color.DarkGray;
        //        Color rowColor2 = Color.Black;

        //        //Color rowColor1 = Color.DarkGray;
        //        //Color rowColor2 = Color.DarkBlue;

        //        Color currentRowColor = rowColor1;

        //        //TODO this needs to be changed so that instrumentCnt not part of gui
        //        //for (int instrumentCnt = 0; instrumentCnt <= DataCollectionLibrary.instrumentList.Count; instrumentCnt++)
        //        {

        //            for (int admWebPositionCounter = 0; admWebPositionCounter < admPositionImportWeb.Count; admWebPositionCounter++)
        //            {
        //                //if (admPositionImportWeb[admWebPositionCounter].instrument.idinstrument 
        //                //        == DataCollectionLibrary.instrumentList[instrumentCnt].idinstrument)
        //                {
        //                    admPositionImportWeb[admWebPositionCounter].liveADMRowIdx = rowIdx;

        //                    switch (rowIdx % 2)
        //                    {
        //                        case 0:
        //                            currentRowColor = rowColor1;
        //                            break;

        //                        case 1:
        //                            currentRowColor = rowColor2;
        //                            break;

        //                    }


        //                    gridLiveFCMData
        //                            .Rows[rowIdx]
        //                                .HeaderCell.Style.BackColor = currentRowColor;

        //                    gridLiveFCMData
        //                            .Rows[rowIdx]
        //                            .HeaderCell.Value =
        //                                admPositionImportWeb[admWebPositionCounter].asset.cqgsymbol;

        //                    //admPositionsCheckAndFillGrid(admPositionImportWeb[stratCounter],
        //                    //            rowIdx, true);

        //                    DateTime currentDate = DateTime.Now;



        //                    gridLiveFCMData.Rows[rowIdx].Cells[(int)OPTION_LIVE_ADM_DATA_COLUMNS.POFFIC].Value =
        //                        admPositionImportWeb[admWebPositionCounter].POFFIC;

        //                    gridLiveFCMData.Rows[rowIdx].Cells[(int)OPTION_LIVE_ADM_DATA_COLUMNS.PACCT].Value =
        //                        admPositionImportWeb[admWebPositionCounter].PACCT;



        //                    gridLiveFCMData.Rows[rowIdx].Cells[(int)OPTION_LIVE_ADM_DATA_COLUMNS.NET_EDITABLE].Value =
        //                        admPositionImportWeb[admWebPositionCounter].netContractsEditable;

        //                    gridLiveFCMData.Rows[rowIdx].Cells[(int)OPTION_LIVE_ADM_DATA_COLUMNS.NET_AT_ADM].Value =
        //                        admPositionImportWeb[admWebPositionCounter].Net;


        //                    gridLiveFCMData.Rows[rowIdx].Cells[(int)OPTION_LIVE_ADM_DATA_COLUMNS.LONG_TRANS].Value =
        //                        admPositionImportWeb[admWebPositionCounter].transNetLong;

        //                    gridLiveFCMData.Rows[rowIdx].Cells[(int)OPTION_LIVE_ADM_DATA_COLUMNS.SHORT_TRANS].Value =
        //                        admPositionImportWeb[admWebPositionCounter].transNetShort;


        //                    //gridLiveADMData.Rows[rowCounter].Cells[(int)OPTION_LIVE_ADM_DATA_COLUMNS.MODEL].Value =
        //                    //                liveADMStrategyInfo.admLegInfo[legCounter].numberOfModelContracts;


        //                    TimeSpan span = admPositionImportWeb[admWebPositionCounter].asset.expirationdate.Date - currentDate.Date;

        //                    gridLiveFCMData.Rows[rowIdx].Cells[(int)OPTION_LIVE_ADM_DATA_COLUMNS.CNTDN].Value =
        //                                        span.Days;

        //                    gridLiveFCMData.Rows[rowIdx].Cells[(int)OPTION_LIVE_ADM_DATA_COLUMNS.EXPR].Value =
        //                        new DateTime(
        //                                            admPositionImportWeb[admWebPositionCounter].asset.expirationdate.Year,
        //                                            admPositionImportWeb[admWebPositionCounter].asset.expirationdate.Month,
        //                                            admPositionImportWeb[admWebPositionCounter].asset.expirationdate.Day,
        //                                            admPositionImportWeb[admWebPositionCounter].asset.optionExpirationTime.Hour,
        //                                            admPositionImportWeb[admWebPositionCounter].asset.optionExpirationTime.Minute,
        //                                            0
        //                                        )
        //                                        .ToString("yyyy-MM-dd HH:mm", DateTimeFormatInfo.InvariantInfo);

        //                    gridLiveFCMData.Rows[rowIdx].Cells[(int)OPTION_LIVE_ADM_DATA_COLUMNS.AVG_LONG_TRANS_PRC].Value =
        //                        admPositionImportWeb[admWebPositionCounter].transAvgLongPrice;

        //                    gridLiveFCMData.Rows[rowIdx].Cells[(int)OPTION_LIVE_ADM_DATA_COLUMNS.AVG_SHORT_TRANS_PRC].Value =
        //                        admPositionImportWeb[admWebPositionCounter].transAvgShortPrice;

        //                    gridLiveFCMData.Rows[rowIdx].Cells[(int)OPTION_LIVE_ADM_DATA_COLUMNS.STRIKE].Value =
        //                        admPositionImportWeb[admWebPositionCounter].strike;

        //                    gridLiveFCMData.Rows[rowIdx].Cells[(int)OPTION_LIVE_ADM_DATA_COLUMNS.DESCRIP].Value =
        //                        admPositionImportWeb[admWebPositionCounter].Description;

        //                    gridLiveFCMData.Rows[rowIdx].Cells[(int)OPTION_LIVE_ADM_DATA_COLUMNS.CUSIP].Value =
        //                        admPositionImportWeb[admWebPositionCounter].PCUSIP;

        //                    gridLiveFCMData.Rows[rowIdx].Cells[(int)OPTION_LIVE_ADM_DATA_COLUMNS.ADMPOSWEB_IDX].Value =
        //                        admWebPositionCounter;

        //                    gridLiveFCMData.Rows[rowIdx].Cells[(int)OPTION_LIVE_ADM_DATA_COLUMNS.INSTRUMENT_ID].Value =
        //                        admPositionImportWeb[admWebPositionCounter].instrument.idinstrument;


        //                    //*********************************


        //                    gridLiveFCMData
        //                            .Rows[rowIdx]
        //                                .HeaderCell.Style.BackColor = currentRowColor;

        //                    rowIdx++;

        //                    //rowIdx += liveADMStrategyInfoList[stratCounter].admLegInfo.Count + 1;



        //                }
        //            }
        //        }

        //        //updateColorOfADMStrategyGrid();

        //    }
        //    catch (Exception ex)
        //    {
        //        TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
        //    }

        //    //return liveLegRowIdexes;
        //}

        //public void sendUpdateToADMPositionsGrid(OptionRealtimeMonitor optionRealtimeMonitor)  //*eQuoteType cqgQuoteType,*/ int spreadExpressionIdx /*int colIdx*/)
        //{
        //    //CQGQuote quote = optionSpreadExpressionList[spreadExpressionIdx].cqgInstrument.Quotes[cqgQuoteType];

        //    DataGridView gridLiveFCMData = optionRealtimeMonitor.getGridLiveFCMData;

        //    try
        //    {

        //        List<ADMPositionImportWeb> admPositionImportWeb = FCM_DataImportLibrary.FCM_Import_Consolidated;

        //        //int optionSpreadCounter = 0;

        //        //List<LiveADMStrategyInfo> liveADMStrategyInfoList = optionSpreadManager.liveADMStrategyInfoList;

        //        for (int admWebPositionCounter = 0; admWebPositionCounter < admPositionImportWeb.Count; admWebPositionCounter++)
        //        {

        //            //    int totalLegs = liveADMStrategyInfoList[optionSpreadCounter].admLegInfo.Count;

        //            //    //if (optionSpreadExpressionList[spreadExpressionIdx].cqgInstrument != null)
        //            //    for (int legCounter = 0; legCounter < totalLegs; legCounter++)
        //            //    {
        //            if (admPositionImportWeb[admWebPositionCounter].optionSpreadExpression != null)
        //            {
        //                CQGInstrument cqgInstrument = admPositionImportWeb[admWebPositionCounter].optionSpreadExpression.cqgInstrument;

        //                if (cqgInstrument != null)  // && CQG. cqgInstrument)
        //                {
        //                    MongoDB_OptionSpreadExpression optionSpreadExpressionList =
        //                        admPositionImportWeb[admWebPositionCounter].optionSpreadExpression;

        //                    //checkUpdateStatus(admPositionImportWeb[admWebPositionCounter].liveADMRowIdx,
        //                    //    optionSpreadExpressionList, true);

        //                    //optionSpreadManager.statusAndConnectedUpdates.checkUpdateStatus(gridLiveFCMData, admPositionImportWeb[admWebPositionCounter].liveADMRowIdx,
        //                    //        (int)OPTION_LIVE_ADM_DATA_COLUMNS.TIME,
        //                    //        optionSpreadExpressionList);

        //                    //if (optionSpreadManager.realtimeMonitorSettings.eodAnalysis)
        //                    if (optionSpreadExpressionList.instrument.eodAnalysisAtInstrument)
        //                    {
        //                        gridLiveFCMData.Columns[(int)OPTION_LIVE_ADM_DATA_COLUMNS.TIME].DefaultCellStyle.Font = new Font("Tahoma", 6);

        //                        fillLiveADMDataPage(optionRealtimeMonitor, admPositionImportWeb[admWebPositionCounter].liveADMRowIdx,
        //                                (int)OPTION_LIVE_ADM_DATA_COLUMNS.TIME,
        //                                optionSpreadExpressionList.lastTimeUpdated.ToString("yyyy-MM-dd HH:mm", DateTimeFormatInfo.InvariantInfo),
        //                                false, 0);
        //                    }
        //                    else
        //                    {
        //                        gridLiveFCMData.Columns[(int)OPTION_LIVE_ADM_DATA_COLUMNS.TIME].DefaultCellStyle.Font = new Font("Tahoma", 8);

        //                        fillLiveADMDataPage(optionRealtimeMonitor, admPositionImportWeb[admWebPositionCounter].liveADMRowIdx,
        //                                (int)OPTION_LIVE_ADM_DATA_COLUMNS.TIME,
        //                                optionSpreadExpressionList.lastTimeUpdated.ToString("HH:mm", DateTimeFormatInfo.InvariantInfo),
        //                                false, 0);
        //                    }

        //                    fillLiveADMDataPage(optionRealtimeMonitor, admPositionImportWeb[admWebPositionCounter].liveADMRowIdx,
        //                            (int)OPTION_LIVE_ADM_DATA_COLUMNS.ASK,
        //                            cqgInstrument.ToDisplayPrice(optionSpreadExpressionList.ask), false, optionSpreadExpressionList.ask);

        //                    fillLiveADMDataPage(optionRealtimeMonitor, admPositionImportWeb[admWebPositionCounter].liveADMRowIdx,
        //                            (int)OPTION_LIVE_ADM_DATA_COLUMNS.BID,
        //                            cqgInstrument.ToDisplayPrice(optionSpreadExpressionList.bid), false, optionSpreadExpressionList.bid);

        //                    fillLiveADMDataPage(optionRealtimeMonitor, admPositionImportWeb[admWebPositionCounter].liveADMRowIdx,
        //                            (int)OPTION_LIVE_ADM_DATA_COLUMNS.LAST,
        //                            cqgInstrument.ToDisplayPrice(optionSpreadExpressionList.trade), false, optionSpreadExpressionList.trade);

        //                    fillLiveADMDataPage(optionRealtimeMonitor, admPositionImportWeb[admWebPositionCounter].liveADMRowIdx,
        //                            (int)OPTION_LIVE_ADM_DATA_COLUMNS.DFLT_PRICE,
        //                            cqgInstrument.ToDisplayPrice(optionSpreadExpressionList.defaultPrice),
        //                                false, optionSpreadExpressionList.defaultPrice);

        //                    fillLiveADMDataPage(optionRealtimeMonitor, admPositionImportWeb[admWebPositionCounter].liveADMRowIdx,
        //                            (int)OPTION_LIVE_ADM_DATA_COLUMNS.STTLE,
        //                            cqgInstrument.ToDisplayPrice(optionSpreadExpressionList.settlement),
        //                                false, optionSpreadExpressionList.settlement);

        //                    if (optionSpreadExpressionList.settlementDateTime.Date.CompareTo(DateTime.Now.Date) >= 0)
        //                    {
        //                        fillLiveADMDataPage(optionRealtimeMonitor, admPositionImportWeb[admWebPositionCounter].liveADMRowIdx,
        //                                (int)OPTION_LIVE_ADM_DATA_COLUMNS.SETL_TIME,
        //                                optionSpreadExpressionList.settlementDateTime.ToString("yyyy-MM-dd HH:mm", DateTimeFormatInfo.InvariantInfo),
        //                                    true, 1);
        //                    }
        //                    else
        //                    {
        //                        fillLiveADMDataPage(optionRealtimeMonitor, admPositionImportWeb[admWebPositionCounter].liveADMRowIdx,
        //                                (int)OPTION_LIVE_ADM_DATA_COLUMNS.SETL_TIME,
        //                                optionSpreadExpressionList.settlementDateTime.ToString("yyyy-MM-dd HH:mm", DateTimeFormatInfo.InvariantInfo),
        //                                    true, -1);
        //                    }

        //                    fillLiveADMDataPage(optionRealtimeMonitor, admPositionImportWeb[admWebPositionCounter].liveADMRowIdx,
        //                                (int)OPTION_LIVE_ADM_DATA_COLUMNS.YEST_STTLE,
        //                                cqgInstrument.ToDisplayPrice(optionSpreadExpressionList.yesterdaySettlement),
        //                                    false, optionSpreadExpressionList.yesterdaySettlement);

        //                    fillLiveADMDataPage(optionRealtimeMonitor, admPositionImportWeb[admWebPositionCounter].liveADMRowIdx,
        //                                (int)OPTION_LIVE_ADM_DATA_COLUMNS.IMPL_VOL,
        //                                Math.Round(optionSpreadExpressionList.impliedVol, 2).ToString(),
        //                                false, optionSpreadExpressionList.impliedVol);

        //                    fillLiveADMDataPage(optionRealtimeMonitor, admPositionImportWeb[admWebPositionCounter].liveADMRowIdx,
        //                                (int)OPTION_LIVE_ADM_DATA_COLUMNS.THEOR_PRICE,
        //                                cqgInstrument.ToDisplayPrice(
        //                                optionSpreadExpressionList.theoreticalOptionPrice),
        //                                false, optionSpreadExpressionList.theoreticalOptionPrice);

        //                    fillLiveADMDataPage(optionRealtimeMonitor, admPositionImportWeb[admWebPositionCounter].liveADMRowIdx,
        //                                (int)OPTION_LIVE_ADM_DATA_COLUMNS.SPAN_IMPL_VOL,
        //                                Math.Round(optionSpreadExpressionList.impliedVolFromSpan, 2).ToString(),
        //                                false, optionSpreadExpressionList.impliedVolFromSpan);

        //                    fillLiveADMDataPage(optionRealtimeMonitor, admPositionImportWeb[admWebPositionCounter].liveADMRowIdx,
        //                                (int)OPTION_LIVE_ADM_DATA_COLUMNS.SETL_IMPL_VOL,
        //                                Math.Round(optionSpreadExpressionList.settlementImpliedVol, 2).ToString(),
        //                                false, optionSpreadExpressionList.settlementImpliedVol);

        //                    fillLiveADMDataPage(optionRealtimeMonitor, admPositionImportWeb[admWebPositionCounter].liveADMRowIdx,
        //                                (int)OPTION_LIVE_ADM_DATA_COLUMNS.RFR,
        //                                optionSpreadExpressionList.riskFreeRate.ToString(),
        //                                false, optionSpreadExpressionList.riskFreeRate);


        //                    fillLiveADMDataPage(optionRealtimeMonitor, admPositionImportWeb[admWebPositionCounter].liveADMRowIdx,
        //                                (int)OPTION_LIVE_ADM_DATA_COLUMNS.PL_DAY_CHG,
        //                                    Math.Round(admPositionImportWeb[admWebPositionCounter].positionTotals.pAndLDay, 2).ToString(),
        //                                    true, admPositionImportWeb[admWebPositionCounter].positionTotals.pAndLDay);

        //                    fillLiveADMDataPage(optionRealtimeMonitor, admPositionImportWeb[admWebPositionCounter].liveADMRowIdx,
        //                                (int)OPTION_LIVE_ADM_DATA_COLUMNS.PL_TRANS,
        //                                    Math.Round(admPositionImportWeb[admWebPositionCounter].positionTotals.pAndLDayOrders, 2).ToString(),
        //                                    true, admPositionImportWeb[admWebPositionCounter].positionTotals.pAndLDayOrders);

        //                    //fillLiveADMDataPage(optionRealtimeMonitor, admPositionImportWeb[admWebPositionCounter].liveADMRowIdx,
        //                    //            (int)OPTION_LIVE_ADM_DATA_COLUMNS.PL_DAY_CHG,
        //                    //                Math.Round(admPositionImportWeb[admWebPositionCounter].contractData.pAndLDay, 2).ToString(),
        //                    //                true, admPositionImportWeb[admWebPositionCounter].contractData.pAndLDay);


        //                    fillLiveADMDataPage(optionRealtimeMonitor, admPositionImportWeb[admWebPositionCounter].liveADMRowIdx,
        //                                (int)OPTION_LIVE_ADM_DATA_COLUMNS.DELTA,
        //                                    Math.Round(admPositionImportWeb[admWebPositionCounter].positionTotals.delta, 2).ToString(),
        //                                    true, admPositionImportWeb[admWebPositionCounter].positionTotals.delta);



        //                    //            //int numberOfContracts = (int)optionStrategies[optionSpreadCounter].optionStrategyParameters[
        //                    //            //                    (int)TBL_STRATEGY_STATE_FIELDS.currentPosition].stateValueParsed[legCounter];

        //                    //            //fillLiveADMDataPage(liveADMStrategyInfoList[optionSpreadCounter].admLegInfo[legCounter].rowIndex,
        //                    //            //            (int)OPTION_LIVE_ADM_DATA_COLUMNS.SPREAD_QTY,
        //                    //            //                numberOfContracts.ToString(), true, numberOfContracts);
        //                    //        }
        //                }
        //            }

        //            //    fillLiveADMDataPage(liveADMStrategyInfoList[optionSpreadCounter].summaryRowIdx,
        //            //                (int)OPTION_LIVE_ADM_DATA_COLUMNS.PL_DAY_CHG,
        //            //                    Math.Round(liveADMStrategyInfoList[optionSpreadCounter].liveSpreadADMTotals.pAndLDay, 2).ToString(),
        //            //                    true, liveADMStrategyInfoList[optionSpreadCounter].liveSpreadADMTotals.pAndLDay);

        //            //    fillLiveADMDataPage(liveADMStrategyInfoList[optionSpreadCounter].summaryRowIdx,
        //            //                (int)OPTION_LIVE_ADM_DATA_COLUMNS.DELTA,
        //            //                    Math.Round(liveADMStrategyInfoList[optionSpreadCounter].liveSpreadADMTotals.delta, 2).ToString(),
        //            //                    true, liveADMStrategyInfoList[optionSpreadCounter].liveSpreadADMTotals.delta);

        //            //    optionSpreadCounter++;
        //        }



        //    }
        //    catch (Exception ex)
        //    {
        //        TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
        //    }

        //}

        //delegate void ThreadSafeFillLiveDataPageDelegate(OptionRealtimeMonitor optionRealtimeMonitor,
        //    int row, int col, String displayValue,
        //    bool updateColor, double value);

        //public void fillLiveADMDataPage(OptionRealtimeMonitor optionRealtimeMonitor,
        //    int row, int col, String displayValue,
        //    bool updateColor, double value)
        //{
        //    try
        //    {
        //        if (optionRealtimeMonitor.InvokeRequired)
        //        {
        //            ThreadSafeFillLiveDataPageDelegate d = new ThreadSafeFillLiveDataPageDelegate(threadSafeFillLiveADMDataPage);

        //            optionRealtimeMonitor.Invoke(d, optionRealtimeMonitor, row, col, displayValue, updateColor, value);
        //        }
        //        else
        //        {
        //            threadSafeFillLiveADMDataPage(optionRealtimeMonitor, row, col, displayValue, updateColor, value);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
        //    }
        //}

        //public void threadSafeFillLiveADMDataPage(OptionRealtimeMonitor optionRealtimeMonitor,
        //    int row, int col, String displayValue,
        //    bool updateColor, double value)
        //{
        //    DataGridView gridLiveFCMData = optionRealtimeMonitor.getGridLiveFCMData;

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
        //                gridLiveFCMData.Rows[rowToUpdate].Cells[col].Style.BackColor = CommonFormManipulation.plUpDownColor(value);
        //            }
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
        //    }
        //}

    }
}
