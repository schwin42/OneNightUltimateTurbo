using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;


[System.Serializable]
public class GamePlayer : ILocation
{
	//Assigned in order

	//0. Game init
	private string _userId;
	public string userId {
		get {
			return _userId;
		}
	}

	public string name {
		get {
			return userId.Split (':') [0];
		}
	}

	private int _locationId = -1;
	public int locationId { 
		get {
			return _locationId;
		}
	}

	//1. Deal cards
	public RealCard dealtCard;

	//2. Display prompts
	public RealizedPrompt prompt;

	//3. Collect night actions - one selection per night action, in corresponding order
	public int[][] nightLocationSelection;	

	//4. Manipulate cards
	private RealCard _currentCard;
	public RealCard currentCard { get {
			return _currentCard;
		}
		set {
			_currentCard = value;
		}
	}
	//public Mark currentMark;
	//TODO To accommodate marks, add team, nature, and win condition properties to derive status from card and mark (mark overrides in  most if not all cases)

	//5. Notify seers
	public List<Observation> observations;

	//6. Enable voting
	public int votedLocation;

	//7. Result
	public bool killed = false;
	public bool didWin;

	//	public Role originalRole;

	//	public Mark currentMark;
	//	public Artifact currentArtifact;

	public GamePlayer (GameMaster gameMaster, string userId)
	{
		this._userId = userId;

		this._locationId = gameMaster.RegisterLocation(this);
		this.observations = new List<Observation>();
	}

	public void ReceiveDealtCard(RealCard card) {
		this.dealtCard = card;
		this._currentCard = card;
	}
}
