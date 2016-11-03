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

                            dataTable.Rows[rowCount][(int)CONTRACTSUMMARY_DATA_COLUMNS.TIME]
                                = p.mose.lastTimeUpdated.ToString("MM-dd HH:mm", DateTimeFormatInfo.InvariantInfo);

                            //dataTable.Rows[rowCount][(int)CONTRACTSUMMARY_DATA_COLUMNS.PL_DAY_CHG]
                            //    = (p.mose.defaultPrice - p.mose.yesterdaySettlement) / im.ticksize * im.tickvalue * p.prev_qty;

                            dataTable.Rows[rowCount][(int)CONTRACTSUMMARY_DATA_COLUMNS.PL_DAY_CHG]
                                = p.positionTotals.pAndLDay;

                            dataTable.Rows[rowCount][(int)CONTRACTSUMMARY_DATA_COLUMNS.ORDER_PL_CHG]
                                = p.positionTotals.pAndLDayOrders;

                            dataTable.Rows[rowCount][(int)CONTRACTSUMMARY_DATA_COLUMNS.DELTA]
                                = Math.Round(p.positionTotals.delta, 2);

                            dataTable.Rows[rowCount][(int)CONTRACTSUMMARY_DATA_COLUMNS.DFLT_PRICE]
                                = p.mose.defaultPrice;

                            dataTable.Rows[rowCount][(int)CONTRACTSUMMARY_DATA_COLUMNS.THEOR_PRICE]
                                = p.mose.theoreticalOptionPrice;

                            dataTable.Rows[rowCount][(int)CONTRACTSUMMARY_DATA_COLUMNS.SPAN_IMPL_VOL]
                                = Math.Round(p.mose.impliedVolFromSpan,2);

                            dataTable.Rows[rowCount][(int)CONTRACTSUMMARY_DATA_COLUMNS.SETL_IMPL_VOL]
                                = Math.Round(p.mose.settlementImpliedVol,2);

                            dataTable.Rows[rowCount][(int)CONTRACTSUMMARY_DATA_COLUMNS.IMPL_VOL]
                                = Math.Round(p.mose.impliedVol,2);

                            dataTable.Rows[rowCount][(int)CONTRACTSUMMARY_DATA_COLUMNS.BID]
                                = p.mose.bid;

                            dataTable.Rows[rowCount][(int)CONTRACTSUMMARY_DATA_COLUMNS.ASK]
                                = p.mose.ask;

                            dataTable.Rows[rowCount][(int)CONTRACTSUMMARY_DATA_COLUMNS.LAST]
                                = p.mose.trade;

                            dataTable.Rows[rowCount][(int)CONTRACTSUMMARY_DATA_COLUMNS.STTLE]
                                = p.mose.settlement;

                            dataTable.Rows[rowCount][(int)CONTRACTSUMMARY_DATA_COLUMNS.SETL_TIME]
                                = p.mose.settlementDateTime.ToString("MM-dd HH:mm", DateTimeFormatInfo.InvariantInfo);

                            dataTable.Rows[rowCount][(int)CONTRACTSUMMARY_DATA_COLUMNS.YEST_STTLE]
                                = p.mose.yesterdaySettlement;


                            rowCount++;
                        }
                    }


                }
            }
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
