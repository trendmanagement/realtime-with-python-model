using RealtimeSpreadMonitor.Forms;
using RealtimeSpreadMonitor.Model;
using System;
using System.Data;
using System.Globalization;
using System.Text;
using System.Windows.Forms;

namespace RealtimeSpreadMonitor.FormManipulation
{
    internal class GridViewContractSummaryManipulation
    {
        internal GridViewContractSummaryManipulation()
        {

        }



        public void setupContractSummaryLiveData(OptionRealtimeMonitor optionRealtimeMonitor)
        {

            DataGridView gridViewContractSummary = optionRealtimeMonitor.getGridViewContractSummary;

            gridViewContractSummary.DataSource = DataCollectionLibrary.contractSummaryDataTable;

            try
            {
                Type contractSummaryColTypes = typeof(CONTRACTSUMMARY_DATA_COLUMNS);
                Array contractSummaryColTypesArray = Enum.GetNames(contractSummaryColTypes);

                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < contractSummaryColTypesArray.Length; i++)
                {
                    sb.Clear();

                    sb.Append(contractSummaryColTypesArray.GetValue(i).ToString());

                    DataCollectionLibrary.contractSummaryDataTable.Columns.Add(sb.ToString().Replace('_', ' '));
                }

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
                        if (DataCollectionLibrary.portfolioAllocation.accountAllocation_KeyAccountname.ContainsKey(ap.name))
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

                                    dataTable.Rows[rowCount][(int)CONTRACTSUMMARY_DATA_COLUMNS.ORDER_QTY] = p.qty - p.prev_qty;

                                    dataTable.Rows[rowCount][(int)CONTRACTSUMMARY_DATA_COLUMNS.INSTRUMENT_ID] = im.idinstrument;

                                    dataTable.Rows[rowCount][(int)CONTRACTSUMMARY_DATA_COLUMNS.ACCOUNT] = ap.name;

                                    dataTable.Rows[rowCount][(int)CONTRACTSUMMARY_DATA_COLUMNS.FCM_OFFICE] = ac.FCM_OFFICE;

                                    dataTable.Rows[rowCount][(int)CONTRACTSUMMARY_DATA_COLUMNS.FCM_ACCT] = ac.FCM_ACCT;

                                    dataTable.Rows[rowCount][(int)CONTRACTSUMMARY_DATA_COLUMNS.STRIKE_PRICE] = p.asset.strikeprice;
                                }

                                UpdateCell(rowCount, (int)CONTRACTSUMMARY_DATA_COLUMNS.RFRSH_TIME,
                                    dataTable, ap.date_now.ToString("MM-dd HH:mm", DateTimeFormatInfo.InvariantInfo));
                                //dataTable.Rows[rowCount][(int)CONTRACTSUMMARY_DATA_COLUMNS.RFRSH_TIME]
                                //    = ap.date_now.ToString("MM-dd HH:mm", DateTimeFormatInfo.InvariantInfo);

                                UpdateCell(rowCount, (int)CONTRACTSUMMARY_DATA_COLUMNS.TIME,
                                    dataTable, p.mose.lastTimeUpdated.ToString("MM-dd HH:mm", DateTimeFormatInfo.InvariantInfo));
                                //dataTable.Rows[rowCount][(int)CONTRACTSUMMARY_DATA_COLUMNS.TIME]
                                //    = p.mose.lastTimeUpdated.ToString("MM-dd HH:mm", DateTimeFormatInfo.InvariantInfo);

                                UpdateCell(rowCount, (int)CONTRACTSUMMARY_DATA_COLUMNS.PL_DAY_CHG,
                                    dataTable, Math.Round(p.positionTotals.pAndLDay, 2).ToString());
                                //dataTable.Rows[rowCount][(int)CONTRACTSUMMARY_DATA_COLUMNS.PL_DAY_CHG]
                                //    = Math.Round(p.positionTotals.pAndLDay,2);

                                UpdateCell(rowCount, (int)CONTRACTSUMMARY_DATA_COLUMNS.ORDER_PL_CHG,
                                    dataTable, Math.Round(p.positionTotals.pAndLDayOrders, 2).ToString());
                                //dataTable.Rows[rowCount][(int)CONTRACTSUMMARY_DATA_COLUMNS.ORDER_PL_CHG]
                                //    = Math.Round(p.positionTotals.pAndLDayOrders,2);

                                UpdateCell(rowCount, (int)CONTRACTSUMMARY_DATA_COLUMNS.DELTA,
                                    dataTable, Math.Round(p.positionTotals.delta, 2).ToString());
                                //dataTable.Rows[rowCount][(int)CONTRACTSUMMARY_DATA_COLUMNS.DELTA]
                                //    = Math.Round(p.positionTotals.delta, 2);

                                UpdateCell(rowCount, (int)CONTRACTSUMMARY_DATA_COLUMNS.DFLT_PRICE,
                                    dataTable, p.mose.defaultPrice.ToString());
                                //dataTable.Rows[rowCount][(int)CONTRACTSUMMARY_DATA_COLUMNS.DFLT_PRICE]
                                //    = p.mose.defaultPrice;

                                UpdateCell(rowCount, (int)CONTRACTSUMMARY_DATA_COLUMNS.THEOR_PRICE,
                                    dataTable, p.mose.theoreticalOptionPrice.ToString());
                                //dataTable.Rows[rowCount][(int)CONTRACTSUMMARY_DATA_COLUMNS.THEOR_PRICE]
                                //    = p.mose.theoreticalOptionPrice;

