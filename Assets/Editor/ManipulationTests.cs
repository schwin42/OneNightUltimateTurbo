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
		GameController.instance.players = new List<GamePlayer> {
			new GamePlayer("A"),
			new GamePlayer("B"),
			new GamePlayer("C"),
		};

		int dealtTroublemakerLocationId = -1;
		int dealtWerewolfLocationId = -1;
		int dealtVillagerLocationId = -1;

		for(int i = 0; i < GameController.instance.players.Count; i++) {
			GamePlayer player = GameController.instance.players[i];
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


		GameController.ExecuteNightActionsInOrder();

		//Assert
		Assert.IsTrue(GameController.instance.players.Single(p => p.locationId == dealtVillagerLocationId).currentCard.data.role == Role.Werewolf &&
			GameController.instance.players.Single(p => p.locationId == dealtWerewolfLocationId).currentCard.data.role == Role.Villager);
	}



}
