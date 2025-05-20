using Microsoft.AspNetCore.Mvc;
using SkillChallenge.DTOs;
using SkillChallenge.Interfaces;
using SkillChallenge.Mappers;

namespace SkillChallenge.Controllers
{
    [ApiController]
    [Route("/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepo;

        public UserController(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userRepo.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetUserById([FromRoute] int id)
        {
            var user = await _userRepo.GetUserByIdAsync(id);

            if (user == null)
            {
                return NotFound($"User with id {id} was not found in the database");
            }

            return Ok(user);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDTO newUser)
        {
            var user = newUser.ToUserFromCreateUserDTO();
            await _userRepo.CreateUserAsync(user);
            return CreatedAtAction(nameof(GetUserById), new { id = user.UserId }, user);
        }

        [HttpPut]
        [Route("{id:int}")]
        public async Task<IActionResult> UpdateUser(
            [FromRoute] int id,
            [FromBody] UpdateUserDTO updateUser
        )
        {
            var user = await _userRepo.UpdateUserAsync(id, updateUser);

            if (user == null)
            {
                return NotFound($"User with id {id} was not found in the database");
            }

            return Ok(user);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteUser([FromRoute] int id)
        {
            var user = await _userRepo.DeleteUserAsync(id);

            if (user == null)
            {
                return NotFound($"User with id {id} was not found in the database");
            }

            return NoContent();
        }
    }
}
