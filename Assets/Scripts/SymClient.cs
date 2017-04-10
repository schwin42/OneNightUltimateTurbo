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
	public int ClientId { 
		get { 
			return selfClientId; 
		}
	}
	public int selfClientId = -1;

	public List<Role> selectedDeckBlueprint;

	Dictionary<int, string> playerNamesByClientId;

	//Configuration
	private EditorSymConnector _connector;
	public EditorSymConnector connector
	{
		get
		{
			return _connector;
		}
	}

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

	public SymClient() {
		_connector = new EditorSymConnector(this);
	}

	public void BeginGame() {
		float randomSeed = Random.value; //Used to achieve deterministic consistency across clients
		connector.BroadcastEvent(new StartGamePayload(selfClientId, randomSeed));
	}

	public void HandleRemotePayload(RemotePayload payload) {
//		Debug.Log("self: " + selfClientId);
		//If game event, pass to GameMaster
		if(payload is GamePayload) {
			gm.ReceiveDirective((GamePayload)payload);
		} else if(payload is WelcomeBasketPayload) { 
			WelcomeBasketPayload basket = ((WelcomeBasketPayload)payload);
			Debug.Log("Welcome basket received for : " + basket.sourceClientId);
			this.selfClientId = basket.sourceClientId;
			Debug.Log("Self client id set to: " + selfClientId);
			playerNamesByClientId = basket.playerNamesByClientId;
			ui.HandleClientJoined(PlayerName);
			ui.HandlePlayersUpdated(playerNamesByClientId.Select(kp => kp.Value).ToList());
		} else if(payload is UpdateOtherPayload) {
			UpdateOtherPayload update = ((UpdateOtherPayload)payload);
			this.playerNamesByClientId = update.playerNamesByClientId;
//			Debug.Log("Update other payload received by " + this.selfClientId + ": source, players, ids: " + this.playerNames.Count + ", " + this.playerNames.Count);
			ui.HandlePlayersUpdated(playerNamesByClientId.Select(kp => kp.Value).ToList());
//			print("update other for " + this.PlayerName + ". Player names: " + playerNames.Count);
		} else if (payload is StartGamePayload) {
			Debug.Log("Start game received by: " + selfClientId);
			StartGamePayload start = ((StartGamePayload)payload);
			int randomSeed = Mathf.FloorToInt(start.randomSeed * 1000000);
			gm = new GameMaster(ui); //Implement random seed

			//Shuffle deck
			print("RANDOM SEED: "+ randomSeed);
			selectedDeckBlueprint = DeckGenerator.GenerateRandomizedDeck(playerNamesByClientId.Count + 3, randomSeed, true);
			selectedDeckBlueprint = Utility.ShuffleListBySeed (selectedDeckBlueprint, randomSeed);

//			selectedDeckBlueprint = new List<Role> { Role.Werewolf, Role.MysticWolf, Role.DreamWolf, Role.Mason, Role.Mason, Role.Insomniac, Role.Troublemaker, Role.Drunk } ;

			gm.StartGame(playerNamesByClientId, selectedDeckBlueprint);
		} else {
			Debug.LogError("Unexpected payload type: " + payload.ToString());
		}
	}

	public void JoinSession(string s)
	{
		connector.JoinSession(this.PlayerName);
	}

	public void SubmitNightAction(int[][] selection) {
		connector.BroadcastEvent (new NightActionPayload (selfClientId, selection)); 
	}

	public void SubmitVote(int locationId) {
		connector.BroadcastEvent (new VotePayload (selfClientId, locationId));
	}

	public void Initialize()
	{
		_ui = GetComponent<PlayerUi>();
		_ui.Initialize(this);
	}
}
