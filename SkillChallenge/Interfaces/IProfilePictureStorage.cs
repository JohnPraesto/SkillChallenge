namespace SkillChallenge.Interfaces
{
    public interface IProfilePictureStorage
    {
        Task<string> SaveAsync(IFormFile file);
    }
}
