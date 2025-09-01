using Microsoft.AspNetCore.Http;
using BLLProject.Interfaces;
using DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PL.DTOs;
using System.Security.Claims;
using Utilities;

namespace PL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = SD.AdminRole)]
    public class UserController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<UserController> _logger;

        public UserController(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<UserController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        #region Create

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
        {
            try
            {
                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(dto.Email);
                if (existingUser != null)
                    return BadRequest("User with this email already exists.");

                var existingUserByName = await _userManager.FindByNameAsync(dto.UserName);
                if (existingUserByName != null)
                    return BadRequest("User with this username already exists.");

                var user = new AppUser
                {
                    UserName = dto.UserName,
                    Email = dto.Email,
                    EmailConfirmed = dto.EmailConfirmed,
                    PhoneNumber = dto.PhoneNumber,
                    PhoneNumberConfirmed = dto.PhoneNumberConfirmed,
                    LockoutEnabled = dto.LockoutEnabled,
                    ManagerId = dto.ManagerId
                };

                var result = await _userManager.CreateAsync(user, dto.Password);
                if (!result.Succeeded)
                    return BadRequest(result.Errors);

                // Assign role if provided
                if (!string.IsNullOrEmpty(dto.Role))
                {
                    var roleExists = await _roleManager.RoleExistsAsync(dto.Role);
                    if (!roleExists)
                        return BadRequest($"Role '{dto.Role}' does not exist.");

                    await _userManager.AddToRoleAsync(user, dto.Role);
                }

                _logger.LogInformation($"User {user.UserName} created successfully by admin {User.Identity.Name}");

                return CreatedAtAction(nameof(GetById), new { id = user.Id }, new { user.Id, user.UserName, user.Email });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return StatusCode(500, "An error occurred while creating the user.");
            }
        }

        #endregion

        #region GetAll

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var users = _userManager.Users
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var userDtos = new List<object>();

                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    userDtos.Add(new
                    {
                        user.Id,
                        user.UserName,
                        user.Email,
                        user.PhoneNumber,
                        user.EmailConfirmed,
                        user.PhoneNumberConfirmed,
                        user.LockoutEnabled,
                        user.LockoutEnd,
                        user.AccessFailedCount,
                        user.ManagerId,
                        Roles = roles
                    });
                }

                var totalUsers = _userManager.Users.Count();

                return Ok(new
                {
                    Users = userDtos,
                    TotalCount = totalUsers,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalUsers / pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users");
                return StatusCode(500, "An error occurred while retrieving users.");
            }
        }

        #endregion

        #region GetById

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                    return NotFound();

                var roles = await _userManager.GetRolesAsync(user);

                return Ok(new
                {
                    user.Id,
                    user.UserName,
                    user.Email,
                    user.PhoneNumber,
                    user.EmailConfirmed,
                    user.PhoneNumberConfirmed,
                    user.LockoutEnabled,
                    user.LockoutEnd,
                    user.AccessFailedCount,
                    user.ManagerId,
                    Roles = roles
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving user with ID {id}");
                return StatusCode(500, "An error occurred while retrieving the user.");
            }
        }

        #endregion

        #region Update

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateUserDto dto)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                    return NotFound();

                if (!string.IsNullOrEmpty(dto.Email) && dto.Email != user.Email)
                {
                    var existingUser = await _userManager.FindByEmailAsync(dto.Email);
                    if (existingUser != null)
                        return BadRequest("Email is already taken by another user.");
                    user.Email = dto.Email;
                }

                if (!string.IsNullOrEmpty(dto.UserName) && dto.UserName != user.UserName)
                {
                    var existingUser = await _userManager.FindByNameAsync(dto.UserName);
                    if (existingUser != null)
                        return BadRequest("Username is already taken by another user.");
                    user.UserName = dto.UserName;
                }

                if (!string.IsNullOrEmpty(dto.PhoneNumber))
                    user.PhoneNumber = dto.PhoneNumber;

                if (dto.EmailConfirmed.HasValue)
                    user.EmailConfirmed = dto.EmailConfirmed.Value;

                if (dto.PhoneNumberConfirmed.HasValue)
                    user.PhoneNumberConfirmed = dto.PhoneNumberConfirmed.Value;

                if (dto.LockoutEnabled.HasValue)
                    user.LockoutEnabled = dto.LockoutEnabled.Value;

                if (!string.IsNullOrEmpty(dto.ManagerId))
                    user.ManagerId = dto.ManagerId;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                    return BadRequest(result.Errors);

                if (dto.Roles != null && dto.Roles.Any())
                {
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);

                    foreach (var role in dto.Roles)
                    {
                        var roleExists = await _roleManager.RoleExistsAsync(role);
                        if (roleExists)
                            await _userManager.AddToRoleAsync(user, role);
                    }
                }

                _logger.LogInformation($"User {user.UserName} updated successfully by admin {User.Identity.Name}");

                return Ok(new { Message = "User updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating user with ID {id}");
                return StatusCode(500, "An error occurred while updating the user.");
            }
        }

        #endregion

        #region Delete

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (id == currentUserId)
                    return BadRequest("You cannot delete your own account.");

                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                    return NotFound();

                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                    return BadRequest(result.Errors);

                _logger.LogInformation($"User {user.UserName} deleted successfully by admin {User.Identity.Name}");

                return Ok(new { Message = "User deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting user with ID {id}");
                return StatusCode(500, "An error occurred while deleting the user.");
            }
        }

        #endregion

        #region Additional Admin Operations

        #region ChangePassword

        [HttpPost("{id}/change-password")]
        public async Task<IActionResult> ChangePassword(string id, [FromBody] ChangePasswordDto dto)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                    return NotFound();

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);

                if (!result.Succeeded)
                    return BadRequest(result.Errors);

                _logger.LogInformation($"Password changed for user {user.UserName} by admin {User.Identity.Name}");

                return Ok(new { Message = "Password changed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error changing password for user with ID {id}");
                return StatusCode(500, "An error occurred while changing the password.");
            }
        }

        #endregion



        #endregion
    }
}