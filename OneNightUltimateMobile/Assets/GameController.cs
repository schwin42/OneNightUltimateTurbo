using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class GameController : MonoBehaviour {

	public enum GameState {
		Pregame = 0, //Actions: Player entry, select roles
		Night_Input = 1, //Actions: Take night action
		Night_Reveal = 2, //Actions: Confirm night reveal
		Day = 4, //Actions: Manipulate tokens, vote for players
		Result = 5, //Start new game, return to lobby
	}

	private static GameController _instance;
	public static GameController instance {
		get {
			if(_instance == null) {
				_instance = GameObject.FindObjectOfType<GameController>();
			}
			return _instance;
		}
	}

	private static GameState _currentState = GameState.Pregame;
	public static GameState currentState {
		get {
			return _currentState;
		}
	}

	//Configuration
	[SerializeField]
	public List<Card> deck =
		new List<Card> ()
	{ Card.Werewolf, Card.Werewolf, Card.Troublemaker, Card.Robber, Card.Villager, Card.Villager, Card.Seer, Card.Minion, Card.Tanner }
		; //The deck will be selected/ randomly generated before game start
	public static string[] playerNames = { "Allen", "Becky", "Chris", "David", "Ellen", "Frank", };

	//Game state
	public static List<Player> players;
	public static List<Card> centerCards { get; private set; }
	public static List<OnuGameObject> ogusById = new List<OnuGameObject>();

	//Bookkeeping
	List<PlayerUi> playerUis;
	static Dictionary<Player, PlayerUi> playerUisByPlayer;
	List<Player> playersChoosingNightAction;

	void Start() {
		playerUis = GameObject.FindObjectsOfType<PlayerUi>().ToList();

		SetState(GameState.Night_Input);
	}

	private static void SetState(GameState targetState) {
		if(targetState == currentState) return;
		_currentState = targetState;
		switch(targetState) {
		case GameState.Night_Input:
			if(instance.deck.Count != playerNames.Length + 3) {
				Debug.LogError("Invalid configuration: there are not exactly three more cards than players: players = " + playerNames.Length + ", deck = " + instance.deck.Count);
				return;
			}

			//Start game

			//Create players
			playerUisByPlayer = new Dictionary<Player, PlayerUi>();
			players = new List<Player>();
			for(int i = 0; i < playerNames.Length; i++) {
				Player player = new Player(playerNames[i]);
				players.Add(player);
				instance.playerUis[i].Initialize(player);
				playerUisByPlayer.Add(player, instance.playerUis[i]);
			}

			//Shuffle cards
			System.Random rnd = new System.Random();
			instance.deck = instance.deck.OrderBy(item => rnd.Next()).ToList();

			//Deal cards
			foreach(Player player in players) {
				player.dealtCard = PullFirstCardFromDeck();
				playerUisByPlayer[player].WriteRoleToTitle();
			}
			centerCards = new List<Card>();
			for(int i = 0; i < 3; i++) {
				centerCards.Add(PullFirstCardFromDeck());
			}
			if(instance.deck.Count != 0) {
				Debug.LogError("Deal left cards remaining in deck");
				return;
			}

			//Print player cards
			foreach(Player player in players) {
				print(player.playerName + " is the " + player.dealtCard.ToString() + " " + player.dealtCard.order.ToString());
			}
			foreach(Card card in centerCards) {
				print(card.ToString() + " is in the center");
			}

			//Prompt players for action and set controls
			foreach(Player player in players) {
				playerUisByPlayer[player].DisplayDescription();
			}

			//Wait for responses
			instance.playersChoosingNightAction = new List<Player>(players);

			break;
		case GameState.Night_Reveal:
			//Execute actions in order

			//Reveal information to seer roles

			break;
		case GameState.Day:

			break;
		case GameState.Result:

			break;
		}
	}

	private static Card PullFirstCardFromDeck() {
		Card card = instance.deck[0];
		instance.deck.Remove(card);
		return card;
	}

	public static Prompt GetPrompt (Player player)
	{
		if (player.dealtCard.inputInfo == null)
			return null;

		switch (player.dealtCard.inputInfo.condition) {
		case StateCondition.Always:
			return player.dealtCard.inputInfo.promptIfTrue;
		case StateCondition.AtLeastOneOtherWerewolf:
			return players.Where (p => p.dealtCard is Werewolf && p.playerName != player.playerName).Count () > 0 ? 
				player.dealtCard.inputInfo.promptIfTrue : player.dealtCard.inputInfo.promptIfFalse; 
		default:
			Debug.LogError ("Unhandled condition: " + player.dealtCard.inputInfo.condition);
			return null;
		}
	}

	public static int RegisterOnuGameObject(OnuGameObject ogu) {
		ogusById.Add(ogu);
		return ogusById.Count - 1;
	}

	public static void SubmitNightAction(Player player, int[] targetOguIds) {
		if(currentState != GameState.Night_Input) {
			Debug.LogError("Received night action outside of Night_Input phase");
			return;
		}
		player.nightAction = targetOguIds;
		instance.playersChoosingNightAction.Remove(player);

		if(instance.playersChoosingNightAction.Count == 0) {
			SetState(GameState.Night_Reveal);
		}
	}

}
