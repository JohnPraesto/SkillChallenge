using System.ComponentModel.DataAnnotations;

namespace SkillChallenge.DTOs.Account
{
    public class ForgotPasswordDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Url]
        public string ResetLinkBaseUrl { get; set; }
    }
}
