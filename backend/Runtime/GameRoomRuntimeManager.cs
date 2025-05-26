using System.Collections.Concurrent;
using backend.Models;

namespace backend.Runtime
{
    public class GameRoomRuntimeManager
    {
        private readonly ConcurrentDictionary<string, ActiveGameState> _roomStates;

        public GameRoomRuntimeManager()
        {
            _roomStates = new ConcurrentDictionary<string, ActiveGameState>();
        }

        public bool TryGetState(string roomId, out ActiveGameState state)
        {
            return _roomStates.TryGetValue(roomId, out state);
        }

        public ActiveGameState GetOrCreateState(string roomId, Func<ActiveGameState> factory)
        {
            return _roomStates.GetOrAdd(roomId, _ => factory());
        }

        public bool RemoveState(string roomId)
        {
            return _roomStates.TryRemove(roomId, out _);
        }
    }
}
