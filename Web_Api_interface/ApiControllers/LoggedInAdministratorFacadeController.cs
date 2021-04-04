using EasyNetQ;
using Flight_Center_Project_FinalExam_BL;
using Flight_Center_Project_FinalExam_DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Windows;
using Web_Api_interface.ApiControllers;
using Web_Api_interface.Models;
using Web_Api_interface.Views.FlightSystemMain;

namespace Web_Api_interface.Controllers
{
    [RoutePrefix("api/LoggedInAdministratorFacade")]
    [CustomAuthorize(Roles = "Administrator")]
    public class LoggedInAdministratorFacadeController : LoggedInFacadeControllerBase
    {
        private Random _rnd = new Random();

        private LoggedInAdministratorFacade _loggedInAdministratorFacade;

        public LoggedInAdministratorFacadeController()
        {
            _loggedInAdministratorFacade = _fsc.getFacede<LoggedInAdministratorFacade>();
        }

        #region Private Methods

        private void GenerateUtility_class_UserPasswordAndName(out string nameCrypt, out string passwordCrypt)
        {
            string nameForEncription = Statics.GetUniqueKeyOriginal_BIASED(_rnd.Next(5, 8));
            string passForEncription = Statics.GetUniqueKeyOriginal_BIASED(_rnd.Next(5, 15));

            string encryptedName = EncryptionProvider.Encrypt(nameForEncription);                
            string encryptedPassword = EncryptionProvider.Encrypt(passForEncription);            
            nameCrypt = encryptedName;
            passwordCrypt = encryptedPassword;
        }

        #endregion
        #region Public Methods

        [Route("CheckToken", Name = "CheckToken")]
        [HttpGet]
        public IHttpActionResult CheckToken()
        {
            return Ok("You are Administator");
        }

        [Route("CreateNewAirline", Name = "CreateNewAirline")]
        [HttpPost]
        public IHttpActionResult CreateNewAirline([FromBody] AirlineCompany airline)
        {
            bool isAuthorized = false;
            bool isCreated = false;
            bool isAirlineAlreadyExists = false;
            Action act = () =>
            {
                isAuthorized = GetInternalLoginTokenInternal<Administrator>(out LoginToken<Administrator> loginTokenAdministrator);

                if (isAuthorized)
                {
                    GenerateUtility_class_UserPasswordAndName(out string nameCrypt, out string passCrypt);
                    isCreated = _loggedInAdministratorFacade.CreateNewAirline(loginTokenAdministrator, airline, nameCrypt, passCrypt, out isAirlineAlreadyExists);
                }
            };
            ProcessExceptions(act);
            if (!isAuthorized) return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, $"Sorry, but you're not an Administrator. Your accsess is denied."));

