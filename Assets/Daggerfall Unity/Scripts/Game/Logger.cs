using UnityEngine;
using System.Collections;
using DaggerfallConnect.Utility;
using DaggerfallWorkshop.Game;
using System.IO;

public class Logger {

    //public static Logger instance { get; private set; }
    private static Logger instance;
    public static Logger GetInstance() { 
        if (instance == null) {
            instance = new Logger();
        }
        return instance;
    }

    public bool outputToDevConsole = true;
    public bool outputToLogFile = true;
    public bool aggressiveFlushing = true; // flushes to disk after every write to the output buffer
    private const string logFilePath = "dfunity.log";

    private FileProxy managedFile = new FileProxy();
    private StreamWriter writer;
    [System.NonSerialized] // Necessary to prevent errors when switching from edit mode to running in the Unity editor
    private GameObject uiOwner; 

    ~Logger() {
        writer.Close();
    }

    public void log(string line) {
        if (uiOwner != null && outputToDevConsole) {
            uiOwner.SendMessage("devConsole_displayText", line);
        }

        if (writer != null) { 
            writer.WriteLine(System.DateTime.UtcNow + " *** " + line);
            if (aggressiveFlushing) {
                writer.Flush();
            }
        }
    }

	public bool Setup() { 
        if (!managedFile.Load(logFilePath, DaggerfallConnect.FileUsage.AppendToDisk, false, true)) {
            //DevConsole.displayText("Failed to set up logging with file: " + logFilePath);

            return false;
        }
        uiOwner = GameObject.FindGameObjectWithTag("UIOwner");
        writer = managedFile.GetStreamWriter();
        log("Logging started");
        return true;
    }

}
