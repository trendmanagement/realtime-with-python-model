using RealtimeSpreadMonitor.Mongo;
using System;
using System.Collections.Generic;
//using CQG;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RealtimeSpreadMonitor
{

    public class TMLAzureModelDBQueries : TMLAzureDBConnection
    {
        //DataClassesTMLDBDataContext contextTMLDB = new DataClassesTMLDBDataContext(
        //    System.Configuration.ConfigurationManager.ConnectionStrings["RealtimeSpreadMonitor.Properties.Settings.TMLDBConnectionString"].ConnectionString);

        //public TMLAzureModelDBQueries()
        //{

        //}

        /// <summary>
        /// fills the contract settlements from SQL
        /// </summary>
        /// <param name="mose"></param>
        public void GetContractLatestSettlement(
            MongoDB_OptionSpreadExpression mose)
        {

            try
            {
                tbldailycontractsettlement dailyContractSettlement =
                    contextTMLDB.tbldailycontractsettlements
                    .Where(c => c.idcontract == mose.asset.idcontract)
                    //.Where(c => c.date.CompareTo(new DateTime(2016, 9, 1)) >= 0)
                    .OrderByDescending(c => c.date).First();//Take(1);

                if (dailyContractSettlement != null)
                {                    
                    mose.yesterdaySettlement = dailyContractSettlement.settlement;
                    mose.yesterdaySettlementDateTime = dailyContractSettlement.date;
                    mose.yesterdaySettlementFilled = true;
                }
            }
            catch (InvalidOperationException)
            {
                //return DateTime.Today.AddDays(-1);
            }
            catch (SqlException)
            {
                //return DateTime.Today.AddDays(-1);
            }

            //return DateTime.Today.AddDays(-1);
        }

        /// <summary>
        /// fills the settlement and implied vol of options
        /// </summary>
        /// <param name="mose"></param>
        public void GetOptionLatestSettlementAndImpliedVol(
            MongoDB_OptionSpreadExpression mose)
        {

            try
            {
                tbloptiondata dailyOptionSettlement =
                    contextTMLDB.tbloptiondatas
                    .Where(c => c.idoption == mose.asset.idoption)
                    //.Where(c => c.date.CompareTo(new DateTime(2016, 9, 1)) >= 0)
                    .OrderByDescending(c => c.datetime).First();//Take(1);

                if (dailyOptionSettlement != null)
                {
                    mose.yesterdaySettlement = dailyOptionSettlement.price;
                    mose.yesterdaySettlementDateTime = dailyOptionSettlement.datetime;
                    mose.impliedVolFromSpan = dailyOptionSettlement.impliedvol;
                    mose.yesterdaySettlementFilled = true;
                }
            }
            catch (InvalidOperationException)
            {
                //return DateTime.Today.AddDays(-1);
            }
            catch (SqlException)
            {
                //return DateTime.Today.AddDays(-1);
            }

            //return DateTime.Today.AddDays(-1);
        }

        public DateTime GetContractPreviousDateTime(
            long idcontract)
        {

            try
            {
                tbldailycontractsettlement dailyContractSettlement =
                    contextTMLDB.tbldailycontractsettlements
                    .Where(c => c.idcontract == idcontract)
                    //.Where(c => c.date.CompareTo(new DateTime(2016, 9, 1)) >= 0)
                    .OrderByDescending(c => c.date).First();//Take(1);

                if (dailyContractSettlement != null)
                {
                    return dailyContractSettlement.date;
                }
            }
            catch (InvalidOperationException)
            {
                return DateTime.Today.AddDays(-1);
            }
            catch (SqlException)
            {
                return DateTime.Today.AddDays(-1);
            }

            return DateTime.Today.AddDays(-1);
        }



        /// <summary>
        /// Queries the future information and data from cloud.
        /// </summary>
        /// <param name="idContract">The identifier contract.</param>
        /// <param name="legInfo">The leg information.</param>
        /// <param name="legData">The leg data.</param>
        /// <param name="optionArrayTypes">The option array types.</param>
        /// <param name="futureDataSetHashSet">The future data set hash set.</param>
        

        public OptionInputFieldsFromTblOptionInputSymbols queryOptionInputSymbols(
            int idinstrument, int idoptioninputtype)
        {
            try
            {
                OptionInputFieldsFromTblOptionInputSymbols optionInputFieldsFromTblOptionInputSymbols = null;

                DataSet instrumentDataSet = new DataSet();

                StringBuilder dataQuery = new StringBuilder();

                if (idoptioninputtype == (int)OPTION_FORMULA_INPUT_TYPES.OPTION_RISK_FREE_RATE)
                {
                    dataQuery.Append("SELECT idoptioninputsymbol, optioninputcqgsymbol, idinstrument, idoptioninputtype, multiplier");
                    dataQuery.Append(" FROM cqgdb.tbloptioninputsymbols");
                    dataQuery.Append(" WHERE ");
                    dataQuery.Append(" idoptioninputtype = ");
                    dataQuery.Append(idoptioninputtype);
                }
                else
                {
                    dataQuery.Append("SELECT idoptioninputsymbol, optioninputcqgsymbol, idinstrument, idoptioninputtype, multiplier");
                    dataQuery.Append(" FROM cqgdb.tbloptioninputsymbols");
                    dataQuery.Append(" WHERE idinstrument = ");
                    dataQuery.Append(idinstrument);
                    dataQuery.Append(" AND idoptioninputtype = ");
                    dataQuery.Append(idoptioninputtype);
                }

                //MySqlDataAdapter cmdGetInstruments = new MySqlDataAdapter(dataQuery.ToString(), conn);
                //int instrumentRows = cmdGetInstruments.Fill(instrumentDataSet);


                int instrumentRows = ConnectDBSqlDataAdapter(dataQuery.ToString(), instrumentDataSet);



                DataRow[] instrumentArray = instrumentDataSet.Tables[0].Select();

                if (instrumentRows > 0)
                {
                    optionInputFieldsFromTblOptionInputSymbols = new OptionInputFieldsFromTblOptionInputSymbols();

                    int i = 0;

                    optionInputFieldsFromTblOptionInputSymbols.idOptionInputSymbol =
                        Convert.ToInt16(instrumentArray[i].ItemArray.GetValue(0));
                    optionInputFieldsFromTblOptionInputSymbols.optionInputCQGSymbol =
                        Convert.ToString(instrumentArray[i].ItemArray.GetValue(1));
                    optionInputFieldsFromTblOptionInputSymbols.idinstrument =
                        Convert.ToInt16(instrumentArray[i].ItemArray.GetValue(2));
                    optionInputFieldsFromTblOptionInputSymbols.idOptionInputType =
                        Convert.ToInt16(instrumentArray[i].ItemArray.GetValue(3));
                    optionInputFieldsFromTblOptionInputSymbols.multiplier =
                        Convert.ToDouble(instrumentArray[i].ItemArray.GetValue(4));
                }

                return optionInputFieldsFromTblOptionInputSymbols;
            }
            catch (Exception ex)
            {
                TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
            }

            return null;

        }


        public DateTime QueryOptionExpirationTimes(long idInstrument, int optionMonthInt)
        {


            try
            {
                //StringBuilder dataQuery = new StringBuilder();

                //dataQuery.Append("SELECT expirationlasttradetime");
                //dataQuery.Append(" FROM cqgdb.tbloptionproperties");
                //dataQuery.Append(" WHERE");
                //dataQuery.Append(" idinstrument = " + idInstrument);
                //dataQuery.Append(" AND optionmonthint = ");
                //dataQuery.Append(optionMonthInt);

                tbloptionproperty top =
                    contextTMLDB.tbloptionproperties
                    .Where(c => c.idinstrument == idInstrument)
                    .Where(c => c.optionmonthint == optionMonthInt).First();
                    //.Where(c => c.date.CompareTo(new DateTime(2016, 9, 1)) >= 0)
                    //.OrderByDescending(c => c.datetime).First();//Take(1);

                //TSErrorCatch.debugWriteOut(dataQuery.ToString());

                //DataSet strategyDataSet = new DataSet();

                //MySqlDataAdapter cmdGetStrategyData = new MySqlDataAdapter(dataQuery.ToString(), conn);

                //int instRows = cmdGetStrategyData.Fill(strategyDataSet);


                //int instRows = ConnectDBSqlDataAdapter(dataQuery.ToString(), strategyDataSet);

                

                //DataRow[] strategyDataRows = strategyDataSet.Tables[0].Select();

                //if (instRows > 0)
                //{
                    return top.expirationlasttradetime;
                //}

                
            }
            catch (Exception ex)
            {
                TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
            }

            return new DateTime(0, 0, 0, 0, 0, 0);

        }


        public DataRow[][] selectOptionSystemPlResults(DateTime queryDateTime, int idPortfoliogroup, int fcmData)
        {
            StringBuilder dataQuery = new StringBuilder();

            //dataQuery.Append("SELECT tblsystemplresults.contract, tblsystemplresults.idinstrument,");
            //dataQuery.Append(" tblsystemplresults.date,");
            //dataQuery.Append(" tblsystemplresults.plSettleDayChg, tblsystemplresults.plLongSettleOrderChg, tblsystemplresults.plShortSettleOrderChg,");
            //dataQuery.Append(" tblsystemplresults.totalQty, tblsystemplresults.longOrders, tblsystemplresults.shortOrders,");
            //dataQuery.Append(" tblsystemplresults.longTransAvgPrice, tblsystemplresults.shortTransAvgPrice,");
            //dataQuery.Append(" tblsystemplresults.settle, tblsystemplresults.yesterdaySettle, tblsystemplresults.settlementImpliedVol,");
            //dataQuery.Append(" tblsystemplresults.idContract, tblsystemplresults.idOption, tblsystemplresults.callOrPutOrFuture,");

            ////dataQuery.Append(" tbloptiondata.datetime, tbloptiondata.price, tbloptiondata.impliedVol,");

            //dataQuery.Append(" tbloptions.expirationdate, tblsystemplresults.officeAcct");

            ////dataQuery.Append(" FROM cqgdb.tblsystemplresults, cqgdb.tbloptiondata, cqgdb.tbloptions");

            //dataQuery.Append(" FROM cqgdb.tblsystemplresults, cqgdb.tbloptions");

            //dataQuery.Append(" WHERE tblsystemplresults.callOrPutOrFuture != 'F'");

            //dataQuery.Append(" AND tblsystemplresults.idportfoliogroup = ");
            //dataQuery.Append(idPortfoliogroup);

            //dataQuery.Append(" AND tblsystemplresults.fcmData = ");
            //dataQuery.Append(fcmData);

            //dataQuery.Append(" AND tblsystemplresults.date = '");
            //dataQuery.Append(queryDateTime.ToString("yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo));

            ////dataQuery.Append("' AND tblsystemplresults.idoption = tbloptiondata.idoption");

            //dataQuery.Append("' AND tblsystemplresults.idoption = tbloptions.idoption");

            //dataQuery.Append(" AND tbloptiondata.datetime = '");
            //dataQuery.Append(queryDateTime.ToString("yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo));

            //dataQuery.Append("' ORDER BY idinstrument");

            //MySqlDataAdapter cmdGetSystemPlResults = new MySqlDataAdapter(dataQuery.ToString(), conn);

            dataQuery.Append("execute cqgdb.sp_selectSystemPLResults '");

            dataQuery.Append(queryDateTime.ToString("yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo));

            dataQuery.Append("', ");
            dataQuery.Append(idPortfoliogroup);
            dataQuery.Append(",");
            dataQuery.Append(fcmData);

            DataSet systemPlResultsDataSet = new DataSet();

            //int configRows = cmdGetSystemPlResults.Fill(systemPlResultsDataSet);

            //DataRow[] systemPlResults = systemPlResultsDataSet.Tables[0].Select();


            int optionDateRows = ConnectDBSqlDataAdapter(dataQuery.ToString(), systemPlResultsDataSet);

            DataRow[][] systemPlResults = new DataRow[2][];

            systemPlResults[0] = systemPlResultsDataSet.Tables[0].Select();
            systemPlResults[1] = systemPlResultsDataSet.Tables[1].Select();

            //DataRow[] systemPlResults = systemPlResultsDataSet.Tables[0].Select();


            return systemPlResults;
        }

       

        public DataRow[] selectFutureSystemPlResults(DateTime queryDateTime, int idPortfoliogroup, int fcmData)
        {
            StringBuilder dataQuery = new StringBuilder();

            dataQuery.Append("SELECT tblsystemplresults.contract, tblsystemplresults.idinstrument,");
            dataQuery.Append(" tblsystemplresults.date,");
            dataQuery.Append(" tblsystemplresults.plSettleDayChg, tblsystemplresults.plLongSettleOrderChg, tblsystemplresults.plShortSettleOrderChg,");
            dataQuery.Append(" tblsystemplresults.totalQty, tblsystemplresults.longOrders, tblsystemplresults.shortOrders,");
            dataQuery.Append(" tblsystemplresults.longTransAvgPrice, tblsystemplresults.shortTransAvgPrice,");
            dataQuery.Append(" tblsystemplresults.settle, tblsystemplresults.yesterdaySettle,");
            dataQuery.Append(" tblsystemplresults.idContract, tblsystemplresults.idOption, tblsystemplresults.callOrPutOrFuture,");

            dataQuery.Append(" tbldailycontractsettlements.date, tbldailycontractsettlements.settlement,");

            dataQuery.Append(" tblcontracts.expirationdate, tblsystemplresults.officeAcct");

            dataQuery.Append(" FROM cqgdb.tblsystemplresults, cqgdb.tbldailycontractsettlements, cqgdb.tblcontracts");

            dataQuery.Append(" WHERE tblsystemplresults.callOrPutOrFuture = 'F'");

            dataQuery.Append(" AND tblsystemplresults.idportfoliogroup = ");
            dataQuery.Append(idPortfoliogroup);

            dataQuery.Append(" AND tblsystemplresults.fcmData = ");
            dataQuery.Append(fcmData);

            dataQuery.Append(" AND tblsystemplresults.date = '");
            dataQuery.Append(queryDateTime.ToString("yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo));

            dataQuery.Append("' AND tblsystemplresults.idContract = tbldailycontractsettlements.idcontract");

            dataQuery.Append(" AND tblsystemplresults.idContract = tblcontracts.idcontract");

            dataQuery.Append(" AND tbldailycontractsettlements.date = '");
            dataQuery.Append(queryDateTime.ToString("yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo));

            dataQuery.Append("' ORDER BY idinstrument");

            //MySqlDataAdapter cmdGetSystemPlResults = new MySqlDataAdapter(dataQuery.ToString(), conn);

            DataSet systemPlResultsDataSet = new DataSet();

            //int configRows = cmdGetSystemPlResults.Fill(systemPlResultsDataSet);

            //DataRow[] systemPlResults = systemPlResultsDataSet.Tables[0].Select();


            int optionDateRows = ConnectDBSqlDataAdapter(dataQuery.ToString(), systemPlResultsDataSet);

            DataRow[] systemPlResults = systemPlResultsDataSet.Tables[0].Select();


            return systemPlResults;
        }


    }
}
