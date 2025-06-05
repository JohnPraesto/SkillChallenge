namespace SkillChallenge.DTOs
{
    public class UnderCategoryDTO
    {
        public int UnderCategoryId { get; set; }
        public string UnderCategoryName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
    }
}
