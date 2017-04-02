using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void PayloadHandler(RemotePayload payload); //A payload tells the client to change its local state

//[System.Serializable]
//public abstract class RemoteConnector { //Singleton instance stored on GameController, not monobehavior
//
//	//State
//	public Dictionary<int, string> connectedPlayers;
//
//	public int selfClientId = -1; //Acquired on successful connection to lobby
//
//	//Events
//	public event PayloadHandler OnPayloadReceived;
//
//	public RemoteConnector (PayloadHandler handler) {
//		OnPayloadReceived += handler;
//	}
//
//	public abstract void JoinSession(string name);
//
////	public abstract void JoinSession() {
//////		OnPayloadReceived.Invoke();
////	}
//
//	public abstract void BroadcastEvent(RemotePayload payload);
//
//	public void HandlePayloadReceived(RemotePayload payload) {
//		OnPayloadReceived.Invoke(payload);
//	}
//}

[System.Serializable]
public class EditorConnector {

	public int selfClientId = -1;
	public Dictionary<int, string> connectedPlayersById;

	//Events
	public event PayloadHandler OnPayloadReceived;

	public EditorConnector(PayloadHandler handler) {
		OnPayloadReceived += handler;
	}

	public void JoinSession (string name) {
		Debug.Log("Sending join session event for: " + name);
		SimulatedRoom.instance.server.HandleClientNewUser(this, name);

	}

	public void BroadcastEvent (RemotePayload payload) {
		SimulatedRoom.instance.server.HandleClientSendEvent(payload);
		Debug.Log(selfClientId.ToString() + " sent " + payload.ToString() + " to server");
	}

	public void HandlePayloadReceived(RemotePayload payload) {
		OnPayloadReceived.Invoke(payload);
	}
}

public abstract class RemotePayload {
	public int sourceClientId;
	public Dictionary<int, string> connectedPlayersById;

	public RemotePayload(int sourceClientId) {
		this.sourceClientId = sourceClientId;
	}
}

public abstract class GamePayload : RemotePayload {
	public GamePayload(int sourceClientId) : base(sourceClientId) { }
}

public class NightActionPayload : GamePayload {
	public Selection selection;

	public NightActionPayload (int sourceClientId, Selection selection) : base (sourceClientId) {
		this.selection = selection;
	}
}

public class VotePayload : GamePayload {
	public int voteeLocationId;

	public VotePayload (int sourceClientId, int voteeLocationId) : base (sourceClientId) {
		this.voteeLocationId = voteeLocationId;
	}
}

public abstract class PlayerUpdatePayload : RemotePayload { //Player join, player leave
	public Dictionary <int, string> connectedPlayersByClientId;

	public PlayerUpdatePayload (int sourceClientId, Dictionary<int, string> connectedPlayersByClientId) : base (sourceClientId) {
		this.connectedPlayersByClientId = new Dictionary<int, string> (connectedPlayersByClientId);
	}
}

public class WelcomeBasketPayload : PlayerUpdatePayload { //This is the only event that is only sent to one device. Don't you feel special?
	//Source location id is newly assigned self
	public WelcomeBasketPayload (int sourceClientId, Dictionary<int, string> connectedPlayersByLocationId) : 
	base (sourceClientId, connectedPlayersByLocationId) {}

}

public class UpdateOtherPayload : PlayerUpdatePayload {
	public UpdateOtherPayload (int sourceClientId, Dictionary<int, string> connectedPlayersByLocationId) : 
	base (sourceClientId, connectedPlayersByLocationId) {}
}

public class StartGamePayload : RemotePayload {
	public float randomSeed;

	public StartGamePayload (int sourceClientId, float randomSeed) : base(sourceClientId) {
		this.randomSeed = randomSeed;
	}

}
