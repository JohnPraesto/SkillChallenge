using Microsoft.EntityFrameworkCore;
using SkillChallenge.Data;
using SkillChallenge.DTOs;
using SkillChallenge.Interfaces;
using SkillChallenge.Models;

namespace SkillChallenge.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        //public async Task<User?> GetUserByIdAsync(string id)
        //{
        //    return await _context.Users.FindAsync(id);
        //}

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);
        }

        public async Task<User> CreateUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User?> UpdateUserAsync(string id, UpdateUserDTO updateUser)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return null;

            user.UserName = updateUser.UserName;
            //user.Password = updateUser.Password;
            // chatten: " If you need to update the password, use UserManager.ChangePasswordAsync or similar in your service/controller layer."
            user.ProfilePicture = updateUser.ProfilePicture;

            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User?> DeleteUserAsync(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return null;
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> UserExists(string id)
        {
            return await _context.Users.AnyAsync(u => u.Id == id);
        }
    }
}
