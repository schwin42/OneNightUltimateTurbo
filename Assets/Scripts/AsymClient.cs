﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Networking;

[System.Serializable]
public class AsymClient : MonoBehaviour {
	public const short PORT = 7777;
	public string playerName;
	public List<string> clientPlayerNamesByClientIds;
	public NetworkClient client;
	public int selfClientId = -1;
	public Server localServer = null;
	public GameMaster gameMaster; //Game masters don't need to exist outside the scope of the game
	public Role[] selectedDeckBlueprint = new Role[] { Role.Werewolf, Role.Werewolf, Role.Minion, Role.Robber, Role.Troublemaker, Role.Insomniac, Role.Drunk, Role.Mason, Role.Mason };

	private PlayerUi _ui;
	public PlayerUi ui
	{
		get
		{
			return _ui;
		}
	}

//	public void HandleRemotePayload(RemotePayload payload) {
//		Debug.LogError("Handle remote payload done broke.");
////		//If game event, pass to GameMaster
////		if(payload is GamePayload) {
////			gameMaster.ReceiveDirective((GamePayload)payload);
////		} else if(payload is WelcomeBasketPayload) { 
////			WelcomeBasketPayload basket = ((WelcomeBasketPayload)payload);
////			Debug.Log("Welcome basket received for : " + basket.sourceClientId);
////			this.selfClientId = basket.sourceClientId;
////			Debug.Log("Self client id set to: " + selfClientId);
////			playerNames = basket.playerNames;
////			connectedClientIds = basket.clientIds;
////			ui.HandlePlayersUpdated(playerNames);
////			print("welcome basket for " + playerName + ". Player names: " + playerNames.Count);
////		} else if(payload is UpdateOtherPayload) {
////			UpdateOtherPayload update = ((UpdateOtherPayload)payload);
////			this.playerNames = update.playerNames;
////			this.connectedClientIds = update.clientIds;
////			Debug.Log("Update other payload received by " + this.selfClientId + ": source, players, ids: " + this.playerNames.Count + ", " + this.playerNames.Count);
////			ui.HandlePlayersUpdated(playerNames);
////			print("update other for " + playerName + ". Player names: " + playerNames.Count);
////		} else if (payload is StartGamePayload) {
////			Debug.Log("Start game received by: " + selfClientId);
////			StartGamePayload start = ((StartGamePayload)payload);
////			int randomSeed = Mathf.FloorToInt(start.randomSeed * 1000000);
////			gameMaster = new GameMaster(ui); //Implement random seed
////			gameMaster.StartGame(playerNames, connectedClientIds, selectedDeckBlueprint.ToArray(), true, randomSeed);
////		} else {
////			Debug.LogError("Unexpected payload type: " + payload.ToString());
////		}
//	}

	public void SetName(string s) {
		this.playerName = s;
	}

	public void HostRoom() {
		Debug.Log("Attempting to host room.");
		SetupServer ();
		SetupLocalClient ();
	}

	public void JoinRoom(string networkAddress) {
		SetupClient (networkAddress);
	}

	public void BeginGame() {
		print ("Begin game");
		int randomSeed = Mathf.FloorToInt(Random.value * 1000000); //Used to achieve deterministic consistency across clients

		OnuBroadcastMessage(OnuMessage.StartGame, new StartGameMessage () { randomSeed = randomSeed });
	}

	public void SubmitNightAction(Selection selection) {
		Debug.Log ("Sending night action");
		OnuBroadcastMessage (OnuMessage.NightAction, new NightActionMessage () { sourceClientId = selfClientId, selection = selection.locationIds });
	}

	public void SubmitVote(int votee) {
		Debug.Log ("Sending vote");
		OnuBroadcastMessage (OnuMessage.VotePayload, new VoteMessage () { sourceClientId = selfClientId, voteeLocationId = votee });
	}

	private void OnuBroadcastMessage(short msgType, MessageBase message) {
		if (localServer != null) {
			NetworkServer.SendToAll (msgType, message);
		} else {
			client.Send (msgType, message);
		}
	}

	private void SubscribeToMessages(NetworkClient client) {
		client.RegisterHandler (MsgType.Connect, OnClientConnected);
		client.RegisterHandler (OnuMessage.Welcome, OnWelcomeReceived);
		client.RegisterHandler (OnuMessage.PlayersUpdated, OnPlayerUpdateReceived);
		client.RegisterHandler (OnuMessage.StartGame, OnStartGameRecieved);
		client.RegisterHandler (OnuMessage.NightAction, OnNightActionReceived);
		client.RegisterHandler (OnuMessage.VotePayload, OnVoteReceived);
	}

