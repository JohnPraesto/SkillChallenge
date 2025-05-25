using SkillChallenge.Models;

namespace SkillChallenge.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(User user);
    }
}
