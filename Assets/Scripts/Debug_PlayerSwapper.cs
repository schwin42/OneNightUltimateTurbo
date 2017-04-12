using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debug_PlayerSwapper : MonoBehaviour {

	public List<Transform> playerPanels;
	public List<PlayerUi> playerUis;
	public List<SymClient> clients;

	public List<SymClient> pendingPlayers;

	bool playersJoined = false;
	bool playersConnected = false;

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
			client.Start();
//			client.JoinSession();
			i++;
		}

//		clients[0].OnEnteredRoom += HandleSessionStarted;
//		clients[0].OnUserConnected += HandleUserConnected;

//		clients[0].BeginSession("0");


//		clients[0].JoinSession(

//		clients[0].BeginGame();

	}

	void ActivatePlayer(int targetIndex) {

		playerPanels[targetIndex].SetAsLastSibling();
	}

	// Update is called once per frame
	void Update()
	{

		if (Input.GetKeyUp(KeyCode.Q)) {
			ActivatePlayer(0);
		} else if (Input.GetKeyUp(KeyCode.W)) {
			ActivatePlayer(1);
		} else if (Input.GetKeyUp(KeyCode.E)) {
			ActivatePlayer(2);
		} else if (Input.GetKeyUp(KeyCode.R)) {
			ActivatePlayer(3);
		} else if (Input.GetKeyUp(KeyCode.T)) {
			ActivatePlayer(4);
		} else if (Input.GetKeyUp(KeyCode.Y)) {
			ActivatePlayer(5);
		} else if (Input.GetKeyUp(KeyCode.U)) {
			ActivatePlayer(6);
		} else if (Input.GetKeyUp(KeyCode.I)) {
			ActivatePlayer(7);
		} else if (Input.GetKeyUp(KeyCode.O)) {
			ActivatePlayer(8);
		} else if (Input.GetKeyUp(KeyCode.P)) {
			ActivatePlayer(9);
		}
	}

	void HandleSessionStarted(SymClient client) {
		pendingPlayers = new List<SymClient>();
		for(int i = 1; i < clients.Count; i++) {
			clients[i].OnEnteredRoom += HandleSessionJoined;
			clients[i].OnUserConnected += HandleUserConnected;
			clients[i].JoinSession(i.ToString(), clients[0].roomKey);
			pendingPlayers.Add(clients[i]);
		}

	}

	void HandleSessionJoined(SymClient client) {

//		pendingPlayers.Remove(client);
//
//		if(pendingPlayers.Count == 0) {
//			playersJoined = true;
//		}
//
//		if(playersJoined && playersConnected) {
//			clients[0].InitiateGame();
//		}
	}

	void HandleUserConnected(string userId) {
//		foreach(SymClient client in clients) {
//			if(client.connectedUsers.Count < clients.Count) {
//				return;
//			}
//		}
//		playersConnected = true;
//
//
//		if(playersJoined && playersConnected) {
//			clients[0].InitiateGame();
//		}
	}
}
