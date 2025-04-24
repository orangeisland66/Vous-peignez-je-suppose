using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using you_draw_i_guess_project.Models;
using you_draw_i_guess_project.Services;

namespace you_draw_i_guess_project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private readonly GameService _gameService;
        private readonly GameRoomService _gameRoomService;

        public GameController(GameService gameService, GameRoomService gameRoomService)
        {
            _gameService = gameService;
            _gameRoomService = gameRoomService;
        }

        // 开始游戏
        [HttpPost("start/{roomId}")]
        public async Task<IActionResult> StartGame(int roomId)
        {
            var room = await _gameRoomService.GetRoomDetailsAsync(roomId);
            if (room == null || room.Status != RoomStatus.Waiting)
            {
                return BadRequest("房间无法开始游戏，可能房间状态不正确");
            }

            var result = await _gameService.StartGameAsync(roomId);
            if (result.IsSuccess)
            {
                return Ok(new { Message = "游戏已开始", RoomId = roomId });
            }
            else
            {
                return BadRequest(new { Message = result.ErrorMessage });
            }
        }

        // 切换到下一个轮次
        [HttpPost("next-round/{roomId}")]
        public async Task<IActionResult> NextRound(int roomId)
        {
            var room = await _gameRoomService.GetRoomDetailsAsync(roomId);
            if (room == null || room.Status != RoomStatus.Playing)
            {
                return BadRequest("房间无法进入下一轮，当前游戏状态不正确");
            }

            var result = await _gameService.NextRoundAsync(roomId);
            if (result.IsSuccess)
            {
                return Ok(new { Message = "进入下一轮" });
            }
            else
            {
                return BadRequest(new { Message = result.ErrorMessage });
            }
        }

        // 获取当前游戏状态
        [HttpGet("status/{roomId}")]
        public async Task<IActionResult> GetGameStatus(int roomId)
        {
            var status = await _gameService.GetGameStatusAsync(roomId);
            if (status == null)
            {
                return NotFound(new { Message = "游戏状态不可用" });
            }
            return Ok(status);
        }

        // 玩家作画
        [HttpPost("draw/{roomId}")]
        public async Task<IActionResult> Draw(int roomId, [FromBody] Stroke stroke)
        {
            var room = await _gameRoomService.GetRoomDetailsAsync(roomId);
            if (room == null || room.Status != RoomStatus.Playing)
            {
                return BadRequest("当前房间不在进行中，无法作画");
            }

            var result = await _gameService.PlayerDrawAsync(roomId, stroke);
            if (result.IsSuccess)
            {
                return Ok(new { Message = "画作已更新" });
            }
            else
            {
                return BadRequest(new { Message = result.ErrorMessage });
            }
        }

        // 玩家猜词
        [HttpPost("guess/{roomId}")]
        public async Task<IActionResult> Guess(int roomId, [FromBody] string guessedWord)
        {
            var room = await _gameRoomService.GetRoomDetailsAsync(roomId);
            if (room == null || room.Status != RoomStatus.Playing)
            {
                return BadRequest("当前房间不在进行中，无法猜词");
            }

            var result = await _gameService.PlayerGuessAsync(roomId, guessedWord);
            if (result.IsSuccess)
            {
                return Ok(new { Message = "猜词结果已处理", Correct = result.Correct });
            }
            else
            {
                return BadRequest(new { Message = result.ErrorMessage });
            }
        }

        // 获取当前轮次的游戏结果
        [HttpGet("round-result/{roomId}")]
        public async Task<IActionResult> GetRoundResult(int roomId)
        {
            var result = await _gameService.GetRoundResultAsync(roomId);
            if (result == null)
            {
                return NotFound(new { Message = "轮次结果不可用" });
            }
            return Ok(result);
        }

        // 游戏结束，获取最终得分
        [HttpGet("final-score/{roomId}")]
        public async Task<IActionResult> GetFinalScore(int roomId)
        {
            var room = await _gameRoomService.GetRoomDetailsAsync(roomId);
            if (room == null || room.Status != RoomStatus.Completed)
            {
                return BadRequest("游戏尚未结束，无法获取最终得分");
            }

            var result = await _gameService.GetFinalScoreAsync(roomId);
            if (result == null)
            {
                return NotFound(new { Message = "最终得分不可用" });
            }

            return Ok(new { Message = "游戏结束", Scores = result });
        }
    }
}
