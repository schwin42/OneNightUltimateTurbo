﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

[System.Serializable]
public class OnutClient : MonoBehaviour, IClient
{

	public const string VERSION = "1.0.5";

	public string UserId { 
		get { 
			return selfUserId; 
		}
	}

	public string selfUserId;

	public string accessKey;

	public string RoomKey { get { return roomKey; } }

	private string roomKey;

	public List<string> connectedUsers;

	public List<Role> selectedDeckBlueprint;

	private PlayerUi _ui;

	public PlayerUi ui {
		get {
			return _ui;
		}
	}

	//State
	public GameMaster Gm { get { return gm; } }

	public GameMaster gm;
	//Game masters don't need to exist outside the scope of the game

	bool hasInitialized = false;

	public delegate void ClientHandler (OnutClient client);

	public event ClientHandler OnEnteredRoom;
	//	public delegate void UserIdHandler(string userId);
	//	public event UserIdHandler OnUserConnected;

	public void Start ()
	{
		if (!hasInitialized) {
			_ui = GetComponent<PlayerUi> ();
			_ui.Initialize (this);
			hasInitialized = true;
		}
	}

	public void InitiateGame ()
	{
		int randomSeed = Mathf.FloorToInt (UnityEngine.Random.value * 10000000);
		RemoteConnector.instance.StartGame (this, new StartGamePayload (randomSeed));
	}

	public void HandleSessionStarted (string userId, string accessKey, string roomKey)
	{
		Debug.Log (userId + ": received handle session started at " + roomKey);
		this.selfUserId = userId;
		connectedUsers.Add (selfUserId);
		this.accessKey = accessKey;
		this.roomKey = roomKey;
//		playerNamesByUserId = basket.playerNamesByClientId;
		ui.HandleEnteredRoom (connectedUsers);

		if (OnEnteredRoom != null) {
			OnEnteredRoom.Invoke (this);
		}
	}

	public void HandleJoinedSession (string selfUserId, string accessKey, List<string> allUsers)
	{
		Debug.Log (selfUserId + ": received joined session with " + allUsers.Count + " users in room");
		this.selfUserId = selfUserId;
		this.accessKey = accessKey;
		this.connectedUsers = allUsers;
		ui.HandleEnteredRoom (connectedUsers);

		if (OnEnteredRoom != null) {
			OnEnteredRoom.Invoke (this);
		}
	}

	//Additive implementation to cohere with node server API
	public void HandleOtherJoined (string userId)
	{
		print (selfUserId + ": received handle other for " + userId);
		connectedUsers.Add (userId);
		ui.HandlePlayersUpdated (connectedUsers);
//		if(OnUserConnected != null) {
//			OnUserConnected.Invoke(userId);
//		}
	}

	public void HandleOtherLeft (string userId)
	{
		print (selfUserId + " received other left for " + userId);
		if (gm == null) {
			connectedUsers.Remove (userId);
			ui.HandlePlayersUpdated (connectedUsers);
		} else {
			ui.ThrowError (userId.Split (':') [0] + " disconnected during game, returning to title.");
		}

	}

	//Preferred aggregative implementation
	public void HandleOtherJoined (string[] userIds)
	{
		print (selfUserId + ": received player update with player count: " + userIds.Length.ToString ());
		connectedUsers = userIds.ToList ();
		ui.HandlePlayersUpdated (connectedUsers);
//		if(OnUserConnected != null) {
//			OnUserConnected.Invoke(userId);
//		}
	}

	public void HandleGameStarted (int randomSeed)
	{
		if (!(gm == null || gm.currentPhase == GameMaster.GamePhase.Result)) {
			Debug.LogWarning ("Unable to start game. Game already in progress.");
			return;
		}
		gm = new GameMaster (ui); //Implement random seed
		selectedDeckBlueprint = DeckGenerator.GenerateRandomizedDeck (connectedUsers.Count + 3, randomSeed, true);

		selectedDeckBlueprint = Utility.ShuffleListBySeed (selectedDeckBlueprint, randomSeed);
//		selectedDeckBlueprint = new List<Role>() { Role.Hunter, Role.Werewolf, Role.Troublemaker, Role.Drunk, Role.Werewolf, Role.ApprenticeSeer };

		connectedUsers = connectedUsers.OrderBy (s => s).ToList ();
		gm.StartGame (connectedUsers, new GameSettings (selectedDeckBlueprint));
	}

	public void HandleActionMessage (string userId, int[][] selection)
	{
		print ("user id: " + userId);
		Debug.Log (userId + " received action message: " + selection);
		gm.ReceiveNightAction (userId, selection);
	}

	public void HandleVoteMessage (string userId, int votee)
	{
		Debug.Log (userId + " received vote message: " + votee);
		gm.ReceiveVote (userId, votee);
	}

	public void HandleRemoteError (ErrorType error, string s = null)
	{
		switch (error) {
		case ErrorType.UnableToAuthenticate: //TODO Throw error dialog
			ui.ThrowError (s);
			ui.ThrowError ("Invalid room key, dingus");
			break;
		case ErrorType.Generic:
			s = "Connection error: " + s;
			print(s);
			ui.ThrowError (s);
			break;
		}
	}

	public void BeginSession (string playerName)
	{
		connectedUsers = new List<string> ();
		RemoteConnector.instance.BeginSession (this, playerName);
	}

	public void JoinSession (string playerName, string roomKey)
	{
		this.roomKey = roomKey;
		connectedUsers = new List<string> ();
		RemoteConnector.instance.JoinSession (this, playerName, roomKey);
	}

	public void SubmitNightAction (int[][] selection)
	{
		RemoteConnector.instance.BroadcastPayload (this, new ActionPayload (selfUserId, selection)); 
	}

	public void SubmitVote (int locationId)
	{
		RemoteConnector.instance.BroadcastPayload (this, new VotePayload (selfUserId, locationId));
	}

	public void Disconnect ()
	{
		RemoteConnector.instance.Disconnect (this);
	}
}
