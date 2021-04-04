using Flight_Center_Project_FinalExam_DAL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DataBase_Generator_WPF.Models
{
    /// <summary>
    /// this class is used to create the poco objects as a medium between the raw information 
    /// that come from an outside sources or generated and the database entries.
    /// </summary>
    class Factory
    {
        private Random _rnd = new Random();
        private readonly string ENCRIPTION_PHRASE;
        private string _apiUrl = "https://randomuser.me/api";

        private RandomUserAPIObject deserializedRandomUserObject;

        private List<AirlineCompany> _airlineCompanies;
        private List<Country> _countries;
        private List<Customer> _customers;
        private List<Flight> _flights;
        private List<Dictionary<string, string>> _airlineCompaniesFromExternalSource;

        private Regex _regex = new Regex("('|\"|„|”|«|»)");


        public Factory(string encriptionPhrase, IEnumerable<AirlineCompany> airlineCompanies, IEnumerable<Country> countries, List<Dictionary<string, string>> airlineCompaniesFromExternalSource, IEnumerable<Customer> customers, IEnumerable<Flight> flights)
        {
            _airlineCompanies = airlineCompanies.ToList();
            _countries = countries.ToList();
            _airlineCompaniesFromExternalSource = airlineCompaniesFromExternalSource;
            _customers = customers.ToList();
            _flights = flights.ToList();


            ENCRIPTION_PHRASE = encriptionPhrase;
            
        }


        /// <summary>
        /// Creates a "ready to use" object of the type "Customer"
        /// </summary>
        /// <returns>"Customer" object</returns>
        public Customer CreateCustomer(int index) { return CreateCustomer(); }
        public Customer CreateCustomer()
        {
            deserializedRandomUserObject = JsonConvert.DeserializeObject<RandomUserAPIObject>(Statics.ReadFromUrl(_apiUrl));

            Customer customer = new Customer();
            customer.FIRST_NAME = deserializedRandomUserObject.results[0].name.first;
            customer.LAST_NAME = deserializedRandomUserObject.results[0].name.last;
            customer.PHONE_NO = deserializedRandomUserObject.results[0].phone;
            //customer.ADDRESS = $"{deserializedRandomUserObject.results[0].location.city}, {deserializedRandomUserObject.results[0].location.street} st, {deserializedRandomUserObject.results[0].location.coordinates.latitude} / {deserializedRandomUserObject.results[0].location.coordinates.longitude}";
            customer.ADDRESS = _regex.Replace($"{deserializedRandomUserObject.results[0].location.city}, {deserializedRandomUserObject.results[0].location.street.name} st, {deserializedRandomUserObject.results[0].location.street.number}", string.Empty);
            customer.CREDIT_CARD_NUMBER = Statics.Encrypt(Statics.DashingString(Statics.GetUniqueKeyOriginal_BIASED(20, Charset.OnlyNumber), 4), ENCRIPTION_PHRASE);
            customer.IMAGE = ImageProvider.GetResizedCustomerImageAs64BaseString(4);
            return customer;
        }
        public Administrator CreateAdministrator(int index) { return CreateAdministrator(); }
        public Administrator CreateAdministrator()
        {
            deserializedRandomUserObject = JsonConvert.DeserializeObject<RandomUserAPIObject>(Statics.ReadFromUrl(_apiUrl));

            Administrator administrator = new Administrator();
            administrator.NAME = $"{deserializedRandomUserObject.results[0].name.first} {deserializedRandomUserObject.results[0].name.last}";
            return administrator;
        }
        public Flight CreateFlight(int index) { return CreateFlight(); }
        public Flight CreateFlight()
        {
            Flight flight = new Flight();
            AirlineCompany randomAirline = _airlineCompanies[_rnd.Next(_airlineCompanies.Count)];
            flight.AIRLINECOMPANY_ID = randomAirline.ID;
            flight.ORIGIN_COUNTRY_CODE = _countries[_rnd.Next(_countries.Count)].ID;
            flight.DESTINATION_COUNTRY_CODE = _countries[_rnd.Next(_countries.Count)].ID;

            var departureDateTime = Statics.GetRandomDate(DateTime.Now, DateTime.Now.AddHours(Convert.ToDouble(_rnd.Next(5, 18))).AddMinutes(Convert.ToDouble(_rnd.Next(10, 55))));
            flight.DEPARTURE_TIME = departureDateTime;
            //flight.LANDING_TIME = Statics.GetRandomDate(flight.DEPARTURE_TIME, new DateTime(flight.DEPARTURE_TIME.Year, flight.DEPARTURE_TIME.Month, flight.DEPARTURE_TIME.Day, flight.DEPARTURE_TIME.Hour + _rnd.Next(0, 24 - flight.DEPARTURE_TIME.Hour), 0, 0));
            flight.LANDING_TIME = Statics.GetRandomDate(flight.DEPARTURE_TIME, flight.DEPARTURE_TIME.AddHours(_rnd.Next(2, 8)).AddMinutes(_rnd.Next(5, 45)));
            flight.REMAINING_TICKETS = _rnd.Next(0, 500);
            flight.PRICE = _rnd.Next(50, 200);

            return flight;
        }

        public AirlineCompany CreateAirlineCompany(int index)
        {
            AirlineCompany airline = new AirlineCompany();
            airline.AIRLINE_NAME = _regex.Replace(_airlineCompaniesFromExternalSource[index]["name"], string.Empty);
            airline.IMAGE = ImageProvider.GetResizedAirlineImageAs64BaseString(4);
            airline.ADORNING = _airlineCompaniesFromExternalSource[index]["adorning"] + " " + Statics.GetUniqueKeyOriginal_BIASED(_rnd.Next(2, 4), Charset.OnlyNumber);
            string airline_country_name = _regex.Replace(_airlineCompaniesFromExternalSource[index]["country"], string.Empty);
            foreach (var s_country in _countries)
            {
                if (airline_country_name.Equals(s_country.COUNTRY_NAME))
                    airline.COUNTRY_CODE = s_country.ID;
            }

            return airline;

        }

        public Ticket CreateTicket(int index) { return CreateTicket(); }
        public Ticket CreateTicket()
        {
            Ticket ticket = new Ticket();
            Customer randomCustomer = _customers[_rnd.Next(_customers.Count)];
            Flight randomFlight = _flights[_rnd.Next(_flights.Count)];

            ticket.CUSTOMER_ID = randomCustomer.ID;
            ticket.FLIGHT_ID = randomFlight.ID;
            return ticket;
        }



    }
}
