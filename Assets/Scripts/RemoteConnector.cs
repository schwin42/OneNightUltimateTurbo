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

public class EditorConnector {

	Client player;

	public EditorConnector(Client player) {
		this.player = player;
	}

	public void JoinSession (string name) {
		Debug.Log("Sending join session event for: " + name);
		VirtualServer.instance.HandleClientNewUser(this, name);
	}

	public void BroadcastEvent (RemotePayload payload) {
		SimulatedRoom.instance.server.HandleClientSendEvent(payload);
//		Debug.Log(selfClientId.ToString() + " sent " + payload.ToString() + " to server");
	}

	public void HandlePayloadReceived(RemotePayload payload) {
		player.HandleRemotePayload(payload);
	}
}

public abstract class RemotePayload {
}

public abstract class GamePayload : RemotePayload {
}

public class NightActionPayload : GamePayload {
	public int sourceClientId;
	public Selection selection;

	public NightActionPayload (int sourceClientId, Selection selection) {
		this.sourceClientId = sourceClientId;
		this.selection = selection;
	}
}

public class VotePayload : GamePayload {
	public int sourceClientId;
	public int voteeLocationId;

	public VotePayload (int sourceClientId, int voteeLocationId) {
		this.sourceClientId = sourceClientId;
		this.voteeLocationId = voteeLocationId;
	}
}

public abstract class PlayerUpdatePayload : RemotePayload { //Player join, player leave
//	public int sourceClientId;
//	public List<string> playerNames;
//	public List<int> clientIds;
//
//	public PlayerUpdatePayload (int sourceClientId, List<string> playerNames, List<int> clientIds) {
//		this.sourceClientId = sourceClientId;
//		this.playerNames = playerNames;
//		this.clientIds = clientIds;
//	}
}

public class WelcomeBasketPayload : PlayerUpdatePayload { //This is the only event that is only sent to one device. Don't you feel special?
	//Source location id is newly assigned self
	public int sourceClientId;
	public List<string> playerNames;
	public List<int> clientIds;

	public WelcomeBasketPayload (int sourceClientId, List<string> playerNames, List<int> clientIds) {
		this.sourceClientId = sourceClientId;
		this.playerNames = playerNames;
		this.clientIds = clientIds;
	}

}

public class UpdateOtherPayload : PlayerUpdatePayload {
	public int sourceClientId;
	public List<string> playerNames;
	public List<int> clientIds;

	public UpdateOtherPayload (int sourceClientId, List<string> playerNames, List<int> clientIds) {
		this.sourceClientId = sourceClientId;
		this.playerNames = playerNames;
		this.clientIds = clientIds;
	}
}

public class StartGamePayload : RemotePayload {
	public int sourceClientId;
	public float randomSeed;

	public StartGamePayload (int sourceClientId, float randomSeed) {
		this.sourceClientId = sourceClientId;
		this.randomSeed = randomSeed;
	}

}
