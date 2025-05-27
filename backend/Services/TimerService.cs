// using System;
// using System.Collections.Generic;
// using System.Threading;
// using System.Threading.Tasks;
// using Microsoft.AspNetCore.SignalR;
// using backend.Models;
// using backend.Data;
// using System.Collections.Concurrent;
// using backend.Hubs;
// using Microsoft.EntityFrameworkCore.Diagnostics;
// using System;
// using System.Collections.Concurrent;
// using System.Threading;
// using System.Threading.Tasks;
// using Microsoft.EntityFrameworkCore;
// using backend.Models;
// using backend.Data;
// using backend.Runtime; // 假设这是你的DbContext命名空间
// //好像还有问题///////////////////////////////////////
// //////////////////////////////////////////////////////
// /////////////////////////////////////////////////////
// /// ////////////////////////////////////////////////
// namespace backend.Services
// {
//     public class TimerService
//     {
//         // 注入GameRoomRuntimeManager
//         private readonly GameRoomRuntimeManager _runtimeManager;

//         // 存储每个房间的计时器信息
//         private readonly ConcurrentDictionary<string, TimerInfo> _activeTimers;

//         // 注入依赖
//         // private readonly OurDbContext _context; // 数据库上下文
//         //private readonly GameHub _gameHub; //GameHub实例

//         private readonly IHubContext<GameHub> _hubContext;


//         public TimerService(GameRoomRuntimeManager runtimeManager, IHubContext<GameHub> hubContext)
//         {
//             _activeTimers = new ConcurrentDictionary<string, TimerInfo>();
//             // _context = context;
//             _hubContext = hubContext;
//             _runtimeManager = runtimeManager;
//         }

//         // 不知道上面在干什么
//         // 启动回合倒计时
//         public async Task StartRoundTimer(string roomId, GamePhase phase, int durationSeconds)
//         {
//             try
//             {
//                 Console.WriteLine($"启动房间{roomId}的计时器，阶段:{phase}, 时长{durationSeconds}秒");

//                 // 停止现有的计时器
//                 //await StopTimer(roomId);

//                 // // 更新数据库的RoundStartTime
//                 // await UpdateGameStateTimeStamp(roomId, DateTime.UtcNow);
//                 // if (_runtimeManager.TryGetState(roomId, out var gameState))
//                 // {
//                 //     gameState.RoundStartTime = DateTime.UtcNow;
//                 //     gameState.CurrentGamePhase = phase;
//                 // }
//                 // else
//                 // {
//                 //     Console.WriteLine($"未找到{roomId}的房间的游戏状态");
//                 // }

//                 //创建计时器信息
//                 var timerInfo = new TimerInfo
//                 {
//                     RoomId = roomId,
//                     Phase = phase,
//                     StartTime = DateTime.UtcNow,
//                     DurationSeconds = durationSeconds
//                 };

//                 // 创建并启动计时器
//                 //timerInfo.Timer = new Timer(OnTimerTick, roomId, 0, 1000); //每秒触发一次

//                 // 保存到活跃计时器字典
//                 //_activeTimers[roomId] = timerInfo;

//                 Console.WriteLine($"房间{roomId}的计时器已启动");
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"启动房间{roomId}的计时器出错:{ex.Message}");
//                 throw; //重新抛出异常
//             }
//         }

//         // 计时器的回调函数，每秒触发
//         private async void OnTimerTick(object state)
//         {
//             var roomId = (string)state;

//             try
//             {
//                 if (!_activeTimers.TryGetValue(roomId, out var timerInfo))
//                 {
//                     return;
//                 }

//                 //计算剩余时间
//                 var elapsed = (DateTime.UtcNow - timerInfo.StartTime).TotalSeconds;
//                 var remainingSeconds = Math.Max(0, timerInfo.DurationSeconds - (int)elapsed);

//                 // 更新剩余时间
//                 timerInfo.RemainingSeconds = remainingSeconds;

//                 // 向前端广播计时器更新 //检查到这个地方
//                 //await _gameHub.NotifyTimerUpdate(roomId, remainingSeconds);
//                 await _hubContext.Clients.Group(roomId).SendAsync("TimerUpdate", remainingSeconds);
//                 Console.WriteLine($"房间{roomId}计时器更新: 剩余时间 {remainingSeconds}秒");
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"计时器回调执行失败:{ex.Message}");
//             }
//         }

//         // 获取剩余时间
//         // public async Task<int> GetRemainingTime(string roomId)
//         // {
//         //     try
//         //     {
//         //         if (_activeTimers.TryGetValue(roomId, out var timerInfo))
//         //         {
//         //             var elapsed = (DateTime.UtcNow - timerInfo.StartTime).TotalSeconds;
//         //             var remainingSeconds = Math.Max(0, timerInfo.DurationSeconds - (int)elapsed);
//         //             return remainingSeconds;
//         //         }

//         //         return 0;
//         //     }
//         //     catch (Exception ex)
//         //     {
//         //         Console.WriteLine($"获取剩余时间失败:{ex.Message}");
//         //         return 0;
//         //     }
//         // }

//         //停止计时器
//         public async Task StopTimer(string roomId)
//         {
//             try
//             {
//                 if (_activeTimers.TryRemove(roomId, out var timerInfo))
//                 {
//                     timerInfo.Timer?.Dispose(); //释放计时器资源
//                     Console.WriteLine($"房间{roomId}的计时器已停止");
//                 }
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"停止计时器失败");
//             }
//         }

//         // // 更新数据库中的RoundStartTime
//         // private async Task UpdateGameStateTimeStamp(string roomId, DateTime startTime)
//         // {
//         //     try
//         //     {
//         //         // 将String类型的roomId转换成int 不知道这个有没有错
//         //         if (int.TryParse(roomId, out int gameRoomId))
//         //         {
//         //             var gameState = await _context.ActiveGameStates
//         //             .FirstOrDefaultAsync(gs => gs.GameRoomId == gameRoomId);

//         //             if (gameState != null)
//         //             {
//         //                 gameState.RoundStartTime = startTime;
//         //                 await _context.SaveChangesAsync();
//         //                 Console.WriteLine($"房间{roomId}的RoundStartTime已经更新");
//         //             }
//         //             else
//         //             {
//         //                 Console.WriteLine($"未找到房间{roomId}的游戏状态");
//         //             }
//         //         }
//         //         else
//         //         {
//         //             Console.WriteLine($"无效的房间ID格式:{roomId}");
//         //         }
//         //     }
//         //     catch (Exception ex)
//         //     {
//         //         Console.WriteLine($"更新RoundStartTime失败:{ex.Message}");
//         //     }
//         // }
//         public void Dispose()
//         {
//             foreach (var timeInfo in _activeTimers.Values)
//             {
//                 timeInfo.Timer?.Dispose();
//             }
//             _activeTimers.Clear();
//         }
//     }

//     // 计时器信息类
//     internal class TimerInfo
//     {
//         public string? RoomId { get; set; } //房间ID
//         public GamePhase Phase { get; set; } //当前游戏阶段
//         public DateTime StartTime { get; set; } //计时器开始时间
//         public int DurationSeconds { get; set; } //计时器总时长
//         public int RemainingSeconds { get; set; } //计时器剩余时间
//         public Timer? Timer { get; set; } //计时器示例
//     }
// }