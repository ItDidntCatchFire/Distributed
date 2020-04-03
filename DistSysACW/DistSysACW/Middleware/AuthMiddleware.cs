using System;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using DistSysACW.Models;

namespace DistSysACW.Middleware
{
    public class AuthMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, Data.IUserRepository userRepository)
        {
            #region Task5
            // TODO:  Find if a header ‘ApiKey’ exists, and if it does, check the database to determine if the given API Key is valid
            //        Then set the correct roles for the User, using claims

            if (context.Request.Headers.TryGetValue("ApiKey", out var apiKey))
            {
                var user = await userRepository.GetByIdAsync(apiKey);
                if (user != null)
                {
                    var claims = new List<Claim>()
                    {
                        new Claim(ClaimTypes.Name, user.UserName),
                        new Claim(ClaimTypes.Role, user.eRole.ToString())
                    };

                    var cli = new ClaimsIdentity(claims, "ApiKey");
                    cli.AddClaims(claims);

                    context.User.AddIdentity(cli);
                }
            }
            #endregion

            // Call the next delegate/middleware in the pipeline
            await _next(context);
        }

    }
}
