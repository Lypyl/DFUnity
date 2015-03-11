using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Daggerfall.Game.Events {
    public enum GameTimerEventType { GAME_TIMER_COMPLETE };

    public class GameTimerEvent {
        public GameTimer gameTimer;
        public GameTimerEventType eventType;

        public GameTimerEvent(GameTimer gameTimer, GameTimerEventType gameTimerEventType) {
            this.gameTimer = gameTimer;
            this.eventType = gameTimerEventType;
        }
    }
}
