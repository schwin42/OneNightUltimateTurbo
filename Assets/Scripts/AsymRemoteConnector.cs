using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class AsymRemoteConnector { //Singleton instance stored on GameController, not monobehavior

	//Configuration
	AsymClient client;

	//State
	public Dictionary<int, string> connectedPlayers;

	public int selfClientId = -1; //Acquired on successful connection to lobby

	public AsymRemoteConnector (AsymClient client) {
		this.client = client;
	}

	public abstract void HostSession (string name);

	public abstract void JoinSession(string name);

	public abstract void BroadcastEvent(RemotePayload payload);

	public void HandlePayloadReceived(RemotePayload payload) {
		client.HandleRemotePayload(payload);
	}
}

public class EditorAsymConnector : AsymRemoteConnector {

	public EditorAsymConnector(AsymClient client) : base(client) { }

	public override void HostSession(string name) {
		Debug.Log ("Hosting session");
	}

	public override void JoinSession (string name) {
		Debug.Log("Sending join session event for: " + name);
//		AsymVirtualServer.instance.HandleClientNewUser(this, name);
	}

	public override void BroadcastEvent (RemotePayload payload) {
		Debug.Log ("Broadcasting event: " + payload);
//		AsymVirtualServer.instance.HandleClientSendEvent(payload);
	}
}
