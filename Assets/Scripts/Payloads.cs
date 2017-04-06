using System.Collections.Generic;

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