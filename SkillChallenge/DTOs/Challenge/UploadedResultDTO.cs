using SkillChallenge.Models;

namespace SkillChallenge.DTOs.Challenge
{
    public class UploadedResultDTO
    {
        public int UploadedResultId { get; set; }
        public string? Url { get; set; }
        public int ChallengeId { get; set; }
        public ICollection<VoteEntity> Votes { get; set; } = new List<VoteEntity>();
        public string UserId { get; set; }
        public string UserName { get; set; }
        public DateTime SubmissionDate { get; set; }
    }
}
