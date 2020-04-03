using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DistSysACW.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Crypto.Tls;

namespace DistSysACW.Controllers
{
    public class UserController : BaseController
    {
        private readonly Data.IUserRepository _userRepository;

        /// <summary>
        /// Constructs a User controller, taking the UserContext through dependency injection
        /// </summary>
        /// <param name="userRepository">IUserRepository set as a service in Startup.cs and dependency injected</param>
        public UserController(Data.IUserRepository userRepository) : base()
        {
            _userRepository = userRepository;
        }
        
        [HttpGet]
        [ActionName("new")]
        public async Task<IActionResult> GetUserByUserNameAsync([FromQuery] string userName)
        {
            if (String.IsNullOrEmpty(userName))
                return Ok("\"False - User Does Not Exist! Did you mean to do a POST to create a new user?\"");

            if (await _userRepository.UserExistsByUserNameAsync(userName))
                return Ok("\"True - User Does Exist! Did you mean to do a POST to create a new user?\"");
            else
                return Ok("\"False - User Does Not Exist! Did you mean to do a POST to create a new user?\"");
        }

        [HttpPost]
        [ActionName("new")]
        public async Task<IActionResult> AddUserByUserName([FromBody] string userName)
        {
            var user = _userRepository.UserExistsByUserNameAsync(userName);

            if (String.IsNullOrEmpty(userName))
                return BadRequest(
                    "Oops. Make sure your body contains a string with your username and your Content-Type is Content-Type:application/json");

            if (user.Result)
                //return Forbid("Oops. This username is already in use. Please try again with a new username.");
                return StatusCode(403,
                    "Oops. This username is already in use. Please try again with a new username.");

            var newUser = await _userRepository.NewUserAsync(userName);
            await _userRepository.SaveAsync();
            if (await _userRepository.CountAsync() == 1)
            {
                newUser.eRole = Models.User.Roles.Admin;
                await _userRepository.UpdateAsync(newUser);

                _ = _userRepository.SaveAsync();
            }

            return Ok(newUser.ApiKey);
        }

        [HttpDelete]
        [ActionName("RemoveUser")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> RemoveUser([FromQuery] string userName)
        {
            var tUser =  _userRepository.GetByUsernameAsync(userName); 
            if (HttpContext.Request.Headers.TryGetValue("ApiKey", out var apiKey))
            {
                try
                {
                    var user = await tUser;
                    if (user != null && user.ApiKey == apiKey)
                    {
                        await _userRepository.DeleteAsync(user);

                        await _userRepository.SaveAsync();
                        return Ok(true);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }

            return Ok(false);
        }

        [HttpPost]
        [ActionName("ChangeRole")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangeRole([FromBody] JObject json)
        {
            try
            {
                if (!json.TryGetValue("username", out var username) || !json.TryGetValue("role", out var role))
                    return BadRequest("NOT DONE: An error occured");
                
                var tUser = _userRepository.GetByUsernameAsync(username.ToString());

                if (!await _userRepository.UserExistsByUserNameAsync(username.ToString()))
                    return BadRequest("NOT DONE: Username does not exist");

                
                if (!Enum.IsDefined(typeof(Models.User.Roles), role.ToString()))
                    return BadRequest("NOT DONE: Role does not exist");

                var user = await tUser;
                
                var updateUser = new User()
                {
                    ApiKey = user.ApiKey,
                    UserName = user.UserName,
                    eRole = (Models.User.Roles)Enum.Parse(typeof(Models.User.Roles), role.ToString())
                };

                await _userRepository.UpdateAsync(updateUser);
                _ = _userRepository.SaveAsync();
                
                return Ok("DONE");
            }
            catch (Exception ex)
            {
                return BadRequest("NOT DONE: An error occured");
            }
        }

        [RouteAttribute("/api/Protected/[Action]")]
        [HttpGet]
        [ActionName("hello")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> Hello()
        {
            if (HttpContext.Request.Headers.TryGetValue("ApiKey", out var apiKey))
            {
                var user = await _userRepository.GetByIdAsync(apiKey);
                return Ok("Hello " + user.UserName);
            }
            else
            {
                //This should never be hit as we will have an ApiKey that works
                return BadRequest();
            }
        }
    }
}