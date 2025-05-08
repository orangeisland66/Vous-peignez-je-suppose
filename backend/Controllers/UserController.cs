using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Http;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        // 用户注册接口
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            if (user == null)
            {
                return BadRequest("用户信息不能为空");
            }

            try
            {
                var result = await _userService.RegisterAsync(user);
                return Ok(new { Message = "注册成功", UserId = result.Id });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        // 用户登录接口
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest loginRequest)
        {
            if (loginRequest == null || string.IsNullOrEmpty(loginRequest.Username) || string.IsNullOrEmpty(loginRequest.Password))
            {
                return BadRequest("用户名和密码不能为空");
            }

            var user = await _userService.LoginAsync(loginRequest.Username, loginRequest.Password);
            if (user != null)
            {
                // 这里应该生成JWT token
                return Ok(new { Message = "登录成功", UserId = user.Id });
            }
            else
            {
                return Unauthorized(new { Message = "用户名或密码错误" });
            }
        }

        // 获取当前用户信息接口
        [HttpGet("profile")]
        public async Task<IActionResult> GetUserProfile(int userId)
        {
            try
            {
                var totalScore = await _userService.GetTotalScore(userId);
                return Ok(new { TotalScore = totalScore });
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
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
