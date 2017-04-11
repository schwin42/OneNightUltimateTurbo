using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class SimulatedRoom : MonoBehaviour { //Analogous to having n devices in a room with 1 God player having access to all of them

	private static SimulatedRoom _instance;
	public static SimulatedRoom instance {
		get {
			if(_instance == null) {
				_instance = GameObject.FindObjectOfType<SimulatedRoom>();
			}
			return _instance;
		}
	}

	public const int playerCount = 5;

	private SymVirtualServer _server;
	public SymVirtualServer server { 
		get {
			return _server;
		}
	}

	public List<SymClient> players;

	public void LaunchGame(int playerCount, List<Role> deckTemplate) {

		GameObject serverGo = new GameObject();
		serverGo.name = "Server";
		_server = serverGo.AddComponent<SymVirtualServer>();


		players = new List<SymClient>();
		for(int i = 0; i < playerCount; i++) {

			GameObject go = new GameObject();
			SymClient player = go.AddComponent<SymClient>();
			players.Add(player);
		}
			
		for(int i = 0; i < players.Count; i++) {
			string name = "Player" + i.ToString();
			players[i].gameObject.name = name;
			players[i].PlayerName = name;
			players[i].JoinSession("");
		}

	}

	void Start() {

		//SimulatedRoom.instance.LaunchGame(10, new List<Role> () { Role.Werewolf, Role.Werewolf, Role.Drunk, Role.Insomniac, Role.Tanner,
		//	Role.Mason, Role.Mason, Role.Minion, Role.Robber, Role.Troublemaker, Role.Villager, Role.Villager, Role.Villager  } );

		//SimulatedRoom.instance.players[0].BeginGame();

		////Make night action selections for all charaters
		//foreach(PersistentPlayer player in SimulatedRoom.instance.players) {
		//	GamePlayer gamePlayer = player.gameMaster.players.Single(gp => gp.clientId == player.selfClientId);

		//	Selection selection = null;
		//	switch(gamePlayer.dealtCard.data.role) {
		//	case Role.Werewolf:
		//		selection = new Selection(-1);
		//		break;
		//	case Role.Villager:
		//		selection = new Selection(-1);
		//		break;
		//	case Role.Mason:
		//		selection = new Selection(-1);
		//		break;
		//	case Role.Minion:
		//		selection = new Selection(-1);
		//		break;
		//	case Role.Robber:
		//		selection = new Selection(player.gameMaster.players.Single(gp => gp.dealtCard.data.role == Role.Minion).locationId);
		//		break;
		//	case Role.Troublemaker:
		//		selection = new Selection(player.gameMaster.players.Single(gp => gp.dealtCard.data.role == Role.Minion).locationId, 
		//			player.gameMaster.players.Single(gp => gp.dealtCard.data.role == Role.Insomniac).locationId); 
		//		break;
		//	case Role.Drunk:
		//		selection = new Selection(player.gameMaster.centerCards[0].locationId);
		//		break;
		//	case Role.Insomniac:
		//		selection = new Selection(player.gameMaster.players.Single(gp => gp.dealtCard.data.role == Role.Robber).locationId);
		//		break;
		//	}
		//	player.connector.BroadcastEvent(new NightActionPayload(player.selfClientId, selection));
		//}

		////Input votes
		//foreach(PersistentPlayer player in SimulatedRoom.instance.players) {
		//	player.connector.BroadcastEvent(new VotePayload(player.selfClientId, 0));
		//}

		//List<bool> checks = new List<bool>();
		//foreach(PersistentPlayer player in SimulatedRoom.instance.players) {
		//	bool b = !player.gameMaster.players.Single(gp => gp.locationId == 0).didWin;
		//	print("B: " + b);
		//	checks.Add(b);

		//}

		//Debug.Log("Passed? " + checks.All(b => b == true));
	}

}
