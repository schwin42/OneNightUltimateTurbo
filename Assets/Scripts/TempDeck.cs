using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TempDeck : MonoBehaviour {

	public int cardsInDeck;
	public List<CardData> deck;

	void Start() {
//		for(int i = 0; i < 100; i++) {
//			deck = DeckGenerator.GenerateRandomizedDeck(cardsInDeck);


//			List<CardData> instancePool = GameData.instance.cardPool.OrderBy(x => Random.value).ToList();
//			instancePool.MoveRoleToPosition(Role.Werewolf, 0);
//			instancePool.MoveRoleToPosition(Role.Werewolf, 1);
//			List<CardData> resultantInstancePool;
//			List<int> replacementIndeces;
//			deck = DeckGenerator.GenerateNewUnfixedDeck(cardsInDeck, instancePool, out resultantInstancePool, out replacementIndeces);
//			Debug.Log(deck.ToStringCardList());
//			Debug.Log("deck's mason count: " + deck.Count(cd => cd.role == Role.Mason));
//		}


	}


}
