using Flight_Center_Project_FinalExam_BL;
using Flight_Center_Project_FinalExam_DAL;
using Google.Apis.Auth;
using Microsoft.Ajax.Utilities;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Web_Api_interface.ApiControllers;
using Web_Api_interface.ApiControllers.JWTAutenticationAuxiliaries;
using static Google.Apis.Auth.GoogleJsonWebSignature;

namespace Web_Api_interface.Controllers.JWTAutenticationAuxiliaries
{
    public class TokenValidationHandler : DelegatingHandler
    {

        private TokenValidationHandlerAssistant _tokenValidationAssistant = new TokenValidationHandlerAssistant();

        private static bool TryRetrieveToken(HttpRequestMessage request, out string token)
        {
            token = null;
            IEnumerable<string> authzHeaders;
            //if (!request.Headers.TryGetValues("Authorization", out authzHeaders) || authzHeaders.Count() > 1 || authzHeaders.FirstOrDefault() == null || authzHeaders.FirstOrDefault().Contains("null"))
            if (!request.Headers.TryGetValues("Authorization", out authzHeaders) || authzHeaders.Count() > 1)
            {
                return false;
            }
            var bearerToken = authzHeaders.ElementAt(0);
            token = bearerToken.StartsWith("Bearer ") ? bearerToken.Substring(7) : bearerToken;
            return true;
        }

        private HttpStatusCode _statusCode;
        async protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            //HttpStatusCode _statusCode;
            string token;

            //chek if a token exists in the request header
            if (!TryRetrieveToken(request, out token))
            {
                _statusCode = HttpStatusCode.Unauthorized;
                //allow requests with no token - whether a action method needs an authentication can be set with the claimsauthorization attribute
                return await base.SendAsync(request, cancellationToken);
            }

            try
            {
                

                _statusCode = HttpStatusCode.OK;

                ClaimsPrincipal claimsPrincipal  = _tokenValidationAssistant.AssistTokenValidation(token); //here is the exception
                Thread.CurrentPrincipal = claimsPrincipal;
                HttpContext.Current.User = claimsPrincipal;

                return await base.SendAsync(request, cancellationToken);
            }
            /*catch(ArgumentException argEx)
            {
                if(argEx.Message == "IDX12741: JWT: 'System.String' must have three segments (JWS) or five segments (JWE)." && argEx.HResult == -2147024809)
                {
                    throw new SecurityTokenValidationException();
                }
            }*/

