using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using backend.Models;
using backend.Services;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameRoomController : ControllerBase
    {
        private readonly GameRoomService _gameRoomService;

        public GameRoomController(GameRoomService gameRoomService)
        {
            _gameRoomService = gameRoomService;
        }

        // 获取所有游戏房间
        [HttpGet("list")]
        public async Task<IActionResult> GetRoomList()
        {
            var rooms = await _gameRoomService.GetAllRoomsAsync();
            return Ok(rooms);
        }

        // 创建一个新的游戏房间
        [HttpPost("create")]
        public async Task<IActionResult> CreateRoom([FromBody] GameRoom newRoom)
        {
            if (newRoom == null || string.IsNullOrEmpty(newRoom.Name))
            {
                return BadRequest("房间信息不完整");
            }

            var result = await _gameRoomService.CreateRoomAsync(newRoom);
            if (result != null)
            {
                return Ok(new { Message = "房间创建成功", RoomId = result.Id });
            }
            else
            {
                return BadRequest(new { Message = "房间创建失败" });
            }
        }

        // 加入一个已有的游戏房间
        [HttpPost("join/{roomId}")]
        public async Task<IActionResult> JoinRoom(int roomId, [FromBody] Player player)
        {
            if (player == null || string.IsNullOrEmpty(player.Username))
            {
                return BadRequest("玩家信息不完整");
            }

            var result = await _gameRoomService.JoinRoomAsync(roomId, player);
            if (result)
            {
                return Ok(new { Message = "成功加入房间", RoomId = roomId });
            }
            else
            {
                return BadRequest(new { Message = "加入房间失败" });
            }
        }

        // 获取房间详情
        [HttpGet("details/{roomId}")]
        public async Task<IActionResult> GetRoomDetails(int roomId)
        {
            var room = await _gameRoomService.GetRoomDetailsAsync(roomId);
            if (room == null)
            {
                return NotFound(new { Message = "房间不存在" });
            }
            return Ok(room);
        }

        // 开始游戏
        [HttpPost("start/{roomId}")]
        public async Task<IActionResult> StartGame(int roomId)
        {
            var result = await _gameRoomService.StartGameAsync(roomId);
            if (result)
            {
                return Ok(new { Message = "游戏已开始" });
            }
            else
            {
                return BadRequest(new { Message = "游戏开始失败" });
            }
        }

        // 获取游戏状态
        [HttpGet("status/{roomId}")]
        public async Task<IActionResult> GetGameStatus(int roomId)
        {
            var room = await _gameRoomService.GetRoomDetailsAsync(roomId);
            if (room == null)
            {
                return NotFound(new { Message = "游戏状态不可用" });
            }
            return Ok(new { Status = room.Status });
        }

        // 结束当前游戏并获取结果
        [HttpPost("end/{roomId}")]
        public async Task<IActionResult> EndGame(int roomId)
        {
            var result = await _gameRoomService.EndGameAsync(roomId);
            if (result)
            {
                return Ok(new { Message = "游戏结束" });
            }
            else
            {
                return BadRequest(new { Message = "游戏结束失败" });
            }
        }
    }
}
