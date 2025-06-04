namespace SkillChallenge.DTOs
{
    public class CategoryDTO
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public List<UnderCategoryDTO> UnderCategories { get; set; } = new();
    }
}
