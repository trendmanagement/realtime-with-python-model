using RealtimeSpreadMonitor.GMI;
using RealtimeSpreadMonitor.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using static RealtimeSpreadMonitor.GMI.GMI_FileRead;

namespace RealtimeSpreadMonitor
{
    public class ADMDataCommonMethods
    {
        Array optionMonthTypesArray;



        public ADMDataCommonMethods()
        {
            Type optionMonthTypes = typeof(OPTION_MONTHS);
            optionMonthTypesArray = Enum.GetNames(optionMonthTypes);
        }

        public void subContractType(ADM_Input_Data admObject,
            String PSUBTY)
        {
            if (PSUBTY.Trim().Length == 0)
            {
                admObject.isFuture = true;

                admObject.callPutOrFuture = OPTION_SPREAD_CONTRACT_TYPE.FUTURE;
            }
            else
            {
                admObject.isFuture = false;

                if (PSUBTY.CompareTo("C") == 0)
                {
                    admObject.callPutOrFuture = OPTION_SPREAD_CONTRACT_TYPE.CALL;
                }
                else
                {
                    admObject.callPutOrFuture = OPTION_SPREAD_CONTRACT_TYPE.PUT;
                }
            }


        }

        public String generateOptionCQGSymbol(ADM_Input_Data admSummaryImport,
            String PSUBTY, DateTime PCTYM,
            Instrument_mongo instrument)
        {
            StringBuilder cqgSymbol = new StringBuilder();

#if DEBUG
            try
#endif
            {
                //String typeSymbol = admSummaryImport.PSUBTY;

                if (admSummaryImport.isFuture)
                {
                    cqgSymbol.Append("F");

                    cqgSymbol.Append(".");

                    cqgSymbol.Append(instrument.cqgsymbol);

                    //int year = (Convert.ToInt16(admSummaryImport.PCTYM.Substring(0, 4)) - 2000) % 10;

                    //**********
                    //String dateFormat = "yyyyMM";

                    //DateTime admSummaryYearMonth = DateTime.ParseExact(PCTYM, dateFormat, CultureInfo.InvariantCulture);
                    int year = PCTYM.Year % 100;
                    //**********

                    String monthChar = optionMonthTypesArray.GetValue(PCTYM.Month - 1).ToString();

                    cqgSymbol.Append(monthChar);

                    cqgSymbol.Append(year);
                }
                else
                {
                    cqgSymbol.Append(PSUBTY);

                    cqgSymbol.Append(".");

                    cqgSymbol.Append(instrument.cqgsymbol);

                    //int year = (Convert.ToInt16(admSummaryImport.PCTYM.Substring(0, 4)) - 2000) % 10;

                    //**********
                    //String dateFormat = "yyyyMM";

                    //DateTime admSummaryYearMonth = DateTime.ParseExact(PCTYM, dateFormat, CultureInfo.InvariantCulture);
                    int year = PCTYM.Year % 10;
                    //**********

                    String monthChar = optionMonthTypesArray.GetValue(PCTYM.Month - 1).ToString();

                    cqgSymbol.Append(monthChar);

                    cqgSymbol.Append(year);

                    cqgSymbol.Append(admSummaryImport.strike);
                }


            }
#if DEBUG
            catch (Exception ex)
            {
                TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
            }
#endif

            return cqgSymbol.ToString();
        }


