using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using DistSysACW.Models;
using Microsoft.EntityFrameworkCore;

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
                    "\"Oops. Make sure your body contains a string with your username and your Content-Type is Content-Type:application/json\"");

            if (user.Result)
                //return Forbid("Oops. This username is already in use. Please try again with a new username.");
                return StatusCode(403,
                    "\"Oops. This username is already in use. Please try again with a new username.\"");

            var newUser = await _userRepository.NewUserAsync(userName);
            await _userRepository.SaveAsync();
            if (await _userRepository.CountAsync() == 1)
            {
                newUser.Role = Models.User.Roles.Admin;
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
            if (HttpContext.Request.Headers.TryGetValue("ApiKey", out var apiKey))
            {
                if (await _userRepository.UserExistsByApiKeyUserNameAsync(apiKey, userName))
                {
                    await _userRepository.DeleteAsync(apiKey);

                    _ = _userRepository.SaveAsync();
                    return Ok(true);
                }
            }

            return Ok(false);
        }

        [HttpPost]
        [ActionName("ChangeRole")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangeRole([FromBody] Models.User user)
        {
            try
            {
                var tUser = _userRepository.GetByUsernameAsync(user.UserName);

                if (!await _userRepository.UserExistsByUserNameAsync(user.UserName))
                    return BadRequest("NOT DONE: Username does not exist");

                //validate role
                
                _ = _userRepository.UpdateAsync(
                    new User()
                    {
                        ApiKey = tUser.Result.ApiKey,
                        UserName = user.UserName,
                        Role = user.Role
                    }
                );
                return Ok("DONE");
            }
            catch (Exception)
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
                var user = await _userRepository.GetByIdAsync((apiKey));
                return Ok("Hello " + user.UserName);
            }
            else
            {
                return BadRequest();
            }
        }

    }
}