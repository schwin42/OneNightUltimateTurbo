using UnityEngine;
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

		gm.StartGame(new Dictionary<int, string> { { 0, "0"  }, { 1, "1" }, {2, "2" } },
			new List<Role> {Role.Werewolf, Role.Villager, Role.Drunk, Role.Werewolf, Role.Mason, Role.Mason }
		);
			
		GamePlayer werewolfDealtPlayer = gm.players.Single(p => p.dealtCard.data.role == Role.Werewolf);
		GamePlayer villagerDealtPlayer = gm.players.Single(p => p.dealtCard.data.role == Role.Villager);
		GamePlayer drunkDealtPlayer = gm.players.Single(p => p.dealtCard.data.role == Role.Drunk);

		gm.ReceiveNightAction(werewolfDealtPlayer, new int[][] { new int[] { -1 } } );
		gm.ReceiveNightAction(villagerDealtPlayer, null);
		gm.ReceiveNightAction(drunkDealtPlayer, new int[][] { new int[] { gm.centerSlots.Single(cs => cs.centerCardIndex == 0).locationId, drunkDealtPlayer.locationId } });

		gm.ReceiveVote(werewolfDealtPlayer, villagerDealtPlayer.locationId);
		gm.ReceiveVote(villagerDealtPlayer, werewolfDealtPlayer.locationId);
		gm.ReceiveVote(drunkDealtPlayer, villagerDealtPlayer.locationId);

		Assert.IsTrue(drunkDealtPlayer.didWin);
	}
		
	[Test]
	public void VillagersWinAndWerewolvesWinIfBothDie() {
		GameMaster gm = new GameMaster();

		gm.StartGame(new Dictionary<int, string> { { 0, "0"  }, { 1, "1" }, {2, "2" }, {3, "3"}, {4, "4"} },
			new List<Role> {Role.Werewolf, Role.Werewolf, Role.Villager, Role.Villager, Role.Tanner, Role.Mason, Role.Mason, Role.Troublemaker }
		);

		GamePlayer[] werewolfDealtPlayers = gm.players.Where(p => p.dealtCard.data.role == Role.Werewolf).ToArray();
		GamePlayer[] villagerDealtPlayers = gm.players.Where(p => p.dealtCard.data.role == Role.Villager).ToArray();
		GamePlayer tannerDealtPlayer = gm.players.Single(p => p.dealtCard.data.role == Role.Tanner);

		foreach(GamePlayer player in werewolfDealtPlayers) {
			gm.ReceiveNightAction(player, new int[][] { new int[] { -1 } });
		}

		foreach(GamePlayer player in villagerDealtPlayers) {
			gm.ReceiveNightAction(player, new int[][] { new int[] { -1 } });
		}

		gm.ReceiveNightAction(tannerDealtPlayer, new int[][] { new int[] { -1 } });

		foreach(GamePlayer player in werewolfDealtPlayers) {
			gm.ReceiveVote(player, villagerDealtPlayers[0].locationId);
		}

		foreach(GamePlayer player in villagerDealtPlayers) {
			gm.ReceiveVote(player, werewolfDealtPlayers[0].locationId);
		}

		gm.ReceiveVote(tannerDealtPlayer, tannerDealtPlayer.locationId);

		Assert.IsTrue(!WerewolvesDidWin(gm.players) && VillagersDidWin(gm.players));
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
