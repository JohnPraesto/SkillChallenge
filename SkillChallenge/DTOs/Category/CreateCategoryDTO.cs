namespace SkillChallenge.DTOs.Category
{
    public class CreateCategoryDTO
    {
        public string CategoryName { get; set; } = string.Empty;
        public IFormFile? Image { get; set; }
    }
}
