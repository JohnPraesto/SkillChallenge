namespace SkillChallenge.DTOs
{
    public class LeaderboardCategoryDTO
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public List<LeaderboardSubCategoryDTO> SubCategories { get; set; }
    }

    public class LeaderboardSubCategoryDTO
    {
        public int SubCategoryId { get; set; }
        public string SubCategoryName { get; set; }
        public string? ImagePath { get; set; }
        public List<LeaderboardUserDTO> TopUsers { get; set; }
    }

    public class LeaderboardUserDTO
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string ProfilePicture { get; set; }
        public int Rating { get; set; }
    }
}
