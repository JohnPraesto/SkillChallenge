namespace SkillChallenge.Models
{
    public class UploadedResultRequest
    {
        public string? YoutubeUrl { get; set; }
        public IFormFile? File { get; set; }
    }
}
