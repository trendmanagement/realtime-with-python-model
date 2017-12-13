using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using RealtimeSpreadMonitor.Model;

namespace RealtimeSpreadMonitor.Forms
{
    public partial class FCM_ReportSummaryForm : Form
    {
        delegate void ThreadSafeHideUnhideContractSummaryLiveDataDelegate(int row, bool visible);

        //private OptionStrategy[] optionStrategies;
        //private Instrument_mongo[] instruments;

        //private OptionSpreadManager optionSpreadManager;

        private List<ADMPositionImportWeb> admSummaryFieldsList;

        //private ADM_IMPORT_FILE_TYPES fileImportType;

        private long instrumentSelectedIdx;

        public FCM_ReportSummaryForm()
        {
            InitializeComponent();
            
            //this.optionStrategies = optionStrategies;
            //this.instruments = instrumentArray;
            
        }



        public void fillFileDateTimeLabel(DateTime fileDateTime)
        {
            StringBuilder lastFileWriteTime = new StringBuilder();

            lastFileWriteTime.Append("ADM FILE LAST LOADED ");

            lastFileWriteTime.Append(
                fileDateTime.ToString("MMM dd yyyy HH:mm:ss", DateTimeFormatInfo.InvariantInfo));

            lblLatestADMFileDate.Text = lastFileWriteTime.ToString();
                
        }

