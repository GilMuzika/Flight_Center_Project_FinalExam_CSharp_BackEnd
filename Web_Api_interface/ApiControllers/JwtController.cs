using Flight_Center_Project_FinalExam_BL;
using Flight_Center_Project_FinalExam_DAL;
using Google.Apis.Auth;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.UI.WebControls;
using Web_Api_interface.ApiControllers.JWTAutenticationAuxiliaries;
using Web_Api_interface.Controllers.JWTAutenticationAuxiliaries;
using Web_Api_interface.Models;
using Web_Api_interface.Views.FlightSystemMain;
using static Google.Apis.Auth.GoogleJsonWebSignature;

namespace Web_Api_interface.Controllers
{
    [RoutePrefix("api/jwt")]
    public class JwtController : ApiController
    {
        private JsonToDictionaryConverter _jsonToDictionaryConverter = new JsonToDictionaryConverter();


        /// <summary>
        /// "SecretKey" string needed to initialize the "JWTService" service that creates the JWT token
        /// </summary>
        private string Secretkey = Convert.ToBase64String(Encoding.Default.GetBytes(ConfigurationManager.AppSettings["JWT_SecretKey"]));
        private JWTService _jwtService;

        public JwtController()
        {
            _jwtService = new JWTService(Secretkey);
        }

        private UserValidator _userValidator = new UserValidator();

        [HttpGet]
        [Authorize]
        [Route("ok")]
        public IHttpActionResult Authenticated() => Ok("Authenticated");

        [HttpGet]
        [Route("notok")]
        public IHttpActionResult NotAuthorized() => Ok("Unauthorized");



        [HttpGet]
        [Route("getResurrectedGoogleCredentialsBasedNativeToken")]
        public IHttpActionResult GetResurrectedGoogleCredentialsBasedNativeToken()
        {
            string resurrectedGoogleCredentialsBasedNativeToken = EncryptionProvider.Decryprt(File.ReadAllText("_fileOperationsAllowedHere/googleCredentialsBasedNativeToken.txt", Encoding.Default));

                
                TokenValidationHandlerAssistant tokenValidationAssistant = new TokenValidationHandlerAssistant();
                ClaimsPrincipal authorisedClaimsPrincipal = tokenValidationAssistant.AssistTokenValidation(resurrectedGoogleCredentialsBasedNativeToken);
                string currentUserName = authorisedClaimsPrincipal.Identity.Name;
                string currentUserRole = "NOT DETERMINED YET";
                try
                {
                    if (authorisedClaimsPrincipal.Identity.GetType().GetField("m_instanceClaims", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance) != null)
                        currentUserRole = ((List<Claim>)(authorisedClaimsPrincipal.Identity.GetType().GetField("m_instanceClaims", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(authorisedClaimsPrincipal.Identity)))[1].Value;
                }
                catch (NullReferenceException)
                {

                }

            TokenOwnerInformation ownerInfo = new TokenOwnerInformation();
            ownerInfo.USER_KIND = currentUserRole;
            switch(currentUserRole)
            {
                case "Administrator":
                    ownerInfo.AdministratorName = currentUserName;
                    break;
                case "AirlineCompany":
                    ownerInfo.AirlineCompanyName = currentUserName;
                    break;
                case "Customer":
                    if (currentUserName.Contains(' '))
                    {
                        string[] firstAndlastNames = currentUserName.Split(' ');
                        ownerInfo.CustomerFirstName = firstAndlastNames[0];
                        ownerInfo.CustomerLastName = firstAndlastNames[1];
                    }
                    else
                    {
                        ownerInfo.CustomerFirstName = currentUserName;
                    }
                    break;
            }

            return Ok(ownerInfo);
        }

        /// <summary>
        /// This method takes argument as JObject in thuis format:
        /// {
        ///  "username": "actual username",
        ///  "password": "actual password",
        /// }
        /// </summary>
        /// <param name="credentials">JObject in format: ["username" : "actual username", "password": "actual password"]</param>
        /// <returns></returns>
        [HttpPost]
        [Route("createJwtToken")]
        public IHttpActionResult Authenticate([FromBody] JObject credentials)
        {
            if (credentials == null) return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.UnsupportedMediaType, $"Sorry, but your credentials came in unsupported format."));

            Dictionary<string, object> credentialsData = _jsonToDictionaryConverter.ProvideAPIDataFromJSON(credentials);

            string username = string.Empty;
            string password = string.Empty;

            foreach(var s in credentialsData)
            {
                if (s.Key.Contains("username")) username = s.Value.ToString();
                if (s.Key.Contains("password")) password = s.Value.ToString();
            }



