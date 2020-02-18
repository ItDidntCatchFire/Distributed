using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DistSysACW.Controllers
{
    public class UserController : BaseController
    {
        /// <summary>
        /// Constructs a User controller, taking the UserContext through dependency injection
        /// </summary>
        /// <param name="context">DbContext set as a service in Startup.cs and dependency injected</param>
        public UserController(Models.UserContext context) : base(context) { }

        [HttpGet]
        [ActionName("new")]
        public async Task<IActionResult> GetUserByUserNameAsync(string userName)
        {
            if (String.IsNullOrEmpty(userName))
                return Ok("False - User Does Not Exist! Did you mean to do a POST to create a new user?");
            throw new NotImplementedException();
            if (await Models.UserDatabaseAccess.UserExistsByUserNameAsync(userName, _context))
                return Ok("True - User Does Exist! Did you mean to do a POST to create a new user?");
            else
                return Ok("False - User Does Not Exist! Did you mean to do a POST to create a new user?");
        }

        [HttpPost]
        [ActionName("new")]
        public async Task<IActionResult> UpdateUserNameAsync([FromBody]string userName)
        {
            //await Models.UserDatabaseAccess.NewUserAsync(userName, _context);
            throw new NotImplementedException();
        }

    }
}