        public void setupAdmInputSummaryGrid(List<ADMPositionImportWeb> admSummaryFieldsList)
        {
#if DEBUG
            try
#endif
            {
                

                this.admSummaryFieldsList = admSummaryFieldsList;

                fcm_InputSummaryGrid.DataSource = FCM_DataImportLibrary.FCM_fullImportDataTableForDisplay;

                Type admSummaryFieldDisplayedTypes = typeof(ADM_SUMMARY_WEB_POSITION_FIELDS_DISPLAYED);
                Array admSummaryFieldDisplayedTypeArray = Enum.GetNames(admSummaryFieldDisplayedTypes);

                //admInputSummaryGrid.ColumnCount = admSummaryFieldDisplayedTypeArray.Length;  // admSummaryFieldTypeArray.Length;

                //admInputSummaryGrid.EnableHeadersVisualStyles = false;

                //DataGridViewCellStyle colTotalPortStyle = admInputSummaryGrid.ColumnHeadersDefaultCellStyle;
                //colTotalPortStyle.BackColor = Color.Black;
                //colTotalPortStyle.ForeColor = Color.White;

                //DataGridViewCellStyle rowTotalPortStyle = admInputSummaryGrid.RowHeadersDefaultCellStyle;
                //rowTotalPortStyle.BackColor = Color.Black;
                //rowTotalPortStyle.ForeColor = Color.White;

                //for (int i = 0; i < admInputSummaryGrid.ColumnCount; i++)
                //{
                //    admInputSummaryGrid.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                //    admInputSummaryGrid.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                //}

                //int col = 0;

                if (FCM_DataImportLibrary.FCM_fullImportDataTableForDisplay.Columns.Count == 0)
                {

                    for (int admCount = 0; admCount < admSummaryFieldDisplayedTypeArray.Length; admCount++)
                    {


                        FCM_DataImportLibrary.FCM_fullImportDataTableForDisplay.Columns.Add(
                            admSummaryFieldDisplayedTypeArray.GetValue(admCount).ToString().Replace('_', ' '));
                    }

                }



                //admInputSummaryGrid.RowCount = admSummaryFieldsList.Count;

                //int col = 0;



                //for (int admRowCounter = 0; admRowCounter < admSummaryFieldsList.Count; admRowCounter++)
                //{
                //    admInputSummaryGrid.Rows[admRowCounter].Cells[(int)ADM_SUMMARY_WEB_POSITION_FIELDS_DISPLAYED.RecordType].Value
                //        = admSummaryFieldsList[admRowCounter].RecordType;


                //    admInputSummaryGrid.Rows[admRowCounter].Cells[(int)ADM_SUMMARY_WEB_POSITION_FIELDS_DISPLAYED.Office].Value
                //        = admSummaryFieldsList[admRowCounter].POFFIC;

                //    admInputSummaryGrid.Rows[admRowCounter].Cells[(int)ADM_SUMMARY_WEB_POSITION_FIELDS_DISPLAYED.Acct].Value
                //        = admSummaryFieldsList[admRowCounter].PACCT;


                //    admInputSummaryGrid.Rows[admRowCounter].Cells[(int)ADM_SUMMARY_WEB_POSITION_FIELDS_DISPLAYED.Description].Value
                //        = admSummaryFieldsList[admRowCounter].Description;

                //    admInputSummaryGrid.Rows[admRowCounter].Cells[(int)ADM_SUMMARY_WEB_POSITION_FIELDS_DISPLAYED.LongQuantity].Value
                //        = (int)admSummaryFieldsList[admRowCounter].LongQuantity;

                //    admInputSummaryGrid.Rows[admRowCounter].Cells[(int)ADM_SUMMARY_WEB_POSITION_FIELDS_DISPLAYED.ShortQuantity].Value
                //        = (int)admSummaryFieldsList[admRowCounter].ShortQuantity;


                //    admInputSummaryGrid.Rows[admRowCounter].Cells[(int)ADM_SUMMARY_WEB_POSITION_FIELDS_DISPLAYED.TradeDate].Value
                //        = admSummaryFieldsList[admRowCounter].TradeDate;

                //    admInputSummaryGrid.Rows[admRowCounter].Cells[(int)ADM_SUMMARY_WEB_POSITION_FIELDS_DISPLAYED.TradePrice].Value
                //        = admSummaryFieldsList[admRowCounter].TradePrice;

                //    admInputSummaryGrid.Rows[admRowCounter].Cells[(int)ADM_SUMMARY_WEB_POSITION_FIELDS_DISPLAYED.WeightedPrice].Value
                //        = admSummaryFieldsList[admRowCounter].WeightedPrice;


                //    admInputSummaryGrid.Rows[admRowCounter].Cells[(int)ADM_SUMMARY_WEB_POSITION_FIELDS_DISPLAYED.SettledPrice].Value
                //        = admSummaryFieldsList[admRowCounter].SettledPrice;


                //    admInputSummaryGrid.Rows[admRowCounter].Cells[(int)ADM_SUMMARY_WEB_POSITION_FIELDS_DISPLAYED.SettledValue].Value
                //        = admSummaryFieldsList[admRowCounter].SettledValue;

                //    admInputSummaryGrid.Rows[admRowCounter].Cells[(int)ADM_SUMMARY_WEB_POSITION_FIELDS_DISPLAYED.Currency].Value
                //        = admSummaryFieldsList[admRowCounter].Currency;

                //    //                     admInputSummaryGrid.Rows[admRowCounter].Cells[(int)ADM_SUMMARY_FIELDS.PSUBTY].Value
                //    //                         = admSummaryFieldsList[admRowCounter].PSUBTY;

                //    //                     admInputSummaryGrid.Rows[admRowCounter].Cells[(int)ADM_SUMMARY_FIELDS.PEXCH].Value
                //    //                         = admSummaryFieldsList[admRowCounter].PEXCH;

                //    admInputSummaryGrid.Rows[admRowCounter].Cells[(int)ADM_SUMMARY_WEB_POSITION_FIELDS_DISPLAYED.PFC].Value
                //        = admSummaryFieldsList[admRowCounter].PFC;

                //    admInputSummaryGrid.Rows[admRowCounter].Cells[(int)ADM_SUMMARY_WEB_POSITION_FIELDS_DISPLAYED.Strike].Value
                //        = admSummaryFieldsList[admRowCounter].aDMStrike;

                //    admInputSummaryGrid.Rows[admRowCounter].Cells[(int)ADM_SUMMARY_WEB_POSITION_FIELDS_DISPLAYED.PCTYM].Value
                //        = admSummaryFieldsList[admRowCounter].PCTYM;

                //    admInputSummaryGrid.Rows[admRowCounter].Cells[(int)ADM_SUMMARY_WEB_POSITION_FIELDS_DISPLAYED.INSTRUMENT_ID].Value
                //        = admSummaryFieldsList[admRowCounter].idinstrument;
                //}

                //                updateSelectedInstrumentFromTreeADM(instrumentSelectedIdx);

                fillFCMData();
            }
#if DEBUG
            catch (Exception ex)
            {
                TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
            }
#endif
        }

