using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debug_PlayerSwapper : MonoBehaviour {

	public List<Transform> playerPanels;
	public List<PlayerUi> playerUis;
	public List<SymClient> clients;

	// Use this for initialization
	void Start () {
		playerPanels = new List<Transform>();
		playerUis = new List<PlayerUi>();
		clients = new List<SymClient>();
		int i = 0;
		foreach (Transform child in transform) {
			playerPanels.Add(child);
			playerPanels[i].gameObject.name = i.ToString();
			playerUis.Add(child.GetComponent<PlayerUi>());
			SymClient client = child.GetComponent<SymClient>();
			clients.Add(client);
			client.Initialize();
			client.PlayerName = i.ToString();
			client.JoinSession("");
			i++;
		}

		clients[0].BeginGame();

	}

	void ActivatePlayer(int targetIndex) {

		playerPanels[targetIndex].SetAsLastSibling();
	}

	// Update is called once per frame
	void Update()
	{

		if (Input.GetKeyUp(KeyCode.BackQuote)) {
			ActivatePlayer(0);
		} else if (Input.GetKeyUp(KeyCode.Alpha1)) {
			ActivatePlayer(1);
		} else if (Input.GetKeyUp(KeyCode.Alpha2)) {
			ActivatePlayer(2);
		} else if (Input.GetKeyUp(KeyCode.Alpha3)) {
			ActivatePlayer(3);
		} else if (Input.GetKeyUp(KeyCode.Alpha4)) {
			ActivatePlayer(4);
		} else if (Input.GetKeyUp(KeyCode.Alpha5)) {
			ActivatePlayer(5);
		} else if (Input.GetKeyUp(KeyCode.Alpha6)) {
			ActivatePlayer(6);
		} else if (Input.GetKeyUp(KeyCode.Alpha7)) {
			ActivatePlayer(7);
		} else if (Input.GetKeyUp(KeyCode.Alpha8)) {
			ActivatePlayer(8);
		} else if (Input.GetKeyUp(KeyCode.Alpha9)) {
			ActivatePlayer(9);
		}
	}
}
