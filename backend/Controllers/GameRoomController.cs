// E:\m_Documents\Project\Vous-peignez-je-suppose\backend\Controllers\GameRoomController.cs

using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using backend.Models; // 确保 GameRoom 模型在这里
using backend.Services; // 确保 GameRoomService 在这里

namespace backend.Controllers
{
    // **修改控制器级别的路由**
    // 将 [Route("api/[controller]")] 改为 [Route("rooms")]
    [Route("rooms")] // <-- 修改这里
    [ApiController]
    public class GameRoomController : ControllerBase
    {
        private readonly GameRoomService _gameRoomService;

        public GameRoomController(GameRoomService gameRoomService)
        {
            _gameRoomService = gameRoomService;
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
            // ... (代码保持不变) ...
             if (newRoom == null || string.IsNullOrEmpty(newRoom.Name))
            {
                // 返回 BadRequest，并包含前端期望的 message 字段
                return BadRequest(new { success = false, message = "房间信息不完整" });
            }

            var result = await _gameRoomService.CreateRoomAsync(newRoom);
            if (result != null)
            {
                // 返回 Ok，并包含前端期望的 success 和 roomId 字段
                return Ok(new { success = true, roomId = result.Id, message = "房间创建成功" }); // 添加 success 字段
            }
            else
            {
                 // 返回 BadRequest，并包含前端期望的 success 和 message 字段
                return BadRequest(new { success = false, message = "房间创建失败" }); // 添加 success 字段
            }
        }

        // 加入一个已有的游戏房间
        // 完整路由: POST /rooms/join/{roomId}
        [HttpPost("join/{roomId}")]
        public async Task<IActionResult> JoinRoom(int roomId, [FromBody] Player player)
        {
            // ... (代码保持不变) ...
            if (player == null || string.IsNullOrEmpty(player.Username))
            {
                 return BadRequest(new { success = false, message = "玩家信息不完整" });
            }

            var result = await _gameRoomService.JoinRoomAsync(roomId, player);
            if (result)
            {
                return Ok(new { success = true, message = "成功加入房间", roomId = roomId }); // 添加 success 字段
            }
            else
            {
                return BadRequest(new { success = false, message = "加入房间失败" }); // 添加 success 字段
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

         // 开始游戏
        // 完整路由: POST /rooms/start/{roomId}
        [HttpPost("start/{roomId}")]
        public async Task<IActionResult> StartGame(int roomId)
        {
            var result = await _gameRoomService.StartGameAsync(roomId);
            if (result)
            {
                return Ok(new { success = true, message = "游戏已开始" });
            }
            else
            {
                return BadRequest(new { success = false, message = "游戏开始失败" });
            }
        }

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
    }
}
