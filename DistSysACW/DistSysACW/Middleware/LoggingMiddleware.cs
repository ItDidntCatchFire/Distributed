using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace DistSysACW.Middleware
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public LoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, Data.IUserRepository userRepository)
        {
            if (context.Request.Headers.TryGetValue("ApiKey", out var apiKey))
            {
                var user = await userRepository.GetByIdAsync(apiKey);
                if (user != null)
                {
                    //Logging
                    var request = context.Request.Path;
                    var log = new Models.Log($"User Requested: {request}");
                    if (user.Logs == null)
                        user.Logs = new List<Models.Log>();
                    user.Logs.Add(log);
                    await userRepository.UpdateAsync(user);
                    _ = userRepository.SaveAsync();    
                }
            }
            await _next(context);
        }
    }
}