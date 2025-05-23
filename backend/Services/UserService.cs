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
        public async Task<User> RegisterAsync(string username, string email, string plainPassword)
        {
            string passwordHash = HashPassword(plainPassword);
            var newUser = new User
            {
                Username = username,
                Email = email,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow,
                Status = UserStatus.Active, // 默认状态为激活
                Role = "User", // 默认角色为普通用户
                AvatarUrl = "", // 默认头像链接为空
                totalScore = 0, // 默认总分数为0
                userRole = "User", // 默认角色为普通用户
                isOnline = false // 默认在线状态为false
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            return newUser;
        }


        // 用户登录
        // 修改方法签名，接收明文密码
       public async Task<User> LoginAsync(string username, string email, string password)
        {
            User user = null;
            if (!string.IsNullOrEmpty(username))
            {
                user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            }
            else if (!string.IsNullOrEmpty(email))
            {
                user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            }

            if (user != null && VerifyPassword(password, user.PasswordHash))
            {
                return user;
            }

            return null;
        }

        private bool VerifyPassword(string password, string passwordHash)
        {
            string hashedPassword = HashPassword(password);
            return hashedPassword == passwordHash;
        }

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