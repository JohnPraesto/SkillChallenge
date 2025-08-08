namespace SkillChallenge.Models
{
    public class SubCategoryRatingEntity
    {
        public int Id { get; set; }
        public int SubCategoryId { get; set; }
        public SubCategory SubCategory { get; set; }
        public int Rating { get; set; }
    }
}
