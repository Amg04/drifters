using BLLProject.Interfaces;
using BLLProject.Specifications;
using DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PL.DTOs;
using System.Security.Claims;

namespace PL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;

        public UserController(UserManager<AppUser> userManager, IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _userManager = userManager;
           _unitOfWork = unitOfWork;
            this._hostEnvironment = hostEnvironment;
        }

        #region GetProfile

        [Authorize]
        [HttpGet("Profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized("User ID not found in token");
            var user = await _userManager.FindByIdAsync(userId);
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault();

            var spec = new BaseSpecification<MonitoredEntity>(m => m.UserId == user.Id);
            spec.AddOrderByDescending(m => m.LastUpdate);
            var latestMonitored = _unitOfWork.Repository<MonitoredEntity>().GetEntityWithSpec(spec);

            var UpdateProfileDto = new UpdateProfileDto()
            {
                ImageUrl = user.ImgUrl,
                Role = role,
                Name = user.Name,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Company = latestMonitored?.EntityName,
                Location = latestMonitored?.Location
            };

            return Ok(UpdateProfileDto);
        }

        #endregion

        #region EditProfile

        [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> EditProfile([FromForm] UpdateProfileDto dto, IFormFile? imageFile)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized("User ID not found in token");
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            if (imageFile != null)
            {
                if (!string.IsNullOrEmpty(user.ImgUrl))
                    ImageHelper.DeleteImage(user.ImgUrl, _hostEnvironment);

                user.ImgUrl = ImageHelper.SaveImage(imageFile, _hostEnvironment);
            }

            var spec = new BaseSpecification<MonitoredEntity>(m => m.UserId == user.Id);
            spec.AddOrderByDescending(m => m.LastUpdate);
            var latestMonitored = _unitOfWork.Repository<MonitoredEntity>().GetEntityWithSpec(spec);

            user.Name = dto.Name;
            if (!string.IsNullOrEmpty(dto.Email))
                user.Email = dto.Email;
            user.PhoneNumber = dto.PhoneNumber;
            latestMonitored.EntityName = dto.Company;
            latestMonitored.Location = dto.Location;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            _unitOfWork.Complete();

            return Ok(new { Message = "User updated successfully" });
        }

        #endregion

        #region ChangePassword

        [Authorize]
        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized("User not found in token");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound("User not found");

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (result.Succeeded)
                return Ok(new { message = "Password changed successfully" });

            return BadRequest(result.Errors.Select(e => e.Description));
        }

        #endregion

    }
}