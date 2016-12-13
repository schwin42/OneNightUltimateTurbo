using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class GameController : MonoBehaviour {

	public enum GamePhase {
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

	public GamePhase currentPhase;

	//The deck will be selected/ randomly generated before game start
	public List<RealCard> deck;

	public static string[] playerNames = { "Allen", "Becky", "Chris", "David", "Ellen", "Frank", };

	//Game state

	public List<Player> players;
	public List<CenterCardSlot> centerCards;


	//Bookkeeping
	List<PlayerUi> playerUis;
	static Dictionary<Player, PlayerUi> playerUisByPlayer;
	List<Player> playersChoosingNightAction;
	public List<IGamePiece> gamePiecesById = new List<IGamePiece>();
	public List<ILocation> locationsById = new List<ILocation>();

	void Start() {
		playerUis = GameObject.FindObjectsOfType<PlayerUi>().ToList();

		deck =
			new List<RealCard> () {
			new RealCard(Role.Werewolf),
			new RealCard(Role.Villager), 
//			new RealCard(Role.Drunk	), 
//			new RealCard(Role.Minion), 
//			new RealCard(Role.Seer), 
//			new RealCard(Role.Tanner), 
//			new RealCard(Role.Robber), 
//			new RealCard(Role.Mason), 
//			new RealCard(Role.Insomniac), 
			new RealCard(Role.Villager), new RealCard(Role.Villager), new RealCard(Role.Villager),
			new RealCard(Role.Villager), new RealCard(Role.Villager), new RealCard(Role.Villager), new RealCard(Role.Villager), 

//			new RealCard(Role.Werewolf),
//			new RealCard(Role.Werewolf),
//			new RealCard(Role.Werewolf),
//			new RealCard(Role.Werewolf),
//			new RealCard(Role.Werewolf),
//			new RealCard(Role.Werewolf),
//			new RealCard(Role.Werewolf),
//			new RealCard(Role.Werewolf),
		};

		SetPhase(GamePhase.Night_Input);
	}

	private static void SetPhase(GamePhase targetPhase) {
		if(targetPhase == instance.currentPhase) return;
		instance.currentPhase = targetPhase;
		print("Entering " + targetPhase + " phase.");
		switch(targetPhase) {
		case GamePhase.Night_Input:
			if(instance.deck.Count != playerNames.Length + 3) {
				Debug.LogError("Invalid configuration: there are not exactly three more cards than players: players = " + playerNames.Length + ", deck = " + instance.deck.Count);
				return;
			}

			//Start game

			//Create players
			playerUisByPlayer = new Dictionary<Player, PlayerUi>();
			instance.players = new List<Player>();
			for(int i = 0; i < playerNames.Length; i++) {
				Player player = new Player(playerNames[i]);
				instance.players.Add(player);
				instance.playerUis[i].Initialize(player);
				playerUisByPlayer.Add(player, instance.playerUis[i]);
			}

			//Shuffle cards
			System.Random rnd = new System.Random();
			instance.deck = instance.deck.OrderBy(item => rnd.Next()).ToList();

			//Deal cards
			foreach(Player player in instance.players) {
				player.ReceiveDealtCard(PullFirstCardFromDeck());
				playerUisByPlayer[player].WriteRoleToTitle();
			}
			instance.centerCards = new List<CenterCardSlot>();
			for(int i = 0; i < 3; i++) {
				instance.centerCards.Add(new CenterCardSlot(i, PullFirstCardFromDeck()));
			}
			if(instance.deck.Count != 0) {
				Debug.LogError("Deal left cards remaining in deck");
				return;
			}

			//Print player cards
			foreach(Player player in instance.players) {
				print(player.name + " is the " + player.dealtCard.role.ToString() + " " + player.dealtCard.order.ToString());
			}
			foreach(CenterCardSlot slot in instance.centerCards) {
				print(slot.currentCard.role.ToString() + " is in the center");
			}

			//Prompt players for action and set controls
			foreach(Player player in instance.players) {
				player.prompt = new RealizedPrompt(player);
				playerUisByPlayer[player].DisplayPrompt();
			}

			//Wait for responses
			instance.playersChoosingNightAction = new List<Player>(instance.players);

			break;
		case GamePhase.Night_Reveal:
			instance.ExecuteNightActionsInOrder();

			//Reveal information to seer roles
			foreach(Player player in instance.players) {
				playerUisByPlayer[player].DisplayObservation();
			}

			break;
		case GamePhase.Day:

			break;
		case GamePhase.Result:

			break;
		}
	}

	private void ExecuteNightActionsInOrder ()
	{
		List<Player> actingPlayersByTurnOrder = instance.players.Where (p => p.dealtCard.order.primary.HasValue).OrderBy (p => p.dealtCard.order.primary).
			ThenBy (p => p.dealtCard.order.secondary).ToList ();
		for (int i = 0; i < actingPlayersByTurnOrder.Count; i++) {
			Player actingPlayer = actingPlayersByTurnOrder [i];
			for (int j = 0; j < actingPlayer.dealtCard.nightActions.Length; j++) {
				if(actingPlayer.dealtCard.nightActions[j] is ViewOneNightAction) { //Lone werewolf, robber 2nd, insomniac, mystic wolf, apprentice seer
					ViewOneNightAction vonAction = ((ViewOneNightAction)actingPlayer.dealtCard.nightActions[j]);
					int targetLocationId = vonAction.target == TargetType.Self ? actingPlayer.locationId : 
						actingPlayer.nightLocationSelection.locationIds[((int)vonAction.target)];
					actingPlayer.observations.Add(new Observation(targetLocationId, locationsById[targetLocationId].currentCard.gamePieceId));
				} else if(actingPlayer.dealtCard.nightActions[j] is SwapTwoNightAction) { //Robber 1st, troublemaker, drunk

				} else if(actingPlayer.dealtCard.nightActions[j] is ViewUpToTwoNightAction) { //Seer

				}
//				}
			}
		}
	}

	private static RealCard PullFirstCardFromDeck() {
		RealCard card = instance.deck[0];
		instance.deck.Remove(card);
		return card;
	}

	public static int RegisterGamePiece(IGamePiece gamePiece) {
		instance.gamePiecesById.Add(gamePiece);
		return instance.gamePiecesById.Count - 1;
	}

	public static int RegisterLocation(ILocation location) {
		instance.locationsById.Add(location);
		return instance.locationsById.Count - 1;
	}

	public static void SubmitNightAction(Player player, int[] targetLocationIds) {
		if(instance.currentPhase != GamePhase.Night_Input) {
			Debug.LogError("Received night action outside of Night_Input phase");
			return;
		}
		player.nightLocationSelection = new Selection(targetLocationIds);
		instance.playersChoosingNightAction.Remove(player);

		if(instance.playersChoosingNightAction.Count == 0) {
			SetPhase(GamePhase.Night_Reveal);
		}
	}
}

[System.Serializable]
public class RealizedPrompt {
	public string cohortString = "";
	public OptionsSet options;
	public List<ButtonInfo> buttons = new List<ButtonInfo>();

	public RealizedPrompt(Player player) {
		//Evalutate cohort and realize strings
		switch(player.dealtCard.cohort) {
		case CohortType.None:
			if(player.dealtCard.prompt != null) {
				cohortString = player.dealtCard.prompt.explanation;
				options = player.dealtCard.prompt.options;
			}
			break;
		case CohortType.WerewolfNature:
			List<Player> cohorts = GameController.instance.players.Where (p => p.dealtCard.nature == Nature.Werewolf && p.name != player.name).ToList();
			if(cohorts.Count == 0) {
				cohortString = player.dealtCard.prompt.explanation;
				options = player.dealtCard.prompt.options;
			} else {
				for(int i = 0; i < cohorts.Count; i++) {
					if( i != 0) {
						cohortString += " ";
					}
					cohortString += string.Format(player.dealtCard.promptIfCohort.explanation, cohorts[i].name);
				}
				options = player.dealtCard.promptIfCohort.options;
			}
			break;
		}

		switch(options) {
		case OptionsSet.None:
			buttons.Add(new ButtonInfo(-1, "Ready"));
			break;
		case OptionsSet.May_CenterCard: //Apprentice seer, lone werewolf
			for (int i = 0; i < GameController.instance.centerCards.Count; i++) {
				CenterCardSlot slot = GameController.instance.centerCards[i];
				buttons.Add(new ButtonInfo(slot.locationId, "Center Card #" + (i + 1).ToString()));
			}
			buttons.Add(new ButtonInfo(-1, "Pass"));
			break;
		case OptionsSet.Must_CenterCard: //Drunk and copycat
			for (int i = 0; i < GameController.instance.centerCards.Count; i++) {
				CenterCardSlot slot = GameController.instance.centerCards[i];
				buttons.Add(new ButtonInfo(slot.locationId, "Center Card #" + (i + 1).ToString()));
			}
			break;
		case OptionsSet.May_OtherPlayer: //Robber, mystic wolf
			Debug.Log("Not implemented: may other player");
			buttons.Add(new ButtonInfo(-1, "Ready"));
			break;
		default:
			Debug.LogError("Unhandled options set: " + options);
			break;
		}

//		this.options = player.dealtCard.prom prompt.options;

		//Derive cohort string from abstract prompt and game state
	}
}

[System.Serializable]
public struct ButtonInfo {
	public int locationId;
	public string label;
	public ButtonInfo(int ogoId, string label) {
		this.locationId = ogoId;
		this.label = label;
	}
}

[System.Serializable]
public class RealCard : Card, IGamePiece {
	
	private int _gamePieceId;
	public int gamePieceId {
		get {
			return _gamePieceId;
		}
	}
	public string name {
		get {
			return role.ToString();
		}
	}
	//any viewed roles (e.g., Doppleganger, paranormal investigator, copycat)

	public RealCard(Role role) : base(role) {
		_gamePieceId = GameController.RegisterGamePiece(this);
		Debug.Log("Registered " + role.ToString() + " as gamePieceId = " + gamePieceId);
	}
}

[System.Serializable]
public class CenterCardSlot : ILocation {
	private int _locationId;
	public int locationId { get {
			return _locationId;
		}
	}
	public string name { 
		get {
			return "center card #" + (centerCardIndex + 1);
		}
	}
	public int centerCardIndex;
	public RealCard dealtCard;
	private RealCard _currentCard;
	public RealCard currentCard {
		get {
			return _currentCard;
		}
	}
	public CenterCardSlot(int index, RealCard card) {
		this.centerCardIndex = index;
		this.dealtCard = card;
		this._currentCard = card;
		_locationId = GameController.RegisterLocation(this);
		Debug.Log("Registered center card #" + (index + 1) + " as locationId = " + _locationId);
	}
}

[System.Serializable]
public struct Observation {
	public int locationId;
	public int gamePieceId;
	public Observation(int locationId, int gamePieceId) {
		this.locationId = locationId;
		this.gamePieceId = gamePieceId;
	}
}

[System.Serializable]
public struct Selection {
	public int[] locationIds;
	public Selection(params int[] locationIds) {
		this.locationIds = locationIds;
	}
}