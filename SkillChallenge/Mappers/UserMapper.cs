using SkillChallenge.DTOs;
using SkillChallenge.Models;

namespace SkillChallenge.Mappers
{
    public static class UserMapper
    {
        public static User ToUserFromCreateUserDTO(this CreateUserDTO user)
        {
            return new User
            {
                UserName = user.UserName,
                Password = user.Password,
                ProfilePicture = user.ProfilePicture,
            };
        }
    }
}