	void Start() {
		_ui = GetComponent<PlayerUi>();
		_ui.Initialize(this);
	}

	private void SetupServer() {
		print ("Setting up server.");
		NetworkServer.Listen (PORT);
		NetworkServer.RegisterHandler (OnuMessage.Introduction, OnServerIntroductionReceived);
		NetworkServer.RegisterHandler (OnuMessage.StartGame, ServerEchoMessage);
		NetworkServer.RegisterHandler (OnuMessage.NightAction, ServerEchoMessage);
		NetworkServer.RegisterHandler (OnuMessage.VotePayload, ServerEchoMessage);
		//TODO Player disconnect
		localServer = new Server ();
	}

	private void SetupClient(string hostAddress) {
		print ("Setting up client");
		client = new NetworkClient ();
		SubscribeToMessages (client);
		client.Connect (hostAddress, PORT);
	}

	private void SetupLocalClient() {
		print ("Setting up local client");
		client = ClientScene.ConnectLocalServer ();
		SubscribeToMessages (client);
	}

	private void OnClientConnected(NetworkMessage message) {
		Debug.Log ("Client connected, sending introduction");
		ui.HandleClientJoined (playerName);
		client.Send (OnuMessage.Introduction, new IntroductionMessage () { playerName = playerName });
	}

	private void OnServerIntroductionReceived(NetworkMessage netMessage) {
		IntroductionMessage message = netMessage.ReadMessage<IntroductionMessage> ();
		print ("Introduction received by server: " + message.playerName);
		localServer.playerNamesByClientId.Add(message.playerName);

		netMessage.conn.Send (OnuMessage.Welcome, new WelcomeMessage () { clientId = localServer.playerNamesByClientId.Count - 1 });
		NetworkServer.SendToAll (OnuMessage.PlayersUpdated, new PlayersUpdatedMessage () { playerNamesByClientId =  localServer.playerNamesByClientId.ToArray() });
	}

	private void OnWelcomeReceived(NetworkMessage netMessage) {
		print ("Welcome received.");
		WelcomeMessage message = netMessage.ReadMessage<WelcomeMessage> ();
		selfClientId = message.clientId;
	}

	private void ServerEchoMessage(NetworkMessage netMessage) {
		print ("Server echoing message: " + netMessage.msgType);
		if (netMessage.msgType == OnuMessage.StartGame) {
			NetworkServer.SendToAll (netMessage.msgType, netMessage.ReadMessage<StartGameMessage> ());
		} else if (netMessage.msgType == OnuMessage.NightAction) {
			NetworkServer.SendToAll (netMessage.msgType, netMessage.ReadMessage<NightActionMessage> ());
		} else if (netMessage.msgType == OnuMessage.VotePayload) {
			NetworkServer.SendToAll (netMessage.msgType, netMessage.ReadMessage<VoteMessage> ());
		} else {
			Debug.LogError ("Unhandled message type: " + netMessage.msgType);
		}
	}

	private void OnPlayerUpdateReceived(NetworkMessage netMessage) {
		print ("Received players updated message");
		PlayersUpdatedMessage message = netMessage.ReadMessage<PlayersUpdatedMessage> ();
		clientPlayerNamesByClientIds = message.playerNamesByClientId.ToList();
		ui.HandlePlayersUpdated (clientPlayerNamesByClientIds);
	}

	private void OnStartGameRecieved(NetworkMessage netMessage) {
		print ("Start game received.");
		StartGameMessage message = netMessage.ReadMessage<StartGameMessage> ();
		gameMaster = new GameMaster(ui); //Implement random seed
		gameMaster.StartGame(clientPlayerNamesByClientIds, selectedDeckBlueprint, true, message.randomSeed);
	}

	private void OnNightActionReceived(NetworkMessage netMessage) {
		print ("Night action received.");
		NightActionMessage message = netMessage.ReadMessage<NightActionMessage> ();
		gameMaster.ReceiveNightAction (message.sourceClientId, new Selection (message.selection));
	}

	private void OnVoteReceived(NetworkMessage netMessage) {
		print ("Vote received");
		VoteMessage message = netMessage.ReadMessage<VoteMessage> ();
		gameMaster.ReceiveVote (message.sourceClientId, message.voteeLocationId);
	}

	public class Server {
		public List<string> playerNamesByClientId;

		public Server () {
			this.playerNamesByClientId = new List<string>();
		}
	}
}
