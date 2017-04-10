using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

public class DeckGeneratorTests {

	[Test]
	public void AddingAMasonAddsOtherMason()
	{
		//Arrange
		int cardsInDeck = 10;
		List<List<CardData>> decks = new List<List<CardData>>();
		Dictionary<int, int> deckToMasonCount = new Dictionary<int, int>();
		List<List<int>> replacementIndecesByDeck = new List<List<int>>();
		for(int i = 0; i < cardsInDeck; i++) {
			List<CardData> instancePool = GameData.instance.totalCardPool.OrderBy(x => Random.value).ToList();
			instancePool.MoveRoleToPosition(Role.Mason, i);
			List<CardData> resultantInstancePool;
			List<int> replacementIndeces;
			List<CardData> deck = DeckGenerator.GenerateNewUnfixedDeck(cardsInDeck, instancePool, out resultantInstancePool, out replacementIndeces);
			replacementIndecesByDeck.Add(replacementIndeces);
			int masonCount = deck.Count(cd => cd.role == Role.Mason);
			deckToMasonCount.Add(i, masonCount);
			decks.Add(deck);
			Debug.Log("Output deck " + i + ": " + deck.ToStringCardList());
			Debug.Log("Deck id, masons: " + i.ToString() + ", " + masonCount);
		}
		//Assert
		List<bool> deckOfIndexIsCorrect = new List<bool>(); //Fix test to work with all possible attempted values to insert Mason
		for(int i = 0; i < deckToMasonCount.Count; i++) {
			if(i == (deckToMasonCount.Count - 1)) {
//				deckOfIndexIsCorrect.Add(decks[i].Count(cd => cd.role == Role.Mason) == 0);
				continue;
			} else if (i == 0){
				deckOfIndexIsCorrect.Add(decks[i].Count(cd => cd.role == Role.Mason) == 2);
			} else {
				//Can't be guaranteed of the positions of any other than the first deck
				continue;
			}
		}
		Assert.IsTrue(deckOfIndexIsCorrect.Count != 0 && deckOfIndexIsCorrect.All(b => b == true));
	}

	[Test]
	public void DeckToStartGameUnit() {
		GameMaster gm = new GameMaster();
		List<Role> selectedDeckBlueprint = DeckGenerator.GenerateRandomizedDeck(3 + 3, Mathf.FloorToInt(Random.value * 100000), true).ToList();
		gm.StartGame(new Dictionary<int, string> { { 0, "0"  }, { 1, "1" }, {2, "2" } },
			selectedDeckBlueprint
		);

		Assert.IsTrue(gm.centerSlots.Count == 3);
	}
}
