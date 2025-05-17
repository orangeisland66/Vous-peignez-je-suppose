using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using backend.Models;
using backend.Data;
using System.Security.Cryptography;
using System.Text;

namespace backend.Services
{
    public class UserService
    {
        private readonly OurDbContext _context;

        public UserService(OurDbContext context)
        {
            _context = context;
        }

        // 用户注册
        public async Task<User> RegisterAsync(User user)
        {
            // 检查用户名是否已存在
            if (await _context.Users.AnyAsync(u => u.Username == user.Username))
            {
                throw new ArgumentException("用户名已存在");
            }

            // 密码加密
            user.PasswordHash = HashPassword(user.PasswordHash);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

         // 密码加密方法
        private string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        // 用户登录
        public async Task<User> LoginAsync(string username, string passwordHash)
        {
            var hashedPassword = HashPassword(passwordHash);
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username && u.PasswordHash == hashedPassword);
        }

        // 更新用户资料
        public async Task<User> UpdateProfileAsync(User user)
        {
            var existingUser = await _context.Users.FindAsync(user.Id);
            if (existingUser == null)
            {
                throw new ArgumentException("用户不存在");
            }

            existingUser.Username = user.Username;
            existingUser.PasswordHash = user.PasswordHash;
            existingUser.totalScore = user.totalScore;
            existingUser.Role = user.Role;
            existingUser.isOnline = user.isOnline;

            await _context.SaveChangesAsync();
            return existingUser;
        }

        // 获取用户总分数
        public async Task<int> GetTotalScore(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("用户不存在");
            }

            return user.totalScore;
        }

        // 设置用户角色
        public async Task SetUserRole(int userId, string role)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("用户不存在");
            }

            user.Role = role;
            await _context.SaveChangesAsync();
        }

        // 设置用户在线状态
        public async Task SetOnlineStatus(int userId, bool status)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("用户不存在");
            }

            user.isOnline = status;
            await _context.SaveChangesAsync();
        }
    }
}