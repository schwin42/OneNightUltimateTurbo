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
			client.Start();
//			client.JoinSession();
			i++;
		}

//		clients[0].BeginSession();
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
}
