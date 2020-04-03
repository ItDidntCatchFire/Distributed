using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DistSysACW.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CoreExtensions;

namespace DistSysACW.Controllers
{
    public class ProtectedController : BaseController
    {
        private readonly RSACryptoServiceProvider _cspAsymmetric;
        
        public ProtectedController(RSACryptoServiceProvider cspAsymmetric) : base()
        {
            _cspAsymmetric = cspAsymmetric;
        }

        [HttpGet]
        [ActionName("sha1")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> Sha1([FromQuery] string message)
        {
            if (string.IsNullOrEmpty(message))
                return BadRequest("Bad Request");
            
            byte[] data = Encoding.ASCII.GetBytes(message);
            var sha = new SHA1CryptoServiceProvider();
            var result = sha.ComputeHash(data);
            
            return Ok(BitConverter.ToString(result).Replace("-", ""));
        }
        
        [HttpGet]
        [ActionName("sha256")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> Sha256([FromQuery] string message)
        {
            if (string.IsNullOrEmpty(message))
                return BadRequest("Bad Request");
            
            byte[] data = Encoding.ASCII.GetBytes(message);
            var sha = new SHA256CryptoServiceProvider();
            var result = sha.ComputeHash(data);
            
            return Ok(BitConverter.ToString(result).Replace("-", ""));
        }

        [HttpGet]
        [ActionName("GetPublicKey")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> GetPublicKey()
            => Ok(_cspAsymmetric.ToXmlStringCore22());

        [HttpGet]
        [ActionName("Sign")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> Sign([FromQuery]string message)
        {
            var byteConverter = new ASCIIEncoding();
            var originalData = byteConverter.GetBytes(message);

            var result = _cspAsymmetric.SignData(originalData, new SHA1CryptoServiceProvider());
            
            return Ok(BitConverter.ToString(result));
        }
        
        [HttpGet]
        [ActionName("AddFifty")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddFify(string encryptedInteger, string encryptedSymKey, string encryptedIV)
        {
            return Ok();
        }
       
       private static byte[] StringToByteArray(string hex)
       {
           var NumberChars = hex.Length;
           var bytes = new byte[NumberChars / 2];
           for (var i = 0; i < NumberChars; i += 2)
               bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
           return bytes;
       }
    }
}