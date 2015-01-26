using UnityEngine;
using System.Collections;
using DaggerfallWorkshop.Demo;
using DaggerfallWorkshop.Game;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {
    private bool _isUIOpen = false;
    public bool isUIOpen { 
        get { return _isUIOpen; }
    }

    public Canvas MenuOptions1;
    public Canvas DevConsole;
    public Canvas PlayerSheet;
    public Camera playerCamera; // necessary to change PlayerMouseLook behavior

	// Use this for initialization
	void Start () {
        MenuOptions1.enabled = false;
        DevConsole.enabled = false;
        PlayerSheet.enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
        // This whole area will need game logic for what can be opened when
	    if (Input.GetKeyDown(KeyCode.Alpha1)) {
            MenuOptions1.enabled = !MenuOptions1.enabled;
            _isUIOpen = MenuOptions1.enabled;
            //playerCamera.GetComponent<PlayerMouseLook>().enableMLook = !MenuOptions1.enabled;
        } else if (Input.GetButtonDown("DevConsole")) { 
            if (!_isUIOpen) {
                _isUIOpen = true;
                DevConsole.enabled = true;
                DevConsole devConsole = DevConsole.GetComponent<DevConsole>();
                devConsole.enabled = true;
                devConsole.inputField.enabled = true;
                devConsole.inputField.Select();
            } else {
                _isUIOpen = false;
                DevConsole.enabled = false;
                DevConsole devConsole = DevConsole.GetComponent<DevConsole>();
                devConsole.enabled = false;
                devConsole.inputField.enabled = false;
            }
        } else if (Input.GetButtonDown("PlayerSheet")) { 
            PlayerSheet.enabled = !PlayerSheet.enabled;
            _isUIOpen = PlayerSheet.enabled; 
        }
	}


    void devConsole_displayText(string line) {
        DevConsole.GetComponent<DevConsole>().displayText(line);
    }
}
