using DAL.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PL.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Utilities;

namespace PL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly SignInManager<AppUser> _signIn;

        public AccountController(RoleManager<IdentityRole> roleManager,
            UserManager<AppUser> userManager,
            IConfiguration configuration,
            SignInManager<AppUser> SignIn)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _configuration = configuration;
            _signIn = SignIn;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterDTO UserFromRequest)
        {
            AppUser appUser = new AppUser()
            {
                Email = UserFromRequest.Email,
                UserName = UserFromRequest.UserName,
            };

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;


            string role;
            if (User.IsInRole(SD.ManagerRole))
            {
                role = SD.ObserverRole;
                appUser.ManagerId = userId;
            }
            else
                role = SD.ManagerRole;

            IdentityResult result = await _userManager.CreateAsync(appUser, UserFromRequest.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return BadRequest(ModelState);
            }

            bool roleExists = await _roleManager.RoleExistsAsync(role);
            if (!roleExists)
            {
                return BadRequest($"Role '{role}' does not exist.");
            }

            var roleResult = await _userManager.AddToRoleAsync(appUser, role);
            if (!roleResult.Succeeded)
            {
                return BadRequest("Failed to assign role to user.");
            }

            await _signIn.SignInAsync(appUser, isPersistent: false);
            return Ok(new { message = "User Created Successfully", User = appUser.UserName });
        }




        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginDTO UserFromRequest)
        {
            AppUser? UserFromDB = await _userManager.FindByNameAsync(UserFromRequest.UserName);

            if (UserFromDB == null || !await _userManager.CheckPasswordAsync(UserFromDB, UserFromRequest.Password))
            {
                return Unauthorized(new { message = "Invalid Email or Password" });
            }

            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, UserFromDB.Id),
                new Claim(ClaimTypes.Name, UserFromDB.UserName),
            };

            var UserRoles = await _userManager.GetRolesAsync(UserFromDB);
            userClaims.AddRange(UserRoles.Select(role => new Claim(ClaimTypes.Role, role)));

            userClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));

            // key 
            var SignInKey = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes(_configuration["JWT:SecretKey"]));
            SigningCredentials signingCred = new SigningCredentials(SignInKey, SecurityAlgorithms.HmacSha256);

            // design token 
            JwtSecurityToken myToken = new JwtSecurityToken(
                issuer: _configuration["JWT:IssuerIP"],
                audience: _configuration["JWT:AudienceIP"],
                expires: DateTime.UtcNow.AddMonths(12),
                claims: userClaims,
                signingCredentials: signingCred
                );

            // generate Token response
            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(myToken),
                expiration = myToken.ValidTo
            });
        }
    }
}
