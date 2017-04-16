using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class RemoteConnector : MonoBehaviour { //Singleton instance stored on GameController, not monobehavior

	private static RemoteConnector _instance;
	public static RemoteConnector instance {
		get {
			if(_instance == null) {
				_instance = GameObject.FindObjectOfType<RemoteConnector>();
			}
			return _instance;
		}
	}

	public abstract void BeginSession(OnutClient client, string name);

	public abstract void JoinSession(OnutClient client, string name, string roomKey);

	public abstract void StartGame (OnutClient client, StartGamePayload payload);

	public abstract void BroadcastPayload(OnutClient client, RemotePayload payload);

	public abstract void Disconnect(OnutClient client);
}
