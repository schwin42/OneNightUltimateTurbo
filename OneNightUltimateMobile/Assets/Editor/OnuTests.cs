using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System.Collections.Generic;

public class EditorTest {

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

		//Assert
		bool allPlayersWon = true;
		foreach(Player player in GameController.instance.players) {
			player.didWin = GameController.EvaluateRequirementRecursive(player, player.currentCard.winRequirements);
			if(!player.didWin) allPlayersWon = false;
			break;
		}
		Assert.IsTrue(allPlayersWon);
			
//		Assert.AreEqual(newGameObjectName, gameObject.name);
	}
}
