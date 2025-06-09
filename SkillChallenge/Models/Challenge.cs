namespace SkillChallenge.Models
{
    public class Challenge
    {
        public int ChallengeId { get; set; }
        public string ChallengeName { get; set; } = string.Empty;
        public DateTime EndDate { get; set; }
        public DateTime TimePeriod { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsPublic { get; set; }
        public ICollection<User> Users { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public User? Creator { get; set; }

        // Many-to-many relationship med User?
        // Connection table?
        public int? SubCategoryId { get; set; }
        public SubCategory? SubCategory { get; set; }
    }
}
