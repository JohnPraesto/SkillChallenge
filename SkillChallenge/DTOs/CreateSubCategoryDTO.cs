namespace SkillChallenge.DTOs
{
    public class CreateSubCategoryDTO
    {
        public string SubCategoryName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public IFormFile? Image { get; set; }
    }
}
