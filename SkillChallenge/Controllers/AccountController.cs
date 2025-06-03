using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillChallenge.DTOs;
using SkillChallenge.Interfaces;
using SkillChallenge.Models;

namespace ASPNET_VisualStudio_Tutorial.Controllers
{
    [Route("account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly ITokenService _tokenService;
        private readonly SignInManager<User> _signinManager;

        public AccountController(
            UserManager<User> userManager,
            ITokenService tokenService,
            SignInManager<User> signInManager
        )
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _signinManager = signInManager;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO loginDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = await _userManager.Users.FirstOrDefaultAsync(x =>
                x.UserName == loginDTO.UserName.ToLower()
            );

            if (user == null)
                return Unauthorized("Invalid username!");

            var result = await _signinManager.CheckPasswordSignInAsync(
                user,
                loginDTO.Password,
                false
            );

            if (!result.Succeeded)
                return Unauthorized("Username not found or password incorrect");

            var token = await _tokenService.CreateToken(user);

            return Ok(
                new NewUserDTO
                {
                    UserName = user.UserName,
                    Email = user.Email,
                    Token = token,
                }
            );
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDTO registerDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var user = new User { UserName = registerDTO.Username, Email = registerDTO.Email };
                var createdUser = await _userManager.CreateAsync(user, registerDTO.Password);

                if (createdUser.Succeeded)
                {
                    var roleResult = await _userManager.AddToRoleAsync(user, "User");
                    if (roleResult.Succeeded)
                    {
                        var token = await _tokenService.CreateToken(user);
                        return Ok(
                            new NewUserDTO
                            {
                                UserName = user.UserName,
                                Email = user.Email,
                                Token = token,
                            }
                        );
                    }
                    else
                    {
                        return StatusCode(500, roleResult.Errors);
                    }
                }
                else
                {
                    return BadRequest(createdUser.Errors);
                }
            }
            catch (Exception e)
            {
                return StatusCode(500, e);
            }
        }
    }
}
