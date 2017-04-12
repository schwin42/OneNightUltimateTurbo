using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorSymConnector : SymRemoteConnector {

	public override void BeginSession(SymClient client, string name) {
		Debug.LogError("Not implemented");
	}

	public override void JoinSession (SymClient client, string name, string roomKey) {
		SymVirtualServer.instance.HandleClientNewUser(client, name);
	}

	public override void StartGame(SymClient client) {
		Debug.LogError ("Not implemented");
	}

	public override void BroadcastMessage (SymClient client, RemotePayload payload) {
		SymVirtualServer.instance.HandleClientSendEvent(payload);
		//		Debug.Log(selfClientId.ToString() + " sent " + payload.ToString() + " to server");
	}

	public override void Disconnect(SymClient client) {
		SymVirtualServer.instance.Disconnect (client);
	}
}
