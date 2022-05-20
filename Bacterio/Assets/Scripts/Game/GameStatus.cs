using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bacterio.Game
{
    public sealed class GameStatus
    {
        public static event Action<int> ActiveWoundCountChanged;
        public static event Action<long> ElapsedTimeChanged;
        public static event Action<GameEndResult> GameEndResultChanged;

        private int _activeWoundCount;
        public int ActiveWoundCount { get { return _activeWoundCount; } set { _activeWoundCount = value; ActiveWoundCountChanged?.Invoke(_activeWoundCount); } }

        private long _elapsedTimeMs;
        public long ElapsedTimeMs { get { return _elapsedTimeMs; } set { _elapsedTimeMs = value; ElapsedTimeChanged?.Invoke(_elapsedTimeMs); } }

        private GameEndResult _endResult = GameEndResult.Invalid;
        public GameEndResult EndResult { get { return _endResult; } set { _endResult = value; GameEndResultChanged?.Invoke(_endResult); } }
    }
}
