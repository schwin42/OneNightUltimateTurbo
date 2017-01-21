using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

public class ManipulationTests {

	[Test]
	public void TroublemakersNightActionWorks() {
		//Arrange
		GameController.instance.StartGame(new string[] { "A", "B", "C", },
			new [] { Role.Troublemaker, Role.Werewolf, Role.Villager, Role.Villager, Role.Werewolf, Role.Villager },
			false
		);

		Player troublemakerDealtPlayer = GameController.instance.players.Single(p => p.dealtCard.data.role == Role.Troublemaker);
		Player villagerDealtPlayer = GameController.instance.players.Single(p => p.dealtCard.data.role == Role.Villager);
		Player werewolfDealtPlayer = GameController.instance.players.Single(p => p.dealtCard.data.role == Role.Werewolf);

		GameController.SubmitNightAction(troublemakerDealtPlayer, new Selection(werewolfDealtPlayer.locationId, villagerDealtPlayer.locationId));
		GameController.SubmitNightAction(villagerDealtPlayer, new Selection(-1));
		GameController.SubmitNightAction(werewolfDealtPlayer, new Selection(-1));

		//Assert
		Assert.IsTrue(villagerDealtPlayer.currentCard.data.role == Role.Werewolf && werewolfDealtPlayer.currentCard.data.role == Role.Villager);
	}

	[Test]
	public void RobberAndInsomniacObserveTheirOwnRole() {
		GameController.instance.StartGame(new string[] { "A", "B", "C", },
			new [] { Role.Robber, Role.Insomniac, Role.Villager, Role.Villager, Role.Werewolf, Role.Villager },
			false
		);

		Player villagerDealtPlayer = GameController.instance.players.Single(p => p.dealtCard.data.role == Role.Villager);
		Player insomniacDealtPlayer = GameController.instance.players.Single(p => p.dealtCard.data.role == Role.Insomniac);
		Player robberDealtPlayer = GameController.instance.players.Single(p => p.dealtCard.data.role == Role.Robber);

		int robberCardId = robberDealtPlayer.dealtCard.gamePieceId;
		int insomniacCardId = insomniacDealtPlayer.dealtCard.gamePieceId;

		GameController.SubmitNightAction(robberDealtPlayer, new Selection(insomniacDealtPlayer.locationId));
		GameController.SubmitNightAction(villagerDealtPlayer, new Selection());
		GameController.SubmitNightAction(insomniacDealtPlayer, new Selection());

		Assert.IsTrue(
			robberDealtPlayer.observations[0].gamePieceId == insomniacCardId && robberDealtPlayer.observations[0].locationId == robberDealtPlayer.locationId &&
			insomniacDealtPlayer.observations[0].gamePieceId == robberCardId && insomniacDealtPlayer.observations[0].locationId == insomniacDealtPlayer.locationId
		);

	}

}
