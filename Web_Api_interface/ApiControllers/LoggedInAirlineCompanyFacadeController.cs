using Flight_Center_Project_FinalExam_BL;
using Flight_Center_Project_FinalExam_DAL;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Windows;
using Web_Api_interface.ApiControllers;
using Web_Api_interface.IControllers;
using Web_Api_interface.Models;
using Web_Api_interface.Views.FlightSystemMain;

namespace Web_Api_interface.Controllers
{ 
    [RoutePrefix("api/LoggedInAirlineCompanyFacade")]
    public class LoggedInAirlineCompanyFacadeController : LoggedInFacadeControllerBase, ILoggedInAirlineCompanyFacadeController
    {
        const string ENCRIPTION_PHRASE = "4r8rjfnklefjkljghggGKJHnif5r5242";

        private JsonToDictionaryConverter _jsonToDictionaryConverter = new JsonToDictionaryConverter();

        private LoggedInAirlineFacade _loggedInAirlineFacade;

        public LoggedInAirlineCompanyFacadeController()
        {
            _loggedInAirlineFacade = _fsc.getFacede<LoggedInAirlineFacade>();
        }

        #region Private Methods



        #endregion
        #region Public Methods

        [CustomAuthorize(Roles = "Customer, Administrator, AirlineCompany", UnathorisedRequestCusstomMessage = "asasasasasasasasasasasa")]
        [ResponseType(typeof(long))]
        [HttpGet]
        [Route("PreloadAllAirlineIDs", Name = "AirlinePreloadAllAirlineIDs")]
        public async Task<IHttpActionResult> PreloadAllAirlineIDs()
        {
            List<long> airlineIDs = null;
            Func<Task<List<long>>> func = async () =>
            {
                return await _loggedInAirlineFacade.PreloadAllAirlineIDsAsync();
            };
            airlineIDs = await ProcessExceptionsAsync(func);

            return Ok(airlineIDs);
        }

        [ResponseType(typeof(Flight))]
        [HttpGet]
        [Route("GetAllFlights", Name = "GetAllFlights")]
        [CustomAuthorize(Roles = "AirlineCompany")]
        public IHttpActionResult GetAllFlights()
        {

            bool isAuthorized = GetInternalLoginTokenInternal<AirlineCompany>(out LoginToken<AirlineCompany> loginTokenAirline);
            //string airlineName = Statics.Decrypt(loginTokenAirline.UserAsUser.USER_NAME, ENCRIPTION_PHRASE);
            //string airlineName = Statics.Encrypt("ABSA - Aerolinhas Brasileiras", ENCRIPTION_PHRASE);
            //ABSA - Aerolinhas Brasileiras
            //string airlinePass = Statics.Decrypt(loginTokenAirline.UserAsUser.PASSWORD, ENCRIPTION_PHRASE);

            List<Flight> flights = _loggedInAirlineFacade.GetAllFlightsByAirlineName(loginTokenAirline.UserAsUser.ID);
            //List<Flight> flights = _loggedInAirlineFacade.GetAllFlightsByAirlineName(21281L);
            if (flights == null || flights.Count == 0) return NotFound();

            return Ok(CreateFlightDataIE(flights));
        }
        [ResponseType(typeof(Flight))]
        [HttpGet]
        [Route("GetAllFlightsAsync", Name = "GetAllFlightsAsync")]
        [CustomAuthorize(Roles = "AirlineCompany")]
        public async Task<IHttpActionResult> GetAllFlightsAsync()
        {
            LoginToken<AirlineCompany> loginTokenAirline = await GetInternalLoginTokenInternalAsync<AirlineCompany>();

            List<Flight> flights = await _loggedInAirlineFacade.GetAllFlightsByAirlineNameAsync(loginTokenAirline.UserAsUser.ID);
            if (flights == null || flights.Count == 0) return NotFound();

            return Ok(CreateFlightDataIE(flights));
        }
        private IEnumerable<FlightData> CreateFlightDataIE(IEnumerable<Flight> flight)
        {
            foreach (Flight s in flight)
            {
                AirlineCompany company = _loggedInAirlineFacade.Get<AirlineCompany>(s.AIRLINECOMPANY_ID);
                Country departureCountry = _loggedInAirlineFacade.Get<Country>(s.ORIGIN_COUNTRY_CODE);
                Country destinationCountry = _loggedInAirlineFacade.Get<Country>(s.DESTINATION_COUNTRY_CODE);

                string image64base = null;
                try
                {
                    image64base = ImageRestorer.GetFormattedImage64baseString(company.IMAGE);
                }
                catch 
                {
                    image64base = ImageRestorer.GetFormattedImage64baseString(ImageRestorer.UnformatImage64BaseString(Statics.Base64ImageAbsenceImage));
                }

                yield return new FlightData
                {
                    iD = s.ID,
                    Adorning = company.ADORNING,
                    Image = image64base,
                    AirlineName = company.AIRLINE_NAME,
                    DepartureCountryName = departureCountry.COUNTRY_NAME,
                    DestinationCountryName = destinationCountry.COUNTRY_NAME,
                    EstimatedTime = s.LANDING_TIME.ToString(),
                    Price = s.PRICE,
                    RemainingTickets = s.REMAINING_TICKETS,

                    DEPARTURE_TIME = s.DEPARTURE_TIME,
                    LANDING_TIME = s.LANDING_TIME,
                    FlightDuration = s.LANDING_TIME - s.DEPARTURE_TIME,

                    AirlineCompany = company,
                    DepartureCountry = departureCountry,
                    DestinationCountry = destinationCountry,
                    
                    
                };
            }
        }
        private Flight FlightDataToFlight(FlightData flightData)
        {

            AirlineCompany company = _loggedInAirlineFacade.GetByAny<AirlineCompany>(flightData.AirlineName, "AIRLINE_NAME");
            Country originCountry = _loggedInAirlineFacade.GetByAny<Country>(flightData.DepartureCountryName, "COUNTRY_NAME");
            Country destinationCountry = _loggedInAirlineFacade.GetByAny<Country>(flightData.DestinationCountryName, "COUNTRY_NAME");


            Flight flight = new Flight 
            {
                ID = flightData.iD,
                AIRLINECOMPANY_ID = company.ID,
                DEPARTURE_TIME = flightData.DEPARTURE_TIME,
                LANDING_TIME = flightData.LANDING_TIME,
                DESTINATION_COUNTRY_CODE = destinationCountry.ID,
                ORIGIN_COUNTRY_CODE = originCountry.ID,
                PRICE = flightData.Price,
                REMAINING_TICKETS = flightData.RemainingTickets,
            };
            return flight;
        }

