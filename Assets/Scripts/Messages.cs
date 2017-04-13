using System.Collections.Generic;
using UnityEngine.Networking;

public class OnuMessage {
	public static short Introduction = MsgType.Highest + 1;
	public static short Welcome = MsgType.Highest + 2;
	public static short PlayersUpdated = MsgType.Highest + 3;
	public static short StartGame = MsgType.Highest + 4;
	public static short NightAction = MsgType.Highest + 5;
	public static short Vote = MsgType.Highest + 6;
}

public class IntroductionMessage : MessageBase { //Sent from client to server on connect
	public string playerName;
}

public class WelcomeMessage : MessageBase {
	public string userId;
}

public class PlayersUpdatedMessage : MessageBase { //Sent from server to all clients on player join
	public string[] userIds;
}

public class StartGameMessage : MessageBase {
	public int randomSeed;
}

public class NightActionMessage : MessageBase {
	public string sourceUserId;
	public int[][] selection;
}

public class VoteMessage : MessageBase {
	public string sourceUserId;
	public int voteeLocationId;
}