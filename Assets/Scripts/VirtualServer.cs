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
	private int nextLocationId = 0;
	public Dictionary<int, EditorConnector> connectorsByClientId = new Dictionary<int, EditorConnector>();
	List<int> clientIds = new List<int>();
	List<string> clientNames = new List<string>();
	
	public void HandleClientNewUser(EditorConnector newConnector, string name) {
//		Debug.Log("Server received new user");
		//Send players updated payload
		int newLocationId = nextLocationId;
		nextLocationId++;

		connectorsByClientId.Add(newLocationId, newConnector);

		clientIds.Add(newLocationId);
		clientNames.Add(name);
		
		foreach(int clientId in clientIds.ToArray()) {
//			Debug.Log("clientId key, self: " + kp.Key + ", " + selfClientId);
			if(clientId == newLocationId) { //Send welcome payload only to new player
				Debug.Log("Sending welcome basket");
				newConnector.HandlePayloadReceived(new WelcomeBasketPayload(newLocationId, clientNames, clientIds));
			} else {
//				Debug.Log("Sending update other, connector client id:" + selfClientId);

				connectorsByClientId[clientId].HandlePayloadReceived(new UpdateOtherPayload(newLocationId, clientNames, clientIds));
			}
		}


	}

	public void HandleClientSendEvent(RemotePayload payload) {
		//Echo event to all players
		foreach(KeyValuePair<int, EditorConnector> kp in connectorsByClientId) {
			kp.Value.HandlePayloadReceived(payload);
		}
	}
}
