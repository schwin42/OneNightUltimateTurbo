using System.Collections;
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
	public int selfClientId;
	public Server localServer = null;
	public GameMaster gameMaster; //Game masters don't need to exist outside the scope of the game
	public Role[] selectedDeckBlueprint = new Role[] { Role.Werewolf, Role.Minion, Role.Robber, Role.Troublemaker, Role.Insomniac, Role.Drunk };

	private PlayerUi _ui;
	public PlayerUi ui
	{
		get
		{
			return _ui;
		}
	}

	public void SetName(string s) {
		playerName = s;
	}

	public void BeginGame() {
		int randomSeed = Mathf.FloorToInt(Random.value * 1000000); //Used to achieve deterministic consistency across clients
		NetworkServer.SendToAll(OnuMessage.StartGame, new StartGameMessage() { randomSeed = randomSeed });
		//TODO Send event to self?
//		OnStartGameRecieved(
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

	public void HostRoom() {
		Debug.Log("Attempting to host room.");
		SetupServer ();
		SetupLocalClient ();
	}

	public void SubmitNightAction(Selection selection) {
		Debug.LogError ("Not implemented");
	}

	public void SubmitVote(int votee) {
		Debug.LogError ("Not implemented");
	}

	private void SubscribeToMessages(NetworkClient client) {
		client.RegisterHandler (MsgType.Connect, OnClientConnected);
		client.RegisterHandler (OnuMessage.PlayersUpdated, OnPlayerUpdateReceived);
		client.RegisterHandler (OnuMessage.StartGame, OnStartGameRecieved);
		client.RegisterHandler (OnuMessage.NightAction, OnNightActionReceived);
		client.RegisterHandler (OnuMessage.VotePayload, OnVoteReceived);
	}

	public void JoinRoom(string networkAddress) {
		SetupClient (networkAddress);
	}

	public void SendNameMessage(string s) {
		print ("Sending name message");
		PlayersUpdatedMessage message = new PlayersUpdatedMessage ();
		NetworkServer.SendToAll (OnuMessage.PlayersUpdated, message);
	}

	void Start() {
		_ui = GetComponent<PlayerUi>();
		_ui.Initialize(this);
	}

	private void SetupServer() {
		print ("Setting up server.");
		NetworkServer.Listen (PORT);
		NetworkServer.RegisterHandler (OnuMessage.Introduction, OnServerIntroductionReceived);
		localServer = new Server ();
	}

	private void SetupClient(string hostAddress) {
		print ("Setting up client");
		client = new NetworkClient ();
		SubscribeToMessages (client);
		if (Application.platform == RuntimePlatform.WindowsEditor) {
			print ("Connecting on windows");
			client.Connect ("127.0.0.1", PORT);
		} else if (Application.platform == RuntimePlatform.Android) {
			client.Connect (hostAddress, PORT);
		} else {
			Debug.LogError("Platform not implemented, you fuck.");
		}
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
		NetworkServer.SendToAll (OnuMessage.PlayersUpdated, new PlayersUpdatedMessage () { playerNamesByClientId =  localServer.playerNamesByClientId.ToArray() });
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
//		netMessage.
//		gameMaster.ReceiveNightAction(
	}

	private void OnVoteReceived(NetworkMessage netMessage) {
		print ("Vote received");
	}

	public class Server {
		public List<string> playerNamesByClientId;

		public Server () {
			this.playerNamesByClientId = new List<string>();
		}
	}
}
