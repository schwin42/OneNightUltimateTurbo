using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class VirtualServer {

	//State
	private int nextLocationId = 0;
	public Dictionary<int, int> connectorsByClientId = new Dictionary<int, EditorConnector>();
	Dictionary<int, string> playerNamesByClientId = new Dictionary<int, string>();
	
	public void HandleClientNewUser(EditorConnector connector, string name) {
//		Debug.Log("Server received new user");
		//Send players updated payload
		int newLocationId = nextLocationId;
		nextLocationId++;

		connectorsByClientId.Add(newLocationId, connector);
		playerNamesByClientId.Add(newLocationId, name);

		Debug.Log("Entering loop, count: " + connectorsByClientId.Count);
		foreach(KeyValuePair<int, EditorConnector> kp in connectorsByClientId.ToArray()) {
			Debug.Log("clientId key, self: " + kp.Key + ", " + connector.selfClientId);
			if(kp.Key == newLocationId) { //Send welcome payload only to new player
				Debug.Log("Sending welcome basket");
				connector.HandlePayloadReceived(new WelcomeBasketPayload(newLocationId, playerNamesByClientId));
			} else {
				Debug.Log("Sending update other, connector client id:" + connector.selfClientId);

				connector.HandlePayloadReceived(new UpdateOtherPayload(newLocationId, playerNamesByClientId));
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
