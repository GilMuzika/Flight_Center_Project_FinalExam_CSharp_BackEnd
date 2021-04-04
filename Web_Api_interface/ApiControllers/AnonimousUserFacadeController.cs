using Flight_Center_Project_FinalExam_BL;
using Flight_Center_Project_FinalExam_DAL;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Windows;
using Web_Api_interface.Comparers;
using Web_Api_interface.IControllers;
using Web_Api_interface.Models;
using Web_Api_interface.Views.FlightSystemMain;

namespace Web_Api_interface.Controllers
{
    [RoutePrefix("api/AnonimousUserFacade")]
    public class AnonimousUserFacadeController : ApiController, IAnonimousUserFacadeController
    {
        FlyingCenterSystem _fsc = FlyingCenterSystem.GetInstance();

        AnonimousUserFacade _anonimousUserFacade;

        public AnonimousUserFacadeController()
        {
            _anonimousUserFacade = _fsc.GetAnonimousFacade();
        }

        //09/08/20
        /// <summary>
        /// ////////////////////////////////////////////////////////////////////////////////////
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("PreloadNames/{searchCriterion}")]
        public IHttpActionResult PreloadNames([FromUri] SearchFlightsCriterion searchCriterion)
        {
            List<string> names = null;

            switch(searchCriterion)
            {
                case SearchFlightsCriterion.AirlineName:
                    names = _anonimousUserFacade.PreloadAllAirlineNames();
                    break;
                case SearchFlightsCriterion.DestinationCountryName:
                case SearchFlightsCriterion.OriginCountryName:
                    names = _anonimousUserFacade.PreloadAllCountryNames();
                    break;
                case SearchFlightsCriterion.Adorning:
                    names = _anonimousUserFacade.PreloadingAdornings();
                    break;
            }
            if(names.Count == 0)
                return StatusCode(HttpStatusCode.Created);

            return Ok(names);
        }

        private void ConstructFlightDataObjectThreadPoolCallBack(object flightAsObject)
        {
            try
            {
                Flight flight = (Flight)flightAsObject;

                long airlineId = flight.AIRLINECOMPANY_ID;
                AirlineCompany airline = _anonimousUserFacade.Get<AirlineCompany>(airlineId);
                Country destinationCountry = _anonimousUserFacade.Get<Country>(flight.DESTINATION_COUNTRY_CODE);
                Country originCountry = _anonimousUserFacade.Get<Country>(flight.ORIGIN_COUNTRY_CODE);

                FlightData flightData = new FlightData();
                flightData.Adorning = airline.ADORNING;
                flightData.Image = ImageRestorer.GetFormattedImage64baseString(airline.IMAGE);
                flightData.AirlineName = airline.AIRLINE_NAME;
                flightData.DepartureCountryName = originCountry.COUNTRY_NAME;
                flightData.DestinationCountryName = destinationCountry.COUNTRY_NAME;

                TimingAndStatusCode timingAndStatusCode = new TimingAndStatusCode(flight, "some_string_that_represent_any_pageKind_that_isn't_Departures_or_Landings");

                flightData.EstimatedTime = timingAndStatusCode.EstimatedTime.ToString();
                flightData.StatusMessage = timingAndStatusCode.StatusMessage;
                flightData.StatusColor = timingAndStatusCode.StatusBoxColor;
                flightData.Price = flight.PRICE;
                flightData.DEPARTURE_TIME = flight.DEPARTURE_TIME;
                flightData.LANDING_TIME = flight.LANDING_TIME;
                flightData.FlightDuration = flight.LANDING_TIME.Subtract(flight.DEPARTURE_TIME);
                flightData.RemainingTickets = flight.REMAINING_TICKETS;

                flightData_s.Add(flightData);
            }
            catch { }
        }

