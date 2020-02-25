using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace DistSysACW.Controllers
{
    [Route("api/[Controller]/[Action]")]
    [ApiController]
    public class TalkBackController : ControllerBase
    {
        /// <summary>
        /// Constructs a TalkBack controller, taking the UserContext through dependency injection
        /// </summary>
        /// <param name="context">DbContext set as a service in Startup.cs and dependency injected</param>
        public TalkBackController()
        {

        }


        [ActionName("Hello")]
        public IActionResult Get()
        {
            #region TASK1
            return Ok("Hello World");
            #endregion
        }

        [ActionName("Sort")]
        public IActionResult Get([FromQuery]string[] integers)
        {
            #region TASK1
            try
            {
                var numbers = new List<int>(Array.ConvertAll(integers, s => int.Parse(s)));

                numbers.Sort();

                return Ok(numbers);
            }
            catch (Exception ex)
            {
                return StatusCode(400, "Bad Request");
            }
            #endregion
        }
    }
}
