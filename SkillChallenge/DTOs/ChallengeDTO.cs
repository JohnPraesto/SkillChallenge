namespace SkillChallenge.DTOs
{
    public class ChallengeDTO
    {
        public int ChallengeId { get; set; }
        public string ChallengeName { get; set; } = string.Empty;
        public DateTime EndDate { get; set; }
        public DateTime TimePeriod { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsPublic { get; set; }
        public int? UnderCategoryId { get; set; }
        public List<int> UserIds { get; set; } = new();
    }
}
