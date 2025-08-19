namespace SkillChallenge.DTOs.User
{
    public class DisplayUserDTO
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string ProfilePicture { get; set; }
        public List<CategoryRatingDTO> CategoryRatingEntities { get; set; } = new();
    }

    public class CategoryRatingDTO
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public List<SubCategoryRatingDTO> SubCategoryRatingEntities { get; set; } = new();
    }

    public class SubCategoryRatingDTO
    {
        public int SubCategoryId { get; set; }
        public string SubCategoryName { get; set; }
        public int Rating { get; set; }
    }
}
