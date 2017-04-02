using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class RemoteTests {

	public int MAX_PLAYERS = 10;

//	[Test]
//	public void SendReceiveWelcomeBasket() {
//		// Use the Assert class to test conditions.
//		SimulatedRoom.instance.LaunchGame(10, new List<Role> () { Role.Werewolf, Role.Werewolf, Role.Villager, Role.Villager, Role.Villager, Role.Minion, Role.Mason, Role.Mason, Role.Troublemaker, Role.Robber });
//		List<PersistentPlayer> clients = new List<PersistentPlayer>();
//		for(int i = 0; i < MAX_PLAYERS; i++) {
//			clients.Add(new PersistentPlayer());
//		}
//
//		List<bool> playerCounts = new List<bool>();
//
//	}

	[Test]
	public void EndToEndTest () {
//		SimulatedRoom.instance.LaunchGame(MAX_PLAYERS, new List<Role> () { Role.Werewolf, Role.Werewolf, Role.Villager, Role.Villager, Role.Villager, 
//			Role.Mason, Role.Mason, Role.Minion, Role.Robber, Role.Troublemaker, Role.Drunk, Role.Insomniac } );
//
//		SimulatedRoom.instance.players[0].BeginGame();
//
//		//Make night action selections for all charaters
//		foreach(PersistentPlayer player in SimulatedRoom.instance.players) {
//			GamePlayer gamePlayer = player.gameMaster.players.Single(gp => gp.clientId == player.selfClientId);
//
//			Selection selection = null;
//			switch(gamePlayer.dealtCard.data.role) {
//			case Role.Werewolf:
//				selection = new Selection(-1);
//				break;
//			case Role.Villager:
//				selection = new Selection(-1);
//				break;
//			case Role.Mason:
//				selection = new Selection(-1);
//				break;
//			case Role.Minion:
//				selection = new Selection(-1);
//				break;
//			case Role.Robber:
//				selection = new Selection(player.gameMaster.players.Single(gp => gp.dealtCard.data.role == Role.Minion).locationId);
//				break;
//			case Role.Troublemaker:
//				selection = new Selection(player.gameMaster.players.Single(gp => gp.dealtCard.data.role == Role.Minion).locationId, 
//					player.gameMaster.players.Single(gp => gp.dealtCard.data.role == Role.Insomniac).locationId); 
//				break;
//			case Role.Drunk:
//				selection = new Selection(player.gameMaster.centerCards[0].locationId);
//				break;
//			case Role.Insomniac:
//				selection = new Selection(player.gameMaster.players.Single(gp => gp.dealtCard.data.role == Role.Robber).locationId);
//				break;
//			}
//			player.connector.BroadcastEvent(new NightActionPayload(player.selfClientId, selection));
//		}
//
//		//Input votes
//		foreach(PersistentPlayer player in SimulatedRoom.instance.players) {
//			player.connector.BroadcastEvent(new VotePayload(player.selfClientId, 0));
//		}
//
//		List<bool> checks = new List<bool>();
//		foreach(PersistentPlayer player in SimulatedRoom.instance.players) {
//			checks.Add(player.gameMaster.players.Single(gp => gp.locationId == 0).didWin);
//		}
//
//		Assert.IsTrue(checks.All(b => b == true));
	}
}
