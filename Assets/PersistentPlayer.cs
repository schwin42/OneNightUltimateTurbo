using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class PersistentPlayer {

	public int clientId {
		get {
			return connector.selfClientId;
		}
	}

	string name;
	private RemoteConnector _connector;
	public RemoteConnector connector {
		get {
			return _connector;
		}
	}
	GameMaster gameMaster; //Game masters don't need to exist outside the scope of the game

	public List<Role> selectedDeckBlueprint;	

	public PersistentPlayer() {
		_connector = new EditorConnector(HandleRemotePayload);
		_connector.JoinSession();
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
		//If game event, pass to GameMaster
		if(payload is GamePayload) {
			gameMaster.ReceiveDirective((GamePayload)payload);
		} else if(payload is WelcomeBasketPayload) { 
			WelcomeBasketPayload basket = ((WelcomeBasketPayload)payload);
			connector.selfClientId = basket.sourceClientId;
			connector.connectedPlayers = basket.connectedPlayersByClientId;
		} else if(payload is UpdateOtherPayload) {
			UpdateOtherPayload update = ((UpdateOtherPayload)payload);
			connector.connectedPlayers = update.connectedPlayersByClientId;
		} else if (payload is StartGamePayload) {
			StartGamePayload start = ((StartGamePayload)payload);
			float randomSeed = start.randomSeed;
			gameMaster = new GameMaster(); //Implement random seed
			gameMaster.StartGame(connector.connectedPlayers, selectedDeckBlueprint.ToArray(), true, randomSeed); //TODO Pass random seed
		} else {
			Debug.LogError("Unexpected payload type: " + payload.ToString());
		}

	}
}
