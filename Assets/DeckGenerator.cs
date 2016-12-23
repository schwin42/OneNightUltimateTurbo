using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class DeckGenerator {

	public static List<CardData> GenerateRandomizedDeck(int cardCount) {
		List<CardData> instancePool;
		List<CardData> deck;
		List<int> replacementIndeces;
		for(int i = 0; i < 100; i++) { //Iterate to 100 instead of while loop, to prevent infinite loops
			deck = GenerateRandomUnfixedDeck(cardCount, out instancePool, out replacementIndeces);
			int werewolfOrVampireCount = deck.Count(cd => cd.nature == Nature.Werewolf || cd.nature == Nature.Vampire);
			if (werewolfOrVampireCount < 2) {
				if(replacementIndeces.Count < 2 - werewolfOrVampireCount) {
					continue;
				} else {
					return ReplaceUnseededCardsWithWerewolfOrVampire(deck, instancePool, replacementIndeces, 2 - werewolfOrVampireCount);
				}
			} else {
				Debug.Log ("Deck already valid, no need to fix");
				return deck;
			}
		}
		Debug.LogError("Exceeded 100 attempts to generate valid deck");
		return null;
	}

	private static List<CardData> GenerateRandomUnfixedDeck(int cardCount, out List<CardData> instancePool, out List<int> replacementIndeces) {
		instancePool = GameData.instance.cardPool.OrderBy(x => Random.value).ToList();
		replacementIndeces = new List<int>();
		List<CardData> deck = new List<CardData>();
		for(int i = 0; i < cardCount; i++) {
			if (i != cardCount - 1) {
				CardData card = instancePool [0];
				deck.Add(card);
				Debug.Log ("Adding: " + card.role.ToString ());
				instancePool.RemoveAt(0);


				//TODO Check if seed requirement already exists
//				List<Card> deckIndex = card.seedRequirement.GetFirstIndex(deck);

				//Add seed requirement if it exists
				int seedIndex = card.seedRequirement.GetFirstIndex(instancePool);
				if(seedIndex != -1) {
					deck.Add (instancePool [seedIndex]);
					Debug.Log("Adding " + card.role.ToString() + "'s seed requirement: " + instancePool[seedIndex].role.ToString()); 
					instancePool.RemoveAt (seedIndex);
					i++;
				} else {
					replacementIndeces.Add(i);
				}
			} else {
				//Move next card with no seed requirement from pool to deck
				int nextSeedIndex = instancePool.IndexOf(instancePool.First(cd => cd.seedRequirement.isEmpty));
				CardData card = instancePool [nextSeedIndex];
				deck.Add (card);
				Debug.Log("Adding seedless card: " + card.role.ToString());
				instancePool.RemoveAt (nextSeedIndex);
			}
		}
		return deck;
	}

	private static List<CardData> ReplaceUnseededCardsWithWerewolfOrVampire(List<CardData> deck, List<CardData> instancePool, List<int> replacementIndeces, int count) {
		//TODO Allow these cards to check seed requirements as well
		Debug.Log ("Replacing " + count + " cards with werewolves or vampires" );
		for (int i = 0; i < 2 - count; i++) {
			int nextWovCard = instancePool.IndexOf(instancePool.First (cd => cd.nature == Nature.Werewolf || cd.nature == Nature.Vampire));
			Debug.Log ("Swapping " + deck [i].role.ToString () + " for " + instancePool [nextWovCard].role.ToString ());
			deck.RemoveAt (replacementIndeces[i]);
			deck.Insert (replacementIndeces[i], instancePool [nextWovCard]);
			instancePool.RemoveAt(nextWovCard);
		}
		return deck;
	}
}
