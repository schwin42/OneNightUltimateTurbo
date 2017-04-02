using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class VirtualServer {

	//State
	private int nextLocationId = 0;
	public Dictionary<int, RemoteConnector> connectorsByLocationId = new Dictionary<int, RemoteConnector>();
	Dictionary<int, string> playerNamesByClientId = new Dictionary<int, string>();
	
	public void HandleClientNewUser(RemoteConnector connector) {
		//Send players updated payload
		int newLocationId = nextLocationId;



		connectorsByLocationId.Add(newLocationId, connector);
		nextLocationId++;
		foreach(KeyValuePair<int, RemoteConnector> kp in connectorsByLocationId) {
			if(kp.Key == newLocationId) { //Send welcome payload only to new player
				connector.HandlePayloadReceived(new WelcomeBasketPayload(newLocationId, playerNamesByClientId));
			} else {
				connector.HandlePayloadReceived(new UpdateOtherPayload(newLocationId, playerNamesByClientId));
			}
		}


	}

	public void HandleClientSendEvent(RemotePayload payload) {
		//Echo event to all players
		foreach(KeyValuePair<int, RemoteConnector> kp in connectorsByLocationId) {
			kp.Value.HandlePayloadReceived(payload);
		}
	}
}
