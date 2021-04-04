using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace Web_Api_interface.Models
{
    public class LoginResponseVM
    {
        public LoginResponseVM()
        {

            this.Token = "";
            this.ResponseMsg = new HttpResponseMessage() { StatusCode = System.Net.HttpStatusCode.Unauthorized };
            this.Information = null;
        }

        public LoginResponseVM(HttpResponseMessage responseMsg)
        {
            this.Token = "";
            this.ResponseMsg = responseMsg;
            this.Information = null;
        }

        public LoginResponseVM(HttpResponseMessage responseMsg, string token, TokenOwnerInformation information)
        {
            this.Token = token;
            this.ResponseMsg = responseMsg;
            this.Information = information;

        }

        public string Token { get; set; }
        public HttpResponseMessage ResponseMsg { get; set; }
        public TokenOwnerInformation Information { get; set; }
    }

    public class TokenOwnerInformation
    {
        public string USER_NAME { get; set; }
        public string PASSWORD { get; set; }
        public string USER_KIND { get; set; }

        public string Image { get; set; }
        public string AdministratorName { get; set; }
        public string AirlineCompanyName { get; set; }
        public string CustomerFirstName { get; set; }
        public string CustomerLastName { get; set; }

    }
}