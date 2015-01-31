using UnityEngine;
using System.Collections;

// Performs initialization unique to DFUnity
public class DFUnityInit : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Logger.GetInstance().Setup();	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
