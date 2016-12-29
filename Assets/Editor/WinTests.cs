using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System.Collections.Generic;

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
			player.locationIdVote = -1;
		}

		GameController.KillPlayers();
		GameController.DetermineWinners();

		//Assert
		bool allPlayersWon = true;
		foreach(Player player in GameController.instance.players) {
			if(!player.didWin) allPlayersWon = false;
			break;
		}
		Assert.IsTrue(allPlayersWon);
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
				player.locationIdVote = GameController.instance.players[1].locationId;
			} else {
				player.ReceiveDealtCard(new RealCard(Role.Villager));
				player.locationIdVote = GameController.instance.players[0].locationId;
			}
		}

		GameController.KillPlayers();
		GameController.DetermineWinners();

		bool villagersWon = true;
		bool werewolvesWon = true;
		foreach(Player player in GameController.instance.players) {
			if(player.currentCard.data.role == Role.Villager) {
				if(!player.didWin) villagersWon = false;
			} else if(player.currentCard.data.role == Role.Werewolf) {
				werewolvesWon = player.didWin;
			}
			break;
		}
		Assert.IsTrue(villagersWon && !werewolvesWon);
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
				player.locationIdVote = GameController.instance.players[1].locationId;
			} else {
				player.ReceiveDealtCard(new RealCard(Role.Villager));
				player.locationIdVote = GameController.instance.players[1].locationId;
			}
		}

		GameController.KillPlayers();
		GameController.DetermineWinners();

		bool villagersWon = false;
		bool werewolvesWon = true;
		foreach(Player player in GameController.instance.players) {
			if(player.currentCard.data.role == Role.Villager) {
				villagersWon = player.didWin;
			} else if(player.currentCard.data.role == Role.Werewolf) {
				werewolvesWon = player.didWin;
			}
			break;
		}
		Assert.IsTrue(!villagersWon && werewolvesWon);
	}

	//Villagers win and werewolves lose if both a villager and werewolf die
}
