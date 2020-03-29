using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;

namespace DistSysACW.Filters
{
    public class AuthFilter : AuthorizeAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var authAttribute = (AuthorizeAttribute)context.ActionDescriptor.EndpointMetadata.FirstOrDefault(e => e.GetType() == typeof(AuthorizeAttribute));
            try
            {
                if (authAttribute == null) return; //No auth required for method
                if (!context.HttpContext.User.Identities.Any()) return; //User has no identities
                

                //if the user is valid
                var roles = authAttribute.Roles.Split(',');
                if (roles.Any(role => context.HttpContext.User.IsInRole(role)))
                    return;

                //if it is not admin access only
                if (roles.First() != nameof(Models.User.Roles.Admin))
                    throw new UnauthorizedAccessException();
                
                //if it is admin access only
                context.HttpContext.Response.StatusCode = 401;
                context.Result = new JsonResult("Unauthorized. Admin access only.");                    
            }
            catch
            {
                context.HttpContext.Response.StatusCode = 401;
                context.Result = new JsonResult("Unauthorized. Check ApiKey in Header is correct.");
            }
        }
    }
}
