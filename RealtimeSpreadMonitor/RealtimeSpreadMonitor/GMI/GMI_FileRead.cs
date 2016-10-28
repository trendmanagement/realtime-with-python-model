using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealtimeSpreadMonitor.GMI
{
    class GMI_FileRead
    {
        public delegate ADMPositionImportWeb Fill_GMI_ImportDelegate(List<string> stringList);

        public static ADMPositionImportWeb FillGMI_ImportFromBackup_Or_ADMWeb(List<string> stringList)
        {
            if (stringList.Count > 0
                    && stringList[(int)ADM_DETAIL_FIELDS.PEXCH].Trim().Length > 0
                    && stringList[(int)ADM_DETAIL_FIELDS.PFC].Trim().Length > 0
                    && stringList[(int)ADM_DETAIL_FIELDS.PCTYM].Trim().Length > 0)
            {

                ADMPositionImportWeb aDMSummaryImport = new ADMPositionImportWeb();

                aDMSummaryImport.RecordType = stringList[(int)ADM_DETAIL_FIELDS.RecordType];
                aDMSummaryImport.POFFIC = stringList[(int)ADM_DETAIL_FIELDS.POFFIC];
                aDMSummaryImport.PACCT = stringList[(int)ADM_DETAIL_FIELDS.PACCT];
                aDMSummaryImport.PCUSIP = stringList[(int)ADM_DETAIL_FIELDS.PCUSIP];
                aDMSummaryImport.PCUSIP2 = stringList[(int)ADM_DETAIL_FIELDS.PCUSIP2];
                aDMSummaryImport.Description = stringList[(int)ADM_DETAIL_FIELDS.Description];

                aDMSummaryImport.LongQuantity = getDoubleOutOfStringList(stringList, ADM_DETAIL_FIELDS.LongQuantity);
                aDMSummaryImport.ShortQuantity = getDoubleOutOfStringList(stringList, ADM_DETAIL_FIELDS.ShortQuantity);
                //aDMSummaryImport.Net = getDoubleOutOfStringList(stringList, ADM_DETAIL_FIELDS.TradeDate);

                //aDMSummaryImport.netContractsEditable = aDMSummaryImport.Net;

                aDMSummaryImport.TradeDate = stringList[(int)ADM_DETAIL_FIELDS.TradeDate];
                aDMSummaryImport.TradePrice = getDoubleOutOfStringList(stringList, ADM_DETAIL_FIELDS.TradePrice);

                aDMSummaryImport.WeightedPrice = getDoubleOutOfStringList(stringList, ADM_DETAIL_FIELDS.WeightedPrice);

                //aDMSummaryImport.AveragePrice = getDoubleOutOfStringList(stringList, ADM_DETAIL_FIELDS.AveragePrice);

                aDMSummaryImport.RealTimePrice = getDoubleOutOfStringList(stringList, ADM_DETAIL_FIELDS.RealTimePrice);
                aDMSummaryImport.SettledPrice = getDoubleOutOfStringList(stringList, ADM_DETAIL_FIELDS.SettledPrice);
                aDMSummaryImport.PrelimPrice = getDoubleOutOfStringList(stringList, ADM_DETAIL_FIELDS.PrelimPrice);
                aDMSummaryImport.Value = getDoubleOutOfStringList(stringList, ADM_DETAIL_FIELDS.Value);
                aDMSummaryImport.ClosedValue = getDoubleOutOfStringList(stringList, ADM_DETAIL_FIELDS.ClosedValue);
                aDMSummaryImport.SettledValue = getDoubleOutOfStringList(stringList, ADM_DETAIL_FIELDS.SettledValue);

                aDMSummaryImport.Currency = stringList[(int)ADM_DETAIL_FIELDS.Currency];
                aDMSummaryImport.PSUBTY = stringList[(int)ADM_DETAIL_FIELDS.PSUBTY];
                aDMSummaryImport.PEXCH = getIntOutOfStringList(stringList, ADM_DETAIL_FIELDS.PEXCH);
                aDMSummaryImport.PFC = stringList[(int)ADM_DETAIL_FIELDS.PFC];
                aDMSummaryImport.aDMStrike = stringList[(int)ADM_DETAIL_FIELDS.Strike];

                aDMSummaryImport.PCTYM = stringList[(int)ADM_DETAIL_FIELDS.PCTYM];

                aDMSummaryImport.PCARD = stringList[(int)ADM_DETAIL_FIELDS.PCARD];

                return aDMSummaryImport;
            }

            return null;
        }

        public static ADMPositionImportWeb FillGMI_Position_Wedbush(List<string> stringList)
        {
            if(stringList.Count > 283
               && stringList[0].CompareTo("C") != 0)
            {
                ADMPositionImportWeb aDMSummaryImport = new ADMPositionImportWeb();

                if (stringList[0].CompareTo("P") == 0)
                {
                    aDMSummaryImport.RecordType = "Position";
                }
                else
                {
                    aDMSummaryImport.RecordType = "Transaction";
                }


                aDMSummaryImport.POFFIC = stringList[2];
                aDMSummaryImport.PACCT = stringList[3];
                aDMSummaryImport.PCUSIP = stringList[5];
                aDMSummaryImport.PCUSIP2 = stringList[6];
                aDMSummaryImport.Description = stringList[77];

                aDMSummaryImport.LongQuantity = Convert.ToDouble(stringList[274]);
                aDMSummaryImport.ShortQuantity = Convert.ToDouble(stringList[275]);

                aDMSummaryImport.TradeDate = stringList[16];
                aDMSummaryImport.TradePrice = Convert.ToDouble(stringList[17]);

                aDMSummaryImport.WeightedPrice = 0;

                //aDMSummaryImport.AveragePrice = getDoubleOutOfStringList(stringList, ADM_DETAIL_FIELDS.AveragePrice);

                aDMSummaryImport.RealTimePrice = 0;
                aDMSummaryImport.SettledPrice = 0;
                aDMSummaryImport.PrelimPrice = 0;
                aDMSummaryImport.Value = 0;
                aDMSummaryImport.ClosedValue = 0;
                aDMSummaryImport.SettledValue = 0;

                aDMSummaryImport.Currency = stringList[283];
                aDMSummaryImport.PSUBTY = stringList[10];
                aDMSummaryImport.PEXCH = Convert.ToInt32(stringList[97]);
                aDMSummaryImport.PFC = stringList[98];
                aDMSummaryImport.aDMStrike = stringList[12];

                aDMSummaryImport.PCTYM = stringList[7];

                aDMSummaryImport.PCARD = stringList[139];

                return aDMSummaryImport;
            }

            return null;
        }

        public static ADMPositionImportWeb FillGMI_Position_RCG(List<string> stringList)
        {
            if (stringList.Count > 100
               && stringList[(int)RCG_POS.RID].CompareTo("C") != 0)
            {
                ADMPositionImportWeb aDMSummaryImport = new ADMPositionImportWeb();

                if (stringList[(int)RCG_POS.RID].CompareTo("P") == 0)
                {
                    aDMSummaryImport.RecordType = "Position";
                }
                else
                {
                    aDMSummaryImport.RecordType = "Transaction";
                }


                aDMSummaryImport.POFFIC = stringList[(int)RCG_POS.OFF];
                aDMSummaryImport.PACCT = stringList[(int)RCG_POS.ACCT];
                aDMSummaryImport.PCUSIP = stringList[(int)RCG_POS.CUSIP];
                aDMSummaryImport.PCUSIP2 = stringList[(int)RCG_POS.TRACER];
                aDMSummaryImport.Description = stringList[(int)RCG_POS.SDSC1];

                aDMSummaryImport.LongQuantity = Convert.ToDouble(stringList[(int)RCG_POS.BQTY]);
                aDMSummaryImport.ShortQuantity = Convert.ToDouble(stringList[(int)RCG_POS.SQTY]);

                aDMSummaryImport.TradeDate = stringList[(int)RCG_POS.TRD_DT];
                aDMSummaryImport.TradePrice = Convert.ToDouble(stringList[(int)RCG_POS.TPRIC]);

                aDMSummaryImport.WeightedPrice = 0;

                //aDMSummaryImport.AveragePrice = getDoubleOutOfStringList(stringList, ADM_DETAIL_FIELDS.AveragePrice);

                aDMSummaryImport.RealTimePrice = 0;
                aDMSummaryImport.SettledPrice = 0;
                aDMSummaryImport.PrelimPrice = 0;
                aDMSummaryImport.Value = 0;
                aDMSummaryImport.ClosedValue = 0;
                aDMSummaryImport.SettledValue = 0;

                aDMSummaryImport.Currency = stringList[(int)RCG_POS.CSY];
                aDMSummaryImport.PSUBTY = stringList[(int)RCG_POS.SUBTY];
                aDMSummaryImport.PEXCH = Convert.ToInt32(stringList[(int)RCG_POS.EX]);
                aDMSummaryImport.PFC = stringList[(int)RCG_POS.FC];
                aDMSummaryImport.aDMStrike = stringList[(int)RCG_POS.STRIK];

                aDMSummaryImport.PCTYM = stringList[(int)RCG_POS.CTYM];

                aDMSummaryImport.PCARD = stringList[(int)RCG_POS.CARD];

                return aDMSummaryImport;
            }

            return null;
        }

        public static ADMPositionImportWeb FillGMI_Transaction_Wedbush(List<string> stringList)
        {
            if (stringList.Count > 64
                &&
                (stringList[1].CompareTo("C") != 0))
            {
                ADMPositionImportWeb aDMSummaryImport = new ADMPositionImportWeb();

                if (stringList[1].CompareTo("T") == 0)
                {
                    aDMSummaryImport.RecordType = "Transaction";
                }
                else
                {
                    aDMSummaryImport.RecordType = "Position";
                }


                aDMSummaryImport.POFFIC = stringList[3];
                aDMSummaryImport.PACCT = stringList[5];
                aDMSummaryImport.PCUSIP = "";
                aDMSummaryImport.PCUSIP2 = "";
                aDMSummaryImport.Description = stringList[17];

                aDMSummaryImport.LongQuantity = Convert.ToDouble(stringList[43]);
                aDMSummaryImport.ShortQuantity = Convert.ToDouble(stringList[44]);

                aDMSummaryImport.TradeDate = stringList[40];
                aDMSummaryImport.TradePrice = Convert.ToDouble(stringList[49]);

                aDMSummaryImport.WeightedPrice = 0;

                aDMSummaryImport.RealTimePrice = Convert.ToDouble(stringList[27]);
                aDMSummaryImport.SettledPrice = 0;
                aDMSummaryImport.PrelimPrice = 0;
                aDMSummaryImport.Value = 0;
                aDMSummaryImport.ClosedValue = 0;
                aDMSummaryImport.SettledValue = 0;

                aDMSummaryImport.Currency = stringList[36];
                aDMSummaryImport.PSUBTY = stringList[23];
                aDMSummaryImport.PEXCH = Convert.ToInt32(stringList[14]);
                aDMSummaryImport.PFC = stringList[16];
                aDMSummaryImport.aDMStrike = stringList[24];

                aDMSummaryImport.PCTYM = stringList[20];

                aDMSummaryImport.PCARD = stringList[64];
            }

            return null;
        }

        public static ADMPositionImportWeb FillGMI_Transaction_RCG(List<string> stringList)
        {
            if (stringList.Count > 64
                &&
                (stringList[(int)RCG_TRND.FRECID].CompareTo("C") != 0))
            {

                ADMPositionImportWeb aDMSummaryImport = new ADMPositionImportWeb();

                if (stringList[(int)RCG_TRND.FRECID].CompareTo("T") == 0)
                {
                    aDMSummaryImport.RecordType = "Transaction";
                }
                else
                {
                    aDMSummaryImport.RecordType = "Position";
                }

                aDMSummaryImport.POFFIC = stringList[(int)RCG_TRND.FOFFIC];
                aDMSummaryImport.PACCT = stringList[(int)RCG_TRND.FACCT];
                aDMSummaryImport.PCUSIP = "";
                aDMSummaryImport.PCUSIP2 = "";
                aDMSummaryImport.Description = stringList[(int)RCG_TRND.FSDSC1];

                aDMSummaryImport.LongQuantity = Convert.ToDouble(stringList[(int)RCG_TRND.FBQTY]);
                aDMSummaryImport.ShortQuantity = Convert.ToDouble(stringList[(int)RCG_TRND.FSQTY]);

                aDMSummaryImport.TradeDate = stringList[(int)RCG_TRND.FTDATE];
                aDMSummaryImport.TradePrice = Convert.ToDouble(stringList[(int)RCG_TRND.FTPRIC]);

                aDMSummaryImport.WeightedPrice = 0;

                aDMSummaryImport.RealTimePrice = 0;
                aDMSummaryImport.SettledPrice = 0;
                aDMSummaryImport.PrelimPrice = 0;
                aDMSummaryImport.Value = 0;
                aDMSummaryImport.ClosedValue = 0;
                aDMSummaryImport.SettledValue = 0;

                aDMSummaryImport.Currency = stringList[(int)RCG_TRND.FCURSY];
                aDMSummaryImport.PSUBTY = stringList[(int)RCG_TRND.FSUBTY];
                aDMSummaryImport.PEXCH = Convert.ToInt32(stringList[(int)RCG_TRND.FEXCH]);
                aDMSummaryImport.PFC = stringList[(int)RCG_TRND.FFC];
                aDMSummaryImport.aDMStrike = stringList[(int)RCG_TRND.FSTRIK];

                aDMSummaryImport.PCTYM = stringList[(int)RCG_TRND.FCTYM];

                aDMSummaryImport.PCARD = stringList[(int)RCG_TRND.FCARD];

                return aDMSummaryImport;
            }

            return null;
        }

        public static ADMPositionImportWeb FillGMI_Position_ADM(List<string> stringList)
        {
            if(stringList[0].CompareTo("C") != 0)
                {
                ADMPositionImportWeb aDMSummaryImport = new ADMPositionImportWeb();

                if (stringList[0].CompareTo("P") == 0)
                {
                    aDMSummaryImport.RecordType = "Position";
                }
                else
                {
                    aDMSummaryImport.RecordType = "Transaction";
                }


                aDMSummaryImport.POFFIC = stringList[2];
                aDMSummaryImport.PACCT = stringList[3];
                aDMSummaryImport.PCUSIP = stringList[5];
                aDMSummaryImport.PCUSIP2 = "";
                aDMSummaryImport.Description = stringList[20];

                aDMSummaryImport.LongQuantity = Convert.ToDouble(stringList[71]);
                aDMSummaryImport.ShortQuantity = Convert.ToDouble(stringList[72]);

                aDMSummaryImport.TradeDate = stringList[13];


                aDMSummaryImport.TradePrice = Convert.ToDouble(stringList[14]);



                aDMSummaryImport.WeightedPrice = 0;

                //aDMSummaryImport.AveragePrice = getDoubleOutOfStringList(stringList, ADM_DETAIL_FIELDS.AveragePrice);

                aDMSummaryImport.RealTimePrice = 0;
                aDMSummaryImport.SettledPrice = 0;
                aDMSummaryImport.PrelimPrice = 0;
                aDMSummaryImport.Value = 0;
                aDMSummaryImport.ClosedValue = 0;
                aDMSummaryImport.SettledValue = 0;

                aDMSummaryImport.Currency = stringList[76];
                aDMSummaryImport.PSUBTY = stringList[9];
                aDMSummaryImport.PEXCH = Convert.ToInt32(stringList[29]);
                aDMSummaryImport.PFC = stringList[30];
                aDMSummaryImport.aDMStrike = stringList[11];

                aDMSummaryImport.PCTYM = stringList[6];

                aDMSummaryImport.PCARD = stringList[53];

                return aDMSummaryImport;
            }

            return null;
        }

        public static ADMPositionImportWeb FillGMI_Transaction_ADM(List<string> stringList)
        {
            if (stringList.Count >= 38
                &&
                (stringList[0].CompareTo("C") != 0))
            {
                DateTime tradeDateTest = new DateTime(Convert.ToInt16(stringList[12]),
                    Convert.ToInt16(stringList[10]),
                    Convert.ToInt16(stringList[11]));



                if (stringList[0].CompareTo("T") == 0
                    && tradeDateTest.CompareTo(DateTime.Now.Date) != 0)
                {
                    return null;
                }
                else
                {

                    ADMPositionImportWeb aDMSummaryImport = new ADMPositionImportWeb();


                    if (stringList[0].CompareTo("P") == 0)
                    {
                        aDMSummaryImport.RecordType = "Position";
                    }
                    else
                    {
                        aDMSummaryImport.RecordType = "Transaction";
                    }


                    aDMSummaryImport.POFFIC = stringList[2];
                    aDMSummaryImport.PACCT = stringList[3];
                    aDMSummaryImport.PCUSIP = stringList[5];
                    aDMSummaryImport.PCUSIP2 = "";
                    aDMSummaryImport.Description = stringList[25];

                    if (stringList[14].Trim().Length > 0)
                    {
                        aDMSummaryImport.LongQuantity = Convert.ToDouble(stringList[14]);
                    }

                    if (stringList[15].Trim().Length > 0)
                    {
                        aDMSummaryImport.ShortQuantity = Convert.ToDouble(stringList[15]);
                    }

                    StringBuilder tradeDate = new StringBuilder();
                    tradeDate.Append(stringList[12]);
                    tradeDate.Append(stringList[10]);
                    tradeDate.Append(stringList[11]);

                    aDMSummaryImport.TradeDate = tradeDate.ToString();


                    aDMSummaryImport.TradePrice = Convert.ToDouble(stringList[29]);



                    aDMSummaryImport.WeightedPrice = 0;

                    //aDMSummaryImport.AveragePrice = getDoubleOutOfStringList(stringList, ADM_DETAIL_FIELDS.AveragePrice);

                    aDMSummaryImport.RealTimePrice = 0;
                    aDMSummaryImport.SettledPrice = 0;
                    aDMSummaryImport.PrelimPrice = 0;
                    aDMSummaryImport.Value = 0;
                    aDMSummaryImport.ClosedValue = 0;
                    aDMSummaryImport.SettledValue = 0;

                    aDMSummaryImport.Currency = stringList[32];
                    aDMSummaryImport.PSUBTY = stringList[7];
                    aDMSummaryImport.PEXCH = Convert.ToInt32(stringList[27]);
                    aDMSummaryImport.PFC = stringList[26];

                    if (stringList[8].Trim().Length > 0)
                    {
                        aDMSummaryImport.aDMStrike = stringList[8].Trim();
                    }
                    else
                    {
                        aDMSummaryImport.aDMStrike = "0";
                    }

                    aDMSummaryImport.PCTYM = stringList[6];

                    aDMSummaryImport.PCARD = stringList[13];

                    return aDMSummaryImport;
                }
            }

            return null;
        }

        private static double getDoubleOutOfStringList(List<String> stringList, ADM_DETAIL_FIELDS summaryField)
        {

            try

            {
                return Convert.ToDouble(stringList[(int)summaryField]);
            }

            catch (Exception ex)
            {
            }

            return 0;
        }

        private static int getIntOutOfStringList(List<String> stringList, ADM_DETAIL_FIELDS summaryField)
        {

            try

            {
                return Convert.ToInt32(stringList[(int)summaryField]);
            }

            catch (Exception ex)
            {
            }

            return 0;
        }

        private static double getDoubleOutOfStringList(List<String> stringList, ADM_SUMMARY_FIELDS summaryField)
        {

            try

            {
                return Convert.ToDouble(stringList[(int)summaryField]);
            }

            catch (Exception ex)
            {
            }

            return 0;
        }

        private static int getIntOutOfStringList(List<String> stringList, ADM_SUMMARY_FIELDS summaryField)
        {

            try

            {
                return Convert.ToInt32(stringList[(int)summaryField]);
            }

            catch (Exception ex)
            {
            }

            return 0;
        }


    }
}
