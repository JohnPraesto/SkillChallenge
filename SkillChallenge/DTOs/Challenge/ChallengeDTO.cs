using SkillChallenge.DTOs.SubCategory;

namespace SkillChallenge.DTOs.Challenge
{
    public class ChallengeDTO
    {
        public int ChallengeId { get; set; }
        public string ChallengeName { get; set; } = string.Empty;
        public DateTime EndDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsPublic { get; set; }
        public int? SubCategoryId { get; set; }
        public SubCategoryDTO? SubCategory { get; set; }
        public List<string> JoinedUsers { get; set; } = new();
        public string CreatedBy { get; set; } = string.Empty;
        public string CreatorUserName { get; set; } = string.Empty;
    }
}
