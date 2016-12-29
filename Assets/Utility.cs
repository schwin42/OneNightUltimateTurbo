using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class Utility {
	
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
