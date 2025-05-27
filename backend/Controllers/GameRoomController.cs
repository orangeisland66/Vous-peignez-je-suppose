// E:\m_Documents\Project\Vous-peignez-je-suppose\backend\Controllers\GameRoomController.cs
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using backend.Models; // 确保 GameRoom 模型在这里
using backend.Services;
using backend.Hubs; // 确保 GameHub 在这里
using System.Runtime.InteropServices; // 确保 GameRoomService 在这里

namespace backend.Controllers
{
    // **修改控制器级别的路由**
    // 将 [Route("api/[controller]")] 改为 [Route("rooms")]
    [Route("rooms")]
    // [Route("rooms")] // <-- 修改这里
    [ApiController]
    public class GameRoomController : ControllerBase
    {
        private readonly GameRoomService _gameRoomService;
        private readonly IHubContext<GameHub> _hubContext;  // 添加这行
        public GameRoomController(
            GameRoomService gameRoomService,
            IHubContext<GameHub> hubContext)
        {
            _gameRoomService = gameRoomService;
            _hubContext = hubContext;
        }

        // 获取所有游戏房间
        // 完整路由: GET /rooms/list
        [HttpGet("list")]
        public async Task<IActionResult> GetRoomList()
        {
            var rooms = await _gameRoomService.GetAllRoomsAsync();
            return Ok(rooms);
        }

        // 创建一个新的游戏房间
        // 完整路由: POST /rooms/create
        [HttpPost("create")]
        public async Task<IActionResult> CreateRoom([FromBody] GameRoom newRoom)
        {
            if (newRoom == null || string.IsNullOrEmpty(newRoom.Name))
            {
                return BadRequest(new { success = false, message = "房间信息不完整" });
            }

            // newRoom.CreatorId 应该已经被模型绑定填充了
            // 注意：newRoom.Players 和 ChatHistory 会是 null 或空，因为前端没有发送它们
            // GameRoom 的构造函数会初始化它们为空列表

            var createdRoomInDb = await _gameRoomService.CreateRoomAsync(newRoom); // 将 newRoom 传递给服务层
            if (createdRoomInDb != null)
            {
                return Ok(new
                {
                    success = true,
                    dbId = createdRoomInDb.Id, // 数据库的 int Id
                    roomId = createdRoomInDb.RoomId, // 前端生成的8位字符串 RoomId
                    message = "房间创建成功"
                });
            }
            else
            {
                return BadRequest(new { success = false, message = "房间创建失败" });
            }
        }

        // 加入一个已有的游戏房间
        // 完整路由: POST /rooms/join/{roomId}
        [HttpPost("join/{roomId}")]
        public async Task<IActionResult> JoinRoom(
            [FromRoute] string roomId,
            [FromQuery] string userId,
            [FromBody] Player player)
        {
            try
            {
                Console.WriteLine($"接收到加入房间的请求 - 房间ID: {roomId}, 用户ID: {userId}, 玩家名称: {player?.Username}");

                // 参数验证
                if (string.IsNullOrEmpty(roomId))
                {
                    return BadRequest(new { success = false, message = "房间ID不能为空" });
                }

                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new { success = false, message = "用户ID不能为空" });
                }

                if (player == null || string.IsNullOrEmpty(player.Username))
                {
                    return BadRequest(new { success = false, message = "玩家信息不完整" });
                }

