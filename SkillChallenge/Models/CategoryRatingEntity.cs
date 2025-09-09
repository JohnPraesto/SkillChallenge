namespace SkillChallenge.Models
{
    public class CategoryRatingEntity
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public string UserId { get; set; }
        public List<SubCategoryRatingEntity> SubCategoryRatingEntities { get; set; } = new List<SubCategoryRatingEntity>();
    }
}
