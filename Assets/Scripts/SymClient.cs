using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class SymClient : MonoBehaviour, IClient {
	private string _playerName = null;
	public string PlayerName { 
		get { 
			return _playerName; 
		}
		set {
			_playerName = value;
		}
	}
	public string UserId { 
		get { 
			return selfUserId; 
		}
	}
	public string selfUserId;

	public string accessKey;
	public string roomKey;

	public List<Role> selectedDeckBlueprint;

	Dictionary<string, string> playerNamesByUserId;

	private PlayerUi _ui;
	public PlayerUi ui
	{
		get
		{
			return _ui;
		}
	}

	//State
	public GameMaster Gm { get { return gm; } }
	public GameMaster gm; //Game masters don't need to exist outside the scope of the game

	bool hasInitialized = false;

	public void Start() {
		if(!hasInitialized) {
			print("start");
			_ui = GetComponent<PlayerUi>();
			_ui.Initialize(this);
			hasInitialized = true;
		}
	}

	public void BeginGame() {
		float randomSeed = Random.value; //Used to achieve deterministic consistency across clients
		EditorSymConnector.instance.BroadcastMessage(this, new StartGamePayload(selfUserId, randomSeed));
	}

	public void HandleSessionStarted(string userId, string accessKey, string roomKey) {
		Debug.Log("received handle session for : " + userId);
		this.selfUserId = userId;
		this.accessKey = accessKey;
		this.roomKey = roomKey;
//		playerNamesByUserId = basket.playerNamesByClientId;
		ui.HandleEnteredRoom(PlayerName, roomKey);
	}

	public void HandleJoinedSession(string userId, string accessKey) {
		Debug.Log("received joined session for : " + userId);
		this.selfUserId = userId;
		this.accessKey = accessKey;
		ui.HandleEnteredRoom(PlayerName, roomKey);
	}


	public void HandleRemotePayload(RemotePayload payload) {
//		Debug.Log("self: " + selfClientId);
		//If game event, pass to GameMaster
		if(payload is GamePayload) {
			gm.ReceiveDirective((GamePayload)payload);
		} else if(payload is WelcomeBasketPayload) { 

		} else if(payload is UpdateOtherPayload) {
			UpdateOtherPayload update = ((UpdateOtherPayload)payload);
			this.playerNamesByUserId = update.playerNamesByClientId;
//			Debug.Log("Update other payload received by " + this.selfClientId + ": source, players, ids: " + this.playerNames.Count + ", " + this.playerNames.Count);
			ui.HandlePlayersUpdated(playerNamesByUserId.Select(kp => kp.Value).ToList());
//			print("update other for " + this.PlayerName + ". Player names: " + playerNames.Count);
		} else if (payload is StartGamePayload) {
			Debug.Log("Start game received by: " + selfUserId);
			if (!(gm == null || gm.currentPhase == GameMaster.GamePhase.Result)) {
				Debug.LogError ("Unable to start game. Game already in progress.");
				return;
			} else {
				StartGamePayload start = ((StartGamePayload)payload);
				int randomSeed = Mathf.FloorToInt(start.randomSeed * 1000000);
				gm = new GameMaster(ui); //Implement random seed

				//Get random deck and shuffle (using seed)
				selectedDeckBlueprint = DeckGenerator.GenerateRandomizedDeck(playerNamesByUserId.Count + 3, randomSeed, true);
				selectedDeckBlueprint = Utility.ShuffleListBySeed (selectedDeckBlueprint, randomSeed);

				//			selectedDeckBlueprint = new List<Role> { Role.ApprenticeSeer, Role.Drunk, Role.MysticWolf, Role.Robber, Role.Seer, Role.Troublemaker, Role.Villager, Role.Villager, Role.Werewolf } ;
//				selectedDeckBlueprint = new List<Role> { Role.Werewolf, Role.DreamWolf, Role.Insomniac, Role.Villager, Role.Werewolf, Role.Robber };
				//			selectedDeckBlueprint = new List<Role> { Role.Werewolf, Role.DreamWolf, Role.Insomniac, Role.Villager, Role.Villager };

				gm.StartGame(playerNamesByUserId, new GameSettings(selectedDeckBlueprint));
			}
		} else {
			Debug.LogError("Unexpected payload type: " + payload.ToString());
		}
	}

	public void BeginSession() {
		SymRemoteConnector.instance.BeginSession(this, this.PlayerName);
	}

	public void JoinSession(string roomKey)
	{
		SymRemoteConnector.instance.JoinSession(this, this.PlayerName, roomKey);
	}

	public void SubmitNightAction(int[][] selection) {
		SymRemoteConnector.instance.BroadcastMessage (this, new NightActionPayload (selfUserId, selection)); 
	}

	public void SubmitVote(int locationId) {
		SymRemoteConnector.instance.BroadcastMessage (this, new VotePayload (selfUserId, locationId));
	}

	public void Disconnect() {
		SymRemoteConnector.instance.Disconnect (this);
	}
}
