using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

public class ManipulationTests {

//	[Test]
//	public void ViewOneWorks()
//	{
//  	}

	[Test]
	public void TroublemakersNightActionWorks() {
		//Arrange
		GameController.instance.players = new List<Player> {
			new Player("A"),
			new Player("B"),
			new Player("C"),
		};

		int dealtTroublemakerLocationId = -1;
		int dealtWerewolfLocationId = -1;
		int dealtVillagerLocationId = -1;

		for(int i = 0; i < GameController.instance.players.Count; i++) {
			Player player = GameController.instance.players[i];
			if(i == 0) {
				player.ReceiveDealtCard(new RealCard(Role.Troublemaker));
				dealtTroublemakerLocationId = player.locationId;
			} else if(i == 1) {
				player.ReceiveDealtCard(new RealCard(Role.Villager));
				dealtVillagerLocationId = player.locationId;
			} else {
				player.ReceiveDealtCard(new RealCard(Role.Werewolf));
				dealtWerewolfLocationId = player.locationId;
			}
		}

		GameController.instance.players.Single(p => p.locationId == dealtTroublemakerLocationId).nightLocationSelection = 
			new Selection(dealtWerewolfLocationId, dealtVillagerLocationId);
		GameController.instance.players.Single(p => p.locationId == dealtWerewolfLocationId).nightLocationSelection = 
			new Selection(-1);


		GameController.instance.ExecuteNightActionsInOrder();

		//Assert
		Assert.IsTrue(GameController.instance.players.Single(p => p.locationId == dealtVillagerLocationId).currentCard.data.role == Role.Werewolf &&
			GameController.instance.players.Single(p => p.locationId == dealtWerewolfLocationId).currentCard.data.role == Role.Villager);
	}



}
