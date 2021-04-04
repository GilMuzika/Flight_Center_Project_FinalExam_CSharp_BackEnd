﻿using EasyNetQ;
using Flight_Center_Project_FinalExam_BL.RedisAuxiliaries;
using Flight_Center_Project_FinalExam_DAL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Flight_Center_Project_FinalExam_BL
{
    public class AnonimousUserFacade : FacadeBase, IAnonymousUserFacade
    {
        public List<AirlineCompany> GetAllAirlineCompanies()
        {
            return _airlineDAO.GetAll();
        }

        public AirlineCompany GetOneAirlineCompaniy(long ID)
        {
            return _airlineDAO.Get(ID);
        }

        public Country GetOneCountry(long ID)
        {
            return _countryDAO.Get(ID);
        }

        public List<Flight> GetAllFlights()
        {
            return _flightDAO.GetAll();
        }

        public List<Flight> GetAllFlightsByAirlineName(long airlineUtilitiClassUserId)
        {
            return _flightDAO.GetAllFlightsByAirlineName(airlineUtilitiClassUserId);
        }
        public Task<List<Flight>> GetAllFlightsByAirlineNameAsync(long airlineUtilitiClassUserId)
        {
            return _flightDAO.GetAllFlightsByAirlineNameAsync(airlineUtilitiClassUserId);
        }

        /*public override List<T> GetAll()
        {

            ExecuteCurrentMethosProcedure execute = (out string commandtextStoredProcedureName, out string commandTextForTextMode, out Dictionary<string, object> storedProcedureParameters) =>
            {
                commandTextForTextMode = $"select * from {GetTableName(typeof(T))}";
                commandtextStoredProcedureName = "DAO_BASE_GetAll_METHOD_QUERY";

                storedProcedureParameters = new Dictionary<string, object>();
                storedProcedureParameters.Add("TABLE_NAME", GetTableName(typeof(T)));
            };

            //return RunToRead(executeCurrentMethosProcedure: execute, commandType: CommandType.StoredProcedure);
            return Run(() => { return ReadFromDb(); }, executeCurrentMethosProcedure: execute, commandType: CommandType.Text);
        }*/

        public List<Flight> GetAllFlightsThatTakeOffInSomeTimeFromNow(TimeSpan someTime, RazorViewStatus status)
        {
            if (status == RazorViewStatus.Landings)
            {
                List<Flight> oveallLandingFlights;
                List<Flight> willLandFlights;

                //first of all, flights that already landed
                oveallLandingFlights = _flightDAO.GetAllFlightsThatTakeOffInSomeTimeFromNow(someTime, status, RazorViewStatus.Future);                
                //second of all, flights tht will land in 4 hours
                willLandFlights = _flightDAO.GetAllFlightsThatTakeOffInSomeTimeFromNow(new TimeSpan(4, 0, 0), status, RazorViewStatus.Past); 
                
                oveallLandingFlights.AddRange(willLandFlights);

                return oveallLandingFlights;
            }

                return _flightDAO.GetAllFlightsThatTakeOffInSomeTimeFromNow(someTime, status, RazorViewStatus.Future);            
        }



        /// <summary>
        /// Builds infrastructure for Get() and GetAll() generic types
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private Dictionary<string, DAO<T>> BuildCorrelationDictionary<T>() where T : class, IPoco, new()
        {
            Dictionary<string, DAO<T>> correlation = new Dictionary<string, DAO<T>>();
            correlation.Add(typeof(Administrator).Name, _administratorDAO as DAO<T>);
            correlation.Add(typeof(AirlineCompany).Name, _airlineDAO as DAO<T>);
            correlation.Add(typeof(Country).Name, _countryDAO as DAO<T>);
            correlation.Add(typeof(Customer).Name, _customerDAO as DAO<T>);
            correlation.Add(typeof(Flight).Name, _flightDAO as DAO<T>);
            correlation.Add(typeof(Ticket).Name, _ticketDAO as DAO<T>);
            correlation.Add(typeof(Admin_Value).Name, _adminValuesDAO as DAO<T>);
            correlation.Add(typeof(Utility_class_User).Name, _utility_Class_UserDAO as DAO<T>);

            return correlation;
        }

        public async Task<string> PleaseGiveMeOneAsync<T>(long ID) where T : class, IPoco, new()
        {

            Task<string> tsk = Task.Run(async () => {
                //T one = this.Get<T>(ID);
                T one = await this.GetAsync<T>(ID);

                //the subscription id here is the current method name            
                string rabbitSubscriptionIdInternal = System.Reflection.MethodBase.GetCurrentMethod().Name;

                //using (var bus = RabbitHutch.CreateBus("host=localhost;virtualHost=stndard_vhost;username=standard_user;password=guest"))
                using (var bus = RabbitHutch.CreateBus("host=localhost"))
                {
                    await bus.PublishAsync<T>(one, rabbitSubscriptionIdInternal);
                }
                return rabbitSubscriptionIdInternal;
            });

            return await tsk;
        }

        public Task PleaseGiveMeAll<T>(out string rabbitSubscriptionId) where T : class, IPoco, new()
        {
            List<T> all = this.GetAll<T>();

            //the subscription id here is the current method name
            string rabbitSubscriptionIdInternal = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task tsk = new Task(() => { });
            //using (var bus = RabbitHutch.CreateBus("host=localhost;virtualHost=stndard_vhost;username=standard_user;password=guest"))
            using (var bus = RabbitHutch.CreateBus("host=localhost"))
            {
                bus.Publish<List<T>>(all, rabbitSubscriptionIdInternal);
            }
            rabbitSubscriptionId = rabbitSubscriptionIdInternal;
            return tsk;
        }

        /// <summary>
        /// Generic GetAll
        /// </summary>
        /// <typeparam name="T">type argument</typeparam>
        /// <returns></returns>
        public List<T> GetAll<T>() where T : class, IPoco, new()
        {
            Dictionary<string, DAO<T>> correlation = BuildCorrelationDictionary<T>();          

            return correlation[typeof(T).Name].GetAll();
        }

        /// <summary>
        /// Generic Get
        /// </summary>
        /// <typeparam name="T">type argument</typeparam>
        /// <returns></returns>
        public T Get<T>(long Id) where T : class, IPoco, new()
        {
            Dictionary<string, DAO<T>> correlation = BuildCorrelationDictionary<T>();

            return correlation[typeof(T).Name].Get(Id);
        }
        public T GetByAny<T>(object getByWhat, string getByWhatProperyName) where T : class, IPoco, new()
        {
            Dictionary<string, DAO<T>> correlation = BuildCorrelationDictionary<T>();

            return correlation[typeof(T).Name].GetByAny(getByWhat, getByWhatProperyName);
        }

        public async Task<T> GetAsync<T>(long Id) where T : class, IPoco, new()
        {
            Dictionary<string, DAO<T>> correlation = BuildCorrelationDictionary<T>();

            return await correlation[typeof(T).Name].GetAsync(Id);
        }

        public T GetSomethingInOneTableBySomethingInAnother<T>(object byWhatInOneTable, string byWhatInOneTable_columnName, string byWahatInAnotherTable_columnName, int anotherPocoTypePropertyNumber, Type anotherPocoType) where T : class, IPoco, new()
        {
            Dictionary<string, DAO<T>> correlation = BuildCorrelationDictionary<T>();

            return correlation[typeof(T).Name].GetSomethingInOneTableBySomethingInAnother(byWhatInOneTable, byWhatInOneTable_columnName, byWahatInAnotherTable_columnName, anotherPocoTypePropertyNumber, anotherPocoType);
        }

        public Dictionary<Flight, int> GetAllFlightsVacancy()
        {
            return _flightDAO.GetAllFlightVacancty();
        }

        public Flight GetFlightByDepartureDate(DateTime departureDate)
        {
            return _flightDAO.GetFlightByDepartureDate(departureDate);
        }

        //GetFlightsByAirligneName
        /// <summary>
        /// Redis implemented here
        /// </summary>
        /// <param name="searchQuery"></param>
        /// <param name="searchCriterion"></param>
        /// <returns></returns>
        public async Task<List<Flight>> GetFlightsByQuery(string searchQuery, SearchFlightsCriterion searchCriterion)
        {
            const string LANDING_QUERY_REDIS_KEY = "aaasssdddfffggghhhjjjkkklll";

            //List<Flight> flights = null;

            IRedisObject result = RedisDataAccess.GetWithTimeStamp(LANDING_QUERY_REDIS_KEY);
            //IRedisObject result = null;
            if (result == null || DateTime.Now.Subtract(result.LastUpdateTime).Seconds > 1)
            {

                List<Flight> flights = await _flightDAO.GetFlightsByQuery(searchQuery, searchCriterion);

                string flightsAsJsonStr = JsonConvert.SerializeObject(flights);
                RedisDataAccess.SaveWithTimeStamp(LANDING_QUERY_REDIS_KEY, flightsAsJsonStr);
                return flights;
            }
            else
            {
                return JsonConvert.DeserializeObject<List<Flight>>(result.JsonData);
            }

        }

        //09/08/20 - Preloadings
        public List<string> PreloadAllAirlineNames()
        {
            return _airlineDAO.PreloadAll<string>("NAME");
        }
        public List<string> PreloadAllCountryNames()
        {
            return _countryDAO.PreloadAll<string>("NAME");
        }
        public List<string> PreloadingAdornings()
        {
            return _airlineDAO.PreloadAdornings();
        }


        /// <summary>
        /// returns dictionary with all the flights as keys and their respective numbers of remaining tickets as values
        /// </summary>
        /// <returns></returns>
        public Flight GetFlightByDestinationCountry(Country destinationCountry)
        {
            return _flightDAO.getFlightByDestinationCountry(destinationCountry);
        }

        public Flight GetFlightByDestinationCountry(int countryCode)
        {
            return _flightDAO.getFlightByDestinationCountry(_countryDAO.Get(countryCode));
        }

        public Flight GetFlightById(long ID)
        {
            return _flightDAO.GetFlightById(ID);
        }

        public Flight GetFlightByLandingDate(DateTime landingDate)
        {
            return _flightDAO.GetFlightByLandingDate(landingDate);
        }

        public Flight GetFlightByOriginCountry(Country originCountry)
        {
            return _flightDAO.GetFlightByOriginCountry(originCountry);
        }

        public Flight GetFlightByOriginCountry(int countryCode)
        {            
            return _flightDAO.GetFlightByOriginCountry(_countryDAO.Get(countryCode));
        }

        protected bool CheckToken<T>(LoginToken<T> token) where T : class, IPoco, new()
        {
            if (token == null || token.ActualUser == null) return false;

            if (!(token.ActualUser is IPoco)) return false;            

            if (!IsUserExists(token.ActualUser)) throw new UserDoesntExistsException<T>(token.ActualUser);

            var allUsers = _utility_Class_UserDAO.GetAll();

            foreach(var s_AsUtilityUser in allUsers)
            {
                if (s_AsUtilityUser.PASSWORD.Equals(token.UserAsUser.PASSWORD)) return true; 
            }            
            throw new WrongPasswordException(token.UserAsUser.PASSWORD);
        }

        protected bool IsUserExists<T>(T user) where T : class, IPoco, new()
        {
            bool isExists = false;

            Dictionary<string, UserBaseMSSQLDAO<T>> correlation = new Dictionary<string, UserBaseMSSQLDAO<T>>();
            correlation.Add(typeof(AirlineCompany).Name, _airlineDAO as UserBaseMSSQLDAO<T>);
            correlation.Add(typeof(Customer).Name, _customerDAO as UserBaseMSSQLDAO<T>);
            correlation.Add(typeof(Administrator).Name, _administratorDAO as UserBaseMSSQLDAO<T>);


            //var users2Lst = correlation[user.GetType().Name].GetAll();
            T user2 = null;
            long currentUserId = (long)user.GetType().GetProperty("ID").GetValue(user);
            long absentUserId = (long)user.GetType().GetProperty("ID").GetValue(new T());
            if (!currentUserId.Equals(absentUserId))
                user2 = correlation[user.GetType().Name].Get((long)user.GetType().GetProperty("ID").GetValue(user));
            else
            {
                int identifierPlaceNumber = -1;
                for(int i = 0; i < typeof(T).GetProperties().Length; i++)
                {
                    if (typeof(T).GetProperties()[i].Name == "IDENTIFIER")
                    {
                        identifierPlaceNumber = i;
                        break;
                    }
                }
                string identifier = (string)user.GetType().GetProperty("IDENTIFIER").GetValue(user);
                user2 = correlation[user.GetType().Name].GetSomethingBySomethingInternal(identifier, identifierPlaceNumber);
            }
            //if (user2.Equals(user)) isExists = true;
            isExists = Statics.BulletprofComparsion(user, user2);

            return isExists;
        }
        protected bool IsSomethingExists<T>(T user) where T : class, IPoco, new()
        {           
            bool isExists = false;

            Dictionary<string, DAO<T>> correlation = new Dictionary<string, DAO<T>>();
            correlation.Add(typeof(Ticket).Name, _ticketDAO as DAO<T>);
            correlation.Add(typeof(Country).Name, _countryDAO as DAO<T>);
            correlation.Add(typeof(Flight).Name, _flightDAO as DAO<T>);
            correlation.Add(typeof(FailedLoginAttempt).Name, _failedLoginAttemptsDAO as DAO<T>);

            var user2 = correlation[user.GetType().Name].Get((long)user.GetType().GetProperty("ID").GetValue(user));
            if (user2.Equals(user)) isExists = true;

            return isExists;            
        }

        /// <summary>
        /// Gets registered user credentials as Utility"_clas_User object by appropriate ID,
        /// which is USER_ID property within registered user poco objects
        /// (Administrator, AirlineCompany and Customer)
        /// </summary>
        /// <param name="registeredUserId">Utility_class_user ID and also registered user USER_ID</param>
        /// <returns></returns>
        public Utility_class_User GetRegisteredUserDetails(long registeredUserId)
        {
            return _utility_Class_UserDAO.Get(registeredUserId);
        }











        /// <summary>
        /// Gets real name from "MoveNext" in Async methods by reflection
        /// </summary>
        /// <param name="asyncMethod">Usage: GetRealMethodFromAsyncMethod(System.Reflection.MethodBase.GetCurrentMethod())</param>
        /// <returns></returns>
        private static MethodBase GetRealMethodFromAsyncMethod(MethodBase asyncMethod)
        {
            var generatedType = asyncMethod.DeclaringType;
            var originalType = generatedType.DeclaringType;
            var matchingMethods =
                from methodInfo in originalType.GetMethods()
                let attr = methodInfo.GetCustomAttribute<AsyncStateMachineAttribute>()
                where attr != null && attr.StateMachineType == generatedType
                select methodInfo;

            // If this throws, the async method scanning failed.
            var foundMethod = matchingMethods.Single();
            return foundMethod;
        }

    }
}
