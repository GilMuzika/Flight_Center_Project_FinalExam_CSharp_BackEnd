using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Web;

namespace Web_Api_interface.ApiControllers.JWTAutenticationAuxiliaries
{
    public class TokenValidationHandlerAssistant
    {
        private string secretKey;
        private SymmetricSecurityKey securityKey;
        JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();

        public TokenValidationHandlerAssistant()
        {
            secretKey = Convert.ToBase64String(Encoding.Default.GetBytes("bl;uitc78oull[:AE5r3=]0klhlkJKHHL:hjh5bh644f"));
            securityKey = new SymmetricSecurityKey(Convert.FromBase64String(secretKey));
        }

        public ClaimsPrincipal AssistTokenValidation(string jwtToken)
        {

            //Replace the issuer and audience with your URL (ex. http:localhost:12345)
            var validationParameters = new TokenValidationParameters
            {
                ValidAudience = "https://localhost:44361/",
                ValidIssuer = "https://localhost:44361/",
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                LifetimeValidator = LifetimeValidator,
                IssuerSigningKey = securityKey
            };

            return handler.ValidateToken(jwtToken, validationParameters, out _);

        }

        public bool LifetimeValidator(DateTime? notBefore, DateTime? expires, SecurityToken securityToken, TokenValidationParameters validationParameters)
        {
            if (expires != null)
            {
                if (DateTime.UtcNow < expires) return true;
            }
            return false;
        }

    }
}