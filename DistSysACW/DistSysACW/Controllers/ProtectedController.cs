using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DistSysACW.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DistSysACW.Controllers
{
    public class ProtectedController : BaseController
    {
        public ProtectedController() : base()
        {
        }

        [HttpGet]
        [ActionName("sha1")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> Sha1([FromQuery] string message)
        {
            if (string.IsNullOrEmpty(message))
                return BadRequest("Bad Request");
            
            byte[] data = Encoding.ASCII.GetBytes(message);
            SHA1 sha = new SHA1CryptoServiceProvider();
            var result = sha.ComputeHash(data);
            
            return Ok(BitConverter.ToString(result).Replace("-", ""));
        }
    }
}