using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Networking;

[System.Serializable]
public class AsymClient : MonoBehaviour{
	public string playerName;
	public int selfClientId = -1;

	public List<string> playerNames;
	public List<int> connectedClientIds;

	//Configuration
//	private EditorAsymConnector _connector;
//	public EditorAsymConnector connector
//	{
//		get
//		{
//			return _connector;
//		}
//	}

	private PlayerUi _ui;
	public PlayerUi ui
	{
		get
		{
			return _ui;
		}
	}

	//State
	public GameMaster gameMaster; //Game masters don't need to exist outside the scope of the game
	private List<Role> selectedDeckBlueprint = new List<Role> { Role.Robber, Role.Werewolf, Role.Troublemaker, Role.Werewolf, Role.Villager, Role.Villager };

//	public AsymClient() {
//		_connector = new EditorAsymConnector(this);
//	}

	public void SetName(string s) {
		playerName = s;
	}

	public void SetSelectedDeck(List<Role> deckBlueprint) {
		this.selectedDeckBlueprint = deckBlueprint;
	}

	public void BeginGame() {
		float randomSeed = Random.value; //Used to achieve deterministic consistency across clients
//		connector.BroadcastEvent(new StartGamePayload(selfClientId, randomSeed));
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
			ui.HandlePlayersUpdated(playerNames);
			print("welcome basket for " + playerName + ". Player names: " + playerNames.Count);
		} else if(payload is UpdateOtherPayload) {
			UpdateOtherPayload update = ((UpdateOtherPayload)payload);
			this.playerNames = update.playerNames;
			this.connectedClientIds = update.clientIds;
			Debug.Log("Update other payload received by " + this.selfClientId + ": source, players, ids: " + this.playerNames.Count + ", " + this.playerNames.Count);
			ui.HandlePlayersUpdated(playerNames);
			print("update other for " + playerName + ". Player names: " + playerNames.Count);
		} else if (payload is StartGamePayload) {
			Debug.Log("Start game received by: " + selfClientId);
			StartGamePayload start = ((StartGamePayload)payload);
			int randomSeed = Mathf.FloorToInt(start.randomSeed * 1000000);
			gameMaster = new GameMaster(ui); //Implement random seed
			gameMaster.StartGame(playerNames, connectedClientIds, selectedDeckBlueprint.ToArray(), true, randomSeed);
		} else {
			Debug.LogError("Unexpected payload type: " + payload.ToString());
		}
	}

	public void HostRoom() {
		Debug.Log("Hosting room");
		NetworkClient client = NetworkManager.singleton.StartHost();
	}

	public void JoinRoom()
	{
//		connector.JoinSession(playerName);
	}

	public void SubmitNightAction(Selection selection) {
//		connector.BroadcastEvent (new NightActionPayload (selfClientId, selection)); 
	}

	public void SubmitVote(int locationId) {
//		connector.BroadcastEvent (new VotePayload (selfClientId, locationId));
	}

	void Start()
	{
		_ui = GetComponent<PlayerUi>();
//		_ui.Initialize(this);

		HostRoom();
	}
}
