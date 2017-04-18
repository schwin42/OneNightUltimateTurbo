using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSettings {
	
	public List<Role> deckList;
	public int gameTimer = 300; //In seconds

	public GameSettings (List<Role> deckList) {
		this.deckList = deckList;
	}
}
