using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class VirtualServer : MonoBehaviour {

	//State
	private int nextLocationId = 0;
	public Dictionary<int, EditorConnector> connectorsByClientId = new Dictionary<int, EditorConnector>();
	List<int> clientIds = new List<int>();
	List<string> clientNames = new List<string>();
	
	public void HandleClientNewUser(EditorConnector connector, string name) {
//		Debug.Log("Server received new user");
		//Send players updated payload
		int newLocationId = nextLocationId;
		nextLocationId++;

		connectorsByClientId.Add(newLocationId, connector);

		clientIds.Add(newLocationId);
		clientNames.Add(name);

		Debug.Log("Entering loop, count: " + connectorsByClientId.Count);
		foreach(int clientId in clientIds.ToArray()) {
//			Debug.Log("clientId key, self: " + kp.Key + ", " + selfClientId);
			if(clientId == newLocationId) { //Send welcome payload only to new player
				Debug.Log("Sending welcome basket");
				connector.HandlePayloadReceived(new WelcomeBasketPayload(newLocationId, clientNames, clientIds));
			} else {
//				Debug.Log("Sending update other, connector client id:" + selfClientId);

				connector.HandlePayloadReceived(new UpdateOtherPayload(newLocationId, clientNames, clientIds));
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
