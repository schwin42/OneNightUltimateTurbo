using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

[System.Serializable]
public class SymClient : MonoBehaviour, IClient {
	
	public string UserId { 
		get { 
			return selfUserId; 
		}
	}
	public string selfUserId;

	public string accessKey;
	public string roomKey;

	public List<string> connectedUsers;

	public List<Role> selectedDeckBlueprint;

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

	public delegate void ClientHandler(SymClient client);
	public event ClientHandler OnEnteredRoom;
	public delegate void UserIdHandler(string userId);
	public event UserIdHandler OnUserConnected;

	public void Start() {
		if(!hasInitialized) {
			_ui = GetComponent<PlayerUi>();
			_ui.Initialize(this);
			hasInitialized = true;
		}
	}

	public void InitiateGame() {
		int randomSeed = Mathf.FloorToInt(UnityEngine.Random.value * 10000000);
		SymRemoteConnector.instance.StartGame(this, new StartGamePayload(randomSeed));
	}

	public void HandleSessionStarted(string userId, string accessKey, string roomKey) {
		Debug.Log(userId + ": received handle session started");
		this.selfUserId = userId;
		connectedUsers.Add (selfUserId);
		this.accessKey = accessKey;
		this.roomKey = roomKey;
//		playerNamesByUserId = basket.playerNamesByClientId;
		ui.HandleEnteredRoom(connectedUsers, roomKey);

		if(OnEnteredRoom != null) {
			OnEnteredRoom.Invoke(this);
		}
	}

	public void HandleJoinedSession(string selfUserId, string accessKey, List<string> allUsers) {
		Debug.Log(selfUserId + ": received joined session with " + allUsers.Count + " users in room");
		this.selfUserId = selfUserId;
		this.accessKey = accessKey;
		this.connectedUsers = allUsers;
		ui.HandleEnteredRoom(connectedUsers, roomKey);

		if(OnEnteredRoom != null) {
			OnEnteredRoom.Invoke(this);
		}
	}

	public void HandleOtherJoined(string userId) {
		print (selfUserId + ": received handle other for " + userId);
		connectedUsers.Add (userId);
		ui.HandlePlayersUpdated (connectedUsers);
		if(OnUserConnected != null) {
			OnUserConnected.Invoke(userId);
		}
	}

	public void HandleStartGamePayload(int randomSeed) {
		if (!(gm == null || gm.currentPhase == GameMaster.GamePhase.Result)) {
			Debug.LogError ("Unable to start game. Game already in progress.");
			return;
		}
		gm = new GameMaster(ui); //Implement random seed
		selectedDeckBlueprint = DeckGenerator.GenerateRandomizedDeck(connectedUsers.Count + 3, randomSeed, true);

//		selectedDeckBlueprint = new List<Role>() { Role.Insomniac, Role.Villager, Role.Villager, Role.Werewolf, Role.Insomniac, Role.Troublemaker };
		selectedDeckBlueprint = new List<Role>() { Role.Robber, Role.MysticWolf, Role.Troublemaker, Role.Drunk, Role.Seer, Role.ApprenticeSeer };

		selectedDeckBlueprint = Utility.ShuffleListBySeed (selectedDeckBlueprint, randomSeed);
		connectedUsers = connectedUsers.OrderBy(s => s).ToList();
		gm.StartGame (connectedUsers, new GameSettings (selectedDeckBlueprint));
	}

	public void HandleActionMessage(string userId, int[][] selection) {
		print("user id: " + userId);
		Debug.Log(userId + " received action message: " + selection);
		gm.ReceiveNightAction(userId, selection);
	}

	public void HandleVoteMessage(string userId, int votee) {
		Debug.Log(userId + " received vote message: " + votee);
		gm.ReceiveVote(userId, votee);
	}

	public void HandleRemoteError(ErrorType error) {
		switch(error) {
			case ErrorType.UnableToAuthenticate: //TODO Throw error dialog
				Debug.LogWarning ("Unable to authenticate. Check your room key.");
				break;
		}
	}

//	public void HandleRemotePayload(RemotePayload payload) {
////		Debug.Log("self: " + selfClientId);
//		//If game event, pass to GameMaster
//		if(payload is GamePayload) {
//			gm.ReceiveDirective((GamePayload)payload);
//		} else if(payload is WelcomeBasketPayload) { 
//
//		} else if(payload is UpdateOtherPayload) {
//			UpdateOtherPayload update = ((UpdateOtherPayload)payload);
//			this.playerNamesByUserId = update.playerNamesByClientId;
////			Debug.Log("Update other payload received by " + this.selfClientId + ": source, players, ids: " + this.playerNames.Count + ", " + this.playerNames.Count);
//			ui.HandlePlayersUpdated(playerNamesByUserId.Select(kp => kp.Value).ToList());
////			print("update other for " + this.PlayerName + ". Player names: " + playerNames.Count);
//		} else if (payload is StartGamePayload) {
//			Debug.Log("Start game received by: " + selfUserId);
//			}
//		} else {
//			Debug.LogError("Unexpected payload type: " + payload.ToString());
//		}
//	}

	public void BeginSession(string playerName) {
		connectedUsers = new List<string> ();
		SymRemoteConnector.instance.BeginSession(this, playerName);
	}

	public void JoinSession(string playerName, string roomKey)
	{
		connectedUsers = new List<string> ();
		SymRemoteConnector.instance.JoinSession(this, playerName, roomKey);
	}

	public void SubmitNightAction(int[][] selection) {
		SymRemoteConnector.instance.BroadcastMessage (this, new ActionPayload (selfUserId, selection)); 
	}

	public void SubmitVote(int locationId) {
		SymRemoteConnector.instance.BroadcastMessage (this, new VotePayload (selfUserId, locationId));
	}

	public void Disconnect() {
		SymRemoteConnector.instance.Disconnect (this);
	}
}
