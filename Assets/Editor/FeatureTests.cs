using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

public class FeatureTests {


	[Test]
	public void TannerWinsIfHeDies() {
		GameMaster gm = new GameMaster();
		gm.StartGame(new List<string> { "A", "B", "C" }, 
			new GameSettings(new List<Role> { Role.Tanner, Role.Villager, Role.Werewolf, Role.Mason, Role.Mason, Role.Minion }));

		GamePlayer tannerDealtPlayer = gm.players.Single(p => p.dealtCard.data.role == Role.Tanner);

		foreach(GamePlayer player in gm.players) {
			player.votedLocation = tannerDealtPlayer.locationId;
		}

		gm.KillPlayers();
		gm.DetermineWinners();

		Assert.IsTrue(!WinTests.VillagersDidWin(gm.players) && !WinTests.WerewolvesDidWin(gm.players) && tannerDealtPlayer.didWin);
	}

	[Test]
	public void TannerLosesIfAnyoneElseDies() {
		GameMaster gm = new GameMaster();
		gm.StartGame(new List<string> { "A", "B", "C" }, 
			new GameSettings(new List<Role> { Role.Tanner, Role.Villager, Role.Werewolf, Role.Mason, Role.Mason, Role.Minion }));

		GamePlayer villagerDealtPlayer = gm.players.Single(p => p.dealtCard.data.role == Role.Villager);
		GamePlayer tannerDealtPlayer = gm.players.Single(p => p.dealtCard.data.role == Role.Tanner);

		foreach(GamePlayer player in gm.players) {
			player.votedLocation = villagerDealtPlayer.locationId;
		}

		gm.KillPlayers();
		gm.DetermineWinners();

		Assert.IsTrue(!WinTests.VillagersDidWin(gm.players) && WinTests.WerewolvesDidWin(gm.players) && !tannerDealtPlayer.didWin);
	}

	[Test]
	public void TannerLosesIfNoOneDies() {
		GameMaster gm = new GameMaster();
		gm.StartGame(new List<string> { "A", "B", "C" }, 
			new GameSettings(new List<Role> { Role.Tanner, Role.Villager, Role.Werewolf, Role.Mason, Role.Mason, Role.Minion }));
		
		GamePlayer tannerDealtPlayer = gm.players.Single(p => p.dealtCard.data.role == Role.Tanner);

		foreach(GamePlayer player in gm.players) {
			player.votedLocation = -1;
		}

		gm.KillPlayers();
		gm.DetermineWinners();

		Assert.IsTrue(!WinTests.VillagersDidWin(gm.players) && WinTests.WerewolvesDidWin(gm.players) && !tannerDealtPlayer.didWin);
	}
}
