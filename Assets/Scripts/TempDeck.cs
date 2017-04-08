using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TempDeck : MonoBehaviour {

	public int cardsInDeck;
	public Role[] deck;

	void Start() {
//		for(int i = 0; i < 100; i++) {
//			deck = DeckGenerator.GenerateRandomizedDeck(cardsInDeck);

		deck = DeckGenerator.GenerateRandomizedDeck(8, true);
		print(deck.ToString());

//		}


	}


}
