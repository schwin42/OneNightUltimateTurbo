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
			new GamePlayer(gm, "0", "0"),
			new GamePlayer(gm, "1", "1"),
			new GamePlayer(gm, "2", "2"),
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

		gm.players.Single(p => p.locationId == dealtTroublemakerLocationId).nightLocationSelection = new int[][] { new int[] { dealtWerewolfLocationId, dealtVillagerLocationId }
		};
		gm.players.Single(p => p.locationId == dealtWerewolfLocationId).nightLocationSelection = new int[][] { new int[] { -1 } };


		gm.ExecuteNightActionsInOrder();

		//Assert
		Assert.IsTrue(gm.players.Single(p => p.locationId == dealtVillagerLocationId).currentCard.data.role == Role.Werewolf &&
			gm.players.Single(p => p.locationId == dealtWerewolfLocationId).currentCard.data.role == Role.Villager);
	}

	[Test]
	public void SeersViewTwoActionWorks() {
		GameMaster gm = new GameMaster();
		gm.players = new List<GamePlayer> {
			new GamePlayer(gm, "0", "0")
		};

		gm.players[0].ReceiveDealtCard(new RealCard(gm, Role.Seer));

		RealCard[] centerCards = new RealCard[] { new RealCard(gm, Role.Villager), new RealCard(gm, Role.Werewolf), new RealCard(gm, Role.Robber) };
		gm.centerSlots = new List<CenterCardSlot>();
		for(int i = 0; i < 3; i++) {
			gm.centerSlots.Add(new CenterCardSlot(gm, i, centerCards[i])); 
		}

		gm.players[0].nightLocationSelection = new int[][] {
			new int[] { 1 }, // Choose to skip first action
			new int[] { -1 }, //View player action not used
			new int[] { gm.centerSlots[0].locationId , gm.centerSlots[2].locationId }, //View first and third
		};

		gm.ExecuteNightActionsInOrder();

		Assert.IsTrue(gm.gamePiecesById[gm.players[0].observations[0].gamePieceId].name == "Villager" && 
			gm.centerSlots[2].dealtCard.gamePieceId == gm.players[0].observations[1].gamePieceId);
	}

	[Test]
	public void SeersViewOneActionWorks() {
		GameMaster gm = new GameMaster();
		gm.players = new List<GamePlayer> {
			new GamePlayer(gm, "0", "0"),
			new GamePlayer(gm, "1", "1"),
		};

		gm.players[0].ReceiveDealtCard(new RealCard(gm, Role.Seer));
		gm.players[1].ReceiveDealtCard(new RealCard(gm, Role.Villager));

		gm.players[0].nightLocationSelection = new int[][] {
			new int[] { 2 }, //Choose to skip second option
			new int[] { gm.players[1].locationId }, //View werewolf
			new int[] { -1 }, //View two not used
		};

		gm.ExecuteNightActionsInOrder();

		Assert.IsTrue(gm.gamePiecesById[gm.players[0].observations[0].gamePieceId].name == "Villager");
	}

	[Test]
	public void ApprenticeSeersNightActionWork() {
		GameMaster gm = new GameMaster();
		gm.players = new List<GamePlayer> {
			new GamePlayer(gm, "0", "0")
		};

		gm.players[0].ReceiveDealtCard(new RealCard(gm, Role.ApprenticeSeer));

		RealCard[] centerCards = new RealCard[] { new RealCard(gm, Role.Villager), new RealCard(gm, Role.Werewolf), new RealCard(gm, Role.Robber) };
		gm.centerSlots = new List<CenterCardSlot>();
		for(int i = 0; i < 3; i++) {
			gm.centerSlots.Add(new CenterCardSlot(gm, i, centerCards[i])); 
		}

		gm.players[0].nightLocationSelection = new int[][] {
			new int[] { gm.centerSlots[0].locationId }, // View villager
		};

		gm.ExecuteNightActionsInOrder();

		Assert.IsTrue(gm.gamePiecesById[gm.players[0].observations[0].gamePieceId].name == "Villager");
	}

	[Test]
	public void WitchNightActionWorks() {
		GameMaster gm = new GameMaster();
		gm.players = new List<GamePlayer>() {
			new GamePlayer(gm, "0", "0"),
			new GamePlayer(gm, "1", "1"),
		};

		gm.players[0].ReceiveDealtCard(new RealCard(gm, Role.Witch));
		gm.players[1].ReceiveDealtCard(new RealCard(gm, Role.Werewolf));

		gm.centerSlots = new List<CenterCardSlot>() {
			new CenterCardSlot(gm, 0, new RealCard(gm, Role.Villager)),
		};

		gm.players[0].nightLocationSelection = new int[][] { new int[] { gm.centerSlots[0].locationId }, new int[] { gm.centerSlots[0].locationId, gm.players[1].locationId } };
		gm.players[1].nightLocationSelection = new int[][] { new int[] { gm.centerSlots[0].locationId } };

		gm.ExecuteNightActionsInOrder();

		Assert.IsTrue(gm.players[1].currentCard.data.role == Role.Villager);
	}
}
