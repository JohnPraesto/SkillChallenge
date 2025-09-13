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
        public long? FileSize { get; set; }
        // The FileSize property is used in the UploadResult endpoint
        // in the ChallengeController. Before a user can upload a result
        // the FileSizes of all UploadedResults in the database are summed up
        // and compared to a GB threshold to make sure the Azure Storage
        // does not store too much and there by surprise costs too much!
    }
}
