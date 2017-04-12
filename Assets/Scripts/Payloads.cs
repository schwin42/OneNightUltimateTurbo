using System.Collections.Generic;

public enum PayloadType {
	InitiateGame,
	SubmitAction,
	SubmitVote,
}

public abstract class RemotePayload {
	public abstract PayloadType type { get; }
}

public abstract class GamePayload : RemotePayload {
}

public class ActionPayload : GamePayload {
	public override PayloadType type { get { return PayloadType.SubmitAction; }	}
	public string sourceUserId;
	public int[][] selection;

	public ActionPayload (string sourceUserId, int[][] selection) {
		this.sourceUserId = sourceUserId;
		this.selection = selection;
	}
}

public class VotePayload : GamePayload {
	public override PayloadType type { get { return PayloadType.SubmitVote; }	}
	public string sourceClientId;
	public int voteeLocationId;

	public VotePayload (string sourceUserId, int voteeLocationId) {
		this.sourceClientId = sourceUserId;
		this.voteeLocationId = voteeLocationId;
	}
}

//public abstract class PlayerUpdatePayload : RemotePayload { //Player join, player leave
//	//Source location id is newly assigned self
//	public string sourceUserId;
//	public Dictionary<string, string> playerNamesByClientId;
//
//	public PlayerUpdatePayload (string sourceUserId, Dictionary<string, string> playerNamesByClientId) {
//		this.sourceUserId = sourceUserId;
//		this.playerNamesByClientId = playerNamesByClientId;
//	}
//}

//public class WelcomeBasketPayload : PlayerUpdatePayload { //This is the only event that is only sent to one device. Don't you feel special?
//	public WelcomeBasketPayload(string sourceUserId, Dictionary<string, string> playerNamesByClientId) : base (sourceUserId, playerNamesByClientId) { }
//}
//
//public class UpdateOtherPayload : PlayerUpdatePayload { 
//	public UpdateOtherPayload(string sourceUserId, Dictionary<string, string> playerNamesByClientId) : base (sourceUserId, playerNamesByClientId) { }
//}

public class StartGamePayload : RemotePayload {
	public override PayloadType type { get { return PayloadType.InitiateGame; }	}
	public float randomSeed;

	public StartGamePayload (float randomSeed) {
		this.randomSeed = randomSeed;
	}
}