using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RedMango_API.Data;
using RedMango_API.Models;
using RedMango_API.Models.DTO;
using RedMango_API.Services;
using RedMango_API.Utilities;
using System.Net;

namespace RedMango_API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DatabaseContext _db;
        private API_Response _response;
        private string secretKey;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityUser> _roleManager;

        public AuthController(DatabaseContext db, IConfiguration configuration,
            UserManager<ApplicationUser> userManager, RoleManager<IdentityUser> roleManager)
        {
            _db = db;
            _response = new API_Response();
            secretKey = configuration.GetValue<string>("ApiSettings:Secret");
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO model)
        {
            ApplicationUser userFromDb = _db.ApplicationUsers.FirstOrDefault(
                u => u.UserName.ToLower() == model.Username.ToLower());

            if (userFromDb == null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Username already exists");

                return BadRequest(_response);
            }

            ApplicationUser newUser = new ApplicationUser()
            {
                UserName = model.Username,
                Email = model.Username,
                NormalizedEmail = model.Username.ToUpper(),
                Name = model.Name
            };

            try
            {
                var result = await _userManager.CreateAsync(newUser, model.Password);

                if (result.Succeeded)
                {
                    if (!_roleManager.RoleExistsAsync(StaticDetail.AdminRole).GetAwaiter().GetResult())
                    {
                        await _roleManager.CreateAsync(new IdentityRole(StaticDetail.AdminRole));
                        await _roleManager.CreateAsync(new IdentityRole(StaticDetail.CustomerRole));
                    }

                    if (model.Role.ToLower() == StaticDetail.AdminRole)
                    {
                        await _userManager.AddToRoleAsync(newUser, StaticDetail.AdminRole);
                    }
                    else
                    {
                        await _userManager.AddToRoleAsync(newUser, StaticDetail.CustomerRole);
                    }

                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    return Ok(_response);
                }
            }
            catch (Exception)
            {
                
            }

            _response.StatusCode = HttpStatusCode.BadRequest;
            _response.IsSuccess = false;
            _response.ErrorMessages.Add("Error While Registering");
            return BadRequest(_response);
        }
    }
}
