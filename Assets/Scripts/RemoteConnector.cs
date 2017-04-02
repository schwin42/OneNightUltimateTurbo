using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void PayloadHandler(RemotePayload payload); //A payload tells the client to change its local state


public abstract class RemoteConnector { //Singleton instance stored on GameController, not monobehavior

	//State
	public Dictionary<int, string> connectedPlayers;
	public int selfClientId; //Acquired on successful connection to lobby

	//Events
	public event PayloadHandler OnPayloadReceived;

	public RemoteConnector (PayloadHandler handler) {
		OnPayloadReceived += handler;
	}

	public abstract void JoinSession();

//	public abstract void JoinSession() {
////		OnPayloadReceived.Invoke();
//	}

	public abstract void BroadcastEvent(RemotePayload payload);

	public void HandlePayloadReceived(RemotePayload payload) {
		OnPayloadReceived.Invoke(payload);
	}
}

public class EditorConnector : RemoteConnector {

	public EditorConnector(PayloadHandler handler) : base (handler) { }

	public override void JoinSession () {
		SimulatedRoom.instance.server.HandleClientNewUser(this);
	}

	public override void BroadcastEvent (RemotePayload payload) {
		SimulatedRoom.instance.server.HandleClientSendEvent(payload);
	}
}

public abstract class RemotePayload {
	public int sourceClientId;

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
