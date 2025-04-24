using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using you_draw_i_guess_project.Models;
using you_draw_i_guess_project.Services;
using Microsoft.AspNetCore.Http;

namespace you_draw_i_guess_project.Controllers
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

            var result = await _userService.RegisterUserAsync(user);
            if (result.IsSuccess)
            {
                return Ok(new { Message = "注册成功" });
            }
            else
            {
                return BadRequest(new { Message = result.ErrorMessage });
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

            var result = await _userService.LoginAsync(loginRequest.Username, loginRequest.Password);
            if (result.IsSuccess)
            {
                return Ok(new { Message = "登录成功", Token = result.Token });
            }
            else
            {
                return Unauthorized(new { Message = result.ErrorMessage });
            }
        }

        // 获取当前用户信息接口
        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
            var currentUser = _userService.GetCurrentUser();
            if (currentUser == null)
            {
                return Unauthorized(new { Message = "用户未登录" });
            }
            return Ok(currentUser);
        }
    }

    // 用户登录请求模型
    public class UserLoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