            var result = await _gameRoomService.JoinRoomAsync(roomId,userId, player);
            Console.WriteLine($"加入房间结果: {result}");
            if (result)
            {
                return Ok(new { success = true, message = "成功加入房间", roomId = roomId }); // 添加 success 字段
            }
            else
            {
                return BadRequest(new { success = false, message = "加入房间失败" }); // 添加 success 字段
            }
        }
            catch (Exception ex)
            {
                Console.WriteLine($"加入房间时发生异常: {ex.Message}");
                return StatusCode(500, new { success = false, message = "服务器错误" }); // 添加 success 字段
            }
        }
        // ... 其他 Action 方法 (根据需要调整路由或返回结构) ...

        // 获取房间详情
        // 完整路由: GET /rooms/details/{roomId}
        [HttpGet("details/{roomId}")]
        public async Task<IActionResult> GetRoomDetails(int roomId)
        {
            var room = await _gameRoomService.GetRoomDetailsAsync(roomId);
            if (room == null)
            {
                return NotFound(new { success = false, message = "房间不存在" }); // 添加 success 字段
            }
            return Ok(new { success = true, room = room }); // 添加 success 字段，并返回房间对象
        }

        // 新增的通过字符串 RoomId 获取房间详情的 API 端点
        // 完整路由: GET /api/rooms/details/by-string-id/{roomIdString}
        [HttpGet("details/by-string-id/{roomIdString}")]
        public async Task<IActionResult> GetRoomDetailsByStringId(string roomIdString)
        {
            Console.WriteLine($"接收到获取房间详情的请求，房间 ID 字符串: {roomIdString}");
            var room = await _gameRoomService.GetRoomDetailsByRoomIdStringAsync(roomIdString);
            if (room == null)
            {
                return NotFound(new { success = false, message = "房间不存在 (ID: " + roomIdString + ")" });
            }
            return Ok(new { success = true, room = room });
        }

        // 新增的 API 端点，用于处理玩家离开或房主解散房间
        // 完整路由: DELETE /rooms/exit/{roomIdString}/{userId}
        [HttpDelete("exit/{roomIdString}/{userId}")]
        public async Task<IActionResult> ExitRoom(string roomIdString, int userId)
        {
            if (string.IsNullOrWhiteSpace(roomIdString))
            {
                return BadRequest(new { success = false, message = "房间ID不能为空。" });
            }
            if (userId <= 0)
            {
                return BadRequest(new { success = false, message = "无效的用户ID。" });
            }

            var result = await _gameRoomService.HandlePlayerLeavingAsync(roomIdString, userId);

            if (!result.Success)
            {
                // 根据具体错误消息决定返回的状态码
                if (result.Message?.Contains("不存在") == true) // 简单判断
                {
                    return NotFound(new { success = false, message = result.Message, roomDisbanded = result.RoomDisbanded });
                }
                return BadRequest(new { success = false, message = result.Message, roomDisbanded = result.RoomDisbanded });
            }

            return Ok(new { success = true, message = result.Message, roomDisbanded = result.RoomDisbanded });
        }
        // 新增的 API 端点，用于开始游戏
        // API 端点: POST /rooms/start-game/{roomIdString}
        [HttpPost("start-game/{roomIdString}")]
        public async Task<IActionResult> StartRoomGame(string roomIdString, [FromBody] StartGameRequestDto request)
        {
            // 1. 验证输入参数
            if (string.IsNullOrWhiteSpace(roomIdString))
            {
                return BadRequest(new { success = false, message = "房间ID (roomIdString) 不能为空。" });
            }
            if (request == null )
            {
                // 注意：这里的 UserId 是从请求体中获取的，代表发起操作的用户
                return BadRequest(new { success = false, message = "请求体中的用户信息 (UserId) 无效或缺失。" });
            }

            // 2. 调用服务层方法
            var result = await _gameRoomService.StartGameByRoomIdStringAsync(roomIdString, request.UserId);

            // 调用后端计时器，开始计时
            

            // 3. 根据服务层返回的结果构造 HTTP 响应
            if (result.Success)
            {
                // 游戏成功开始
                return Ok(new { success = true, message = result.Message });
            }
            else
            {
                // 处理服务层返回的失败情况
                // 可以根据 result.Message 中的特定关键词来返回更具体的 HTTP 状态码
                if (result.Message != null && result.Message.Contains("不存在"))
                {
                    return NotFound(new { success = false, message = result.Message }); // 404 Not Found
                }
                if (result.Message != null && (result.Message.Contains("权限") || result.Message.Contains("房主")))
                {
                    // 如果是权限问题，返回 403 Forbidden
                    return StatusCode(StatusCodes.Status403Forbidden, new { success = false, message = result.Message });
                }
                // 其他业务逻辑错误（例如，状态不对，人数不足）通常返回 400 Bad Request
                return BadRequest(new { success = false, message = result.Message });
            }
        }

        // // 开始游戏
        // // 完整路由: POST /rooms/start/{roomId}
        // [HttpPost("start/{roomId}")]
        // public async Task<IActionResult> StartGame(int roomId)
        // {
        //     var result = await _gameRoomService.StartGameAsync(roomId);
        //     if (result)
        //     {
        //         return Ok(new { success = true, message = "游戏已开始" });
        //     }
        //     else
        //     {
        //         return BadRequest(new { success = false, message = "游戏开始失败" });
        //     }
        // }

        // 获取游戏状态
        // 完整路由: GET /rooms/status/{roomId}
        [HttpGet("status/{roomId}")]
        public async Task<IActionResult> GetGameStatus(int roomId)
        {
            var room = await _gameRoomService.GetRoomDetailsAsync(roomId);
            if (room == null)
            {
                return NotFound(new { success = false, message = "游戏状态不可用" });
            }
            return Ok(new { success = true, status = room.Status }); // 返回状态字段
        }

        // 结束当前游戏并获取结果
        // 完整路由: POST /rooms/end/{roomId}
        [HttpPost("end/{roomId}")]
        public async Task<IActionResult> EndGame(int roomId)
        {
            var result = await _gameRoomService.EndGameAsync(roomId);
            if (result)
            {
                return Ok(new { success = true, message = "游戏结束" });
            }
            else
            {
                return BadRequest(new { success = false, message = "游戏结束失败" });
            }
        }

        // 设置房间私密状态
        // 完整路由: POST /rooms/set-private/{roomId}
        [HttpPost("set-private/{roomId}")]
        public async Task<IActionResult> SetPrivateStatus(int roomId, [FromBody] bool isPrivate)
        {
            var result = await _gameRoomService.SetPrivateStatusAsync(roomId, isPrivate);
            if (result)
            {
                return Ok(new { success = true, message = "房间状态已更新" });
            }
            return BadRequest(new { success = false, message = "更新房间状态失败" });
        }

        // 设置房间密码
        // 完整路由: POST /rooms/set-password/{roomId}
        [HttpPost("set-password/{roomId}")]
        public async Task<IActionResult> SetRoomPassword(int roomId, [FromBody] string password)
        {
            var result = await _gameRoomService.SetRoomPasswordAsync(roomId, password);
            if (result)
            {
                return Ok(new { success = true, message = "房间密码已更新" });
            }
            return BadRequest(new { success = false, message = "更新房间密码失败" });
        }

        // 设置房间最大人数
        // 完整路由: POST /rooms/set-max-players/{roomId}
        [HttpPost("set-max-players/{roomId}")]
        public async Task<IActionResult> SetMaxPlayers(int roomId, [FromBody] int maxPlayers)
        {
            var result = await _gameRoomService.SetMaxPlayersAsync(roomId, maxPlayers);
            if (result)
            {
                return Ok(new { success = true, message = "房间最大人数已更新" });
            }
            return BadRequest(new { success = false, message = "更新房间最大人数失败" });
        }
        // 这个 DTO 用于从请求体中接收开始游戏操作所需的数据，这里我们只需要 UserId
        public class StartGameRequestDto 
        {
            public int UserId { get; set; }
        }
    }
}
