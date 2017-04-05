using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class Utility {

	public static List<RealCard> ShuffleCards(List<RealCard> cards, int randomSeed) {
		System.Random random = new System.Random (randomSeed);
		Dictionary<double, RealCard> cardsByOrder = new Dictionary<double, RealCard>();
		for(int i = 0; i < cards.Count; i++) {
			cardsByOrder.Add (random.NextDouble (), cards [i]);
		}
		return cardsByOrder.OrderBy (kp => kp.Key).Select (kp => kp.Value).ToList ();
	}

	public static void MoveRoleToPosition(this List<CardData> list, Role target, int index) {
		CardData itemToMove = list.First(cd => cd.role == target);
		list.Remove(itemToMove);
		list.Insert(index, itemToMove);
	}

	public static string ToStringCardList(this List<CardData> list) {
		string outputString = "{ ";
		foreach (CardData card in list) {
			outputString += card.role.ToString ();
			outputString += ", ";
		}
		outputString += " }";
		return (outputString);
	}
}
