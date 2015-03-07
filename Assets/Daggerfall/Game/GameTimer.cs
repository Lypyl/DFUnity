using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Utility;
using Daggerfall.Game.Events;

public class GameTimer {

    DaggerfallDateTime startedAt;
    WorldTime worldTime;
    public string id;
    public float durationSeconds;
    public List<string> unknownInformation;

    private bool running;
    private bool complete;

    private GameObject _notifyGO = null;
    private string _notifyGOMethodName = "";

    public GameTimer(WorldTime DFWorldTime) { 
        unknownInformation = new List<string>();
        running = false;
        complete = false;
        worldTime = DFWorldTime;
    }

    public void registerForCompleteEvent(GameObject gameObject, string methodName) { 
        _notifyGO = gameObject;
        _notifyGOMethodName = methodName;
    }

    public string dumpTimer() {
        string outputString = "";

        outputString += "Timer (" + id + "). Duration: " + durationSeconds + " seconds.\n";
        outputString += "        running: " + (running ? "Yes" : "No") + "\n";
        outputString += "       complete: " + (complete ? "Yes" : "No") + "\n";

        foreach (string information in unknownInformation) {
            outputString += "   unknown flag: " + information + "\n";
        }

        if (running) { 
            outputString += "   Started at: " + startedAt.LongDateTimeString() + "\n";
        }
        outputString += "   World Time is: " + worldTime.Now.LongDateTimeString() + "\n";
        outputString += "   Time remaining: " + getTimeRemaining() + "s\n";

        return outputString;
    }

    public bool isRunning() {
        return running;
    }

    /**
     * Starts the timer if it's not already running AND if the timer isn't already complete
     * @returns true if the timer was started, false otherwise
     **/
    public bool start() {
        if (complete) return false;

        startedAt = new DaggerfallDateTime();
        startedAt.FromSeconds(worldTime.Now.ToSeconds());

        running = true;
        return true;
    }

    public void updateTimer() {
        // TODO: Check isRunning, or run logic no matter what? 
        if(getTimeRemaining() <= 0) {
            stopAndSetComplete(); 
        }

    }

    /**
     * Stops the timer if it's running
     * @returns true if it was stopped, false otherwise
     **/
    public bool stop() {
        if (!running) return false;

        running = false;
        return true;
    }


    private void stopAndSetComplete() {
        stop();
        complete = true;
        if(_notifyGO != null && _notifyGOMethodName != "") {
            _notifyGO.SendMessage(_notifyGOMethodName, new GameTimerEvent(this, GameTimerEventType.GAME_TIMER_COMPLETE));
        }
    }

    public bool isComplete() {
        return complete;
    }

    public long getTimeRemaining() {
        if(complete) return 0;
        return ((long)startedAt.ToSeconds() + (long)durationSeconds) - (long)worldTime.Now.ToSeconds();
    }
}