        //TimingAndStatusCode timingAndStatusCode = new TimingAndStatusCode();
        private List<FlightData> flightData_s = new List<FlightData>();
        //08/08/20
        /// <summary>
        /// ////////////////////////////////////////////////////////////////////////////////////
        /// </summary>
        /// <param name="AirlineName"></param>
        /// <returns></returns>
        [ResponseType(typeof(Flight))]
        [HttpPost]
        [Route("GetFlightsByQuery", Name = "AnonimousGetFlightsByQuery")]
        //string searchQuery, SearchFlightsCriterion searchCriterion
        public async Task<IHttpActionResult> GetFlightsByQuery([FromBody] AlikeQueryParameters alikeQueryParameters)
        {
            string searchQuery = alikeQueryParameters.searchQuery;
            SearchFlightsCriterion searchCriterion = alikeQueryParameters.searchCriterion;
            SortFlightsCriterion sortingCriterion = alikeQueryParameters.sortingCriterion;

            //MessageBox.Show(AirlineName);
            List<Flight> flights = null;

            

            try
            {
                //Redis will be implemented in GetFlightsByQuery method
                /////
                /////
                /// Redis ///
                /// 

                flights = await _anonimousUserFacade.GetFlightsByQuery(searchQuery, searchCriterion);
                if (flights.Count == 0)
                {
                    string errorMessage = string.Empty;
                    switch (searchCriterion)
                    {
                        case SearchFlightsCriterion.AirlineName:
                            errorMessage = $"There is no airline company named {searchQuery}.";
                            break;
                        case SearchFlightsCriterion.DestinationCountryName:
                            errorMessage = $"There is no destination country named {searchQuery}";
                            break;
                        case SearchFlightsCriterion.OriginCountryName:
                            errorMessage = $"There is no origin country named {searchQuery}";
                            break;
                        case SearchFlightsCriterion.Adorning:
                            errorMessage = $"There is no airline company with the number {searchQuery}";
                            break;
                        case SearchFlightsCriterion.Indiscriminable:
                            errorMessage = "Sorry, but seems that is no flight exist in the system :(...";
                            break;
                        default:
                            errorMessage = $"Something went wrong. Appropriare search criterion isn't acquired. The search criterion must be a value of the enumeration \"SearchFlightsCriterion\"";
                            break;
                    }

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, errorMessage));
                }


