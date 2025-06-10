namespace SkillChallenge.DTOs.Challenge
{
    public class CreateChallengeDTO
    {
        public string ChallengeName { get; set; } = string.Empty;
        public DateTime EndDate { get; set; }
        public DateTime TimePeriod { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsPublic { get; set; }
        public int? SubCategoryId { get; set; }
    }
}
