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

        // Many-to-many relationship med User?
        // Connection table?
        public UnderCategory? UnderCategory { get; set; }
    }
}
