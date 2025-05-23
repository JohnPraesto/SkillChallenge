﻿using SkillChallenge.DTOs;
using SkillChallenge.Models;

namespace SkillChallenge.Interfaces
{
    public interface IUserRepository
    {
        Task<List<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(int id);
        Task<User> CreateUserAsync(User user);

        Task<User?> UpdateUserAsync(int id, UpdateUserDTO updateUser);
        Task<User?> DeleteUserAsync(int id);
        Task<bool> UserExists(int id);
    }
}
