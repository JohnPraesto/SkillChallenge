using SkillChallenge.Interfaces;

namespace SkillChallenge.Repositories
{
    public class LocalProfilePictureStorage : IProfilePictureStorage
    {
        public async Task<string> SaveAsync(IFormFile file)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "profile-pictures");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/profile-pictures/{uniqueFileName}";
        }
    }
}