            if(isAirlineAlreadyExists) return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, $"This Airline didn't added to the system because it's already exists in it."));

            if(!isCreated) return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, $"Sorry, but this Airline didn't added to the system"));

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, "The Airline has been added to the system sucsessfully"));
        }

        [Route("UpdayeAirlineDetails", Name = "UpdayeAirlineDetails")]
        [HttpPost]
        public IHttpActionResult UpdayeAirlineDetails([FromBody]AirlineCompany airline)
        {
            bool isAuthorized = false;
            bool isUpdated = false;
            bool isAirlineExists = false;
            Action act = () =>
            {
                isAuthorized = GetInternalLoginTokenInternal<Administrator>(out LoginToken<Administrator> loginTokenAdministrator);

                if (isAuthorized)
                {
                    Utility_class_User airlineAsUser = _loggedInAdministratorFacade.GetRegisteredUserDetails(airline.USER_ID);
                    isUpdated = _loggedInAdministratorFacade.UpdateAirlineDetails(loginTokenAdministrator, airline, airlineAsUser.USER_NAME, airlineAsUser.PASSWORD, out isAirlineExists);
                }
            };
            ProcessExceptions(act);
            if (!isAuthorized) return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, $"Sorry, but you're not an Administrator. Your accsess is denied."));

            if (!isAirlineExists) return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.NotFound, $"Sorry, but this Airline doesn't exists in the system"));

            if (!isUpdated) return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.NotModified, $"Sorry, but this Airline (number {airline.ID}) didn't modified"));

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, $"The details of the Airline (number {airline.ID}) was updated sucsessfully."));
        }

        [Route("RemoveAirline", Name = "RemoveAirline")]
        [HttpDelete]
        public IHttpActionResult RemoveAirline([FromBody]AirlineCompany airline)
        {
            bool isAuthorized = false;
            bool isRemoved = false;
            bool isAirlineExists = false;
            Action act = () =>
            {
                isAuthorized = GetInternalLoginTokenInternal<Administrator>(out LoginToken<Administrator> loginTokenAdministrator);

                if (isAuthorized)
                {
                    isRemoved = _loggedInAdministratorFacade.RemoveAirline(loginTokenAdministrator, airline, out isAirlineExists);
                }
            };
            ProcessExceptions(act);
            if (!isAuthorized) return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, $"Sorry, but you're not an Administrator. Your accsess is denied."));

            if (!isAirlineExists) return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.NotFound, $"THis Airline can't be removed because it isn't exists in the systen in the first place"));

            if(!isRemoved) return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, $"Sorry, but this Airline(number {airline.ID}) didn't removed."));

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, $"The Airline number {airline.ID} has been removed sucsessfully."));
        }

        [Route("Administrator", Name = "Administrator")]
        [HttpPost]
        public IHttpActionResult CreateNewCustomer([FromBody] Customer customer)
        {
            bool isAuthorized = false;
            bool isCreated = false;
            bool isAirlineExists = false;
            Action act = () =>
            {
                isAuthorized = GetInternalLoginTokenInternal<Administrator>(out LoginToken<Administrator> loginTokenAdministrator);

                if (isAuthorized)
                {
                    GenerateUtility_class_UserPasswordAndName(out string nameCrypt, out string passCrypt);
                    isCreated = _loggedInAdministratorFacade.CreateNewCustomer(loginTokenAdministrator, customer, nameCrypt, passCrypt, out isAirlineExists);
                }
            };
            ProcessExceptions(act);
            if (!isAuthorized) return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, $"Sorry, but you're not an Administrator. Your accsess is denied."));

            if (isAirlineExists) return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, $"Such a customer (number {customer.ID}) can't be created because it's already exists in the system."));

            if (!isCreated) return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, $"Sorry but the customer with the number {customer.ID} didn't created"));

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, $"The customer number {customer.ID} has been created sucsessfully."));
        }



        /// <summary>
        /// ///////////////  UpdateCustomerDetails  //////////////
        /// </summary>
        /// <param name="customerData"></param>
        /// <returns></returns>
        [Route("UpdateCustomerDetails", Name = "UpdateCustomerDetails")]
        [HttpPost]
        public async Task<IHttpActionResult> UpdateCustomerDetails([FromBody] CustomerData customerData)
        {
            Task<string> tsk = null;
            tsk = _loggedInAdministratorFacade.PleaseGiveMeOneAsync<Customer>(customerData.iD);
            string rabbitSubscriptionId = await tsk;
            // - old usage - important to get customer from the database based on the ID and notjust create a new instance with default values
            // - old usage - Customer customer = _loggedInAdministratorFacade.Get<Customer>(customerData.iD);

            
            Customer customer = null;
            Action act = () =>
            {

                //, out string rabbitSubscriptionId
                using (IBus bus = RabbitHutch.CreateBus("host=localhost"))
                {
                    while (customer == null)
                    {
                        Thread.Sleep(100);
                        ISubscriptionResult subscriptionResult = bus.SubscribeAsync<Customer>(rabbitSubscriptionId, allCustomersLst => { return Task.Run(() =>  customer = allCustomersLst ); });
                    }
                }
            };
            ProcessExceptions(act);


            customer.ADDRESS = customerData.Address;
            customer.FIRST_NAME = customerData.FirstName;
            customer.LAST_NAME = customerData.LastName;
            customer.PHONE_NO = customerData.Phone;
            customer.IMAGE = ImageRestorer.UnformatImage64BaseString(customerData.Image);


            bool isAuthorized = false;
            bool isUpdated = false;
            bool isCustomerExists = false;
            act = () =>
            {
                isAuthorized = GetInternalLoginTokenInternal<Administrator>(out LoginToken<Administrator> loginTokenAdministrator);

                if (isAuthorized)
                {
                    Utility_class_User customerAsUser = _loggedInAdministratorFacade.GetRegisteredUserDetails(customer.USER_ID);
                    isUpdated = _loggedInAdministratorFacade.UpdateCustomerDetails(loginTokenAdministrator, customer, customerAsUser.USER_NAME, customerAsUser.PASSWORD, out isCustomerExists);
                }
            };
            ProcessExceptions(act);
            if (!isAuthorized) return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, $"Sorry, but you're not an Administrator. Your accsess is denied."));

            if (!isCustomerExists) return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.NotFound, $"Sorry, but this Customer doesn't exists in the system"));

            if (!isUpdated) return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.NotModified, $"Sorry, but this Customer (number {customer.ID}) didn't modified"));

            Customer updated = _loggedInAdministratorFacade.Get<Customer>(customerData.iD);

            CustomerData updatedCustomerData = new CustomerData
            {
                Address = updated.ADDRESS,
                FirstName = updated.FIRST_NAME,
                LastName = updated.LAST_NAME,
                Image = ImageRestorer.GetFormattedImage64baseString(updated.IMAGE),
                iD = updated.ID,
                Phone = updated.PHONE_NO
            };

            return Ok(updatedCustomerData);
            //return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, $"The details of the Customer (number {customer.ID}) was updated sucsessfully."));
        }

        [Route("RemoveCustomer", Name = "RemoveCustomer")]
        [HttpDelete]
        public IHttpActionResult RemoveCustomer([FromBody]Customer customer)
        {
            bool isAuthorized = false;
            bool isRemoved = false;
            bool isCustomerExists = false;
            Action act = () =>
            {
                isAuthorized = GetInternalLoginTokenInternal<Administrator>(out LoginToken<Administrator> loginTokenAdministrator);

                if (isAuthorized)
                {
                    isRemoved = _loggedInAdministratorFacade.RemoveCustomer(loginTokenAdministrator, customer, out isCustomerExists);
                }
            };
            ProcessExceptions(act);
            if (!isAuthorized) return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, $"Sorry, but you're not an Administrator. Your accsess is denied."));

            if (!isCustomerExists) return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.NotFound, $"THis customer can't be removed because it isn't exists in the systen in the first place"));

            if (!isRemoved) return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, $"Sorry, but this customer(number {customer.ID}) didn't removed."));

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, $"The customer number {customer.ID} has been removed sucsessfully."));
        }

        /// <summary>
        /// //////////////// GetAllCustomers //////////////////
        /// </summary>
        /// <returns></returns>
        [CustomAuthorize(Roles = "Administrator", UnathorisedRequestCusstomMessage = "asasasasasasasasasasasa")]
        [ResponseType(typeof(Customer))]
        [HttpGet]
        [Route("GetAllAirlineCompanies", Name = "GetAllAirlineCompanies")]
        public IHttpActionResult GetAllAirlineCompanies()
        {
            List<AirlineCompany> airlines = null;
            Task tsk = null;
            int count = 0;
            Action act = () =>
            {
                //first of all we need to ask "LoggenInCustomerFacase" to produce the bus withthe needed object
                tsk = _loggedInAdministratorFacade.PleaseGiveMeAll<AirlineCompany>(out string rabbitSubscriptionId);

                //using (var bus = RabbitHutch.CreateBus("host=localhost;virtualHost=stndard_vhost;username=standard_user;password=guest"))
                using (IBus bus = RabbitHutch.CreateBus("host=localhost"))
                {
                    while (airlines == null)
                    {
                        if (count > 100)
                            break;

                        Thread.Sleep(100);
                        var subsciptionRezult = bus.SubscribeAsync<List<AirlineCompany>>(rabbitSubscriptionId, allAirlinesLst => { return Task.Run(() => { airlines = allAirlinesLst; }); });
                        count++;
                    }
                }

                //older implementation of taking customer list, without RabbitMQ
                //customers = _loggedInCustomerFacade.GetAll<Customer>();
            };
            ProcessExceptions(act);
            if (airlines == null && count > 99)
                return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, $"Sorry, there were more than {count} attempts to get messages"));

            if (airlines == null) return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, $"Sorry, but something wrong happened on the server, so your request can't be answered."));
            if (airlines.Count == 0) return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.NoContent, $"There are no customers in the system."));

            //List<CustomerData> customersData = new List<CustomerData>();

            return Ok(CreateAirlineDataIE(airlines));
        }
        private IEnumerable<AirlineCompanyData> CreateAirlineDataIE(IEnumerable<AirlineCompany> airlines)
        {
            foreach (AirlineCompany s in airlines)
            {
                string image64base = null;
                try
                {
                    image64base = ImageRestorer.GetFormattedImage64baseString(s.IMAGE);
                }
                catch 
                {
                    continue;
                    //image64base = "data:image/jpeg;base64,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";
                }

                yield return new AirlineCompanyData
                {
                    iD = s.ID,
                    Image = image64base,
                    Adorning = s.ADORNING,
                    AirlineName = s.AIRLINE_NAME,
                    BaseCountryName = _loggedInAdministratorFacade.Get<Country>(s.COUNTRY_CODE).COUNTRY_NAME,
                    AirlineAsUtilityClassUser = _loggedInAdministratorFacade.Get<Utility_class_User>(s.USER_ID)
                };
            }
        }



        [CustomAuthorize(Roles = "Administrator", UnathorisedRequestCusstomMessage = "asasasasasasasasasasasa")]
        [HttpGet]
        [Route("TakeOneCustomer/{airlineID}")]
        public async Task<IHttpActionResult> TakeOneAirlineCompany([FromUri] int airlineID)
        {
            return await Task.Run(async () =>
            {
                Task<string> tsk = null;
                tsk = _loggedInAdministratorFacade.PleaseGiveMeOneAsync<AirlineCompany>(airlineID);
                string rabbitSubscriptionId = await tsk;
                AirlineCompany airline = null;
                Action act = () =>
                {

                    using (IBus bus = RabbitHutch.CreateBus("host=localhost"))
                    {
                        while (airline == null)
                        {
                            Thread.Sleep(100);
                            ISubscriptionResult subscriptionResult = bus.SubscribeAsync<AirlineCompany>(rabbitSubscriptionId, oneAirline => { return Task.Run(() => airline = oneAirline); });
                        }
                    }
                };
                ProcessExceptions(act);

                return Ok(AirlineToAirlineData(airline));
            });
        }
        private AirlineCompanyData AirlineToAirlineData(AirlineCompany airline)
        {

            AirlineCompanyData airlineData = new AirlineCompanyData();
            airlineData.iD = airline.ID;
            airlineData.Adorning = airline.ADORNING;
            airlineData.AirlineName = airline.AIRLINE_NAME;
            airlineData.BaseCountryName = _loggedInAdministratorFacade.Get<Country>(airline.COUNTRY_CODE).COUNTRY_NAME;
            airlineData.Image = null;
            try
            {
                airlineData.Image = ImageRestorer.GetFormattedImage64baseString(airline.IMAGE);
            }
            catch 
            {
                airlineData.Image = "data:image/jpeg;base64,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";
            }

            airlineData.AirlineAsUtilityClassUser = _loggedInAdministratorFacade.Get<Utility_class_User>(airline.USER_ID);

            return airlineData;
        }

        [CustomAuthorize(Roles = "Administrator", UnathorisedRequestCusstomMessage = "asasasasasasasasasasasa")]
        [ResponseType(typeof(long))]
        [HttpGet]
        [Route("PreloadAllAirlineCompaniesIDs", Name = "AirlineCompanyPreloadAllAirlineCompaniesIDs")]
        public IHttpActionResult PreloadAllAirlineCompaniesIDs()
        {
            List<long> airlinesIDs = null;
            Action act = () =>
            {
                airlinesIDs = _loggedInAdministratorFacade.PreloadAllAirlineCompaniesIDs();
            };
            ProcessExceptions(act);

            return Ok(airlinesIDs);
        }

        #endregion
    }
}
