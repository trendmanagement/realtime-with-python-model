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
        DataGridView gridViewContractSummary;

        internal GridViewContractSummaryManipulation()
        {

        }



        public void setupContractSummaryLiveData(OptionRealtimeMonitor optionRealtimeMonitor)
        {

            gridViewContractSummary = optionRealtimeMonitor.getGridViewContractSummary;

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

            DataTable dataTable = DataCollectionLibrary.contractSummaryDataTable.Copy();

            foreach (DataRow r in dataTable.Rows)
            {
                r[(int)CONTRACTSUMMARY_DATA_COLUMNS.UPDATED] = false;
            }

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


                                DataRow[] contractSummary_resultId = dataTable.Select(CONTRACTSUMMARY_DATA_COLUMNS.ID.ToString() + "='" + p.asset._id + ap.name + "'");

                                DataRow rowToUpdate;
                                if (contractSummary_resultId.Length > 0)
                                {
                                    rowToUpdate = contractSummary_resultId[0];
                                }
                                else
                                {
                                    dataTable.Rows.Add();
                                    rowToUpdate = dataTable.Rows[dataTable.Rows.Count - 1];

                                    rowToUpdate[(int)CONTRACTSUMMARY_DATA_COLUMNS.ID] = p.asset._id + ap.name;

                                    rowToUpdate[(int)CONTRACTSUMMARY_DATA_COLUMNS.CONTRACT] = p.asset.cqgsymbol;

                                    rowToUpdate[(int)CONTRACTSUMMARY_DATA_COLUMNS.QTY] = p.qty;

                                    rowToUpdate[(int)CONTRACTSUMMARY_DATA_COLUMNS.PREV_QTY] = p.prev_qty;

                                    rowToUpdate[(int)CONTRACTSUMMARY_DATA_COLUMNS.ORDER_QTY] = p.qty - p.prev_qty;

                                    rowToUpdate[(int)CONTRACTSUMMARY_DATA_COLUMNS.INSTRUMENT_ID] = im.idinstrument;

                                    rowToUpdate[(int)CONTRACTSUMMARY_DATA_COLUMNS.ACCOUNT] = ap.name;

                                    rowToUpdate[(int)CONTRACTSUMMARY_DATA_COLUMNS.FCM_OFFICE] = ac.FCM_OFFICE;

                                    rowToUpdate[(int)CONTRACTSUMMARY_DATA_COLUMNS.FCM_ACCT] = ac.FCM_ACCT;

                                    rowToUpdate[(int)CONTRACTSUMMARY_DATA_COLUMNS.STRIKE_PRICE] = p.asset.strikeprice;
                                }


                                rowToUpdate[(int)CONTRACTSUMMARY_DATA_COLUMNS.UPDATED] = true;

                                UpdateCell((int)CONTRACTSUMMARY_DATA_COLUMNS.RFRSH_TIME,
                                    rowToUpdate, ap.date_now.ToString("MM-dd HH:mm", DateTimeFormatInfo.InvariantInfo));
                                //rowToUpdate[(int)CONTRACTSUMMARY_DATA_COLUMNS.RFRSH_TIME]
                                //    = ap.date_now.ToString("MM-dd HH:mm", DateTimeFormatInfo.InvariantInfo);

                                UpdateCell((int)CONTRACTSUMMARY_DATA_COLUMNS.TIME,
                                    rowToUpdate, p.mose.lastTimeUpdated.ToString("MM-dd HH:mm", DateTimeFormatInfo.InvariantInfo));
                                //rowToUpdate[(int)CONTRACTSUMMARY_DATA_COLUMNS.TIME]
                                //    = p.mose.lastTimeUpdated.ToString("MM-dd HH:mm", DateTimeFormatInfo.InvariantInfo);

                                UpdateCell((int)CONTRACTSUMMARY_DATA_COLUMNS.PL_DAY_CHG,
                                    rowToUpdate, Math.Round(p.positionTotals.pAndLDay, 2).ToString());
                                //rowToUpdate[(int)CONTRACTSUMMARY_DATA_COLUMNS.PL_DAY_CHG]
                                //    = Math.Round(p.positionTotals.pAndLDay,2);

                                UpdateCell((int)CONTRACTSUMMARY_DATA_COLUMNS.ORDER_PL_CHG,
                                    rowToUpdate, Math.Round(p.positionTotals.pAndLDayOrders, 2).ToString());
                                //rowToUpdate[(int)CONTRACTSUMMARY_DATA_COLUMNS.ORDER_PL_CHG]
                                //    = Math.Round(p.positionTotals.pAndLDayOrders,2);

                                UpdateCell((int)CONTRACTSUMMARY_DATA_COLUMNS.DELTA,
                                    rowToUpdate, Math.Round(p.positionTotals.delta, 2).ToString());
                                //rowToUpdate[(int)CONTRACTSUMMARY_DATA_COLUMNS.DELTA]
                                //    = Math.Round(p.positionTotals.delta, 2);

                                UpdateCell((int)CONTRACTSUMMARY_DATA_COLUMNS.DFLT_PRICE,
                                    rowToUpdate, p.mose.defaultPrice.ToString());
                                //rowToUpdate[(int)CONTRACTSUMMARY_DATA_COLUMNS.DFLT_PRICE]
                                //    = p.mose.defaultPrice;

                                UpdateCell((int)CONTRACTSUMMARY_DATA_COLUMNS.THEOR_PRICE,
                                    rowToUpdate, p.mose.theoreticalOptionPrice.ToString());
                                //rowToUpdate[(int)CONTRACTSUMMARY_DATA_COLUMNS.THEOR_PRICE]
                                //    = p.mose.theoreticalOptionPrice;

                                UpdateCell((int)CONTRACTSUMMARY_DATA_COLUMNS.SPAN_IMPL_VOL,
                                    rowToUpdate, Math.Round(p.mose.impliedVolFromSpan, 2).ToString());
                                //rowToUpdate[(int)CONTRACTSUMMARY_DATA_COLUMNS.SPAN_IMPL_VOL]
                                //    = Math.Round(p.mose.impliedVolFromSpan,2);

                                UpdateCell((int)CONTRACTSUMMARY_DATA_COLUMNS.SETL_IMPL_VOL,
                                    rowToUpdate, Math.Round(p.mose.settlementImpliedVol, 2).ToString());
                                //rowToUpdate[(int)CONTRACTSUMMARY_DATA_COLUMNS.SETL_IMPL_VOL]
                                //    = Math.Round(p.mose.settlementImpliedVol,2);

                                UpdateCell((int)CONTRACTSUMMARY_DATA_COLUMNS.IMPL_VOL,
                                    rowToUpdate, Math.Round(p.mose.impliedVol, 2).ToString());
                                //rowToUpdate[(int)CONTRACTSUMMARY_DATA_COLUMNS.IMPL_VOL]
                                //    = Math.Round(p.mose.impliedVol,2);

                                UpdateCell((int)CONTRACTSUMMARY_DATA_COLUMNS.BID,
                                    rowToUpdate, p.mose.bid.ToString());
                                //rowToUpdate[(int)CONTRACTSUMMARY_DATA_COLUMNS.BID]
                                //    = p.mose.bid;

                                UpdateCell((int)CONTRACTSUMMARY_DATA_COLUMNS.ASK,
                                    rowToUpdate, p.mose.ask.ToString());
                                //rowToUpdate[(int)CONTRACTSUMMARY_DATA_COLUMNS.ASK]
                                //    = p.mose.ask;

                                UpdateCell((int)CONTRACTSUMMARY_DATA_COLUMNS.LAST,
                                    rowToUpdate, p.mose.trade.ToString());
                                //rowToUpdate[(int)CONTRACTSUMMARY_DATA_COLUMNS.LAST]
                                //    = p.mose.trade;

                                UpdateCell((int)CONTRACTSUMMARY_DATA_COLUMNS.STTLE,
                                    rowToUpdate, p.mose.settlement.ToString());
                                //rowToUpdate[(int)CONTRACTSUMMARY_DATA_COLUMNS.STTLE]
                                //    = p.mose.settlement;

                                UpdateCell((int)CONTRACTSUMMARY_DATA_COLUMNS.SETL_TIME,
                                    rowToUpdate, p.mose.settlementDateTime.ToString("MM-dd HH:mm", DateTimeFormatInfo.InvariantInfo));
                                //rowToUpdate[(int)CONTRACTSUMMARY_DATA_COLUMNS.SETL_TIME]
                                //    = p.mose.settlementDateTime.ToString("MM-dd HH:mm", DateTimeFormatInfo.InvariantInfo);

                                UpdateCell((int)CONTRACTSUMMARY_DATA_COLUMNS.YEST_STTLE,
                                    rowToUpdate, p.mose.yesterdaySettlement.ToString());
                                //rowToUpdate[(int)CONTRACTSUMMARY_DATA_COLUMNS.YEST_STTLE]
                                //    = p.mose.yesterdaySettlement;

                            }
                        }
                    }
                }
            }

            DataRow[] result = dataTable
                .Select(CONTRACTSUMMARY_DATA_COLUMNS.UPDATED.ToString() + "=false");

            foreach (DataRow r in result)
            {
                r.Delete();
            }

            DataCollectionLibrary.contractSummaryDataTable = dataTable;

            int saveRow = 0;
            int saveCol = 0;
            if (gridViewContractSummary.Rows.Count > 0 && gridViewContractSummary.FirstDisplayedCell != null)
                saveRow = gridViewContractSummary.FirstDisplayedCell.RowIndex;

            if (gridViewContractSummary.Columns.Count > 0 && gridViewContractSummary.FirstDisplayedCell != null)
                saveCol = gridViewContractSummary.FirstDisplayedCell.ColumnIndex;

            gridViewContractSummary.DataSource = DataCollectionLibrary.contractSummaryDataTable;

            if (saveRow != 0 && saveRow < gridViewContractSummary.Rows.Count)
                gridViewContractSummary.FirstDisplayedScrollingRowIndex = saveRow;

            if (saveCol != 0 && saveCol < gridViewContractSummary.Columns.Count)
                gridViewContractSummary.FirstDisplayedScrollingColumnIndex = saveCol;
        }

        private void UpdateCell(int col, DataRow dataTable, string displayValue)
        {
            if (dataTable[col] == null
                || dataTable[col].ToString().CompareTo(displayValue) != 0)
            {
                dataTable[col] = displayValue;
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
