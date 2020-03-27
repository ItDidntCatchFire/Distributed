using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DistSysACW.Controllers
{
    public class TalkBackController : BaseController
    {
        /// <summary>
        /// Constructs a TalkBack controller, taking the UserContext through dependency injection
        /// </summary>
        /// <param name="context">DbContext set as a service in Startup.cs and dependency injected</param>
        public TalkBackController()
        {

        }

        [HttpGet]
        [ActionName("Hello")]
        public async Task<IActionResult> Get()
        {
            #region TASK1
            return Ok("Hello World");
            #endregion
        }

        [HttpGet]
        [ActionName("Sort")]
        public async Task<IActionResult> Get([FromQuery]string[] integers)
        {
            #region TASK1
            try
            {
                var numbers = new List<int>(Array.ConvertAll(integers, s => int.Parse(s)));

                numbers.Sort();

                return Ok(numbers);
            }
            catch (Exception)
            {
                return StatusCode(400, "Bad Request");
            }
            #endregion
        }
    }
}
