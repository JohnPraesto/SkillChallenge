namespace SkillChallenge.Models
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public ICollection<UnderCategory> UnderCategories { get; set; }
    }
}
