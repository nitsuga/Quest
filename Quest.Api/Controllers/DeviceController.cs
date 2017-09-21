using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Quest.Common.Messages;

namespace Quest.Api.Controllers
{
    [Route("api/[controller]")]
    public class DeviceController : Controller
    {
   
        [HttpPost]
        [Route("Login")]
        public void Login( [FromBody] LoginRequest request)
        { }
    }
}
