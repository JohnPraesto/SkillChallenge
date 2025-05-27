namespace SkillChallenge.DTOs
{
    public class UpdateChallengeDTO
    {
        public string ChallengeName { get; set; } = string.Empty;
        public DateTime EndDate { get; set; }
        public DateTime TimePeriod { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsPublic { get; set; }
        public int? UnderCategoryId { get; set; }
    }
}