            //SecurityTokenValidationException
            catch (Exception ex)
            {
                if(!(ex is SecurityTokenValidationException))
                {
                    if(!(ex is ArgumentException) || ex.Message != "IDX12741: JWT: 'System.String' must have three segments (JWS) or five segments (JWE)." || ex.HResult != -2147024809)
                    {
                        _statusCode = HttpStatusCode.InternalServerError;
                        return await Task<HttpResponseMessage>.Factory.StartNew(() => new HttpResponseMessage(_statusCode) { }, cancellationToken);
                    }
                }


                /* the token is unathorized by this verification mechanism, 
                 * because it isn't the native token created on this machine by the local rules.
                 * Alternative verification mechanism required.
                 */
                Payload validPayload = null;
                try
                {
                    validPayload = await GoogleJsonWebSignature.ValidateAsync(token);
                }
                catch(InvalidJwtException)
                {
                    string resurrectedGoogleCredentialsBasedNativeToken = EncryptionProvider.Decryprt(File.ReadAllText("_fileOperationsAllowedHere/googleCredentialsBasedNativeToken.txt", Encoding.Default));
                    /*string currentUserName = EncryptionProvider.Decryprt(File.ReadAllText("_fileOperationsAllowedHere/1.txt", Encoding.Default));
                    string currentUserPassword = EncryptionProvider.Decryprt(File.ReadAllText("_fileOperationsAllowedHere/2.txt", Encoding.Default));
                    string currentUserKind = EncryptionProvider.Decryprt(File.ReadAllText("_fileOperationsAllowedHere/3.txt", Encoding.Default));*/



                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", resurrectedGoogleCredentialsBasedNativeToken);

                    ClaimsPrincipal authorisedClaimsPrincipal = _tokenValidationAssistant.AssistTokenValidation(resurrectedGoogleCredentialsBasedNativeToken);

                    /*ClaimsPrincipal unauthorisedUserPrincipal = new ClaimsPrincipal();
                    ClaimsIdentity identity = new ClaimsIdentity();
                    identity.AddClaim(new Claim(ClaimTypes.Name, currentUserName));
                    identity.AddClaim(new Claim(ClaimTypes.Role, currentUserKind));
                    identity.AddClaim(new Claim("Password", currentUserPassword));

                    unauthorisedUserPrincipal.AddIdentity(identity);

                    HttpContext.Current.User = unauthorisedUserPrincipal;
                    Thread.CurrentPrincipal = unauthorisedUserPrincipal;*/

                    Thread.CurrentPrincipal = authorisedClaimsPrincipal;
                    HttpContext.Current.User = authorisedClaimsPrincipal;

                    return await base.SendAsync(request, cancellationToken);
                }

                if (validPayload != null)
                {
                    UserValidator userValidator = new UserValidator();
                    bool isUsernamePasswordValid = userValidator.ValidateUser(username: validPayload.Name, password: validPayload.Email, out LoginToken<IPoco> systemLoginToken);
                    Utility_class_User validatedUserModel = systemLoginToken.UserAsUser;

                    if (isUsernamePasswordValid)
                    {
                        string jwtSecretkey = Convert.ToBase64String(Encoding.Default.GetBytes(ConfigurationManager.AppSettings["JWT_SecretKey"]));
                        JWTService jwtService = new JWTService(jwtSecretkey);

                        string googleCredentialsBasedNativeToken = jwtService.CreateToken(validatedUserModel);
                        _statusCode = HttpStatusCode.OK;

                        ClaimsPrincipal claimsPrincipal = _tokenValidationAssistant.AssistTokenValidation(googleCredentialsBasedNativeToken);

                        try
                        {
                            lock (this)
                            {
                                File.WriteAllText("_fileOperationsAllowedHere/googleCredentialsBasedNativeToken.txt", EncryptionProvider.Encrypt(googleCredentialsBasedNativeToken), Encoding.Default);
                                /*File.WriteAllText("_fileOperationsAllowedHere/1.txt", EncryptionProvider.Encrypt(validatedUserModel.USER_NAME), Encoding.Default);
                                File.WriteAllText("_fileOperationsAllowedHere/2.txt", EncryptionProvider.Encrypt(validatedUserModel.PASSWORD), Encoding.Default);
                                File.WriteAllText("_fileOperationsAllowedHere/3.txt", EncryptionProvider.Encrypt(validatedUserModel.USER_KIND), Encoding.Default);*/
                            }

                        }
                        catch(Exception exp)
                        {
                            var expName = exp.GetType().Name;
                        }

                        Thread.CurrentPrincipal = claimsPrincipal;
                        HttpContext.Current.User = claimsPrincipal;
                    }
                    else
                    {
                        _statusCode = HttpStatusCode.Unauthorized;

                        /*
                        Here is the place to tackle a new, unregistered user
                        */
                        Mailer mailer = new Mailer(validPayload.Email);

                        //"ValidateEmail" have to take email
                        var isEmailSentAnswer = mailer.SendEmailToUser();


                        ClaimsPrincipal unauthorisedUserPrincipal = new ClaimsPrincipal();
                        ClaimsIdentity identity = new ClaimsIdentity();                       
                        identity.AddClaim(new Claim(ClaimTypes.Name, validPayload.Name));
                        identity.AddClaim(new Claim(ClaimTypes.Email, validPayload.Email));

                        //Here we put the Email message into the current principal so making it aviliable through all the project
                        identity.AddClaim(new Claim(ClaimTypes.UserData, isEmailSentAnswer.OptionalProps["messageBody"]));
                        unauthorisedUserPrincipal.AddIdentity(identity);

                        HttpContext.Current.User = unauthorisedUserPrincipal;




                        Thread.CurrentPrincipal = unauthorisedUserPrincipal;
                    }

                    return await base.SendAsync(request, cancellationToken);
                }
                else
                    _statusCode = HttpStatusCode.Unauthorized;


            }
            /*catch (Exception)
            {
                _statusCode = HttpStatusCode.InternalServerError;
            }*/
            return await Task<HttpResponseMessage>.Factory.StartNew(() => new HttpResponseMessage(_statusCode) { }, cancellationToken);

        }

    }
}