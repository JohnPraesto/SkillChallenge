using Microsoft.AspNetCore.Identity;

namespace SkillChallenge.Models
{
    public class User : IdentityUser
    {
        public string? ProfilePicture { get; set; }
        public ICollection<Challenge> Challenges { get; set; } = new List<Challenge>();
        public ICollection<CategoryRatingEntity> CategoryRatingEntities { get; set; } = new List<CategoryRatingEntity>();
    }
}
