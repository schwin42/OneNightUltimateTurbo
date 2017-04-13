using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalEditorConnector : RemoteConnector {

	public override void BeginSession(OnumClient client, string name) {
		VirtualServer.instance.HandleClientNewUser(client, name);
	}

	public override void JoinSession (OnumClient client, string name, string roomKey) {
		VirtualServer.instance.HandleClientNewUser(client, name);
	}

	public override void StartGame(OnumClient client, StartGamePayload payload) {
		VirtualServer.instance.HandleClientSendEvent (payload);
	}

	public override void BroadcastPayload (OnumClient client, RemotePayload payload) {
		VirtualServer.instance.HandleClientSendEvent(payload);
	}

	public override void Disconnect(OnumClient client) {
		VirtualServer.instance.Disconnect (client);
	}
}
