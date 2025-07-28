namespace SkillChallenge.Models
{
    public class VoteEntity
    {
        public int Id { get; set; }
        public int ChallengeId { get; set; }
        public int UploadedResultId { get; set; }
        public string UserId { get; set; }
    }
}