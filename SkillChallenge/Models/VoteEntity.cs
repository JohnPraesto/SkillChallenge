using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace SkillChallenge.Models
{
    [Index(nameof(ChallengeId), nameof(ClientId), IsUnique = true)]
    public class VoteEntity
    {
        public int Id { get; set; }
        public int ChallengeId { get; set; }
        public int UploadedResultId { get; set; }

        [MaxLength(64)]
        public string ClientId { get; set; } = string.Empty; // Random token from secure cookie

        // Something about security
        // A bot could vote/unvote rapidly
        //public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
        //public DateTime? UpdatedUtc { get; set; }

        // Optional hashed metadata for rate limiting / anomaly detection
        [MaxLength(128)]
        public string? IpHash { get; set; }

        [MaxLength(128)]
        public string? UserAgentHash { get; set; }
    }
}
// Above code is for anonymous/not authenticated/not logged in voting






// Below code was used when user had to log in/be authenticated to vote
// namespace SkillChallenge.Models
// {
//     public class VoteEntity
//     {
//         public int Id { get; set; }
//         public int ChallengeId { get; set; }
//         public int UploadedResultId { get; set; }
//         public string UserId { get; set; }
//     }
// }