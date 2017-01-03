using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using MongoDB.Bson.Serialization;
using RealtimeSpreadMonitor.Model;
using AutoMapper;

namespace RealtimeSpreadMonitor.Mongo
{
    class MongoDBConnectionAndSetup
    {
        protected static IMongoClient _client;
        protected static IMongoDatabase _database;

        private static IMongoCollection<Instrument_mongo> _instrumentCollection;

        private static IMongoCollection<Exchange_mongo> _exchangeCollection;

        private static IMongoCollection<Account> _accountCollection;

        private static IMongoCollection<AccountPosition> _accountPositionCollection;

        private static IMongoCollection<AccountPosition> _accountPositionArchiveCollection;

        private static IMongoCollection<Contract_mongo> _contractCollection;

        private static IMongoCollection<Option_mongo> _optionCollection;

        private static IMongoCollection<PortfolioAllocation_Mongo> _portfolioCollection;

        //private static IMongoCollection<PortfolioAllocation> _portfolioCollectionQuery;

        /// <summary>
        /// initializes and get all the needed collections
        /// </summary>
        static MongoDBConnectionAndSetup()
        {
            _client = new MongoClient(
                System.Configuration.ConfigurationManager.ConnectionStrings["DefaultMongoConnection"].ConnectionString);

            _database = _client.GetDatabase(System.Configuration.ConfigurationManager.AppSettings["MongoDbName"]);

            _instrumentCollection = _database.GetCollection<Instrument_mongo>(
                System.Configuration.ConfigurationManager.AppSettings["MongoInstrumentCollection"]);

            _exchangeCollection = _database.GetCollection<Exchange_mongo>(
                System.Configuration.ConfigurationManager.AppSettings["MongoExchangeCollection"]);

            _accountCollection = _database.GetCollection<Account>(
                System.Configuration.ConfigurationManager.AppSettings["MongoAccountCollection"]);

            _accountPositionCollection = _database.GetCollection<AccountPosition>(
                System.Configuration.ConfigurationManager.AppSettings["MongoAccountPositionCollection"]);

            _accountPositionArchiveCollection = _database.GetCollection<AccountPosition>(
                System.Configuration.ConfigurationManager.AppSettings["MongoAccountPositionArchiveCollection"]);


            _contractCollection = _database.GetCollection<Contract_mongo>(
                System.Configuration.ConfigurationManager.AppSettings["MongoContractCollection"]);


            _optionCollection = _database.GetCollection<Option_mongo>(
                System.Configuration.ConfigurationManager.AppSettings["MongoOptionCollection"]);


            _portfolioCollection = _database.GetCollection<PortfolioAllocation_Mongo>(
                System.Configuration.ConfigurationManager.AppSettings["MongoAccountsPortfolio"]);



            //_portfolioCollectionQuery = _database.GetCollection<PortfolioAllocation>(
            //    System.Configuration.ConfigurationManager.AppSettings["MongoAccountsPortfolio"]);
        }

        internal static List<Account> GetAccountInfoFromMongo(List<string> accountList)
        {
            var builder = Builders<Account>.Filter;
            var filter = builder.In(x => x.name, accountList);

            return _accountCollection.Find(filter).ToList();
        }

        internal static List<AccountPosition> GetAccountPositionsInfoFromMongo(List<string> accountList)
        {
            var builder = Builders<AccountPosition>.Filter;
            var filter = builder.In(x => x.name, accountList);

            return _accountPositionCollection.Find(filter).ToList();
        }

        internal static List<AccountPosition> GetAccountArchivePositionsInfoFromMongo(string accountName)
        {
            var builder = Builders<AccountPosition>.Filter;
            var filter = builder.Eq(x => x.name, accountName);

            return _accountPositionArchiveCollection.Find(filter)
                .Sort(Builders<AccountPosition>.Sort.Descending("date_now")).Limit(2).ToList();
        }

        internal static List<Instrument_mongo> GetInstrumentListFromMongo(List<long> instrumentIdList)
        {
            var builder = Builders<Instrument_mongo>.Filter;
            var filter = builder.In(x => x.idinstrument, instrumentIdList);

            return _instrumentCollection.Find(filter).ToList();
        }

        internal static List<Exchange_mongo> GetExchangeListFromMongo(List<long> exchangeIdList)
        {
            var builder = Builders<Exchange_mongo>.Filter;
            var filter = builder.In(x => x.idexchange, exchangeIdList);

            return _exchangeCollection.Find(filter).ToList();
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
            int optionyear, long idinstrument, string PSUBTY, double strikeprice)
        {
            Option_mongo optionQuery = null;

            try
            {
                var builder = Builders<Option_mongo>.Filter;

                var filterForOptions = builder.And(
                        builder.Eq("optionmonthint", optionmonthint),
                        builder.Eq("optionyear", optionyear),
                        builder.Eq("idinstrument", idinstrument),
                        builder.Eq("callorput", PSUBTY),
                        builder.Gte("strikeprice", strikeprice));

                optionQuery = _optionCollection.Find(filterForOptions).First();


            }
            catch (InvalidOperationException)
            {
            }

            return optionQuery;
        }


        public static PortfolioAllocation_Mongo GetAccountsPortfolio(int idportfoliogroup)
        {
            PortfolioAllocation_Mongo portfolioAllocation = null;

            try
            {
                //foreach (Instrument_mongo instrumentx in DataCollectionLibrary.instrumentList)
                {
                    var builder = Builders<PortfolioAllocation_Mongo>.Filter;

                    var filterForPortfolio = builder.Eq("idportfoliogroup", idportfoliogroup);

                    portfolioAllocation
                        = _portfolioCollection.Find(filterForPortfolio).First<PortfolioAllocation_Mongo>();
                    //.ToList<PortfolioAllocation>();




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

            return portfolioAllocation;
        }

        internal static void InsertPortfolioToMongo(PortfolioAllocation_Mongo portfolioAllocation_Mongo)
        {
            try
            {
                _portfolioCollection.InsertOne(portfolioAllocation_Mongo);
            }
            catch (Exception e)
            {

            }
        }

    }

}
