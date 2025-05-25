//using Microsoft.AspNetCore.Mvc;
//using SkillChallenge.DTOs;
//using SkillChallenge.Interfaces;

//namespace SkillChallenge.Controllers
//{
//    [ApiController]
//    [Route("/users")]
//    public class UserController : ControllerBase
//    {
//        private readonly IUserRepository _userRepo;

//        public UserController(IUserRepository userRepo)
//        {
//            _userRepo = userRepo;
//        }

//        [HttpGet]
//        public async Task<IActionResult> GetAllUsers()
//        {
//            var users = await _userRepo.GetAllUsersAsync();
//            return Ok(users);
//        }

//        [HttpGet("{username:string}")]
//        public async Task<IActionResult> GetUserByUsername([FromRoute] string username)
//        {
//            var user = await _userRepo.GetUserByUsernameAsync(username);
//            if (user == null)
//                return NotFound($"User with username '{username}' was not found in the database");

//            return Ok(
//                new DisplayUserDTO
//                {
//                    Id = user.Id,
//                    UserName = user.UserName,
//                    Email = user.Email,
//                    ProfilePicture = user.ProfilePicture,
//                }
//            );
//        }

//        // A user should only be able to update his or her own profile
//        // An admin should be able to update all accounts
//        // This is not yet implemented
//        [HttpPut]
//        [Route("{id:int}")]
//        public async Task<IActionResult> UpdateUser(
//            [FromRoute] string id,
//            [FromBody] UpdateUserDTO updateUser
//        )
//        {
//            var user = await _userRepo.UpdateUserAsync(id, updateUser);

//            if (user == null)
//            {
//                return NotFound($"User with id {id} was not found in the database");
//            }

//            return Ok(user);
//        }

//        // A user should only be able to delete his or her own profile
//        // An admin should be able to delete any account
//        // This is not yet implemented
//        [HttpDelete("{id:int}")]
//        public async Task<IActionResult> DeleteUser([FromRoute] string id)
//        {
//            var user = await _userRepo.DeleteUserAsync(id);

//            if (user == null)
//            {
//                return NotFound($"User with id {id} was not found in the database");
//            }

//            return NoContent();
//        }
//    }
//}
