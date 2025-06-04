namespace SkillChallenge.DTOs
{
    public class UpdateUnderCategoryDTO
    {
        public string UnderCategoryName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public IFormFile? Image { get; set; }
    }
}
