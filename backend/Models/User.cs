using System;
using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class User
    {
        // 用户唯一标识符
        public int Id { get; set; }

        // 用户名，必须唯一
        [Required]
        [StringLength(100)]
        public string Username { get; set; }

        // 密码（存储为哈希值，不直接存储明文密码）
        [Required]
        [StringLength(256)]
        public string PasswordHash { get; set; }

        // 用户邮箱，必须唯一
        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; }

        // 注册时间
        public DateTime CreatedAt { get; set; }

        // 上次登录时间
        public DateTime? LastLoginAt { get; set; }

        // 用户状态（例如激活、禁用等）
        public UserStatus Status { get; set; }

        // 用户角色（例如普通用户、管理员等）
        public string Role { get; set; }

        // 用户的头像链接
        public string AvatarUrl { get; set; }

        // 创建时自动设置创建时间
        public User()
        {
            CreatedAt = DateTime.UtcNow;
            Status = UserStatus.Active;  // 默认状态为激活
        }
    }

    // 用户状态枚举
    public enum UserStatus
    {
        Active = 1,    // 激活
        Suspended = 2, // 被暂停
        Banned = 3     // 被禁用
    }
}
