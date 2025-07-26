namespace SkillChallenge.DTOs.Challenge
{
    public class UploadedResultDTO
    {
        public int UploadedResultId { get; set; }
        public string? Url { get; set; }
        public int ChallengeId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public DateTime SubmissionDate { get; set; }
    }
}
