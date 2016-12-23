using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TempDeck : MonoBehaviour {

	public int cardsInDeck;
	public List<CardData> deck;

	void Start() {
		deck = DeckGenerator.GenerateRandomizedDeck(cardsInDeck);
		string finalDeckString = "Final deck: ";
		foreach (CardData card in deck) {
			finalDeckString += card.role.ToString ();
			finalDeckString += ", ";
		}
		print (finalDeckString);
	}
}
