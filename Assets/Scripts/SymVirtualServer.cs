using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class SymVirtualServer : MonoBehaviour {

	private static SymVirtualServer _instance;
	public static SymVirtualServer instance
	{
		get
		{
			if (_instance == null) {
				_instance = GameObject.FindObjectOfType<SymVirtualServer>();
			}
			return _instance;
		}
	}

	//State
	private int nextUserIdInt = 0;
	public Dictionary<string, SymClient> clientsByUserId = new Dictionary<string, SymClient>();
	Dictionary<string, string> playerNamesByUserId = new Dictionary<string, string>();
	
	public void HandleClientNewUser(SymClient client, string name) {
//		Debug.Log("Server received new user");
		//Send players updated payload
		string newUserId = nextUserIdInt.ToString();
		nextUserIdInt++;

		clientsByUserId.Add(newUserId, client);

		playerNamesByUserId.Add(newUserId, name);

		Debug.LogError ("broke it.");
//		foreach(KeyValuePair<string, string> kp in playerNamesByUserId) {
//			if(kp.Key == newUserId) { //Send welcome payload only to new player
//				client.HandleRemotePayload(new WelcomeBasketPayload(newUserId, playerNamesByUserId));
//			} else {
//				clientsByUserId[kp.Key].HandleRemotePayload(new UpdateOtherPayload(newUserId, playerNamesByUserId));
//			}
//		}
	}

	public void HandleClientSendEvent(RemotePayload payload) {
		if(payload is StartGamePayload) {
		}
		Debug.LogError ("Broke it.");
		//Echo event to all players
//		foreach(KeyValuePair<string, SymClient> kp in clientsByUserId) {
//			kp.Value.HandleRemotePayload(payload);
//		}
	}

	public void Disconnect(SymClient client) {
		string userId = clientsByUserId.Single (kp => kp.Value == client).Key;
		clientsByUserId.Remove (userId);
		playerNamesByUserId.Remove(userId);
	}
}
