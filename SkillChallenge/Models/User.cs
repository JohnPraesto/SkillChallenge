using Microsoft.AspNetCore.Identity;

namespace SkillChallenge.Models
{
    public class User : IdentityUser
    {
        public string ProfilePicture { get; set; } = string.Empty;
        // Tror det ska tillsättas relationstabell prop här..
    }
}
