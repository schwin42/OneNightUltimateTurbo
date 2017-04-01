using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MockupServer : MonoBehaviour {

	private static MockupServer _instance;
	public static MockupServer Instance {
		get {
			if(_instance == null) {
				_instance = GameObject.FindObjectOfType<MockupServer>();
			}
			return _instance;
		}
	}

	//State
	private int nextLocationId = 0;
	public Dictionary<int, RemoteConnector> connectorsByPlayerId = new Dictionary<int, RemoteConnector>();
	public Dictionary<int, string> connectedPlayerNamesByLocationId = new Dictionary<int, string>();



	// Use this for initialization
	void Start () {
		
	}
	
	public void HandleClientNewUser(RemoteConnector connector, string name) {
		//Send players updated payload
		int newLocationId = nextLocationId;



		connectorsByPlayerId.Add(newLocationId, connector);
		connectedPlayerNamesByLocationId.Add(newLocationId, name);
		nextLocationId++;
		foreach(KeyValuePair<int, RemoteConnector> kp in connectorsByPlayerId) {
			if(kp.Key == newLocationId) { //Send welcome payload only to new player
				connector.HandlePayloadReceived(new WelcomeBasketPayload(newLocationId, connectedPlayerNamesByLocationId));
			} else {
				connector.HandlePayloadReceived(new UpdateOtherPayload(newLocationId, connectedPlayerNamesByLocationId));
			}
		}


	}

	public void HandleClientSendEvent(RemotePayload payload) {
		//Echo event to all players
		foreach(KeyValuePair<int, RemoteConnector> kp in connectorsByPlayerId) {
			kp.Value.HandlePayloadReceived(payload);
		}
	}
}
