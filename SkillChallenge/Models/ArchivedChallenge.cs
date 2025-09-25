namespace SkillChallenge.Models
{
    public class ArchivedChallenge
    {
        public int ArchivedChallengeId { get; set; }
        public int ChallengeId { get; set; }
        public string ChallengeName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string SubCategoryName { get; set; }
        public DateTime EndDate { get; set; }
        public List<ArchivedChallengeUser> Users { get; set; }
    }
}
