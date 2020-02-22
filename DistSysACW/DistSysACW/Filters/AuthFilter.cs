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
            try
            {
                AuthorizeAttribute authAttribute = (AuthorizeAttribute)context.ActionDescriptor.EndpointMetadata.Where(e => e.GetType() == typeof(AuthorizeAttribute)).FirstOrDefault();
                if (context.HttpContext.User.Identities.Count() > 0)
                    if (authAttribute != null)
                    {
                        string[] roles = authAttribute.Roles.Split(',');
                        foreach (string role in roles)
                        {
                            if (context.HttpContext.User.IsInRole(role))
                            {
                                return;
                            } else if (role == nameof(Models.User.Roles.Admin))
                            {
                                context.HttpContext.Response.StatusCode = 401;
                                context.Result = new JsonResult("Unauthorized. Admin access only.");
                            }
                        }
                        throw new UnauthorizedAccessException();
                    }
            }
            catch
            {
                context.HttpContext.Response.StatusCode = 401;
                context.Result = new JsonResult("Unauthorized. Check ApiKey in Header is correct.");
            }
        }
    }
}
