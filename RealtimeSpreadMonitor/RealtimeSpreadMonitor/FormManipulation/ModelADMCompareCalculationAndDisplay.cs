using RealtimeSpreadMonitor.Forms;
using RealtimeSpreadMonitor.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RealtimeSpreadMonitor.FormManipulation
{
    internal class ModelADMCompareCalculationAndDisplay
    {
        internal ModelADMCompareCalculationAndDisplay(
            //OptionRealtimeMonitor optionRealtimeMonitor,
            //DataGridView gridViewContractSummary,
            OptionSpreadManager optionSpreadManager)
        {
            //this.optionRealtimeMonitor = optionRealtimeMonitor;
            //this.gridViewContractSummary = gridViewContractSummary;
            this.optionSpreadManager = optionSpreadManager;

        }

        private OptionSpreadManager optionSpreadManager;// { get; set; }


        public void setupGridModelADMComparison(OptionRealtimeMonitor optionRealtimeMonitor)
        {

            DataGridView gridViewModelADMCompare = optionRealtimeMonitor.getGridViewModelADMCompare;

            try
            {

                Type liveColTypes = typeof(ADM_MODEL_POSITION_COMPARE_FIELDS_DISPLAYED);
                Array liveColTypesArray = Enum.GetNames(liveColTypes);

                gridViewModelADMCompare.ColumnCount = liveColTypesArray.Length;

                gridViewModelADMCompare.EnableHeadersVisualStyles = false;

                DataGridViewCellStyle colTotalPortStyle = gridViewModelADMCompare.ColumnHeadersDefaultCellStyle;
                colTotalPortStyle.BackColor = Color.Black;
                colTotalPortStyle.ForeColor = Color.White;

                DataGridViewCellStyle rowTotalPortStyle = gridViewModelADMCompare.RowHeadersDefaultCellStyle;
                rowTotalPortStyle.BackColor = Color.Black;
                rowTotalPortStyle.ForeColor = Color.White;

                gridViewModelADMCompare.Columns[0].Frozen = true;

                DataGridViewCheckBoxColumn zeroPriceCol = new DataGridViewCheckBoxColumn();
                {
                    zeroPriceCol.HeaderText = liveColTypesArray.GetValue(
                        (int)ADM_MODEL_POSITION_COMPARE_FIELDS_DISPLAYED.ZERO_PRICE).ToString().Replace('_', ' ');
                    //column.Name = ColumnName.OutOfOffice.ToString();
                    zeroPriceCol.AutoSizeMode =
                        DataGridViewAutoSizeColumnMode.DisplayedCells;
                    zeroPriceCol.FlatStyle = FlatStyle.Standard;
                    //column.ThreeState = true;
                    zeroPriceCol.CellTemplate = new DataGridViewCheckBoxCell();
                    zeroPriceCol.CellTemplate.Style.BackColor = Color.LightBlue;
                    zeroPriceCol.ReadOnly = false;
                    //column.e = false;
                }

                DataGridViewCheckBoxColumn exceptCol = new DataGridViewCheckBoxColumn();
                {
                    exceptCol.HeaderText = liveColTypesArray.GetValue(
                        (int)ADM_MODEL_POSITION_COMPARE_FIELDS_DISPLAYED.EXCEPTIONS).ToString().Replace('_', ' ');
                    //column.Name = ColumnName.OutOfOffice.ToString();
                    exceptCol.AutoSizeMode =
                        DataGridViewAutoSizeColumnMode.DisplayedCells;
                    exceptCol.FlatStyle = FlatStyle.Standard;
                    //column.ThreeState = true;
                    exceptCol.CellTemplate = new DataGridViewCheckBoxCell();
                    exceptCol.CellTemplate.Style.BackColor = Color.Cyan;
                    exceptCol.ReadOnly = false;
                    //column.e = false;
                }



                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < gridViewModelADMCompare.ColumnCount; i++)
                {
                    gridViewModelADMCompare.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;

                    //if (i != (int)OPTION_LIVE_ADM_DATA_COLUMNS.NET && i != (int)OPTION_LIVE_ADM_DATA_COLUMNS.AVERAGE_PRC)
                    //{
                    //    gridViewModelADMCompare.Columns[i].ReadOnly = true;
                    //}

                    sb.Clear();

                    sb.Append(liveColTypesArray.GetValue(i).ToString());

                    gridViewModelADMCompare
                        .Columns[i]
                        .HeaderCell.Value = sb.ToString().Replace('_', ' ');

                    gridViewModelADMCompare.Columns[i].Width = 50;

                    if (i == (int)ADM_MODEL_POSITION_COMPARE_FIELDS_DISPLAYED.ZERO_PRICE)
                    {
                        gridViewModelADMCompare.Columns.RemoveAt(i);
                        gridViewModelADMCompare.Columns.Insert(i, zeroPriceCol);
                    }
                    else if (i == (int)ADM_MODEL_POSITION_COMPARE_FIELDS_DISPLAYED.EXCEPTIONS)
                    {
                        gridViewModelADMCompare.Columns.RemoveAt(i);
                        gridViewModelADMCompare.Columns.Insert(i, exceptCol);
                    }
                    else
                    {
                        gridViewModelADMCompare.Columns[i].ReadOnly = true;
                    }
                }

                gridViewModelADMCompare.Columns[(int)ADM_MODEL_POSITION_COMPARE_FIELDS_DISPLAYED.REBALANCE].Width = 75;




            }
            catch (Exception ex)
            {
                TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
            }
        }

        //private void updateModelADMCompareViewable()
        //{
        //    if (optionSpreadManager.contractSummaryInstrumentSelectedIdx == instruments.Length)
        //    {
        //        for (int modelADMCompareCntRow = 0;
        //                        modelADMCompareCntRow < gridViewModelADMCompare.RowCount; modelADMCompareCntRow++)
        //        {
        //            hideUnhideSummaryData(gridViewModelADMCompare, modelADMCompareCntRow, true);
        //        }
        //    }
        //    else
        //    {
        //        for (int modelADMCompareCntRow = 0;
        //                modelADMCompareCntRow < gridViewModelADMCompare.RowCount; modelADMCompareCntRow++)
        //        {
        //            int instrumentId = Convert.ToInt16(gridViewModelADMCompare.Rows[modelADMCompareCntRow].Cells[(int)ADM_MODEL_POSITION_COMPARE_FIELDS_DISPLAYED.INSTRUMENT_ID].Value);

        //            if (optionSpreadManager.contractSummaryInstrumentSelectedIdx == instrumentId)
        //            {
        //                hideUnhideSummaryData(gridViewModelADMCompare, modelADMCompareCntRow, true);
        //            }
        //            else
        //            {
        //                hideUnhideSummaryData(gridViewModelADMCompare, modelADMCompareCntRow, false);
        //            }
        //        }
        //    }
        //}


        delegate void ThreadSafeFillGridModelADMComparison(OptionRealtimeMonitor optionRealtimeMonitor);

        public void fillGridModelADMComparison(OptionRealtimeMonitor optionRealtimeMonitor)
        {

            if (optionRealtimeMonitor.InvokeRequired)
            {

                ThreadSafeFillGridModelADMComparison d = new ThreadSafeFillGridModelADMComparison(threadSafeFillGridModelADMComparison);

                optionRealtimeMonitor.Invoke(d, optionRealtimeMonitor);
            }
            else
            {
                threadSafeFillGridModelADMComparison(optionRealtimeMonitor);
            }

        }

        //private string makeDictionaryKeyOfFCMPositionCompare(int acctGroup, string cqgSymbol)
        //{
        //    StringBuilder dictionaryKey = new StringBuilder();
        //    dictionaryKey.Append(acctGroup);
        //    dictionaryKey.Append(":");
        //    dictionaryKey.Append(cqgSymbol);

        //    return dictionaryKey.ToString();
        //}

        public void threadSafeFillGridModelADMComparison(OptionRealtimeMonitor optionRealtimeMonitor)
        {

            DataGridView gridViewModelADMCompare = optionRealtimeMonitor.getGridViewModelADMCompare;

            try
            {
                //List<LiveADMStrategyInfo> liveADMStrategyInfoList = optionSpreadManager.liveADMStrategyInfoList;

                FCM_DataImportLibrary.FCM_PostionList_forCompare.Clear();// = new List<ADMPositionImportWeb>();

                Dictionary<Tuple<int,string>, ADMPositionImportWeb> modelContractAcctGrpFCMCompareDictionary 
                    = new Dictionary<Tuple<int, string>, ADMPositionImportWeb>();


                //****************
                for (int webPos = 0; webPos < FCM_DataImportLibrary.FCM_Import_Consolidated.Count; webPos++)
                {
                    ADMPositionImportWeb aDMPositionImportWeb = new ADMPositionImportWeb();

                    FCM_DataImportLibrary.FCM_PostionList_forCompare.Add(aDMPositionImportWeb);

                    optionSpreadManager.aDMDataCommonMethods.copyADMPositionImportWeb(
                        aDMPositionImportWeb, FCM_DataImportLibrary.FCM_Import_Consolidated[webPos]);

                    //string key = makeDictionaryKeyOfFCMPositionCompare(aDMPositionImportWeb.acctGroup,
                    //    aDMPositionImportWeb.cqgsymbol);

                    var key = Tuple.Create(aDMPositionImportWeb.acctGroup.acctIndex_UsedForTotals_Visibility,
                        aDMPositionImportWeb.asset.cqgsymbol);

                    if (!modelContractAcctGrpFCMCompareDictionary.ContainsKey(key))
                    {
                        modelContractAcctGrpFCMCompareDictionary.Add(key, aDMPositionImportWeb);
                    }

                    //StringBuilder pOfficPAcct = new StringBuilder();
                    //pOfficPAcct.Append(aDMPositionImportWeb.POFFIC);
                    //pOfficPAcct.Append(aDMPositionImportWeb.PACCT);

                    //if (optionSpreadManager.DataCollectionLibrary.portfolioAllocation.portfolioGroupIdxAcct_keyAcctStringHashSet.ContainsKey(pOfficPAcct.ToString()))
                    //{
                    //    aDMPositionImportWeb.acctGroup = optionSpreadManager.DataCollectionLibrary.portfolioAllocation.portfolioGroupIdxAcct_keyAcctStringHashSet[pOfficPAcct.ToString()];
                    //    aDMPositionImportWeb.MODEL_OFFICE_ACCT = DataCollectionLibrary.portfolioAllocation.accountAllocation[aDMPositionImportWeb.acctGroup].FCM_POFFIC_PACCT;

                    //}
                }
                //****************

                //change to hashset of tuple
                HashSet<string> modelContractsAlreadyExamined = new HashSet<string>();


                foreach (AccountPosition ap in DataCollectionLibrary.accountPositionsList)
                {
                    //var key = Tuple.Create(aDMPositionImportWeb.POFFIC, aDMPositionImportWeb.PACCT);

                    AccountAllocation aa = DataCollectionLibrary.portfolioAllocation.accountAllocation_KeyAccountname[ap.name];

                    foreach (Position p in ap.positions)
                    {
                        //ADMPositionImportWeb aDMPositionImportWeb = optionSpreadManager.admPositionImportWebListForCompare[webPos];

                        //optionSpreadManager.admPositionImportWebListForCompare.Add(aDMPositionImportWeb);

                        //optionSpreadManager.aDMDataCommonMethods.copyADMPositionImportWeb(
                        //    aDMPositionImportWeb, optionSpreadManager.admPositionImportWeb[webPos]);

                        //foreach (ADMPositionImportWeb aDMPositionImportWeb in optionSpreadManager.admPositionImportWebListForCompare)
                        bool found = false;
                        int acctPosCount = 0;
                        while (acctPosCount < FCM_DataImportLibrary.FCM_PostionList_forCompare.Count
                            && !found)
                        {
                            ADMPositionImportWeb fcmPositionForCompare
                                = FCM_DataImportLibrary.FCM_PostionList_forCompare[acctPosCount];

                            if(p.asset.cqgsymbol.CompareTo(fcmPositionForCompare.asset.cqgsymbol) == 0
                                && fcmPositionForCompare.POFFIC.CompareTo(aa.FCM_OFFICE) == 0
                                && fcmPositionForCompare.PACCT.CompareTo(aa.FCM_ACCT) == 0)
                            {
                                fcmPositionForCompare.POFFIC = aa.FCM_OFFICE;

                                fcmPositionForCompare.PACCT = aa.FCM_ACCT;

                                fcmPositionForCompare.MODEL_OFFICE_ACCT = aa.FCM_POFFIC_PACCT;

                                fcmPositionForCompare.modelLots = p.prev_qty;

                                fcmPositionForCompare.orderLots = p.qty - p.prev_qty;

                                fcmPositionForCompare.acctGroup = aa;

                                found = true;

                                break;
                            }

                            acctPosCount++;
                        }

                        if(!found)
                        {
                            ADMPositionImportWeb fcmPositionForCompare = new ADMPositionImportWeb();

                            FCM_DataImportLibrary.FCM_PostionList_forCompare.Add(fcmPositionForCompare);

                            fcmPositionForCompare.POFFIC = aa.FCM_OFFICE;

                            fcmPositionForCompare.PACCT = aa.FCM_ACCT;

                            fcmPositionForCompare.MODEL_OFFICE_ACCT = aa.FCM_POFFIC_PACCT;

                            fcmPositionForCompare.modelLots = p.prev_qty;

                            fcmPositionForCompare.orderLots = p.qty - p.prev_qty;

                            fcmPositionForCompare.acctGroup = aa;

                            fcmPositionForCompare.asset = p.asset;

                            fcmPositionForCompare.cqgsymbol =
                                p.asset.cqgsymbol;

                            fcmPositionForCompare.optionSpreadExpression = p.mose;


                            Instrument_mongo im = DataCollectionLibrary.instrumentHashTable_keyinstrumentid[p.asset.idinstrument];

                            fcmPositionForCompare.strike =
                                //optionSpreadManager.optionSpreadExpressionList[
                                //    optionSpreadManager.contractSummaryExpressionListIdx[modelContractsCnt]]
                                //        .strikePrice.ToString();
                                ConversionAndFormatting.convertToTickMovesString(
                                   p.asset.strikeprice,
                                        im.optionstrikeincrement,
                                        im.optionstrikedisplay);

                            //NOV 11 2014 FIXED FORMAT OF STRIKE

                            //aDMPositionImportWeb.strikeInDecimal = optionSpreadManager.optionSpreadExpressionList[
                            //            optionSpreadManager.contractSummaryExpressionListIdx[modelContractsCnt]]
                            //            .strikePrice;


                            fcmPositionForCompare.idinstrument = p.asset.idinstrument;

                        }
                    }

                }


                ////for (int portfolioGroupCnt = 0; portfolioGroupCnt < DataCollectionLibrary.portfolioAllocation.accountAllocation.Count; portfolioGroupCnt++)
                //foreach(AccountAllocation ac in DataCollectionLibrary.portfolioAllocation.accountAllocation)
                //{
                //    for (int modelContractsCnt = 0; modelContractsCnt < optionSpreadManager.contractSummaryExpressionListIdx.Count; modelContractsCnt++)
                //    {

                //        StringBuilder modelContractAcctGroupIdentifier = new StringBuilder();
                //        modelContractAcctGroupIdentifier.Append(ac.acctIndex_UsedForTotals_Visibility);
                //        modelContractAcctGroupIdentifier.Append(":");
                //        modelContractAcctGroupIdentifier.Append(optionSpreadManager.contractSummaryExpressionListIdx[modelContractsCnt]);

                //        if (!modelContractsAlreadyExamined.Contains(modelContractAcctGroupIdentifier.ToString()))
                //        {
                //            ADMPositionImportWeb aDMPositionImportWeb = new ADMPositionImportWeb();

                //            optionSpreadManager.admPositionImportWebListForCompare.Add(aDMPositionImportWeb);

                            

                //            modelContractsAlreadyExamined.Add(modelContractAcctGroupIdentifier.ToString());

                //            aDMPositionImportWeb.acctGroup = ac;

                //            aDMPositionImportWeb.MODEL_OFFICE_ACCT = ac.FCM_POFFIC_PACCT;

                //            aDMPositionImportWeb.cqgsymbol =
                //                DataCollectionLibrary.optionSpreadExpressionList[
                //                optionSpreadManager.contractSummaryExpressionListIdx[modelContractsCnt]].asset.cqgsymbol;

                //            aDMPositionImportWeb.modelLots =
                //                DataCollectionLibrary.optionSpreadExpressionList[
                //                    optionSpreadManager.contractSummaryExpressionListIdx[modelContractsCnt]]
                //                        .numberOfLotsHeldForContractSummary
                //                         * ac.multiple;

                //            aDMPositionImportWeb.optionSpreadExpression =
                //                        DataCollectionLibrary.optionSpreadExpressionList[
                //                            optionSpreadManager.contractSummaryExpressionListIdx[modelContractsCnt]];

                //            //aDMPositionImportWeb.rebalanceLots =
                //            //    aDMPositionImportWeb.modelLots - aDMPositionImportWeb.Net;

                //            aDMPositionImportWeb.strike =
                //                //optionSpreadManager.optionSpreadExpressionList[
                //                //    optionSpreadManager.contractSummaryExpressionListIdx[modelContractsCnt]]
                //                //        .strikePrice.ToString();
                //                ConversionAndFormatting.convertToTickMovesString(
                //                    DataCollectionLibrary.optionSpreadExpressionList[
                //                        optionSpreadManager.contractSummaryExpressionListIdx[modelContractsCnt]]
                //                        .strikePrice,
                //                        DataCollectionLibrary.optionSpreadExpressionList[
                //                        optionSpreadManager.contractSummaryExpressionListIdx[modelContractsCnt]].instrument.optionstrikeincrement,
                //                        DataCollectionLibrary.optionSpreadExpressionList[
                //                        optionSpreadManager.contractSummaryExpressionListIdx[modelContractsCnt]].instrument.optionstrikedisplay);

                //            //NOV 11 2014 FIXED FORMAT OF STRIKE

                //            //aDMPositionImportWeb.strikeInDecimal = optionSpreadManager.optionSpreadExpressionList[
                //            //            optionSpreadManager.contractSummaryExpressionListIdx[modelContractsCnt]]
                //            //            .strikePrice;


                //            aDMPositionImportWeb.idinstrument =
                //                DataCollectionLibrary.optionSpreadExpressionList[
                //                    optionSpreadManager.contractSummaryExpressionListIdx[modelContractsCnt]].instrument.idinstrument;


                //            //string key = makeDictionaryKeyOfFCMPositionCompare(aDMPositionImportWeb.acctGroup,
                //            //    aDMPositionImportWeb.cqgsymbol);

                //            var key = Tuple.Create(aDMPositionImportWeb.acctGroup.acctIndex_UsedForTotals_Visibility,
                //                aDMPositionImportWeb.cqgsymbol);

                //            if (!modelContractAcctGrpFCMCompareDictionary.ContainsKey(key))
                //            {
                //                modelContractAcctGrpFCMCompareDictionary.Add(key, aDMPositionImportWeb);
                //            }

                //        }

                //        //modelContractsCnt++;
                //    }
                //}


                ////modelContractsAlreadyExamined.Clear();

                ////for (int portfolioGroupCnt = 0; portfolioGroupCnt < DataCollectionLibrary.portfolioAllocation.accountAllocation.Count; portfolioGroupCnt++)
                //foreach(AccountAllocation ac in DataCollectionLibrary.portfolioAllocation.accountAllocation)
                //{

                //    for (int cntExpressionList = 0; cntExpressionList < DataCollectionLibrary.optionSpreadExpressionList.Count; cntExpressionList++)
                //    {


                //        if (DataCollectionLibrary.optionSpreadExpressionList[cntExpressionList].numberOfOrderContracts != 0)
                //        {
                //            //string key = makeDictionaryKeyOfFCMPositionCompare(portfolioGroupCnt,
                //            //    DataCollectionLibrary.optionSpreadExpressionList[cntExpressionList].cqgsymbol);

                //            var key = Tuple.Create(ac.acctIndex_UsedForTotals_Visibility,
                //                DataCollectionLibrary.optionSpreadExpressionList[cntExpressionList].asset.cqgsymbol);

                //            bool foundContract = false;

                //            if (modelContractAcctGrpFCMCompareDictionary.ContainsKey(key))
                //            {
                //                ADMPositionImportWeb admPIW = modelContractAcctGrpFCMCompareDictionary[key];





                //                admPIW.orderLots =
                //                            DataCollectionLibrary.optionSpreadExpressionList[cntExpressionList].numberOfOrderContracts
                //                             * ac.multiple;


                //                foundContract = true;
                //            }

                            

                //            if (!foundContract)
                //            {
                //                ADMPositionImportWeb aDMPositionImportWeb = new ADMPositionImportWeb();

                //                optionSpreadManager.admPositionImportWebListForCompare.Add(aDMPositionImportWeb);

                //                if (!modelContractAcctGrpFCMCompareDictionary.ContainsKey(key))
                //                {
                //                    modelContractAcctGrpFCMCompareDictionary.Add(key, aDMPositionImportWeb);
                //                }


                //                aDMPositionImportWeb.acctGroup = ac;

                //                aDMPositionImportWeb.MODEL_OFFICE_ACCT = ac.FCM_POFFIC_PACCT;

                //                aDMPositionImportWeb.cqgsymbol =
                //                    DataCollectionLibrary.optionSpreadExpressionList[cntExpressionList].asset.cqgsymbol;

                //                aDMPositionImportWeb.orderLots =
                //                    DataCollectionLibrary.optionSpreadExpressionList[cntExpressionList].numberOfOrderContracts
                //                     * ac.multiple;

                //                aDMPositionImportWeb.optionSpreadExpression =
                //                        DataCollectionLibrary.optionSpreadExpressionList[cntExpressionList];

                //                //aDMPositionImportWeb.rebalanceLots =
                //                //    aDMPositionImportWeb.modelLots - aDMPositionImportWeb.Net;

                //                aDMPositionImportWeb.strike =
                //                    //optionSpreadManager.optionSpreadExpressionList[cntExpressionList].strikePrice.ToString();
                //                    ConversionAndFormatting.convertToTickMovesString(
                //                        DataCollectionLibrary.optionSpreadExpressionList[cntExpressionList].strikePrice,
                //                            DataCollectionLibrary.optionSpreadExpressionList[cntExpressionList].instrument.optionstrikeincrement,
                //                            DataCollectionLibrary.optionSpreadExpressionList[cntExpressionList].instrument.optionstrikedisplay);
                //                //NOV 11 2014 CHG FORMAT STRIKE

                //                //aDMPositionImportWeb.strikeInDecimal = optionSpreadManager.optionSpreadExpressionList[cntExpressionList].strikePrice;

                //                //aDMPositionImportWeb.contractYear = optionSpreadManager.optionSpreadExpressionList[cntExpressionList].futureContractYear;
                //                //aDMPositionImportWeb.contractMonth = aDMPositionImportWeb.contractInfo.contractMonthInt;
                //                //aDMPositionImportWeb.optionYear = aDMPositionImportWeb.contractInfo.optionYear;
                //                //aDMPositionImportWeb.optionMonth = aDMPositionImportWeb.contractInfo.optionMonthInt;

                //                aDMPositionImportWeb.idinstrument =
                //                    DataCollectionLibrary.optionSpreadExpressionList[cntExpressionList].instrument.idinstrument;

                                

                                
                //            }
                //        }
                //    }
                //}


                modelContractsAlreadyExamined.Clear();
                


                gridViewModelADMCompare.RowCount = FCM_DataImportLibrary.FCM_PostionList_forCompare.Count;

                if (optionSpreadManager.includeExcludeOrdersInModelADMCompare)
                {
                    gridViewModelADMCompare.Columns[(int)ADM_MODEL_POSITION_COMPARE_FIELDS_DISPLAYED.NEW_ORDERS].Visible = true;
                    gridViewModelADMCompare.Columns[(int)ADM_MODEL_POSITION_COMPARE_FIELDS_DISPLAYED.ROLL_INTO_ORDERS].Visible = true;
                }
                else
                {
                    gridViewModelADMCompare.Columns[(int)ADM_MODEL_POSITION_COMPARE_FIELDS_DISPLAYED.NEW_ORDERS].Visible = false;
                    gridViewModelADMCompare.Columns[(int)ADM_MODEL_POSITION_COMPARE_FIELDS_DISPLAYED.ROLL_INTO_ORDERS].Visible = false;
                }

                //Color rowColor1 = Color.DarkGray;
                //Color rowColor2 = Color.DarkBlue;

                //Color currentRowColor = rowColor1;

                Color nonAccentCell = Color.LightGray;
                Color accentCell = Color.White;
                Color rebalanceAccentCell = Color.Yellow;


                //HashSet<int> alreadUsedExcluded = new HashSet<int>();

                for (int admRowCounter = 0; admRowCounter < FCM_DataImportLibrary.FCM_PostionList_forCompare.Count; admRowCounter++)
                {

                    //this calculates the pl for each contract on the compare page
                    //JAN 30 2015

                    //if (optionSpreadManager.admPositionImportWebListForCompare[admRowCounter].optionSpreadExpression != null)
                    //{

                    //    optionSpreadManager.admPositionImportWebListForCompare[admRowCounter].modelPL =
                    //        optionSpreadManager.admPositionImportWebListForCompare[admRowCounter].optionSpreadExpression.plChgForContractSummary
                    //        +
                    //        optionSpreadManager.admPositionImportWebListForCompare[admRowCounter].optionSpreadExpression.plChgOrders;


                    //}

                    FCM_DataImportLibrary.FCM_PostionList_forCompare[admRowCounter].modelPL = 0;

                    FCM_DataImportLibrary.FCM_PostionList_forCompare[admRowCounter].fcmPL =
                            FCM_DataImportLibrary.FCM_PostionList_forCompare[admRowCounter].contractData.pAndLDay
                            +
                            FCM_DataImportLibrary.FCM_PostionList_forCompare[admRowCounter].contractData.pAndLDayOrders;




                    int netContracts = (int)FCM_DataImportLibrary.FCM_PostionList_forCompare[admRowCounter].Net
                        + FCM_DataImportLibrary.FCM_PostionList_forCompare[admRowCounter].transNetLong
                        - FCM_DataImportLibrary.FCM_PostionList_forCompare[admRowCounter].transNetShort;


                    if (optionSpreadManager.includeExcludeOrdersInModelADMCompare)
                    {
                        FCM_DataImportLibrary.FCM_PostionList_forCompare[admRowCounter].rebalanceLots =
                            FCM_DataImportLibrary.FCM_PostionList_forCompare[admRowCounter].modelLots
                            + FCM_DataImportLibrary.FCM_PostionList_forCompare[admRowCounter].orderLots
                            + FCM_DataImportLibrary.FCM_PostionList_forCompare[admRowCounter].rollIntoLots
                            - netContracts;
                    }
                    else
                    {
                        FCM_DataImportLibrary.FCM_PostionList_forCompare[admRowCounter].rebalanceLots =
                            FCM_DataImportLibrary.FCM_PostionList_forCompare[admRowCounter].modelLots
                            - netContracts;
                    }

                    FCM_DataImportLibrary.FCM_PostionList_forCompare[admRowCounter].rebalanceLotsForPayoffWithOrders =
                            FCM_DataImportLibrary.FCM_PostionList_forCompare[admRowCounter].modelLots
                            + FCM_DataImportLibrary.FCM_PostionList_forCompare[admRowCounter].orderLots
                            + FCM_DataImportLibrary.FCM_PostionList_forCompare[admRowCounter].rollIntoLots
                            - netContracts;

                    FCM_DataImportLibrary.FCM_PostionList_forCompare[admRowCounter].rebalanceLotsForPayoffNoOrders =
                            FCM_DataImportLibrary.FCM_PostionList_forCompare[admRowCounter].modelLots
                            - netContracts;





                    if (gridViewModelADMCompare.Rows[admRowCounter].HeaderCell.Value == null
                        ||
                        gridViewModelADMCompare.Rows[admRowCounter].HeaderCell.Value.ToString().CompareTo(
                            FCM_DataImportLibrary.FCM_PostionList_forCompare[admRowCounter].asset.cqgsymbol) != 0
                        )
                    {
                        gridViewModelADMCompare.Rows[admRowCounter]
                            .HeaderCell.Value =
                            FCM_DataImportLibrary.FCM_PostionList_forCompare[admRowCounter].asset.cqgsymbol;

                        //gridViewModelADMCompare.Rows[admRowCounter]
                        //        .HeaderCell.Style.BackColor = currentRowColor;
                    }

                    

                    fillGridViewADMCompareFields(optionRealtimeMonitor, admRowCounter, (int)ADM_MODEL_POSITION_COMPARE_FIELDS_DISPLAYED.MODEL_OFFICE_ACCT,
                                FCM_DataImportLibrary.FCM_PostionList_forCompare[admRowCounter].MODEL_OFFICE_ACCT,
                                Color.LawnGreen);

                    fillGridViewADMCompareFields(optionRealtimeMonitor, admRowCounter, (int)ADM_MODEL_POSITION_COMPARE_FIELDS_DISPLAYED.FCM_OFFICE_ACCT,
                                FCM_DataImportLibrary.FCM_PostionList_forCompare[admRowCounter].POFFIC +
                                FCM_DataImportLibrary.FCM_PostionList_forCompare[admRowCounter].PACCT,
                                Color.LawnGreen);

                    fillGridViewADMCompareFields(optionRealtimeMonitor, admRowCounter, (int)ADM_MODEL_POSITION_COMPARE_FIELDS_DISPLAYED.MODEL_PL,
                                Math.Round(FCM_DataImportLibrary.FCM_PostionList_forCompare[admRowCounter].modelPL).ToString(),
                                FCM_DataImportLibrary.FCM_PostionList_forCompare[admRowCounter].modelPL >= 0 ? Color.LawnGreen :
                                Color.Red);

                    fillGridViewADMCompareFields(optionRealtimeMonitor, admRowCounter, (int)ADM_MODEL_POSITION_COMPARE_FIELDS_DISPLAYED.FCM_PL,
                                Math.Round(FCM_DataImportLibrary.FCM_PostionList_forCompare[admRowCounter].fcmPL).ToString(),
                                FCM_DataImportLibrary.FCM_PostionList_forCompare[admRowCounter].fcmPL >= 0 ? Color.LawnGreen :
                                Color.Red);

                    double diff = FCM_DataImportLibrary.FCM_PostionList_forCompare[admRowCounter].fcmPL
                        - FCM_DataImportLibrary.FCM_PostionList_forCompare[admRowCounter].modelPL;

                    fillGridViewADMCompareFields(optionRealtimeMonitor, admRowCounter, (int)ADM_MODEL_POSITION_COMPARE_FIELDS_DISPLAYED.DIFF_PL,
                                Math.Round(diff).ToString(),
                                diff >= 0 ? Color.LawnGreen :
                                Color.Red);

                    if (FCM_DataImportLibrary.FCM_PostionList_forCompare[admRowCounter].modelLots != 0)
                    {
                        fillGridViewADMCompareFields(optionRealtimeMonitor, admRowCounter, (int)ADM_MODEL_POSITION_COMPARE_FIELDS_DISPLAYED.MODEL,
                                ((int)FCM_DataImportLibrary.FCM_PostionList_forCompare[admRowCounter].modelLots).ToString(), accentCell);
                    }
                    else
                    {
                        fillGridViewADMCompareFields(optionRealtimeMonitor, admRowCounter, (int)ADM_MODEL_POSITION_COMPARE_FIELDS_DISPLAYED.MODEL,
                                ((int)FCM_DataImportLibrary.FCM_PostionList_forCompare[admRowCounter].modelLots).ToString(), nonAccentCell);
                    }

                    if (FCM_DataImportLibrary.FCM_PostionList_forCompare[admRowCounter].orderLots != 0)
                    {
                        fillGridViewADMCompareFields(optionRealtimeMonitor, admRowCounter, (int)ADM_MODEL_POSITION_COMPARE_FIELDS_DISPLAYED.NEW_ORDERS,
                                ((int)FCM_DataImportLibrary.FCM_PostionList_forCompare[admRowCounter].orderLots).ToString(), accentCell);
                    }
                    else
                    {
                        fillGridViewADMCompareFields(optionRealtimeMonitor, admRowCounter, (int)ADM_MODEL_POSITION_COMPARE_FIELDS_DISPLAYED.NEW_ORDERS,
                                ((int)FCM_DataImportLibrary.FCM_PostionList_forCompare[admRowCounter].orderLots).ToString(), nonAccentCell);
                    }


                    if (FCM_DataImportLibrary.FCM_PostionList_forCompare[admRowCounter].rollIntoLots != 0)
                    {
                        fillGridViewADMCompareFields(optionRealtimeMonitor, admRowCounter, (int)ADM_MODEL_POSITION_COMPARE_FIELDS_DISPLAYED.ROLL_INTO_ORDERS,
                                ((int)FCM_DataImportLibrary.FCM_PostionList_forCompare[admRowCounter].rollIntoLots).ToString(), accentCell);
                    }
                    else
                    {
                        fillGridViewADMCompareFields(optionRealtimeMonitor, admRowCounter, (int)ADM_MODEL_POSITION_COMPARE_FIELDS_DISPLAYED.ROLL_INTO_ORDERS,
                                ((int)FCM_DataImportLibrary.FCM_PostionList_forCompare[admRowCounter].rollIntoLots).ToString(), nonAccentCell);
                    }


                    if (netContracts != 0)
                    {
                        fillGridViewADMCompareFields(optionRealtimeMonitor, admRowCounter, (int)ADM_MODEL_POSITION_COMPARE_FIELDS_DISPLAYED.FCM,
                                netContracts.ToString(), accentCell);
                    }
                    else
                    {
                        fillGridViewADMCompareFields(optionRealtimeMonitor, admRowCounter, (int)ADM_MODEL_POSITION_COMPARE_FIELDS_DISPLAYED.FCM,
                                netContracts.ToString(), nonAccentCell);
                    }

                    if (FCM_DataImportLibrary.FCM_PostionList_forCompare[admRowCounter].rebalanceLots != 0)
                    {
                        fillGridViewADMCompareFields(optionRealtimeMonitor, admRowCounter, (int)ADM_MODEL_POSITION_COMPARE_FIELDS_DISPLAYED.REBALANCE,
                            ((int)FCM_DataImportLibrary.FCM_PostionList_forCompare[admRowCounter].rebalanceLots).ToString(), rebalanceAccentCell);
                    }
                    else
                    {
                        fillGridViewADMCompareFields(optionRealtimeMonitor, admRowCounter, (int)ADM_MODEL_POSITION_COMPARE_FIELDS_DISPLAYED.REBALANCE,
                            ((int)FCM_DataImportLibrary.FCM_PostionList_forCompare[admRowCounter].rebalanceLots).ToString(), nonAccentCell);
                    }


                    fillGridViewADMCompareFields(optionRealtimeMonitor, admRowCounter, (int)ADM_MODEL_POSITION_COMPARE_FIELDS_DISPLAYED.Strike,
                        FCM_DataImportLibrary.FCM_PostionList_forCompare[admRowCounter].strike, accentCell);


                    fillGridViewADMCompareFields(optionRealtimeMonitor, admRowCounter, (int)ADM_MODEL_POSITION_COMPARE_FIELDS_DISPLAYED.INSTRUMENT_ID,
                        FCM_DataImportLibrary.FCM_PostionList_forCompare[admRowCounter].idinstrument.ToString(), accentCell);

                    fillGridViewADMCompareFields(optionRealtimeMonitor, admRowCounter, (int)ADM_MODEL_POSITION_COMPARE_FIELDS_DISPLAYED.ACCT_GROUP_IDX,
                                FCM_DataImportLibrary.FCM_PostionList_forCompare[admRowCounter].acctGroup.ToString(),
                                Color.White);

                    setBackgroundZeroPrice_ModelADMCompare(optionRealtimeMonitor, admRowCounter, (int)ADM_MODEL_POSITION_COMPARE_FIELDS_DISPLAYED.ZERO_PRICE);
                }

                optionRealtimeMonitor.updateModelADMCompareViewable();

            }
            catch (Exception ex)
            {
                TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
            }

            //return liveLegRowIdexes;
        }


        delegate void ThreadSafeFillGridViewValueAsStringAndColorDelegate(
            OptionRealtimeMonitor optionRealtimeMonitor, int rowToUpdate, int col, string displayValue, Color color);

        public void fillGridViewADMCompareFields(
            OptionRealtimeMonitor optionRealtimeMonitor, int rowToUpdate, int col, string displayValue, Color color)
        {
            try
            {
                if (optionRealtimeMonitor.InvokeRequired)
                {
                    ThreadSafeFillGridViewValueAsStringAndColorDelegate d = new ThreadSafeFillGridViewValueAsStringAndColorDelegate(
                        threadSafefillGridViewADMCompareFields);

                    optionRealtimeMonitor.Invoke(d, optionRealtimeMonitor, rowToUpdate, col, displayValue, color);
                }
                else
                {
                    threadSafefillGridViewADMCompareFields(optionRealtimeMonitor, rowToUpdate, col, displayValue, color);
                }
            }
            catch (Exception ex)
            {
                TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
            }
        }

        private void threadSafefillGridViewADMCompareFields(
            OptionRealtimeMonitor optionRealtimeMonitor, int rowToUpdate, int col, string displayValue, Color color)
        {
            DataGridView gridViewModelADMCompare = optionRealtimeMonitor.getGridViewModelADMCompare;

            if (gridViewModelADMCompare.Rows[rowToUpdate].Cells[col].Value == null
                        ||
                        gridViewModelADMCompare.Rows[rowToUpdate].Cells[col].Value.ToString().CompareTo(displayValue) != 0
                        )
            {
                gridViewModelADMCompare.Rows[rowToUpdate].Cells[col].Value = displayValue;

                gridViewModelADMCompare.Rows[rowToUpdate].Cells[col].Style.BackColor = color;

            }
        }

        internal void setBackgroundZeroPrice_ModelADMCompare(OptionRealtimeMonitor optionRealtimeMonitor, int row, int col)
        {
            DataGridView gridViewModelADMCompare = optionRealtimeMonitor.getGridViewModelADMCompare;

            if (optionSpreadManager.zeroPriceContractList.Contains(
                        FCM_DataImportLibrary.FCM_PostionList_forCompare[row].asset.cqgsymbol))
            {
                fillGridViewADMCompareFields(optionRealtimeMonitor, row, (int)ADM_MODEL_POSITION_COMPARE_FIELDS_DISPLAYED.ZERO_PRICE,
                        "true", Color.Red);

                fillGridViewADMCompareFields(optionRealtimeMonitor, row, (int)ADM_MODEL_POSITION_COMPARE_FIELDS_DISPLAYED.EXCEPTIONS,
                        "false", Color.Cyan);

                optionSpreadManager.statusAndConnectedUpdates.markLiveAsConnected(gridViewModelADMCompare, row,
                    (int)ADM_MODEL_POSITION_COMPARE_FIELDS_DISPLAYED.REBALANCE,
                    false, Color.Red);
            }
            else if (optionSpreadManager.exceptionContractList.Contains(
                        FCM_DataImportLibrary.FCM_PostionList_forCompare[row].asset.cqgsymbol))
            {
                fillGridViewADMCompareFields(optionRealtimeMonitor, row, (int)ADM_MODEL_POSITION_COMPARE_FIELDS_DISPLAYED.ZERO_PRICE,
                        "false", Color.LightBlue);

                fillGridViewADMCompareFields(optionRealtimeMonitor, row, (int)ADM_MODEL_POSITION_COMPARE_FIELDS_DISPLAYED.EXCEPTIONS,
                        "true", Color.MediumPurple);

                optionSpreadManager.statusAndConnectedUpdates.markLiveAsConnected(gridViewModelADMCompare, row,
                    (int)ADM_MODEL_POSITION_COMPARE_FIELDS_DISPLAYED.REBALANCE,
                    false, Color.MediumPurple);
            }
            else
            {

                fillGridViewADMCompareFields(optionRealtimeMonitor, row, (int)ADM_MODEL_POSITION_COMPARE_FIELDS_DISPLAYED.ZERO_PRICE,
                        "false", Color.LightBlue);

                fillGridViewADMCompareFields(optionRealtimeMonitor, row, (int)ADM_MODEL_POSITION_COMPARE_FIELDS_DISPLAYED.EXCEPTIONS,
                        "false", Color.Cyan);

                if (FCM_DataImportLibrary.FCM_PostionList_forCompare != null && row < FCM_DataImportLibrary.FCM_PostionList_forCompare.Count
                    && FCM_DataImportLibrary.FCM_PostionList_forCompare[row].rebalanceLots == 0)
                {
                    optionSpreadManager.statusAndConnectedUpdates.markLiveAsConnected(gridViewModelADMCompare, row,
                        (int)ADM_MODEL_POSITION_COMPARE_FIELDS_DISPLAYED.REBALANCE,
                        false, Color.LightGray);
                }
                else
                {
                    optionSpreadManager.statusAndConnectedUpdates.markLiveAsConnected(gridViewModelADMCompare, row,
                        (int)ADM_MODEL_POSITION_COMPARE_FIELDS_DISPLAYED.REBALANCE,
                        false, Color.Yellow);
                }
            }
        }




    }
}
