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

        public Task DeleteAsync(string pictureUrl)
        {
            if (string.IsNullOrWhiteSpace(pictureUrl))
                return Task.CompletedTask;

            // Remove leading slash if present
            var relativePath = pictureUrl.StartsWith("/") ? pictureUrl.Substring(1) : pictureUrl;

            // Build the absolute path
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath.Replace('/', Path.DirectorySeparatorChar));

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            return Task.CompletedTask;
        }
    }
}
