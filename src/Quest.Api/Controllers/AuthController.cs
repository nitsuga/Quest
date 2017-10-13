using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Quest.Lib.ServiceBus;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Options;
using Quest.Api.Options;

namespace Quest.Api.Controllers
{
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private AsyncMessageCache _messageCache;
        private readonly JsonSerializerSettings _serializerSettings;
        private JwtIssuerOptions _jwtOptions;

        public AuthController( IOptions<JwtIssuerOptions> jwtOptions, AsyncMessageCache messageCache)
        {
            _jwtOptions = jwtOptions.Value;
            _messageCache = messageCache;
            _serializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };

        }

        /// <returns>Date converted to seconds since Unix epoch (Jan 1, 1970, midnight UTC).</returns>
        private static long ToUnixEpochDate(DateTime date)
          => (long)Math.Round((date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);


        [HttpPost]
        [AllowAnonymous]
        public AuthResult Access([FromQuery] string username, [FromQuery]string password)
        {

            // should call authenication service here
            //var response = request.Submit<LoginResponse>(_messageCache);

            // go look up the username / password in a different way!!
            if (!(username == "fred" && password == "fred"))
            {
                return new AuthResult { Token = "", Message="Failed", Success=false };
            }


            // get here if authorised

            // add some standard claims
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),

                // the session token is used as the unique id for this jwt token
                // this claim is used by all calls so the backend knows we are legit.
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(DateTime.UtcNow).ToString(), ClaimValueTypes.Integer64)
            };

            var identity = new ClaimsIdentity(new GenericIdentity(username, "Token"), claims);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Create the JWT security token and encode it.
            var jwt = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: identity.Claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow+new TimeSpan(2,0,0,0),
                signingCredentials: creds);

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            return new AuthResult { Token = encodedJwt, Message="OK", Success=true };
        }

    }

    public class AuthResult
    {
        public string Token;
        public string Message;
        public bool Success;
    }
}
