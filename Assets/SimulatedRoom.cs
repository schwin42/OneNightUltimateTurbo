using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

	private VirtualServer _server;
	public VirtualServer server { 
		get {
			return _server;
		}
	}

	public List<PersistentPlayer> players;

	public void LaunchGame(int playerCount, List<Role> deckTemplate) {

		_server = new VirtualServer();

		players = new List<PersistentPlayer>();
		for(int i = 0; i < playerCount; i++) {
			players.Add(new PersistentPlayer());
		}
			
		for(int i = 0; i < players.Count; i++) {
			players[i].SetName("Player" + i.ToString());
			players[i].SetSelectedDeck(deckTemplate);
			players[i].connector.JoinSession(players[i].name);
		}

	}

	void Start() {
		LaunchGame(5, new List<Role> { Role.Werewolf, Role.Werewolf, Role.Robber, Role.Troublemaker, Role.Villager, Role.Villager, Role.Mason, Role.Mason });

		players[0].BeginGame();


//		//Make night action selections for all charaters
//		foreach(PersistentPlayer player in players) {
//			player.connector.BroadcastEvent(new NightActionPayload(player.connector.selfClientId, new Selection( new int[] { -1 })));
//		}
//
//		//Input votes
//		foreach(PersistentPlayer player in players) {
//			player.connector.BroadcastEvent(new VotePayload(player.connector.selfClientId, 0));
//		}


	}
}
