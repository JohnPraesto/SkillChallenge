namespace SkillChallenge.DTOs
{
    public class CreateUnderCategoryDTO
    {
        public string UnderCategoryName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public IFormFile? Image { get; set; }
    }
}
