namespace SkillChallenge.Services
{
    public interface IImageService
    {
        Task<string> SaveImageAsync(IFormFile image, string folder);
        string GetImageUrl(string? imagePath);
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
            var uploads = Path.Combine(_env.WebRootPath, "images", folder);
            Directory.CreateDirectory(uploads);
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
            var filePath = Path.Combine(uploads, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }
            return Path.Combine("images", folder, fileName).Replace("\\", "/");
        }

        public string GetImageUrl(string? imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
                return "";
            return $"/{imagePath}";
        }
    }
}
