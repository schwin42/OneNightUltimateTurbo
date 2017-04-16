using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class VirtualServer : MonoBehaviour {

	private static VirtualServer _instance;
	public static VirtualServer instance
	{
		get
		{
			if (_instance == null) {
				_instance = GameObject.FindObjectOfType<VirtualServer>();
			}
			return _instance;
		}
	}

	//State
	public Dictionary<string, OnutClient> clientsByUserId = new Dictionary<string, OnutClient>();
	
	public void HandleClientNewUser(OnutClient client, string playerName) {
		//Send players updated payload
		string newUserId = playerName + ":" + Random.Range(0, 10000);

		clientsByUserId.Add(newUserId, client);

		foreach(KeyValuePair<string, OnutClient> kp in clientsByUserId) {
			if(kp.Key == newUserId) { //Send welcome payload only to new player
				client.HandleJoinedSession(newUserId, null, clientsByUserId.Select(kvp => kvp.Key).ToList());
			} else {
				clientsByUserId [kp.Key].HandleOtherJoined (clientsByUserId.Select (kvp => kvp.Key).ToArray());
			}
		}
	}

	public void HandleClientSendEvent(RemotePayload payload) {
		if (payload is StartGamePayload) {
			foreach (KeyValuePair<string, OnutClient> kvp in clientsByUserId) {
				kvp.Value.HandleGameStarted (((StartGamePayload)payload).randomSeed);
			}
		} else if (payload is ActionPayload) {
			foreach (KeyValuePair<string, OnutClient> kvp in clientsByUserId) {
				kvp.Value.HandleActionMessage (((ActionPayload)payload).sourceUserId, ((ActionPayload)payload).selection);
			}
		} else if (payload is VotePayload) {
			foreach (KeyValuePair<string, OnutClient> kvp in clientsByUserId) {
				kvp.Value.HandleVoteMessage (((VotePayload)payload).sourceUserId, ((VotePayload)payload).voteeLocationId);
			}
		}
	}

	public void Disconnect(OnutClient client) {
		string userId = clientsByUserId.Single (kp => kp.Value == client).Key;
		clientsByUserId.Remove (userId);
	}
}
