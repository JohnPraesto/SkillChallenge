using Microsoft.AspNetCore.Mvc;
using SkillChallenge.DTOs;
using SkillChallenge.Interfaces;

namespace SkillChallenge.Controllers
{
    [ApiController]
    [Route("/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepo;
        private readonly ITokenService _tokenService;

        public UserController(IUserRepository userRepo, ITokenService tokenService)
        {
            _userRepo = userRepo;
            _tokenService = tokenService;
        }

        // Vad ska visas? inte password hash?
        // Hur ska GetAllUsers användas?
        // Av användare på sidan?
        // Vilken information vill/får de se?
        // DisplayUserDTO
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userRepo.GetAllUsersAsync();
            return Ok(users);
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
                    UserName = user.UserName,
                    Email = user.Email,
                    ProfilePicture = user.ProfilePicture,
                }
            );
        }

        // Ska DisplayUserDTO endast ha username och bild kanske?
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
                    UserName = user.UserName,
                    Email = user.Email,
                    ProfilePicture = user.ProfilePicture,
                }
            );
        }

        // A user should only be able to update his or her own profile
        // An admin should be able to update all accounts
        // An admin should be able to handle user passwords?
        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> UpdateUser(
            [FromRoute] string id,
            [FromBody] UpdateUserDTO updateUser
        )
        {
            var (result, updatedUser) = await _userRepo.UpdateUserAsync(id, updateUser);

            if (!result.Succeeded)
            {
                if (result.Errors.Any(e => e.Description.Contains("User not found")))
                    return NotFound($"User with id {id} was not found in the database");

                return BadRequest(result.Errors);
            }

            return Ok(
                new
                {
                    userName = updatedUser.UserName,
                    profilePicture = updatedUser.ProfilePicture,
                    token = _tokenService.CreateToken(updatedUser),
                }
            );
        }

        // Should be for a logged in user changing its own password
        // Behöver kontrolleras.
        [HttpPost("{id}/change-password")]
        public async Task<IActionResult> ChangePassword(string id, [FromBody] ChangePasswordDTO dto)
        {
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

        // A user should only be able to delete his or her own profile
        // An admin should be able to delete any account
        // This is not yet implemented
        // Something with authorize and roles?
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser([FromRoute] string id)
        {
            var response = await _userRepo.DeleteUserAsync(id);

            if (!response)
            {
                return NotFound($"User with id {id} was not found in the database");
            }

            return NoContent();
        }
    }
}
