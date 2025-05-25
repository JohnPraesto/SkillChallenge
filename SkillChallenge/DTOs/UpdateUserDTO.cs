namespace SkillChallenge.DTOs
{
    public class UpdateUserDTO
    {
        public string UserName { get; set; } = string.Empty;

        //public string Password { get; set; } = string.Empty;
        // använd "UserManager" istället... (?) för att ändra lösenord
        public string ProfilePicture { get; set; } = string.Empty;
    }
}
