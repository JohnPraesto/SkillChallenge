namespace SkillChallenge.DTOs
{
    public class SubCategoryDTO
    {
        public int SubCategoryId { get; set; }
        public string SubCategoryName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
    }
}
