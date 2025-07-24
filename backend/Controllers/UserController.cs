using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using backend.Models; // 确保引入 User 和 UserLoginRequest
using backend.Services;
using Microsoft.AspNetCore.Http;
using System; // 用于 ArgumentException
using Microsoft.EntityFrameworkCore; // 如果在 Controller 层检查唯一性
using Microsoft.AspNetCore.Authorization; // 用于 AllowAnonymous

namespace backend.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        // 如果在 Controller 层检查唯一性，需要注入 DbContext
        private readonly backend.Data.OurDbContext _context; //  DbContext 类在 backend.Data 命名空间下

        public UserController(UserService userService, backend.Data.OurDbContext context) // 注入 DbContext
        {
            _userService = userService;
            _context = context;
        }

        // 用户注册接口
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            // 基础验证：检查必需字段是否为空
            if (user == null || string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.PasswordHash))
            {
                return BadRequest(new { message = "用户名、邮箱和密码不能为空" });
            }

            try
            {
                // **4. 调用 UserService 进行注册**
                // 将接收到的用户名、邮箱和明文密码 (从 user.PasswordHash 获取) 传递给 UserService
                var result = await _userService.RegisterAsync(user.Username, user.Email, user.PasswordHash); // user.PasswordHash 此时是明文密码

                // **5. 返回成功响应**
                // 返回 201 Created，表示资源已创建成功
                // 返回一个包含 message 属性的匿名对象，以及一些非敏感的用户信息
                return CreatedAtAction(nameof(GetUserProfile), new { userId = result.Id }, // 可以跳转到 GetUserProfile 路由
                   new { message = "注册成功", userId = result.Id, username = result.Username, email = result.Email });

            }
            catch (ArgumentException ex)
            {
                // 捕获 UserService 可能抛出的 ArgumentException (例如，如果 Service 层也做了重复检查)
                return BadRequest(new { Message = ex.Message });
            }
        }

        // 用户登录接口
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest loginRequest)
        {
            if (loginRequest == null || (string.IsNullOrEmpty(loginRequest.Username)&& string.IsNullOrEmpty(loginRequest.Email)) || string.IsNullOrEmpty(loginRequest.Password))
            {
                return BadRequest("用户名/邮箱和密码不能为空");
            }

            var user = await _userService.LoginAsync(loginRequest.Username, loginRequest.Email, loginRequest.Password);
            if (user != null)
            {
                // 这里应该生成JWT token
                //var token = GenerateJwtToken(user);
                return Ok(new { Message = "登录成功", userId = user.Id, userName=user.Username });
            }
            else
            {
                return Unauthorized(new { Message = "用户名或密码错误" });
            }
        }

        [HttpGet("profile")] // 完整的路由是 /api/users/profile
        // [Authorize] // 通常需要认证才能获取自己的资料
        public async Task<IActionResult> GetUserProfile(int userId)
        {
            // **重要：在实际应用中，这里应该从认证信息中获取当前用户ID，而不是从查询参数或请求体中获取**
            // 例如：var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            // if (currentUserId == null || int.Parse(currentUserId) != userId) { return Forbid(); } // 或者 Unauthorized/Forbidden

            try
            {
                var user = await _context.Users.FindAsync(userId); // 直接从 DbContext 获取用户，或者调用 UserService 的方法
                if (user == null)
                {
                    return NotFound(new { Message = "用户不存在" });
                }
                // 返回用户资料的 DTO 或匿名对象 (不要包含 PasswordHash)
                return Ok(new
                {
                    UserId = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    TotalScore = user.totalScore,
                    Role = user.Role,
                    IsOnline = user.isOnline,
                    AvatarUrl = user.AvatarUrl,
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = user.LastLoginAt,
                    Status = user.Status // 根据需要是否返回状态
                });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] User user)
        {
            try
            {
                var updatedUser = await _userService.UpdateProfileAsync(user);
                return Ok(new { Message = "资料更新成功", User = updatedUser });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPut("role")]
        public async Task<IActionResult> SetUserRole(int userId, [FromBody] string role)
        {
            try
            {
                await _userService.SetUserRole(userId, role);
                return Ok(new { Message = "角色设置成功" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPut("online-status")]
        public async Task<IActionResult> SetOnlineStatus(int userId, [FromBody] bool status)
        {
            try
            {
                await _userService.SetOnlineStatus(userId, status);
                return Ok(new { Message = "在线状态更新成功" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }

    // 用户登录请求模型
    public class UserLoginRequest
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        
    }
}
