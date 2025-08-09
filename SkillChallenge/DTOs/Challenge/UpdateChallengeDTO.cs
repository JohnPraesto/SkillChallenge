namespace SkillChallenge.DTOs.Challenge
{
    public class UpdateChallengeDTO
    {
        public string ChallengeName { get; set; } = string.Empty;
        public DateTime EndDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public int NumberOfParticipants { get; set; }
        public bool IsPublic { get; set; }
        public int? SubCategoryId { get; set; }
    }
}
