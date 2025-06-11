using Microsoft.AspNetCore.Identity;
using SkillChallenge.DTOs.User;
using SkillChallenge.Models;

namespace SkillChallenge.Interfaces
{
    public interface IUserRepository
    {
        Task<List<User>> GetAllUsersAsync();
        Task<User?> GetUserByUsernameAsync(string username);
        Task<User?> GetUserByIdAsync(string id);
        Task<(IdentityResult, User?)> UpdateUserAsync(string id, UpdateUserDTO updateUser);
        Task<IdentityResult> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
        Task<bool> DeleteUserAsync(string id);
        Task<bool> UserExists(string id);
    }
}