        internal void fillFCMData()
        {
            DataTable dataTable = FCM_DataImportLibrary.FCM_fullImportDataTableForDisplay;

            dataTable.Rows.Clear();

            int rowCount = 0;

            foreach (ADMPositionImportWeb fcmPosImport in admSummaryFieldsList)
            {
                if (fcmPosImport.instrument.idinstrument == DataCollectionLibrary.instrumentSelectedInTreeGui
                                || DataCollectionLibrary.instrumentSelectedInTreeGui == TradingSystemConstants.ALL_INSTRUMENTS_SELECTED)
                {
                    //AccountAllocation ac =
                    //    DataCollectionLibrary.portfolioAllocation.accountAllocation_KeyAccountname[ap.name];

                    if (fcmPosImport.acctGroup.visible)
                    {

                        dataTable.Rows.Add();

                        rowCount = dataTable.Rows.Count - 1;

                        dataTable.Rows[rowCount][(int)ADM_SUMMARY_WEB_POSITION_FIELDS_DISPLAYED.RecordType]
                            = fcmPosImport.RecordType;


                        dataTable.Rows[rowCount][(int)ADM_SUMMARY_WEB_POSITION_FIELDS_DISPLAYED.Office]
                            = fcmPosImport.POFFIC;

                        dataTable.Rows[rowCount][(int)ADM_SUMMARY_WEB_POSITION_FIELDS_DISPLAYED.Acct]
                            = fcmPosImport.PACCT;


                        dataTable.Rows[rowCount][(int)ADM_SUMMARY_WEB_POSITION_FIELDS_DISPLAYED.Description]
                            = fcmPosImport.Description;

                        dataTable.Rows[rowCount][(int)ADM_SUMMARY_WEB_POSITION_FIELDS_DISPLAYED.LongQuantity]
                            = (int)fcmPosImport.LongQuantity;

                        dataTable.Rows[rowCount][(int)ADM_SUMMARY_WEB_POSITION_FIELDS_DISPLAYED.ShortQuantity]
                            = (int)fcmPosImport.ShortQuantity;


                        dataTable.Rows[rowCount][(int)ADM_SUMMARY_WEB_POSITION_FIELDS_DISPLAYED.TradeDate]
                            = fcmPosImport.TradeDate;

                        dataTable.Rows[rowCount][(int)ADM_SUMMARY_WEB_POSITION_FIELDS_DISPLAYED.TradePrice]
                            = fcmPosImport.TradePrice;

                        dataTable.Rows[rowCount][(int)ADM_SUMMARY_WEB_POSITION_FIELDS_DISPLAYED.WeightedPrice]
                            = fcmPosImport.WeightedPrice;


                        dataTable.Rows[rowCount][(int)ADM_SUMMARY_WEB_POSITION_FIELDS_DISPLAYED.SettledPrice]
                            = fcmPosImport.SettledPrice;


                        dataTable.Rows[rowCount][(int)ADM_SUMMARY_WEB_POSITION_FIELDS_DISPLAYED.SettledValue]
                            = fcmPosImport.SettledValue;

                        dataTable.Rows[rowCount][(int)ADM_SUMMARY_WEB_POSITION_FIELDS_DISPLAYED.Currency]
                            = fcmPosImport.Currency;

                        //                     dataTable.Rows[admRowCounter][(int)ADM_SUMMARY_FIELDS.PSUBTY]
                        //                         = fcmPosImport.PSUBTY;

                        //                     dataTable.Rows[admRowCounter][(int)ADM_SUMMARY_FIELDS.PEXCH]
                        //                         = fcmPosImport.PEXCH;

                        dataTable.Rows[rowCount][(int)ADM_SUMMARY_WEB_POSITION_FIELDS_DISPLAYED.PFC]
                            = fcmPosImport.PFC;

                        dataTable.Rows[rowCount][(int)ADM_SUMMARY_WEB_POSITION_FIELDS_DISPLAYED.Strike]
                            = fcmPosImport.aDMStrike;

                        dataTable.Rows[rowCount][(int)ADM_SUMMARY_WEB_POSITION_FIELDS_DISPLAYED.PCTYM]
                            = fcmPosImport.PCTYM;

                        dataTable.Rows[rowCount][(int)ADM_SUMMARY_WEB_POSITION_FIELDS_DISPLAYED.INSTRUMENT_ID]
                            = fcmPosImport.idinstrument;
                    }
                }
            }
        }

        private void AdmReportSummaryForm_FormClosing(object sender, FormClosingEventArgs e)
        {
#if DEBUG
            try
#endif
            {
                e.Cancel = true;
                this.Visible = false;
            }
#if DEBUG
            catch (Exception ex)
            {
                TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
            }
#endif
        }

        private void AdmReportSummaryForm_DragEnter(object sender, DragEventArgs e)
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



        


        private void threadSafeDisplayADMInputWithWebPositions(OptionSpreadManager optionSpreadManager,
            int contractSummaryInstrumentSelectedIdx)
        {
            this.Show();

            this.BringToFront();

            setupAdmInputSummaryGrid(FCM_DataImportLibrary.FCM_positionImportNotConsolidated);

        }
    }
}
