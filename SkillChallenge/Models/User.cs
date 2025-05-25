using Microsoft.AspNetCore.Identity;

namespace SkillChallenge.Models
{
    public class User : IdentityUser
    {
        //public int UserId { get; set; } // vad får usern för ID om denna inte är här och tillsätts
        // automatiskt av entity framework när den läggs till i databasen?
        //public string Username { get; set; } = string.Empty;
        //public string Password { get; set; } = string.Empty;
        public string ProfilePicture { get; set; } = string.Empty;
    }
}
