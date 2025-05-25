using SkillChallenge.DTOs;
using SkillChallenge.Models;

namespace SkillChallenge.Interfaces
{
    public interface IUserRepository
    {
        Task<List<User>> GetAllUsersAsync();

        //Task<User?> GetUserByIdAsync(string id);
        Task<User?> GetUserByUsernameAsync(string username);

        Task<User?> UpdateUserAsync(string id, UpdateUserDTO updateUser);
        Task<User?> DeleteUserAsync(string id);
        Task<bool> UserExists(string id);
    }
}
