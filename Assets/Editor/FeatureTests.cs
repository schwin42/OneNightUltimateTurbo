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

		Assert.IsTrue(!VillagersDidWin(gm.players) && !WerewolvesDidWin(gm.players) && tannerDealtPlayer.didWin);
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

		Assert.IsTrue(!VillagersDidWin(gm.players) && WerewolvesDidWin(gm.players) && !tannerDealtPlayer.didWin);
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

		Assert.IsTrue(!VillagersDidWin(gm.players) && WerewolvesDidWin(gm.players) && !tannerDealtPlayer.didWin);
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
}
