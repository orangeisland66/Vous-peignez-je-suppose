using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using backend.Models;

namespace backend.Services
{
    public class UserService
    {
        private readonly DbContext _context;

        public UserService(DbContext context)
        {
            _context = context;
        }

        // 用户注册
        public async Task<User> RegisterAsync(User user)
        {
            // 检查用户名是否已存在
            if (await _context.Users.AnyAsync(u => u.username == user.username))
            {
                throw new ArgumentException("用户名已存在");
            }

            // 这里可以添加密码加密逻辑
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        // 用户登录
        public async Task<User> LoginAsync(string username, string passwordHash)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.username == username && u.passwordHash == passwordHash);
        }

        // 更新用户资料
        public async Task<User> UpdateProfileAsync(User user)
        {
            var existingUser = await _context.Users.FindAsync(user.id);
            if (existingUser == null)
            {
                throw new ArgumentException("用户不存在");
            }

            existingUser.username = user.username;
            existingUser.passwordHash = user.passwordHash;
            existingUser.totalScore = user.totalScore;
            existingUser.userRole = user.userRole;
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

            user.userRole = role;
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