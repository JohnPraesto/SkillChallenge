using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillChallenge.DTOs.Account;
using SkillChallenge.Interfaces;
using SkillChallenge.Models;

namespace ASPNET_VisualStudio_Tutorial.Controllers
{
    [Route("/api/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly ITokenService _tokenService;
        private readonly SignInManager<User> _signinManager;
        private readonly IEmailService _emailService;

        public AccountController(
            UserManager<User> userManager,
            ITokenService tokenService,
            SignInManager<User> signInManager,
            IEmailService emailService
        )
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _signinManager = signInManager;
            _emailService = emailService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO loginDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == loginDTO.UserName.ToLower());

            if (user == null)
                return Unauthorized("Invalid username!");

            var result = await _signinManager.CheckPasswordSignInAsync(user, loginDTO.Password, false);

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

                var emailExists = await _userManager.Users.AnyAsync(u => u.Email == registerDTO.Email);
                if (emailExists)
                {
                    return BadRequest(new { Email = new[] { "Email is already in use." } });
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

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return Ok();

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = System.Web.HttpUtility.UrlEncode(token);
            var resetLink = $"{dto.ResetLinkBaseUrl.TrimEnd('/')}/reset-password?email={System.Web.HttpUtility.UrlEncode(user.Email)}&token={encodedToken}";

            await _emailService.SendEmailAsync(
                user.Email,
                "Password Reset",
                $"Click <a href='{resetLink}'>here</a> to reset your password."
            );

            return Ok(new { Message = "A reset link has been sent." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return Ok(new { Message = "If the email exists, the password has been reset." });

            var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { Message = "Password has been reset successfully." });
        }
    }
}
