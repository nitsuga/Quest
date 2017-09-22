﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Quest.Lib.ServiceBus;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using Quest.Api.Model;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Options;
using Quest.Api.Options;
using Quest.Common.Messages;

namespace Quest.Api.Controllers
{
    [Route("api/[controller]")]
    public class LogonController : Controller
    {
        private AsyncMessageCache _messageCache;
        private readonly JsonSerializerSettings _serializerSettings;
        private JwtIssuerOptions _jwtOptions;

        public LogonController( IOptions<JwtIssuerOptions> jwtOptions, AsyncMessageCache messageCache)
        {
            _jwtOptions = jwtOptions.Value;
            _messageCache = messageCache;
            _serializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };

        }

        /// <summary>
        /// IMAGINE BIG RED WARNING SIGNS HERE!
        /// You'd want to retrieve claims through your claims provider
        /// in whatever way suits you, the below is purely for demo purposes!
        /// </summary>
        private Task<ClaimsIdentity> GetClaimsIdentity(LoginRequest user)
        {
            if (user.Username == "QuestDevice" && user.Password == "f593d801-bb3b-47ae-b288-2498463a7c14")
            {

                // todo
                // var result = request.Submit<LoginResponse>(_messageCache);

                // get session token from quest and pout this as a claim so that subsequent

                // or send token to quest and then use that

                //var claims = _security.GetAppClaims(user.UserName).Select(x => new Claim(x.ClaimType, x.ClaimValue)).ToList();

                var claims = new List<Claim>();

                claims.Add(new Claim(JwtRegisteredClaimNames.Sub, user.Username));
                claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
                claims.Add(new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(DateTime.UtcNow).ToString(), ClaimValueTypes.Integer64));

                return Task.FromResult(new ClaimsIdentity(new GenericIdentity(user.Username, "Token"), claims));
            }

            // Credentials are invalid, or account doesn't exist
            return Task.FromResult<ClaimsIdentity>(null);
        }

        /// <returns>Date converted to seconds since Unix epoch (Jan 1, 1970, midnight UTC).</returns>
        private static long ToUnixEpochDate(DateTime date)
          => (long)Math.Round((date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);


        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Logon([FromForm] LoginRequest applicationUser)
        {
            var identity = await GetClaimsIdentity(applicationUser);
            if (identity == null)
            {
//                _logger.LogInformation($"Invalid username ({applicationUser.UserName}) or password ({applicationUser.Password})");
                return Unauthorized();
            }

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

            // Serialize and return the response
            var response = new LoginResponse
            {
                AccessToken = encodedJwt,
                ValidTo = jwt.ValidTo,
                Callsign ="A102",
                Message ="Logged on",
                QuestApi ="1.0",
                RequiresCallsign =true,
                Status = new Quest.Common.Messages.StatusCode { Code = "AOR", Description = "Available on Radio" },
                Success=true,
                Timestamp = ToUnixEpochDate(DateTime.UtcNow)
            };

            var json = JsonConvert.SerializeObject(response, _serializerSettings);
            return new OkObjectResult(json);
        }

    }
}