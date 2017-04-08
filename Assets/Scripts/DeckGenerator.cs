using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class DeckGenerator {

	public static Role[] GenerateRandomizedDeck(int cardCount, bool readyOnly) {
		List<CardData> instancePool;
		List<CardData> deck;
		List<int> replacementIndeces;
		for(int i = 0; i < 100; i++) { //Iterate to 100 instead of while loop, to prevent infinite loops
			deck = GenerateNewUnfixedDeck(cardCount, out instancePool, out replacementIndeces, readyOnly);
			int werewolfOrVampireCount = deck.Count(cd => cd.nature == Nature.Werewolf || cd.nature == Nature.Vampire);
			if (werewolfOrVampireCount < 2) {
				if(replacementIndeces.Count < 2 - werewolfOrVampireCount) {
					continue;
				} else {
					return ReplaceUnseededCardsWithWerewolfOrVampire(deck, instancePool, replacementIndeces, 2 - werewolfOrVampireCount).Select(cd => cd.role).ToArray();
				}
			} else {
				Debug.Log ("Deck already valid, no need to fix");
				return deck.Select(cd => cd.role).ToArray();
			}
		}
		Debug.LogError("Exceeded 100 attempts to generate valid deck");
		return null;
	}

	public static List<CardData> GenerateNewUnfixedDeck(int cardCount, List<CardData> inputInstancePool, out List<CardData> resultantInstancePool, out List<int> replacementIndeces) {

		replacementIndeces = new List<int>();
		List<CardData> deck = new List<CardData>();
		for(int i = 0; i < cardCount; i++) {
			if (i != cardCount - 1) {
				CardData card = inputInstancePool [0];
				deck.Add(card);
				Debug.Log ("Adding: " + card.role.ToString());
				inputInstancePool.RemoveAt(0);

				//If seed requirement does not exist, continue
				if(card.seedRequirement.isEmpty) continue;

				// Check if seed requirement already exists in the deck
				int index = card.seedRequirement.TryGetFirstIndex(deck);
				if(index != -1 && index != i) { //If seed req exists and deck and is not the current card
					Debug.Log("Seed requirement already exists in deck, continuing...");
					continue;
				}

				//Find seed requirement from pool and add
				int seedIndex = card.seedRequirement.TryGetFirstIndex(inputInstancePool);
				if(seedIndex != -1) {
					deck.Add (inputInstancePool [seedIndex]);
					Debug.Log("Adding " + card.role.ToString() + "'s seed requirement: " + inputInstancePool[seedIndex].role.ToString()); 
					inputInstancePool.RemoveAt (seedIndex);
					i++;
				} else {
					replacementIndeces.Add(i);
				}
			} else {
				//Move next card with no seed requirement from pool to deck
				int nextSeedIndex = inputInstancePool.IndexOf(inputInstancePool.First(cd => cd.seedRequirement.isEmpty));
				CardData card = inputInstancePool [nextSeedIndex];
				deck.Add (card);
				Debug.Log("Adding seedless card: " + card.role.ToString());
				inputInstancePool.RemoveAt (nextSeedIndex);
			}
		}
		resultantInstancePool = inputInstancePool;
		return deck;
	}

	private static List<CardData> GenerateNewUnfixedDeck(int cardCount, out List<CardData> resultantInstancePool, out List<int> replacementIndeces, bool readyOnly) {

		List<CardData> sourceCardPool = readyOnly ? GameData.instance.readyPool : GameData.instance.totalCardPool;
		List<CardData> newRandomInstancePool = sourceCardPool.OrderBy(x => Random.value).ToList();
		return GenerateNewUnfixedDeck(cardCount, newRandomInstancePool, out resultantInstancePool, out replacementIndeces); 
	}

	private static List<CardData> ReplaceUnseededCardsWithWerewolfOrVampire(List<CardData> deck, List<CardData> instancePool, List<int> replacementIndeces, int count) {
		//TODO Allow these cards to check seed requirements as well
		Debug.Log ("Replacing " + count + " cards with werewolves or vampires" );
		for (int i = 0; i < count; i++) {
			int nextWovCard = instancePool.IndexOf(instancePool.First (cd => cd.nature == Nature.Werewolf || cd.nature == Nature.Vampire));
			Debug.Log ("Swapping " + deck [i].role.ToString () + " for " + instancePool [nextWovCard].role.ToString ());
			deck.RemoveAt (replacementIndeces[i]);
			deck.Insert (replacementIndeces[i], instancePool [nextWovCard]);
			instancePool.RemoveAt(nextWovCard);
		}
		return deck;
	}
}
