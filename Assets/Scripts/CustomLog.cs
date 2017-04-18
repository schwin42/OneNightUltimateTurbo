using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CustomLog : EventTrigger {

	//Configuration
	public const float alpha = 0.2f;
	private int MESSAGES_TO_DISPLAY = 15;

	private Image backer;
	private Text console;
	private RectTransform rectTransform;
	private Vector2 startOffset;

	//Status
	private bool isExtended = false;
	List<string> log = new List<string>();

	// Use this for initialization
	void Start () {
		console = GetComponentInChildren<Text> ();
		console.text = "";
		Application.logMessageReceived += HandleLog;

		backer = transform.GetComponent<Image>();
		backer.color = new Color(0, 1, 0, alpha);

		rectTransform = (RectTransform)transform;

		startOffset = rectTransform.offsetMax;

		print ("Console initialized");
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

	public override void OnPointerClick(PointerEventData data)
	{
//		print("position: " + rectTransform.sizeDelta);
		if(isExtended) {
			rectTransform.offsetMax = startOffset;
			isExtended = false;
		} else {
			rectTransform.offsetMax = new Vector2( 0, rectTransform.offsetMax.y);

			isExtended = true;
		}
//		print("position: " + rectTransform.sizeDelta);
	}
}
