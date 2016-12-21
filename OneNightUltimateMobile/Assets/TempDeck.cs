using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TempDeck : MonoBehaviour {

	public int cardsInDeck;
	public List<Card> deck;

	void Start() {
		deck = GameController.GenerateRandomDeck(cardsInDeck);
	}
}
