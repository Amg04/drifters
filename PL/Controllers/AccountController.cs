using BLLProject.Interfaces;
using BLLProject.Specifications;
using DAL.Models;
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
        private readonly IUnitOfWork _unitOfWork;

        public AccountController(RoleManager<IdentityRole> roleManager,
            UserManager<AppUser> userManager,
            IConfiguration configuration,
            SignInManager<AppUser> SignIn,
            IEmailSender emailSender,
            IUnitOfWork unitOfWork)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _configuration = configuration;
            _signIn = SignIn;
            _emailSender = emailSender;
            _unitOfWork = unitOfWork;
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

            await _signIn.SignInAsync(appUser, isPersistent: false);
            return Ok(new { message = "User Created Successfully", User = appUser.UserName });
        }

        #endregion

        #region Login

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginDTO UserFromRequest)
        {
            AppUser? UserFromDB = await _userManager.FindByNameAsync(UserFromRequest.UserName);
            if (UserFromDB == null || !await _userManager.CheckPasswordAsync(UserFromDB, UserFromRequest.Password))
            {
                return Unauthorized(new { message = "Invalid Email or Password" });
            }

            var otp = await GenerateAndSendOtp(UserFromDB);

            return Ok(new { message = "OTP sent to your email, please verify." });
        }

        #endregion

        #region VerifyOtp


        [HttpPost("VerifyOtp")]
        public async Task<IActionResult> VerifyOtp(OtpVerificationDTO otpDto)
        {
            var user = await _userManager.FindByNameAsync(otpDto.UserName);
            if (user == null)
                return Unauthorized(new { message = "Invalid User" });

          var spec = new BaseSpecification<UserOtp>(o => o.UserId == user.Id
                             && o.OtpCode == otpDto.Otp
                             && !o.IsUsed
                             && o.ExpirationTime > DateTime.UtcNow);
            var userOtp = _unitOfWork.Repository<UserOtp>().GetEntityWithSpec(spec);

            bool isOtpValid = true;
            if (userOtp == null)
                isOtpValid = false;

            if (!isOtpValid)
                return Unauthorized(new { message = "Invalid OTP" });

            // Generate JWT token after successful OTP verification
            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
            };

            var UserRoles = await _userManager.GetRolesAsync(user);
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

        #region ResetPassword

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(string Email)
        {
            var user = await _userManager.FindByEmailAsync(Email);
            if (user == null)
                return NotFound();

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var resetLink = Url.Action(nameof(ResetPasswordConfirm), "Account",
                new { email = user.Email, token = token }, Request.Scheme);

            if (user.Email != null)
            {
                var emailBody = $@"
              <div style='font-family: Arial, sans-serif; font-size: 16px; color: #333;'>
                <p>Please reset your password by clicking the button below:</p>
                <a href='{resetLink}' 
                   style='display: inline-block; padding: 12px 24px; margin: 20px 0; 
                          font-size: 16px; color: white; background-color: #007bff; 
                          text-decoration: none; border-radius: 5px;'>
                     Reset Password
                    </a>
                    <p>If you did not request a password reset, please ignore this email.</p>
                    </div>
                ";

                await _emailSender.SendEmailAsync(user.Email, "Reset Password", emailBody);
            }

            return Ok(new { Message = "Password reset email sent. Please check your email." });
        }

        #endregion

        #region ResetPasswordConfirm

        [HttpPost("reset-password-confirm")]
        public async Task<IActionResult> ResetPasswordConfirm([FromBody] ResetPasswordDTO model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return NotFound(new { Message = "User not found." });
            }

            var resetPassResult = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (!resetPassResult.Succeeded)
            {
                foreach (var error in resetPassResult.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
                return BadRequest(ModelState);
            }

            return Ok(new { Message = "Password has been reset successfully." });
        }

        #endregion

        #region Method

        private async Task<string?> GenerateAndSendOtp(AppUser user)
        {
            var otp = new Random().Next(1000, 9999).ToString();

          var userOtp = new UserOtp
            {
                UserId = user.Id,
                OtpCode = otp,
                ExpirationTime = DateTime.UtcNow.AddMinutes(5),
                IsUsed = false
            };

            _unitOfWork.Repository<UserOtp>().Add(userOtp);
            _unitOfWork.Complete();

            if (!string.IsNullOrEmpty(user.Email))
            {
                var emailBody = $"Your OTP code is: {otp}. It expires in 5 minutes.";
                await _emailSender.SendEmailAsync(user.Email, "Your OTP Code", emailBody);
                return otp;
            }

            return null;
        }

        #endregion

    }
}
