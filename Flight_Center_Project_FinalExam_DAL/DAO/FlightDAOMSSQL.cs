using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace Flight_Center_Project_FinalExam_DAL
{
    /// <summary>
    /// for "FlightSystemMainView.cshtml"
    /// </summary>
    [Flags]
    public enum RazorViewStatus
    {
        Departures = 0,
        Landings = 1, 
        Past = 2, 
        Future = 4
    }

    /// <summary>
    /// for "search.html"
    /// </summary>
    public enum SearchFlightsCriterion
    {
        AirlineName = 0, //AirlineCompanies - AIRLINE_NAME
        DestinationCountryName = 1, //Countries - COUNTRY_NAME
        OriginCountryName = 2, //Countries - COUNTRY_NAME
        Adorning = 4, //AirlineCompanies - ADORNING
        Indiscriminable = 8 //All flights without condition
    }

    /// <summary>
    /// for "search.html"
    /// </summary>
    public enum SortFlightsCriterion
    {
        NoSorting = 0,
        ByPrice = 1,
        ByTakingOffTime = 2,
        ByFlightLenght = 4
    }



    public class FlightDAOMSSQL<T> : DAO<T>, IFlightDAO<T> where T : Flight, IPoco, new()
    {
        public FlightDAOMSSQL(): base() { }

        /// <summary>
        /// returns dictionary with all the flights as keys and their respective numbers of remaining tickets as values
        /// </summary>
        /// <returns></returns>
        public Dictionary<Flight, int> GetAllFlightVacancty()
        {            
            Dictionary<Flight, int> allFlightVacancty = new Dictionary<Flight, int>();
            foreach(var s in this.GetAll()) allFlightVacancty.Add(s, s.REMAINING_TICKETS);
            return allFlightVacancty;
        }

        public List<T> GetAllFlightsByAirlineName(long airlineUtilitiClassUserId)
        {

            ExecuteCurrentMethosProcedure execute = CreateDlegateForGetAllFlightsByAirlineNameInternal(airlineUtilitiClassUserId);

            //return RunToRead(executeCurrentMethosProcedure: execute, commandType: CommandType.StoredProcedure);
            return Run(() => { return ReadFromDb(); }, executeCurrentMethosProcedure: execute, commandType: CommandType.Text);
        }
        public Task<List<T>> GetAllFlightsByAirlineNameAsync(long airlineUtilitiClassUserId)
        {

            ExecuteCurrentMethosProcedure execute = CreateDlegateForGetAllFlightsByAirlineNameInternal(airlineUtilitiClassUserId);

            //return RunToRead(executeCurrentMethosProcedure: execute, commandType: CommandType.StoredProcedure);
            return RunAsync(() => { return ReadFromDbAsync(); }, executeCurrentMethosProcedure: execute, commandType: CommandType.Text);
        }
        private ExecuteCurrentMethosProcedure CreateDlegateForGetAllFlightsByAirlineNameInternal(long airlineUtilitiClassUserId)
        {
            return (out string commandtextStoredProcedureName, out string commandTextForTextMode, out Dictionary<string, object> storedProcedureParameters) =>
            {
                //commandTextForTextMode = $"select Flights.* from Flights JOIN AirlineCompanies ON Flights.AIRLINECOMPANY_ID = AirlineCompanies.ID WHERE (AirlineCompanies.AIRLINE_NAME = 'Air Atlantic Dominicana')"
                commandTextForTextMode = $"select Flights.* from Flights JOIN AirlineCompanies ON Flights.AIRLINECOMPANY_ID = AirlineCompanies.ID JOIN Utility_class_Users ON AirlineCompanies.USER_ID = Utility_class_Users.ID WHERE Utility_class_Users.ID = {airlineUtilitiClassUserId}";
                commandtextStoredProcedureName = "DAO_BASE_GetAll_ByAirlineName_METHOD_QUERY";

                storedProcedureParameters = new Dictionary<string, object>();
                storedProcedureParameters.Add("TABLE_NAME", "Flights");
                storedProcedureParameters.Add("AIRLINE_NAME", "Flights");
            };
        }



        public List<Flight> GetFlightsByCustomer1(Customer customer)
        {
            try
            {
                SetAnotherDAOInstance(typeof(Utility_class_User));
                var customerAsUser = _currentUserDAOMSSQL.GetUserByIdentifier(customer);
                SetAnotherDAOInstance(typeof(AirlineCompany));
                AirlineCompany company = _currentAirlineDAOMSSQL.GetAirlineByUsername(customerAsUser.USER_NAME);                
                _connection.Open();
                List<Flight> flightsByCustomer = new List<Flight>();
                
                _command.CommandText = $"SELECT * FROM Flights WHERE AIRLINECOMPANY_ID = {company.ID}";
                using (SqlDataReader reader = _command.ExecuteReader())
                {                    
                    while (reader.Read())
                    {
                        Flight flightByCustomer = new Flight();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            typeof(Flight).GetProperties()[i].SetValue(flightByCustomer, reader.GetValue(i));
                        }
                        flightsByCustomer.Add(flightByCustomer);
                    }                    
                }
                return flightsByCustomer;

            }
            finally { _connection.Close(); }

        }
        /// <summary>
        /// getting a list of 
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        public List<Flight> GetFlightsByCustomer(Customer customer)
        {
            SetAnotherDAOInstance(typeof(Ticket));
            var ticketWithThisCustomerId  = _currentTicketDAOMSSQL.GetManyBySomethingInternal(customer.ID, (int)TicketPropertyNumber.CUSTOMER_ID);
            SetAnotherDAOInstance(typeof(Flight));
            return ticketWithThisCustomerId.Select(x => _currentFlightDAOMSSQL.GetSomethingBySomethingInternal(x.FLIGHT_ID, (int)FlightPropertyNumber.ID)).ToList();
        }




        public Flight GetFlightByCustomer(Customer customer)
        {
            try
            {
                SetAnotherDAOInstance(typeof(Utility_class_User));
                var customerAsUser = _currentUserDAOMSSQL.GetUserByIdentifier(customer);
                SetAnotherDAOInstance(typeof(AirlineCompany));
                AirlineCompany company = _currentAirlineDAOMSSQL.GetAirlineByUsername(customerAsUser.USER_NAME);
                Flight flightByCustomer = GetSomethingBySomethingInternal(company.ID, (int)FlightPropertyNumber.AIRLINECOMPANY_ID);
                return flightByCustomer;
            }
            finally { _connection.Close(); }

        }

        public Flight GetFlightByDepartureDate(DateTime departureDate)
        {
            return GetSomethingBySomethingInternal(departureDate, (int)FlightPropertyNumber.DEPARTURE_TIME);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sometime">Time that defines time range of the flights the method returns</param>
        /// <param name="viewStatus">Defines if flights are for Departures or Arrivals</param>
        /// <param name="viewTime">Defines if Departures or Arrival was or will be</param>
        /// <returns></returns>
        public List<T> GetAllFlightsThatTakeOffInSomeTimeFromNow(TimeSpan sometime, RazorViewStatus viewStatus, RazorViewStatus viewTime)
        {
            var dt = DateTime.Now;
            DateTime selectionDateTime = DateTime.Now.Add(sometime);

            //imminentFlightActionTime -> depatrure time or landing time of the flight in question, depending on "viewStatus" parameter of the method
            string imminentFlightActionTime = "DEPARTURE_TIME";
            char comparsionSymbol = '<';
            char comparsionSymbol2 = '>';


            if (viewStatus == RazorViewStatus.Landings)
            {
                imminentFlightActionTime = "LANDING_TIME";
                if (viewTime == RazorViewStatus.Past)
                {
                    comparsionSymbol = '<';
                    comparsionSymbol2 = '>';
                    selectionDateTime = DateTime.Now.Subtract(sometime);

                }
            }

            ExecuteCurrentMethosProcedure execute = (out string commandtextStoredProcedureName, out string commandTextForTextMode, out Dictionary<string, object> storedProcedureParameters) =>
            {
                commandTextForTextMode = $"select * from {GetTableName(typeof(T))} WHERE {imminentFlightActionTime} {comparsionSymbol}= '{selectionDateTime.ToString("yyyy-MM-dd HH:mm:ss")}' AND {imminentFlightActionTime} {comparsionSymbol2}= '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}'";
                commandtextStoredProcedureName = string.Empty;
                storedProcedureParameters = null;
            };

            return Run(() => { return ReadFromDb(); }, executeCurrentMethosProcedure: execute, commandType: CommandType.Text);
        }


        //07/08/20
        /// <summary>
        /// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// </summary>
        /// <param name="AirlineName"></param>
        /// <returns></returns>
        public async Task<List<T>> GetFlightsByQuery(string searchQuery, SearchFlightsCriterion searchCriterion)
        {
            ExecuteCurrentMethosProcedure execute = (out string commandtextStoredProcedureName, out string commandTextForTextMode, out Dictionary<string, object> storedProcedureParameters) =>
            {
                string tableName = GetTableName(typeof(T));

                commandTextForTextMode = string.Empty;
                switch (searchCriterion)
                {
                    case SearchFlightsCriterion.AirlineName:
                        commandTextForTextMode = $"select {tableName}.ID,AIRLINECOMPANY_ID,ORIGIN_COUNTRY_CODE,DESTINATION_COUNTRY_CODE,DEPARTURE_TIME,LANDING_TIME,{tableName}.IDENTIFIER,REMAINING_TICKETS,PRICE from {tableName} JOIN AirlineCompanies ON {tableName}.AIRLINECOMPANY_ID = AirlineCompanies.ID WHERE AirlineCompanies.AIRLINE_NAME  = '{searchQuery}'";
                        break;
                    case SearchFlightsCriterion.DestinationCountryName:
                        commandTextForTextMode = $"select {tableName}.ID,AIRLINECOMPANY_ID,ORIGIN_COUNTRY_CODE,DESTINATION_COUNTRY_CODE,DEPARTURE_TIME,LANDING_TIME,{tableName}.IDENTIFIER,REMAINING_TICKETS,PRICE from {tableName} JOIN Countries ON {tableName}.DESTINATION_COUNTRY_CODE = Countries.ID WHERE Countries.COUNTRY_NAME  = '{searchQuery}'";
                        break;
                    case SearchFlightsCriterion.OriginCountryName:
                        commandTextForTextMode = $"select {tableName}.ID,AIRLINECOMPANY_ID,ORIGIN_COUNTRY_CODE,DESTINATION_COUNTRY_CODE,DEPARTURE_TIME,LANDING_TIME,{tableName}.IDENTIFIER,REMAINING_TICKETS,PRICE from {tableName} JOIN Countries ON {tableName}.ORIGIN_COUNTRY_CODE = Countries.ID WHERE Countries.COUNTRY_NAME  = '{searchQuery}'";
                        break;
                    case SearchFlightsCriterion.Adorning:
                        commandTextForTextMode = $"select {tableName}.ID,AIRLINECOMPANY_ID,ORIGIN_COUNTRY_CODE,DESTINATION_COUNTRY_CODE,DEPARTURE_TIME,LANDING_TIME,{tableName}.IDENTIFIER,REMAINING_TICKETS,PRICE from {tableName} JOIN AirlineCompanies ON {tableName}.AIRLINECOMPANY_ID = AirlineCompanies.ID WHERE AirlineCompanies.ADORNING  = '{searchQuery}'";
                        break;
                    case SearchFlightsCriterion.Indiscriminable:
                        commandTextForTextMode = $"select * FROM {tableName}";
                        break;
                }

                //commandTextForTextMode = $"select {tableName}.ID,AIRLINECOMPANY_ID,ORIGIN_COUNTRY_CODE,DESTINATION_COUNTRY_CODE,DEPARTURE_TIME,LANDING_TIME,{tableName}.IDENTIFIER,REMAINING_TICKETS from {tableName} JOIN AirlineCompanies ON {tableName}.AIRLINECOMPANY_ID = AirlineCompanies.ID WHERE AirlineCompanies.ADORNING  = '{searchQuery}'";
                commandtextStoredProcedureName = string.Empty;
                storedProcedureParameters = null;
            };

            return await RunAsync(async()=> { return await ReadFromDbAsync(); }, executeCurrentMethosProcedure: execute, commandType: CommandType.Text);
        }

        
        /*
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sometime">Time that defines time range of the flights the method returns</param>
        /// <param name="viewStatus">Defines if flights are for Departures or Arrivals</param>
        /// <param name="viewTime">Defines if Departures or Arrival was or will be</param>
        /// <returns></returns>
        public List<T> GetAllFlightsThatTakeOffInSomeTimeFromNow_old(TimeSpan sometime, RazorViewStatus viewStatus, RazorViewStatus viewTime)
        {
            try
            {
                List<T> toReturn = new List<T>();
                _connection.Open();
                _command.CommandType = CommandType.Text;
                var dt = DateTime.Now;
                DateTime selectionDateTime = DateTime.Now.Add(sometime);

                //imminentFlightActionTime -> depatrure time or landing time of the flight in question, depending on "viewStatus" parameter of the method
                string imminentFlightActionTime = "DEPARTURE_TIME";
                char comparsionSymbol = '<';
                char comparsionSymbol2 = '>';


                if (viewStatus == RazorViewStatus.Landings)
                {
                    imminentFlightActionTime = "LANDING_TIME";
                    if (viewTime == RazorViewStatus.Past)
                    {
                        comparsionSymbol = '<';
                        comparsionSymbol2 = '>';
                        selectionDateTime = DateTime.Now.Subtract(sometime);

                    }
                }

                _command.CommandText = $"select * from {GetTableName(typeof(T))} WHERE {imminentFlightActionTime} {comparsionSymbol}= '{selectionDateTime}' AND {imminentFlightActionTime} {comparsionSymbol2}= '{DateTime.Now}'";
                using (SqlDataReader reader = _command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        T poco = new T();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            object value = reader[i];
                            if (reader[i] is DBNull && typeof(T).GetProperties()[i].GetType().Name.ToLower().Equals("string"))
                            {
                                typeof(T).GetProperties()[i].SetValue(poco, string.Empty);
                            }
                            if (reader[i] is DBNull && typeof(T).GetProperties()[i].GetType().Name.ToLower().Contains("int"))
                            {
                                typeof(T).GetProperties()[i].SetValue(poco, 0);
                            }

                            if (!(reader[i] is DBNull)) { typeof(T).GetProperties()[i].SetValue(poco, value); }

                        }
                        toReturn.Add(poco);
                    }
                }
                return toReturn;

            }
            finally { _connection.Close(); }


        }*/

        public Flight getFlightByDestinationCountry(Country destinationCountry)
        {
            return GetSomethingBySomethingInternal(destinationCountry.ID, (int)FlightPropertyNumber.DESTINATION_COUNTRY_CODE);
        }

        public Flight GetFlightById(long flightID)
        {
            return Get(flightID);
        }

        public Flight GetFlightByLandingDate(DateTime landingDate)
        {
            return GetSomethingBySomethingInternal(landingDate, (int)FlightPropertyNumber.LANDING_TIME);
        }

        public Flight GetFlightByOriginCountry(Country originCountry)
        {
            return GetSomethingBySomethingInternal(originCountry.ID, (int)FlightPropertyNumber.ORIGIN_COUNTRY_CODE);
        }


    }
}
