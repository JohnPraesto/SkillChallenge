namespace SkillChallenge.Models
{
    public class UnderCategory
    {
        public int UnderCategoryId { get; set; }
        public string UnderCategoryName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public string? ImagePath { get; set; }
    }
}
