using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DaggerfallWorkshop;

public class GameTimer : MonoBehaviour {

    WorldTime startedAt;
    WorldTime worldTime;
    public string id;
    public float durationSeconds;
    public List<string> unknownInformation;

    private bool running;
    private bool complete;

    public GameTimer(WorldTime DFWorldTime) { 
        unknownInformation = new List<string>();
        running = false;
        complete = false;
        worldTime = DFWorldTime;
    }

    public string dumpTimer() {
        string outputString = "";

        outputString += "Timer (" + id + "). Duration: " + durationSeconds + " seconds.\n";
        outputString += "        running: " + (running ? "Yes" : "No") + "\n";
        outputString += "       complete: " + (complete ? "Yes" : "No") + "\n";

        foreach (string information in unknownInformation) {
            outputString += "   unknown flag: " + information + "\n";
        }

        /*
        if (running) { 
            outputString += "   Started at: " + startedAtGetDebugDateString() + "\n";
        }
        outputString += " World Time is: " + worldTime.GetDebugDateString() + "\n";
        */

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

        //startedAt = new WorldTime(worldTime);

        running = true;
        return true;
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

    public bool isComplete() {
        return complete;
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
