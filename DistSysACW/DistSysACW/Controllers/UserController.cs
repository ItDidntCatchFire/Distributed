using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace DistSysACW.Controllers
{
    public class UserController : BaseController
    {
        /// <summary>
        /// Constructs a User controller, taking the UserContext through dependency injection
        /// </summary>
        /// <param name="context">DbContext set as a service in Startup.cs and dependency injected</param>
        public UserController(Data.IUserRepository userRepository) : base(userRepository) { }

        [HttpGet]
        [ActionName("new")]
        public async Task<IActionResult> GetUserByUserNameAsync(string userName)
        {
            if (String.IsNullOrEmpty(userName))
                return Ok("\"False - User Does Not Exist! Did you mean to do a POST to create a new user?\"");

            if (await _UserRepository.UserExistsByUserNameAsync(userName))
                return Ok("\"True - User Does Exist! Did you mean to do a POST to create a new user?\"");
            else
                return Ok("\"False - User Does Not Exist! Did you mean to do a POST to create a new user?\"");
        }

        [HttpPost]
        [ActionName("new")]
        public async Task<IActionResult> AddUserByUserName([FromBody] string userName)
        {
            var user = _UserRepository.UserExistsByUserNameAsync(userName);

            if (String.IsNullOrEmpty(userName))
                return BadRequest("\"Oops. Make sure your body contains a string with your username and your Content-Type is Content-Type:application/json\"");

            if (user.Result)
                //return Forbid("Oops. This username is already in use. Please try again with a new username.");
                return StatusCode(403, "\"Oops. This username is already in use. Please try again with a new username.\"");

            var newUser = await _UserRepository.NewUserAsync(userName);
            await _UserRepository.SaveAsync();
            if (await _UserRepository.CountAsync() == 1)
            {
                newUser.Role = Models.User.Roles.Admin;
                await _UserRepository.UpdateAsync(newUser);

                _ = _UserRepository.SaveAsync();
            }

            return Ok(newUser.ApiKey);
        }

        [HttpDelete]
        [ActionName("RemoveUser")]
        [Authorize(Roles = "Admin, User")]
        public async Task<IActionResult> RemoveUser([FromQuery] string userName)
        {
            if (HttpContext.Request.Headers.TryGetValue("ApiKey", out var apiKey))
            {
                if (await _UserRepository.UserExistsByApiKeyUserNameAsync(apiKey, userName))
                {
                    await _UserRepository.DeleteAsync(apiKey);

                    _ = _UserRepository.SaveAsync();
                    return Ok(true);
                }
            }
            return Ok(false);
        }
    }
}