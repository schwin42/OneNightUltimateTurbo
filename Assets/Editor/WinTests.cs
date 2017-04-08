﻿using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

public class WinTests {

	[Test]
	public void VillagersWinIfNoWerewolvesPresentAndNoOneDies()
	{
		//Arrange
		GameMaster gm = new GameMaster();
		gm.players = new List<GamePlayer> {
			new GamePlayer(gm, 0, "A"),
			new GamePlayer(gm, 1, "B"),
			new GamePlayer(gm, 2, "C"),
		};
		foreach(GamePlayer player in gm.players) {
			player.ReceiveDealtCard(new RealCard(gm, Role.Villager));
			player.votedLocation = -1;
		}

		gm.KillPlayers();
		gm.DetermineWinners();

		Assert.IsTrue(VillagersDidWin(gm.players));
	}

	[Test]
	public void VillagersWinAndWerewolvesLoseIfAWerewolfDies() {
		GameMaster gm = new GameMaster();
		gm.players = new List<GamePlayer> {
			new GamePlayer(gm, 0, "A"),
			new GamePlayer(gm, 1, "B"),
			new GamePlayer(gm, 2, "C"),
		};

		for(int i = 0; i < gm.players.Count; i++) {
			GamePlayer player = gm.players[i];
			if(i == 0) {
				player.ReceiveDealtCard(new RealCard(gm, Role.Werewolf));
				player.votedLocation = gm.players[1].locationId;
			} else {
				player.ReceiveDealtCard(new RealCard(gm, Role.Villager));
				player.votedLocation = gm.players[0].locationId;
			}
		}

		gm.KillPlayers();
		gm.DetermineWinners();

		Assert.IsTrue(VillagersDidWin(gm.players) && !WerewolvesDidWin(gm.players));
	}

	[Test]
	public void WerewolvesWinAndVillagersLoseIfNoWerewolfIsKilled() {
		GameMaster gm = new GameMaster();
		gm.players = new List<GamePlayer> {
			new GamePlayer(gm, 0, "A"),
			new GamePlayer(gm, 1, "B"),
			new GamePlayer(gm, 2, "C"),
		};

		for(int i = 0; i < gm.players.Count; i++) {
			GamePlayer player = gm.players[i];
			if(i == 0) {
				player.ReceiveDealtCard(new RealCard(gm, Role.Werewolf));
				player.votedLocation = gm.players[1].locationId;
			} else {
				player.ReceiveDealtCard(new RealCard(gm, Role.Villager));
				player.votedLocation = gm.players[1].locationId;
			}
		}

		gm.KillPlayers();
		gm.DetermineWinners();

		Assert.IsTrue(!VillagersDidWin(gm.players) && WerewolvesDidWin(gm.players));
	}

	[Test]
	public void DrunkWinsIfSwappedForWerewolfAndWerewolvesWin() {
		GameMaster gm = new GameMaster();

		gm.StartGame(new List<string> { "A", "B", "C" },
			new Role[] {Role.Werewolf, Role.Villager, Role.Drunk, Role.Werewolf, Role.Mason, Role.Mason },
			false
		);
			
		GamePlayer werewolfDealtPlayer = gm.players.Single(p => p.dealtCard.data.role == Role.Werewolf);
		GamePlayer villagerDealtPlayer = gm.players.Single(p => p.dealtCard.data.role == Role.Villager);
		GamePlayer drunkDealtPlayer = gm.players.Single(p => p.dealtCard.data.role == Role.Drunk);

		gm.ReceiveNightAction(werewolfDealtPlayer, new Selection(-1));
		gm.ReceiveNightAction(villagerDealtPlayer, new Selection());
		gm.ReceiveNightAction(drunkDealtPlayer, new Selection(gm.centerCards.Single(cs => cs.centerCardIndex == 0).locationId));

		gm.ReceiveVote(werewolfDealtPlayer, villagerDealtPlayer.locationId);
		gm.ReceiveVote(villagerDealtPlayer, werewolfDealtPlayer.locationId);
		gm.ReceiveVote(drunkDealtPlayer, villagerDealtPlayer.locationId);

		Assert.IsTrue(drunkDealtPlayer.didWin);
	}

	private static bool VillagersDidWin(List<GamePlayer> players) {
		List<bool> villagerWins = new List<bool>();
		foreach(GamePlayer player in players) {
			if(player.currentCard.data.nature == Nature.Villageperson) {
				villagerWins.Add(player.didWin);
			}
		}
		return villagerWins.All(p => p == true);
	}

	private static bool WerewolvesDidWin(List<GamePlayer> players) {
		List<bool> werewolfWins = new List<bool>();
		foreach(GamePlayer player in players) {
			if(player.currentCard.data.nature == Nature.Werewolf) {
				werewolfWins.Add(player.didWin);
			}
		}
		return werewolfWins.All(p => p == true);
	}

	//Villagers win and werewolves lose if both a villager and werewolf die
}