                                UpdateCell(rowCount, (int)CONTRACTSUMMARY_DATA_COLUMNS.SPAN_IMPL_VOL,
                                    dataTable, Math.Round(p.mose.impliedVolFromSpan, 2).ToString());
                                //dataTable.Rows[rowCount][(int)CONTRACTSUMMARY_DATA_COLUMNS.SPAN_IMPL_VOL]
                                //    = Math.Round(p.mose.impliedVolFromSpan,2);

                                UpdateCell(rowCount, (int)CONTRACTSUMMARY_DATA_COLUMNS.SETL_IMPL_VOL,
                                    dataTable, Math.Round(p.mose.settlementImpliedVol, 2).ToString());
                                //dataTable.Rows[rowCount][(int)CONTRACTSUMMARY_DATA_COLUMNS.SETL_IMPL_VOL]
                                //    = Math.Round(p.mose.settlementImpliedVol,2);

                                UpdateCell(rowCount, (int)CONTRACTSUMMARY_DATA_COLUMNS.IMPL_VOL,
                                    dataTable, Math.Round(p.mose.impliedVol, 2).ToString());
                                //dataTable.Rows[rowCount][(int)CONTRACTSUMMARY_DATA_COLUMNS.IMPL_VOL]
                                //    = Math.Round(p.mose.impliedVol,2);

                                UpdateCell(rowCount, (int)CONTRACTSUMMARY_DATA_COLUMNS.BID,
                                    dataTable, p.mose.bid.ToString());
                                //dataTable.Rows[rowCount][(int)CONTRACTSUMMARY_DATA_COLUMNS.BID]
                                //    = p.mose.bid;

                                UpdateCell(rowCount, (int)CONTRACTSUMMARY_DATA_COLUMNS.ASK,
                                    dataTable, p.mose.ask.ToString());
                                //dataTable.Rows[rowCount][(int)CONTRACTSUMMARY_DATA_COLUMNS.ASK]
                                //    = p.mose.ask;

                                UpdateCell(rowCount, (int)CONTRACTSUMMARY_DATA_COLUMNS.LAST,
                                    dataTable, p.mose.trade.ToString());
                                //dataTable.Rows[rowCount][(int)CONTRACTSUMMARY_DATA_COLUMNS.LAST]
                                //    = p.mose.trade;

                                UpdateCell(rowCount, (int)CONTRACTSUMMARY_DATA_COLUMNS.STTLE,
                                    dataTable, p.mose.settlement.ToString());
                                //dataTable.Rows[rowCount][(int)CONTRACTSUMMARY_DATA_COLUMNS.STTLE]
                                //    = p.mose.settlement;

                                UpdateCell(rowCount, (int)CONTRACTSUMMARY_DATA_COLUMNS.SETL_TIME,
                                    dataTable, p.mose.settlementDateTime.ToString("MM-dd HH:mm", DateTimeFormatInfo.InvariantInfo));
                                //dataTable.Rows[rowCount][(int)CONTRACTSUMMARY_DATA_COLUMNS.SETL_TIME]
                                //    = p.mose.settlementDateTime.ToString("MM-dd HH:mm", DateTimeFormatInfo.InvariantInfo);

                                UpdateCell(rowCount, (int)CONTRACTSUMMARY_DATA_COLUMNS.YEST_STTLE,
                                    dataTable, p.mose.yesterdaySettlement.ToString());
                                //dataTable.Rows[rowCount][(int)CONTRACTSUMMARY_DATA_COLUMNS.YEST_STTLE]
                                //    = p.mose.yesterdaySettlement;


                                rowCount++;
                            }
                        }
                    }


                }
            }
        }

        private void UpdateCell(int row, int col, DataTable dataTable, string displayValue)
        {
            if (dataTable.Rows[row][col] == null
                || dataTable.Rows[row][col].ToString().CompareTo(displayValue) != 0)
            {
                dataTable.Rows[row][col] = displayValue;
            }
        }


        //delegate void ThreadSafeFillLiveDataPageDelegate(
        //    OptionRealtimeMonitor optionRealtimeMonitor,
        //    int row, int col, String displayValue,
        //    bool updateColor, double value);

        //public void fillContractSummaryLiveData(OptionRealtimeMonitor optionRealtimeMonitor,
        //    int row, int col, String displayValue,
        //    bool updateColor, double value)
        //{
        //    try
        //    {
        //        if (optionRealtimeMonitor.InvokeRequired)
        //        {
        //            ThreadSafeFillLiveDataPageDelegate d = new ThreadSafeFillLiveDataPageDelegate(threadSafeFillContractSummaryLiveData);

        //            optionRealtimeMonitor.Invoke(d, optionRealtimeMonitor, row, col, displayValue, updateColor, value);
        //        }
        //        else
        //        {
        //            threadSafeFillContractSummaryLiveData(optionRealtimeMonitor, row, col, displayValue, updateColor, value);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
        //    }
        //}

        //public void threadSafeFillContractSummaryLiveData(
        //    OptionRealtimeMonitor optionRealtimeMonitor,
        //    int row, int col, String displayValue,
        //    bool updateColor, double value)
        //{
        //    try
        //    {
        //        int rowToUpdate = row;

        //        if (optionRealtimeMonitor.getGridViewContractSummary.Rows[rowToUpdate].Cells[col].Value == null
        //            ||
        //            optionRealtimeMonitor.getGridViewContractSummary.Rows[rowToUpdate].Cells[col].Value.ToString().CompareTo(displayValue) != 0
        //            )
        //        {
        //            optionRealtimeMonitor.getGridViewContractSummary.Rows[rowToUpdate].Cells[col].Value = displayValue;

        //            if (updateColor)
        //            {
        //                optionRealtimeMonitor.getGridViewContractSummary.Rows[rowToUpdate].Cells[col].Style.BackColor
        //                    = CommonFormManipulation.plUpDownColor(value);
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
