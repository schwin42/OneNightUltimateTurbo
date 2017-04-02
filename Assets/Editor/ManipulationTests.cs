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
		GameMaster gm = new GameMaster(); 
		gm.players = new List<GamePlayer> {
			new GamePlayer(gm, 0, "0"),
			new GamePlayer(gm, 1, "1"),
			new GamePlayer(gm, 2, "2"),
		};

		int dealtTroublemakerLocationId = -1;
		int dealtWerewolfLocationId = -1;
		int dealtVillagerLocationId = -1;

		for(int i = 0; i < gm.players.Count; i++) {
			GamePlayer player = gm.players[i];
			if(i == 0) {
				player.ReceiveDealtCard(new RealCard(gm, Role.Troublemaker));
				dealtTroublemakerLocationId = player.locationId;
			} else if(i == 1) {
				player.ReceiveDealtCard(new RealCard(gm, Role.Villager));
				dealtVillagerLocationId = player.locationId;
			} else {
				player.ReceiveDealtCard(new RealCard(gm, Role.Werewolf));
				dealtWerewolfLocationId = player.locationId;
			}
		}

		gm.players.Single(p => p.locationId == dealtTroublemakerLocationId).nightLocationSelection = 
			new Selection(dealtWerewolfLocationId, dealtVillagerLocationId);
		gm.players.Single(p => p.locationId == dealtWerewolfLocationId).nightLocationSelection = 
			new Selection(-1);


		gm.ExecuteNightActionsInOrder();

		//Assert
		Assert.IsTrue(gm.players.Single(p => p.locationId == dealtVillagerLocationId).currentCard.data.role == Role.Werewolf &&
			gm.players.Single(p => p.locationId == dealtWerewolfLocationId).currentCard.data.role == Role.Villager);
	}



}
