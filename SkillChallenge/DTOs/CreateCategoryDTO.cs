namespace SkillChallenge.DTOs
{
    public class CreateCategoryDTO
    {
        public string CategoryName { get; set; } = string.Empty;
        public IFormFile? Image { get; set; }
    }
}
