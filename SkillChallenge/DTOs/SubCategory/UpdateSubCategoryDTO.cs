namespace SkillChallenge.DTOs.SubCategory
{
    public class UpdateSubCategoryDTO
    {
        public string SubCategoryName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public IFormFile? Image { get; set; }
    }
}
