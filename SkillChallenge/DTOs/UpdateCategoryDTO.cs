namespace SkillChallenge.DTOs
{
    public class UpdateCategoryDTO
    {
        public string CategoryName { get; set; } = string.Empty;
        public IFormFile? Image { get; set; }
    }
}
