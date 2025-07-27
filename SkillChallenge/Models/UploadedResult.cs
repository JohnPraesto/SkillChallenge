namespace SkillChallenge.Models
{
    public class UploadedResult
    {
        public int UploadedResultId { get; set; }
        public string? Url { get; set; }
        public int ChallengeId { get; set; }
        public ICollection<VoteEntity> Votes { get; set; } = new List<VoteEntity>();
        public Challenge Challenge { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        public DateTime SubmissionDate { get; set; }
    }
}
