using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomLog : MonoBehaviour {

	List<string> log = new List<string>();

	private Text console;
	private int MESSAGES_TO_DISPLAY = 5;

	// Use this for initialization
	void Start () {
		console = GetComponent<Text> ();
		console.text = "";
		Application.logMessageReceived += HandleLog;
	}

	void HandleLog(string message, string stackTrace, LogType type) {
		log.Insert (0, message);
		UpdateConsole ();
	}

	void UpdateConsole() {
		string consoleText = "";
		for(int i = 0; i < log.Count && i < MESSAGES_TO_DISPLAY; i++) {
			if (i != 0) {
				consoleText += "\n";
			}
			consoleText += log [i];
		}
		console.text = consoleText;
	}
}
