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
	public Dictionary<string, OnumClient> clientsByUserId = new Dictionary<string, OnumClient>();
	
	public void HandleClientNewUser(OnumClient client, string playerName) {
		//Send players updated payload
		string newUserId = playerName + ":" + Random.Range(0, 10000);

		clientsByUserId.Add(newUserId, client);

		foreach(KeyValuePair<string, OnumClient> kp in clientsByUserId) {
			if(kp.Key == newUserId) { //Send welcome payload only to new player
				client.HandleJoinedSession(newUserId, null, clientsByUserId.Select(kvp => kvp.Key).ToList());
			} else {
				clientsByUserId [kp.Key].HandleOtherJoined (clientsByUserId.Select (kvp => kvp.Key).ToArray());
			}
		}
	}

	public void HandleClientSendEvent(RemotePayload payload) {
		if (payload is StartGamePayload) {
			foreach (KeyValuePair<string, OnumClient> kvp in clientsByUserId) {
				kvp.Value.HandleStartGamePayload (((StartGamePayload)payload).randomSeed);
			}
		} else if (payload is ActionPayload) {
			foreach (KeyValuePair<string, OnumClient> kvp in clientsByUserId) {
				kvp.Value.HandleActionMessage (((ActionPayload)payload).sourceUserId, ((ActionPayload)payload).selection);
			}
		} else if (payload is VotePayload) {
			foreach (KeyValuePair<string, OnumClient> kvp in clientsByUserId) {
				kvp.Value.HandleVoteMessage (((VotePayload)payload).sourceUserId, ((VotePayload)payload).voteeLocationId);
			}
		}
	}

	public void Disconnect(OnumClient client) {
		string userId = clientsByUserId.Single (kp => kp.Value == client).Key;
		clientsByUserId.Remove (userId);
	}
}
