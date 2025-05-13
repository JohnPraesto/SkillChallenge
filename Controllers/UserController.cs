using Microsoft.AspNetCore.Mvc;
using SkillChallenge.Interfaces;

namespace SkillChallenge.Controllers
{
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepo;

        public UserController(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        //[HttpGet]
        //public async Task<User> GetAllUsers() { }
    }
}