        internal void readADMDetailedPositionImportWeb(String[] fileNames,
            List<ADMPositionImportWeb> FCM_SummaryFieldsList, ImportFileCheck importFileCheck)
        {
            BrokerImportFiles brokerImportFiles = BrokerImportFiles.BACKUP_SAVED_FILE;
            FTPInputFileTypes cSVImportFileType = FTPInputFileTypes.ADM_WEB_INTERFACE_FILES;

            importFileCheck.importfile = true;

            //bool importFile = true;

#if DEBUG
            try
#endif
            {
                Type admSummaryFieldTypes = typeof(ADM_DETAIL_FIELDS);
                Array admSummaryFieldTypeArray = Enum.GetNames(admSummaryFieldTypes);

                for (int fileCounter = 0; fileCounter < fileNames.Length; fileCounter++)
                {
                    bool firstLine = true;

                    if (File.Exists(fileNames[fileCounter]))
                    {
                        DateTime lastWriteTime = File.GetLastWriteTime(fileNames[fileCounter]);

                        {
                            FileStream fileStream = new FileStream(fileNames[fileCounter],
                                FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                            StreamReader streamReader = new StreamReader(fileStream);


                            if (fileNames[fileCounter].ToLower().EndsWith(".csv"))
                            {


                                string[] parsedFileName = fileNames[fileCounter].Split('\\');

                                if (parsedFileName.Last().Substring(0, 3).ToLower().CompareTo("pos") == 0)
                                {
                                    cSVImportFileType = FTPInputFileTypes.POSITION_FILE_WEDBUSH_OR_RCG;

                                    brokerImportFiles = BrokerImportFiles.GMI_IMPORT_CSV_FILES;
                                }
                                else if (parsedFileName.Last().Substring(0, 3).ToLower().CompareTo("prl") == 0)
                                {
                                    cSVImportFileType = FTPInputFileTypes.TRANSACTION_FILE_WEDBUSH;

                                    brokerImportFiles = BrokerImportFiles.GMI_IMPORT_CSV_FILES;
                                }
                                else if (parsedFileName.Last().Substring(0, 4).ToLower().CompareTo("trnd") == 0)
                                {
                                    cSVImportFileType = FTPInputFileTypes.TRANSACTION_FILE_RCG;

                                    brokerImportFiles = BrokerImportFiles.GMI_IMPORT_CSV_FILES;
                                }
                                else if (parsedFileName.Last().ToLower().EndsWith("adatpos.csv"))
                                {
                                    cSVImportFileType = FTPInputFileTypes.POSITION_FILE_ADM;

                                    brokerImportFiles = BrokerImportFiles.GMI_IMPORT_CSV_FILES;
                                }
                                else if (parsedFileName.Last().ToLower().EndsWith("aaprlmcsv.csv"))
                                {
                                    cSVImportFileType = FTPInputFileTypes.TRANSACTION_FILE_ADM;

                                    brokerImportFiles = BrokerImportFiles.ADM_WEB_IMPORT_FILES;
                                }
                                else
                                {
                                    importFileCheck.importfile = false;

                                    String caption = "An imported file is not a 'pos' or 'prl' file";
                                    String message = fileNames[fileCounter] + "\nis not a 'pos' or 'prl'";
                                    MessageBoxButtons buttons = MessageBoxButtons.OK;
                                    System.Windows.Forms.DialogResult result;

                                    // Displays the MessageBox.
                                    result = MessageBox.Show(message, caption, buttons, MessageBoxIcon.Error);
                                }

                            }
                            else
                            {
                                cSVImportFileType = FTPInputFileTypes.ADM_WEB_INTERFACE_FILES;
                                brokerImportFiles = BrokerImportFiles.ADM_WEB_IMPORT_FILES;
                            }

                            bool setFileReader = false;
                            Fill_GMI_ImportDelegate Import_GMI = null;

                            while (!(streamReader.EndOfStream) && importFileCheck.importfile)
                            {
                                String line = streamReader.ReadLine();
                                if (line.Length > 0)
                                {
                                    List<String> stringList;  // = readSeparatedLine(line, '\t');


                                    if (cSVImportFileType == FTPInputFileTypes.ADM_WEB_INTERFACE_FILES
                                        || importFileCheck.importingBackedUpSavedFile)
                                    {
                                        stringList = readSeparatedLine(line, '\t');
                                    }
                                    else
                                    {
                                        stringList = readSeparatedLine(line, ',');
                                    }

                                    if (cSVImportFileType == FTPInputFileTypes.POSITION_FILE_ADM
                                        || cSVImportFileType == FTPInputFileTypes.TRANSACTION_FILE_ADM)
                                    {
                                        //there isn't a header line in these 2 file types from ADM ftp
                                        firstLine = false;
                                    }





                                    if (firstLine)
                                    {
                                        //firstLine = false;

                                        if (cSVImportFileType == FTPInputFileTypes.POSITION_FILE_WEDBUSH_OR_RCG)
                                        {

                                            if (stringList.Count > 0)
                                            {
                                                //brokerImportFiles = BrokerImportFiles.BACKUP_SAVED_FILE;

                                                if (stringList[0].CompareTo("R I D") == 0)
                                                {
                                                    cSVImportFileType = FTPInputFileTypes.POSITION_FILE_RCG;
                                                }
                                                else
                                                {
                                                    cSVImportFileType = FTPInputFileTypes.POSITION_FILE_WEDBUSH;
                                                }

                                            }
                                        }

                                        else if (importFileCheck.importingBackedUpSavedFile)
                                        {
                                            brokerImportFiles = BrokerImportFiles.BACKUP_SAVED_FILE;


                                        }
                                        //else if (brokerImportFiles == BrokerImportFiles.ADM_FILES)
                                        else if (cSVImportFileType == FTPInputFileTypes.ADM_WEB_INTERFACE_FILES)
                                        {
                                            int stringListCounter = 0;

                                            while (stringListCounter < stringList.Count)
                                            {
                                                if (stringListCounter >= admSummaryFieldTypeArray.Length
                                                    ||
                                                    (stringListCounter < admSummaryFieldTypeArray.Length
                                                    && stringList[stringListCounter].CompareTo(
                                                    admSummaryFieldTypeArray.GetValue(stringListCounter)) != 0))
                                                {
                                                    String caption = "ADM FIELDS HAVE CHANGED, FIELDS DO NOT MATCH PREVIOUS INPUTS";
                                                    String message = fileNames[fileCounter] + "\nADM FIELDS HAVE CHANGED, THERE IS A PROBLEM \nFILE WILL NOT BE IMPORTED";
                                                    MessageBoxButtons buttons = MessageBoxButtons.OK;
                                                    System.Windows.Forms.DialogResult result;

                                                    // Displays the MessageBox.
                                                    result = MessageBox.Show(message, caption, buttons, MessageBoxIcon.Error);

                                                    importFileCheck.importfile = false;

                                                    break;
                                                }

                                                stringListCounter++;
                                            }
                                        }

                                    }

                                    if (!setFileReader)
                                    {
                                        if ((importFileCheck.importingBackedUpSavedFile
                                            || cSVImportFileType == FTPInputFileTypes.ADM_WEB_INTERFACE_FILES)
                                            && importFileCheck.importfile)
                                        {
                                            Import_GMI = new Fill_GMI_ImportDelegate(GMI_FileRead.FillGMI_ImportFromBackup_Or_ADMWeb);
                                            setFileReader = true;
                                        }
                                        else if (cSVImportFileType == FTPInputFileTypes.POSITION_FILE_WEDBUSH)
                                        {
                                            Import_GMI = new Fill_GMI_ImportDelegate(GMI_FileRead.FillGMI_Position_Wedbush);
                                            setFileReader = true;
                                        }
                                        else if (cSVImportFileType == FTPInputFileTypes.TRANSACTION_FILE_WEDBUSH)
                                        {
                                            Import_GMI = new Fill_GMI_ImportDelegate(GMI_FileRead.FillGMI_Transaction_Wedbush);
                                            setFileReader = true;
                                        }
                                        else if (cSVImportFileType == FTPInputFileTypes.POSITION_FILE_RCG)
                                        {
                                            Import_GMI = new Fill_GMI_ImportDelegate(GMI_FileRead.FillGMI_Position_RCG);
                                            setFileReader = true;
                                        }
                                        else if (cSVImportFileType == FTPInputFileTypes.TRANSACTION_FILE_RCG)
                                        {
                                            Import_GMI = new Fill_GMI_ImportDelegate(GMI_FileRead.FillGMI_Transaction_RCG);
                                            setFileReader = true;
                                        }
                                        else if (cSVImportFileType == FTPInputFileTypes.POSITION_FILE_ADM)
                                        {
                                            Import_GMI = new Fill_GMI_ImportDelegate(GMI_FileRead.FillGMI_Position_ADM);
                                            setFileReader = true;
                                        }
                                        else if (cSVImportFileType == FTPInputFileTypes.TRANSACTION_FILE_ADM)
                                        {
                                            Import_GMI = new Fill_GMI_ImportDelegate(GMI_FileRead.FillGMI_Transaction_ADM);
                                            setFileReader = true;
                                        }
                                    }


                                    if (!firstLine && setFileReader)
                                    {
                                        ADMPositionImportWeb aDMSummaryImport = Import_GMI(stringList);

                                        if (aDMSummaryImport != null)
                                        {
                                            

                                            setupInstrumentAndfillStrikeInDecimal(aDMSummaryImport,
                                                brokerImportFiles);

                                            string office = aDMSummaryImport.POFFIC.ToString();
                                            string acct = aDMSummaryImport.PACCT.ToString();

                                            var key = Tuple.Create(office, acct);

                                            //var key = Tuple.Create("test",
                                            //            "test");

                                            //AccountAllocation aa;

                                            if (DataCollectionLibrary.portfolioAllocation.accountAllocation_KeyOfficAcct
                                                
                                                .ContainsKey(key))
                                            {
                                                aDMSummaryImport.acctGroup =
                                                    DataCollectionLibrary.portfolioAllocation.accountAllocation_KeyOfficAcct[key];

                                                //only add accounts that we have set up
                                                FCM_SummaryFieldsList.Add(aDMSummaryImport);

                                                //aDMSummaryImport.acctGroup = aa;
                                            }
                                            else
                                            {
                                                //if (DataCollectionLibrary.portfolioAllocation.accountAllocation.Count > 0)
                                                //{
                                                //    aDMSummaryImport.acctGroup =
                                                //        DataCollectionLibrary.portfolioAllocation.accountAllocation[0];
                                                //}
                                            }

                                            //Console.WriteLine(aDMSummaryImport.POFFIC);
                                            //Console.WriteLine(aDMSummaryImport.PACCT);
                                        }
                                    }

                                    if (firstLine)
                                    {
                                        firstLine = false;
                                    }


                                }
                            }

                            streamReader.Close();
                            fileStream.Close();

                        }

                        //Console.WriteLine("test");

                    }
                }

            }
#if DEBUG
            catch (Exception ex)
            {
                TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
            }
#endif

            if (!importFileCheck.importfile)
            {
                FCM_SummaryFieldsList.Clear();
            }

            //return importFile;
        }


        internal void setupInstrumentAndfillStrikeInDecimal(ADMPositionImportWeb aDMSummaryImport,
            BrokerImportFiles brokerImportFiles)
        {

            if (aDMSummaryImport.PFC.CompareTo("05") == 0)
            {
                aDMSummaryImport.PFC = "S-";
            }
            else if (aDMSummaryImport.PFC.CompareTo("02") == 0)
            {
                aDMSummaryImport.PFC = "C-";
            }

            

            if (DataCollectionLibrary.instrumentHashTable_keyadmcode.ContainsKey(aDMSummaryImport.PFC))
            {
                Instrument_mongo instrument = DataCollectionLibrary.instrumentHashTable_keyadmcode[aDMSummaryImport.PFC];

                aDMSummaryImport.idinstrument = instrument.idinstrument;

                aDMSummaryImport.instrument = instrument;


                subContractType((ADM_Input_Data)aDMSummaryImport,
                    aDMSummaryImport.PSUBTY);


                if (brokerImportFiles == BrokerImportFiles.GMI_IMPORT_CSV_FILES)
                {
                    aDMSummaryImport.strikeInDecimal = Convert.ToDouble(aDMSummaryImport.aDMStrike)
                        * instrument.admfuturepricefactor;
                }
                else if (brokerImportFiles == BrokerImportFiles.ADM_WEB_IMPORT_FILES)
                {
                    aDMSummaryImport.strikeInDecimal =
                        ConversionAndFormatting.convertToTickMovesDouble(aDMSummaryImport.aDMStrike,
                            instrument.optionstrikeincrement,
                            instrument.optionadmstrikedisplay);
                }
                else if (brokerImportFiles == BrokerImportFiles.BACKUP_SAVED_FILE)
                {
                    aDMSummaryImport.strikeInDecimal = Convert.ToDouble(aDMSummaryImport.aDMStrike);
                }


                aDMSummaryImport.strike =
                        ConversionAndFormatting.convertToTickMovesString(aDMSummaryImport.strikeInDecimal,
                            instrument.optionstrikeincrement,
                            instrument.optionstrikedisplay);

            }

        }


        private double getDoubleOutOfStringList(List<String> stringList, ADM_DETAIL_FIELDS summaryField)
        {
#if DEBUG
            try
#endif
            {
                return Convert.ToDouble(stringList[(int)summaryField]);
            }
#if DEBUG
            catch (Exception ex)
            {
                TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
            }
#endif
            return 0;
        }

        private int getIntOutOfStringList(List<String> stringList, ADM_DETAIL_FIELDS summaryField)
        {
#if DEBUG
            try
#endif
            {
                return Convert.ToInt32(stringList[(int)summaryField]);
            }
#if DEBUG
            catch (Exception ex)
            {
                TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
            }
#endif
            return 0;
        }

        private double getDoubleOutOfStringList(List<String> stringList, ADM_SUMMARY_FIELDS summaryField)
        {
#if DEBUG
            try
#endif
            {
                return Convert.ToDouble(stringList[(int)summaryField]);
            }
#if DEBUG
            catch (Exception ex)
            {
                TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
            }
#endif
            return 0;
        }

        private int getIntOutOfStringList(List<String> stringList, ADM_SUMMARY_FIELDS summaryField)
        {
#if DEBUG
            try
#endif
            {
                return Convert.ToInt32(stringList[(int)summaryField]);
            }
#if DEBUG
            catch (Exception ex)
            {
                TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
            }
#endif
            return 0;
        }

        private List<String> readStringToParse(String separatedLine, char separator)
        {
            List<String> listOfStrings = new List<string>();

#if DEBUG
            try
#endif
            {
                String[] separatedArray = separatedLine.Split(separator);

                for (int arrayCnt = 0; arrayCnt < separatedArray.Length; arrayCnt++)
                {
                    String x = separatedArray[arrayCnt].Replace("\"", "");

                    listOfStrings.Add(x); //separatedArray[arrayCnt].Trim());
                }
            }
#if DEBUG
            catch (Exception ex)
            {
                TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
            }
#endif

            return listOfStrings;
        }

        private List<String> readSeparatedLine(String separatedLine, char separator)
        {
            List<String> listOfStrings = new List<string>();

#if DEBUG
            try
#endif
            {
                String[] separatedArray = separatedLine.Split(separator);

                for (int arrayCnt = 0; arrayCnt < separatedArray.Length - 1; arrayCnt++)
                {
                    String x = separatedArray[arrayCnt].Replace("\"", "");

                    listOfStrings.Add(x); //separatedArray[arrayCnt].Trim());
                }



            }
#if DEBUG
            catch (Exception ex)
            {
                TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
            }
#endif

            return listOfStrings;
        }

        public String getNameOfADMPositionImportWebStored()
        {
            return System.IO.Path.Combine(Directory.GetCurrentDirectory(),
                        TradingSystemConstants.FCM_DATA_FOLDER, TradingSystemConstants.FCM_STORED_DATA_FILE);
        }

        public DateTime getFileDateTimeOfADMPositionsWeb(String nameOfStoredFile)
        {
            try
            {
                if (File.Exists(nameOfStoredFile))
                {
                    return File.GetLastWriteTime(nameOfStoredFile);
                }
            }
            catch (Exception ex)
            {
                TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
            }

            return new DateTime(1900, 1, 1);
        }

        public void copyADMStoredDataFile(String[] admInFileNames)
        {
            try
            {
                //                 String fullFile = System.IO.Path.Combine(Directory.GetCurrentDirectory(),
                //                         TradingSystemConstants.ADM_DATA_FOLDER, TradingSystemConstants.ADM_STORED_DATA_FILE);

                String dir = System.IO.Path.Combine(Directory.GetCurrentDirectory(), TradingSystemConstants.FCM_DATA_FOLDER);

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                //System.IO.File.Copy(admInFileName, getNameOfADMPositionImportWebStored(), true);

                //FileOptions fo = new FileOptions();


                System.IO.StreamWriter fs = new System.IO.StreamWriter(getNameOfADMPositionImportWebStored());

                for (int fileCounter = 0; fileCounter < admInFileNames.Length; fileCounter++)
                {

                    bool firstLine = true;



                    if (File.Exists(admInFileNames[fileCounter]))
                    {
                        FileStream fileStream = new FileStream(admInFileNames[fileCounter],
                            FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        StreamReader streamReader = new StreamReader(fileStream);

                        while (!(streamReader.EndOfStream))
                        {
                            String line = streamReader.ReadLine();

                            if (firstLine)
                            {
                                if (fileCounter == 0)
                                {
                                    fs.WriteLine(line);
                                }

                                firstLine = false;

                            }
                            else
                            {
                                fs.WriteLine(line);
                            }


                        }

                        streamReader.Close();

                        fileStream.Close();
                    }
                }

                fs.Close();
            }
            catch (Exception ex)
            {
                TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
            }

        }

        internal void copyADMDataToFile(List<ADMPositionImportWeb> admPositionImportWeb, ImportFileCheck importFileCheck)
        {
            try
            {
                //                 String fullFile = System.IO.Path.Combine(Directory.GetCurrentDirectory(),
                //                         TradingSystemConstants.ADM_DATA_FOLDER, TradingSystemConstants.ADM_STORED_DATA_FILE);

                String dir = System.IO.Path.Combine(Directory.GetCurrentDirectory(), TradingSystemConstants.FCM_DATA_FOLDER);

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }


                System.IO.StreamWriter fs = new System.IO.StreamWriter(getNameOfADMPositionImportWebStored());

                bool firstLine = true;

                StringBuilder stringOut = new StringBuilder();

                for (int lineCounter = 0; lineCounter < admPositionImportWeb.Count; lineCounter++)
                {
                    stringOut.Clear();

                    if (firstLine)
                    {
                        stringOut.Append(Enum.GetName(typeof(BrokerImportFiles), BrokerImportFiles.BACKUP_SAVED_FILE));
                        stringOut.Append("\t");

                        fs.WriteLine(stringOut.ToString());

                        stringOut.Clear();

                        firstLine = false;
                    }




                    stringOut.Append(admPositionImportWeb[lineCounter].RecordType);
                    stringOut.Append("\t");
                    stringOut.Append(admPositionImportWeb[lineCounter].POFFIC);
                    stringOut.Append("\t");
                    stringOut.Append(admPositionImportWeb[lineCounter].PACCT);
                    stringOut.Append("\t");
                    stringOut.Append(admPositionImportWeb[lineCounter].PCUSIP);
                    stringOut.Append("\t");
                    stringOut.Append(admPositionImportWeb[lineCounter].PCUSIP2);
                    stringOut.Append("\t");
                    stringOut.Append(admPositionImportWeb[lineCounter].Description);
                    stringOut.Append("\t");
                    stringOut.Append(admPositionImportWeb[lineCounter].LongQuantity);
                    stringOut.Append("\t");
                    stringOut.Append(admPositionImportWeb[lineCounter].ShortQuantity);
                    stringOut.Append("\t");
                    stringOut.Append(admPositionImportWeb[lineCounter].TradeDate);
                    stringOut.Append("\t");
                    stringOut.Append(admPositionImportWeb[lineCounter].TradePrice / admPositionImportWeb[lineCounter].instrument.admfuturepricefactor);
                    stringOut.Append("\t");
                    stringOut.Append(admPositionImportWeb[lineCounter].WeightedPrice);
                    stringOut.Append("\t");
                    stringOut.Append(admPositionImportWeb[lineCounter].RealTimePrice);
                    stringOut.Append("\t");
                    stringOut.Append(admPositionImportWeb[lineCounter].SettledPrice);
                    stringOut.Append("\t");
                    stringOut.Append(admPositionImportWeb[lineCounter].PrelimPrice);

                    stringOut.Append("\t");
                    stringOut.Append(admPositionImportWeb[lineCounter].Value);
                    stringOut.Append("\t");
                    stringOut.Append(admPositionImportWeb[lineCounter].ClosedValue);
                    stringOut.Append("\t");
                    stringOut.Append(admPositionImportWeb[lineCounter].SettledValue);
                    stringOut.Append("\t");
                    stringOut.Append(admPositionImportWeb[lineCounter].Currency);
                    stringOut.Append("\t");
                    stringOut.Append(admPositionImportWeb[lineCounter].PSUBTY);
                    stringOut.Append("\t");
                    stringOut.Append(admPositionImportWeb[lineCounter].PEXCH);
                    stringOut.Append("\t");
                    stringOut.Append(admPositionImportWeb[lineCounter].PFC);
                    stringOut.Append("\t");
                    stringOut.Append(admPositionImportWeb[lineCounter].strikeInDecimal);
                    stringOut.Append("\t");
                    stringOut.Append(admPositionImportWeb[lineCounter].PCTYM);
                    stringOut.Append("\t");
                    stringOut.Append(admPositionImportWeb[lineCounter].PCARD);
                    stringOut.Append("\t");


                    fs.WriteLine(stringOut.ToString());


                }

                fs.Close();
            }
            catch (Exception ex)
            {
                TSErrorCatch.errorCatchOut(Convert.ToString(this), ex);
            }

        }

        public Asset copyAsset(Asset copyFromAsset)
        {
            Asset asset = new Asset();
            asset.callorput = copyFromAsset.callorput;
            asset.contractname = copyFromAsset.contractname;
            asset.cqgsymbol = copyFromAsset.cqgsymbol;
            asset.expirationdate = copyFromAsset.expirationdate;
            asset.idcontract = copyFromAsset.idcontract;
            asset.idinstrument = copyFromAsset.idinstrument;
            asset.idoption = copyFromAsset.idoption;
            asset.month = copyFromAsset.month;
            asset.monthint = copyFromAsset.monthint;
            asset.name = copyFromAsset.name;
            asset.optionmonth = copyFromAsset.optionmonth;
            asset.optionmonthint = copyFromAsset.optionmonthint;
            asset.optionname = copyFromAsset.optionname;
            asset.optionyear = copyFromAsset.optionyear;
            asset.strikeprice = copyFromAsset.strikeprice;
            asset.year = copyFromAsset.year;
            asset.yearFraction = copyFromAsset.yearFraction;
            asset._type = copyFromAsset._type;

            asset.optioncode = copyFromAsset.optioncode;
            asset.productcode = copyFromAsset.productcode;

            return asset;
        }

        public void copyADMPositionImportWeb(ADMPositionImportWeb copyInto, ADMPositionImportWeb copyFrom)
        {
            copyInto.MODEL_OFFICE_ACCT = copyFrom.MODEL_OFFICE_ACCT;

            copyInto.acctGroup = copyFrom.acctGroup;

            if (copyFrom.asset != null)
            {
                copyInto.asset = copyAsset(copyFrom.asset);
            }

            copyInto.dateTime = copyFrom.dateTime;

            copyInto.idinstrument = copyFrom.idinstrument;

            copyInto.instrument = copyFrom.instrument;

            copyInto.cqgsymbol = copyFrom.cqgsymbol;

            //copyInto.isFuture = copyFrom.isFuture;

            copyInto.callPutOrFuture = copyFrom.callPutOrFuture;

            copyInto.strikeInDecimal = copyFrom.strikeInDecimal;

            copyInto.strike = copyFrom.strike;

            //copyInto.contractInfo = copyFrom.contractInfo;
            copyInto.positionTotals = copyFrom.positionTotals;



            copyInto.modelLots = copyFrom.modelLots;
            copyInto.orderLots = copyFrom.orderLots;
            copyInto.rollIntoLots = copyFrom.rollIntoLots;
            copyInto.rebalanceLots = copyFrom.rebalanceLots;
            copyInto.liveADMRowIdx = copyFrom.liveADMRowIdx;
            copyInto.netContractsEditable = copyFrom.netContractsEditable;


            copyInto.transAvgLongPrice = copyFrom.transAvgLongPrice;
            copyInto.transAvgShortPrice = copyFrom.transAvgShortPrice;

            copyInto.transNetLong = copyFrom.transNetLong;
            copyInto.transNetShort = copyFrom.transNetShort;
            copyInto.Net = copyFrom.Net;



            copyInto.RecordType = copyFrom.RecordType;

            copyInto.POFFIC = copyFrom.POFFIC;
            copyInto.PACCT = copyFrom.PACCT;

            copyInto.PCUSIP = copyFrom.PCUSIP;
            copyInto.PCUSIP2 = copyFrom.PCUSIP2;
            copyInto.Description = copyFrom.Description;
            copyInto.LongQuantity = copyFrom.LongQuantity;
            copyInto.ShortQuantity = copyFrom.ShortQuantity;



            //copyInto.netContractsEditable = copyFrom.netContractsEditable;

            copyInto.WeightedPrice = copyFrom.WeightedPrice;

            //copyInto.AveragePrice = copyFrom.AveragePrice;

            copyInto.RealTimePrice = copyFrom.RealTimePrice;
            copyInto.SettledPrice = copyFrom.SettledPrice;
            copyInto.PrelimPrice = copyFrom.PrelimPrice;
            copyInto.Value = copyFrom.Value;
            copyInto.ClosedValue = copyFrom.ClosedValue;
            copyInto.SettledValue = copyFrom.SettledValue;
            copyInto.Currency = copyFrom.Currency;
            copyInto.PSUBTY = copyFrom.PSUBTY;
            copyInto.PEXCH = copyFrom.PEXCH;
            copyInto.PFC = copyFrom.PFC;

            copyInto.aDMStrike = copyFrom.aDMStrike;

            copyInto.PCTYM = copyFrom.PCTYM;
            copyInto.PCTYM_dateTime = copyFrom.PCTYM_dateTime;

            copyInto.PCARD = copyFrom.PCARD;
        }

        public void saveADMStrategyInfo(OptionSpreadManager optionSpreadManager)
        {
            String dir = System.IO.Path.Combine(Directory.GetCurrentDirectory(), TradingSystemConstants.INITIALIZE_CONFIG_DIRECTORY);

            SaveOutputFile sof = new SaveOutputFile(dir);

            StringBuilder admFileName = new StringBuilder();

            admFileName.Append(DataCollectionLibrary.initializationParms.idPortfolioGroup);
            admFileName.Append("_");
            admFileName.Append(TradingSystemConstants.FCM_EXCLUDE_CONTRACT_INFO_FILE);


            sof.createConfigFile(admFileName.ToString());

            ////sof.writeTextLineFile("test");

            createADMLiveInfoLine(optionSpreadManager, sof);

            sof.closeAndSaveFile();


        }




        public void readADMExcludeContractInfo(OptionSpreadManager optionSpreadManager)
        {
            StringBuilder admFileName = new StringBuilder();

            admFileName.Append(DataCollectionLibrary.initializationParms.idPortfolioGroup);
            admFileName.Append("_");
            admFileName.Append(TradingSystemConstants.FCM_EXCLUDE_CONTRACT_INFO_FILE);

            String fileName = System.IO.Path.Combine(Directory.GetCurrentDirectory(), TradingSystemConstants.INITIALIZE_CONFIG_DIRECTORY,
                admFileName.ToString());

            SaveOutputFile sof = new SaveOutputFile();

            sof.openReadFile(fileName);

            String line = sof.readStoredADMStrategyInfoList_1Line();

            //int spreadId = -1;

            while (line != null)
            {
                List<String> separatedLine = readSeparatedLine(line, ',');

                if (separatedLine.Count <= 2)
                {
                    optionSpreadManager.zeroPriceContractList.Add(separatedLine[0]);
                }
                else if (separatedLine.Count > 2)
                {
                    if (Convert.ToBoolean(separatedLine[1]))
                    {
                        optionSpreadManager.zeroPriceContractList.Add(separatedLine[0]);
                    }
                    else if (Convert.ToBoolean(separatedLine[2]))
                    {
                        optionSpreadManager.exceptionContractList.Add(separatedLine[0]);
                    }
                }

                //spreadId =
                //    readLineFillADMLiveStrategyInfo(optionSpreadManager.liveADMStrategyInfoList, separatedLine, spreadId);

                line = sof.readStoredADMStrategyInfoList_1Line();
            }

            sof.closeReadingStreams();
        }



        private void createADMLiveInfoLine(OptionSpreadManager optionSpreadManager,
            SaveOutputFile sof)
        {
            StringBuilder csvLine = new StringBuilder();

            //optionSpreadManager.admPositionImportWebListForCompare

            for (int instrumentCount = 0;
                instrumentCount < FCM_DataImportLibrary.FCM_PostionList_forCompare.Count;
                instrumentCount++)
            {

                //if (optionSpreadManager.admPositionImportWebListForCompare[instrumentCount].exclude)
                if (optionSpreadManager.zeroPriceContractList.Contains(
                        FCM_DataImportLibrary.FCM_PostionList_forCompare[instrumentCount].asset.cqgsymbol))
                {

                    csvLine.Clear();

                    csvLine.Append(FCM_DataImportLibrary.FCM_PostionList_forCompare[instrumentCount].asset.cqgsymbol);
                    csvLine.Append(",true,false,");

                    sof.writeTextLineFile(csvLine.ToString());
                }
                else if (optionSpreadManager.exceptionContractList.Contains(
                        FCM_DataImportLibrary.FCM_PostionList_forCompare[instrumentCount].asset.cqgsymbol))
                {

                    csvLine.Clear();

                    csvLine.Append(FCM_DataImportLibrary.FCM_PostionList_forCompare[instrumentCount].asset.cqgsymbol);
                    csvLine.Append(",false,true,");

                    sof.writeTextLineFile(csvLine.ToString());
                }
            }


        }


    }
}
