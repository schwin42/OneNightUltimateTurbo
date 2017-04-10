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
	private int nextClientId = 0;
	public Dictionary<int, EditorSymConnector> connectorsByClientId = new Dictionary<int, EditorSymConnector>();
	Dictionary<int, string> playerNamesByClientId = new Dictionary<int, string>();
	
	public void HandleClientNewUser(EditorSymConnector newConnector, string name) {
//		Debug.Log("Server received new user");
		//Send players updated payload
		int newClientId = nextClientId;
		nextClientId++;

		connectorsByClientId.Add(newClientId, newConnector);

		playerNamesByClientId.Add(newClientId, name);
		
		foreach(KeyValuePair<int, string> kp in playerNamesByClientId) {
//			Debug.Log("clientId key, self: " + kp.Key + ", " + selfClientId);
			if(kp.Key == newClientId) { //Send welcome payload only to new player
				Debug.Log("Sending welcome basket");
				newConnector.HandlePayloadReceived(new WelcomeBasketPayload(newClientId, playerNamesByClientId));
			} else {
//				Debug.Log("Sending update other, connector client id:" + selfClientId);

				connectorsByClientId[kp.Key].HandlePayloadReceived(new UpdateOtherPayload(newClientId, playerNamesByClientId));
			}
		}


	}

	public void HandleClientSendEvent(RemotePayload payload) {
		//Echo event to all players
		foreach(KeyValuePair<int, EditorSymConnector> kp in connectorsByClientId) {
			kp.Value.HandlePayloadReceived(payload);
		}
	}
}
