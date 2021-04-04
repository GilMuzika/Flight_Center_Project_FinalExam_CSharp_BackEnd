using DataBase_Generator_WPF.Models;
using Flight_Center_Project_FinalExam_DAL;
using Microsoft.SqlServer.Management.Assessment;
using Microsoft.SqlServer.Management.Smo;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace DataBase_Generator_WPF
{
    class DBGenerator<T> : IDataInterface where T : class, IPoco, new()
    {
        const string ENCRIPTION_PHRASE = "4r8rjfnklefjkljghggGKJHnif5r5242";

        //private string _apiUrl = "https://randomuser.me/api";

        private readonly Factory _factory;
        public Factory Factory
        {
            get => _factory;
        }

        private List<Dictionary<string, string>> _airlineCompaniesFromExternalSource = AirlineGenerator.Airlines;
        private List<Country> _countries;
        private IEnumerable<Country> _countriesIEnumerable;
        private IEnumerable<Customer> _customersIEnumerable;
        private IEnumerable<Flight> _flightsIEnumerable;
        private List<AirlineCompany> _airlineCompanies;
        private IEnumerable<AirlineCompany> _airlineCompaniesIEnumerable;
        private List<Flight> _flights;
        private List<Customer> _customers;
        private FlightDAOMSSQL<Flight> _flightDAO = new FlightDAOMSSQL<Flight>();
        private CountryDAOMSSQL<Country> _countryDAO = new CountryDAOMSSQL<Country>();
        private AirlineDAOMSSQL<AirlineCompany> _airlineCompanyDAO = new AirlineDAOMSSQL<AirlineCompany>();
        private CustomerDAOMSSQL<Customer> _customerDAO = new CustomerDAOMSSQL<Customer>();

        private Regex _regex = new Regex("('|\"|„|”|«|»)");

        private List<Type> _utility_user_types = new List<Type>();
        private WebClient _webClient = new WebClient();
        private Random _rnd = new Random();

        private UserBaseMSSQLDAO<T> _currentWithUtility_clas_UserDAO;
        private DAO<T> _currentDAO;

        private DAOCreatorFactory<T> _appropriateDAOcreator = new DAOCreatorFactory<T>();


        private List<string> GetPropertyKeysForDynamic(dynamic dynamicToGetPropertiesFor)
        {
            JObject attributesAsJObject = dynamicToGetPropertiesFor;
            Dictionary<string, object> values = dynamicToGetPropertiesFor.ToObject<Dictionary<string, object>>();
            List<string> toReturn = new List<string>();
            foreach (string key in values.Keys)
            {
                toReturn.Add(key);
            }
            return toReturn;
        }
        
        public DBGenerator()
        {
            
            Initialize();
            _factory = new Factory(ENCRIPTION_PHRASE, _airlineCompaniesIEnumerable, _countriesIEnumerable, _airlineCompaniesFromExternalSource, _customersIEnumerable, _flightsIEnumerable);
        }
        private Dictionary<Type, Action<long, long, long>> _genericTypeMethodCorrelation = new Dictionary<Type, Action<long, long, long>>();
        /// <summary>
        /// method that selects the appropriate method for adding a a database member depending on the this class Generic parameter ("T")
        /// </summary>
        private void Initialize()
        {
            _countries = _countryDAO.GetAll();
            _countriesIEnumerable = _countryDAO.GetAllASIEnumerable();
            _airlineCompanies = _airlineCompanyDAO.GetAll();            
            _airlineCompaniesIEnumerable = _airlineCompanyDAO.GetAllASIEnumerable();
            _customersIEnumerable = _customerDAO.GetAllASIEnumerable();
            _flightsIEnumerable = _flightDAO.GetAllASIEnumerable();
            _flights = _flightDAO.GetAll();
            _customers = _customerDAO.GetAll();

            _utility_user_types.Add(typeof(Administrator));
            _utility_user_types.Add(typeof(Customer));
            _utility_user_types.Add(typeof(AirlineCompany));



            _genericTypeMethodCorrelation.Add(typeof(Customer), AddCustomers);
            _genericTypeMethodCorrelation.Add(typeof(Administrator), AddAdministrators);
            _genericTypeMethodCorrelation.Add(typeof(Flight), AddFlights);
            _genericTypeMethodCorrelation.Add(typeof(Country), this.AddCountries);
            _genericTypeMethodCorrelation.Add(typeof(AirlineCompany), this.AddAirlineCompanies);
            _genericTypeMethodCorrelation.Add(typeof(Ticket), this.AddTickets);
            //add more "Add["type_name"]" functions to the dictionary when they created
        }
        private void GenerateUtility_class_UserPasswordAndName(out string nameCrypt, out string passwordCrypt)
        {
            string nameForEncription = Statics.GetUniqueKeyOriginal_BIASED(_rnd.Next(5, 8));
            string passForEncription = Statics.GetUniqueKeyOriginal_BIASED(_rnd.Next(5, 15));

            string encryptedName = Statics.Encrypt(nameForEncription, ENCRIPTION_PHRASE);
            string encryptedPassword = Statics.Encrypt(passForEncription, ENCRIPTION_PHRASE);
            nameCrypt = encryptedName;
            passwordCrypt = encryptedPassword;
        }

        public void Add(long from, long to, long fixedNumber)
        {           
            _genericTypeMethodCorrelation[typeof(T)](from, to, fixedNumber);
        }

        /// <summary>
        /// calls to the "SwapDataBases()" methos in the basic common DAO (all the specific DAOs have it by inheritance)
        /// that actualy doing the work of swapping databases
        /// </summary>
        public void SwapDatabases()
        {
            _currentDAO = _appropriateDAOcreator.CreateAppropriateDAO();

            //"SwapDataBases()" in the basic common DAO class is the actual methos that swaps databases
            _currentDAO.SwapDataBases();
        }

        public void UpdateAll()
        {
            var currentDAO = _appropriateDAOcreator.SelectAppropriateDAO();

            List<T> all = currentDAO.GetAll();
            Dictionary<Type, Func<int, IPoco>> genericTypeMethodCorrelation = new Dictionary<Type, Func<int, IPoco>>();
            genericTypeMethodCorrelation.Add(typeof(Customer), _factory.CreateCustomer);
            genericTypeMethodCorrelation.Add(typeof(Administrator), _factory.CreateAdministrator);
            genericTypeMethodCorrelation.Add(typeof(Flight), _factory.CreateFlight);
            genericTypeMethodCorrelation.Add(typeof(AirlineCompany), _factory.CreateAirlineCompany);
            genericTypeMethodCorrelation.Add(typeof(Ticket), _factory.CreateTicket);

            foreach (T s in all)
            {
                T newDBitem = genericTypeMethodCorrelation[typeof(T)](0) as T;

                UpdateDBitem(newDBitem, (long)typeof(T).GetProperty("ID").GetValue(s), currentDAO);
            }
        }
        private void UpdateDBitem(T newDbItem, long oldDbItemID, IBasicDB<T> dao)
        {
            typeof(T).GetProperty("ID").SetValue(newDbItem, oldDbItemID);
            string[] excludedProperties = new string[] { "ID", "IDENTIFIER", "USER_ID", "COUNTRY_CODE", "AIRLINECOMPANY_ID", "ORIGIN_COUNTRY_CODE", "DESTINATION_COUNTRY_CODE", "FLIGHT_ID" };
            PropertyInfo[] propersOfT = typeof(T).GetProperties();
            for (int i = 0; i < propersOfT.Length; i++)
            {
                if (excludedProperties.Contains(propersOfT[i].Name)) continue;

                dao.UpdateOneRow(i, newDbItem);
            }
        }

        //all the "Add["type_name"]" functions must came with the same signature
        private void AddCustomers(long from, long to, long fixedNumber) //this function don't use the parameter "fixedNumber" but it must be there becauase of signature uniformity
        {
            int randomCallingsNum = _rnd.Next(Convert.ToInt32(from), Convert.ToInt32(to));

            for (long i = 0; i < randomCallingsNum; i++) AddOneCustomer();            
        }
        private void AddOneCustomer()
        {
            Customer customer = _factory.CreateCustomer();

            _currentWithUtility_clas_UserDAO = _appropriateDAOcreator.CreateAppropriateDAO_WithUtility_class_User();
            GenerateUtility_class_UserPasswordAndName(out string nameCrypt, out string passsCrypt);
            _currentWithUtility_clas_UserDAO.Add(customer as T, nameCrypt, passsCrypt);
        }
        private void AddAdministrators(long from, long to, long fixedNumber) //this function don't use the parameters "from" and "to" but they must be there becauase of signature uniformity
        {            
            for (int i = 0; i < fixedNumber; i++) AddOneAdministrator();
        }
        private void AddOneAdministrator()
        {
           

            Administrator administrator = _factory.CreateAdministrator();

            _currentWithUtility_clas_UserDAO = _appropriateDAOcreator.CreateAppropriateDAO_WithUtility_class_User();
            GenerateUtility_class_UserPasswordAndName(out string nameCrypt, out string passsCrypt);
            _currentWithUtility_clas_UserDAO.Add(administrator as T, nameCrypt, passsCrypt);
            
        }
        private void AddFlights(long from, long to, long fixedNumber) //this function don't use the parameters "from" and "to" but they must be there becauase of signature uniformity
        {
            for (int i = 0; i < fixedNumber; i++) AddOneFlight();
        }
        private void AddOneFlight()
        {
            Flight flight = _factory.CreateFlight();

            _currentDAO = _appropriateDAOcreator.CreateAppropriateDAO();
            _currentDAO.Add(flight as T);
        }      
        private void AddCountries(long from, long to, long fixedNumber) //this function don't use none of it's parameters, but they still need to be here because of the signature uniformity
        {
            AddCountries();       
        }
        /// <summary>
        /// Fill the Countries table from the Database basing on the AirlineCompanies data source
        /// </summary>
        private void AddCountries()
        {                                 
            _currentDAO = _appropriateDAOcreator.CreateAppropriateDAO();
            _currentDAO.DeleteAllNotRegardingForeignKeys();

            List<string> countries_names = new List<string>();
            foreach(var s_airline in _airlineCompaniesFromExternalSource)
            {
                s_airline.TryGetValue("country", out string s_airline_country_name);
                if (!countries_names.Contains(s_airline_country_name)) countries_names.Add(s_airline_country_name);
            }


            foreach (var s_country_name in countries_names)
            {                
                Country country = new Country();
                country.COUNTRY_NAME = _regex.Replace(s_country_name, string.Empty);                               
                //country.COUNTRY_IDENTIFIER = Convert.ToInt64(Statics.GetUniqueKeyOriginal_BIASED(_rnd.Next(4, 9), Charset.OnlyNumber));
                _currentDAO.Add(country as T);
            }
            
        }
        
        private void AddAirlineCompanies(long from, long to, long fixedNumber) //this function don't use the parameter "fixedNumber" but it must be there becauase of signature uniformity
        {
            int randomCallingsNum = _rnd.Next(Convert.ToInt32(from), Convert.ToInt32(to));
            List<long> indexses = new List<long>();

            for (long i = 0; i < randomCallingsNum; i++) indexses.Add(_rnd.Next(_airlineCompaniesFromExternalSource.Count-1));

            indexses = Statics.ShuffleList(indexses);

            indexses.ForEach(x => AddOneAirlineCompany(Convert.ToInt32(x)));
        }
        private void AddOneAirlineCompany(int index)
        {
            AirlineCompany airline = _factory.CreateAirlineCompany(index);

            _currentWithUtility_clas_UserDAO = _appropriateDAOcreator.CreateAppropriateDAO_WithUtility_class_User();
            GenerateUtility_class_UserPasswordAndName(out string nameCrypt, out string passsCrypt);
            _currentWithUtility_clas_UserDAO.Add(airline as T, nameCrypt, passsCrypt);
        }
        private void AddTickets(long from, long to, long fixedNumber) //this function don't use the parameter "fixedNumber" but it must be there becauase of signature uniformity
        {
            int randomCallingsNum = _rnd.Next(Convert.ToInt32(from), Convert.ToInt32(to));
            List<long> indexses = new List<long>();

            for (long i = 0; i < randomCallingsNum; i++) indexses.Add(i);

            indexses = Statics.ShuffleList(indexses);

            indexses.ForEach(x => AddOneTicket());
        }
        private void AddOneTicket()
        {
            Ticket ticket = _factory.CreateTicket();

            _currentDAO = _appropriateDAOcreator.CreateAppropriateDAO();
            _currentDAO.Add(ticket as T);
        }






    }








}