            // if there was an attempt to acsess the DB without sending any username and password, 
            // probably because the user didn't try to sign in altogether, 
            // bypassing the login form. 
            // When the client JS script try to approach the server without given any username and password parameters, 
            // it sends empty strings instead.
            if(password == string.Empty && username == string.Empty)
            {
                //password = "password";
                //username = "username";
                return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "Pease sign up or sign in"));
            }

            bool isUsernamePasswordValid = _userValidator.ValidateUser(username, password, out LoginToken<IPoco> validatedUserModel);

            //if credentials are invalid
            if (!isUsernamePasswordValid)
            {
                FailedAttemptsFacade failedFacade = FlyingCenterSystem.GetInstance().getFacede<FailedAttemptsFacade>();

                FailedLoginAttempt attemptByPassword = failedFacade.GetByPassword(password);
                FailedLoginAttempt attempByUsername = failedFacade.GetByUserName(username);
                

                long failedAttemptNum = 0;
                long failedAttemptNumToDisplay = 1;

                bool isTheAttemptIsFirst = attemptByPassword == null ? true : attemptByPassword.Equals(new FailedLoginAttempt());  
                


                if(isTheAttemptIsFirst)
                {
                    failedFacade.AddBlackUser(new FailedLoginAttempt(username, password, 1, Guid.NewGuid().ToString(), DateTime.Now));
                }
                else
                {
                    //bool attemptsComparsion = Statics.BulletprofComparsion(attemptByPassword, attempByUsername);
                    //if (!attemptsComparsion) return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Your username or password is incorrect, also there is no consistency between them! Acsess denied."));



                    //long.TryParse(ConfigurationManager.AppSettings["Permitted_Login_Attempts_Num"], out long permittedLOginAttempts);
                    if (attemptByPassword.FAILED_ATTEMPTS_NUM <= 3)
                    {
                        failedAttemptNum = attemptByPassword.FAILED_ATTEMPTS_NUM;
                        failedAttemptNumToDisplay = failedAttemptNum;
                        failedAttemptNum++;                        
                        attemptByPassword.FAILED_ATTEMPTS_NUM = failedAttemptNum;
                        bool isUpdated = failedFacade.UpdateBlackUser(attemptByPassword);
                    }
                    else
                    {                        
                        if (DateTime.Now.AddDays(-1) < attemptByPassword.FAILURE_TIME) 
                        {
                            TimeSpan timeRemainder = new TimeSpan(24, 0, 0) - DateTime.Now.Subtract(attemptByPassword.FAILURE_TIME);
                            return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, $"Sorry, but the system didn't regocnyzed you as registered user. Your accsess is denied. You're had tried to aouthorize more tham 3 times. Wait {timeRemainder.Hours} hours and {timeRemainder.Minutes} minutes until new attempt!"));
                        }
                        else
                        {
                            failedAttemptNum = 1;                            
                            attemptByPassword.FAILED_ATTEMPTS_NUM = failedAttemptNum;
                            attemptByPassword.FAILURE_TIME = DateTime.Now;
                            bool updated = failedFacade.UpdateBlackUser(attemptByPassword);
                        }
                        
                    }
                }



                return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, $"Sorry, but the system didn't regocnyzed you as registered user. Your accsess is denied. You're had tried to aouthorize {failedAttemptNumToDisplay} times."));
            }

            //if credentials are valid
            LoginResponseVM loginResponce = null;
            if (isUsernamePasswordValid)
            {
                TokenOwnerInformation ownerInfo = new TokenOwnerInformation();
                ownerInfo.PASSWORD = validatedUserModel.UserAsUser.PASSWORD.Length > 50 ? EncryptionProvider.Decryprt(validatedUserModel.UserAsUser.PASSWORD) : validatedUserModel.UserAsUser.PASSWORD;
                ownerInfo.USER_NAME = validatedUserModel.UserAsUser.USER_NAME.Length > 50 ? EncryptionProvider.Decryprt(validatedUserModel.UserAsUser.USER_NAME) : validatedUserModel.UserAsUser.USER_NAME;
                ownerInfo.USER_KIND = validatedUserModel.UserAsUser.USER_KIND;
                switch (validatedUserModel.ActualUser.GetType().Name)
                {
                    case "Administrator":
                        var actualAdmin = validatedUserModel.ActualUser as Administrator;
                        ownerInfo.AdministratorName = actualAdmin.NAME;
                        break;
                    case "AirlineCompany":
                        var actualAirline = validatedUserModel.ActualUser as AirlineCompany;
                        ownerInfo.AirlineCompanyName = actualAirline.AIRLINE_NAME;
                        ownerInfo.Image = ImageRestorer.GetFormattedImage64baseString(actualAirline.IMAGE);
                        break;
                    case "Customer":
                        var actualCustomer = validatedUserModel.ActualUser as Customer;
                        ownerInfo.CustomerFirstName = actualCustomer.FIRST_NAME;
                        ownerInfo.CustomerLastName = actualCustomer.LAST_NAME;
                        ownerInfo.Image = ImageRestorer.GetFormattedImage64baseString(actualCustomer.IMAGE);
                        break;
                }

                var token = _jwtService.CreateToken(validatedUserModel.UserAsUser);
                loginResponce = new LoginResponseVM(new HttpResponseMessage(HttpStatusCode.OK), token, ownerInfo);
                return Ok(loginResponce);
            }
            loginResponce = new LoginResponseVM();
            //if credentials are nt valid send unathorized status code in response
            loginResponce.ResponseMsg.StatusCode = HttpStatusCode.Unauthorized;
            IHttpActionResult response = ResponseMessage(loginResponce.ResponseMsg);
            return response;
        }


    }


}
