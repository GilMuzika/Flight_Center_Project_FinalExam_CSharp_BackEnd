using Google.Apis.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.UI.WebControls;

namespace Web_Api_interface.ApiControllers
{
    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        //this is the custom message that hardcoded into the class that uses the attribute
        public string UnathorisedRequestCusstomMessage { get; set; }


        public override void OnAuthorization(HttpActionContext actionContext)
        {
            base.OnAuthorization(actionContext);
        }

        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            string[] rolesList =  Regex.Replace(Roles, @"\s", "").Split(',');

            var princ = Thread.CurrentPrincipal;
           
            string currentUserName = princ.Identity.Name;

            string currentUserPassword = "CANT BE RETRIVED";

            List<Claim> claimList = null;
            try
            {
                if(princ.Identity.GetType().GetField("m_instanceClaims", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance) != null)
                    claimList = ((List<Claim>)(princ.Identity.GetType().GetField("m_instanceClaims", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(princ.Identity)));
            }
            catch (NullReferenceException)
            {

            }
            if(claimList != null)
                currentUserPassword = claimList.Where(xx => xx.Type == "Password").FirstOrDefault().Value;

            string currentUserRole = "UNAUTHORIZED. FORBIDDEN. NO ROLE";

            //Needed to make acssess to "Role" as similar to acssess to "Pasword" (as in the row 31)
            if (princ.Identity.GetType().GetField("m_instanceClaims", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance) != null)
                currentUserRole = ((List<Claim>)(princ.Identity.GetType().GetField("m_instanceClaims", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(princ.Identity)))[1].Value;

            string messageToUserIfUnathorised = $"\nSorry, dear {currentUserName}, but you're unauthorised.\n Your password is: {currentUserPassword}";

            if (!rolesList.Contains(currentUserRole) && claimList != null)
            {
                messageToUserIfUnathorised = $"\nSorry, your authorisation failed. You're registered as {currentUserRole}, \n only ";

                string x = ", ";
                for (int i = 0; i < rolesList.Length; i++)
                {

                    if (i == rolesList.Length - 2)
                        x = " and ";
                    else if (i == rolesList.Length - 1)
                        x = " ";

                    messageToUserIfUnathorised += Pluralize(rolesList[i]) + x;

                }
                messageToUserIfUnathorised += "have acsess to this data. We're very sorry.";
            }
            else
            {
                messageToUserIfUnathorised = "Sorry, but you tried to enter the systm without any autorisation, or your autorisation token (JWT or GoogleToken) missing or corrupted. You can't be authorized now :(";
            }

            actionContext.Response = new System.Net.Http.HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.Forbidden,
                Content = new StringContent(messageToUserIfUnathorised)
            };
        }

        private string Pluralize(string str)
        {
            string ret = "";
            if (str.Last() == 'y')
                ret = "\""+ChopCharsfromTheEnd(str, 1) + "ies\"";
            else ret = "\"" + str + "s\"";

            return ret;
        }
        private string ChopCharsfromTheEnd(string str, int charsNum)
        {
            string strOut = string.Empty;
            for (int i = 0; i < str.Length; i++)
            {
                if (i < str.Length - charsNum) strOut += str[i];
            }
            return strOut;
        }

    }
}