using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class SymRemoteConnector : MonoBehaviour { //Singleton instance stored on GameController, not monobehavior

	private static SymRemoteConnector _instance;
	public static SymRemoteConnector instance {
		get {
			if(_instance == null) {
				_instance = GameObject.FindObjectOfType<SymRemoteConnector>();
			}
			return _instance;
		}
	}

	public abstract void BeginSession(SymClient client, string name);

	public abstract void JoinSession(SymClient client, string name, string roomKey);

	public abstract void StartGame (SymClient client, StartGamePayload payload);

	public abstract void BroadcastMessage(SymClient client, RemotePayload payload);

	public abstract void Disconnect(SymClient client);
}