                int threadsCount = 0;
                foreach (Flight flight in flights)
                {

                    Thread thread1 = new Thread(()=> {                        
                        ConstructFlightDataObjectThreadPoolCallBack(flight);
                    });
                    Thread.Sleep(100);

                    thread1.Start();
                    threadsCount++;

                    //ConstructFlightDataObjectThreadPoolCallBack(flight);
                    /*Thread.Sleep(40);
                    ThreadPool.QueueUserWorkItem(new WaitCallback(ConstructFlightDataObjectThreadPoolCallBack), flight);
                    threadsCount++;*/

                   

                }

            }
            catch(Exception ex)
            {
                if(flights == null) 
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, $"Something went wrong on the server. Maybe {ex.GetType().Name} occured. If so, the message is: \n \"{ex.Message}\""));
                else 
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.ExpectationFailed, $"Something went wrong on the server. Maybe {ex.GetType().Name} occured. If so, the message is: \n \"{ex.Message}\""));

            }

            var flyghtData_sArr = flightData_s.ToArray();

            //Here we will do sorting
            switch(sortingCriterion)
            {
                case SortFlightsCriterion.NoSorting:
                    //no action
                    break;
                case SortFlightsCriterion.ByPrice:
                    Array.Sort(flyghtData_sArr, new ComparerByNumericValuedProperty<FlightData>("Price"));
                    break;
                case SortFlightsCriterion.ByTakingOffTime:
                    Array.Sort(flyghtData_sArr, new ComparerByTimeValuedProperty<FlightData>("DEPARTURE_TIME"));
                    break;
                case SortFlightsCriterion.ByFlightLenght:
                    Array.Sort(flyghtData_sArr, new ComparerByTimeValuedProperty<FlightData>("FlightDuration"));
                    break;
            }

            return Ok(flyghtData_sArr);
            //return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, $"There is no tickets in the system."));
        }

        [ResponseType(typeof(AirlineCompany))]
        [HttpGet]
        [Route("GetAllAirlineCompanies", Name = "AnonimousGetAllAirlineCompanies")]
        public IHttpActionResult GetAllAirlineCompanies()
        {
            List<AirlineCompany> airlineCompanies =  _anonimousUserFacade.GetAllAirlineCompanies();
            if (airlineCompanies.Count == 0) return NotFound();

            return Ok(airlineCompanies);
        }

        [ResponseType(typeof(AirlineCompany))]
        [HttpGet]
        [Route("GetOneAirline", Name = "AnonimousGetOneAirline")]
        public IHttpActionResult GetOneAirline(long ID)
        {
            AirlineCompany company = _anonimousUserFacade.GetOneAirlineCompaniy(ID);
            if (company != null)
                return Ok(company);
            else return NotFound();
        }

        [ResponseType(typeof(Country))]
        [HttpGet]
        [Route("GetOneCountry/{ID}", Name = "AnonimousGetOneCountry")]
        public IHttpActionResult GetOneCountry([FromUri] long ID)
        {
            Country country = _anonimousUserFacade.GetOneCountry(ID);
            if (country != null)
                return Ok(country);
            else return NotFound();
        }

        /* The function is moved to "LoggedInCustomerFacadeController"
         * 
         * 
        [ResponseType(typeof(Customer))]
        [HttpGet]
        [Route("GetAllCustomers", Name = "GetAllCustomers")]
        public IHttpActionResult GetAllCustomers()
        {
            List<Customer> customers = _anonimousUserFacade.GetAll<Customer>();
            if (customers.Count == 0) return NotFound();

            return Ok(customers);
        }*/

        [ResponseType(typeof(Flight))]
        [HttpGet]
        [Route("GetAllFlights", Name = "AnonimousGetAllFlights")]
        public IHttpActionResult GetAllFlights()
        {

            /*List<Flight> flights = _anonimousUserFacade.GetAllFlights();
            if (flights.Count == 0) return NotFound();

            return Ok(flights);*/

            List<Flight> flights = null;

            try
            {

                flights = _anonimousUserFacade.GetAllFlights();
                if (flights.Count == 0)
                {
                    string errorMessage = "ERROR! ERROR!";

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, errorMessage));
                }


                int threadsCount = 0;
                foreach (Flight flight in flights)
                {

                    Thread thread1 = new Thread(() => {
                        ConstructFlightDataObjectThreadPoolCallBack(flight);
                    });
                    Thread.Sleep(120);

                    thread1.Start();
                    threadsCount++;

                    //ConstructFlightDataObjectThreadPoolCallBack(flight);
                    /*Thread.Sleep(40);
                    ThreadPool.QueueUserWorkItem(new WaitCallback(ConstructFlightDataObjectThreadPoolCallBack), flight);
                    threadsCount++;*/



                }

            }
            catch (Exception ex)
            {
                if (flights == null)
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, $"Something went wrong on the server. Maybe {ex.GetType().Name} occured. If so, the message is: \n \"{ex.Message}\""));
                else
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.ExpectationFailed, $"Something went wrong on the server. Maybe {ex.GetType().Name} occured. If so, the message is: \n \"{ex.Message}\""));

            }

            var flyghtData_sArr = flightData_s.ToArray();

            return Ok(flyghtData_sArr);
        }

        [ResponseType(typeof(Flight))]
        [HttpGet]
        [Route("GetAllCountries")]
        public IHttpActionResult GetAllCountries()
        {
            List<Country> allCountries = _anonimousUserFacade.GetAll<Country>();
            if (allCountries == null || allCountries.Count == 0) return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, "This is a special method. Maybe something went wrong"));
            return Ok(allCountries);
        }

        [ResponseType(typeof(Flight))]
        [HttpGet]
        [Route("GetAllFlightsAndRemainingTichketsForEachFly", Name = "AnonimousGetAllFlightsAndRemainingTichketsForEachFly")]
        public IHttpActionResult GetAllFlightsVacancy()
        {

            Dictionary<Flight, int> flightsVacancy = _anonimousUserFacade.GetAllFlightsVacancy();
            if (flightsVacancy.Count == 0) return NotFound();

            return Ok(flightsVacancy);            
        }

        [ResponseType(typeof(Flight))]
        [HttpPost]
        [Route("GetFlightByDepartureDate", Name = "AnonimousGetFlightByDepartureDate")]
        public IHttpActionResult GetFlightByDepartureDate([FromBody] DateTime departureTime)
        {
            Flight flight =  _anonimousUserFacade.GetFlightByDepartureDate(departureTime);
            if (flight == null) return NotFound();

            return Ok(flight);           
        }

        [ResponseType(typeof(Flight))]
        [HttpPost]
        [Route("GetFlightByLandingDate", Name = "AnonimousGetFlightByLandingDate")]
        public IHttpActionResult GetFlightByLandingDate([FromBody] DateTime landingDate)
        {
            Flight flight = _anonimousUserFacade.GetFlightByLandingDate(landingDate);
            if (flight == null) return NotFound();

            return Ok(flight);
        }

        [ResponseType(typeof(Flight))]
        [HttpPost]
        [Route("GetFlightByDestinationCountry", Name = "AnonimousGetFlightByDestinationCountry")]
        public IHttpActionResult GetFlightByDestinationCountry([FromBody] Country  destinationCountry)
        {
            Flight flight = _anonimousUserFacade.GetFlightByDestinationCountry(destinationCountry);
            if (flight == null) return NotFound();

            return Ok(flight);
        }

        [ResponseType(typeof(Flight))]
        [HttpGet]
        [Route("GetFlightByDestinationCountry/{destinationCountryCode}", Name = "AnonimousGetFlightByDestinationCountryCode")]
        public IHttpActionResult GetFlightByDestinationCountry([FromUri] int destinationCountryCode)
        {
            Flight flight = _anonimousUserFacade.GetFlightByDestinationCountry(destinationCountryCode);
            if (flight == null) return NotFound();

            return Ok(flight);
        }

        [ResponseType(typeof(Flight))]
        [HttpPost]
        [Route("GetFlightByOriginCountry", Name = "AnonimousGetFlightByOriginCountry")]
        public IHttpActionResult GetFlightByOriginCountry([FromBody] Country originCountry)
        {
            Flight flight = _anonimousUserFacade.GetFlightByOriginCountry(originCountry);
            if (flight == null) return NotFound();

            return Ok(flight);
        }

        [ResponseType(typeof(Flight))]
        [HttpGet]
        [Route("GetFlightByOriginCountry/{originCountryCode}", Name = "AnonimousGetFlightByOriginCountryCode")]
        public IHttpActionResult GetFlightByOriginCountry([FromUri] int originCountryCode)
        {
            Flight flight = _anonimousUserFacade.GetFlightByOriginCountry(originCountryCode);
            if (flight == null) return NotFound();

            return Ok(flight);            
        }



































        [ResponseType(typeof(Flight))]
        [HttpGet]
        [Route("GetFlightsByQueryGet", Name = "AnonimousGetFlightsByQueryGet")]
        //string searchQuery, SearchFlightsCriterion searchCriterion
        public async Task<IHttpActionResult> GetFlightsByQueryGet(string searchQuery = "United States", SearchFlightsCriterion searchCriterion = SearchFlightsCriterion.DestinationCountryName, SortFlightsCriterion sortingCriterion = SortFlightsCriterion.NoSorting)
        {

            searchQuery = "United States"; 
            searchCriterion = SearchFlightsCriterion.DestinationCountryName; 
            sortingCriterion = SortFlightsCriterion.NoSorting;
            //MessageBox.Show(AirlineName);
            List<Flight> flights = null;



            try
            {
                //Redis will be implemented in GetFlightsByQuery method
                /////
                /////
                /// Redis ///
                /// 

                flights = await _anonimousUserFacade.GetFlightsByQuery(searchQuery, searchCriterion);
                //flights = _anonimousUserFacade.GetAll<Flight>();
                if (flights == null || flights.Count == 0)
                {
                    string errorMessage = string.Empty;
                    switch (searchCriterion)
                    {
                        case SearchFlightsCriterion.AirlineName:
                            errorMessage = $"There is no airline company named {searchQuery}.";
                            break;
                        case SearchFlightsCriterion.DestinationCountryName:
                            errorMessage = $"There is no destination country named {searchQuery}";
                            break;
                        case SearchFlightsCriterion.OriginCountryName:
                            errorMessage = $"There is no origin country named {searchQuery}";
                            break;
                        case SearchFlightsCriterion.Adorning:
                            errorMessage = $"There is no airline company with the number {searchQuery}";
                            break;
                        case SearchFlightsCriterion.Indiscriminable:
                            errorMessage = "Sorry, but seems that is no flight exist in the system :(...";
                            break;
                        default:
                            errorMessage = $"Something went wrong. Appropriare search criterion isn't acquired. The search criterion must be a value of the enumeration \"SearchFlightsCriterion\"";
                            break;
                    }

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, errorMessage));
                }


                int threadsCount = 0;
                foreach (Flight flight in flights)
                {

                    Thread thread1 = new Thread(() => {
                        ConstructFlightDataObjectThreadPoolCallBack(flight);
                    });
                    Thread.Sleep(100);

                    thread1.Start();
                    threadsCount++;

                    //ConstructFlightDataObjectThreadPoolCallBack(flight);
                    /*Thread.Sleep(40);
                    ThreadPool.QueueUserWorkItem(new WaitCallback(ConstructFlightDataObjectThreadPoolCallBack), flight);
                    threadsCount++;*/



                }

            }
            catch (Exception ex)
            {
                if (flights == null)
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, $"Something went wrong on the server. Maybe {ex.GetType().Name} occured. If so, the message is: \n \"{ex.Message}\""));
                else
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.ExpectationFailed, $"Something went wrong on the server. Maybe {ex.GetType().Name} occured. If so, the message is: \n \"{ex.Message}\""));

            }

            var flyghtData_sArr = flightData_s.ToArray();

            //Here we will do sorting
            switch (sortingCriterion)
            {
                case SortFlightsCriterion.NoSorting:
                    //no action
                    break;
                case SortFlightsCriterion.ByPrice:
                    Array.Sort(flyghtData_sArr, new ComparerByNumericValuedProperty<FlightData>("Price"));
                    break;
                case SortFlightsCriterion.ByTakingOffTime:
                    Array.Sort(flyghtData_sArr, new ComparerByTimeValuedProperty<FlightData>("DEPARTURE_TIME"));
                    break;
                case SortFlightsCriterion.ByFlightLenght:
                    Array.Sort(flyghtData_sArr, new ComparerByTimeValuedProperty<FlightData>("FlightDuration"));
                    break;
            }

            return Ok(flyghtData_sArr);
            //return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, $"There is no tickets in the system."));
        }



    }
}
