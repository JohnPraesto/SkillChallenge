namespace SkillChallenge.Interfaces
{
    public interface IProfilePictureStorage
    {
        Task<string> SaveAsync(IFormFile file);
        Task DeleteAsync(string pictureUrl);
    }
}
