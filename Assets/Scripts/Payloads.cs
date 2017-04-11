using System.Collections.Generic;

public abstract class RemotePayload {
}

public abstract class GamePayload : RemotePayload {
}

public class NightActionPayload : GamePayload {
	public string sourceClientId;
	public int[][] selection;

	public NightActionPayload (string sourceUserId, int[][] selection) {
		this.sourceClientId = sourceUserId;
		this.selection = selection;
	}
}

public class VotePayload : GamePayload {
	public string sourceClientId;
	public int voteeLocationId;

	public VotePayload (string sourceUserId, int voteeLocationId) {
		this.sourceClientId = sourceUserId;
		this.voteeLocationId = voteeLocationId;
	}
}

public abstract class PlayerUpdatePayload : RemotePayload { //Player join, player leave
	//Source location id is newly assigned self
	public string sourceUserId;
	public Dictionary<string, string> playerNamesByClientId;

	public PlayerUpdatePayload (string sourceUserId, Dictionary<string, string> playerNamesByClientId) {
		this.sourceUserId = sourceUserId;
		this.playerNamesByClientId = playerNamesByClientId;
	}
}

public class WelcomeBasketPayload : PlayerUpdatePayload { //This is the only event that is only sent to one device. Don't you feel special?
	public WelcomeBasketPayload(string sourceUserId, Dictionary<string, string> playerNamesByClientId) : base (sourceUserId, playerNamesByClientId) { }
}

public class UpdateOtherPayload : PlayerUpdatePayload { 
	public UpdateOtherPayload(string sourceUserId, Dictionary<string, string> playerNamesByClientId) : base (sourceUserId, playerNamesByClientId) { }
}

public class StartGamePayload : RemotePayload {
	public string sourceClientId;
	public float randomSeed;

	public StartGamePayload (string sourceClientId, float randomSeed) {
		this.sourceClientId = sourceClientId;
		this.randomSeed = randomSeed;
	}
}