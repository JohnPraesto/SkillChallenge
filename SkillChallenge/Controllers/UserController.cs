using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SkillChallenge.DTOs;
using SkillChallenge.DTOs.Account;
using SkillChallenge.DTOs.User;
using SkillChallenge.Interfaces;
using SkillChallenge.Models;
using System.Security.Claims;

namespace SkillChallenge.Controllers
{
    [ApiController]
    [Route("/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepo;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserRepository userRepo, UserManager<User> userManager, ILogger<UserController> logger)
        {
            _userRepo = userRepo;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet("test-log")]
        public IActionResult TestLog()
        {
            _logger.LogInformation("This is a test log");
            return Ok("Logged!");
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userRepo.GetAllUsersAsync();
            var displayUsers = users
                .Select(u => new DisplayUserDTO
                {
                    Id = u.Id,
                    UserName = u.UserName ?? string.Empty,
                    Email = u.Email ?? string.Empty,
                    ProfilePicture = u.ProfilePicture,
                })
                .ToList();

            return Ok(displayUsers);
        }

        [HttpGet("id/{id}")]
        public async Task<IActionResult> GetUserById([FromRoute] string id)
        {
            var user = await _userRepo.GetUserByIdAsync(id);
            if (user == null)
                return NotFound($"User with id '{id}' was not found in the database");

            return Ok(
                new DisplayUserDTO
                {
                    Id = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    ProfilePicture = user.ProfilePicture,
                }
            );
        }

        [HttpGet("username/{username}")]
        public async Task<IActionResult> GetUserByUsername([FromRoute] string username)
        {
            var user = await _userRepo.GetUserByUsernameAsync(username);
            if (user == null)
                return NotFound($"User with username '{username}' was not found in the database");

            return Ok(
                new DisplayUserDTO
                {
                    Id = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    ProfilePicture = user.ProfilePicture,
                }
            );
        }

        [HttpPost("create-admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateAdmin([FromBody] RegisterUserDTO createAdminDTO)
        {
            var user = new User
            {
                UserName = createAdminDTO.Username,
                Email = createAdminDTO.Email,
            };

            var result = await _userManager.CreateAsync(user, createAdminDTO.Password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            await _userManager.AddToRoleAsync(user, "Admin");

            return Ok(
                new DisplayUserDTO
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    ProfilePicture = user.ProfilePicture,
                }
            );
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateUser(
            [FromRoute] string id,
            [FromBody] UpdateUserDTO updateUser
        )
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (userRole != "Admin" && currentUserId != id)
            {
                return Forbid();
            }

            var (result, user) = await _userRepo.UpdateUserAsync(id, updateUser);

            if (user == null)
            {
                return NotFound($"User with id {id} was not found in the database");
            }

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok(
                new DisplayUserDTO
                {
                    Id = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    ProfilePicture = user.ProfilePicture,
                }
            );
        }


        [HttpPost("{id}/upload-profile-picture")]
        public async Task<IActionResult> UploadProfilePicture(
            string id,
            IFormFile file,
            [FromServices] IProfilePictureStorage storage)
        {
            _logger.LogInformation("CUSTOM DEBUG MESSAGE: UploadProfilePicture endpoint called for user {UserId}", id);

            try
            {
                if (file == null || file.Length == 0)
                {
                    _logger.LogWarning("CUSTOM DEBUG MESSAGE: No file uploaded for user {UserId}", id);
                    return BadRequest("No file uploaded.");
                }


                var user = await _userRepo.GetUserByIdAsync(id);
                if (user == null)
                {
                    _logger.LogWarning("CUSTOM DEBUG MESSAGE: User not found: {UserId}", id);
                    return NotFound();
                }

                if (!string.IsNullOrEmpty(user.ProfilePicture))
                    await storage.DeleteAsync(user.ProfilePicture);

                var pictureUrl = await storage.SaveAsync(file);

                user.ProfilePicture = pictureUrl;
                await _userRepo.UpdateUserAsync(id, new UpdateUserDTO { ProfilePicture = user.ProfilePicture });

                _logger.LogInformation("CUSTOM DEBUG MESSAGE: Profile picture uploaded successfully for user {UserId}", id);
                _logger.LogInformation("CUSTOM DEBUG MESSAGE: Uploaded file URL: {Url}", pictureUrl);
                return Ok(new { profilePictureUrl = user.ProfilePicture });
            }
            catch (Exception ex)
            {
                // Log the exception (to console, file, or Application Insights)
                _logger.LogError(ex, "CUSTOM DEBUG MESSAGE: Error while uploading profile picture for user {UserId}", id);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("{id}/change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(string id, [FromBody] ChangePasswordDTO dto)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (userRole != "Admin" && currentUserId != id)
            {
                return Forbid("You can only change your own password");
            }

            var result = await _userRepo.ChangePasswordAsync(
                id,
                dto.CurrentPassword,
                dto.NewPassword
            );

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok("Password changed successfully.");
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteUser([FromRoute] string id, [FromServices] IProfilePictureStorage storage)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var user = await _userRepo.GetUserByIdAsync(id); // kan IUserRepository returnera usern istället för att kalla på den här?

            if (userRole != "Admin" && currentUserId != id)
            {
                return Forbid();
            }

            var response = await _userRepo.DeleteUserAsync(id);

            if (!response)
            {
                return NotFound($"User with id {id} was not found in the database");
            }

            if (!string.IsNullOrEmpty(user.ProfilePicture))
                await storage.DeleteAsync(user.ProfilePicture);

            return NoContent();
        }
    }
}
