using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;

public class UnityNetworkConnector : RemoteConnector {

	public const short PORT = 7777;

	//State
	public Server localServer = null;
	public Dictionary<OnumClient, NetworkClient> networkClientsByOnumClients = new Dictionary<OnumClient, NetworkClient>();

	public override void BeginSession(OnumClient client, string playerName) {
		Debug.Log("Attempting to host room.");
		InitializeServer (client);
		InitializeLocalClient (client, playerName);
	}

	public override void JoinSession(OnumClient client, string hostAddress, string playerName) {
		InitializeClient (client, hostAddress, playerName);
	}

	public override void StartGame (OnumClient client, StartGamePayload payload) {
//		OnuBroadcastMessage(OnuMessage.StartGame, new StartGameMessage () { randomSeed = randomSeed });
		OnumBroadcastMessage(client, OnuMessage.StartGame, new StartGameMessage() { randomSeed = payload.randomSeed });
	}

	public override void BroadcastPayload(OnumClient client, RemotePayload payload) {
		short msgType;
		MessageBase message;
		if (payload is ActionPayload) {
			msgType = OnuMessage.NightAction;
			message = new NightActionMessage () {
				sourceUserId = client.selfUserId,
				selection = ((ActionPayload)payload).selection.Select (a => a.ToArray ()).ToArray ()
			};
		} else if (payload is VotePayload) {
			msgType = OnuMessage.Vote;
			message = new VoteMessage () {
				sourceUserId = client.selfUserId,
				voteeLocationId = ((VotePayload)payload).voteeLocationId
			};
		} else {
			Debug.LogError ("Unexpected payload type: " + payload);
			return;
		}

		OnumBroadcastMessage (client, msgType, message);
	}

	private void OnumBroadcastMessage(OnumClient client, short msgType, MessageBase message) {
		if (localServer != null) {
			NetworkServer.SendToAll (msgType, message);
		} else {
			networkClientsByOnumClients[client].Send (msgType, message);
		}
	}

	public override void Disconnect(OnumClient client) {
		//TODO Implement
	}

	private void InitializeServer(OnumClient client) {
		NetworkServer.Listen (PORT);
		NetworkServer.RegisterHandler (OnuMessage.Introduction, OnServerIntroductionReceived);
		NetworkServer.RegisterHandler (OnuMessage.StartGame, ServerEchoMessage);
		NetworkServer.RegisterHandler (OnuMessage.NightAction, ServerEchoMessage);
		NetworkServer.RegisterHandler (OnuMessage.Vote, ServerEchoMessage);
		//TODO Player disconnect
		localServer = new Server (client);
	}

	private void InitializeLocalClient(OnumClient onumClient, string playerName) {
		print ("Setting up local client");
		NetworkClient networkClient = ClientScene.ConnectLocalServer ();
		networkClientsByOnumClients.Add (onumClient, networkClient);
		SubscribeToClientMessages (networkClient, onumClient, playerName);
	}

	private void InitializeClient(OnumClient onumClient, string hostAddress, string playerName) {
		NetworkClient networkClient = new NetworkClient ();
		networkClientsByOnumClients.Add (onumClient, networkClient);
		SubscribeToClientMessages (networkClient, onumClient, playerName);
		print ("connecting to host: " + hostAddress + ":" + PORT);
		networkClient.Connect (hostAddress, PORT);
	}

	private void ServerEchoMessage(NetworkMessage netMessage) {
		print ("Server echoing message: " + netMessage.msgType);
		if (netMessage.msgType == OnuMessage.StartGame) {
			NetworkServer.SendToAll (netMessage.msgType, netMessage.ReadMessage<StartGameMessage> ());
		} else if (netMessage.msgType == OnuMessage.NightAction) {
			NetworkServer.SendToAll (netMessage.msgType, netMessage.ReadMessage<NightActionMessage> ());
		} else if (netMessage.msgType == OnuMessage.Vote) {
			NetworkServer.SendToAll (netMessage.msgType, netMessage.ReadMessage<VoteMessage> ());
		} else {
			Debug.LogError ("Unhandled message type: " + netMessage.msgType);
		}
	}

	private void SubscribeToClientMessages(NetworkClient networkClient, OnumClient onumClient, string playerName) {
		networkClient.RegisterHandler (MsgType.Connect, (networkMessage) => OnClientConnected(networkClient, networkMessage, playerName));
		networkClient.RegisterHandler (OnuMessage.Welcome, (networkMessage) => OnWelcomeReceived(onumClient, networkMessage));
		networkClient.RegisterHandler (OnuMessage.PlayersUpdated, (networkMessage) => OnPlayerUpdateReceived(onumClient, networkMessage));
		networkClient.RegisterHandler (OnuMessage.StartGame, (networkMessage) => OnStartGameRecieved(onumClient, networkMessage));
		networkClient.RegisterHandler (OnuMessage.NightAction, (networkMessage) => OnNightActionReceived(onumClient, networkMessage));
		networkClient.RegisterHandler (OnuMessage.Vote, (networkMessage) => OnVoteReceived(onumClient, networkMessage));
	}

	private void OnServerIntroductionReceived(NetworkMessage netMessage) {
		IntroductionMessage message = netMessage.ReadMessage<IntroductionMessage> ();
		string userId = message.playerName + ":" + Random.Range (0, 10000).ToString();
		localServer.connectedUserIds.Add(userId);

		netMessage.conn.Send (OnuMessage.Welcome, new WelcomeMessage () { userId = userId });
		NetworkServer.SendToAll (OnuMessage.PlayersUpdated, new PlayersUpdatedMessage () { userIds = localServer.connectedUserIds.ToArray()  });
	}

	private void OnClientConnected(NetworkClient client, NetworkMessage message, string playerName) {
		client.Send (OnuMessage.Introduction, new IntroductionMessage () { playerName = playerName });
	}

	private void OnWelcomeReceived(OnumClient client, NetworkMessage netMessage) {
		WelcomeMessage message = netMessage.ReadMessage<WelcomeMessage> ();
//		client.selfUserId = message.userId;
		if (client == localServer.associatedClient) {
			client.HandleSessionStarted (message.userId, null, Network.player.ipAddress);
		} else {
			client.HandleJoinedSession (message.userId, null, new List<string> { });
		}

	}

	private void OnPlayerUpdateReceived(OnumClient client, NetworkMessage netMessage) {
		PlayersUpdatedMessage message = netMessage.ReadMessage<PlayersUpdatedMessage> ();
		client.HandleOtherJoined (message.userIds);
	}

	private void OnStartGameRecieved(OnumClient client, NetworkMessage netMessage) {
		StartGameMessage message = netMessage.ReadMessage<StartGameMessage> ();
		client.HandleGameStarted (message.randomSeed);
	}

	private void OnNightActionReceived(OnumClient client, NetworkMessage netMessage) {
		print ("Night action received.");
		NightActionMessage message = netMessage.ReadMessage<NightActionMessage> ();
		client.HandleActionMessage (message.sourceUserId, message.selection);
	}

	private void OnVoteReceived(OnumClient client, NetworkMessage netMessage) {
		print ("Vote received");
		VoteMessage message = netMessage.ReadMessage<VoteMessage> ();
		client.HandleVoteMessage (message.sourceUserId, message.voteeLocationId);
	}

	public class Server {
		public OnumClient associatedClient;
		public List<string> connectedUserIds;

		public Server (OnumClient associatedClient) {
			this.associatedClient = associatedClient;
			this.connectedUserIds = new List<string>();
		}
	}
}
