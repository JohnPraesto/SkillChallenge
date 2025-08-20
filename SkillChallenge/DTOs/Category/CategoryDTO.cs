using SkillChallenge.DTOs.SubCategory;

namespace SkillChallenge.DTOs.Category
{
    public class CategoryDTO
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public List<SubCategoryDTO> SubCategories { get; set; } = new();
    }
}
