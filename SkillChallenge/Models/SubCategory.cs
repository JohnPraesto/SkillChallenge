namespace SkillChallenge.Models
{
    public class SubCategory
    {
        public int SubCategoryId { get; set; }
        public string SubCategoryName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public string? ImagePath { get; set; }
    }
}
