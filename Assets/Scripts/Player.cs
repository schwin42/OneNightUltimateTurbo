using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;


[System.Serializable]
public class Player : ILocation
{
	//Assigned in order

	//0. Game init_namezation
	private int _locationId = -1;
	public int locationId { 
		get {
			return _locationId;
		}
	}

	private string _name;
	public string name { 
		get {
			return _name;
		}
	}

	//1. Deal cards
	public RealCard dealtCard;

	//2. Display prompts
	public int[] cohortLocations;
	public RealizedPrompt prompt;

	//3. Collect night actions - one selection per night action, in corresponding order
	public Selection nightLocationSelection;	

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

	//5. Notify seers
	public List<Observation> observations;

	//6. Enable voting
	public int locationIdVote;

	//7. Result
	public bool killed = false;
	public bool didWin;

	//	public Role originalRole;

	//	public Mark currentMark;
	//	public Artifact currentArtifact;

	public Player (string playerName)
	{
		this._name = playerName;

		this._locationId = GameController.RegisterLocation(this);
		Debug.Log("Registered player " + playerName + " as locationId = " + locationId);
		this.observations = new List<Observation>();
	}

	public void ReceiveDealtCard(RealCard card) {
		this.dealtCard = card;
		this._currentCard = card;
	}
}
