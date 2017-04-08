using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class SymRemoteConnector { //Singleton instance stored on GameController, not monobehavior

	//Configuration
	SymClient client;

	//State
	public Dictionary<int, string> connectedPlayers;

	public int selfClientId = -1; //Acquired on successful connection to lobby

	public SymRemoteConnector (SymClient client) {
		this.client = client;
	}

	public abstract void JoinSession(string name);

	public abstract void BroadcastEvent(RemotePayload payload);

	public void HandlePayloadReceived(RemotePayload payload) {
		client.HandleRemotePayload(payload);
	}
}

public class EditorSymConnector : SymRemoteConnector {

	public EditorSymConnector(SymClient client) : base(client) { }

	public override void JoinSession (string name) {
		Debug.Log("Sending join session event for: " + name);
		SymVirtualServer.instance.HandleClientNewUser(this, name);
	}

	public override void BroadcastEvent (RemotePayload payload) {
		SymVirtualServer.instance.HandleClientSendEvent(payload);
//		Debug.Log(selfClientId.ToString() + " sent " + payload.ToString() + " to server");
	}
}
