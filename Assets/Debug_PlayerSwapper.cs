using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debug_PlayerSwapper : MonoBehaviour {

	List<Transform> playerPanels;

	// Use this for initialization
	void Start () {
		playerPanels = new List<Transform>();
		foreach (Transform child in transform) {
			playerPanels.Add(child);
		}

		ActivatePlayer(0);
	}

	void ActivatePlayer(int targetIndex) {

		for (int i = 0; i < playerPanels.Count; i++) {
			if (i == targetIndex) {
				playerPanels[i].gameObject.SetActive(true);
			} else {
				playerPanels[i].gameObject.SetActive(false);
			}
		}
	}

	// Update is called once per frame
	void Update()
	{

		if (Input.GetKeyUp(KeyCode.Alpha1)) {
			ActivatePlayer(0);
		} else if (Input.GetKeyUp(KeyCode.Alpha2)) {
			ActivatePlayer(1);
		} else if (Input.GetKeyUp(KeyCode.Alpha3)) {
			ActivatePlayer(2);
		} else if (Input.GetKeyUp(KeyCode.Alpha4)) {
			ActivatePlayer(3);
		} else if (Input.GetKeyUp(KeyCode.Alpha5)) {
			ActivatePlayer(4);
		} else if (Input.GetKeyUp(KeyCode.Alpha6)) {
			ActivatePlayer(5);
		} else if (Input.GetKeyUp(KeyCode.Alpha7)) {
			ActivatePlayer(6);
		} else if (Input.GetKeyUp(KeyCode.Alpha8)) {
			ActivatePlayer(7);
		} else if (Input.GetKeyUp(KeyCode.Alpha9)) {
			ActivatePlayer(8);
		} else if (Input.GetKeyUp(KeyCode.Alpha0)) {
			ActivatePlayer(9);
		}
	}
}