        [ResponseType(typeof(Ticket))]
        [Route("GetAllTickets", Name = "GetAllTickets")]
        [HttpGet]
        [Authorize(Roles = "AirlineCompany")]
        public IHttpActionResult GetAllTickets()
        {
            IList<Ticket> ticketLst = null;
            bool isAuthorized = false;
            Action act = () =>
            {
                isAuthorized = GetInternalLoginTokenInternal<AirlineCompany>(out LoginToken<AirlineCompany> loginTokenAirline);
                if (isAuthorized) ticketLst = _loggedInAirlineFacade.GetAllTickets(loginTokenAirline);
            };
            ProcessExceptions(act);
            if (!isAuthorized) return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, $"Sorry, but you're not an Airline Company. Your accsess is denied."));
            if (ticketLst == null) return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotFound, $"There is no tickets in the system."));

            return Ok(ticketLst);
        }

        [Route("CancelFlight", Name = "CancelFlight")]
        [HttpDelete]
        [Authorize(Roles = "AirlineCompany")]
        public IHttpActionResult CancelFlight([FromBody]Flight flight)
        {
            bool isAuthorized = false;
            bool isCanceled = false;
            Action act = () =>
            {
                isAuthorized = GetInternalLoginTokenInternal<AirlineCompany>(out LoginToken<AirlineCompany> loginTokenAirline);

                if (isAuthorized)
                {
                    isCanceled = _loggedInAirlineFacade.CancelFlight(loginTokenAirline, flight);
                }
            };
            ProcessExceptions(act);
            if (!isAuthorized) return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, $"Sorry, but you're not an Airline Company. Ypur accsess is denied."));

            if (!isCanceled) return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.NotFound, $"The flight number \"{flight.ID}\" alredy not exists, so can't be deleted"));

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, $"The flight number \"{flight.ID}\" has been deleted"));
        }

        [Route("CreateFlight", Name = "CreateFlight")]
        [HttpPost]
        [Authorize(Roles = "AirlineCompany")]
        public IHttpActionResult CreateFlight([FromBody]Flight flight)
        {
            bool isAuthorized = false;
            bool isCreated = false;
            Action act = () =>
            {
                isAuthorized = GetInternalLoginTokenInternal<AirlineCompany>(out LoginToken<AirlineCompany> loginTokenAirline);

                if (isAuthorized)
                {
                    isCreated = _loggedInAirlineFacade.CreateFlight(loginTokenAirline, flight);
                }
            };
            ProcessExceptions(act);
            if (!isAuthorized) return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, $"Sorry, but you're not an Airline Company. Your accsess is denied."));

            if (!isCreated) return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, $"Sorry, but the flight number \"{flight.ID}\" doesn't created."));

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, $"The flight number \"{flight.ID}\" has been created"));
        }

        [Route("UpdateFlight", Name = "UpdateFlight")]
        [HttpPost]
        [Authorize(Roles = "AirlineCompany")]
        public IHttpActionResult UpdateFlight([FromBody]FlightData flightData)
        {
            Flight flight = FlightDataToFlight(flightData);

            

            bool isAuthorized = false;
            bool isUpdated = false;
            bool isFound = false;
            Action act = () =>
            {
                isAuthorized = GetInternalLoginTokenInternal<AirlineCompany>(out LoginToken<AirlineCompany> loginTokenAirline);

                if (isAuthorized)
                {
                    isUpdated = _loggedInAirlineFacade.UpdateFlight(loginTokenAirline, flight, out isFound);
                }
            };
            ProcessExceptions(act);
            if (!isAuthorized) return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, $"Sorry, but you're not an Airline Company. Your accsess is denied."));

            if (!isFound) return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.NotFound, $"Sorry, but the flight number \"{flight.ID}\" can't be update beause it don't exists in the system in the first place."));

            if (!isUpdated) return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.NotModified, $"Sorry, but the flight number \"{flight.ID}\" doesn't updated."));

            AirlineCompany airline = _loggedInAirlineFacade.Get<AirlineCompany>(flight.AIRLINECOMPANY_ID);
            Country departureCountry = _loggedInAirlineFacade.Get<Country>(flight.ORIGIN_COUNTRY_CODE);
            Country destinationCountry = _loggedInAirlineFacade.Get<Country>(flight.DESTINATION_COUNTRY_CODE);

            FlightData updatedFlightData = CreateFlightDataIE(new Flight[] { flight }).FirstOrDefault();

            return Ok(updatedFlightData);
            //return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, $"The flight number \"{flight.ID}\" has been updated. Now it seems like that: \n\n {_loggedInAirlineFacade.GetFlightById(flight.ID)}\n\nEnjoy it!"));
        }

        /// <summary>
        /// This method takes argument as JObject in thuis format:
        /// {
        ///  "oldpass": "pld password",
        ///  "newpass": "new password",
        /// }
        /// </summary>
        /// <param name="credentials">JObject in format: ["oldpass" : "old password", "newpass": "new password"]</param>
        /// <returns></returns>
        [Route("ChangeMypassword", Name = "ChangeMypassword")]
        [HttpPost]
        [Authorize(Roles = "AirlineCompany")]
        public IHttpActionResult ChangeMypassword([FromBody] JObject olsAndNewPassword)
        {
            if (olsAndNewPassword == null) return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.UnsupportedMediaType, $"Sorry, but your credentials came in unsupported format."));

            Dictionary<string, object> credentialsDataDict = _jsonToDictionaryConverter.ProvideAPIDataFromJSON(olsAndNewPassword);

            string oldPass = string.Empty;
            string newPass = string.Empty;
            int count = 0;
            bool isContainsRequiredWords = true;
            foreach(var s in credentialsDataDict)
            {
                if (s.Key.Contains("oldpass")) oldPass = s.Value.ToString();
                else isContainsRequiredWords = false;
                if (s.Key.Contains("newpass")) newPass = s.Value.ToString();
                else isContainsRequiredWords = false;

                count++;
            }

            if (isContainsRequiredWords == false) return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.UnsupportedMediaType, $"Your credentials must contain words \"oldpass\" and \"newpass\" "));
            if(count != 2) return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.UnsupportedMediaType, $"Your credentials must be a JObject with 2 properties (no more no less)"));

            bool isAuthorized = false;
            bool isChanged = false;
            bool isPasswordWrong = true;
            Action act = () =>
            {
                isAuthorized = GetInternalLoginTokenInternal<AirlineCompany>(out LoginToken<AirlineCompany> loginTokenAirline);

                if (isAuthorized)
                {
                    isChanged = _loggedInAirlineFacade.ChangeMyPassword(loginTokenAirline, oldPass, newPass, out isPasswordWrong);
                }
            };
            ProcessExceptions(act);
            if (!isAuthorized) return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, $"Sorry, but you're not an Airline Company. Your accsess is denied."));

            if (isPasswordWrong) return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, $"Sorry, but the password \"{oldPass}\" is wrong. You need to feed the right password in order to change it."));

            if (!isChanged) return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "Sorry, but your password does not changed"));

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, $"your password changed to \"{newPass}\" "));
        }

        [Route("MofidyAirlineDetails", Name = "MofidyAirlineDetails")]
        [HttpPut]
        [Authorize(Roles = "AirlineCompany")]
        public IHttpActionResult MofidyAirlineDetails([FromBody]AirlineCompany airline)
        {
            bool isAuthorized = false;
            bool isModified = false;
            Action act = () =>
            {
                isAuthorized = GetInternalLoginTokenInternal<AirlineCompany>(out LoginToken<AirlineCompany> loginTokenAirline);

                if (isAuthorized)
                {
                    isModified = _loggedInAirlineFacade.MofidyAirlineDetails(loginTokenAirline, airline);
                }
            };
            ProcessExceptions(act);
            if (!isAuthorized) return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, $"Sorry, but you're not an Airline Company. Your accsess is denied."));

            if(!isModified) return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, $"Sorry, but the Airline isn't modified"));

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, $"The Airline number {airline.ID} is modified"));
        }

        #endregion




    }
}
