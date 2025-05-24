namespace SkillChallenge.DTOs
{
    public class CreateUserDTO
    {
        //[Required]
        //[EmailAddress]
        //[Unique]
        //public string Email { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ProfilePicture { get; set; } = string.Empty;
    }
}
