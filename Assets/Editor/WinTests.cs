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
		GameController.instance.players = new List<Player> {
			new Player("A"),
			new Player("B"),
			new Player("C"),
		};
		foreach(Player player in GameController.instance.players) {
			player.ReceiveDealtCard(new RealCard(Role.Villager));
			player.votedLocation = -1;
		}

		GameController.KillPlayers();
		GameController.DetermineWinners();

		Assert.IsTrue(VillagersDidWin());
	}

	[Test]
	public void VillagersWinAndWerewolvesLoseIfAWerewolfDies() {
		GameController.instance.players = new List<Player> {
			new Player("A"),
			new Player("B"),
			new Player("C"),
		};

		for(int i = 0; i < GameController.instance.players.Count; i++) {
			Player player = GameController.instance.players[i];
			if(i == 0) {
				player.ReceiveDealtCard(new RealCard(Role.Werewolf));
				player.votedLocation = GameController.instance.players[1].locationId;
			} else {
				player.ReceiveDealtCard(new RealCard(Role.Villager));
				player.votedLocation = GameController.instance.players[0].locationId;
			}
		}

		GameController.KillPlayers();
		GameController.DetermineWinners();

		Assert.IsTrue(VillagersDidWin() && !WerewolvesDidWin());
	}

	[Test]
	public void WerewolvesWinAndVillagersLoseIfNoWerewolfIsKilled() {
		GameController.instance.players = new List<Player> {
			new Player("A"),
			new Player("B"),
			new Player("C"),
		};

		for(int i = 0; i < GameController.instance.players.Count; i++) {
			Player player = GameController.instance.players[i];
			if(i == 0) {
				player.ReceiveDealtCard(new RealCard(Role.Werewolf));
				player.votedLocation = GameController.instance.players[1].locationId;
			} else {
				player.ReceiveDealtCard(new RealCard(Role.Villager));
				player.votedLocation = GameController.instance.players[1].locationId;
			}
		}

		GameController.KillPlayers();
		GameController.DetermineWinners();

		Assert.IsTrue(!VillagersDidWin() && WerewolvesDidWin());
	}

	[Test]
	public void DrunkWinsIfSwappedForWerewolfAndWerewolvesWin() {
//		GameController.instance.players = new List<Player> {
//			new Player("A"),
//			new Player("B"),
//			new Player("C"),
//		};
		PlayerUi.uiEnabled = false;
		GameController.instance.StartGame(new string[] { "A", "B", "C", },
			new [] { Role.Werewolf, Role.Villager, Role.Drunk, Role.Werewolf, Role.Mason, Role.Mason },
			false
		);
			
		Player werewolfDealtPlayer = GameController.instance.players.Single(p => p.dealtCard.data.role == Role.Werewolf);
		Player villagerDealtPlayer = GameController.instance.players.Single(p => p.dealtCard.data.role == Role.Villager);
		Player drunkDealtPlayer = GameController.instance.players.Single(p => p.dealtCard.data.role == Role.Drunk);

		GameController.SubmitNightAction(werewolfDealtPlayer, new Selection(-1));
		GameController.SubmitNightAction(villagerDealtPlayer, new Selection());
		GameController.SubmitNightAction(drunkDealtPlayer, new Selection(GameController.instance.centerCards.Single(cs => cs.centerCardIndex == 0).locationId));

		GameController.SubmitVote(werewolfDealtPlayer, villagerDealtPlayer.locationId);
		GameController.SubmitVote(villagerDealtPlayer, werewolfDealtPlayer.locationId);
		GameController.SubmitVote(drunkDealtPlayer, villagerDealtPlayer.locationId);

//		GameController.ExecuteNightActionsInOrder();
//		GameController.KillPlayers();
//		GameController.DetermineWinners();

		Assert.IsTrue(drunkDealtPlayer.didWin);
	}

	private bool VillagersDidWin() {
		List<bool> villagerWins = new List<bool>();
		foreach(Player player in GameController.instance.players) {
			if(player.currentCard.data.nature == Nature.Villageperson) {
				villagerWins.Add(player.didWin);
			}
		}
		return villagerWins.All(p => p == true);
	}

	private bool WerewolvesDidWin() {
		List<bool> werewolfWins = new List<bool>();
		foreach(Player player in GameController.instance.players) {
			if(player.currentCard.data.nature == Nature.Werewolf) {
				werewolfWins.Add(player.didWin);
			}
		}
		return werewolfWins.All(p => p == true);
	}

	//Villagers win and werewolves lose if both a villager and werewolf die
}
