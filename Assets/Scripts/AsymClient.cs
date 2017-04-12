using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Networking;

[System.Serializable]
public class AsymClient : MonoBehaviour, IClient {

	public string RoomKey { get { return null; } }
	public const short PORT = 7777;
	private string _playerName = null;
	public string PlayerName {
		get {
			return _playerName;
		}
		set {
			_playerName = value;
		}
	}
	public Dictionary<string, string> playerNamesByUserIds;
	public NetworkClient client;
	public string UserId { get { return selfUserId; } }
	public string selfUserId;
	public Server localServer = null;
	public GameMaster Gm { get { return gm; } }
	public GameMaster gm; //Game masters don't need to exist outside the scope of the game

	private PlayerUi _ui;
	public PlayerUi ui
	{
		get
		{
			return _ui;
		}
	}

	public void BeginSession(string name) {
		Debug.Log("Attempting to host room.");
		SetupServer ();
		SetupLocalClient ();
	}

	public void JoinSession(string name, string networkAddress) {
		Debug.LogError("I broke it.");
//		SetupClient (networkAddress);
	}

	public void InitiateGame() {
		print ("Begin game");
		int randomSeed = Mathf.FloorToInt(Random.value * 1000000); //Used to achieve deterministic consistency across clients

		OnuBroadcastMessage(OnuMessage.StartGame, new StartGameMessage () { randomSeed = randomSeed });
	}

	public void SubmitNightAction(int[][] selection) {
		Debug.Log ("Sending night action");
		OnuBroadcastMessage (OnuMessage.NightAction, new NightActionMessage () { sourceUserId = selfUserId, selection = selection.Select(a => a.ToArray()).ToArray() });
	}

	public void SubmitVote(int votee) {
		Debug.Log ("Sending vote");
		OnuBroadcastMessage (OnuMessage.VotePayload, new VoteMessage () { sourceUserId = selfUserId, voteeLocationId = votee });
	}

	private void OnuBroadcastMessage(short msgType, MessageBase message) {
		if (localServer != null) {
			NetworkServer.SendToAll (msgType, message);
		} else {
			client.Send (msgType, message);
		}
	}

	public void Disconnect() {
		//TODO Implement
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
		Debug.LogError("Busted");
//		ui.HandleSessionStarted (this.PlayerName);
		client.Send (OnuMessage.Introduction, new IntroductionMessage () { playerName = this.PlayerName });
	}

	private void OnServerIntroductionReceived(NetworkMessage netMessage) {
		IntroductionMessage message = netMessage.ReadMessage<IntroductionMessage> ();
		print ("Introduction received by server: " + message.playerName);
		localServer.playerNamesByClientId.Add(message.playerName);

		Debug.LogError("Done broke it.");
//		netMessage.conn.Send (OnuMessage.Welcome, new WelcomeMessage () { userId = localServer.playerNamesByClientId.Count - 1 });
		NetworkServer.SendToAll (OnuMessage.PlayersUpdated, new PlayersUpdatedMessage () { playerNamesByClientId =  localServer.playerNamesByClientId.ToArray() });
	}

	private void OnWelcomeReceived(NetworkMessage netMessage) {
		print ("Welcome received.");
		WelcomeMessage message = netMessage.ReadMessage<WelcomeMessage> ();
		selfUserId = message.userId;
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
//		clientPlayerNamesByClientIds = message.playerNamesByClientId;
		playerNamesByUserIds = new Dictionary<string, string>();

		Debug.LogError("Broke it.");
//		for(int i = 0; i < message.playerNamesByClientId.Length; i++) {
//			playerNamesByUserIds.Add(i, message.playerNamesByClientId[i]);
//		}

		ui.HandlePlayersUpdated (message.playerNamesByClientId.ToList());
	}

	private void OnStartGameRecieved(NetworkMessage netMessage) {
		print ("Start game received.");
		StartGameMessage message = netMessage.ReadMessage<StartGameMessage> ();
		gm = new GameMaster(ui); //Implement random seed
		Debug.LogError("I BROKE IT, OK??");
//		List<Role> selectedDeckBlueprint = DeckGenerator.GenerateRandomizedDeck(playerNamesByUserIds.Count + 3, message.randomSeed, true).ToList();
//		gm.StartGame(playerNamesByUserIds, new GameSettings(selectedDeckBlueprint));
	}

	private void OnNightActionReceived(NetworkMessage netMessage) {
		print ("Night action received.");
		NightActionMessage message = netMessage.ReadMessage<NightActionMessage> ();
		gm.ReceiveNightAction (message.sourceUserId, message.selection);
	}

	private void OnVoteReceived(NetworkMessage netMessage) {
		print ("Vote received");
		VoteMessage message = netMessage.ReadMessage<VoteMessage> ();
		gm.ReceiveVote (message.sourceUserId, message.voteeLocationId);
	}

	public class Server {
		public List<string> playerNamesByClientId;

		public Server () {
			this.playerNamesByClientId = new List<string>();
		}
	}
}
