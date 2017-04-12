using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomLog : MonoBehaviour {

	List<string> log = new List<string>();

	public const float alpha = 0.2f;

	private Text console;
	private int MESSAGES_TO_DISPLAY = 5;

	private Image backer;

	// Use this for initialization
	void Start () {
		console = GetComponent<Text> ();
		console.text = "";
		Application.logMessageReceived += HandleLog;

		backer = transform.parent.Find("Image").GetComponent<Image>();
		backer.color = new Color(0, 1, 0, alpha);

	}

	void HandleLog(string message, string stackTrace, LogType type) {
		log.Insert (0, message);
		Debug.Log("log type: " + type.ToString());
		if (type == LogType.Error || type == LogType.Exception) {
			backer.color = new Color (1, 0, 0, alpha);
		} else if (type == LogType.Warning) {
			Color yellow = Color.yellow;
			backer.color = new Color(yellow.r, yellow.g, yellow.b, alpha);
		}
		UpdateConsole (type);
	}

	void UpdateConsole(LogType type) {
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
