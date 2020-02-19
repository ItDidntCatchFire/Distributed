using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DistSysACW.Controllers
{
    [Route("api/[Controller]/[Action]")]
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        protected readonly Data.IUserRepository _UserRepository; 
        public BaseController(Data.IUserRepository UserRepository)
        {
            _UserRepository = UserRepository;
        }
    }
}