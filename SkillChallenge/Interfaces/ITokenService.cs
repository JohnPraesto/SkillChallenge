using SkillChallenge.Models;

namespace SkillChallenge.Interfaces
{
    public interface ITokenService
    {
        Task<string> CreateToken(User user);
    }
}
