﻿using System.Collections.Generic;
using UnityEngine.Networking;

public class OnuMessage {
	public static short Introduction = MsgType.Highest + 1;
	public static short PlayersUpdated = MsgType.Highest + 2;
	public static short StartGame = MsgType.Highest + 3;
	public static short NightAction = MsgType.Highest + 4;
	public static short VotePayload = MsgType.Highest + 5;
}

public class IntroductionMessage : MessageBase { //Sent from client to server on connect
	public string playerName;
}

public class PlayersUpdatedMessage : MessageBase { //Sent from server to all clients on player join
	public string[] playerNamesByClientId;
}

public class StartGameMessage : MessageBase {
	public int randomSeed;
}

public class NightActionMessage : MessageBase {
	public short[] selection;
}

public class VoteMessage : MessageBase {
	public int votee;
}