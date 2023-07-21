using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RedMango_API.Utilities;

namespace RedMango_API.Controllers
{
    [Route("api/AuthTest")]
    [ApiController]
    public class AuthTestController : ControllerBase
    {
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<string>> GetSomething()
        {
            return "U R Authenticated";
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = StaticDetail.AdminRole)]
        public async Task<ActionResult<string>> GetSomething(int someIntValue)
        {
            return "U R Authorized with the role of Admin";
        }
    }
}
