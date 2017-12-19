using MongoDB.Bson;
using MongoDB.Driver;
using RealtimeSpreadMonitor.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace RealtimeSpreadMonitor.Mongo
{
    public class MongoDBConnectionAndSetup
    {
        protected static IMongoClient _client;
        protected static IMongoDatabase _database;

        protected static IMongoClient _client_live_futures;
        protected static IMongoDatabase _database_live_futures;

        private static IMongoCollection<Instrument_mongo> _instrumentCollection;

        private static IMongoCollection<Instrument_Info> _instrumentInfoCollection;

        private static IMongoCollection<Exchange_mongo> _exchangeCollection;

        private static IMongoCollection<Account> _accountCollection;

        private static IMongoCollection<AccountPosition> _accountPositionCollection;

        private static IMongoCollection<AccountPosition> _accountPositionArchiveCollection;

        private static IMongoCollection<Contract_mongo> _contractCollection;

        private static IMongoCollection<Option_mongo> _optionCollection;

        private static IMongoCollection<PortfolioAllocation_Mongo> _portfolioCollection;

        private static IMongoCollection<Futures_Contract_Settlements> _future_contract_settlements;

        private static IMongoCollection<Options_Data> _option_data;

        private static IMongoCollection<OptionInputData> _option_input_data;

        private static IMongoCollection<Futures_Contract_Minutebars> _futures_live_data;

        //private static IMongoCollection<PortfolioAllocation> _portfolioCollectionQuery;

        /// <summary>
        /// initializes and get all the needed collections
        /// </summary>
        static MongoDBConnectionAndSetup()
        {
            _client = new MongoClient(
                System.Configuration.ConfigurationManager.ConnectionStrings["DefaultMongoConnection"].ConnectionString);

            _database = _client.GetDatabase(System.Configuration.ConfigurationManager.AppSettings["MongoDbName"]);


            _client_live_futures = new MongoClient(
                System.Configuration.ConfigurationManager.ConnectionStrings["MongoConnection_Live"].ConnectionString);

            _database_live_futures = _client_live_futures.GetDatabase(System.Configuration.ConfigurationManager.AppSettings["MongoDbName_V1_LIVE"]);


            _instrumentCollection = _database.GetCollection<Instrument_mongo>(
                System.Configuration.ConfigurationManager.AppSettings["MongoInstrumentCollection"]);

            _instrumentInfoCollection = _database.GetCollection<Instrument_Info>(
                System.Configuration.ConfigurationManager.AppSettings["MongoInstrumentInfoCollection"]);

            _exchangeCollection = _database.GetCollection<Exchange_mongo>(
                System.Configuration.ConfigurationManager.AppSettings["MongoExchangeCollection"]);

            _accountCollection = _database.GetCollection<Account>(
                System.Configuration.ConfigurationManager.AppSettings["MongoAccountCollection"]);

            _accountPositionCollection = _database.GetCollection<AccountPosition>(
                System.Configuration.ConfigurationManager.AppSettings["MongoAccountPositionCollection"]);

            _accountPositionCollection.Indexes.CreateOneAsync(
                Builders<AccountPosition>
                .IndexKeys.Ascending(_ => _.name).Ascending(_ => _.date_now));

            _accountPositionArchiveCollection = _database.GetCollection<AccountPosition>(
                System.Configuration.ConfigurationManager.AppSettings["MongoAccountPositionArchiveCollection"]);

            _accountPositionArchiveCollection.Indexes.CreateOneAsync(
                Builders<AccountPosition>
                .IndexKeys.Ascending(_ => _.name).Ascending(_ => _.date_now));

            _contractCollection = _database.GetCollection<Contract_mongo>(
                System.Configuration.ConfigurationManager.AppSettings["MongoContractCollection"]);


            _optionCollection = _database.GetCollection<Option_mongo>(
                System.Configuration.ConfigurationManager.AppSettings["MongoOptionCollection"]);


            _portfolioCollection = _database.GetCollection<PortfolioAllocation_Mongo>(
                System.Configuration.ConfigurationManager.AppSettings["MongoAccountsPortfolio"]);

            _future_contract_settlements = _database.GetCollection<Futures_Contract_Settlements>(
                System.Configuration.ConfigurationManager.AppSettings["MongoFuturesContractSettlementsCollection"]);

            _option_data = _database.GetCollection<Options_Data>(
                System.Configuration.ConfigurationManager.AppSettings["MongoOptionsDataCollection"]);

            _option_input_data = _database.GetCollection<OptionInputData>(
                System.Configuration.ConfigurationManager.AppSettings["MongoOptionsInputDataCollection"]);

            //_portfolioCollectionQuery = _database.GetCollection<PortfolioAllocation>(
            //    System.Configuration.ConfigurationManager.AppSettings["MongoAccountsPortfolio"]);

            _futures_live_data = _database_live_futures.GetCollection<Futures_Contract_Minutebars>(
                System.Configuration.ConfigurationManager.AppSettings["Mongo_live_contract_minute_bars_collection"]);
        }

        internal static List<Account> GetAccountInfoFromMongo(List<string> accountList)
        {
            try
            {
                var builder = Builders<Account>.Filter;
                var filter = builder.In(x => x.name, accountList);

                return _accountCollection.Find(filter).ToList();
            }
            catch
            {
                return new List<Account>();
            }
        }

        internal static List<AccountPosition> GetAccountPositionsInfoFromMongo(List<string> accountList)
        {
            try
            {
                var builder = Builders<AccountPosition>.Filter;
                var filter = builder.In(x => x.name, accountList);

                return _accountPositionCollection.Find(filter).ToList();
            }
            catch
            {
                return new List<AccountPosition>();
            }
        }

        internal static AccountPosition GetAccountArchivePositionsInfoFromMongo(string accountName)
        {
            try
            {
                var builder = Builders<AccountPosition>.Filter;
                var filter = builder.Eq(x => x.name, accountName);
                //var filter = builder.And(builder.Eq(x => x.name, accountName), builder.Eq('date_now', '2017-02-10'));

                //var filter = builder.And(builder.Eq("name", accountName),
                //            builder.Lt("date_now", new DateTime(2017,2,10)));

                //return _accountPositionArchiveCollection.Find(filter)
                //    .Sort(Builders<AccountPosition>.Sort.Descending("date_now")).Limit(2).ToList();

                return _accountPositionArchiveCollection.Find(filter)
                    .Sort(Builders<AccountPosition>.Sort.Descending("date_now")).Limit(1).First();
            }
            catch
            {
                return null;
            }
        }

        internal static List<Instrument_mongo> GetInstrumentListFromMongo(List<long> instrumentIdList)//List<long> instrumentIdList)
        {
            try
            {
                var builder = Builders<Instrument_mongo>.Filter;
                var filter = builder.In(x => x.idinstrument, instrumentIdList);
                //var filter = builder.In(x => x.exchangesymbol, instrumentExchangeSymbolList);

                return _instrumentCollection.Find(filter).ToList();
            }
            catch
            {
                return new List<Instrument_mongo>();
            }
        }

        internal static List<Instrument_Info> GetInstrumentInfoListFromMongo(List<long> instrumentIdList)
        {
            try
            {
                var builder = Builders<Instrument_Info>.Filter;
                var filter = builder.In(x => x.idinstrument, instrumentIdList);

                return _instrumentInfoCollection.Find(filter).ToList();
            }
            catch
            {
                return new List<Instrument_Info>();
            }
        }

        internal static List<Exchange_mongo> GetExchangeListFromMongo(List<long> exchangeIdList)
        {
            try
            {
                var builder = Builders<Exchange_mongo>.Filter;
                var filter = builder.In(x => x.idexchange, exchangeIdList);

                return _exchangeCollection.Find(filter).ToList();
            }
            catch
            {
                return new List<Exchange_mongo>();
            }
        }

        public static List<Contract_mongo> GetContracts(DateTime todaysDate, long idinstrument)
        {
            List<Contract_mongo> contractQuery = null;

            try
            {
                //foreach (Instrument_mongo instrumentx in DataCollectionLibrary.instrumentList)
                {
                    var builder = Builders<Contract_mongo>.Filter;

                    var filterForContracts = builder.And(builder.Eq("idinstrument", idinstrument),
                            builder.Gte("expirationdate", todaysDate.AddDays(-10)));

                    contractQuery = _contractCollection.Find(filterForContracts)
                        .Sort(Builders<Contract_mongo>.Sort.Ascending("expirationdate"))
                        .Limit(4)
                        .ToList<Contract_mongo>();


                    //Mapper.Initialize(cfg => cfg.CreateMap<Contract_mongo, Asset>());

                    //foreach (Contract_mongo contract in contractQuery)
                    //{
                    //    Console.WriteLine(contract.idcontract + " " + contract.idinstrument + " " + contract.contractname
                    //        + " " + contract.expirationdate);

                    //    Asset asset = Mapper.Map<Asset>(contract);

                    //    asset._type = ASSET_TYPE_MONGO.fut.ToString();

                    //    MongoDB_OptionSpreadExpression mose = new MongoDB_OptionSpreadExpression(
                    //        OPTION_SPREAD_CONTRACT_TYPE.FUTURE,
                    //            OPTION_EXPRESSION_TYPES.SPREAD_LEG_PRICE);

                    //    mose.asset = asset;

                    //    DataCollectionLibrary.optionSpreadExpressionList.Add(mose);

                    //    var key = Tuple.Create(asset.idcontract, asset._type);

                    //    DataCollectionLibrary.optionSpreadExpressionHashTable_key_Id_Type
                    //        .TryAdd(key, mose);

                    //}
                }

            }
            catch (InvalidOperationException)
            {
            }

            return contractQuery;
        }

        public static Contract_mongo GetContract(long idinstrument,
            int contractyear, int contractmonthint)
        {
            Contract_mongo contractQuery = null;

            try
            {
                var builder = Builders<Contract_mongo>.Filter;

                var filterForContracts = builder.And(builder.Eq("idinstrument", idinstrument),
                       builder.Eq("year", contractyear),
                       builder.Eq("monthint", contractmonthint));

                contractQuery = _contractCollection.Find(filterForContracts).First();

            }
            catch (InvalidOperationException)
            {
            }

            return contractQuery;
        }

        public static Option_mongo GetOption(int optionmonthint,
            int optionyear, long idinstrument, string PSUBTY, double strikeprice, string optioncode, bool useOptionCode)
        {
            Option_mongo optionQuery = null;

            try
            {
                var builder = Builders<Option_mongo>.Filter;

                FilterDefinition<Option_mongo> filterForOptions;
                if (useOptionCode)
                {
                    filterForOptions = builder.And(
                            builder.Eq("optionmonthint", optionmonthint),
                            builder.Eq("optionyear", optionyear),
                            builder.Eq("idinstrument", idinstrument),
                            builder.Eq("callorput", PSUBTY),
                            builder.Eq("optioncode", optioncode),
                            builder.Gte("strikeprice", strikeprice));
                }
                else
                {
                    filterForOptions = builder.And(
                            builder.Eq("optionmonthint", optionmonthint),
                            builder.Eq("optionyear", optionyear),
                            builder.Eq("idinstrument", idinstrument),
                            builder.Eq("callorput", PSUBTY),
                            builder.Gte("strikeprice", strikeprice));
                }

                optionQuery = _optionCollection.Find(filterForOptions) //.First();
                    .Sort(Builders<Option_mongo>.Sort.Ascending("strikeprice")).First();


            }
            catch (InvalidOperationException)
            {
                TSErrorCatch.debugWriteOut("Error");
            }

            return optionQuery;
        }


        public static PortfolioAllocation_Mongo GetAccountsPortfolio(int idportfoliogroup)
        {
            PortfolioAllocation_Mongo portfolioAllocation = null;

            try
            {

                var builder = Builders<PortfolioAllocation_Mongo>.Filter;

                var filterForPortfolio = builder.Eq("idportfoliogroup", idportfoliogroup);

                portfolioAllocation
                    = _portfolioCollection.Find(filterForPortfolio)

                    .Sort(Builders<PortfolioAllocation_Mongo>.Sort.Ascending("account"))
                    .First<PortfolioAllocation_Mongo>();


            }
            catch (Exception e)
            {
                MessageBox.Show("MongoDB error " + e.Message);
                Console.Write(e.Message);
            }

            return portfolioAllocation;
        }

        internal static void InsertPortfolioToMongo(PortfolioAllocation_Mongo portfolioAllocation_Mongo)
        {
            try
            {
                _portfolioCollection.InsertOne(portfolioAllocation_Mongo);
            }
            catch (Exception)
            {

            }
        }

        public static Futures_Contract_Settlements GetContractLatestSettlement(long idcontract)
        {
            Futures_Contract_Settlements contractSettlement = null;

            try
            {
                var builder = Builders<Futures_Contract_Settlements>.Filter;

                FilterDefinition<Futures_Contract_Settlements> filterForContract
                    = builder.Eq("idcontract", idcontract);


                contractSettlement = _future_contract_settlements.Find(filterForContract)
                    .Sort(Builders<Futures_Contract_Settlements>.Sort.Descending("date")).First();

            }
            catch (InvalidOperationException)
            {
                TSErrorCatch.debugWriteOut("Error");
            }

            return contractSettlement;
        }

        public static Options_Data GetOptionLatestSettlementAndImpliedVol(long idoption)
        {
            Options_Data optionSettlements = null;

            try
            {
                var builder = Builders<Options_Data>.Filter;

                FilterDefinition<Options_Data> filterForOption
                    = builder.Eq("idoption", idoption);


                optionSettlements = _option_data.Find(filterForOption)
                    .Sort(Builders<Options_Data>.Sort.Descending("datetime")).First();

            }
            catch (InvalidOperationException)
            {
                TSErrorCatch.debugWriteOut("Error");
            }

            return optionSettlements;
        }

        public static OptionInputData GetRiskFreeRate(long idoptioninputsymbol)
        {
            OptionInputData riskFreeRate = null;

            try
            {
                var builder = Builders<OptionInputData>.Filter;

                FilterDefinition<OptionInputData> filterOptionInputSymbol
                    = builder.Eq("idoptioninputsymbol", idoptioninputsymbol);


                riskFreeRate = _option_input_data.Find(filterOptionInputSymbol)
                    .Sort(Builders<OptionInputData>.Sort.Descending("optioninputdatetime")).First();

            }
            catch (InvalidOperationException)
            {
                TSErrorCatch.debugWriteOut("Error");
            }

            return riskFreeRate;
        }

        public static List<BsonDocument> GetFuturesContractMinutebars(List<long> contractId_List, DateTime start)
        {
            try
            {
                var builder = Builders<Futures_Contract_Minutebars>.Filter;

                //var start = new DateTime(2017, 11, 15);
                //var start = DateTime.Now.Date;

                FilterDefinition<Futures_Contract_Minutebars> filterOptionInputSymbol
                    = builder.And(
                        builder.Gte("bartime", start),
                        builder.In("idcontract", contractId_List));

                

                var group = new BsonDocument
                 {
                    {"_id", "$idcontract"},
                    {"bartime",new BsonDocument{

                                {"$last", "$bartime"}

                        }
                    },
                    {"close",new BsonDocument{
                            
                                {"$last", "$close"}
                            
                        }
                    }
                 };

                var sort_1 = new BsonDocument { { "bartime", 1 } };
                var sort_2 = new BsonDocument { { "idcontract", 1 } };

                var aggregate = _futures_live_data.Aggregate().Match(filterOptionInputSymbol).Sort(sort_1)
                    .Sort(sort_2).Group(group);

                return aggregate.ToListAsync().Result;
                
            }
            catch (Exception error)
            {
                TSErrorCatch.debugWriteOut(error.ToString());
            }

            return null;
        }






        public double SquareRoot(double input)
        {
            return input / 5;
        }

    }

}
