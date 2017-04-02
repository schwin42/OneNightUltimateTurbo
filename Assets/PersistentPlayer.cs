using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class PersistentPlayer : MonoBehaviour{
	public string playerName;
	public int selfClientId = -1;

	public List<string> playerNames;
	public List<int> connectedClientIds;

	public EditorConnector connector;
	public GameMaster gameMaster; //Game masters don't need to exist outside the scope of the game
	public List<Role> selectedDeckBlueprint;

	public PersistentPlayer() {
		connector = new EditorConnector(this);
	}

	public void SetName(string s) {
		playerName = s;
	}

	public void SetSelectedDeck(List<Role> deckBlueprint) {
		this.selectedDeckBlueprint = deckBlueprint;
	}

	public void BeginGame() {
		float randomSeed = Random.value; //Used to achieve deterministic consistency across clients
		connector.BroadcastEvent(new StartGamePayload(selfClientId, randomSeed));
	}

	public void HandleRemotePayload(RemotePayload payload) {
//		Debug.Log("self: " + selfClientId);
		//If game event, pass to GameMaster
		if(payload is GamePayload) {
			gameMaster.ReceiveDirective((GamePayload)payload);
		} else if(payload is WelcomeBasketPayload) { 
			WelcomeBasketPayload basket = ((WelcomeBasketPayload)payload);
			Debug.Log("Welcome basket received for : " + basket.sourceClientId);
			this.selfClientId = basket.sourceClientId;
			Debug.Log("Self client id set to: " + selfClientId);
			playerNames = basket.playerNames;
			connectedClientIds = basket.clientIds;
//			StartCoroutine(SanityCheck(this));
		} else if(payload is UpdateOtherPayload) {
			UpdateOtherPayload update = ((UpdateOtherPayload)payload);
			this.playerNames = update.playerNames;
			this.connectedClientIds = update.clientIds;
			Debug.Log("Update other payload received by " + this.selfClientId + ": source, players, ids: " + this.playerNames.Count + ", " + this.playerNames.Count);

		} else if (payload is StartGamePayload) {
			Debug.Log("Start game received by: " + selfClientId);
			StartGamePayload start = ((StartGamePayload)payload);
			float randomSeed = start.randomSeed;
			gameMaster = new GameMaster(); //Implement random seed
			gameMaster.StartGame(playerNames, connectedClientIds, selectedDeckBlueprint.ToArray(), true, randomSeed); //TODO Pass random seed
			Debug.Log("Final self, names, ids: " + selfClientId + ", " + playerNames.Count + ", " + connectedClientIds.Count);
		} else {
			Debug.LogError("Unexpected payload type: " + payload.ToString());
		}
	}

//	IEnumerator SanityCheck(PersistentPlayer player) {
//		yield return new WaitForSeconds(1);
//
//		Debug.Log("self client id = " + player.selfClientId);
//	}
}
