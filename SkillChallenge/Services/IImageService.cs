namespace SkillChallenge.Services
{
    public interface IImageService
    {
        Task<string> SaveImageAsync(IFormFile image, string folder);
        string GetImageUrl(string? imagePath);
        Task DeleteImageAsync(string pictureUrl);
    }

    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _env;

        public ImageService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> SaveImageAsync(IFormFile image, string folder)
        {
            var uploads = Path.Combine(_env.WebRootPath, folder);
            Directory.CreateDirectory(uploads);
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
            var filePath = Path.Combine(uploads, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }
            return Path.Combine(folder, fileName).Replace("\\", "/");
        }

        public string GetImageUrl(string? imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
                return "";
            return imagePath.StartsWith("/") ? imagePath : "/" + imagePath;
        }

        public Task DeleteImageAsync(string pictureUrl)
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
