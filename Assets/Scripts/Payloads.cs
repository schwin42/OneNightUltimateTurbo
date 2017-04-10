using System.Collections.Generic;

public abstract class RemotePayload {
}

public abstract class GamePayload : RemotePayload {
}

public class NightActionPayload : GamePayload {
	public int sourceClientId;
	public int[][] selection;

	public NightActionPayload (int sourceClientId, int[][] selection) {
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
	//Source location id is newly assigned self
	public int sourceClientId;
	public Dictionary<int, string> playerNamesByClientId;

	public PlayerUpdatePayload (int sourceClientId, Dictionary<int, string> playerNamesByClientId) {
		this.sourceClientId = sourceClientId;
		this.playerNamesByClientId = playerNamesByClientId;
	}
}

public class WelcomeBasketPayload : PlayerUpdatePayload { //This is the only event that is only sent to one device. Don't you feel special?
	public WelcomeBasketPayload(int sourceClientId, Dictionary<int, string> playerNamesByClientId) : base (sourceClientId, playerNamesByClientId) { }
}

public class UpdateOtherPayload : PlayerUpdatePayload { 
	public UpdateOtherPayload(int sourceClientId, Dictionary<int, string> playerNamesByClientId) : base (sourceClientId, playerNamesByClientId) { }
}

public class StartGamePayload : RemotePayload {
	public int sourceClientId;
	public float randomSeed;

	public StartGamePayload (int sourceClientId, float randomSeed) {
		this.sourceClientId = sourceClientId;
		this.randomSeed = randomSeed;
	}
}