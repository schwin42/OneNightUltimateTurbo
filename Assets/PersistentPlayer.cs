using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class PersistentPlayer {
	public string name;
	public EditorConnector connector;
	GameMaster gameMaster; //Game masters don't need to exist outside the scope of the game

	public List<Role> selectedDeckBlueprint;

	public PersistentPlayer() {
		connector = new EditorConnector(HandleRemotePayload);
	}

	public void SetName(string s) {
		name = s;
	}

	public void SetSelectedDeck(List<Role> deckBlueprint) {
		this.selectedDeckBlueprint = deckBlueprint;
	}

	public void BeginGame() {
		float randomSeed = Random.value; //Used to achieve deterministic consistency across clients
		connector.BroadcastEvent(new StartGamePayload(connector.selfClientId, randomSeed));
	}

	void HandleRemotePayload(RemotePayload payload) {
		Debug.Log("self: " + connector.selfClientId);
		//If game event, pass to GameMaster
		if(payload is GamePayload) {
			gameMaster.ReceiveDirective((GamePayload)payload);
		} else if(payload is WelcomeBasketPayload) { 
			WelcomeBasketPayload basket = ((WelcomeBasketPayload)payload);
			Debug.Log("Welcome basket received for : " + basket.sourceClientId);
			connector.selfClientId = basket.sourceClientId;
			Debug.Log("Self client id set to: " + connector.selfClientId);
			connector.connectedPlayersById = basket.connectedPlayersByClientId;
		} else if(payload is UpdateOtherPayload) {
			UpdateOtherPayload update = ((UpdateOtherPayload)payload);
			Debug.Log("Update other payload received by " + connector.selfClientId + ": source, count: " + update.sourceClientId.ToString() + ", " + update.connectedPlayersByClientId.Count);
			connector.connectedPlayersById = update.connectedPlayersByClientId;
		} else if (payload is StartGamePayload) {
			Debug.Log("Start game received by: " + connector.selfClientId);
			StartGamePayload start = ((StartGamePayload)payload);
			float randomSeed = start.randomSeed;
			gameMaster = new GameMaster(); //Implement random seed
			gameMaster.StartGame(connector.connectedPlayersById, selectedDeckBlueprint.ToArray(), true, randomSeed); //TODO Pass random seed
		} else {
			Debug.LogError("Unexpected payload type: " + payload.ToString());
		}

	}
}
