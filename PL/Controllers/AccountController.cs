using BLLProject.Interfaces;
using DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PL.DTOs;
using PL.Email;
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
        private readonly IEmailSender _emailSender;

        public AccountController(RoleManager<IdentityRole> roleManager,
            UserManager<AppUser> userManager,
            IConfiguration configuration,
            SignInManager<AppUser> SignIn,
            IEmailSender emailSender
            )
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _configuration = configuration;
            _signIn = SignIn;
            _emailSender = emailSender;
        }

  
        #region Register

        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterDTO UserFromRequest)
        {
            if (await _userManager.FindByEmailAsync(UserFromRequest.Email) != null)
            {
                ModelState.AddModelError(nameof(UserFromRequest.Email), "This email is already in use.");
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            AppUser appUser = new AppUser()
            {
                Email = UserFromRequest.Email,
                UserName = UserFromRequest.UserName,
            };

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            string role;
            if (User.IsInRole(SD.ManagerRole))
            {
                role = SD.ObserverRole;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();
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

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(appUser);
            
            var confirmationLink = Url.Action(nameof(ConfirmEmail), "Account",
                new { userId = appUser.Id, token = token }, Request.Scheme);

            var emailBody = $@"
            <div style='font-family: Arial, sans-serif; font-size: 16px; color: #333;'>
                <h2>Welcome to Our Service!</h2>
                <p>Thank you for registering. Please confirm your email address by clicking the button below:</p>
                <a href='{confirmationLink}'
                   style='display: inline-block; padding: 12px 24px; margin: 20px 0;
                          font-size: 16px; color: white; background-color: #007bff;
                          text-decoration: none; border-radius: 5px;'>
                    Confirm Email
                </a>
                <p>If you did not create this account, please ignore this email.</p>
                <hr style='border:none; border-top:1px solid #eee;'/>
                <p style='font-size: 12px; color: #999;'>© {DateTime.UtcNow.Year} Your Company. All rights reserved.</p>
            </div>";

            if (!string.IsNullOrEmpty(appUser.Email))
                await _emailSender.SendEmailAsync(appUser.Email, "Confirm your email", emailBody);

            return Ok(new { message = "User registered successfully. Please check your email to confirm the account.", User = appUser.UserName });
        }

        #endregion

        #region ConfirmEmail

        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
                return BadRequest("Invalid Email confirmation request.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound("User not found.");

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
                return Ok("Email confirmed successfully. You can now log in.");
            else
                return BadRequest("Email confirmation failed.");
        }

        #endregion

        #region Login

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginDTO UserFromRequest)
        {
            AppUser? UserFromDB = await _userManager.FindByNameAsync(UserFromRequest.UserName);
            if (UserFromDB == null || !await _userManager.CheckPasswordAsync(UserFromDB, UserFromRequest.Password))
            {
                return Unauthorized(new { message = "Invalid UserName or Password" });
            }

            if (!await _userManager.IsEmailConfirmedAsync(UserFromDB))
            {
                return Unauthorized(new { message = "Email not confirmed yet. Please confirm your email." });
            }

            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, UserFromDB.Id),
                new Claim(ClaimTypes.Name, UserFromDB.UserName),
            };

            var UserRoles = await _userManager.GetRolesAsync(UserFromDB);
            userClaims.AddRange(UserRoles.Select(role => new Claim(ClaimTypes.Role, role)));
            userClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));

            var SignInKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"]));
            var signingCred = new SigningCredentials(SignInKey, SecurityAlgorithms.HmacSha256);

            var myToken = new JwtSecurityToken(
                issuer: _configuration["JWT:IssuerIP"],
                audience: _configuration["JWT:AudienceIP"],
                expires: DateTime.UtcNow.AddMonths(12),
                claims: userClaims,
                signingCredentials: signingCred);

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(myToken),
                expiration = myToken.ValidTo
            });
        }

        #endregion

        #region Logout

        [HttpPost("Logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signIn.SignOutAsync();
            return Ok(new { message = "User logged out successfully." });
        }


        #endregion

        #region ResetPassword

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound(new { Message = "User not found." });

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = Uri.EscapeDataString(token);

            var resetLink = Url.Action(nameof(ShowResetPasswordPage), "Account",
                new { email = user.Email, token = encodedToken }, Request.Scheme);
            
                var emailBody = $@"
                    <div style='font-family: Arial, sans-serif; font-size: 16px; color: #333;'>
                        <p>To reset your password, please click the button below:</p>
                        <a href='{resetLink}' 
                           style='display: inline-block; padding: 12px 24px; margin: 20px 0; 
                           font-size: 16px; color: white; background-color: #007bff; 
                           text-decoration: none; border-radius: 5px;'>
                           Reset Password
                        </a>
                        <p>If you did not request a password reset, please ignore this email.</p>
                    </div>";

            if (user.Email != null)
                await _emailSender.SendEmailAsync(user.Email, "Reset Password", emailBody);

            return Ok(new { Message = "Password reset email sent. Please check your email." });
        }


        #endregion

        #region ShowResetPasswordPage

        [HttpGet("ShowResetPasswordPage")]
        public IActionResult ShowResetPasswordPage(string email, string token)
        {
            string html = $@"
                <html>
                <head>
                    <title>Reset Password</title>
                    <style>
                        body {{
                            font-family: Arial, sans-serif;
                            background-color: #f4f7fa;
                            display: flex;
                            justify-content: center;
                            align-items: center;
                            height: 100vh;
                            margin: 0;
                        }}
                        .container {{
                            background-color: white;
                            padding: 30px 40px;
                            border-radius: 8px;
                            box-shadow: 0 4px 12px rgba(0,0,0,0.1);
                            max-width: 400px;
                            width: 100%;
                            box-sizing: border-box;
                        }}
                        h2 {{
                            margin-bottom: 20px;
                            color: #333;
                            text-align: center;
                        }}
                        label {{
                            display: block;
                            margin-bottom: 6px;
                            color: #555;
                            font-weight: 600;
                        }}
                        input[type=password] {{
                            width: 100%;
                            padding: 10px;
                            margin-bottom: 20px;
                            border: 1px solid #ccc;
                            border-radius: 4px;
                            box-sizing: border-box;
                            font-size: 14px;
                            transition: border-color 0.3s ease;
                        }}
                        input[type=password]:focus {{
                            border-color: #007bff;
                            outline: none;
                        }}
                        button {{
                            background-color: #007bff;
                            color: white;
                            padding: 12px;
                            border: none;
                            border-radius: 5px;
                            width: 100%;
                            font-size: 16px;
                            cursor: pointer;
                            transition: background-color 0.3s ease;
                        }}
                        button:hover {{
                            background-color: #0056b3;
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <h2>Reset Your Password</h2>
                        <form method='post' action='/api/Account/ResetPasswordConfirm'>
                            <input type='hidden' name='Email' value='{email}' />
                            <input type='hidden' name='Token' value='{token}' />
                            <label>New Password:</label>
                            <input type='password' name='Password' required minlength='6' />
                            <label>Confirm Password:</label>
                            <input type='password' name='ConfirmPassword' required minlength='6' />
                            <button type='submit'>Reset Password</button>
                        </form>
                    </div>
                </body>
                </html>
                ";

            return Content(html, "text/html");
        }

        #endregion

        #region ResetPasswordConfirm

        [HttpPost("ResetPasswordConfirm")]
        public async Task<IActionResult> ResetPasswordConfirm([FromForm] ResetPasswordDTO model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return NotFound(new { Message = "User not found." });

            var decodedToken = Uri.UnescapeDataString(model.Token);

            var resetPassResult = await _userManager.ResetPasswordAsync(user, decodedToken, model.Password);
            if (!resetPassResult.Succeeded)
            {
                foreach (var error in resetPassResult.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
                return BadRequest(ModelState);
            }

            string html = @"
                <html>
                <head>
                    <title>Password Reset Successful</title>
                    <style>
                        body {
                            font-family: Arial, sans-serif;
                            background-color: #e6f4ea;
                            display: flex;
                            justify-content: center;
                            align-items: center;
                            height: 100vh;
                            margin: 0;
                        }
                        .message-box {
                            background-color: #d4edda;
                            border: 1px solid #c3e6cb;
                            color: #155724;
                            padding: 30px 40px;
                            border-radius: 8px;
                            box-shadow: 0 4px 12px rgba(0,0,0,0.1);
                            max-width: 400px;
                            width: 100%;
                            text-align: center;
                            box-sizing: border-box;
                        }
                        h2 {
                            margin-bottom: 20px;
                        }
                        p {
                            font-size: 16px;
                            line-height: 1.4;
                            margin: 0;
                        }
                    </style>
                </head>
                <body>
                    <div class='message-box'>
                        <h2>Password Has Been Reset Successfully!</h2>
                        <p>Please log in on your mobile app with your new password.</p>
                    </div>
                </body>
                </html>";

            return Content(html, "text/html");
        }


        #endregion
    }
}
