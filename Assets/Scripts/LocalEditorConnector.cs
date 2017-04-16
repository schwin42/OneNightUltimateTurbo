using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalEditorConnector : RemoteConnector {

	public override void BeginSession(OnutClient client, string name) {
		VirtualServer.instance.HandleClientNewUser(client, name);
	}

	public override void JoinSession (OnutClient client, string name, string roomKey) {
		VirtualServer.instance.HandleClientNewUser(client, name);
	}

	public override void StartGame(OnutClient client, StartGamePayload payload) {
		VirtualServer.instance.HandleClientSendEvent (payload);
	}

	public override void BroadcastPayload (OnutClient client, RemotePayload payload) {
		VirtualServer.instance.HandleClientSendEvent(payload);
	}

	public override void Disconnect(OnutClient client) {
		VirtualServer.instance.Disconnect (client);
	}
}
