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

        public UserController(IUserRepository userRepo, UserManager<User> userManager)
        {
            _userRepo = userRepo;
            _userManager = userManager;
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
                    CategoryRatingEntities = user.CategoryRatingEntities.Select(cre => new CategoryRatingDTO
                    {
                        CategoryId = cre.CategoryId,
                        CategoryName = cre.Category?.CategoryName ?? string.Empty,
                        SubCategoryRatingEntities = cre.SubCategoryRatingEntities.Select(sre => new SubCategoryRatingDTO
                        {
                            SubCategoryId = sre.SubCategoryId,
                            SubCategoryName = sre.SubCategory?.SubCategoryName ?? string.Empty,
                            Rating = sre.Rating
                        }).ToList()
                    }).ToList()
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
                    CategoryRatingEntities = user.CategoryRatingEntities.Select(cre => new CategoryRatingDTO
                    {
                        CategoryId = cre.CategoryId,
                        CategoryName = cre.Category?.CategoryName ?? string.Empty,
                        SubCategoryRatingEntities = cre.SubCategoryRatingEntities.Select(sre => new SubCategoryRatingDTO
                        {
                            SubCategoryId = sre.SubCategoryId,
                            SubCategoryName = sre.SubCategory?.SubCategoryName ?? string.Empty,
                            Rating = sre.Rating
                        }).ToList()
                    }).ToList()
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
        public async Task<IActionResult> UpdateUser([FromRoute] string id, [FromBody] UpdateUserDTO updateUser)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (userRole != "Admin" && currentUserId != id)
                return Forbid();

            var (result, user) = await _userRepo.UpdateUserAsync(id, updateUser);

            if (user == null)
                return NotFound($"User with id {id} was not found in the database");

            if (!result.Succeeded)
                return BadRequest(result.Errors);

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
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file uploaded.");
                }

                var user = await _userRepo.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                if (!string.IsNullOrEmpty(user.ProfilePicture))
                    await storage.DeleteAsync(user.ProfilePicture);

                var pictureUrl = await storage.SaveAsync(file);

                user.ProfilePicture = pictureUrl;
                await _userRepo.UpdateUserAsync(id, new UpdateUserDTO { ProfilePicture = user.ProfilePicture });

                return Ok(new { profilePictureUrl = user.ProfilePicture });
            }
            catch (Exception ex)
            {
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

        [HttpPut("{id}/change-role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUserRole([FromRoute] string id, [FromBody] string role)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (userRole != "Admin")
                return Forbid();

            if (role != "Admin" && role != "User")
                return BadRequest("Role must be either 'Admin' or 'User'.");

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound($"User with id {id} was not found in the database");

            var currentRoles = await _userManager.GetRolesAsync(user);
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);

            if (!removeResult.Succeeded)
                return StatusCode(500, removeResult.Errors);

            var addResult = await _userManager.AddToRoleAsync(user, role);

            if (!addResult.Succeeded)
                return StatusCode(500, addResult.Errors);

            return Ok($"User role updated to '{role}'.");
        }
    }
}
